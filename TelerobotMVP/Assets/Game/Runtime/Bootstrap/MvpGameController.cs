using System;
using System.Collections.Generic;
using System.IO;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using Telerobot.Game.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Telerobot.Game.Runtime
{
    public sealed class MvpGameController : MonoBehaviour
    {
        [SerializeField] private MvpContentCatalog catalog;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private int sessionSeed = 1001;
        [SerializeField] private bool acceleratedSpawning;

        private SpawnSystem spawnSystem;
        private PhaseSystem phaseSystem;
        private UpgradeSystem upgradeSystem;
        private WarningSystem warningSystem;
        private XorShiftRng rng;
        private DomainEventBus events;
        private JsonLinesTelemetrySink telemetrySink;
        private PhaseState phaseState;
        private List<SpawnEntry> spawnQueue;
        private ContinuousSpawnScheduler spawnScheduler;
        private int spawnIndex;
        private float pressureTimer;
        private float baseSampleTimer;
        private bool resupplyInProgress;
        private float resupplyRemaining;
        private SupplyKind resupplyKind;
        private int zombieSerial;
        private bool sessionEnded;
        private bool paused;
        private readonly Dictionary<RouteId, List<Material>> routeMaterials = new Dictionary<RouteId, List<Material>>();
        private readonly Dictionary<RouteId, BarrierRuntime> barriers = new Dictionary<RouteId, BarrierRuntime>();
        private CombatHud hud;
        private RobotCommandMenu commandMenu;
        private UpgradeSelectionView upgradeView;
        private SettingsOverlay settingsOverlay;
        private RobotSelectionModel robotSelection;

        public MvpContentCatalog Catalog { get { return catalog; } }
        public GameplayConfig Config { get; private set; }
        public SessionState Session { get; private set; }
        public PlayerState PlayerState { get; private set; }
        public BaseState BaseState { get; private set; }
        public RuntimeModifiers Modifiers { get; private set; }
        public RobotCommandSystem CommandSystem { get; private set; }
        public ThirdPersonPlayerController PlayerActor { get; private set; }
        public MedicalRobotActor MedicalActor { get; private set; }
        public Transform BaseTransform { get; private set; }
        public Vector3 ChargingPosition { get; private set; }
        public List<HaetaeRobotActor> Robots { get; } = new List<HaetaeRobotActor>();
        public List<ZombieActor> AliveZombies { get; } = new List<ZombieActor>();
        public Dictionary<RouteId, List<Transform>> RouteTargets { get; } = new Dictionary<RouteId, List<Transform>>();
        public HaetaeRobotActor SelectedRobot
        {
            get
            {
                if (robotSelection == null || robotSelection.SelectedIds.Count == 0) return null;
                return Robots.Find(item => item.State.Id == robotSelection.SelectedIds[0]);
            }
            set { SelectOnlyRobot(value); }
        }
        public List<HaetaeRobotActor> SelectedRobots
        {
            get
            {
                var result = new List<HaetaeRobotActor>();
                if (robotSelection == null) return result;
                foreach (var id in robotSelection.SelectedIds)
                {
                    var robot = Robots.Find(item => item.State.Id == id);
                    if (robot != null) result.Add(robot);
                }
                return result;
            }
        }
        public bool AreAllRobotsSelected { get { return robotSelection != null && robotSelection.IsAllSelected; } }
        public int CurrentPhase { get { return phaseState == null ? 0 : phaseState.Number; } }
        public bool IsFinished { get { return Session != null && Session.Result != GameResult.InProgress; } }
        public bool IsPaused { get { return paused; } }
        public bool SettingsOpen { get { return settingsOverlay != null && settingsOverlay.IsOpen; } }
        public bool UpgradeOpen { get { return upgradeView != null && upgradeView.IsOpen; } }
        public bool InputBlocked { get { return IsFinished || IsPaused || SettingsOpen || UpgradeOpen || (commandMenu != null && commandMenu.IsOpen); } }
        public bool MenuConsumesPointer { get { return IsPaused || SettingsOpen || UpgradeOpen || (commandMenu != null && commandMenu.IsOpen); } }
        public int SpawnedCount { get { return spawnIndex; } }
        public int TotalSpawnCount { get { return spawnQueue == null ? 0 : spawnQueue.Count; } }
        public int MaxAliveConcurrent { get { return phaseState == null ? 0 : Config.GetPhase(phaseState.Number).MaxAliveConcurrent; } }
        public bool IsResupplying { get { return resupplyInProgress; } }
        public float ResupplyRemainingSeconds { get { return resupplyRemaining; } }
        public IReadOnlyList<RouteId> OpenRoutes { get { return phaseState == null ? Array.Empty<RouteId>() : phaseState.OpenRoutes; } }
        public IReadOnlyList<DomainEvent> EventHistory { get { return events == null ? Array.Empty<DomainEvent>() : events.History; } }
        public IReadOnlyList<UpgradeConfig> CurrentUpgradeOffer { get { return upgradeView == null ? Array.Empty<UpgradeConfig>() : upgradeView.Offer; } }

        public void SetCatalog(MvpContentCatalog value)
        {
            catalog = value;
        }

        public void SetInputActions(InputActionAsset value)
        {
            inputActions = value;
        }

        public void SelectOnlyRobot(HaetaeRobotActor robot)
        {
            if (robot == null || robotSelection == null) return;
            robotSelection.SelectOnly(robot.State.Id);
        }

        public void ToggleSelectAllRobots(bool selected)
        {
            if (robotSelection != null) robotSelection.ToggleAll(selected);
        }

        public int IssueCommandToSelected(RobotCommand command, RouteId route)
        {
            var accepted = 0;
            foreach (var robot in SelectedRobots) if (robot.Issue(command, route)) accepted++;
            return accepted;
        }

        public Vector3 ToVector(Float3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public void SetAcceleratedSpawningForTests(bool value)
        {
            acceleratedSpawning = value;
        }

        public void SpawnAllNowForTests()
        {
            while (spawnQueue != null && spawnIndex < spawnQueue.Count) SpawnZombie(spawnQueue[spawnIndex++]);
        }

        public void ClearCurrentWaveForTests()
        {
            spawnIndex = spawnQueue == null ? 0 : spawnQueue.Count;
            foreach (var zombie in new List<ZombieActor>(AliveZombies)) zombie.ReceiveDamage(99999f, "test");
        }

        public void TogglePause()
        {
            if (SettingsOpen)
            {
                settingsOverlay.CancelAndClose();
                RefreshCursorState();
                return;
            }
            SetPaused(!paused);
        }

        public void SetPaused(bool value)
        {
            if ((value && IsFinished) || paused == value) return;
            if (!value && SettingsOpen) settingsOverlay.CancelAndClose();
            paused = value;
            Time.timeScale = value ? 0f : 1f;
            RefreshCursorState();
            if (events != null) Emit("game_paused", "active", value.ToString());
        }

        public void RestartSession()
        {
            if (events != null) Emit("session_restarted");
            if (telemetrySink != null) telemetrySink.Flush();
            paused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OpenSettings()
        {
            if (settingsOverlay == null || IsFinished) return;
            if (!paused) SetPaused(true);
            settingsOverlay.Open();
            RefreshCursorState();
        }

        public void ReturnToMainMenu()
        {
            if (events != null) Emit("returned_to_main_menu");
            if (telemetrySink != null) telemetrySink.Flush();
            paused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void RefreshCursorState()
        {
            var showCursor = paused || SettingsOpen || IsFinished || UpgradeOpen || (commandMenu != null && commandMenu.IsOpen);
            Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showCursor;
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            if (catalog == null)
            {
                Debug.LogError("MVP content catalog is not assigned. Run Tools/Telerobot/Build MVP Project.");
                enabled = false;
                return;
            }
            Config = MvpDataMapper.Map(catalog);
            PlayerPreferences.Initialize(catalog.playerSettings);
            Session = new SessionState(sessionSeed);
            PlayerState = new PlayerState(Config.Game.PlayerMaxHealth, Config.Weapon.MagazineSize,
                Config.Ammo.StartReserveAmmo, Config.Weapon.GrenadesPerPhase);
            BaseState = new BaseState(Config.Base.MaxHealth);
            Modifiers = new RuntimeModifiers();
            CommandSystem = new RobotCommandSystem(Config.Commands);
            rng = new XorShiftRng(sessionSeed);
            spawnSystem = new SpawnSystem(Config);
            phaseSystem = new PhaseSystem(Config.Base);
            upgradeSystem = new UpgradeSystem(Config);
            warningSystem = new WarningSystem(Config.Warnings, Config.Base);
            events = new DomainEventBus();

            var telemetryPath = Path.Combine(Application.persistentDataPath, Config.Telemetry.SinkFolder,
                "session-" + sessionSeed + ".jsonl");
            telemetrySink = new JsonLinesTelemetrySink(telemetryPath);
            var bridge = new TelemetryBridge(events, telemetrySink, Application.version, catalog.dataVersion,
                "runtime-" + sessionSeed + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"), sessionSeed);
        }

        private void Start()
        {
            BuildRuntimeWorld();
            Emit("session_started", "dataVersion", catalog.dataVersion);
            Radio("radio.game_start");
            BeginPhase(1);
            RuntimePlayerSmoke.MarkGameplayReadyAndQuit();
        }

        private void Update()
        {
            if (phaseState == null || IsFinished || IsPaused) return;
            Session.ElapsedTime += Time.deltaTime;
            UpdateSpawning();
            UpdateResupply();
            phaseState.AliveCount = AliveZombies.Count;
            phaseState.AllSpawned = spawnQueue != null && spawnIndex >= spawnQueue.Count;
            UpdateWarnings();
            UpdateSampleTelemetry();

            var transition = phaseSystem.Evaluate(Session, phaseState, BaseState, PlayerState);
            if (transition == PhaseTransition.Defeat) FinishSession(false);
            else if (transition == PhaseTransition.AwaitingUpgrade) HandlePhaseClear();
            else if (transition == PhaseTransition.Victory) FinishSession(true);

            if (acceleratedSpawning && UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame)
            {
                foreach (var zombie in new List<ZombieActor>(AliveZombies)) zombie.ReceiveDamage(99999f, "debug");
                spawnIndex = spawnQueue.Count;
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (telemetrySink == null) return;
            telemetrySink.Flush();
            telemetrySink.Dispose();
        }

        private void BuildRuntimeWorld()
        {
            RenderSettings.ambientLight = new Color(0.25f, 0.28f, 0.34f);
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            CreateBox("Ground", new Vector3(0f, -0.55f, 12f), new Vector3(65f, 1f, 70f), new Color(0.09f, 0.11f, 0.14f));
            var baseObject = CreateBox("Central Base", ToVector(Config.World.BasePosition), new Vector3(8f, 3f, 8f), new Color(0.12f, 0.42f, 0.58f));
            BaseTransform = baseObject.transform;

            foreach (var route in catalog.routes) BuildRoute(route);
            BuildSouthTunnelCover();

            ChargingPosition = ToVector(Config.World.ChargingStation);
            CreateCylinder("Charging Station", ChargingPosition, new Vector3(2f, 0.25f, 2f), new Color(0.05f, 0.85f, 0.95f));
            CreateCylinder("Safe Ammo Supply", ToVector(Config.World.SafeSupply), new Vector3(1.4f, 0.35f, 1.4f), new Color(0.2f, 0.8f, 0.25f));
            CreateCylinder("Risky Ammo Supply", ToVector(Config.World.RiskySupply), new Vector3(1.4f, 0.35f, 1.4f), new Color(1f, 0.55f, 0.08f));

            var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Field Commander";
            playerObject.transform.position = ToVector(Config.World.PlayerStart);
            ApplyColor(playerObject, new Color(0.18f, 0.55f, 1f));
            var capsule = playerObject.GetComponent<CapsuleCollider>();
            if (capsule != null) capsule.enabled = false;
            PlayerActor = playerObject.AddComponent<ThirdPersonPlayerController>();
            var playerInput = playerObject.AddComponent<InputSystemPlayerInput>();
            playerInput.Initialize(inputActions);
            PlayerActor.Initialize(this, PlayerState, Config.Weapon, playerInput);

            SpawnRobot("haetae-1", ToVector(Config.World.RobotStarts[0]), RouteId.NorthRoad, new Color(0.95f, 0.7f, 0.1f));
            SpawnRobot("haetae-2", ToVector(Config.World.RobotStarts[1]), RouteId.NorthRoad, new Color(1f, 0.35f, 0.08f));
            robotSelection = new RobotSelectionModel(Robots.ConvertAll(item => item.State.Id));
            robotSelection.SelectOnly(Robots[0].State.Id);

            var ui = new GameObject("MVP HUD");
            settingsOverlay = ui.AddComponent<SettingsOverlay>();
            settingsOverlay.Initialize(catalog);
            hud = ui.AddComponent<CombatHud>();
            hud.Initialize(this, events);
            commandMenu = ui.AddComponent<RobotCommandMenu>();
            commandMenu.Initialize(this);
            upgradeView = ui.AddComponent<UpgradeSelectionView>();
            upgradeView.Initialize(this);
        }

        private void BuildRoute(RouteDefinitionAsset route)
        {
            var targets = new List<Transform>();
            var materials = new List<Material>();
            for (var index = 0; index < route.waypoints.Length; index++)
            {
                var point = new GameObject(route.id + " Waypoint " + index);
                point.transform.position = route.waypoints[index];
                point.transform.SetParent(transform);
                targets.Add(point.transform);
                if (index == 0) continue;
                var previous = route.waypoints[index - 1];
                var current = route.waypoints[index];
                var center = (previous + current) * 0.5f + Vector3.down * 0.43f;
                var length = Vector3.Distance(previous, current);
                var road = CreateBox(route.id + " Segment " + index, center, new Vector3(route.width, 0.12f, length), route.routeColor * 0.45f);
                road.transform.rotation = Quaternion.LookRotation(current - previous);
                materials.Add(road.GetComponent<Renderer>().material);
            }
            routeMaterials[route.id] = materials;
            RouteTargets[route.id] = targets;
        }

        private void BuildSouthTunnelCover()
        {
            var route = catalog.Route(RouteId.SouthTunnel);
            for (var index = 0; index < route.waypoints.Length - 1; index++)
            {
                var a = route.waypoints[index];
                var b = route.waypoints[index + 1];
                var center = (a + b) * 0.5f;
                var direction = (b - a).normalized;
                var side = Vector3.Cross(Vector3.up, direction) * (route.width * 0.5f + 0.8f);
                CreateBox("Tunnel Wall L", center + side + Vector3.up * 1.4f, new Vector3(0.5f, 3.2f, Vector3.Distance(a, b)), new Color(0.18f, 0.16f, 0.21f)).transform.rotation = Quaternion.LookRotation(direction);
                CreateBox("Tunnel Wall R", center - side + Vector3.up * 1.4f, new Vector3(0.5f, 3.2f, Vector3.Distance(a, b)), new Color(0.18f, 0.16f, 0.21f)).transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void SpawnRobot(string id, Vector3 position, RouteId route, Color color)
        {
            var actorObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actorObject.name = id;
            actorObject.transform.position = position;
            actorObject.transform.localScale = new Vector3(1.1f, 0.75f, 1.5f);
            ApplyColor(actorObject, color);
            var actor = actorObject.AddComponent<HaetaeRobotActor>();
            actor.Initialize(this, id, Config.Robot, Config.Battery, route);
            Robots.Add(actor);
        }

        private void BeginPhase(int number)
        {
            var phase = Config.GetPhase(number);
            foreach (var robot in Robots) robot.RestoreForPhaseStart();
            phaseState = new PhaseState(number, phase.OpenRoutes);
            Session.CurrentPhase = number;
            if (Config.Ammo.GrenadeResupplyPolicy == GrenadeResupplyPolicy.PhaseResetOnly)
                PlayerState.Grenades = Config.Weapon.GrenadesPerPhase;
            spawnQueue = spawnSystem.Compose(phase, rng);
            spawnIndex = 0;
            spawnScheduler = new ContinuousSpawnScheduler(phase, rng);
            RemoveBarriers();
            if (Modifiers.EmergencyBarrier) SpawnBarriers(phase.OpenRoutes);
            Emit("phase_started", "spawnCount", spawnQueue.Count.ToString());
            var openedRoute = phase.OpenRoutes[phase.OpenRoutes.Length - 1];
            HighlightRoute(openedRoute);
            Emit("route_opened", "routeId", openedRoute.ToString());
            Radio(number == 1 ? "radio.phase1" : number == 2 ? "radio.phase2" : "radio.phase3");
            if (number == 3) SpawnMedicalRobot();
        }

        private void UpdateSpawning()
        {
            if (spawnQueue == null || spawnIndex >= spawnQueue.Count) return;
            var phase = Config.GetPhase(phaseState.Number);
            var elapsed = acceleratedSpawning ? Math.Max(phase.PhaseStartDelaySeconds, phase.GroupIntervalSeconds) : Time.deltaTime;
            var count = spawnScheduler.Advance(elapsed, AliveZombies.Count, spawnQueue.Count - spawnIndex);
            for (var index = 0; index < count; index++) SpawnZombie(spawnQueue[spawnIndex++]);
        }

        private void SpawnZombie(SpawnEntry entry)
        {
            var asset = catalog.Zombie(entry.Type);
            var config = Config.GetZombie(entry.Type);
            var route = catalog.Route(entry.Route);
            var approachPath = CreateZombieApproachPath(route, config);
            var actorObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actorObject.name = entry.Type + " " + (++zombieSerial);
            actorObject.transform.position = approachPath[0];
            actorObject.transform.localScale = asset.displayScale;
            ApplyColor(actorObject, asset.displayColor);
            var actor = actorObject.AddComponent<ZombieActor>();
            actor.Initialize(this, "zombie-" + zombieSerial, config, entry.Route, approachPath, asset);
            AliveZombies.Add(actor);
            Emit("zombie_spawned", "type", entry.Type.ToString(), "routeId", entry.Route.ToString());
            if (entry.Type == ZombieType.Ripper) Emit("ripper_spawned", "routeId", entry.Route.ToString());
        }

        private Vector3[] CreateZombieApproachPath(RouteDefinitionAsset route, ZombieConfig config)
        {
            var source = route.waypoints;
            var result = new Vector3[source.Length];
            var maximumLateral = route.width * config.PathVariationFraction;
            for (var index = 0; index < source.Length; index++)
            {
                var previous = source[Mathf.Max(0, index - 1)];
                var next = source[Mathf.Min(source.Length - 1, index + 1)];
                var tangent = next - previous;
                tangent.y = 0f;
                if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.forward;
                tangent.Normalize();
                var side = Vector3.Cross(Vector3.up, tangent).normalized;
                var lateral = (rng.NextFloat() * 2f - 1f) * maximumLateral;
                result[index] = source[index] + side * lateral;
            }

            var startForward = source.Length > 1 ? source[1] - source[0] : Vector3.forward;
            startForward.y = 0f;
            if (startForward.sqrMagnitude < 0.001f) startForward = Vector3.forward;
            startForward.Normalize();
            var startSide = Vector3.Cross(Vector3.up, startForward).normalized;
            var bestPosition = result[0];
            var bestClearance = -1f;
            for (var attempt = 0; attempt < 24; attempt++)
            {
                var lateral = (rng.NextFloat() * 2f - 1f) * maximumLateral;
                var longitudinal = (rng.NextFloat() * 2f - 1f) * config.SeparationRadius * 2f;
                var candidate = source[0] + startSide * lateral + startForward * longitudinal;
                var clearance = MinimumZombieClearance(candidate, config.SeparationRadius);
                if (clearance <= bestClearance) continue;
                bestPosition = candidate;
                bestClearance = clearance;
            }
            result[0] = bestPosition;
            return result;
        }

        private float MinimumZombieClearance(Vector3 candidate, float desiredRadius)
        {
            var minimum = float.MaxValue;
            foreach (var other in AliveZombies)
            {
                if (other == null) continue;
                var required = Mathf.Max(desiredRadius, other.SeparationRadius);
                minimum = Mathf.Min(minimum, Vector3.Distance(candidate, other.transform.position) / required);
            }
            return minimum == float.MaxValue ? 1000f : minimum;
        }

        public Vector3 GetZombieAvoidance(ZombieActor actor)
        {
            if (actor == null) return Vector3.zero;
            var result = Vector3.zero;
            foreach (var other in AliveZombies)
            {
                if (other == null || other == actor) continue;
                var delta = actor.transform.position - other.transform.position;
                delta.y = 0f;
                var desired = Mathf.Max(actor.SeparationRadius, other.SeparationRadius);
                var distance = delta.magnitude;
                if (distance >= desired) continue;
                if (distance < 0.001f)
                {
                    var sign = string.CompareOrdinal(actor.State.Id, other.State.Id) < 0 ? -1f : 1f;
                    delta = new Vector3(sign, 0f, 0f);
                    distance = 0f;
                }
                result += delta.normalized * (1f - distance / desired);
            }
            return result;
        }

        private void HandlePhaseClear()
        {
            Emit("phase_cleared", "phase", phaseState.Number.ToString());
            Emit("base_hp_sampled", "hp", BaseState.Health.Current.ToString("F1"));
            Emit("player_hp_at_phase_end", "hp", PlayerState.Health.Current.ToString("F1"));
            Radio("radio.phase_clear");
            upgradeView.Show(upgradeSystem.Offer(rng, Session.SelectedUpgrades));
        }

        public void SelectUpgrade(UpgradeConfig selected)
        {
            var states = new List<RobotState>();
            foreach (var robot in Robots) states.Add(robot.State);
            if (!upgradeSystem.Apply(selected, Session, BaseState, states, PlayerState, Modifiers)) return;
            Emit("upgrade_selected", "upgradeId", selected.Id, "rewardStep", phaseState.Number.ToString());
            upgradeView.Hide();
            BeginPhase(phaseState.Number + 1);
        }

        public void NotifyZombieKilled(ZombieActor zombie, string source)
        {
            AliveZombies.Remove(zombie);
            Emit("zombie_killed", "type", zombie.Type.ToString(), "by", source);
        }

        public void NotifyPlayerHit(HitRegion region, float damage, bool killed)
        {
            Emit("player_hit_confirmed", "region", region.ToString(), "damage", damage.ToString("F1"),
                "killed", killed.ToString());
        }

        public void DamageBase(float amount)
        {
            var applied = CombatRules.ApplyDamage(BaseState.Health, amount);
            Emit("base_damaged", "amount", applied.ToString("F1"), "hp", BaseState.Health.Current.ToString("F1"));
            CheckImmediateDefeat();
        }

        public void CheckImmediateDefeat()
        {
            if (!BaseState.Health.IsDead && !PlayerState.Health.IsDead) return;
            var transition = phaseSystem.Evaluate(Session, phaseState, BaseState, PlayerState);
            if (transition == PhaseTransition.Defeat) FinishSession(false);
        }

        private void FinishSession(bool victory)
        {
            if (sessionEnded) return;
            sessionEnded = true;
            paused = false;
            Time.timeScale = 1f;
            if (victory)
            {
                Session.Result = GameResult.Victory;
                Emit("phase_cleared", "phase", phaseState.Number.ToString());
                Emit("base_hp_sampled", "hp", BaseState.Health.Current.ToString("F1"));
                Emit("player_hp_at_phase_end", "hp", PlayerState.Health.Current.ToString("F1"));
                Radio("radio.phase_clear");
                Radio("radio.victory");
            }
            else
            {
                Emit("phase_failed", "defeatReason", Session.DefeatReason.ToString());
            }
            Emit("session_ended", "result", Session.Result.ToString(), "defeatReason", Session.DefeatReason.ToString(),
                "durationSeconds", Session.ElapsedTime.ToString("F1"));
            telemetrySink.Flush();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void UpdateWarnings()
        {
            var baseChanged = warningSystem.TryBaseTransition(BaseState.Health.Current, BaseState.Health.Maximum, out var baseActive);
            if (baseChanged)
            {
                BaseState.WarningActive = baseActive;
                Emit("base_warning", "active", baseActive.ToString());
                if (baseActive) Radio("radio.base_danger");
            }
            for (var index = 0; index < Robots.Count; index++)
            {
                if (!warningSystem.TryBatteryTransition(Robots[index].State.Id, Robots[index].State.Battery, Robots[index].State.MaximumBattery, out var severity)) continue;
                Emit("battery_warning", "robotId", Robots[index].State.Id, "severity", severity.ToString());
                if (severity != WarningSeverity.None) Radio("radio.battery_warning");
            }
        }

        private void UpdateSampleTelemetry()
        {
            baseSampleTimer += Time.deltaTime;
            if (baseSampleTimer >= Config.Telemetry.SampleIntervalSeconds)
            {
                baseSampleTimer -= Config.Telemetry.SampleIntervalSeconds;
                Emit("base_hp_sampled", "hp", BaseState.Health.Current.ToString("F1"));
            }
            pressureTimer += Time.deltaTime;
            if (pressureTimer < Config.Telemetry.RoutePressureSampleIntervalSeconds) return;
            pressureTimer -= Config.Telemetry.RoutePressureSampleIntervalSeconds;
            foreach (var route in phaseState.OpenRoutes)
            {
                var count = AliveZombies.FindAll(item => item.State.Route == route).Count;
                Emit("route_pressure_sampled", "routeId", route.ToString(), "aliveCount", count.ToString());
            }
        }

        public Vector3 GetRobotFormationPosition(HaetaeRobotActor robot, Vector3 center)
        {
            var index = Mathf.Max(0, Robots.IndexOf(robot));
            var count = Mathf.Max(1, Robots.Count);
            var centeredIndex = index - (count - 1) * 0.5f;
            return center + Vector3.right * (centeredIndex * Config.Robot.FormationSpacing);
        }

        public Vector3 GetRobotAvoidance(HaetaeRobotActor robot)
        {
            if (robot == null) return Vector3.zero;
            var result = Vector3.zero;
            foreach (var other in Robots)
            {
                if (other == null || other == robot || other.State.IsDestroyed) continue;
                var delta = robot.transform.position - other.transform.position;
                delta.y = 0f;
                var distance = delta.magnitude;
                var desired = Mathf.Max(robot.SeparationRadius, other.SeparationRadius);
                if (distance >= desired) continue;
                if (distance < 0.001f)
                {
                    var sign = string.CompareOrdinal(robot.State.Id, other.State.Id) < 0 ? -1f : 1f;
                    delta = new Vector3(sign, 0f, 0f);
                    distance = 0f;
                }
                result += delta.normalized * (1f - distance / desired);
            }
            return result;
        }

        public Vector3 ResolveRobotSeparation(HaetaeRobotActor robot, Vector3 candidate)
        {
            if (robot == null) return candidate;
            foreach (var other in Robots)
            {
                if (other == null || other == robot || other.State.IsDestroyed) continue;
                var delta = candidate - other.transform.position;
                delta.y = 0f;
                var distance = delta.magnitude;
                var required = Mathf.Max(robot.SeparationRadius, other.SeparationRadius);
                if (distance >= required) continue;
                if (distance < 0.001f)
                {
                    var sign = string.CompareOrdinal(robot.State.Id, other.State.Id) < 0 ? -1f : 1f;
                    delta = new Vector3(sign, 0f, 0f);
                }
                candidate += delta.normalized * (required - distance);
            }
            return candidate;
        }

        public ZombieActor FindRobotTarget(HaetaeRobotActor robot, float radius)
        {
            ZombieActor best = null;
            var bestScore = float.MaxValue;
            var bestRobotDistance = float.MaxValue;
            var defending = robot.State.Command == RobotCommand.DefendPosition;
            foreach (var zombie in AliveZombies)
            {
                if (zombie == null) continue;
                if ((robot.State.Command == RobotCommand.PatrolRoute || robot.State.Command == RobotCommand.DefendPosition) &&
                    zombie.State.Route != robot.State.AssignedRoute) continue;
                var robotDistance = Vector3.Distance(robot.transform.position, zombie.transform.position);
                var score = robotDistance;
                if (defending)
                {
                    score = Vector3.Distance(BaseTransform.position, zombie.transform.position);
                    if (score > Config.Robot.DefendLeashRadius) continue;
                }
                else if (robotDistance > radius) continue;
                if (score > bestScore || Mathf.Approximately(score, bestScore) && robotDistance >= bestRobotDistance) continue;
                best = zombie;
                bestScore = score;
                bestRobotDistance = robotDistance;
            }
            return best;
        }

        public bool IsRobotFollowUpTargetValid(HaetaeRobotActor robot, ZombieActor zombie, float radius)
        {
            if (robot == null || zombie == null || !AliveZombies.Contains(zombie)) return false;
            if (Vector3.Distance(robot.transform.position, zombie.transform.position) > radius) return false;
            return robot.State.Command != RobotCommand.DefendPosition ||
                Vector3.Distance(BaseTransform.position, zombie.transform.position) <= Config.Robot.DefendLeashRadius;
        }

        public ZombieActor FindRobotFollowUpTarget(HaetaeRobotActor robot, float radius)
        {
            ZombieActor best = null;
            var bestDistance = float.MaxValue;
            foreach (var zombie in AliveZombies)
            {
                if (!IsRobotFollowUpTargetValid(robot, zombie, radius)) continue;
                var distance = Vector3.Distance(robot.transform.position, zombie.transform.position);
                if (distance >= bestDistance) continue;
                best = zombie;
                bestDistance = distance;
            }
            return best;
        }

        public void NotifyRipperAttack(HaetaeRobotActor robot)
        {
            Emit("ripper_attacked_robot", "robotId", robot.State.Id, "batteryDrained", Config.Battery.RipperHitDrain.ToString("F1"));
        }

        public bool TryGetNearbySupply(Vector3 playerPosition, out SupplyKind kind)
        {
            var safeDistance = HorizontalDistance(playerPosition, ToVector(Config.World.SafeSupply));
            var riskyDistance = HorizontalDistance(playerPosition, ToVector(Config.World.RiskySupply));
            kind = safeDistance <= riskyDistance ? SupplyKind.Safe : SupplyKind.Risky;
            return Mathf.Min(safeDistance, riskyDistance) <= Config.World.SupplyInteractionRadius;
        }

        public bool TryResupply(Vector3 playerPosition)
        {
            if (resupplyInProgress || PlayerState.Ammo.Reserve >= Config.Ammo.ReserveAmmoMax ||
                !TryGetNearbySupply(playerPosition, out var kind)) return false;
            resupplyInProgress = true;
            resupplyRemaining = Config.Ammo.ResupplyUseSeconds;
            resupplyKind = kind;
            return true;
        }

        private void UpdateResupply()
        {
            if (!resupplyInProgress || PlayerActor == null) return;
            var supplyPosition = resupplyKind == SupplyKind.Safe ? Config.World.SafeSupply : Config.World.RiskySupply;
            var exitRadius = Config.World.SupplyInteractionRadius + Config.World.SupplyExitTolerance;
            if (HorizontalDistance(PlayerActor.transform.position, ToVector(supplyPosition)) > exitRadius)
            {
                resupplyInProgress = false;
                resupplyRemaining = 0f;
                return;
            }
            resupplyRemaining = Mathf.Max(0f, resupplyRemaining - Time.deltaTime);
            if (resupplyRemaining > 0f) return;
            CombatRules.Resupply(PlayerState.Ammo, Config.Ammo);
            resupplyInProgress = false;
            Emit("ammo_resupplied", "supplyKind", resupplyKind.ToString());
        }

        private static float HorizontalDistance(Vector3 first, Vector3 second)
        {
            var delta = first - second;
            delta.y = 0f;
            return delta.magnitude;
        }

        public bool TryGetBarrier(RouteId route, out BarrierRuntime barrier)
        {
            return barriers.TryGetValue(route, out barrier) && barrier != null && barrier.IsAlive;
        }

        private void SpawnBarriers(IEnumerable<RouteId> routes)
        {
            foreach (var route in routes)
            {
                var routeAsset = catalog.Route(route);
                var position = routeAsset.waypoints[routeAsset.waypoints.Length - 2];
                var barrierObject = CreateBox(route + " Emergency Barrier", position, new Vector3(6f, 2.5f, 0.6f), new Color(0.25f, 0.8f, 1f));
                var inwardDirection = routeAsset.waypoints[routeAsset.waypoints.Length - 1] - position;
                inwardDirection.y = 0f;
                if (inwardDirection.sqrMagnitude > 0.0001f)
                    barrierObject.transform.rotation = Quaternion.LookRotation(inwardDirection.normalized, Vector3.up);
                var barrier = barrierObject.AddComponent<BarrierRuntime>();
                barrier.Initialize(this, route, Config.Barrier.MaxHealth);
                barriers[route] = barrier;
            }
        }

        private void RemoveBarriers()
        {
            foreach (var barrier in barriers.Values) if (barrier != null) Destroy(barrier.gameObject);
            barriers.Clear();
        }

        private void SpawnMedicalRobot()
        {
            var medicalObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            medicalObject.name = "Medical Robot";
            medicalObject.transform.position = ToVector(Config.World.MedicalAnchor);
            medicalObject.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
            ApplyColor(medicalObject, new Color(0.2f, 1f, 0.65f));
            MedicalActor = medicalObject.AddComponent<MedicalRobotActor>();
            MedicalActor.Initialize(this, Config.Medical);
            var zone = CreateCylinder("Medical Zone", medicalObject.transform.position + Vector3.down * 0.65f,
                new Vector3(Config.Medical.Radius * 2f, 0.05f, Config.Medical.Radius * 2f), new Color(0.1f, 0.75f, 0.45f, 0.2f));
            var collider = zone.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        private void HighlightRoute(RouteId route)
        {
            if (!routeMaterials.TryGetValue(route, out var materials)) return;
            foreach (var material in materials) material.color = catalog.Route(route).routeColor;
        }

        public void Radio(string key)
        {
            Emit("radio_event", "key", key);
        }

        public void Emit(string eventName)
        {
            Emit(eventName, new Dictionary<string, string>());
        }

        public void Emit(string eventName, string key, string value)
        {
            Emit(eventName, new Dictionary<string, string> { { key, value } });
        }

        public void Emit(string eventName, string key1, string value1, string key2, string value2)
        {
            Emit(eventName, new Dictionary<string, string> { { key1, value1 }, { key2, value2 } });
        }

        public void Emit(string eventName, string key1, string value1, string key2, string value2, string key3, string value3)
        {
            Emit(eventName, new Dictionary<string, string> { { key1, value1 }, { key2, value2 }, { key3, value3 } });
        }

        public void Emit(string eventName, Dictionary<string, string> payload)
        {
            var gameEvent = new DomainEvent(eventName, Session == null ? 0f : Session.ElapsedTime, CurrentPhase);
            foreach (var pair in payload) gameEvent.With(pair.Key, pair.Value);
            events.Publish(gameEvent);
        }

        public GameObject SpawnPulse(Vector3 position, float diameter, Color color, float lifetime = 0.25f,
            string effectName = "Effect Pulse", float lightIntensity = 0f)
        {
            var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = effectName;
            pulse.transform.position = position;
            pulse.transform.localScale = Vector3.one * diameter;
            ApplyColor(pulse, color);
            var collider = pulse.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            if (lightIntensity > 0f)
            {
                var pulseLight = pulse.AddComponent<Light>();
                pulseLight.type = LightType.Point;
                pulseLight.color = color;
                pulseLight.intensity = lightIntensity;
                pulseLight.range = Mathf.Max(1.5f, diameter * 7f);
            }
            Destroy(pulse, Mathf.Max(0.01f, lifetime));
            return pulse;
        }

        private GameObject CreateBox(string name, Vector3 position, Vector3 scale, Color color)
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            result.name = name;
            result.transform.position = position;
            result.transform.localScale = scale;
            ApplyColor(result, color);
            return result;
        }

        private GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Color color)
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            result.name = name;
            result.transform.position = position;
            result.transform.localScale = scale;
            ApplyColor(result, color);
            return result;
        }

        private void ApplyColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            var template = catalog == null ? null : catalog.runtimeMaterialTemplate;
            if (template == null || template.shader == null)
            {
                Debug.LogError("Runtime material template is unavailable. Rebuild the MVP project assets before creating a player build.");
                return;
            }

            renderer.sharedMaterial = new Material(template) { color = color };
        }
    }

}
