using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public static class PlayerPreferences
    {
        private const string SensitivityKey = "Telerobot.Settings.MouseSensitivity";
        private const string MasterVolumeKey = "Telerobot.Settings.MasterVolume";
        private const string EffectsVolumeKey = "Telerobot.Settings.EffectsVolume";
        private const string ResolutionWidthKey = "Telerobot.Settings.ResolutionWidth";
        private const string ResolutionHeightKey = "Telerobot.Settings.ResolutionHeight";
        private const string FullscreenKey = "Telerobot.Settings.Fullscreen";
        private const string PerspectiveKey = "Telerobot.Settings.DefaultPerspective";

        public static bool IsInitialized { get; private set; }
        public static float MouseSensitivity { get; private set; }
        public static float MasterVolume { get; private set; }
        public static float EffectsVolume { get; private set; }
        public static int ResolutionWidth { get; private set; }
        public static int ResolutionHeight { get; private set; }
        public static bool Fullscreen { get; private set; }
        public static CameraPerspective DefaultPerspective { get; private set; }

        public static void Initialize(PlayerSettingsAsset defaults, bool applyDisplay = true)
        {
            RequireDefaults(defaults);
            MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, defaults.defaultMouseSensitivity),
                defaults.minimumMouseSensitivity, defaults.maximumMouseSensitivity);
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, defaults.defaultMasterVolume));
            EffectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, defaults.defaultEffectsVolume));
            ResolutionWidth = Mathf.Max(defaults.minimumResolutionWidth,
                PlayerPrefs.GetInt(ResolutionWidthKey, defaults.defaultResolutionWidth));
            ResolutionHeight = Mathf.Max(defaults.minimumResolutionHeight,
                PlayerPrefs.GetInt(ResolutionHeightKey, defaults.defaultResolutionHeight));
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, defaults.defaultFullscreen ? 1 : 0) == 1;
            var perspective = PlayerPrefs.GetInt(PerspectiveKey, (int)defaults.defaultPerspective);
            DefaultPerspective = System.Enum.IsDefined(typeof(CameraPerspective), perspective)
                ? (CameraPerspective)perspective : defaults.defaultPerspective;
            IsInitialized = true;
            ApplyCurrent(applyDisplay);
        }

        public static void Save(PlayerSettingsAsset defaults, float sensitivity, float masterVolume,
            float effectsVolume, int resolutionWidth, int resolutionHeight, bool fullscreen,
            CameraPerspective perspective, bool applyDisplay = true)
        {
            RequireDefaults(defaults);
            MouseSensitivity = Mathf.Clamp(sensitivity, defaults.minimumMouseSensitivity, defaults.maximumMouseSensitivity);
            MasterVolume = Mathf.Clamp01(masterVolume);
            EffectsVolume = Mathf.Clamp01(effectsVolume);
            ResolutionWidth = Mathf.Max(defaults.minimumResolutionWidth, resolutionWidth);
            ResolutionHeight = Mathf.Max(defaults.minimumResolutionHeight, resolutionHeight);
            Fullscreen = fullscreen;
            DefaultPerspective = System.Enum.IsDefined(typeof(CameraPerspective), perspective)
                ? perspective : defaults.defaultPerspective;

            PlayerPrefs.SetFloat(SensitivityKey, MouseSensitivity);
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            PlayerPrefs.SetFloat(EffectsVolumeKey, EffectsVolume);
            PlayerPrefs.SetInt(ResolutionWidthKey, ResolutionWidth);
            PlayerPrefs.SetInt(ResolutionHeightKey, ResolutionHeight);
            PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(PerspectiveKey, (int)DefaultPerspective);
            PlayerPrefs.Save();
            IsInitialized = true;
            ApplyCurrent(applyDisplay);
        }

        public static void ClearSavedValuesForTests()
        {
            PlayerPrefs.DeleteKey(SensitivityKey);
            PlayerPrefs.DeleteKey(MasterVolumeKey);
            PlayerPrefs.DeleteKey(EffectsVolumeKey);
            PlayerPrefs.DeleteKey(ResolutionWidthKey);
            PlayerPrefs.DeleteKey(ResolutionHeightKey);
            PlayerPrefs.DeleteKey(FullscreenKey);
            PlayerPrefs.DeleteKey(PerspectiveKey);
            PlayerPrefs.Save();
            IsInitialized = false;
        }

        private static void ApplyCurrent(bool applyDisplay)
        {
            AudioListener.volume = MasterVolume;
            if (!applyDisplay || Application.isEditor) return;
            Screen.SetResolution(ResolutionWidth, ResolutionHeight,
                Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        private static void RequireDefaults(PlayerSettingsAsset defaults)
        {
            if (defaults == null) throw new System.ArgumentNullException(nameof(defaults));
        }
    }
}
