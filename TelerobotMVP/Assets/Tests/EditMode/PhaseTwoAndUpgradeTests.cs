using System.Collections.Generic;
using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseTwoAndUpgradeTests
    {
        [Test]
        public void PhaseTwoCompositionPreservesThreeBruisersWithinBudget()
        {
            var config = TestConfigFactory.Create();
            var spawns = new SpawnSystem(config).Compose(config.GetPhase(2), new XorShiftRng(1001));
            Assert.That(spawns.FindAll(item => item.Type == ZombieType.Bruiser).Count, Is.EqualTo(3));
            var cost = spawns.FindAll(item => item.Type == ZombieType.Runner).Count + spawns.FindAll(item => item.Type == ZombieType.Bruiser).Count * 5;
            Assert.That(cost, Is.LessThanOrEqualTo(60));
        }

        [Test]
        public void OfferHasThreeUniqueChoicesAndSelectionAppliesImmediately()
        {
            var config = TestConfigFactory.Create();
            var system = new UpgradeSystem(config);
            var offer = system.Offer(new XorShiftRng(2));
            Assert.That(offer.Count, Is.EqualTo(3));
            Assert.That(new HashSet<string>(offer.ConvertAll(item => item.Id)).Count, Is.EqualTo(3));
            var session = new SessionState(2);
            var baseState = new BaseState(1000f);
            var robot = new RobotState("one", 300f, 100f);
            var player = new PlayerState(100f, 30, 180, 2);
            var modifiers = new RuntimeModifiers();
            Assert.That(system.Apply(config.Upgrades[0], session, baseState, new[] { robot }, player, modifiers), Is.True);
            Assert.That(robot.MaximumBattery, Is.EqualTo(120f));
            Assert.That(robot.Battery, Is.EqualTo(120f));
        }

        [Test]
        public void BarrierTakesCumulativeDamage()
        {
            var barrier = new BarrierState(RouteId.NorthRoad, 300f);
            CombatRules.ApplyDamage(barrier.Health, 120f);
            CombatRules.ApplyDamage(barrier.Health, 180f);
            Assert.That(barrier.Health.IsDead, Is.True);
        }
    }
}
