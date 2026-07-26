using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class PhaseDefinitionAsset : ScriptableObject
    {
        public int number;
        public int threatBudget;
        public float targetDurationSeconds;
        public RouteId[] openRoutes;
        public bool opensNewRoute;
        public RouteId newlyOpenedRoute;
        public IntRangeConfig runnerCount;
        public IntRangeConfig bruiserCount;
        public IntRangeConfig ripperCount;
        public IntRangeConfig learningTotal;
        public int runnerMinimum;
        public int bruiserMinimum;
        public int ripperMinimum;
        public SpawnTrimTarget[] trimOrder;
        public float phaseStartDelaySeconds;
        public float groupIntervalSeconds;
        public IntRangeConfig groupSize;
        public int maxAliveConcurrent;
        public RouteWeightConfig[] routeWeights;
        public ZombieRouteWeightConfig[] zombieTypeRouteWeights;
    }
}
