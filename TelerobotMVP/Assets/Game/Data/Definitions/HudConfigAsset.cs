using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class HudConfigAsset : ScriptableObject
    {
        public string[] elements;
        public string[] informationPriority;
        [Min(0)] public int lowAmmoThreshold;
        [Min(0.1f)] public float damageIndicatorSeconds;
        [Min(0.01f)] public float hitMarkerSeconds;
        [Min(0.01f)] public float headshotLabelSeconds;
    }
}
