using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class WarningTests
    {
        [Test]
        public void BatteryAndBaseThresholdBoundariesMatchSpec()
        {
            var warnings = new WarningSystem(TestConfigFactory.Create().Warnings);
            Assert.That(warnings.BatterySeverity(25f, 100f), Is.EqualTo(WarningSeverity.None));
            Assert.That(warnings.BatterySeverity(24.9f, 100f), Is.EqualTo(WarningSeverity.Yellow));
            Assert.That(warnings.BatterySeverity(10f, 100f), Is.EqualTo(WarningSeverity.Yellow));
            Assert.That(warnings.BatterySeverity(9.9f, 100f), Is.EqualTo(WarningSeverity.Red));
            Assert.That(warnings.IsBaseWarning(300f, 1000f), Is.True);
        }
    }
}
