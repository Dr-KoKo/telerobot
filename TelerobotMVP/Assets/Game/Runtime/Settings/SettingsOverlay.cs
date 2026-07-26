using System.Collections.Generic;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class SettingsOverlay : MonoBehaviour
    {
        private MvpContentCatalog catalog;
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        private float sensitivity;
        private float masterVolume;
        private float effectsVolume;
        private int resolutionIndex;
        private bool fullscreen;
        private CameraPerspective perspective;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle centeredStyle;

        public bool IsOpen { get; private set; }
        public CameraPerspective DraftPerspective { get { return perspective; } }

        public void Initialize(MvpContentCatalog content)
        {
            catalog = content;
            BuildResolutionOptions();
        }

        public void Open()
        {
            if (catalog == null) return;
            if (!PlayerPreferences.IsInitialized) PlayerPreferences.Initialize(catalog.playerSettings);
            sensitivity = PlayerPreferences.MouseSensitivity;
            masterVolume = PlayerPreferences.MasterVolume;
            effectsVolume = PlayerPreferences.EffectsVolume;
            fullscreen = PlayerPreferences.Fullscreen;
            perspective = PlayerPreferences.DefaultPerspective;
            resolutionIndex = FindResolutionIndex(PlayerPreferences.ResolutionWidth, PlayerPreferences.ResolutionHeight);
            IsOpen = true;
        }

        public void CancelAndClose()
        {
            IsOpen = false;
        }

        public void ApplyAndClose()
        {
            if (catalog == null || resolutions.Count == 0) return;
            var resolution = resolutions[resolutionIndex];
            PlayerPreferences.Save(catalog.playerSettings, sensitivity, masterVolume, effectsVolume,
                resolution.x, resolution.y, fullscreen, perspective);
            IsOpen = false;
        }

        public void SetDraftPerspectiveForTests(CameraPerspective value)
        {
            perspective = value;
        }

        private void BuildResolutionOptions()
        {
            resolutions.Clear();
            if (catalog == null || catalog.playerSettings == null) return;
            foreach (var available in Screen.resolutions)
            {
                if (available.width < catalog.playerSettings.minimumResolutionWidth ||
                    available.height < catalog.playerSettings.minimumResolutionHeight) continue;
                var option = new Vector2Int(available.width, available.height);
                if (!resolutions.Contains(option)) resolutions.Add(option);
            }
            var configured = new Vector2Int(catalog.playerSettings.defaultResolutionWidth,
                catalog.playerSettings.defaultResolutionHeight);
            if (!resolutions.Contains(configured)) resolutions.Add(configured);
            resolutions.Sort((left, right) => left.x == right.x ? left.y.CompareTo(right.y) : left.x.CompareTo(right.x));
        }

        private int FindResolutionIndex(int width, int height)
        {
            var requested = new Vector2Int(width, height);
            if (!resolutions.Contains(requested))
            {
                resolutions.Add(requested);
                resolutions.Sort((left, right) => left.x == right.x ? left.y.CompareTo(right.y) : left.x.CompareTo(right.x));
            }
            return Mathf.Max(0, resolutions.IndexOf(requested));
        }

        private void CycleResolution(int direction)
        {
            if (resolutions.Count == 0) return;
            resolutionIndex = (resolutionIndex + direction + resolutions.Count) % resolutions.Count;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = GuardianGuiTheme.ResolveColor(catalog, "ally.haetae", Color.white) }
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = GuardianGuiTheme.ResolveColor(catalog, "ui.text", Color.white) }
            };
            centeredStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            GuardianGuiTheme.ApplyFont(titleStyle, catalog, true);
            GuardianGuiTheme.ApplyFont(labelStyle, catalog, false);
            GuardianGuiTheme.ApplyFont(centeredStyle, catalog, false);
        }

        private void OnGUI()
        {
            if (!IsOpen || catalog == null || catalog.strings == null || resolutions.Count == 0) return;
            GUI.depth = -100;
            EnsureStyles();
            var strings = catalog.strings;
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0.01f, 0.025f, 0.96f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var width = Mathf.Min(680f, Screen.width - 40f);
            var panel = new Rect((Screen.width - width) * 0.5f, Mathf.Max(20f, (Screen.height - 620f) * 0.5f), width, 620f);
            GuardianGuiTheme.DrawPanel(panel, catalog, 0.98f, 3f);
            GUI.Label(new Rect(panel.x + 20f, panel.y + 18f, panel.width - 40f, 48f), strings.Get("settings.title"), titleStyle);

            var left = panel.x + 48f;
            var valueLeft = panel.x + 290f;
            var row = panel.y + 92f;
            GUI.Label(new Rect(left, row, 230f, 30f), strings.Get("settings.sensitivity"), labelStyle);
            sensitivity = GUI.HorizontalSlider(new Rect(valueLeft, row + 8f, 270f, 22f), sensitivity,
                catalog.playerSettings.minimumMouseSensitivity, catalog.playerSettings.maximumMouseSensitivity);
            GUI.Label(new Rect(valueLeft + 280f, row, 70f, 30f), sensitivity.ToString("0.00"), labelStyle);

            row += 68f;
            GUI.Label(new Rect(left, row, 230f, 30f), strings.Get("settings.master_volume"), labelStyle);
            masterVolume = GUI.HorizontalSlider(new Rect(valueLeft, row + 8f, 270f, 22f), masterVolume, 0f, 1f);
            GUI.Label(new Rect(valueLeft + 280f, row, 70f, 30f), Mathf.RoundToInt(masterVolume * 100f) + "%", labelStyle);

            row += 68f;
            GUI.Label(new Rect(left, row, 230f, 30f), strings.Get("settings.effects_volume"), labelStyle);
            effectsVolume = GUI.HorizontalSlider(new Rect(valueLeft, row + 8f, 270f, 22f), effectsVolume, 0f, 1f);
            GUI.Label(new Rect(valueLeft + 280f, row, 70f, 30f), Mathf.RoundToInt(effectsVolume * 100f) + "%", labelStyle);

            row += 68f;
            GUI.Label(new Rect(left, row, 230f, 36f), strings.Get("settings.resolution"), labelStyle);
            if (GUI.Button(new Rect(valueLeft, row, 42f, 36f), "‹")) CycleResolution(-1);
            var resolution = resolutions[resolutionIndex];
            GUI.Label(new Rect(valueLeft + 48f, row, 172f, 36f), resolution.x + " × " + resolution.y, centeredStyle);
            if (GUI.Button(new Rect(valueLeft + 226f, row, 42f, 36f), "›")) CycleResolution(1);

            row += 68f;
            GUI.Label(new Rect(left, row, 230f, 36f), strings.Get("settings.fullscreen"), labelStyle);
            if (GUI.Button(new Rect(valueLeft, row, 268f, 36f),
                    strings.Get(fullscreen ? "settings.on" : "settings.off"))) fullscreen = !fullscreen;

            row += 68f;
            GUI.Label(new Rect(left, row, 230f, 36f), strings.Get("settings.default_perspective"), labelStyle);
            var perspectiveKey = perspective == CameraPerspective.FirstPerson ? "hud.first_person" : "hud.third_person";
            if (GUI.Button(new Rect(valueLeft, row, 268f, 36f), strings.Get(perspectiveKey)))
                perspective = perspective == CameraPerspective.FirstPerson
                    ? CameraPerspective.ThirdPerson : CameraPerspective.FirstPerson;

            row += 82f;
            if (GUI.Button(new Rect(panel.center.x - 230f, row, 210f, 48f), strings.Get("settings.cancel"))) CancelAndClose();
            if (GUI.Button(new Rect(panel.center.x + 20f, row, 210f, 48f), strings.Get("settings.apply"))) ApplyAndClose();
            GUI.color = previousColor;
        }
    }
}
