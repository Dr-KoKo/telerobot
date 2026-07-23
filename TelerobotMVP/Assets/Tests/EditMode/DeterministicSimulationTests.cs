using NUnit.Framework;
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
            Assert.That(TelemetryEventNames.ConstitutionMinimum.Length, Is.EqualTo(17));
        }
    }
}
