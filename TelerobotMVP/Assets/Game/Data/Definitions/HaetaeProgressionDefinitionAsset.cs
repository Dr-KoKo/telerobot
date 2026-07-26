using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class HaetaeProgressionDefinitionAsset : ScriptableObject
    {
        [Min(1)] public int experiencePerLevel = 100;
        [Min(0.01f)] public float readyAlertSeconds = 4f;
        [Min(0f)] public float powerDamageBonusPerRank = 0.10f;
        [Range(0f, 1f)] public float armorDamageReductionPerRank = 0.08f;
        [Range(0f, 1f)] public float efficiencyBatteryReductionPerRank = 0.08f;
        [Range(0f, 1f)] public float attackSpeedBonusPerRank = 0.10f;
        [Range(0f, 1f)] public float minimumReductionMultiplier = 0.50f;
        public HaetaeSpecializationDefinitionAsset[] specializations;
    }
}
