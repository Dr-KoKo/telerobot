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
        private const float RobotStatusLineHeight = 22f;
        private const float RobotMasteryLineHeight = 20f;
        private const float RobotBarHeight = 20f;
        private const float RobotElementGap = 3f;
        private const float RobotBottomPadding = 8f;
        private const float RobotSelectionColumnWidth = 22f;

        private MvpGameController game;
        private StringTableAsset strings;
        private string radioCaption;
        private float radioUntil;
        private float ripperUntil;
        private float hitMarkerUntil;
        private float headshotUntil;
        private float damageIndicatorUntil;
        private float progressionNotificationUntil;
        private string progressionNotificationRobotId;
        private float lastDamageAngle;
        private GUIStyle label;
        private GUIStyle header;
        private GUIStyle centered;
        private GUIStyle robotDetail;
        private GUIStyle robotMastery;
        private GUIStyle barText;
        private GUIStyle selectionMarker;
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
        public bool ProgressionNotificationActive
        {
            get { return Time.unscaledTime < progressionNotificationUntil; }
        }
        public string ProgressionNotificationRobotId { get { return progressionNotificationRobotId; } }
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
            if (gameEvent.Name == "haetae_specialization_ready" &&
                gameEvent.Payload.TryGetValue("robotId", out var robotId))
            {
                progressionNotificationRobotId = robotId;
                progressionNotificationUntil = Time.unscaledTime +
                    game.Config.HaetaeProgression.ReadyAlertSeconds;
                PlayCallout(1.18f);
            }
        }

        public string GetRobotProgressionText(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null) return string.Empty;
            return GetRobotIdentityText(robot) + "\n" +
                   GetRobotStatusText(robot) + "\n" +
                   GetRobotMasteryText(robot);
        }

        public float GetRobotHealthProgress(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null || robot.State.Health.Maximum <= 0f) return 0f;
            return Mathf.Clamp01(robot.State.Health.Current / robot.State.Health.Maximum);
        }

        public string GetRobotHealthBarText(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null) return string.Empty;
            return strings.Get("hud.haetae_health") + " " +
                   Mathf.Max(0f, robot.State.Health.Current).ToString("0") + " / " +
                   robot.State.Health.Maximum.ToString("0");
        }

        public float GetRobotBatteryProgress(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null || robot.State.MaximumBattery <= 0f) return 0f;
            return Mathf.Clamp01(robot.State.Battery / robot.State.MaximumBattery);
        }

        public string GetRobotBatteryBarText(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null) return string.Empty;
            return strings.Get("hud.haetae_battery") + " " +
                   Mathf.Max(0f, robot.State.Battery).ToString("0") + " / " +
                   robot.State.MaximumBattery.ToString("0");
        }

        public WarningSeverity GetRobotBatteryWarningSeverity(string robotId)
        {
            if (game == null || FindRobot(robotId) == null) return WarningSeverity.None;
            var ratio = GetRobotBatteryProgress(robotId);
            if (ratio < game.Config.Warnings.BatteryRedFraction) return WarningSeverity.Red;
            if (ratio < game.Config.Warnings.BatteryYellowFraction) return WarningSeverity.Yellow;
            return WarningSeverity.None;
        }

        public float GetRobotExperienceProgress(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null) return 0f;

            var experiencePerLevel = game.Config.HaetaeProgression.ExperiencePerLevel;
            if (experiencePerLevel <= 0) return 0f;
            return GetExperienceInCurrentLevel(robot) / (float)experiencePerLevel;
        }

        public string GetRobotExperienceBarText(string robotId)
        {
            var robot = FindRobot(robotId);
            if (robot == null) return string.Empty;

            var experiencePerLevel = game.Config.HaetaeProgression.ExperiencePerLevel;
            if (experiencePerLevel <= 0) return string.Empty;
            return strings.Get("hud.haetae_experience") + " " +
                   GetExperienceInCurrentLevel(robot) + " / " + experiencePerLevel;
        }

        private HaetaeRobotActor FindRobot(string robotId)
        {
            if (game == null) return null;
            return game.Robots.Find(item => item != null && item.State.Id == robotId);
        }

        private int GetExperienceInCurrentLevel(HaetaeRobotActor robot)
        {
            var experiencePerLevel = game.Config.HaetaeProgression.ExperiencePerLevel;
            if (experiencePerLevel <= 0) return 0;

            var progression = robot.State.Progression;
            var currentLevelStart = (long)Mathf.Max(0, progression.Level - 1) * experiencePerLevel;
            var experienceInCurrentLevel = System.Math.Max(0L,
                (long)progression.Experience - currentLevelStart);
            return (int)System.Math.Min(experienceInCurrentLevel, experiencePerLevel);
        }

        private string GetRobotIdentityText(HaetaeRobotActor robot)
        {
            var progression = robot.State.Progression;
            var role = RoleName(progression.Specialization);
            var ready = progression.SpecializationReady
                ? "  " + strings.Get("hud.haetae_specialization_ready")
                : string.Empty;
            return robot.State.Id + "  " + strings.Get("hud.haetae_level") + " " + progression.Level +
                   "  " + role + ready;
        }

        private string GetRobotStatusText(HaetaeRobotActor robot)
        {
            return strings.Get("hud.haetae_status") + " " + robot.State.Mode;
        }

        private string GetRobotMasteryText(HaetaeRobotActor robot)
        {
            var progression = robot.State.Progression;
            return "P" + progression.PowerRank +
                   "/A" + progression.ArmorRank +
                   "/E" + progression.EfficiencyRank +
                   "/S" + progression.AttackSpeedRank +
                   "  " + strings.Get("hud.haetae_mastery_points") + " " +
                   progression.UnspentMasteryPoints;
        }

        public bool IsProgressionReadyHighlighted(string robotId)
        {
            if (game == null) return false;
            var robot = game.Robots.Find(item => item != null && item.State.Id == robotId);
            return robot != null && robot.State.Progression.SpecializationReady;
        }

        private string RoleName(HaetaeSpecialization specialization)
        {
            if (specialization == HaetaeSpecialization.Melee)
                return strings.Get("haetae.specialization.melee");
            if (specialization == HaetaeSpecialization.Ranged)
                return strings.Get("haetae.specialization.ranged");
            if (specialization == HaetaeSpecialization.Balanced)
                return strings.Get("haetae.specialization.balanced");
            return strings.Get("hud.haetae_general");
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
                normal = { textColor = GuardianGuiTheme.ResolveColor(game.Catalog, "ui.text", Color.white) }
            };
            header = new GUIStyle(label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            centered = new GUIStyle(header) { alignment = TextAnchor.MiddleCenter };
            robotDetail = new GUIStyle(label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
            robotMastery = new GUIStyle(robotDetail) { fontSize = 13 };
            barText = new GUIStyle(robotDetail)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            selectionMarker = new GUIStyle(label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            GuardianGuiTheme.ApplyFont(label, game.Catalog, false);
            GuardianGuiTheme.ApplyFont(header, game.Catalog, true);
            GuardianGuiTheme.ApplyFont(centered, game.Catalog, true);
            GuardianGuiTheme.ApplyFont(robotDetail, game.Catalog, false);
            GuardianGuiTheme.ApplyFont(robotMastery, game.Catalog, false);
            GuardianGuiTheme.ApplyFont(barText, game.Catalog, false);
            GuardianGuiTheme.ApplyFont(selectionMarker, game.Catalog, true);
            bodyLineHeight = Mathf.Ceil(Mathf.Max(MinimumBodyLineHeight,
                label.CalcHeight(new GUIContent(strings.Get("hud.player") + " Ag 100 / 100"), 340f) + 8f));
            headerLineHeight = Mathf.Ceil(Mathf.Max(MinimumHeaderLineHeight,
                header.CalcHeight(new GUIContent(strings.Get("hud.base") + " 1000 / 1000"), 340f) + 8f));
        }

        private float CalculateStatusPanelHeight()
        {
            var robotRows = game == null ? 0 : game.Robots.Count;
            return Mathf.Max(228f, 14f + headerLineHeight + bodyLineHeight * 3f +
                RobotRowHeight * robotRows + 14f);
        }

        private float RobotRowHeight
        {
            get
            {
                return bodyLineHeight + RobotStatusLineHeight + RobotMasteryLineHeight +
                       RobotBarHeight * 3f + RobotElementGap * 5f + RobotBottomPadding;
            }
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawLabeledStatusBar(Rect rect, float progress, Color fillColor, string text)
        {
            DrawSolidRect(rect, new Color(0.04f, 0.08f, 0.12f, 0.96f));
            var inner = new Rect(rect.x + 2f, rect.y + 2f,
                (rect.width - 4f) * Mathf.Clamp01(progress), rect.height - 4f);
            if (inner.width > 0f) DrawSolidRect(inner, fillColor);
            var previous = GUI.color;
            GUI.color = Color.white;
            GUI.Label(rect, text, barText);
            GUI.color = previous;
        }

        private Color GetRobotBatteryBarColor(string robotId, bool flashOn)
        {
            var severity = GetRobotBatteryWarningSeverity(robotId);
            if (severity == WarningSeverity.Red)
                return flashOn ? new Color(1f, 0.12f, 0.08f) : new Color(0.65f, 0.04f, 0.03f);
            if (severity == WarningSeverity.Yellow)
                return flashOn ? new Color(1f, 0.72f, 0.08f) : new Color(0.72f, 0.48f, 0.03f);
            return new Color(0.12f, 0.72f, 1f);
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
            GuardianGuiTheme.DrawPanel(panel, game.Catalog, 0.86f);

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

            var selectedRobots = game.SelectedRobots;
            foreach (var robot in game.Robots)
            {
                var selected = selectedRobots.Contains(robot);
                var flashOn = Mathf.PingPong(Time.unscaledTime * 5f, 1f) > 0.42f;
                if (robot.State.Progression.SpecializationReady)
                    DrawSolidRect(new Rect(panel.x + 10f, rowY, panel.width - 20f, RobotRowHeight),
                        new Color(0.22f, 0.55f, 1f, flashOn ? 0.38f : 0.2f));

                if (selected)
                {
                    GUI.color = new Color(1f, 0.82f, 0.16f);
                    GUI.Label(new Rect(panel.x + 10f, rowY, RobotSelectionColumnWidth,
                        bodyLineHeight), "▶", selectionMarker);
                }

                var contentX = panel.x + 14f + RobotSelectionColumnWidth;
                var contentWidth = panel.width - 28f - RobotSelectionColumnWidth;
                var contentY = rowY;

                GUI.color = Color.cyan;
                GUI.Label(new Rect(contentX, contentY, contentWidth, bodyLineHeight),
                    GetRobotIdentityText(robot), label);
                contentY += bodyLineHeight;

                GUI.color = Color.cyan;
                GUI.Label(new Rect(contentX, contentY, contentWidth, RobotStatusLineHeight),
                    GetRobotStatusText(robot), robotDetail);
                contentY += RobotStatusLineHeight;

                GUI.color = Color.white;
                GUI.Label(new Rect(contentX, contentY, contentWidth, RobotMasteryLineHeight),
                    GetRobotMasteryText(robot), robotMastery);
                contentY += RobotMasteryLineHeight + RobotElementGap;

                DrawLabeledStatusBar(
                    new Rect(contentX, contentY, contentWidth, RobotBarHeight),
                    GetRobotHealthProgress(robot.State.Id),
                    new Color(0.16f, 0.78f, 0.34f, 1f),
                    GetRobotHealthBarText(robot.State.Id));
                contentY += RobotBarHeight + RobotElementGap;

                DrawLabeledStatusBar(
                    new Rect(contentX, contentY, contentWidth, RobotBarHeight),
                    GetRobotBatteryProgress(robot.State.Id),
                    GetRobotBatteryBarColor(robot.State.Id, flashOn),
                    GetRobotBatteryBarText(robot.State.Id));
                contentY += RobotBarHeight + RobotElementGap;

                DrawLabeledStatusBar(
                    new Rect(contentX, contentY, contentWidth, RobotBarHeight),
                    GetRobotExperienceProgress(robot.State.Id),
                    new Color(0.12f, 0.72f, 1f, 1f),
                    GetRobotExperienceBarText(robot.State.Id));
                rowY += RobotRowHeight;
            }
            GUI.color = Color.white;

            var routePanelHeight = 14f + headerLineHeight + game.OpenRoutes.Count * bodyLineHeight + 10f;
            var routePanel = new Rect(Screen.width - 270f, 18f, 252f, routePanelHeight);
            GuardianGuiTheme.DrawPanel(routePanel, game.Catalog, 0.82f);
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
                var ripperPanel = new Rect(Screen.width * 0.5f - 110f, 80f, 220f, 46f);
                GuardianGuiTheme.DrawPanel(ripperPanel, game.Catalog, 0.94f, 3f);
                var previous = GUI.color;
                GUI.color = GuardianGuiTheme.ResolveColor(game.Catalog, "enemy.ripper", new Color(0.9f, 0.08f, 0.4f));
                GUI.Label(new Rect(Screen.width * 0.5f - 105f, 86f, 210f, 34f), strings.Get("hud.ripper"), centered);
                GUI.color = previous;
            }

            if (Time.unscaledTime < radioUntil && !string.IsNullOrEmpty(radioCaption))
            {
                GuardianGuiTheme.DrawPanel(new Rect(Screen.width * 0.5f - 330f, Screen.height - 110f, 660f, 58f),
                    game.Catalog, 0.93f);
                GUI.Label(new Rect(Screen.width * 0.5f - 315f, Screen.height - 100f, 630f, 40f), radioCaption, centered);
            }

            if (ProgressionNotificationActive && !string.IsNullOrEmpty(progressionNotificationRobotId))
            {
                GUI.color = new Color(0.05f, 0.28f, 0.55f, 0.94f);
                GUI.Box(new Rect(Screen.width * 0.5f - 220f, 136f, 440f, 56f), GUIContent.none);
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width * 0.5f - 210f, 145f, 420f, 38f),
                    progressionNotificationRobotId + "  " +
                    strings.Get("hud.haetae_specialization_ready") + "  " +
                    strings.Get("hud.haetae_specialization_hint"), centered);
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
