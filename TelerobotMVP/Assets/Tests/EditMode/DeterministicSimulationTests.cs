using NUnit.Framework;
using System.Linq;
using Telerobot.Game.Core;
using Telerobot.Game.Simulation;

namespace Telerobot.Game.Tests
{
    public sealed class DeterministicSimulationTests
    {
        [Test]
        public void SameSeedAndDataVersionProduceIdenticalTelemetry()
        {
            var config = TestConfigFactory.Create();
            Assert.That(config.Validation.FixedStepSeconds, Is.EqualTo(1f / 60f));
            var simulator = new DeterministicSessionSimulator(config);
            var first = new InMemoryTelemetrySink();
            var second = new InMemoryTelemetrySink();
            var firstSummary = simulator.Run(1001, SimProfileId.Baseline, first);
            var secondSummary = simulator.Run(1001, SimProfileId.Baseline, second);
            Assert.That(first.CanonicalText(), Is.EqualTo(second.CanonicalText()));
            Assert.That(firstSummary.Result, Is.EqualTo(secondSummary.Result));
            Assert.That(firstSummary.DurationSeconds, Is.EqualTo(secondSummary.DurationSeconds));
            Assert.That(firstSummary.DurationSeconds, Is.GreaterThan(0f));
            Assert.That(firstSummary.PeakAliveCount, Is.LessThanOrEqualTo(firstSummary.PeakAliveCap));
            foreach (var record in first.Records)
                Assert.That(record.SimProfileId, Is.EqualTo(SimProfileId.Baseline.ToString()));
        }

        [Test]
        public void ProfilesDriveDifferentPlayerBehaviorAndSimulationCanDefeat()
        {
            var config = TestConfigFactory.Create();
            var simulator = new DeterministicSessionSimulator(config);
            var novice = new InMemoryTelemetrySink();
            var skilled = new InMemoryTelemetrySink();
            simulator.Run(1002, SimProfileId.Novice, novice);
            simulator.Run(1002, SimProfileId.Skilled, skilled);
            Assert.That(novice.CanonicalText(), Is.Not.EqualTo(skilled.CanonicalText()));

            config.Base.MaxHealth = 1f;
            config.GetSimPlayerProfile(SimProfileId.Novice).AimAccuracy = 0f;
            config.Robot.DashDamage = 0.01f;
            config.Robot.BiteDamage = 0.01f;
            var defeat = simulator.Run(1003, SimProfileId.Novice, new InMemoryTelemetrySink());
            Assert.That(defeat.Result, Is.EqualTo(GameResult.Defeat));
            Assert.That(defeat.DefeatReason, Is.EqualTo(DefeatReason.BaseDestroyed));
        }

        [Test]
        public void BaselineBalanceEvaluationReportsAllSuccessCriteriaInputs()
        {
            var config = TestConfigFactory.Create();
            var report = new DeterministicSessionSimulator(config).EvaluateBalance(
                new[] { 1001, 1002, 1003 }, SimProfileId.Baseline);
            Assert.That(report.ProfileId, Is.EqualTo(SimProfileId.Baseline));
            Assert.That(report.RunCount, Is.EqualTo(3));
            Assert.That(report.AverageDurationSeconds, Is.GreaterThan(0f));
            Assert.That(report.PhaseOneClearRate, Is.InRange(0f, 1f));
            Assert.That(report.PhaseTwoClearRate, Is.InRange(0f, 1f));
            Assert.That(report.PhaseThreeClearRate, Is.InRange(0f, 1f));
        }

        [Test]
        public void SampledTelemetryUsesConfiguredSimulationClockCadences()
        {
            var config = TestConfigFactory.Create();
            var sink = new InMemoryTelemetrySink();
            new DeterministicSessionSimulator(config).Run(1001, SimProfileId.Baseline, sink);
            var baseSamples = sink.Records.FindAll(item => item.EventName == "base_hp_sampled" && item.Phase == 1);
            var pressureSamples = sink.Records.FindAll(item => item.EventName == "route_pressure_sampled" && item.Phase == 1);
            Assert.That(baseSamples.Count, Is.GreaterThan(2));
            Assert.That(pressureSamples.Count, Is.GreaterThan(2));
            Assert.That(baseSamples[1].SimTime - baseSamples[0].SimTime,
                Is.EqualTo(config.Telemetry.SampleIntervalSeconds).Within(config.Validation.FixedStepSeconds * 2f));
            Assert.That(pressureSamples[1].SimTime - pressureSamples[0].SimTime,
                Is.EqualTo(config.Telemetry.RoutePressureSampleIntervalSeconds).Within(config.Validation.FixedStepSeconds * 2f));
            Assert.That(config.Telemetry.RequiredFields, Does.Contain("simProfileId"));
        }

        [Test]
        public void MinimumTelemetrySchemaIsDeclared()
        {
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("simulation_run_completed"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("ripper_attacked_robot"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("haetae_xp_gained"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Contain("haetae_specialization_selected"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum, Does.Not.Contain("upgrade_selected"));
            Assert.That(TelemetryEventNames.ConstitutionMinimum.Length, Is.EqualTo(20));
        }

        [Test]
        public void ProgressionEventsAreDeterministicAndSharedContributorsReceiveFullReward()
        {
            var config = TestConfigFactory.Create();
            var simulator = new DeterministicSessionSimulator(config);
            var first = new InMemoryTelemetrySink();
            var second = new InMemoryTelemetrySink();

            simulator.Run(1001, SimProfileId.Baseline, first);
            simulator.Run(1001, SimProfileId.Baseline, second);

            var firstProgression = first.Records.Where(item => item.EventName.StartsWith("haetae_")).ToArray();
            var secondProgression = second.Records.Where(item => item.EventName.StartsWith("haetae_")).ToArray();
            Assert.That(firstProgression.Length, Is.GreaterThan(0));
            Assert.That(firstProgression.Select(JsonLinesTelemetrySink.ToJson),
                Is.EqualTo(secondProgression.Select(JsonLinesTelemetrySink.ToJson)));

            var sharedZombie = firstProgression.Where(item => item.EventName == "haetae_xp_gained")
                .GroupBy(item => item.Payload["zombieId"])
                .FirstOrDefault(group => group.Select(item => item.Payload["robotId"]).Distinct().Count() == 2);
            Assert.That(sharedZombie, Is.Not.Null);
            Assert.That(sharedZombie.All(item => item.Payload["rewardAmount"] == item.Payload["appliedAmount"]), Is.True);
        }

        [Test]
        public void SpecializationLoadoutOverrideDoesNotChangeSpawnStream()
        {
            var config = TestConfigFactory.Create();
            var simulator = new DeterministicSessionSimulator(config);
            var meleeRanged = new InMemoryTelemetrySink();
            var rangedMelee = new InMemoryTelemetrySink();

            simulator.Run(1001, SimProfileId.Baseline, meleeRanged, new SimulationRunOptions
            {
                SpecializationLoadout =
                    new HaetaeSpecializationPair(HaetaeSpecialization.Melee, HaetaeSpecialization.Ranged)
            });
            simulator.Run(1001, SimProfileId.Baseline, rangedMelee, new SimulationRunOptions
            {
                SpecializationLoadout =
                    new HaetaeSpecializationPair(HaetaeSpecialization.Ranged, HaetaeSpecialization.Melee)
            });

            var firstSpawns = meleeRanged.Records.Where(item => item.EventName == "zombie_spawned")
                .Select(item => item.Phase + ":" + item.Payload["type"] + ":" + item.Payload["routeId"]);
            var secondSpawns = rangedMelee.Records.Where(item => item.EventName == "zombie_spawned")
                .Select(item => item.Phase + ":" + item.Payload["type"] + ":" + item.Payload["routeId"]);
            Assert.That(firstSpawns, Is.EqualTo(secondSpawns));
        }

        [Test]
        public void EightPhasesProgressWithoutUpgradeSelectionOrUpgradeRandomness()
        {
            var config = TestConfigFactory.Create();
            config.Base.MaxHealth = 100000f;
            config.Game.PlayerMaxHealth = 100000f;
            config.Game.TargetSessionMaximumSeconds = 5000f;
            foreach (var zombie in config.Zombies)
            {
                zombie.MaxHealth = 1f;
                zombie.BaseDamage = 0f;
                zombie.PlayerDamage = 0f;
                zombie.RobotDamage = 0f;
            }
            var simulator = new DeterministicSessionSimulator(config);
            var first = new InMemoryTelemetrySink();
            var second = new InMemoryTelemetrySink();

            var firstSummary = simulator.Run(1001, SimProfileId.Baseline, first);
            var secondSummary = simulator.Run(1001, SimProfileId.Baseline, second);

            Assert.That(firstSummary.PhasesCleared, Is.EqualTo(8));
            Assert.That(firstSummary.Result, Is.EqualTo(GameResult.Victory));
            Assert.That(first.Records.Count(item => item.EventName == "phase_started"), Is.EqualTo(8));
            Assert.That(first.Records.Count(item => item.EventName == "phase_started" && item.Phase == 8),
                Is.EqualTo(1));
            Assert.That(first.Records.Any(item => item.EventName == "upgrade_selected"), Is.False);
            Assert.That(second.Records.Any(item => item.EventName == "upgrade_selected"), Is.False);
            Assert.That(first.CanonicalText(), Is.EqualTo(second.CanonicalText()));
        }

        [Test]
        public void AllNineOrderedLoadoutsProduceDeterministicPerRobotRoleMetrics()
        {
            var roles = new[]
            {
                HaetaeSpecialization.Melee,
                HaetaeSpecialization.Ranged,
                HaetaeSpecialization.Balanced
            };
            var config = TestConfigFactory.Create();
            var simulator = new DeterministicSessionSimulator(config);
            foreach (var firstRole in roles)
            foreach (var secondRole in roles)
            {
                var options = new SimulationRunOptions
                {
                    SpecializationLoadout = new HaetaeSpecializationPair(firstRole, secondRole)
                };
                var first = simulator.Run(1001, SimProfileId.Baseline, new InMemoryTelemetrySink(), options);
                var second = simulator.Run(1001, SimProfileId.Baseline, new InMemoryTelemetrySink(), options);
                Assert.That(first.Haetae1Specialization, Is.EqualTo(firstRole));
                Assert.That(first.Haetae2Specialization, Is.EqualTo(secondRole));
                Assert.That(first.Haetae1DamageDealt, Is.EqualTo(second.Haetae1DamageDealt));
                Assert.That(first.Haetae2DamageDealt, Is.EqualTo(second.Haetae2DamageDealt));
                Assert.That(first.Haetae1CombatBatterySpent, Is.GreaterThanOrEqualTo(0f));
                Assert.That(first.Haetae2CombatBatterySpent, Is.GreaterThanOrEqualTo(0f));
            }
        }
    }
}
