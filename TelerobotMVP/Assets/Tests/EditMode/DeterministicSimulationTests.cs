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
            var firstSummary = simulator.Run(1001, first);
            var secondSummary = simulator.Run(1001, second);
            Assert.That(first.CanonicalText(), Is.EqualTo(second.CanonicalText()));
            Assert.That(firstSummary.Result, Is.EqualTo(GameResult.Victory));
            Assert.That(firstSummary.DurationSeconds, Is.InRange(600f, 900f));
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
