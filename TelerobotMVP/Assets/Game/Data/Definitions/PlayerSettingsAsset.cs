using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class PlayerSettingsAsset : ScriptableObject
    {
        public float minimumMouseSensitivity;
        public float maximumMouseSensitivity;
        public float defaultMouseSensitivity;
        [Range(0f, 1f)] public float defaultMasterVolume;
        [Range(0f, 1f)] public float defaultEffectsVolume;
        public int minimumResolutionWidth;
        public int minimumResolutionHeight;
        public int defaultResolutionWidth;
        public int defaultResolutionHeight;
        public bool defaultFullscreen;
        public CameraPerspective defaultPerspective;
    }
}
