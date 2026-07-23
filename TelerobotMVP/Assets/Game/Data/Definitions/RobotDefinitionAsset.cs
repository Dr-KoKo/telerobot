using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class RobotDefinitionAsset : ScriptableObject
    {
        public float maxHealth;
        public float moveSpeed;
        public float dashDamage;
        public float biteDamage;
        public float biteCooldownSeconds;
        public float dashCooldownSeconds;
        public float detectionRadius;
        public float engageRange;
        [Min(0.1f)] public float separationRadius;
        [Min(0f)] public float separationStrength;
        [Min(0.1f)] public float formationSpacing;
        [Min(1f)] public float defendLeashRadius;
        public float runnerKillTargetMinimumSeconds;
        public float runnerKillTargetMaximumSeconds;
        public float bruiserKillTargetMinimumSeconds;
        public float bruiserKillTargetMaximumSeconds;
    }
}
