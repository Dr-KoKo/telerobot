using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class ZombieDefinitionAsset : ScriptableObject
    {
        public ZombieType type;
        public float maxHealth;
        public float moveSpeed;
        public float baseDamage;
        public float playerDamage;
        public float robotDamage;
        public float attackInterval;
        public float attackRange;
        [Range(0f, 0.45f)] public float pathVariationFraction;
        [Min(0.1f)] public float separationRadius;
        [Min(0f)] public float separationStrength;
        public int threatCost;
        public int firstPhase;
        public TargetKind[] targetPriority;
        public Color displayColor = Color.green;
        public Vector3 displayScale = Vector3.one;
        [Min(0.01f)] public float hitFlashSeconds;
        [Min(0.05f)] public float deathEffectSeconds;
        [Min(0.01f)] public float deathPulseSize;
    }
}
