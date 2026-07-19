using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class BatteryConfigAsset : ScriptableObject
    {
        public float maximum;
        public float lowPowerMaximum;
        public float criticalMaximum;
        public float idleDrainPerSecond;
        public float patrolDrainPerSecond;
        public float combatDrainPerSecond;
        public float ripperHitDrain;
        public float chargePerSecond;
        public float lowPowerMoveMultiplier;
        public float lowPowerAttackMultiplier;
        public float disabledHoldSeconds;
        public float recoveryPerSecond;
        public float moveEnableThreshold;
        [Range(0f, 1f)] public float yellowWarningFraction;
        [Range(0f, 1f)] public float redWarningFraction;
    }
}
