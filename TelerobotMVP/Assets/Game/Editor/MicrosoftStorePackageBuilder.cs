using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Telerobot.Game.Editor
{
    public static class MicrosoftStorePackageBuilder
    {
        public const string PackageIdentityName = "Dr-Ko.telerobot";
        public const string PackagePublisher = "CN=D7C3F8A8-2C26-4CBC-BEDF-193632AAF7DC";
        public const string PublisherDisplayName = "Dr-Ko";

        private const string DisplayName = "Telerobot";
        private const string Description = "미래 한국의 해태 로봇과 함께 기지를 방어하는 3D 액션 게임";
        private const string ApplicationId = "TelerobotMVP";
        private const string ExecutableName = "TelerobotMVP.exe";
        private const string StoreFolder = "Builds/Store";
        private const string PlayerFolder = "Builds/Store/Windows";
        private const string StagingFolder = "Builds/Store/Staging";
        private const string StoreSourceFolder = "Documentation/Store";

        private static readonly string[] VisualAssetNames =
        {
            "Square44x44Logo.png",
            "Square150x150Logo.png",
            "StoreLogo.png"
        };

        [MenuItem("Tools/Telerobot/Build Microsoft Store MSIX")]
        public static void BuildMicrosoftStoreMsix()
        {
            MvpProjectBuilder.BuildAll();

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var storeOutput = Path.Combine(projectRoot, StoreFolder);
            var playerOutput = Path.Combine(projectRoot, PlayerFolder);
            var stagingOutput = Path.Combine(projectRoot, StagingFolder);

            ResetProjectSubfolder(projectRoot, playerOutput);
            ResetProjectSubfolder(projectRoot, stagingOutput);
            Directory.CreateDirectory(storeOutput);

            WindowsBuildPipeline.BuildWindowsPlayer(playerOutput, BuildOptions.None);
            CopyRuntimePayload(playerOutput, stagingOutput);
            CopyVisualAssets(projectRoot, stagingOutput);

            var storeVersion = GetStoreVersion(PlayerSettings.bundleVersion);
            File.WriteAllText(
                Path.Combine(stagingOutput, "AppxManifest.xml"),
                CreateManifest(storeVersion),
                new UTF8Encoding(false));
            WriteSubmissionGuide(storeOutput, storeVersion);

            var makeAppxPath = FindMakeAppxPath();
            if (string.IsNullOrEmpty(makeAppxPath))
            {
                throw new FileNotFoundException(
                    "Microsoft Store staging completed, but MakeAppx.exe was not found. " +
                    "Install the Windows 10/11 SDK, then run Tools > Telerobot > Build Microsoft Store MSIX again. " +
                    "Staging: " + stagingOutput);
            }

            var packagePath = Path.Combine(storeOutput, GetStorePackageName(storeVersion));
            RunMakeAppx(makeAppxPath, stagingOutput, packagePath);
            UnityEngine.Debug.Log("Microsoft Store MSIX completed: " + packagePath);
            UnityEngine.Debug.Log(
                "The generated MSIX is intentionally unsigned. Upload it to Partner Center; Microsoft signs the certified Store package.");
        }

        public static void BuildMicrosoftStoreMsixBatch()
        {
            BuildMicrosoftStoreMsix();
        }

        public static string GetStoreVersion(string playerVersion)
        {
            if (string.IsNullOrWhiteSpace(playerVersion))
                throw new ArgumentException("A player version is required.", nameof(playerVersion));

            var parts = playerVersion.Split('.');
            if (parts.Length < 1 || parts.Length > 4)
                throw new FormatException("The Store version must contain one to four numeric parts.");

            var values = new List<ushort>(4);
            foreach (var part in parts)
            {
                ushort value;
                if (string.IsNullOrWhiteSpace(part) || !ushort.TryParse(part, out value))
                    throw new FormatException("Each Store version part must be a number from 0 to 65535.");
                values.Add(value);
            }

            while (values.Count < 4) values.Add(0);
            return string.Join(".", values.Select(value => value.ToString()).ToArray());
        }

        public static string GetStorePackageName(string version)
        {
            return "TelerobotMVP-Store-v" + GetStoreVersion(version) + "-x64.msix";
        }

        public static string CreateManifest(string version)
        {
            var storeVersion = GetStoreVersion(version);
            XNamespace foundation = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace uap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XNamespace uap10 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/10";
            XNamespace rescap = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(foundation + "Package",
                    new XAttribute(XNamespace.Xmlns + "uap", uap.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "uap10", uap10.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "rescap", rescap.NamespaceName),
                    new XElement(foundation + "Identity",
                        new XAttribute("Name", PackageIdentityName),
                        new XAttribute("Version", storeVersion),
                        new XAttribute("Publisher", PackagePublisher),
                        new XAttribute("ProcessorArchitecture", "x64")),
                    new XElement(foundation + "Properties",
                        new XElement(foundation + "DisplayName", DisplayName),
                        new XElement(foundation + "PublisherDisplayName", PublisherDisplayName),
                        new XElement(foundation + "Description", Description),
                        new XElement(foundation + "Logo", @"Assets\StoreLogo.png")),
                    new XElement(foundation + "Resources",
                        new XElement(foundation + "Resource", new XAttribute("Language", "ko-kr"))),
                    new XElement(foundation + "Dependencies",
                        new XElement(foundation + "TargetDeviceFamily",
                            new XAttribute("Name", "Windows.Desktop"),
                            new XAttribute("MinVersion", "10.0.19041.0"),
                            new XAttribute("MaxVersionTested", "10.0.26100.0"))),
                    new XElement(foundation + "Capabilities",
                        new XElement(rescap + "Capability", new XAttribute("Name", "runFullTrust"))),
                    new XElement(foundation + "Applications",
                        new XElement(foundation + "Application",
                            new XAttribute("Id", ApplicationId),
                            new XAttribute("Executable", ExecutableName),
                            new XAttribute(uap10 + "RuntimeBehavior", "packagedClassicApp"),
                            new XAttribute(uap10 + "TrustLevel", "mediumIL"),
                            new XElement(uap + "VisualElements",
                                new XAttribute("DisplayName", DisplayName),
                                new XAttribute("Description", Description),
                                new XAttribute("Square150x150Logo", @"Assets\Square150x150Logo.png"),
                                new XAttribute("Square44x44Logo", @"Assets\Square44x44Logo.png"),
                                new XAttribute("BackgroundColor", "#07142E"))))));

            return document.Declaration + Environment.NewLine + document.Root;
        }

        public static string FindMakeAppxPath()
        {
            var roots = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "App Certification Kit")
            };

            var candidates = new List<string>();
            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    candidates.AddRange(Directory.GetFiles(root, "MakeAppx.exe", SearchOption.AllDirectories));
                }
                catch (UnauthorizedAccessException)
                {
                    // Continue with the SDK locations that are readable by the current user.
                }
            }

            return candidates
                .Where(path => path.IndexOf(Path.DirectorySeparatorChar + "x64" + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.IndexOf("App Certification Kit", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private static void CopyRuntimePayload(string sourceFolder, string destinationFolder)
        {
            foreach (var filePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = filePath.Substring(sourceFolder.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (!WindowsBuildPipeline.ShouldIncludeInDistribution(relativePath)) continue;

                var destinationPath = Path.Combine(destinationFolder, relativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);
                File.Copy(filePath, destinationPath, true);
            }
        }

        private static void CopyVisualAssets(string projectRoot, string stagingFolder)
        {
            var sourceFolder = Path.Combine(projectRoot, StoreSourceFolder, "Assets");
            var destinationFolder = Path.Combine(stagingFolder, "Assets");
            Directory.CreateDirectory(destinationFolder);

            foreach (var assetName in VisualAssetNames)
            {
                var sourcePath = Path.Combine(sourceFolder, assetName);
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException("A required Microsoft Store visual asset is missing.", sourcePath);
                File.Copy(sourcePath, Path.Combine(destinationFolder, assetName), true);
            }
        }

        private static void ResetProjectSubfolder(string projectRoot, string outputFolder)
        {
            var normalizedRoot = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedOutput = Path.GetFullPath(outputFolder);
            if (!normalizedOutput.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Refusing to reset a folder outside the Unity project: " + normalizedOutput);

            if (Directory.Exists(normalizedOutput)) Directory.Delete(normalizedOutput, true);
            Directory.CreateDirectory(normalizedOutput);
        }

        private static void RunMakeAppx(string makeAppxPath, string stagingFolder, string packagePath)
        {
            var output = new StringBuilder();
            var errors = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = makeAppxPath,
                Arguments = "pack /v /o /d " + Quote(stagingFolder) + " /p " + Quote(packagePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (_, args) => { if (args.Data != null) output.AppendLine(args.Data); };
                process.ErrorDataReceived += (_, args) => { if (args.Data != null) errors.AppendLine(args.Data); };
                if (!process.Start()) throw new InvalidOperationException("MakeAppx.exe could not be started.");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(
                        "MakeAppx.exe failed with exit code " + process.ExitCode + Environment.NewLine +
                        output + Environment.NewLine + errors);
            }

            UnityEngine.Debug.Log(output.ToString());
        }

        private static string Quote(string path)
        {
            return "\"" + path.Replace("\"", "\\\"") + "\"";
        }

        private static void WriteSubmissionGuide(string storeOutput, string storeVersion)
        {
            var guide =
                "# Microsoft Store 제출 안내\r\n\r\n" +
                "- 패키지 ID: `" + PackageIdentityName + "`\r\n" +
                "- 게시자: `" + PackagePublisher + "`\r\n" +
                "- 표시 게시자: `" + PublisherDisplayName + "`\r\n" +
                "- 패키지 버전: `" + storeVersion + "`\r\n" +
                "- 제출 파일: `" + GetStorePackageName(storeVersion) + "`\r\n\r\n" +
                "## 중요한 차이\r\n\r\n" +
                "이 빌드 명령이 만드는 `.msix`는 의도적으로 서명되지 않습니다. 로컬에서 더블클릭 설치하지 말고 " +
                "Partner Center의 패키지 제출 단계에 업로드하세요. Microsoft Store 인증이 끝난 뒤 Microsoft가 서명한 " +
                "Store 설치본에서 게시자 신뢰가 적용되며 ‘인식할 수 없는 앱’ 경고를 피할 수 있습니다.\r\n\r\n" +
                "로컬 패키지 동작은 관리자 권한이 아닌 PowerShell에서 Staging 폴더로 이동한 뒤 " +
                "`Add-AppxPackage -Register .\\AppxManifest.xml`로 느슨한 등록 테스트를 할 수 있습니다. " +
                "테스트를 끝낸 뒤 Windows 설정의 설치된 앱에서 Telerobot을 제거할 수 있습니다.\r\n";
            File.WriteAllText(Path.Combine(storeOutput, "STORE-SUBMISSION-KO.md"), guide, new UTF8Encoding(true));
        }
    }
}
