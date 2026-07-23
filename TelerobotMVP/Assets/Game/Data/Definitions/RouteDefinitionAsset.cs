using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class RouteDefinitionAsset : ScriptableObject
    {
        public RouteId id;
        public int openPhase;
        public string displayNameKey;
        public Vector3[] waypoints;
        public Color routeColor;
        public float width;
    }
}
