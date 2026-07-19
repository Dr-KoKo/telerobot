using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class RobotDefinitionAsset : ScriptableObject
    {
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackInterval;
        public float detectionRadius;
        public float attackRange;
    }
}
