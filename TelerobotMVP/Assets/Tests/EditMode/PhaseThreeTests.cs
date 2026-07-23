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

            var phaseSystem = new PhaseSystem(config.Base);
            var session = new SessionState(1) { CurrentPhase = 3 };
            var phase = new PhaseState(3, config.GetPhase(3).OpenRoutes) { AllSpawned = true };
            Assert.That(phaseSystem.Evaluate(session, phase, new BaseState(1000f), new PlayerState(100f, 30, 180, 2)), Is.EqualTo(PhaseTransition.Victory));
            Assert.That(session.Result, Is.EqualTo(GameResult.Victory));
        }

        [Test]
        public void MedicalDamageIsIncidentalOnlyAndNeverChangesActiveTargetPriority()
        {
            var config = TestConfigFactory.Create();
            var ripper = config.GetZombie(ZombieType.Ripper);
            var target = TargetingSystem.Select(ripper, new[]
            {
                new TargetCandidate("base", TargetKind.Base, 4f, true),
                new TargetCandidate("player", TargetKind.Player, 3f, true),
                new TargetCandidate("haetae", TargetKind.Robot, 5f, true)
            });
            Assert.That(target.Id, Is.EqualTo("haetae"));
            Assert.That(MedicalRules.ShouldApplyIncidentalDamage(1.5f, ripper.AttackRange, true), Is.True);
            Assert.That(MedicalRules.ShouldApplyIncidentalDamage(3f, ripper.AttackRange, true), Is.False);
        }

        [Test]
        public void DataDrivenDashAndBiteMeetRunnerAndBruiserKillTimeBands()
        {
            var config = TestConfigFactory.Create();
            var attack = new RobotAttackSystem(config.Robot);
            Assert.That(attack.EstimateKillTime(config.GetZombie(ZombieType.Runner).MaxHealth, 1f), Is.InRange(1f, 2f));
            Assert.That(attack.EstimateKillTime(config.GetZombie(ZombieType.Bruiser).MaxHealth, 1f), Is.InRange(6f, 10f));
            Assert.That(attack.FirstDashDamage(1.4f), Is.EqualTo(84f));
        }
    }
}
