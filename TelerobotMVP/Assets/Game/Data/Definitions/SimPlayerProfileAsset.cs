using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class SimPlayerProfileAsset : ScriptableObject
    {
        public SimProfileId id;
        [Range(0f, 1f)] public float aimAccuracy;
        [Range(0f, 1f)] public float headshotRate;
        public float reactionDelaySeconds;
        public float fireIntervalSeconds;
        public SimRoutePriorityPolicy routePriorityPolicy;
        [Range(0f, 1f)] public float ripperFocus;
        [Range(0f, 1f)] public float robotChargeThresholdFraction;
        public SimUpgradeSelectionPolicy upgradeSelectionPolicy;
        public SimGrenadeUsePolicy grenadeUsePolicy;
        public int grenadeClusterThreshold;
    }
}
