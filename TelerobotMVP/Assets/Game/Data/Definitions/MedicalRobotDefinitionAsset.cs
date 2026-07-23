using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class MedicalRobotDefinitionAsset : ScriptableObject
    {
        public float maxHealth;
        public float healPerSecond;
        public float radius;
    }
}
