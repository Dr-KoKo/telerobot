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
                { ZombieType.Runner, SampleCount(phase.RunnerCount, phase.RunnerMinimum, rng) },
                { ZombieType.Bruiser, SampleCount(phase.BruiserCount, phase.BruiserMinimum, rng) },
                { ZombieType.Ripper, SampleCount(phase.RipperCount, phase.RipperMinimum, rng) }
            };
            var trimOrder = phase.TrimOrder ?? Array.Empty<SpawnTrimTarget>();
            foreach (var target in trimOrder)
            {
                var type = ToZombieType(target);
                var minimum = Math.Max(MinimumFor(phase, type), RangeFor(phase, type).Min);
                while (ThreatCost(counts) > phase.ThreatBudget && counts[type] > minimum) counts[type]--;
            }
            if (ThreatCost(counts) > phase.ThreatBudget)
                throw new InvalidOperationException("Special minimums exceed phase threat budget.");

            var result = new List<SpawnEntry>();
            foreach (var pair in counts)
            {
                var routes = AllocateRoutes(phase, pair.Key, pair.Value);
                for (var index = 0; index < routes.Count; index++) result.Add(new SpawnEntry(pair.Key, routes[index]));
            }
            for (var index = result.Count - 1; index > 0; index--)
            {
                var swap = rng.NextInt(index + 1);
                var held = result[index];
                result[index] = result[swap];
                result[swap] = held;
            }
            return result;
        }

        private static int SampleCount(IntRangeConfig range, int minimum, IDeterministicRng rng)
        {
            if (range == null) throw new InvalidOperationException("Phase composition range is missing.");
            return Math.Max(minimum, Math.Max(0, range.Sample(rng)));
        }

        private static ZombieType ToZombieType(SpawnTrimTarget target)
        {
            switch (target)
            {
                case SpawnTrimTarget.Runner: return ZombieType.Runner;
                case SpawnTrimTarget.Bruiser: return ZombieType.Bruiser;
                default: return ZombieType.Ripper;
            }
        }

        private static int MinimumFor(PhaseConfig phase, ZombieType type)
        {
            if (type == ZombieType.Runner) return phase.RunnerMinimum;
            if (type == ZombieType.Bruiser) return phase.BruiserMinimum;
            return phase.RipperMinimum;
        }

        private static IntRangeConfig RangeFor(PhaseConfig phase, ZombieType type)
        {
            if (type == ZombieType.Runner) return phase.RunnerCount;
            if (type == ZombieType.Bruiser) return phase.BruiserCount;
            return phase.RipperCount;
        }

        private static List<RouteId> AllocateRoutes(PhaseConfig phase, ZombieType type, int count)
        {
            var weights = phase.RouteWeights;
            if (phase.ZombieTypeRouteWeights != null)
            {
                var typed = Array.Find(phase.ZombieTypeRouteWeights, item => item != null && item.Type == type);
                if (typed != null && typed.Routes != null && typed.Routes.Length > 0) weights = typed.Routes;
            }

            var activeWeights = new List<RouteWeightConfig>();
            foreach (var route in phase.OpenRoutes)
            {
                var configured = weights == null ? null : Array.Find(weights, item => item != null && item.Route == route);
                activeWeights.Add(new RouteWeightConfig { Route = route, Weight = configured == null ? 1f : Math.Max(0f, configured.Weight) });
            }

            var totalWeight = 0f;
            foreach (var item in activeWeights) totalWeight += item.Weight;
            if (totalWeight <= 0f) throw new InvalidOperationException("Spawn route weights must include a positive weight.");

            var allocations = new int[activeWeights.Count];
            var remainders = new float[activeWeights.Count];
            var assigned = 0;
            for (var index = 0; index < activeWeights.Count; index++)
            {
                var exact = count * activeWeights[index].Weight / totalWeight;
                allocations[index] = (int)Math.Floor(exact);
                remainders[index] = exact - allocations[index];
                assigned += allocations[index];
            }
            while (assigned < count)
            {
                var best = 0;
                for (var index = 1; index < remainders.Length; index++)
                    if (remainders[index] > remainders[best]) best = index;
                allocations[best]++;
                remainders[best] = -1f;
                assigned++;
            }

            var routes = new List<RouteId>(count);
            for (var index = 0; index < allocations.Length; index++)
                for (var routeCount = 0; routeCount < allocations[index]; routeCount++) routes.Add(activeWeights[index].Route);
            return routes;
        }

        public int ThreatCost(IDictionary<ZombieType, int> counts)
        {
            var total = 0;
            foreach (var pair in counts) total += config.GetZombie(pair.Key).ThreatCost * pair.Value;
            return total;
        }
    }

    public sealed class ContinuousSpawnScheduler
    {
        private readonly PhaseConfig phase;
        private readonly IDeterministicRng rng;
        private float untilNextGroup;

        public ContinuousSpawnScheduler(PhaseConfig phase, IDeterministicRng rng)
        {
            if (phase == null) throw new ArgumentNullException("phase");
            if (rng == null) throw new ArgumentNullException("rng");
            if (phase.GroupSize == null || phase.GroupSize.Min <= 0 || phase.GroupSize.Max < phase.GroupSize.Min)
                throw new InvalidOperationException("Spawn group size is invalid.");
            if (phase.MaxAliveConcurrent <= 0 || phase.GroupIntervalSeconds <= 0f || phase.PhaseStartDelaySeconds < 0f)
                throw new InvalidOperationException("Spawn schedule timing or concurrent cap is invalid.");
            this.phase = phase;
            this.rng = rng;
            untilNextGroup = phase.PhaseStartDelaySeconds;
        }

        public int Advance(float deltaTime, int aliveCount, int remainingCount)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException("deltaTime");
            if (remainingCount <= 0) return 0;
            untilNextGroup = Math.Max(0f, untilNextGroup - deltaTime);
            if (untilNextGroup > 0f) return 0;

            var capacity = Math.Max(0, phase.MaxAliveConcurrent - Math.Max(0, aliveCount));
            if (capacity <= 0) return 0;
            var groupSize = phase.GroupSize.Sample(rng);
            var spawned = Math.Min(remainingCount, Math.Min(capacity, groupSize));
            if (spawned > 0) untilNextGroup = phase.GroupIntervalSeconds;
            return spawned;
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
