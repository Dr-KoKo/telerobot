using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class BaseConfigAsset : ScriptableObject
    {
        public float maxHealth;
        [Range(0f, 1f)] public float phaseRecoveryFraction;
        [Range(0f, 1f)] public float warningFraction;
        public bool allowPlayerRepair;
    }
}
