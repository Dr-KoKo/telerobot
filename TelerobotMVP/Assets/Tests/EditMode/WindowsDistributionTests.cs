using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Telerobot.Game.Editor;
using UnityEngine;

namespace Telerobot.Game.Tests
{
    public sealed class WindowsDistributionTests
    {
        [TestCase("TelerobotMVP.exe", true)]
        [TestCase("TelerobotMVP_Data/globalgamemanagers", true)]
        [TestCase("TelerobotMVP_BurstDebugInformation_DoNotShip/file.txt", false)]
        [TestCase("TelerobotMVP_BackUpThisFolder_ButDontShipItWithYourGame/file.txt", false)]
        [TestCase("GameAssembly.pdb", false)]
        public void DistributionFilterKeepsRuntimeFilesAndExcludesDeveloperArtifacts(string path, bool expected)
        {
            Assert.That(WindowsBuildPipeline.ShouldIncludeInDistribution(path), Is.EqualTo(expected));
        }

        [Test]
        public void DistributionArchiveNameIncludesPlayerVersion()
        {
            Assert.That(WindowsBuildPipeline.GetDistributionArchiveName("0.2.2"),
                Is.EqualTo("TelerobotMVP-Windows-v0.2.2.zip"));
        }

        [TestCase("START-HERE-KO.txt")]
        [TestCase("FEEDBACK-FORM-KO.md")]
        [TestCase("ITCH-IO-UPLOAD-KO.md")]
        public void RequiredDistributionDocumentTemplateExists(string fileName)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(projectRoot, "Documentation", "Playtest", fileName);
            Assert.That(File.Exists(path), Is.True, path);
        }

        [Test]
        public void StoreIdentityMatchesPartnerCenterReservation()
        {
            Assert.That(MicrosoftStorePackageBuilder.PackageIdentityName, Is.EqualTo("Dr-Ko.telerobot"));
            Assert.That(MicrosoftStorePackageBuilder.PackagePublisher,
                Is.EqualTo("CN=D7C3F8A8-2C26-4CBC-BEDF-193632AAF7DC"));
            Assert.That(MicrosoftStorePackageBuilder.PublisherDisplayName, Is.EqualTo("Dr-Ko"));
        }

        [TestCase("0.2.2", "0.2.2.0")]
        [TestCase("1.4.12.3", "1.4.12.3")]
        public void StoreVersionUsesFourNumericParts(string playerVersion, string expected)
        {
            Assert.That(MicrosoftStorePackageBuilder.GetStoreVersion(playerVersion), Is.EqualTo(expected));
        }

        [Test]
        public void StorePackageNameIncludesStoreVersionAndArchitecture()
        {
            Assert.That(MicrosoftStorePackageBuilder.GetStorePackageName("0.2.2"),
                Is.EqualTo("TelerobotMVP-Store-v0.2.2.0-x64.msix"));
        }

        [Test]
        public void StoreManifestUsesReservedIdentityAndFullTrustDesktopEntry()
        {
            var manifest = XDocument.Parse(MicrosoftStorePackageBuilder.CreateManifest("0.2.2"));
            XNamespace foundation = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace uap10 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/10";
            XNamespace rescap = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";

            var identity = manifest.Root.Element(foundation + "Identity");
            Assert.That((string)identity.Attribute("Name"), Is.EqualTo("Dr-Ko.telerobot"));
            Assert.That((string)identity.Attribute("Publisher"),
                Is.EqualTo("CN=D7C3F8A8-2C26-4CBC-BEDF-193632AAF7DC"));
            Assert.That((string)identity.Attribute("Version"), Is.EqualTo("0.2.2.0"));
            Assert.That((string)identity.Attribute("ProcessorArchitecture"), Is.EqualTo("x64"));

            var application = manifest.Descendants(foundation + "Application").Single();
            Assert.That((string)application.Attribute("Executable"), Is.EqualTo("TelerobotMVP.exe"));
            Assert.That((string)application.Attribute(uap10 + "RuntimeBehavior"), Is.EqualTo("packagedClassicApp"));
            Assert.That((string)application.Attribute(uap10 + "TrustLevel"), Is.EqualTo("mediumIL"));
            Assert.That(manifest.Descendants(rescap + "Capability").Single().Attribute("Name").Value,
                Is.EqualTo("runFullTrust"));
        }

        [TestCase("Square44x44Logo.png")]
        [TestCase("Square150x150Logo.png")]
        [TestCase("StoreLogo.png")]
        public void RequiredStoreVisualAssetExists(string fileName)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(projectRoot, "Documentation", "Store", "Assets", fileName);
            Assert.That(File.Exists(path), Is.True, path);
        }
    }
}
