using System.Collections.Generic;
using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseTwoAndUpgradeTests
    {
        [Test]
        public void PhaseTwoCompositionPreservesBruiserMinimumWithinBudget()
        {
            var config = TestConfigFactory.Create();
            var spawns = new SpawnSystem(config).Compose(config.GetPhase(2), new XorShiftRng(1001));
            Assert.That(spawns.FindAll(item => item.Type == ZombieType.Bruiser).Count, Is.InRange(2, 3));
            var cost = spawns.FindAll(item => item.Type == ZombieType.Runner).Count + spawns.FindAll(item => item.Type == ZombieType.Bruiser).Count * 5;
            Assert.That(cost, Is.LessThanOrEqualTo(60));
        }

        [Test]
        public void ContinuousSpawnSchedulerPausesAtCapAndResumesWhenCapacityReturns()
        {
            var phase = TestConfigFactory.Create().GetPhase(1);
            var scheduler = new ContinuousSpawnScheduler(phase, new XorShiftRng(1001));

            Assert.That(scheduler.Advance(phase.PhaseStartDelaySeconds - 0.01f, 0, 20), Is.Zero);
            var firstGroup = scheduler.Advance(0.01f, 0, 20);
            Assert.That(firstGroup, Is.InRange(phase.GroupSize.Min, phase.GroupSize.Max));
            Assert.That(scheduler.Advance(phase.GroupIntervalSeconds, phase.MaxAliveConcurrent, 20), Is.Zero);
            Assert.That(scheduler.Advance(0f, phase.MaxAliveConcurrent - 2, 20), Is.EqualTo(2));
        }

        [Test]
        public void PhaseThreeCompositionKeepsSpecialMinimumsAndWeightsRippersToSouthTunnel()
        {
            var config = TestConfigFactory.Create();
            var phase = config.GetPhase(3);
            var spawns = new SpawnSystem(config).Compose(phase, new XorShiftRng(1001));
            var bruisers = spawns.FindAll(item => item.Type == ZombieType.Bruiser).Count;
            var rippers = spawns.FindAll(item => item.Type == ZombieType.Ripper).Count;
            var southRippers = spawns.FindAll(item => item.Type == ZombieType.Ripper && item.Route == RouteId.SouthTunnel).Count;
            var northRippers = spawns.FindAll(item => item.Type == ZombieType.Ripper && item.Route == RouteId.NorthRoad).Count;
            var eastRippers = spawns.FindAll(item => item.Type == ZombieType.Ripper && item.Route == RouteId.EastAlley).Count;

            Assert.That(bruisers, Is.InRange(2, 3));
            Assert.That(rippers, Is.InRange(3, 5));
            Assert.That(southRippers, Is.GreaterThan(northRippers));
            Assert.That(southRippers, Is.GreaterThan(eastRippers));
            Assert.That(spawns.Count, Is.InRange(phase.LearningTotal.Min, phase.LearningTotal.Max));
            Assert.That(new SpawnSystem(config).ThreatCost(CountByType(spawns)), Is.LessThanOrEqualTo(phase.ThreatBudget));
        }

        private static Dictionary<ZombieType, int> CountByType(List<SpawnEntry> spawns)
        {
            var counts = new Dictionary<ZombieType, int>
            {
                { ZombieType.Runner, 0 }, { ZombieType.Bruiser, 0 }, { ZombieType.Ripper, 0 }
            };
            foreach (var spawn in spawns) counts[spawn.Type]++;
            return counts;
        }

        [Test]
        public void PhaseTwoClearImmediatelyAdvancesAndPreservesSpecializationReadyState()
        {
            var config = TestConfigFactory.Create();
            var system = new PhaseSystem(config.Base, config.Phases.Count);
            var session = new SessionState(2);
            var baseState = new BaseState(1000f);
            var player = new PlayerState(100f, 30, 180, 2);
            var phase = new PhaseState(2, new[] { RouteId.NorthRoad, RouteId.EastAlley })
            {
                AllSpawned = true
            };
            var robot = new RobotState("haetae-1", 300f, 100f);
            robot.Progression.Level = 2;
            robot.Progression.Experience = config.HaetaeProgression.ExperiencePerLevel;

            Assert.That(system.Evaluate(session, phase, baseState, player), Is.EqualTo(PhaseTransition.NextPhase));
            Assert.That(robot.Progression.Level, Is.EqualTo(2));
            Assert.That(robot.Progression.SpecializationReady, Is.True);
            Assert.That(robot.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Unselected));
        }

        [Test]
        public void PhaseThreeAndSevenAdvanceWhilePhaseEightEndsInVictory()
        {
            var config = TestConfigFactory.Create();
            var system = new PhaseSystem(config.Base, config.Phases.Count);
            var session = new SessionState(2);
            var baseState = new BaseState(1000f);
            var player = new PlayerState(100f, 30, 180, 2);

            foreach (var phaseNumber in new[] { 3, 7 })
            {
                var phase = new PhaseState(phaseNumber, config.GetPhase(phaseNumber).OpenRoutes)
                {
                    AllSpawned = true
                };
                Assert.That(system.Evaluate(session, phase, baseState, player),
                    Is.EqualTo(PhaseTransition.NextPhase));
                Assert.That(session.Result, Is.EqualTo(GameResult.InProgress));
            }

            var finalPhase = new PhaseState(8, config.GetPhase(8).OpenRoutes) { AllSpawned = true };
            Assert.That(system.Evaluate(session, finalPhase, baseState, player),
                Is.EqualTo(PhaseTransition.Victory));
            Assert.That(session.Result, Is.EqualTo(GameResult.Victory));
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
