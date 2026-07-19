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
        public int runnerTarget;
        public int bruiserTarget;
        public int ripperTarget;
    }
}
