using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class GrenadeDefinitionAsset : ScriptableObject
    {
        public float radius;
        public float innerRadius;
        public float centerDamage;
        public float edgeDamage;
        public int maxTargets;
        public float throwDistance;
    }
}
