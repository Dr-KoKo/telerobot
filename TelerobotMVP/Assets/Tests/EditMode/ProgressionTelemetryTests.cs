using System;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Simulation;

namespace Telerobot.Game.Tests
{
    public sealed class ProgressionTelemetryTests
    {
        [Test]
        public void MvpTwoRecordsCarryRequiredEnvelopeAndRetireUpgradeSelection()
        {
            var sink = RunBaseline();

            Assert.That(sink.Records, Is.Not.Empty);
            foreach (var record in sink.Records)
            {
                Assert.That(record.BuildVersion, Is.Not.Empty);
                Assert.That(record.DataVersion, Is.EqualTo("mvp-2.0.0"));
                Assert.That(record.SessionId, Is.Not.Empty);
                Assert.That(record.Seed, Is.EqualTo(1001));
                Assert.That(record.SimProfileId, Is.EqualTo("Baseline"));
                Assert.That(record.Phase, Is.InRange(0, 8));
                Assert.That(record.SimTime, Is.GreaterThanOrEqualTo(0f));
                Assert.That(record.EventName, Is.Not.Empty);
                Assert.That(record.Payload, Is.Not.Null);
            }

            Assert.That(sink.Records.Any(item => item.EventName == "upgrade_selected"), Is.False);
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("robot_auto_charge_started"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("haetae_specialization_selected"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Not.Contain("upgrade_selected"));

            var summary = sink.Records.Single(item => item.EventName == "simulation_run_completed");
            foreach (var key in new[]
                     {
                         "haetae1Level2Phase", "haetae1Level2SimTime",
                         "haetae2Level2Phase", "haetae2Level2SimTime",
                         "firstLevel2WithinPhase2SixtySeconds", "bothLevel2BeforePhase3",
                         "haetae1Specialization", "haetae2Specialization",
                         "haetae1DamageDealt", "haetae2DamageDealt",
                         "haetae1KillsContributed", "haetae2KillsContributed",
                         "haetae1CombatBatterySpent", "haetae2CombatBatterySpent",
                         "haetae1DisabledCount", "haetae2DisabledCount",
                         "haetae1DestroyedCount", "haetae2DestroyedCount",
                         "baseHealthRemaining"
                     })
                Assert.That(summary.Payload.ContainsKey(key), Is.True, "Missing summary metric " + key);
        }

        [Test]
        public void LethalHitOrdersProgressionBeforeKillAndNeverDuplicatesRewards()
        {
            var records = RunBaseline().Records;
            var xpEvents = records.Where(item => item.EventName == "haetae_xp_gained").ToArray();
            var killEvents = records.Where(item => item.EventName == "zombie_killed").ToArray();

            Assert.That(xpEvents, Is.Not.Empty);
            Assert.That(killEvents, Is.Not.Empty);
            Assert.That(xpEvents.GroupBy(item => item.Payload["robotId"] + "|" + item.Payload["zombieId"])
                .All(group => group.Count() == 1), Is.True);
            Assert.That(killEvents.GroupBy(item => item.Payload["zombieId"]).All(group => group.Count() == 1), Is.True);

            foreach (var kill in killEvents.Where(item =>
                         int.Parse(item.Payload["contributingHaetaeCount"]) > 0))
            {
                var killIndex = records.IndexOf(kill);
                var zombieXp = xpEvents.Where(item => item.Payload["zombieId"] == kill.Payload["zombieId"])
                    .OrderBy(item => records.IndexOf(item)).ToArray();
                var robotIds = zombieXp.Select(item => item.Payload["robotId"]).ToArray();
                Assert.That(robotIds, Is.EqualTo(robotIds.OrderBy(item => item, StringComparer.Ordinal)));
                Assert.That(zombieXp.All(item => records.IndexOf(item) < killIndex), Is.True);
                var contributorIds = kill.Payload["contributingHaetaeIds"].Split('|');
                Assert.That(contributorIds, Is.EqualTo(contributorIds.OrderBy(item => item, StringComparer.Ordinal)));
                Assert.That(robotIds.All(item => contributorIds.Contains(item)), Is.True);
            }

            var levels = records.Where(item => item.EventName == "haetae_level_reached").ToArray();
            Assert.That(levels.Any(item => int.Parse(item.Payload["toLevel"]) >= 3), Is.True);
            Assert.That(records.Where(item => item.EventName == "haetae_specialization_ready")
                .GroupBy(item => item.Payload["robotId"]).All(group => group.Count() == 1), Is.True);
            Assert.That(records.Any(item => item.EventName == "haetae_mastery_point_gained"), Is.True);
            Assert.That(records.Any(item => item.EventName == "haetae_mastery_selected"), Is.True);
            Assert.That(records.Where(item => item.EventName == "haetae_mastery_selected")
                .All(item => item.Payload.ContainsKey("attackSpeedRank")), Is.True);

            foreach (var level in levels)
            {
                var levelIndex = records.IndexOf(level);
                var readyIndex = records.FindIndex(levelIndex + 1, item =>
                    item.EventName == "haetae_specialization_ready" &&
                    item.Payload["robotId"] == level.Payload["robotId"]);
                var xpIndex = records.FindLastIndex(levelIndex - 1, item =>
                    item.EventName == "haetae_xp_gained" &&
                    item.Payload["robotId"] == level.Payload["robotId"]);
                var killIndex = records.FindIndex(levelIndex + 1, item =>
                    item.EventName == "zombie_killed" && item.SimTime == level.SimTime);

                Assert.That(xpIndex, Is.GreaterThanOrEqualTo(0));
                Assert.That(xpIndex, Is.LessThan(levelIndex));
                if (int.Parse(level.Payload["toLevel"]) == 2)
                {
                    Assert.That(readyIndex, Is.EqualTo(levelIndex + 1));
                    Assert.That(killIndex, Is.GreaterThan(readyIndex));
                }
                else
                {
                    Assert.That(readyIndex == levelIndex + 1, Is.False);
                    Assert.That(killIndex, Is.GreaterThan(levelIndex));
                }
            }
        }

        private static InMemoryTelemetrySink RunBaseline()
        {
            var sink = new InMemoryTelemetrySink();
            var config = TestConfigFactory.Create();
            config.Base.MaxHealth = 100000f;
            config.Game.PlayerMaxHealth = 100000f;
            config.Game.TargetSessionMaximumSeconds = 5000f;
            config.Weapon.GrenadesPerPhase = 0;
            config.GetSimPlayerProfile(Telerobot.Game.Core.SimProfileId.Baseline).AimAccuracy = 0f;
            foreach (var zombie in config.Zombies)
            {
                zombie.MaxHealth = 1f;
                zombie.BaseDamage = 0f;
                zombie.PlayerDamage = 0f;
                zombie.RobotDamage = 0f;
            }
            new DeterministicSessionSimulator(config)
                .Run(1001, Telerobot.Game.Core.SimProfileId.Baseline, sink);
            return sink;
        }
    }
}
