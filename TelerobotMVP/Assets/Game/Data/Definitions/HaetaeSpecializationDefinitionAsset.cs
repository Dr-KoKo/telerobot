using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class HaetaeSpecializationDefinitionAsset : ScriptableObject
    {
        public HaetaeSpecialization id;
        public string displayNameKey;
        public string descriptionKey;
        [Min(0f)] public float preferredMinRange;
        [Min(0f)] public float preferredMaxRange;
        [Min(0f)] public float dashDamageMultiplier;
        [Min(0f)] public float biteDamageMultiplier;
        [Min(0f)] public float rangedDamage;
        [Min(0f)] public float rangedCooldownSeconds;
        [Min(0f)] public float cleaveRadius;
        [Min(1)] public int maximumTargets = 1;
        [Min(0.01f)] public float incomingDamageMultiplier = 1f;
        [Min(0.01f)] public float combatBatteryMultiplier = 1f;
        public Color bodyColor = Color.white;
        public Color attackPulseColor = Color.white;
        public Color tracerColor = Color.white;
    }
}
