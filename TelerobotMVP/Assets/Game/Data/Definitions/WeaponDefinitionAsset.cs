using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class WeaponDefinitionAsset : ScriptableObject
    {
        public float baseDamage;
        public float headshotMultiplier;
        public int magazineSize;
        public int reserveAmmo;
        public float reloadSeconds;
        public int grenadesPerPhase;
        public float range;

        [Header("Combat Feedback")]
        [Min(0f)] public float recoilPitchDegrees;
        [Min(0f)] public float recoilYawDegrees;
        [Min(0.1f)] public float recoilRecoveryDegreesPerSecond;
        [Min(0.01f)] public float muzzleFlashSeconds;
        [Min(0.01f)] public float muzzleFlashSize;
        [Min(0.01f)] public float impactPulseSize;
        [Min(20f)] public float fireSoundFrequency;
        [Min(20f)] public float bodyHitSoundFrequency;
        [Min(20f)] public float headshotSoundFrequency;
        [Min(0.01f)] public float combatSoundSeconds;
        [Range(0f, 1f)] public float fireSoundVolume;
        [Range(0f, 1f)] public float hitSoundVolume;
    }
}
