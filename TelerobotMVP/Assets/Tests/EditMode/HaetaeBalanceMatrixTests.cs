using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Simulation;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeBalanceMatrixTests
    {
        private static readonly HaetaeSpecialization[] Roles =
        {
            HaetaeSpecialization.Melee,
            HaetaeSpecialization.Ranged,
            HaetaeSpecialization.Balanced
        };

        [Test]
        public void TwentySeedsAcrossNineOrderedLoadoutsMeetAutomatedPhaseTwoCriteria()
        {
            var config = TestConfigFactory.Create();
            var simulator = new DeterministicSessionSimulator(config);
            var summaries = new List<SimulationSummary>();

            for (var seed = 1101; seed <= 1120; seed++)
            foreach (var firstRole in Roles)
            foreach (var secondRole in Roles)
            {
                summaries.Add(simulator.Run(seed, SimProfileId.Baseline, new InMemoryTelemetrySink(),
                    new SimulationRunOptions
                    {
                        SpecializationLoadout = new HaetaeSpecializationPair(firstRole, secondRole)
                    }));
            }

            var firstReadyRate = summaries.Count(item => item.FirstLevel2WithinPhase2SixtySeconds) /
                                 (float)summaries.Count;
            var phaseThreeEligible = summaries.Where(item => item.PhasesCleared >= 2).ToArray();
            var bothReadyRate = phaseThreeEligible.Length == 0
                ? 0f
                : phaseThreeEligible.Count(item => item.BothLevel2BeforePhase3) /
                  (float)phaseThreeEligible.Length;
            var averageDuration = summaries.Average(item => item.DurationSeconds);
            var phaseTwoClearRate = phaseThreeEligible.Length / (float)summaries.Count;
            var durationTargetMet = averageDuration >= config.Game.TargetSessionMinimumSeconds &&
                                    averageDuration <= config.Game.TargetSessionMaximumSeconds;
            var loadoutClears = string.Join(", ", summaries
                .GroupBy(item => item.Haetae1Specialization + "/" + item.Haetae2Specialization)
                .OrderBy(group => group.Key)
                .Select(group => group.Key + "=" + group.Count(item => item.PhasesCleared >= 2) + "/" + group.Count()));
            var diagnostic = "runs=" + summaries.Count +
                             ", firstReadyRate=" + firstReadyRate.ToString("P1") +
                             ", bothReadyAmongPhase3Eligible=" + bothReadyRate.ToString("P1") +
                             ", phaseTwoClearRate=" + phaseTwoClearRate.ToString("P1") +
                             ", averageDuration=" + averageDuration.ToString("F1") +
                             ", durationTargetMet=" + durationTargetMet +
                             ", durationRange=" + summaries.Min(item => item.DurationSeconds).ToString("F1") +
                             "-" + summaries.Max(item => item.DurationSeconds).ToString("F1") +
                             ", phase1Clears=" + summaries.Count(item => item.PhasesCleared >= 1) +
                             ", phase2Clears=" + summaries.Count(item => item.PhasesCleared >= 2) +
                             ", phase3Clears=" + summaries.Count(item => item.PhasesCleared >= 3) +
                             ", haetae1Level2=" + summaries.Count(item => item.Haetae1Level2SimTime >= 0f) +
                             ", haetae2Level2=" + summaries.Count(item => item.Haetae2Level2SimTime >= 0f) +
                             ", baseDefeats=" + summaries.Count(item => item.DefeatReason == DefeatReason.BaseDestroyed) +
                             ", playerDefeats=" + summaries.Count(item => item.DefeatReason == DefeatReason.PlayerDeath) +
                             ", avgRobotDamage=" + summaries.Average(item =>
                                 item.Haetae1DamageDealt + item.Haetae2DamageDealt).ToString("F1") +
                             ", victories=" + summaries.Count(item => item.Result == GameResult.Victory) +
                             ", loadoutPhase2Clears=[" + loadoutClears + "]";

            TestContext.WriteLine(diagnostic);
            Assert.That(firstReadyRate >= 0.8f &&
                        bothReadyRate >= 0.8f &&
                        phaseThreeEligible.Length > 0,
                Is.True, diagnostic);
        }

        [Test]
        public void GoldenSeedIsByteStableAcrossEveryOrderedLoadout()
        {
            var simulator = new DeterministicSessionSimulator(TestConfigFactory.Create());
            foreach (var firstRole in Roles)
            foreach (var secondRole in Roles)
            {
                var options = new SimulationRunOptions
                {
                    SpecializationLoadout = new HaetaeSpecializationPair(firstRole, secondRole)
                };
                var first = new InMemoryTelemetrySink();
                var second = new InMemoryTelemetrySink();
                simulator.Run(9001, SimProfileId.Baseline, first, options);
                simulator.Run(9001, SimProfileId.Baseline, second, options);
                Assert.That(first.CanonicalText(), Is.EqualTo(second.CanonicalText()),
                    firstRole + "/" + secondRole);
            }
        }
    }
}
