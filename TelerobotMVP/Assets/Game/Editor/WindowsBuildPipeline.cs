using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Telerobot.Game.Editor
{
    public static class WindowsBuildPipeline
    {
        private const string BuildFolder = "Builds/Windows";
        private const string ShareBuildFolder = "Builds/Shareable/Windows";
        private const string DistributionFolder = "Builds/Distribution";
        private const string DocumentTemplateFolder = "Documentation/Playtest";
        private const string ExecutableName = "TelerobotMVP.exe";

        [MenuItem("Tools/Telerobot/Build Windows Playtest")]
        public static void BuildWindowsPlaytest()
        {
            MvpProjectBuilder.BuildAll();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputFolder = Path.Combine(projectRoot, BuildFolder);
            Directory.CreateDirectory(outputFolder);

            var executablePath = BuildWindowsPlayer(outputFolder, BuildOptions.Development);
            WriteDevelopmentGuide(outputFolder);
            Debug.Log("Windows playtest build completed: " + executablePath);
        }

        [MenuItem("Tools/Telerobot/Build Shareable Windows Package")]
        public static void BuildShareableWindowsPlaytest()
        {
            MvpProjectBuilder.BuildAll();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputFolder = Path.Combine(projectRoot, ShareBuildFolder);
            ResetOutputFolder(projectRoot, outputFolder);

            var executablePath = BuildWindowsPlayer(outputFolder, BuildOptions.None);
            var distributionFolder = Path.Combine(projectRoot, DistributionFolder);
            Directory.CreateDirectory(distributionFolder);

            var version = PlayerSettings.bundleVersion;
            var archiveName = GetDistributionArchiveName(version);
            var archivePath = Path.Combine(distributionFolder, archiveName);
            WriteDistributionDocuments(projectRoot, outputFolder, distributionFolder, version, archiveName);
            CreateDistributionArchive(outputFolder, archivePath);

            Debug.Log("Shareable Windows playtest completed: " + executablePath);
            Debug.Log("Shareable archive completed: " + archivePath);
        }

        internal static string BuildWindowsPlayer(string outputFolder, BuildOptions options)
        {
            Directory.CreateDirectory(outputFolder);

            var enabledScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled) enabledScenes.Add(scene.path);
            if (enabledScenes.Count == 0) throw new InvalidOperationException("No enabled scenes are available for the Windows build.");

            var executablePath = Path.Combine(outputFolder, ExecutableName);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = options
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows playtest build failed: " + report.summary.result);

            return executablePath;
        }

        private static void WriteDevelopmentGuide(string outputFolder)
        {
            var guide =
                "텔레로봇 MVP Windows 플레이테스트\r\n\r\n" +
                "1. TelerobotMVP.exe를 더블클릭합니다.\r\n" +
                "2. 시작 화면에서 설정을 먼저 확인합니다.\r\n" +
                "3. 게임 시작을 누릅니다.\r\n\r\n" +
                "조작: WASD 이동, 마우스 조준, Shift 달리기, Space 점프, V 시점 전환, " +
                "마우스 왼쪽 사격, R 재장전, G 수류탄, E 보급, Esc 일시정지.\r\n\r\n" +
                "중요: Builds/Windows 폴더 안의 파일과 폴더를 전부 함께 보관해야 합니다.\r\n";
            File.WriteAllText(Path.Combine(outputFolder, "README-KO.txt"), guide, new UTF8Encoding(true));
        }

        public static void BuildWindowsPlaytestBatch()
        {
            BuildWindowsPlaytest();
        }

        public static void BuildShareableWindowsPlaytestBatch()
        {
            BuildShareableWindowsPlaytest();
        }

        public static bool ShouldIncludeInDistribution(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            var normalized = relativePath.Replace('\\', '/').ToLowerInvariant();
            if (normalized.Contains("donotship") || normalized.Contains("dontshipitwithyourgame")) return false;
            var extension = Path.GetExtension(normalized);
            return extension != ".pdb" && extension != ".mdb";
        }

        public static string GetDistributionArchiveName(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("A player version is required.", nameof(version));
            foreach (var invalid in Path.GetInvalidFileNameChars()) version = version.Replace(invalid, '-');
            return "TelerobotMVP-Windows-v" + version + ".zip";
        }

        private static void ResetOutputFolder(string projectRoot, string outputFolder)
        {
            var normalizedRoot = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedOutput = Path.GetFullPath(outputFolder);
            if (!normalizedOutput.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Refusing to reset a build folder outside the Unity project: " + normalizedOutput);
            if (Directory.Exists(normalizedOutput)) Directory.Delete(normalizedOutput, true);
            Directory.CreateDirectory(normalizedOutput);
        }

        private static void WriteDistributionDocuments(
            string projectRoot,
            string outputFolder,
            string distributionFolder,
            string version,
            string archiveName)
        {
            var templateFolder = Path.Combine(projectRoot, DocumentTemplateFolder);
            WriteRenderedTemplate(templateFolder, "START-HERE-KO.txt", outputFolder, version, archiveName);
            WriteRenderedTemplate(templateFolder, "FEEDBACK-FORM-KO.md", distributionFolder, version, archiveName);
            WriteRenderedTemplate(templateFolder, "ITCH-IO-UPLOAD-KO.md", distributionFolder, version, archiveName);
        }

        private static void WriteRenderedTemplate(
            string templateFolder,
            string fileName,
            string outputFolder,
            string version,
            string archiveName)
        {
            var templatePath = Path.Combine(templateFolder, fileName);
            if (!File.Exists(templatePath)) throw new FileNotFoundException("Distribution document template is missing.", templatePath);
            var contents = File.ReadAllText(templatePath, Encoding.UTF8)
                .Replace("{{BUILD_VERSION}}", version)
                .Replace("{{ARCHIVE_NAME}}", archiveName);
            File.WriteAllText(Path.Combine(outputFolder, fileName), contents, new UTF8Encoding(true));
        }

        private static void CreateDistributionArchive(string sourceFolder, string archivePath)
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            using (var fileStream = new FileStream(archivePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                foreach (var filePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    var relativePath = filePath.Substring(sourceFolder.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (!ShouldIncludeInDistribution(relativePath)) continue;
                    var entry = archive.CreateEntry(relativePath.Replace('\\', '/'),
                        System.IO.Compression.CompressionLevel.Optimal);
                    using (var input = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var output = entry.Open())
                        input.CopyTo(output);
                }
            }
        }
    }
}
