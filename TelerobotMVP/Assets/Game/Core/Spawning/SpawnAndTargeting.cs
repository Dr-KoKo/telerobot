using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public sealed class TargetCandidate
    {
        public string Id;
        public TargetKind Kind;
        public float Distance;
        public bool IsAlive;

        public TargetCandidate(string id, TargetKind kind, float distance, bool isAlive)
        {
            Id = id;
            Kind = kind;
            Distance = distance;
            IsAlive = isAlive;
        }
    }

    public sealed class SpawnSystem
    {
        private readonly GameplayConfig config;

        public SpawnSystem(GameplayConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public List<SpawnEntry> Compose(PhaseConfig phase, IDeterministicRng rng)
        {
            if (phase == null) throw new ArgumentNullException("phase");
            if (rng == null) throw new ArgumentNullException("rng");

            var counts = new Dictionary<ZombieType, int>
            {
                { ZombieType.Runner, Math.Max(0, phase.RunnerTarget) },
                { ZombieType.Bruiser, Math.Max(0, phase.BruiserTarget) },
                { ZombieType.Ripper, Math.Max(0, phase.RipperTarget) }
            };
            while (ThreatCost(counts) > phase.ThreatBudget && counts[ZombieType.Runner] > 0)
            {
                counts[ZombieType.Runner]--;
            }
            while (ThreatCost(counts) > phase.ThreatBudget && counts[ZombieType.Bruiser] > 0 && phase.Number < 3)
            {
                counts[ZombieType.Bruiser]--;
            }
            if (ThreatCost(counts) > phase.ThreatBudget)
                throw new InvalidOperationException("Special minimums exceed phase threat budget.");

            var types = new List<ZombieType>();
            foreach (var pair in counts)
            {
                for (var index = 0; index < pair.Value; index++) types.Add(pair.Key);
            }
            for (var index = types.Count - 1; index > 0; index--)
            {
                var swap = rng.NextInt(index + 1);
                var held = types[index];
                types[index] = types[swap];
                types[swap] = held;
            }

            var result = new List<SpawnEntry>(types.Count);
            for (var index = 0; index < types.Count; index++)
            {
                var routeIndex = phase.OpenRoutes.Length == 1 ? 0 : rng.NextInt(phase.OpenRoutes.Length);
                if (types[index] == ZombieType.Ripper && Array.IndexOf(phase.OpenRoutes, RouteId.SouthTunnel) >= 0 && index % 3 != 0)
                    routeIndex = Array.IndexOf(phase.OpenRoutes, RouteId.SouthTunnel);
                result.Add(new SpawnEntry(types[index], phase.OpenRoutes[routeIndex]));
            }
            return result;
        }

        public int ThreatCost(IDictionary<ZombieType, int> counts)
        {
            var total = 0;
            foreach (var pair in counts) total += config.GetZombie(pair.Key).ThreatCost * pair.Value;
            return total;
        }
    }

    public static class TargetingSystem
    {
        public static TargetCandidate Select(ZombieConfig zombie, IEnumerable<TargetCandidate> candidates)
        {
            if (zombie == null) throw new ArgumentNullException("zombie");
            foreach (var desiredKind in zombie.TargetPriority)
            {
                TargetCandidate best = null;
                foreach (var candidate in candidates)
                {
                    if (candidate == null || !candidate.IsAlive || candidate.Kind != desiredKind) continue;
                    if (best == null || candidate.Distance < best.Distance ||
                        (Math.Abs(candidate.Distance - best.Distance) < 0.0001f && string.CompareOrdinal(candidate.Id, best.Id) < 0))
                        best = candidate;
                }
                if (best != null) return best;
            }
            return null;
        }
    }
}
