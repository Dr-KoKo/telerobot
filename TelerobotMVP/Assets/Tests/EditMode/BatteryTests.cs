using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class BatteryTests
    {
        [Test]
        public void ActivityRatesAndLowPowerPenaltiesMatchSpec()
        {
            var config = TestConfigFactory.Create();
            var system = new BatterySystem(config.Battery);
            var robot = new RobotState("one", 300f, 100f);
            system.Drain(robot, RobotActivity.Combat, 1f, 1f);
            Assert.That(robot.Battery, Is.EqualTo(97.5f));
            robot.Battery = 30f;
            Assert.That(system.BandFor(robot), Is.EqualTo(BatteryBand.LowPower));
            Assert.That(system.MoveMultiplier(robot), Is.EqualTo(0.85f));
            Assert.That(system.AttackMultiplier(robot), Is.EqualTo(0.9f));
        }

        [Test]
        public void DepletedRobotRecoversThenReturnsToCharge()
        {
            var config = TestConfigFactory.Create();
            var system = new BatterySystem(config.Battery);
            var robot = new RobotState("one", 300f, 100f) { Battery = 1f };
            system.Drain(robot, RobotActivity.Combat, 1f, 1f);
            Assert.That(robot.Mode, Is.EqualTo(RobotMode.Disabled));
            Assert.That(robot.CanMove, Is.False);
            Assert.That(robot.CanAttack, Is.False);
            system.TickDisabledRecovery(robot, 5f);
            Assert.That(robot.Mode, Is.EqualTo(RobotMode.Recovery));
            system.TickDisabledRecovery(robot, 10f);
            Assert.That(robot.Battery, Is.EqualTo(5f));
            Assert.That(robot.Mode, Is.EqualTo(RobotMode.ReturnToCharge));
            system.Charge(robot, 1f, 1f);
            Assert.That(robot.Battery, Is.EqualTo(9f));
            Assert.That(robot.CanAttack, Is.False);
        }

        [Test]
        public void RipperHitDrainsFiveAdditionalBattery()
        {
            var config = TestConfigFactory.Create();
            var system = new BatterySystem(config.Battery);
            var robot = new RobotState("one", 300f, 100f);
            system.ApplyRipperHit(robot);
            Assert.That(robot.Battery, Is.EqualTo(95f));
        }
    }
}
