using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class SupplyPointDefinitionAsset : ScriptableObject
    {
        public string id;
        public SupplyKind kind;
        public Vector3 position;
        public float interactionRadius;
    }
}
