using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeProgressionTests
    {
        private HaetaeProgressionSystem system;
        private HaetaeProgressionConfig config;
        private List<RobotState> robots;

        [SetUp]
        public void SetUp()
        {
            system = new HaetaeProgressionSystem();
            config = new HaetaeProgressionConfig
            {
                ExperiencePerLevel = 100,
                ReadyAlertSeconds = 4f,
                PowerDamageBonusPerRank = 0.10f,
                ArmorDamageReductionPerRank = 0.08f,
                EfficiencyBatteryReductionPerRank = 0.08f,
                AttackSpeedBonusPerRank = 0.10f,
                MinimumReductionMultiplier = 0.50f
            };
            robots = new List<RobotState>
            {
                new RobotState("haetae-1", 300f, 100f),
                new RobotState("haetae-2", 300f, 100f)
            };
        }

        [Test]
        public void NewRobotsStartIndependentlyAtLevelOneGeneral()
        {
            Assert.That(robots.Select(item => item.Progression.Level), Is.EqualTo(new[] { 1, 1 }));
            Assert.That(robots.Select(item => item.Progression.Experience), Is.EqualTo(new[] { 0, 0 }));
            Assert.That(robots.Select(item => item.Progression.Specialization),
                Is.EqualTo(new[] { HaetaeSpecialization.Unselected, HaetaeSpecialization.Unselected }));
            Assert.That(ReferenceEquals(robots[0].Progression, robots[1].Progression), Is.False);
        }

        [Test]
        public void OneContributorReceivesFullRewardAndPlayerOnlyDamageDoesNotContribute()
        {
            var zombie = DeadZombie("runner", ZombieType.Runner);
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 1f, robots),
                Is.EqualTo(ContributionResult.Recorded));
            Assert.That(system.RecordContribution(zombie, DamageSource.Player("player"), 50f, robots),
                Is.EqualTo(ContributionResult.NotEligible));

            var awards = system.AwardForDeath(zombie, 5, robots, config);

            Assert.That(awards.Select(item => item.RobotId), Is.EqualTo(new[] { "haetae-1" }));
            Assert.That(awards[0].RewardAmount, Is.EqualTo(5));
            Assert.That(awards[0].AppliedAmount, Is.EqualTo(5));
            Assert.That(robots[0].Progression.Experience, Is.EqualTo(5));
            Assert.That(robots[1].Progression.Experience, Is.Zero);
        }

        [Test]
        public void SharedKillAwardsEveryUniqueContributorOnceInOrdinalOrder()
        {
            var zombie = DeadZombie("bruiser", ZombieType.Bruiser);
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("haetae-2"), 4f, robots),
                Is.EqualTo(ContributionResult.Recorded));
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 3f, robots),
                Is.EqualTo(ContributionResult.Recorded));
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 2f, robots),
                Is.EqualTo(ContributionResult.AlreadyRecorded));

            var awards = system.AwardForDeath(zombie, 25, robots, config);

            Assert.That(awards.Select(item => item.RobotId), Is.EqualTo(new[] { "haetae-1", "haetae-2" }));
            Assert.That(robots.Select(item => item.Progression.Experience), Is.EqualTo(new[] { 25, 25 }));
            Assert.That(system.AwardForDeath(zombie, 25, robots, config), Is.Empty);
        }

        [Test]
        public void UnknownOrZeroAppliedHaetaeDamageIsRejected()
        {
            var zombie = DeadZombie("runner", ZombieType.Runner);
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("unknown"), 1f, robots),
                Is.EqualTo(ContributionResult.UnknownRobot));
            Assert.That(system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 0f, robots),
                Is.EqualTo(ContributionResult.NotEligible));
            Assert.That(system.AwardForDeath(zombie, 5, robots, config), Is.Empty);
        }

        [Test]
        public void DestroyedContributorStillReceivesReward()
        {
            var zombie = DeadZombie("ripper", ZombieType.Ripper);
            system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 1f, robots);
            robots[0].Health.Current = 0f;
            robots[0].Mode = RobotMode.Destroyed;

            system.AwardForDeath(zombie, 20, robots, config);

            Assert.That(robots[0].Progression.Experience, Is.EqualTo(20));
        }

        [Test]
        public void AwardPreservesOverflowAndContinuesBeyondLevelTwo()
        {
            robots[0].Progression.Experience = 90;
            var firstZombie = DeadZombie("bruiser-1", ZombieType.Bruiser);
            system.RecordContribution(firstZombie, DamageSource.Haetae("haetae-1"), 1f, robots);

            var unlock = system.AwardForDeath(firstZombie, 25, robots, config).Single();

            Assert.That(unlock.RewardAmount, Is.EqualTo(25));
            Assert.That(unlock.AppliedAmount, Is.EqualTo(25));
            Assert.That(unlock.LevelBefore, Is.EqualTo(1));
            Assert.That(unlock.LevelAfter, Is.EqualTo(2));
            Assert.That(unlock.LevelReached, Is.True);
            Assert.That(unlock.SpecializationUnlocked, Is.True);
            Assert.That(robots[0].Progression.Experience, Is.EqualTo(115));
            Assert.That(robots[0].Progression.Level, Is.EqualTo(2));
            Assert.That(robots[0].Progression.SpecializationReady, Is.True);

            var secondZombie = DeadZombie("bruiser-2", ZombieType.Bruiser);
            system.RecordContribution(secondZombie, DamageSource.Haetae("haetae-1"), 1f, robots);
            var laterLevel = system.AwardForDeath(secondZombie, 100, robots, config).Single();

            Assert.That(laterLevel.AppliedAmount, Is.EqualTo(100));
            Assert.That(laterLevel.LevelBefore, Is.EqualTo(2));
            Assert.That(laterLevel.LevelAfter, Is.EqualTo(3));
            Assert.That(laterLevel.LevelReached, Is.True);
            Assert.That(laterLevel.SpecializationUnlocked, Is.False);
            Assert.That(laterLevel.MasteryPointsGained, Is.EqualTo(1));
            Assert.That(robots[0].Progression.Experience, Is.EqualTo(215));
            Assert.That(robots[0].Progression.Level, Is.EqualTo(3));
            Assert.That(robots[0].Progression.SpecializationReady, Is.True);
            Assert.That(robots[0].Progression.UnspentMasteryPoints, Is.EqualTo(1));
        }

        [Test]
        public void MasteryPointsRequireSpecializationAndStackIndependently()
        {
            var first = robots[0];
            first.Progression.Level = 4;
            first.Progression.Experience = 300;
            first.Progression.UnspentMasteryPoints = 3;

            Assert.That(system.SelectMasteryUpgrade(first, HaetaeMasteryUpgrade.Power),
                Is.EqualTo(MasterySelectionResult.NotSpecialized));
            Assert.That(system.SelectSpecialization(first, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(system.SelectMasteryUpgrade(first, HaetaeMasteryUpgrade.Power),
                Is.EqualTo(MasterySelectionResult.Selected));
            Assert.That(system.SelectMasteryUpgrade(first, HaetaeMasteryUpgrade.AttackSpeed),
                Is.EqualTo(MasterySelectionResult.Selected));
            Assert.That(system.SelectMasteryUpgrade(first, HaetaeMasteryUpgrade.AttackSpeed),
                Is.EqualTo(MasterySelectionResult.Selected));
            Assert.That(system.SelectMasteryUpgrade(first, HaetaeMasteryUpgrade.Armor),
                Is.EqualTo(MasterySelectionResult.NoPoint));

            Assert.That(first.Progression.PowerRank, Is.EqualTo(1));
            Assert.That(first.Progression.AttackSpeedRank, Is.EqualTo(2));
            Assert.That(first.Progression.UnspentMasteryPoints, Is.Zero);
            Assert.That(robots[1].Progression.PowerRank, Is.Zero);
            Assert.That(robots[1].Progression.AttackSpeedRank, Is.Zero);
        }

        [Test]
        public void MasteryMultipliersUseConfiguredRanksAndReductionFloor()
        {
            var progression = robots[0].Progression;
            progression.PowerRank = 3;
            progression.ArmorRank = 10;
            progression.EfficiencyRank = 10;
            progression.AttackSpeedRank = 10;

            Assert.That(config.DamageMultiplier(progression), Is.EqualTo(1.3f).Within(0.0001f));
            Assert.That(config.IncomingDamageMultiplier(progression), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(config.CombatBatteryMultiplier(progression), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(config.AttackCooldownMultiplier(progression), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SharedKillCanLevelBothRobotsWithoutCrossMutation()
        {
            foreach (var robot in robots) robot.Progression.Experience = 95;
            var zombie = DeadZombie("runner", ZombieType.Runner);
            foreach (var robot in robots)
                system.RecordContribution(zombie, DamageSource.Haetae(robot.Id), 1f, robots);

            var awards = system.AwardForDeath(zombie, 5, robots, config);

            Assert.That(awards.All(item => item.LevelReached), Is.True);
            Assert.That(robots.All(item => item.Progression.SpecializationReady), Is.True);
        }

        [Test]
        public void OneHundredAwardsNeverMutateTheOtherRobot()
        {
            for (var iteration = 0; iteration < 100; iteration++)
            {
                robots[0].Progression.Experience = 0;
                robots[0].Progression.Level = 1;
                robots[1].Progression.Experience = iteration % 100;
                var expectedOther = robots[1].Progression.Experience;
                var zombie = DeadZombie("runner-" + iteration, ZombieType.Runner);
                system.RecordContribution(zombie, DamageSource.Haetae("haetae-1"), 1f, robots);
                system.AwardForDeath(zombie, 5, robots, config);
                Assert.That(robots[1].Progression.Experience, Is.EqualTo(expectedOther));
            }
        }

        private static ZombieState DeadZombie(string id, ZombieType type)
        {
            var zombie = new ZombieState(id, type, RouteId.NorthRoad, 10f);
            zombie.Health.Current = 0f;
            return zombie;
        }
    }
}
