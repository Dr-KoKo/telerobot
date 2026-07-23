using System.Globalization;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class CombatHud : MonoBehaviour
    {
        private const float MinimumBodyLineHeight = 31f;
        private const float MinimumHeaderLineHeight = 39f;

        private MvpGameController game;
        private StringTableAsset strings;
        private string radioCaption;
        private float radioUntil;
        private float ripperUntil;
        private float hitMarkerUntil;
        private float headshotUntil;
        private float damageIndicatorUntil;
        private float lastDamageAngle;
        private GUIStyle label;
        private GUIStyle header;
        private GUIStyle centered;
        private DomainEventBus eventBus;
        private AudioSource calloutAudio;
        private AudioClip calloutBeep;
        private float bodyLineHeight = MinimumBodyLineHeight;
        private float headerLineHeight = MinimumHeaderLineHeight;

        public bool HitFeedbackActive { get { return Time.unscaledTime < hitMarkerUntil; } }
        public bool HeadshotFeedbackActive { get { return Time.unscaledTime < headshotUntil; } }
        public bool DamageFeedbackActive { get { return Time.unscaledTime < damageIndicatorUntil; } }
        public float LastDamageAngle { get { return lastDamageAngle; } }
        public float BodyLineHeight { get { return bodyLineHeight; } }
        public float HeaderLineHeight { get { return headerLineHeight; } }
        public float StatusPanelHeight { get { return CalculateStatusPanelHeight(); } }
        public TextClipping TextClippingMode { get { return TextClipping.Overflow; } }
        public bool LowAmmoWarningActive
        {
            get
            {
                return game != null && game.PlayerState != null &&
                       !game.PlayerState.Ammo.IsReloading &&
                       game.PlayerState.Ammo.Loaded <= game.Catalog.hud.lowAmmoThreshold;
            }
        }
        public bool ReloadProgressVisible
        {
            get { return game != null && game.PlayerState != null && game.PlayerState.Ammo.IsReloading; }
        }
        public float ReloadProgress
        {
            get
            {
                if (!ReloadProgressVisible) return 0f;
                return Mathf.Clamp01(1f - game.PlayerState.Ammo.ReloadRemaining /
                    Mathf.Max(0.01f, game.Config.Weapon.ReloadSeconds));
            }
        }
        public bool SupplyPromptActive
        {
            get
            {
                return game != null && game.PlayerActor != null &&
                       game.TryGetNearbySupply(game.PlayerActor.transform.position, out _);
            }
        }

        public void Initialize(MvpGameController owner, DomainEventBus eventBus)
        {
            game = owner;
            strings = owner.Catalog.strings;
            this.eventBus = eventBus;
            eventBus.EventPublished += OnDomainEvent;
            calloutAudio = gameObject.AddComponent<AudioSource>();
            calloutAudio.playOnAwake = false;
            calloutAudio.spatialBlend = 0f;
            calloutBeep = CreateCalloutBeep();
        }

        private void OnDomainEvent(DomainEvent gameEvent)
        {
            if (gameEvent.Name == "radio_event" && gameEvent.Payload.TryGetValue("key", out var key))
            {
                radioCaption = strings.Get(key);
                radioUntil = Time.unscaledTime + 4.5f;
                PlayCallout(1f);
            }
            if (gameEvent.Name == "ripper_spawned")
            {
                ripperUntil = Time.unscaledTime + 8f;
                PlayCallout(0.72f);
            }
            if (gameEvent.Name == "player_hit_confirmed")
            {
                hitMarkerUntil = Time.unscaledTime + game.Catalog.hud.hitMarkerSeconds;
                if (gameEvent.Payload.TryGetValue("region", out var region) && region == HitRegion.Head.ToString())
                    headshotUntil = Time.unscaledTime + game.Catalog.hud.headshotLabelSeconds;
            }
            if (gameEvent.Name == "player_damaged")
            {
                damageIndicatorUntil = Time.unscaledTime + game.Catalog.hud.damageIndicatorSeconds;
                if (gameEvent.Payload.TryGetValue("directionAngle", out var angle))
                    float.TryParse(angle, NumberStyles.Float, CultureInfo.InvariantCulture, out lastDamageAngle);
            }
        }

        private void PlayCallout(float pitch)
        {
            if (calloutAudio == null || calloutBeep == null) return;
            calloutAudio.pitch = pitch;
            calloutAudio.PlayOneShot(calloutBeep, 0.22f * PlayerPreferences.EffectsVolume);
        }

        private static AudioClip CreateCalloutBeep()
        {
            const int sampleRate = 22050;
            const float duration = 0.16f;
            var sampleCount = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var index = 0; index < samples.Length; index++)
            {
                var time = index / (float)sampleRate;
                var envelope = 1f - index / (float)samples.Length;
                samples[index] = Mathf.Sin(2f * Mathf.PI * 880f * time) * envelope * 0.35f;
            }
            var clip = AudioClip.Create("Radio Callout Beep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (eventBus != null) eventBus.EventPublished -= OnDomainEvent;
            if (calloutBeep != null) Destroy(calloutBeep);
        }

        private void EnsureStyles()
        {
            if (label != null) return;
            label = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClippingMode,
                normal = { textColor = Color.white }
            };
            header = new GUIStyle(label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            centered = new GUIStyle(header) { alignment = TextAnchor.MiddleCenter };
            bodyLineHeight = Mathf.Ceil(Mathf.Max(MinimumBodyLineHeight,
                label.CalcHeight(new GUIContent(strings.Get("hud.player") + " Ag 100 / 100"), 340f) + 8f));
            headerLineHeight = Mathf.Ceil(Mathf.Max(MinimumHeaderLineHeight,
                header.CalcHeight(new GUIContent(strings.Get("hud.base") + " 1000 / 1000"), 340f) + 8f));
        }

        private float CalculateStatusPanelHeight()
        {
            var robotRows = game == null ? 0 : game.Robots.Count;
            return Mathf.Max(228f, 14f + headerLineHeight + bodyLineHeight * (3f + robotRows) + 14f);
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawAimFeedback()
        {
            var x = Screen.width * 0.5f;
            var y = Screen.height * 0.5f;
            DrawSolidRect(new Rect(x - 10f, y - 1f, 7f, 2f), Color.white);
            DrawSolidRect(new Rect(x + 3f, y - 1f, 7f, 2f), Color.white);
            DrawSolidRect(new Rect(x - 1f, y - 10f, 2f, 7f), Color.white);
            DrawSolidRect(new Rect(x - 1f, y + 3f, 2f, 7f), Color.white);

            if (!HitFeedbackActive) return;
            var hitColor = HeadshotFeedbackActive ? new Color(1f, 0.32f, 0.08f) : Color.white;
            DrawSolidRect(new Rect(x - 18f, y - 2f, 10f, 4f), hitColor);
            DrawSolidRect(new Rect(x + 8f, y - 2f, 10f, 4f), hitColor);
            DrawSolidRect(new Rect(x - 2f, y - 18f, 4f, 10f), hitColor);
            DrawSolidRect(new Rect(x - 2f, y + 8f, 4f, 10f), hitColor);
            if (HeadshotFeedbackActive)
                GUI.Label(new Rect(x - 100f, y + 24f, 200f, 30f), strings.Get("hud.headshot"), centered);
        }

        private void DrawDamageFeedback()
        {
            if (!DamageFeedbackActive) return;
            var duration = Mathf.Max(0.1f, game.Catalog.hud.damageIndicatorSeconds);
            var remaining = Mathf.Clamp01((damageIndicatorUntil - Time.unscaledTime) / duration);
            var radians = lastDamageAngle * Mathf.Deg2Rad;
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var indicator = center + new Vector2(Mathf.Sin(radians) * 155f, -Mathf.Cos(radians) * 110f);
            var previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(lastDamageAngle, indicator);
            DrawSolidRect(new Rect(indicator.x - 34f, indicator.y - 4f, 68f, 8f),
                new Color(1f, 0.08f, 0.04f, 0.35f + remaining * 0.65f));
            GUI.matrix = previousMatrix;
        }

        private void DrawPlayerPrompts()
        {
            if (LowAmmoWarningActive)
            {
                var warningColor = Mathf.PingPong(Time.unscaledTime * 4f, 1f) > 0.45f
                    ? new Color(1f, 0.24f, 0.08f) : new Color(1f, 0.72f, 0.1f);
                var previous = GUI.color;
                GUI.color = warningColor;
                GUI.Label(new Rect(Screen.width * 0.5f - 120f, Screen.height * 0.5f + 58f, 240f, 34f),
                    strings.Get("hud.low_ammo") + "  [R]", centered);
                GUI.color = previous;
            }

            if (ReloadProgressVisible)
            {
                var bar = new Rect(Screen.width * 0.5f - 110f, Screen.height * 0.5f + 62f, 220f, 18f);
                DrawSolidRect(bar, new Color(0.02f, 0.04f, 0.07f, 0.9f));
                DrawSolidRect(new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * ReloadProgress, bar.height - 4f),
                    new Color(0.15f, 0.8f, 1f, 0.95f));
                GUI.Label(new Rect(bar.x, bar.y - 30f, bar.width, 28f), strings.Get("hud.reloading"), centered);
            }

            if (!game.TryGetNearbySupply(game.PlayerActor.transform.position, out var kind)) return;
            var locationKey = kind == SupplyKind.Safe ? "hud.safe_supply" : "hud.risky_supply";
            var panel = new Rect(Screen.width * 0.5f - 185f, Screen.height - 190f, 370f, 48f);
            var panelColor = kind == SupplyKind.Safe
                ? new Color(0.04f, 0.28f, 0.12f, 0.9f) : new Color(0.38f, 0.16f, 0.02f, 0.9f);
            DrawSolidRect(panel, panelColor);
            GUI.Label(panel, "[E]  " + strings.Get("hud.resupply") + "  ·  " + strings.Get(locationKey), centered);
        }

        private void DrawPauseOverlay()
        {
            GUI.color = new Color(0f, 0f, 0f, 0.86f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = Color.white;
            var centerX = Screen.width * 0.5f;
            var centerY = Screen.height * 0.5f;
            GUI.Label(new Rect(centerX - 220f, centerY - 190f, 440f, 60f), strings.Get("hud.pause"), centered);
            if (GUI.Button(new Rect(centerX - 120f, centerY - 108f, 240f, 48f), strings.Get("hud.resume")))
                game.SetPaused(false);
            if (GUI.Button(new Rect(centerX - 120f, centerY - 50f, 240f, 48f), strings.Get("menu.settings")))
                game.OpenSettings();
            if (GUI.Button(new Rect(centerX - 120f, centerY + 8f, 240f, 48f), strings.Get("hud.restart")))
                game.RestartSession();
            if (GUI.Button(new Rect(centerX - 120f, centerY + 66f, 240f, 48f), strings.Get("menu.main")))
                game.ReturnToMainMenu();
        }

        private void OnGUI()
        {
            if (game == null || game.PlayerState == null) return;
            EnsureStyles();
            if (game.SettingsOpen) return;

            var panel = new Rect(18f, 18f, 380f, StatusPanelHeight);
            GUI.color = new Color(0.02f, 0.04f, 0.07f, 0.86f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = Color.white;

            var baseHealth = game.BaseState.Health;
            var playerHealth = game.PlayerState.Health;
            var rowY = panel.y + 7f;
            GUI.Label(new Rect(panel.x + 16f, rowY, 340f, headerLineHeight),
                strings.Get("hud.base") + "  " + baseHealth.Current.ToString("0") + " / " + baseHealth.Maximum.ToString("0"), header);
            rowY += headerLineHeight;
            GUI.Label(new Rect(panel.x + 16f, rowY, 340f, bodyLineHeight),
                strings.Get("hud.phase") + "  " + game.CurrentPhase + "   " + game.SpawnedCount + " / " + game.TotalSpawnCount, label);
            rowY += bodyLineHeight;
            GUI.Label(new Rect(panel.x + 16f, rowY, 340f, bodyLineHeight),
                strings.Get("hud.player") + "  " + playerHealth.Current.ToString("0") + " / " + playerHealth.Maximum.ToString("0"), label);
            rowY += bodyLineHeight;
            GUI.Label(new Rect(panel.x + 16f, rowY, 340f, bodyLineHeight),
                strings.Get("hud.ammo") + "  " + game.PlayerState.Ammo.Loaded + " / " + game.PlayerState.Ammo.Reserve +
                "   " + strings.Get("hud.grenade") + " " + game.PlayerState.Grenades, label);
            rowY += bodyLineHeight;

            foreach (var robot in game.Robots)
            {
                var selected = game.SelectedRobot == robot ? "▶ " : "";
                var ratio = robot.State.MaximumBattery <= 0f ? 0f : robot.State.Battery / robot.State.MaximumBattery;
                var flashOn = Mathf.PingPong(Time.unscaledTime * 5f, 1f) > 0.42f;
                GUI.color = ratio < game.Config.Warnings.BatteryRedFraction
                    ? (flashOn ? Color.red : new Color(0.3f, 0f, 0f))
                    : ratio < game.Config.Warnings.BatteryYellowFraction
                        ? (flashOn ? Color.yellow : new Color(0.35f, 0.25f, 0f))
                        : Color.cyan;
                GUI.Label(new Rect(panel.x + 16f, rowY, 350f, bodyLineHeight),
                    selected + robot.State.Id + "  " + robot.State.Battery.ToString("0") + " / " +
                    robot.State.MaximumBattery.ToString("0") + "  " + robot.State.Mode, label);
                rowY += bodyLineHeight;
            }
            GUI.color = Color.white;

            var routePanelHeight = 14f + headerLineHeight + game.OpenRoutes.Count * bodyLineHeight + 10f;
            var routePanel = new Rect(Screen.width - 270f, 18f, 252f, routePanelHeight);
            GUI.color = new Color(0.02f, 0.04f, 0.07f, 0.82f);
            GUI.Box(routePanel, GUIContent.none);
            GUI.color = Color.white;
            var routeY = routePanel.y + 5f;
            GUI.Label(new Rect(routePanel.x + 18f, routeY, 220f, headerLineHeight), strings.Get("hud.routes"), header);
            routeY += headerLineHeight;
            for (var index = 0; index < game.OpenRoutes.Count; index++)
            {
                var route = game.OpenRoutes[index];
                var pressure = game.AliveZombies.FindAll(item => item.State.Route == route).Count;
                GUI.Label(new Rect(routePanel.x + 18f, routeY, 220f, bodyLineHeight),
                    strings.Get(game.Catalog.Route(route).displayNameKey) + "  " + pressure, label);
                routeY += bodyLineHeight;
            }

            if (game.BaseState.WarningActive)
            {
                var alpha = 0.24f + Mathf.PingPong(Time.unscaledTime * 1.8f, 0.36f);
                GUI.color = new Color(1f, 0.02f, 0.02f, alpha);
                GUI.Box(new Rect(0f, 0f, Screen.width, 18f), GUIContent.none);
                GUI.Box(new Rect(0f, Screen.height - 18f, Screen.width, 18f), GUIContent.none);
                GUI.Box(new Rect(0f, 0f, 18f, Screen.height), GUIContent.none);
                GUI.Box(new Rect(Screen.width - 18f, 0f, 18f, Screen.height), GUIContent.none);
                GUI.color = Color.white;
            }

            if (!game.MenuConsumesPointer && !game.IsFinished)
            {
                DrawAimFeedback();
                DrawDamageFeedback();
                DrawPlayerPrompts();
                var perspectiveKey = game.PlayerActor.Perspective == CameraPerspective.FirstPerson
                    ? "hud.first_person" : "hud.third_person";
                GUI.Label(new Rect(Screen.width - 210f, Screen.height - 52f, 190f, bodyLineHeight + 6f),
                    strings.Get(perspectiveKey) + "  [V]", label);
            }

            if (Time.unscaledTime < ripperUntil)
            {
                GUI.color = new Color(0.8f, 0.05f, 0.12f, 0.92f);
                GUI.Box(new Rect(Screen.width * 0.5f - 110f, 80f, 220f, 46f), GUIContent.none);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width * 0.5f - 105f, 86f, 210f, 34f), strings.Get("hud.ripper"), centered);
            }

            if (Time.unscaledTime < radioUntil && !string.IsNullOrEmpty(radioCaption))
            {
                GUI.color = new Color(0.02f, 0.08f, 0.12f, 0.93f);
                GUI.Box(new Rect(Screen.width * 0.5f - 330f, Screen.height - 110f, 660f, 58f), GUIContent.none);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width * 0.5f - 315f, Screen.height - 100f, 630f, 40f), radioCaption, centered);
            }

            if (game.IsPaused)
            {
                DrawPauseOverlay();
                return;
            }
            if (!game.IsFinished) return;
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = Color.white;
            var resultKey = game.Session.Result == GameResult.Victory ? "hud.victory" : "hud.defeat";
            var resultCenterX = Screen.width * 0.5f;
            var resultCenterY = Screen.height * 0.5f;
            GUI.Label(new Rect(resultCenterX - 300f, resultCenterY - 90f, 600f, 100f), strings.Get(resultKey), centered);
            if (GUI.Button(new Rect(resultCenterX - 120f, resultCenterY + 30f, 240f, 48f), strings.Get("hud.restart")))
                game.RestartSession();
            if (GUI.Button(new Rect(resultCenterX - 120f, resultCenterY + 92f, 240f, 48f), strings.Get("menu.main")))
                game.ReturnToMainMenu();
        }
    }
}
