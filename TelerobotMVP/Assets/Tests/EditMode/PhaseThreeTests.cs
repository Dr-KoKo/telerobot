using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseThreeTests
    {
        [Test]
        public void MedicalHealsEightPerSecondWithinSixMeters()
        {
            var config = TestConfigFactory.Create();
            var health = new HealthState(100f) { Current = 50f };
            var healed = MedicalRules.HealPlayer(config.Medical, new RuntimeModifiers(), health, 6f, 1f, true);
            Assert.That(healed, Is.EqualTo(8f));
            Assert.That(MedicalRules.HealPlayer(config.Medical, new RuntimeModifiers(), health, 6.1f, 1f, true), Is.Zero);
        }

        [Test]
        public void RipperPrefersRobotAndFinalClearWins()
        {
            var config = TestConfigFactory.Create();
            var target = RipperRules.SelectTarget(config.GetZombie(ZombieType.Ripper),
                new TargetCandidate("base", TargetKind.Base, 1f, true),
                new TargetCandidate("robot", TargetKind.Robot, 10f, true),
                new TargetCandidate("player", TargetKind.Player, 2f, true));
            Assert.That(target.Kind, Is.EqualTo(TargetKind.Robot));

            var phaseSystem = new PhaseSystem(config.Game);
            var session = new SessionState(1) { CurrentPhase = 3 };
            var phase = new PhaseState(3, config.GetPhase(3).OpenRoutes) { AllSpawned = true };
            Assert.That(phaseSystem.Evaluate(session, phase, new BaseState(1000f), new PlayerState(100f, 30, 180, 2)), Is.EqualTo(PhaseTransition.Victory));
            Assert.That(session.Result, Is.EqualTo(GameResult.Victory));
        }
    }
}
