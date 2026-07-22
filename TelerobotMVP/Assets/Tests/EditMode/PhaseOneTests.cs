using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseOneTests
    {
        [Test]
        public void RunnerAndBruiserBaseDamageMatchSpec()
        {
            var config = TestConfigFactory.Create();
            Assert.That(config.GetZombie(ZombieType.Runner).BaseDamage, Is.EqualTo(8f));
            Assert.That(config.GetZombie(ZombieType.Bruiser).BaseDamage, Is.EqualTo(60f));
        }

        [Test]
        public void CameraAndJumpTuningHasSafePlayableRanges()
        {
            var rules = TestConfigFactory.Create().Game;
            Assert.That(rules.JumpHeight, Is.InRange(0.5f, 3f));
            Assert.That(rules.FirstPersonEyeHeight, Is.GreaterThan(0f));
            Assert.That(rules.CameraCollisionRadius, Is.GreaterThan(0f));
            Assert.That(rules.FirstPersonFieldOfView, Is.InRange(50f, 100f));
            Assert.That(rules.ThirdPersonFieldOfView, Is.InRange(50f, 100f));
        }

        [Test]
        public void SprintTuningIsFasterThanWalkingWithoutExtremeSpeed()
        {
            var rules = TestConfigFactory.Create().Game;
            Assert.That(rules.SprintMultiplier, Is.InRange(1.1f, 2f));
            Assert.That(rules.PlayerMoveSpeed * rules.SprintMultiplier, Is.LessThanOrEqualTo(16f));
        }

        [Test]
        public void DifficultyTuningKeepsWavePressureWithinReducedTargets()
        {
            var config = TestConfigFactory.Create();
            var phaseOne = config.GetPhase(1);
            var phaseTwo = config.GetPhase(2);
            var phaseThree = config.GetPhase(3);

            Assert.That(phaseOne.LearningTotal.Max, Is.EqualTo(24));
            Assert.That(phaseOne.MaxAliveConcurrent, Is.EqualTo(15));
            Assert.That(phaseTwo.LearningTotal.Max, Is.EqualTo(39));
            Assert.That(phaseTwo.MaxAliveConcurrent, Is.EqualTo(20));
            Assert.That(phaseThree.LearningTotal.Max, Is.EqualTo(55));
            Assert.That(phaseThree.MaxAliveConcurrent, Is.EqualTo(24));
        }

        [Test]
        public void ClearRequiresAllSpawnedNoAliveAndBaseAlive()
        {
            var config = TestConfigFactory.Create();
            var system = new PhaseSystem(config.Base);
            var session = new SessionState(1001);
            var phase = new PhaseState(1, new[] { RouteId.NorthRoad });
            var baseState = new BaseState(1000f);
            var player = new PlayerState(100f, 30, 180, 2);
            Assert.That(system.Evaluate(session, phase, baseState, player), Is.EqualTo(PhaseTransition.None));
            phase.AllSpawned = true;
            Assert.That(system.Evaluate(session, phase, baseState, player), Is.EqualTo(PhaseTransition.AwaitingUpgrade));
        }

        [TestCase(true, false, DefeatReason.BaseDestroyed)]
        [TestCase(false, true, DefeatReason.PlayerDeath)]
        public void BaseOrPlayerDeathImmediatelyDefeats(bool killBase, bool killPlayer, DefeatReason reason)
        {
            var config = TestConfigFactory.Create();
            var system = new PhaseSystem(config.Base);
            var session = new SessionState(1);
            var phase = new PhaseState(1, new[] { RouteId.NorthRoad });
            var baseState = new BaseState(1000f);
            var player = new PlayerState(100f, 30, 180, 2);
            if (killBase) baseState.Health.Current = 0f;
            if (killPlayer) player.Health.Current = 0f;
            Assert.That(system.Evaluate(session, phase, baseState, player), Is.EqualTo(PhaseTransition.Defeat));
            Assert.That(session.DefeatReason, Is.EqualTo(reason));
        }

        [Test]
        public void ConfigurationOwnershipHasNoBaseOrReserveAmmoMirrors()
        {
            var config = TestConfigFactory.Create();
            Assert.That(typeof(GameRulesConfig).GetField("BaseMaxHealth"), Is.Null);
            Assert.That(typeof(GameRulesConfig).GetField("BasePhaseRecoveryFraction"), Is.Null);
            Assert.That(typeof(WeaponConfig).GetField("ReserveAmmo"), Is.Null);
            Assert.That(config.Base.MaxHealth, Is.EqualTo(1000f));
            Assert.That(config.Ammo.ReserveAmmoMax, Is.EqualTo(240));
        }
    }
}
