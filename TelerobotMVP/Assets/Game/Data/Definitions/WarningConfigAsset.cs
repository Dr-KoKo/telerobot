using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class WarningConfigAsset : ScriptableObject
    {
        [Range(0f, 1f)] public float batteryYellowFraction;
        [Range(0f, 1f)] public float batteryRedFraction;
        [Range(0f, 1f)] public float baseWarningFraction;
    }
}
