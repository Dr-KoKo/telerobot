using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using Telerobot.Game.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Telerobot.Game.Editor
{
    public static class MvpProjectBuilder
    {
        private const string DataRoot = "Assets/Game/Data/Assets";
        private const string MainMenuScenePath = "Assets/Game/Scenes/MainMenu.unity";
        private const string MvpScenePath = "Assets/Game/Scenes/MVP.unity";
        private const string VisualThemePath = DataRoot + "/VisualTheme.asset";
        private const string DesignAssetCatalogPath = DataRoot + "/DesignAssetCatalog.asset";
        private const string HaetaeGeneralLod0Path =
            "Assets/Game/Art/Models/Haetae/Haetae_General_LOD0.fbx";
        private const string HaetaeGeneralLod1Path =
            "Assets/Game/Art/Models/Haetae/Haetae_General_LOD1.fbx";
        private const string HaetaeMeleeLod0Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Melee_LOD0.fbx";
        private const string HaetaeMeleeLod1Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Melee_LOD1.fbx";
        private const string HaetaeRangedLod0Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Ranged_LOD0.fbx";
        private const string HaetaeRangedLod1Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Ranged_LOD1.fbx";
        private const string HaetaeBalancedLod0Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Balanced_LOD0.fbx";
        private const string HaetaeBalancedLod1Path =
            "Assets/Game/Art/Models/Haetae/Haetae_Balanced_LOD1.fbx";
        private const string ZombieRunnerLod0Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Runner_LOD0.fbx";
        private const string ZombieRunnerLod1Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Runner_LOD1.fbx";
        private const string ZombieBruiserLod0Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Bruiser_LOD0.fbx";
        private const string ZombieBruiserLod1Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Bruiser_LOD1.fbx";
        private const string ZombieRipperLod0Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Ripper_LOD0.fbx";
        private const string ZombieRipperLod1Path =
            "Assets/Game/Art/Models/Zombies/Zombie_Ripper_LOD1.fbx";

        [InitializeOnLoadMethod]
        private static void ScheduleFirstImportBuild()
        {
            if (File.Exists(MainMenuScenePath) && File.Exists(MvpScenePath) &&
                File.Exists(VisualThemePath) && File.Exists(DesignAssetCatalogPath)) return;
            EditorApplication.delayCall += () =>
            {
                if ((!File.Exists(MainMenuScenePath) || !File.Exists(MvpScenePath) ||
                     !File.Exists(VisualThemePath) || !File.Exists(DesignAssetCatalogPath)) &&
                    !EditorApplication.isCompiling) BuildAll();
            };
        }

        [MenuItem("Tools/Telerobot/Build MVP Project")]
        public static void BuildAll()
        {
            EnsureFolder("Assets/Game/Data/Assets");
            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Art");
            EnsureFolder("Assets/Game/Art/Concepts");
            EnsureFolder("Assets/Game/Art/Concepts/Haetae");
            EnsureFolder("Assets/Game/Art/Fonts");
            EnsureFolder("Assets/Game/Art/Materials");
            EnsureFolder("Assets/Game/Art/Menu");
            EnsureFolder("Assets/Game/Art/Models");
            EnsureFolder("Assets/Game/Art/Models/Haetae");
            EnsureFolder("Assets/Game/Art/Models/Zombies");
            EnsureFolder("Assets/Game/Art/SourceRecords");

            var game = Asset<GameConfigAsset>("GameConfig", item =>
            {
                item.playerMaxHealth = 100f;
                item.targetSessionMinimumSeconds = 600f;
                item.targetSessionMaximumSeconds = 900f;
                item.fixedStepSeconds = 1f / 60f;
                item.playerMoveSpeed = 8f;
                item.sprintMultiplier = 1.5f;
                item.gravity = 24f;
                item.mouseSensitivity = 0.12f;
                item.cameraDistance = 6f;
                item.thirdPersonFieldOfView = 65f;
                item.firstPersonFieldOfView = 75f;
                item.firstPersonEyeHeight = 0.65f;
                item.cameraCollisionRadius = 0.22f;
                item.cameraCollisionPadding = 0.08f;
                item.jumpHeight = 1.35f;
                item.groundedVelocity = -2f;
            });
            var baseConfig = Asset<BaseConfigAsset>("BaseConfig", item =>
            {
                item.maxHealth = 1000f;
                item.phaseRecoveryFraction = 0.15f;
                item.warningFraction = 0.30f;
                item.allowPlayerRepair = false;
            });
            var ammo = Asset<AmmoConfigAsset>("AmmoConfig", item =>
            {
                item.startReserveAmmo = 120;
                item.reserveAmmoMax = 240;
                item.resupplyPolicy = ResupplyPolicy.FullReserve;
                item.resupplyAmount = 0;
                item.resupplyUseSeconds = 1.5f;
                item.resupplyCooldownSeconds = 0f;
                item.grenadeResupplyPolicy = GrenadeResupplyPolicy.PhaseResetOnly;
            });
            var weapon = Asset<WeaponDefinitionAsset>("AssaultRifle", item =>
            {
                item.baseDamage = 30f;
                item.headshotMultiplier = 2.5f;
                item.magazineSize = 30;
                item.reloadSeconds = 2f;
                item.fireIntervalSeconds = 0.12f;
                item.grenadesPerPhase = 2;
                item.range = 200f;
                item.recoilPitchMinimumDegrees = 0.7f;
                item.recoilPitchMaximumDegrees = 1.15f;
                item.recoilYawMaximumDegrees = 0.32f;
                item.recoilRecoveryDegreesPerSecond = 9f;
                item.muzzleFlashSeconds = 0.07f;
                item.muzzleFlashSize = 0.32f;
                item.impactPulseSize = 0.22f;
                item.fireSoundFrequency = 135f;
                item.bodyHitSoundFrequency = 520f;
                item.headshotSoundFrequency = 920f;
                item.combatSoundSeconds = 0.09f;
                item.fireSoundVolume = 0.32f;
                item.hitSoundVolume = 0.22f;
            });
            var grenade = Asset<GrenadeDefinitionAsset>("Grenade", item =>
            {
                item.radius = 5f;
                item.innerRadius = 2f;
                item.centerDamage = 150f;
                item.edgeDamage = 60f;
                item.maxTargets = 10;
                item.throwDistance = 8f;
            });
            var battery = Asset<BatteryConfigAsset>("BatteryConfig", item =>
            {
                item.maximum = 100f;
                item.lowPowerMaximum = 30f;
                item.criticalMaximum = 10f;
                item.idleDrainPerSecond = 0.3f;
                item.patrolDrainPerSecond = 0.8f;
                item.combatDrainPerSecond = 2.5f;
                item.ripperHitDrain = 5f;
                item.chargePerSecond = 4f;
                item.lowPowerMoveMultiplier = 0.85f;
                item.lowPowerAttackMultiplier = 0.90f;
                item.disabledHoldSeconds = 5f;
                item.recoveryPerSecond = 0.5f;
                item.moveEnableThreshold = 5f;
                item.yellowWarningFraction = 0.25f;
                item.redWarningFraction = 0.10f;
            });
            var robot = Asset<RobotDefinitionAsset>("HaetaeRobot", item =>
            {
                item.maxHealth = 300f;
                item.moveSpeed = 10f;
                item.dashDamage = 60f;
                item.biteDamage = 40f;
                item.biteCooldownSeconds = 0.6f;
                item.dashCooldownSeconds = 3f;
                item.detectionRadius = 15f;
                item.engageRange = 2f;
                item.separationRadius = 2.2f;
                item.separationStrength = 1.8f;
                item.formationSpacing = 3f;
                item.defendLeashRadius = 14f;
                item.runnerKillTargetMinimumSeconds = 1f;
                item.runnerKillTargetMaximumSeconds = 2f;
                item.bruiserKillTargetMinimumSeconds = 6f;
                item.bruiserKillTargetMaximumSeconds = 10f;
            });
            var meleeSpecialization = HaetaeSpecializationAsset(
                "HaetaeMelee", HaetaeSpecialization.Melee,
                "haetae.specialization.melee", "haetae.specialization.melee.description",
                0f, 2f, 4f, 4f, 0f, 0f, 2.5f, 3, 0.7f, 1.2f,
                new Color(0.95f, 0.32f, 0.14f), new Vector3(1.12f, 1.08f, 1.12f),
                new Color(1f, 0.38f, 0.12f), new Color(1f, 0.55f, 0.2f));
            var rangedSpecialization = HaetaeSpecializationAsset(
                "HaetaeRanged", HaetaeSpecialization.Ranged,
                "haetae.specialization.ranged", "haetae.specialization.ranged.description",
                6f, 12f, 0f, 0f, 200f, 0.35f, 0f, 1, 1.15f, 1f,
                new Color(0.2f, 0.65f, 1f), new Vector3(0.96f, 1f, 1.08f),
                new Color(0.15f, 0.75f, 1f), new Color(0.25f, 0.9f, 1f));
            var balancedSpecialization = HaetaeSpecializationAsset(
                "HaetaeBalanced", HaetaeSpecialization.Balanced,
                "haetae.specialization.balanced", "haetae.specialization.balanced.description",
                0f, 8f, 2.5f, 2.5f, 190f, 0.35f, 0f, 1, 1f, 0.9f,
                new Color(0.68f, 0.38f, 0.95f), Vector3.one,
                new Color(0.78f, 0.45f, 1f), new Color(0.65f, 0.55f, 1f));
            var haetaeProgression = Asset<HaetaeProgressionDefinitionAsset>("HaetaeProgression", item =>
            {
                item.experiencePerLevel = 75;
                item.readyAlertSeconds = 4f;
                item.powerDamageBonusPerRank = 0.10f;
                item.armorDamageReductionPerRank = 0.08f;
                item.efficiencyBatteryReductionPerRank = 0.08f;
                item.attackSpeedBonusPerRank = 0.10f;
                item.minimumReductionMultiplier = 0.50f;
                item.specializations = new[] { meleeSpecialization, rangedSpecialization, balancedSpecialization };
            });
            var medical = Asset<MedicalRobotDefinitionAsset>("MedicalRobot", item =>
            {
                item.maxHealth = 150f;
                item.healPerSecond = 8f;
                item.radius = 6f;
            });
            var barrier = Asset<BarrierConfigAsset>("BarrierConfig", item => item.maxHealth = 300f);
            var warnings = Asset<WarningConfigAsset>("WarningConfig", item =>
            {
                item.batteryYellowFraction = 0.25f;
                item.batteryRedFraction = 0.10f;
            });
            var world = Asset<WorldLayoutAsset>("WorldLayout", item =>
            {
                item.basePosition = Vector3.zero;
                item.playerStart = new Vector3(0f, 1f, -7f);
                item.robotStarts = new[] { new Vector3(-2.4f, 0.8f, -3.8f), new Vector3(2.4f, 0.8f, -3.8f) };
                item.baseRally = new Vector3(0f, 0.8f, -4f);
                item.chargingStation = new Vector3(4.5f, 0.5f, -4.5f);
                item.safeSupply = new Vector3(-3f, 0.5f, -2f);
                item.riskySupply = new Vector3(0f, 0.5f, 18f);
                item.medicalAnchor = new Vector3(-4.5f, 0.8f, -4.5f);
                item.supplyInteractionRadius = 2.5f;
                item.supplyExitTolerance = 0.75f;
                item.baseChargingRadius = 6f;
                item.chargingArrivalRadius = 1.2f;
            });
            var commands = Asset<CommandConfigAsset>("CommandConfig", item => item.commands = new[]
            {
                RobotCommand.DefendPosition, RobotCommand.PatrolRoute, RobotCommand.ReturnToBase
            });
            var hud = Asset<HudConfigAsset>("HudConfig", item =>
            {
                item.elements = new[] { "baseHp", "phaseProgress", "routeAlert", "robotBattery", "playerHp", "ammo", "commandMenu" };
                item.informationPriority = new[] { "baseHp", "robotBattery", "routeAlert" };
                item.lowAmmoThreshold = 6;
                item.damageIndicatorSeconds = 0.8f;
                item.hitMarkerSeconds = 0.22f;
                item.headshotLabelSeconds = 0.55f;
            });
            var playerSettings = Asset<PlayerSettingsAsset>("PlayerSettings", item =>
            {
                item.minimumMouseSensitivity = 0.04f;
                item.maximumMouseSensitivity = 0.35f;
                item.defaultMouseSensitivity = game.mouseSensitivity;
                item.defaultMasterVolume = 0.85f;
                item.defaultEffectsVolume = 0.9f;
                item.minimumResolutionWidth = 960;
                item.minimumResolutionHeight = 540;
                item.defaultResolutionWidth = 1280;
                item.defaultResolutionHeight = 720;
                item.defaultFullscreen = false;
                item.defaultPerspective = CameraPerspective.ThirdPerson;
            });
            var telemetry = Asset<TelemetryConfigAsset>("TelemetryConfig", item =>
            {
                item.enabledEvents = new[]
                {
                    "session_started", "session_ended", "phase_started", "phase_cleared", "phase_failed",
                    "zombie_spawned", "zombie_killed", "base_damaged", "player_damaged", "player_died",
                    "robot_battery_changed", "robot_auto_charge_started", "robot_disabled", "ripper_attacked_robot",
                    "route_pressure_sampled", "simulation_run_completed", "base_hp_sampled",
                    "player_hp_at_phase_end", "grenade_used", "ammo_resupplied", "barrier_damaged", "barrier_destroyed",
                    "medical_heal_applied", "medical_robot_destroyed", "robot_damaged", "robot_destroyed", "base_warning", "battery_warning",
                    "radio_event", "ripper_spawned", "route_opened", "camera_perspective_changed",
                    "player_jumped", "player_hit_confirmed", "game_paused", "session_restarted",
                    "returned_to_main_menu", "haetae_xp_gained", "haetae_level_reached",
                    "haetae_specialization_ready", "haetae_specialization_selected",
                    "haetae_mastery_point_gained", "haetae_mastery_selected"
                };
                item.sinkFolder = "Telerobot/Telemetry";
                item.requiredFields = new[] { "buildVersion", "dataVersion", "sessionId", "seed", "simProfileId", "phase", "simTime" };
                item.sampleIntervalSeconds = 1f;
                item.routePressureSampleIntervalSeconds = 2f;
                item.batteryEmitPolicy = BatteryEmitPolicy.OnThresholdCrossing | BatteryEmitPolicy.EveryNSeconds;
                item.batteryEmitIntervalSeconds = 1f;
            });
            var validation = Asset<ValidationConfigAsset>("ValidationConfig", item =>
            {
                item.seeds = new[] { 1001, 1002, 1003, 2001, 2002, 2003 };
                item.fixedStepSeconds = 1f / 60f;
            });
            var safeSupply = Asset<SupplyPointDefinitionAsset>("SafeSupplyPoint", item =>
            {
                item.id = "safe";
                item.kind = SupplyKind.Safe;
                item.position = world.safeSupply;
                item.interactionRadius = world.supplyInteractionRadius;
            });
            var riskySupply = Asset<SupplyPointDefinitionAsset>("RiskySupplyPoint", item =>
            {
                item.id = "risky";
                item.kind = SupplyKind.Risky;
                item.position = world.riskySupply;
                item.interactionRadius = world.supplyInteractionRadius;
            });
            var noviceProfile = SimProfile("SimPlayerNovice", SimProfileId.Novice, 0.55f, 0.10f, 1.2f, 2.6f,
                SimRoutePriorityPolicy.LateReactive, 0.2f, 0.10f, SimGrenadeUsePolicy.Rarely, 8,
                new HaetaeSpecializationPair(HaetaeSpecialization.Balanced, HaetaeSpecialization.Balanced));
            var baselineProfile = SimProfile("SimPlayerBaseline", SimProfileId.Baseline, 0.75f, 0.25f, 0.6f, 1.8f,
                SimRoutePriorityPolicy.BalancedCoverage, 0.6f, 0.25f, SimGrenadeUsePolicy.DenseClusters, 4,
                new HaetaeSpecializationPair(HaetaeSpecialization.Melee, HaetaeSpecialization.Ranged));
            var skilledProfile = SimProfile("SimPlayerSkilled", SimProfileId.Skilled, 0.92f, 0.45f, 0.25f, 1.0f,
                SimRoutePriorityPolicy.HighestPressure, 1f, 0.40f, SimGrenadeUsePolicy.DenseClustersAndBruisers, 3,
                new HaetaeSpecializationPair(HaetaeSpecialization.Ranged, HaetaeSpecialization.Melee));

            var runner = Zombie("Runner", ZombieType.Runner, 5, 90f, 6.5f, 8f, 12f, 8f, 1f, 1, 1,
                new[] { TargetKind.Base, TargetKind.Player, TargetKind.Robot }, new Color(0.38f, 0.9f, 0.28f), new Vector3(0.72f, 0.86f, 0.72f));
            var bruiser = Zombie("Bruiser", ZombieType.Bruiser, 25, 500f, 2.6f, 60f, 30f, 25f, 2f, 5, 2,
                new[] { TargetKind.Base, TargetKind.Robot, TargetKind.Player }, new Color(0.52f, 0.24f, 0.12f), new Vector3(1.45f, 1.4f, 1.45f));
            var ripper = Zombie("Ripper", ZombieType.Ripper, 20, 180f, 7.2f, 10f, 18f, 18f, 0.9f, 4, 3,
                new[] { TargetKind.Robot, TargetKind.Player, TargetKind.Base }, new Color(0.92f, 0.05f, 0.45f), new Vector3(0.9f, 1.05f, 0.9f));

            var north = Route("NorthRoad", RouteId.NorthRoad, 1, "route.north", new Color(0.15f, 0.55f, 0.95f), 9f,
                new[] { new Vector3(0f, 1f, 50f), new Vector3(0f, 1f, 34f), new Vector3(0f, 1f, 20f), new Vector3(0f, 1f, 10f), new Vector3(0f, 1f, 4.5f) });
            var east = Route("EastAlley", RouteId.EastAlley, 2, "route.east", new Color(0.95f, 0.62f, 0.12f), 5f,
                new[] { new Vector3(40f, 1f, 13f), new Vector3(27f, 1f, 13f), new Vector3(16f, 1f, 9f), new Vector3(8f, 1f, 6f), new Vector3(4.5f, 1f, 2.5f) });
            var south = Route("SouthTunnel", RouteId.SouthTunnel, 3, "route.south", new Color(0.65f, 0.18f, 0.75f), 6f,
                new[] { new Vector3(-40f, 1f, -10f), new Vector3(-29f, 1f, -8f), new Vector3(-19f, 1f, -4f), new Vector3(-10f, 1f, 1f), new Vector3(-4.5f, 1f, 2.5f) });

            var phase1 = Phase("Phase1", 1, 40, 35f, new[] { RouteId.NorthRoad });
            var phase2 = Phase("Phase2", 2, 60, 40f, new[] { RouteId.NorthRoad, RouteId.EastAlley });
            var phase3 = Phase("Phase3", 3, 80, 40f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });
            var phase4 = Phase("Phase4", 4, 255, 100f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });
            var phase5 = Phase("Phase5", 5, 286, 100f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });
            var phase6 = Phase("Phase6", 6, 317, 100f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });
            var phase7 = Phase("Phase7", 7, 348, 100f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });
            var phase8 = Phase("Phase8", 8, 379, 100f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });

            var strings = BuildStringTable();
            var runtimeMaterial = MaterialAsset("RuntimeGreyboxMaterial");
            var visualTheme = BuildVisualTheme();
            var designAssets = BuildDesignAssetCatalog(visualTheme);

            var catalog = Asset<MvpContentCatalog>("MvpBalanceCatalog", item =>
            {
                item.dataVersion = "mvp-2.0.0";
                item.game = game;
                item.baseConfig = baseConfig;
                item.ammo = ammo;
                item.weapon = weapon;
                item.grenade = grenade;
                item.battery = battery;
                item.robot = robot;
                item.medical = medical;
                item.barrier = barrier;
                item.warnings = warnings;
                item.world = world;
                item.commands = commands;
                item.hud = hud;
                item.playerSettings = playerSettings;
                item.telemetry = telemetry;
                item.validation = validation;
                item.haetaeProgression = haetaeProgression;
                item.haetaeSpecializations = new[] { meleeSpecialization, rangedSpecialization, balancedSpecialization };
                item.simPlayerProfiles = new[] { noviceProfile, baselineProfile, skilledProfile };
                item.supplyPoints = new[] { safeSupply, riskySupply };
                item.zombies = new[] { runner, bruiser, ripper };
                item.phases = new[] { phase1, phase2, phase3, phase4, phase5, phase6, phase7, phase8 };
                item.routes = new[] { north, east, south };
                item.strings = strings;
                item.runtimeMaterialTemplate = runtimeMaterial;
                item.visualTheme = visualTheme;
                item.designAssets = designAssets;
            });

            MvpDataMapper.Validate(catalog);
            visualTheme.Validate();
            designAssets.Validate();
            BuildScenes(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlayerSettings.companyName = "Telerobot Team";
            PlayerSettings.productName = "TelerobotMVP";
            PlayerSettings.bundleVersion = "0.2.3";
            Debug.Log("Telerobot MVP assets and scenes built successfully: " + MainMenuScenePath + ", " + MvpScenePath);
        }

        [MenuItem("Tools/Telerobot/Open Guardian Visual Gallery")]
        public static void OpenVisualGallery()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(VisualThemePath);
            var template = AssetDatabase.LoadAssetAtPath<Material>(DataRoot + "/RuntimeGreyboxMaterial.mat");
            if (theme == null || template == null)
            {
                BuildAll();
                theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(VisualThemePath);
                template = AssetDatabase.LoadAssetAtPath<Material>(DataRoot + "/RuntimeGreyboxMaterial.mat");
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var lightObject = new GameObject("Gallery Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            lightObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            var cameraObject = new GameObject("Gallery Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0f, 4.8f, -15f);
            cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = theme.ColorFor("world.ground", new Color(0.02f, 0.03f, 0.05f));

            var library = new PresentationMaterialLibrary(theme, template);
            var factory = new LowPolyModelFactory(library);
            var roles = new[]
            {
                PresentationRole.HaetaeGeneralUnit1,
                PresentationRole.HaetaeGeneralUnit2,
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview,
                PresentationRole.MedicalRobot,
                PresentationRole.Runner,
                PresentationRole.Bruiser,
                PresentationRole.Ripper
            };
            for (var index = 0; index < roles.Length; index++)
            {
                var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.name = roles[index].ToString();
                root.transform.position = new Vector3((index % 5 - 2) * 3f, index < 5 ? 1.2f : -1.8f, 0f);
                factory.Attach(root, roles[index]);
            }
            Selection.activeGameObject = cameraObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static ZombieDefinitionAsset Zombie(string name, ZombieType type, int experienceReward, float hp, float speed, float baseDamage,
            float playerDamage, float robotDamage, float attackInterval, int cost, int firstPhase, TargetKind[] priority,
            Color color, Vector3 scale)
        {
            return Asset<ZombieDefinitionAsset>(name, item =>
            {
                item.type = type;
                item.haetaeExperienceReward = experienceReward;
                item.maxHealth = hp;
                item.moveSpeed = speed;
                item.baseDamage = baseDamage;
                item.playerDamage = playerDamage;
                item.robotDamage = robotDamage;
                item.attackInterval = attackInterval;
                item.attackRange = 1.8f;
                item.pathVariationFraction = 0.4f;
                item.separationRadius = type == ZombieType.Bruiser ? 1.9f : type == ZombieType.Ripper ? 1.3f : 1.1f;
                item.separationStrength = 1.6f;
                item.threatCost = cost;
                item.firstPhase = firstPhase;
                item.targetPriority = priority;
                item.displayColor = color;
                item.displayScale = scale;
                item.hitFlashSeconds = 0.12f;
                item.deathEffectSeconds = 0.42f;
                item.deathPulseSize = type == ZombieType.Bruiser ? 1.4f : 0.9f;
            });
        }

        private static RouteDefinitionAsset Route(string name, RouteId id, int phase, string key, Color color, float width, Vector3[] points)
        {
            return Asset<RouteDefinitionAsset>(name, item =>
            {
                item.id = id;
                item.openPhase = phase;
                item.displayNameKey = key;
                item.routeColor = color;
                item.width = width;
                item.waypoints = points;
            });
        }

        private static PhaseDefinitionAsset Phase(string name, int number, int budget, float duration, RouteId[] routes)
        {
            return Asset<PhaseDefinitionAsset>(name, item =>
            {
                item.number = number;
                item.threatBudget = budget;
                item.targetDurationSeconds = duration;
                item.openRoutes = routes;
                item.opensNewRoute = number <= 3;
                item.newlyOpenedRoute = routes[routes.Length - 1];
                item.phaseStartDelaySeconds = 2f;
                item.trimOrder = new[] { SpawnTrimTarget.Runner, SpawnTrimTarget.Bruiser };

                if (number == 1)
                {
                    item.runnerCount = Range(18, 24);
                    item.bruiserCount = Range(0, 0);
                    item.ripperCount = Range(0, 0);
                    item.learningTotal = Range(18, 24);
                    item.groupIntervalSeconds = 4f;
                    item.groupSize = Range(3, 4);
                    item.maxAliveConcurrent = 15;
                    item.routeWeights = Weights(RouteId.NorthRoad, 1f);
                    item.zombieTypeRouteWeights = new[]
                    {
                        TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 1f))
                    };
                }
                else if (number == 2)
                {
                    item.runnerCount = Range(28, 36);
                    item.bruiserCount = Range(2, 3);
                    item.ripperCount = Range(0, 0);
                    item.learningTotal = Range(30, 39);
                    item.bruiserMinimum = 2;
                    item.groupIntervalSeconds = 3.5f;
                    item.groupSize = Range(3, 5);
                    item.maxAliveConcurrent = 20;
                    item.routeWeights = Weights(RouteId.NorthRoad, 0.55f, RouteId.EastAlley, 0.45f);
                    item.zombieTypeRouteWeights = new[]
                    {
                        TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 0.6f, RouteId.EastAlley, 0.4f)),
                        TypeWeights(ZombieType.Bruiser, Weights(RouteId.NorthRoad, 0.65f, RouteId.EastAlley, 0.35f))
                    };
                }
                else if (number == 3)
                {
                    item.runnerCount = Range(42, 48);
                    item.bruiserCount = Range(2, 3);
                    item.ripperCount = Range(3, 4);
                    item.learningTotal = Range(47, 55);
                    item.bruiserMinimum = 2;
                    item.ripperMinimum = 3;
                    item.groupIntervalSeconds = 3f;
                    item.groupSize = Range(4, 6);
                    item.maxAliveConcurrent = 24;
                    item.routeWeights = Weights(RouteId.NorthRoad, 0.4f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.3f);
                    item.zombieTypeRouteWeights = new[]
                    {
                        TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 0.4f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.3f)),
                        TypeWeights(ZombieType.Bruiser, Weights(RouteId.NorthRoad, 0.5f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.2f)),
                        TypeWeights(ZombieType.Ripper, Weights(RouteId.NorthRoad, 0.15f, RouteId.EastAlley, 0.2f, RouteId.SouthTunnel, 0.65f))
                    };
                }
                else
                {
                    if (number == 4)
                    {
                        item.runnerCount = Range(135, 145);
                        item.bruiserCount = Range(12, 14);
                        item.ripperCount = Range(8, 10);
                        item.learningTotal = Range(155, 169);
                    }
                    else if (number == 5)
                    {
                        item.runnerCount = Range(130, 140);
                        item.bruiserCount = Range(16, 18);
                        item.ripperCount = Range(12, 14);
                        item.learningTotal = Range(158, 172);
                    }
                    else if (number == 6)
                    {
                        item.runnerCount = Range(125, 135);
                        item.bruiserCount = Range(20, 22);
                        item.ripperCount = Range(16, 18);
                        item.learningTotal = Range(161, 175);
                    }
                    else if (number == 7)
                    {
                        item.runnerCount = Range(120, 130);
                        item.bruiserCount = Range(24, 26);
                        item.ripperCount = Range(20, 22);
                        item.learningTotal = Range(164, 178);
                    }
                    else
                    {
                        item.runnerCount = Range(115, 125);
                        item.bruiserCount = Range(28, 30);
                        item.ripperCount = Range(24, 26);
                        item.learningTotal = Range(167, 181);
                    }
                    item.bruiserMinimum = item.bruiserCount.Min;
                    item.ripperMinimum = item.ripperCount.Min;
                    item.groupIntervalSeconds = 3f;
                    item.groupSize = Range(4, 6);
                    item.maxAliveConcurrent = 24;
                    item.routeWeights = LatePhaseWeights(number);
                    item.zombieTypeRouteWeights = new[]
                    {
                        TypeWeights(ZombieType.Runner, LatePhaseWeights(number)),
                        TypeWeights(ZombieType.Bruiser, LatePhaseWeights(number)),
                        TypeWeights(ZombieType.Ripper, LatePhaseWeights(number))
                    };
                }
            });
        }

        private static RouteWeightConfig[] LatePhaseWeights(int number)
        {
            if (number == 5)
                return Weights(RouteId.NorthRoad, 0.25f, RouteId.EastAlley, 0.5f, RouteId.SouthTunnel, 0.25f);
            if (number == 6)
                return Weights(RouteId.NorthRoad, 0.5f, RouteId.EastAlley, 0.25f, RouteId.SouthTunnel, 0.25f);
            if (number == 7)
                return Weights(RouteId.NorthRoad, 0.25f, RouteId.EastAlley, 0.25f, RouteId.SouthTunnel, 0.5f);
            return Weights(RouteId.NorthRoad, 0.34f, RouteId.EastAlley, 0.33f, RouteId.SouthTunnel, 0.33f);
        }

        private static IntRangeConfig Range(int minimum, int maximum)
        {
            return new IntRangeConfig { Min = minimum, Max = maximum };
        }

        private static RouteWeightConfig[] Weights(params object[] values)
        {
            var result = new RouteWeightConfig[values.Length / 2];
            for (var index = 0; index < result.Length; index++)
                result[index] = new RouteWeightConfig
                {
                    Route = (RouteId)values[index * 2],
                    Weight = Convert.ToSingle(values[index * 2 + 1])
                };
            return result;
        }

        private static ZombieRouteWeightConfig TypeWeights(ZombieType type, RouteWeightConfig[] routes)
        {
            return new ZombieRouteWeightConfig { Type = type, Routes = routes };
        }

        private static SimPlayerProfileAsset SimProfile(string name, SimProfileId id, float accuracy, float headshot,
            float reaction, float fireInterval, SimRoutePriorityPolicy routePolicy, float ripperFocus,
            float chargeThreshold, SimGrenadeUsePolicy grenadePolicy,
            int grenadeClusterThreshold, HaetaeSpecializationPair defaultLoadout)
        {
            return Asset<SimPlayerProfileAsset>(name, item =>
            {
                item.id = id;
                item.aimAccuracy = accuracy;
                item.headshotRate = headshot;
                item.reactionDelaySeconds = reaction;
                item.fireIntervalSeconds = fireInterval;
                item.routePriorityPolicy = routePolicy;
                item.ripperFocus = ripperFocus;
                item.robotChargeThresholdFraction = chargeThreshold;
                item.grenadeUsePolicy = grenadePolicy;
                item.grenadeClusterThreshold = grenadeClusterThreshold;
                item.defaultSpecializationLoadout = defaultLoadout;
            });
        }

        private static HaetaeSpecializationDefinitionAsset HaetaeSpecializationAsset(
            string name, HaetaeSpecialization id, string displayNameKey, string descriptionKey,
            float preferredMinRange, float preferredMaxRange, float dashDamageMultiplier,
            float biteDamageMultiplier, float rangedDamage, float rangedCooldownSeconds,
            float cleaveRadius, int maximumTargets, float incomingDamageMultiplier,
            float combatBatteryMultiplier, Color bodyColor, Vector3 scaleMultiplier,
            Color attackPulseColor, Color tracerColor)
        {
            return Asset<HaetaeSpecializationDefinitionAsset>(name, item =>
            {
                item.id = id;
                item.displayNameKey = displayNameKey;
                item.descriptionKey = descriptionKey;
                item.preferredMinRange = preferredMinRange;
                item.preferredMaxRange = preferredMaxRange;
                item.dashDamageMultiplier = dashDamageMultiplier;
                item.biteDamageMultiplier = biteDamageMultiplier;
                item.rangedDamage = rangedDamage;
                item.rangedCooldownSeconds = rangedCooldownSeconds;
                item.cleaveRadius = cleaveRadius;
                item.maximumTargets = maximumTargets;
                item.incomingDamageMultiplier = incomingDamageMultiplier;
                item.combatBatteryMultiplier = combatBatteryMultiplier;
                item.bodyColor = bodyColor;
                item.scaleMultiplier = scaleMultiplier;
                item.attackPulseColor = attackPulseColor;
                item.tracerColor = tracerColor;
            });
        }

        private static StringTableAsset BuildStringTable()
        {
            return Asset<StringTableAsset>("StringTable", item =>
            {
                item.entries = new List<StringEntry>
                {
                    Entry("radio.game_start", "텔레 로봇팀, 출격하라."),
                    Entry("radio.phase1", "감염체 접근. 북쪽 도로 방어 준비."),
                    Entry("radio.phase2", "동쪽 골목에서 추가 접근 신호 감지."),
                    Entry("radio.phase3", "남쪽 터널 개방. 메디컬 로봇 투입."),
                    Entry("radio.phase4", "후반 공세 개시. 세 경로를 사수하라."),
                    Entry("radio.phase5", "동쪽 골목의 적 밀도가 증가한다."),
                    Entry("radio.phase6", "북쪽 도로에 중장 감염체가 집결한다."),
                    Entry("radio.phase7", "남쪽 터널 압박이 거세진다."),
                    Entry("radio.phase8", "최종 공세다. 거점을 끝까지 사수하라."),
                    Entry("radio.battery_warning", "해태 1호, 배터리 위험."),
                    Entry("radio.base_danger", "거점 방어선 붕괴 임박."),
                    Entry("radio.phase_clear", "위협 제거. 재정비 단계 진입."),
                    Entry("radio.victory", "거점 생존 확인. 작전 성공."),
                    Entry("cmd.defend", "거점 사수"), Entry("cmd.patrol", "경로 순찰"),
                    Entry("cmd.return", "기지 복귀"),
                    Entry("route.north", "북쪽 도로"), Entry("route.east", "동쪽 골목"), Entry("route.south", "남쪽 터널"),
                    Entry("hud.base", "거점"), Entry("hud.phase", "페이즈"), Entry("hud.player", "플레이어"),
                    Entry("hud.ammo", "탄약"), Entry("hud.grenade", "수류탄"), Entry("hud.routes", "경로 경보"),
                    Entry("hud.command", "로봇 명령"), Entry("hud.target", "대상 경로"), Entry("hud.upgrade", "업그레이드 선택"),
                    Entry("hud.all_robots", "전체 로봇"),
                    Entry("hud.ripper", "리퍼 출현"), Entry("hud.victory", "작전 성공"), Entry("hud.defeat", "작전 실패"),
                    Entry("hud.pause", "일시정지"), Entry("hud.resume", "계속하기"), Entry("hud.restart", "다시 시작"),
                    Entry("hud.first_person", "1인칭"), Entry("hud.third_person", "3인칭"), Entry("hud.headshot", "헤드샷"),
                    Entry("hud.low_ammo", "탄약 부족"), Entry("hud.resupply", "탄약 보급"),
                    Entry("hud.safe_supply", "안전 보급지"), Entry("hud.risky_supply", "위험 보급지"),
                    Entry("hud.reloading", "재장전 중")
                    ,Entry("hud.haetae_level", "레벨")
                    ,Entry("hud.haetae_experience", "경험치")
                    ,Entry("hud.haetae_health", "체력")
                    ,Entry("hud.haetae_status", "상태")
                    ,Entry("hud.haetae_battery", "배터리")
                    ,Entry("hud.haetae_general", "일반형")
                    ,Entry("hud.haetae_specialization_ready", "전문화 가능")
                    ,Entry("hud.haetae_choose_specialization", "전문화 선택")
                    ,Entry("hud.haetae_specialization_hint", "[B] 해태 빌드")
                    ,Entry("hud.haetae_mastery_points", "강화 포인트")
                    ,Entry("haetae.specialization.melee", "근거리형")
                    ,Entry("haetae.specialization.ranged", "원거리형")
                    ,Entry("haetae.specialization.balanced", "균형형")
                    ,Entry("haetae.specialization.melee.description", "근접 돌입과 다중 타격에 특화. 전투 배터리 소모가 증가합니다.")
                    ,Entry("haetae.specialization.ranged.description", "안전 거리를 유지하며 사격. 받는 피해가 증가합니다.")
                    ,Entry("haetae.specialization.balanced.description", "접근 중 사격하고 근거리에서는 물어뜯기로 전환합니다.")
                    ,Entry("haetae.specialization.panel_title", "해태 전문화")
                    ,Entry("haetae.specialization.next_robot", "다음 해태")
                    ,Entry("haetae.specialization.close", "닫기")
                    ,Entry("haetae.mastery.panel_title", "해태 강화")
                    ,Entry("haetae.mastery.power", "화력 강화")
                    ,Entry("haetae.mastery.power.description", "모든 공격 피해가 등급당 10% 증가합니다.")
                    ,Entry("haetae.mastery.armor", "장갑 강화")
                    ,Entry("haetae.mastery.armor.description", "받는 피해가 등급당 8% 감소합니다. 최대 50%까지 감소합니다.")
                    ,Entry("haetae.mastery.efficiency", "동력 효율")
                    ,Entry("haetae.mastery.efficiency.description", "전투 중 배터리 소모가 등급당 8% 감소합니다. 최대 50%까지 감소합니다.")
                    ,Entry("haetae.mastery.attack_speed", "공격 속도")
                    ,Entry("haetae.mastery.attack_speed.description", "모든 해태 공격 간격이 등급당 10% 감소합니다. 최대 50%까지 감소합니다.")
                    ,Entry("menu.title", "텔레 로봇팀, 출격하라")
                    ,Entry("menu.subtitle", "세 경로를 방어하고 해태 로봇팀을 지휘하십시오")
                    ,Entry("menu.play", "게임 시작"), Entry("menu.settings", "설정"), Entry("menu.quit", "게임 종료")
                    ,Entry("menu.main", "시작 화면으로"), Entry("menu.controls_hint", "WASD 이동 · 마우스 조준 · V 시점 전환 · Space 점프")
                    ,Entry("settings.title", "설정"), Entry("settings.sensitivity", "마우스 감도")
                    ,Entry("settings.master_volume", "전체 음량"), Entry("settings.effects_volume", "효과음 음량")
                    ,Entry("settings.resolution", "해상도"), Entry("settings.fullscreen", "전체 화면")
                    ,Entry("settings.default_perspective", "기본 시점"), Entry("settings.apply", "저장하고 적용")
                    ,Entry("settings.cancel", "취소"), Entry("settings.on", "켜기"), Entry("settings.off", "끄기")
                };
            });
        }

        private static StringEntry Entry(string key, string value)
        {
            return new StringEntry { key = key, value = value };
        }

        private static void BuildScenes(MvpContentCatalog catalog)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("MVP Game Controller");
            var controller = root.AddComponent<MvpGameController>();
            controller.SetCatalog(catalog);
            controller.SetInputActions(AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
            EditorSceneManager.SaveScene(scene, MvpScenePath);

            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var menuRoot = new GameObject("Main Menu");
            var menuController = menuRoot.AddComponent<MainMenuController>();
            menuController.SetCatalog(catalog);
            var cameraObject = new GameObject("Main Menu Camera");
            var menuCamera = cameraObject.AddComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.015f, 0.035f, 0.065f);
            menuCamera.cullingMask = 0;
            EditorSceneManager.SaveScene(menuScene, MainMenuScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(MvpScenePath, true)
            };
        }

        private static T Asset<T>(string name, Action<T> configure) where T : ScriptableObject
        {
            var path = DataRoot + "/" + name + ".asset";
            var result = AssetDatabase.LoadAssetAtPath<T>(path);
            if (result == null)
            {
                result = ScriptableObject.CreateInstance<T>();
                result.name = name;
                AssetDatabase.CreateAsset(result, path);
            }
            configure(result);
            EditorUtility.SetDirty(result);
            return result;
        }

        private static Material MaterialAsset(string name)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Universal Render Pipeline/Lit shader is unavailable in the editor.");

            var path = DataRoot + "/" + name + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null)
            {
                result = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(result, path);
            }
            else if (result.shader != shader)
            {
                result.shader = shader;
            }

            result.color = Color.white;
            result.enableInstancing = true;
            EditorUtility.SetDirty(result);
            return result;
        }

        private static VisualThemeDefinitionAsset BuildVisualTheme()
        {
            var colors = new[]
            {
                ColorEntry("world.ground", new Color32(17, 24, 33, 255)),
                ColorEntry("world.structure", new Color32(37, 48, 59, 255)),
                ColorEntry("world.trim", new Color32(89, 103, 117, 255)),
                ColorEntry("ally.frame", new Color32(18, 28, 48, 255)),
                ColorEntry("ally.ceramic", new Color32(211, 202, 176, 255)),
                ColorEntry("ally.joint", new Color32(12, 15, 22, 255)),
                ColorEntry("ally.energy", new Color32(33, 212, 242, 255)),
                ColorEntry("ally.haetae", new Color32(231, 167, 43, 255)),
                ColorEntry("ally.unit2", new Color32(255, 110, 45, 255)),
                ColorEntry("ally.medical", new Color32(56, 226, 160, 255)),
                ColorEntry("enemy.body", new Color32(79, 88, 93, 255)),
                ColorEntry("enemy.corruption", new Color32(227, 54, 54, 255)),
                ColorEntry("enemy.ripper", new Color32(239, 63, 158, 255)),
                ColorEntry("route.north", new Color32(56, 147, 242, 255)),
                ColorEntry("route.east", new Color32(242, 167, 43, 255)),
                ColorEntry("route.south", new Color32(166, 80, 214, 255)),
                ColorEntry("state.safe", new Color32(63, 214, 116, 255)),
                ColorEntry("state.caution", new Color32(242, 167, 43, 255)),
                ColorEntry("state.danger", new Color32(242, 60, 60, 255)),
                ColorEntry("ui.panel", new Color32(8, 18, 28, 235)),
                ColorEntry("ui.line", new Color32(72, 218, 240, 255)),
                ColorEntry("ui.text", new Color32(225, 239, 245, 255)),
                ColorEntry("ui.muted", new Color32(137, 159, 171, 255))
            };

            var byKey = colors.ToDictionary(item => item.key, item => item.value, StringComparer.Ordinal);
            var materials = new[]
            {
                MaterialRole("world.ground", byKey["world.ground"], 0.05f, 0.25f),
                MaterialRole("world.structure", byKey["world.structure"], 0.35f, 0.32f),
                MaterialRole("world.trim", byKey["world.trim"], 0.55f, 0.42f),
                MaterialRole("ally.armor", new Color32(48, 61, 72, 255), 0.7f, 0.5f),
                MaterialRole("ally.frame", byKey["ally.frame"], 0.82f, 0.58f),
                MaterialRole("ally.ceramic", byKey["ally.ceramic"], 0.52f, 0.4f),
                MaterialRole("ally.joint", byKey["ally.joint"], 0.72f, 0.34f),
                MaterialRole("ally.energy", byKey["ally.energy"], 0.2f, 0.55f, 2.4f),
                MaterialRole("ally.haetae", byKey["ally.haetae"], 0.75f, 0.48f, 0.35f),
                MaterialRole("ally.unit2", byKey["ally.unit2"], 0.35f, 0.48f, 1.1f),
                MaterialRole("ally.medical", byKey["ally.medical"], 0.2f, 0.45f, 1.8f),
                MaterialRole("enemy.body", byKey["enemy.body"], 0.15f, 0.28f),
                MaterialRole("enemy.armor", new Color32(54, 43, 44, 255), 0.55f, 0.32f),
                MaterialRole("enemy.corruption", byKey["enemy.corruption"], 0.15f, 0.35f, 1.45f),
                MaterialRole("enemy.ripper", byKey["enemy.ripper"], 0.18f, 0.42f, 2.1f),
                MaterialRole("state.safe", byKey["state.safe"], 0.15f, 0.4f, 1.25f),
                MaterialRole("state.caution", byKey["state.caution"], 0.15f, 0.4f, 1.25f),
                MaterialRole("state.danger", byKey["state.danger"], 0.15f, 0.4f, 1.6f),
                MaterialRole("ui.panel", byKey["ui.panel"], 0f, 0.25f)
            };

            return Asset<VisualThemeDefinitionAsset>("VisualTheme", item =>
            {
                item.themeId = "guardian-night-v1";
                item.colors = colors;
                item.materials = materials;
                item.menuBackdrop = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Game/Art/Menu/guardian-night-menu.png");
                item.bodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Art/Fonts/NotoSansKR-VF.ttf");
                item.headingFont = item.bodyFont;
                item.haetaeGeneralModel =
                    AssetDatabase.LoadAssetAtPath<GameObject>(HaetaeGeneralLod0Path);
                item.haetaeGeneralLod1 =
                    AssetDatabase.LoadAssetAtPath<GameObject>(HaetaeGeneralLod1Path);
                item.haetaeUpgradeModels = new[]
                {
                    AuthoredHaetaeModel(
                        PresentationRole.HaetaeMeleePreview,
                        "character.haetae.melee",
                        "haetae.authored.melee.ram",
                        HaetaeMeleeLod0Path,
                        HaetaeMeleeLod1Path),
                    AuthoredHaetaeModel(
                        PresentationRole.HaetaeRangedPreview,
                        "character.haetae.ranged",
                        "haetae.authored.ranged.turret",
                        HaetaeRangedLod0Path,
                        HaetaeRangedLod1Path),
                    AuthoredHaetaeModel(
                        PresentationRole.HaetaeBalancedPreview,
                        "character.haetae.balanced",
                        "haetae.authored.balanced.asymmetric",
                        HaetaeBalancedLod0Path,
                        HaetaeBalancedLod1Path)
                };
                item.authoredZombieModels = new[]
                {
                    AuthoredZombieModel(
                        PresentationRole.Runner,
                        "enemy.runner",
                        "zombie.authored.runner.pursuit",
                        ZombieRunnerLod0Path,
                        ZombieRunnerLod1Path),
                    AuthoredZombieModel(
                        PresentationRole.Bruiser,
                        "enemy.bruiser",
                        "zombie.authored.bruiser.siege",
                        ZombieBruiserLod0Path,
                        ZombieBruiserLod1Path),
                    AuthoredZombieModel(
                        PresentationRole.Ripper,
                        "enemy.ripper",
                        "zombie.authored.ripper.scythe",
                        ZombieRipperLod0Path,
                        ZombieRipperLod1Path)
                };
                item.characterMotionProfiles = new[]
                {
                    MotionProfile(PresentationRole.HaetaeGeneralUnit1, "haetae.general.unit1",
                        0.78f, 0.018f, 0.045f, 2.4f, -2f, 17f, 30f, 0.1f, 11f, 62f, 0.34f, 0.15f),
                    MotionProfile(PresentationRole.HaetaeGeneralUnit2, "haetae.general.unit2",
                        0.82f, 0.02f, 0.048f, 2.8f, -2f, 18f, 32f, 0.11f, 12f, 64f, 0.34f, 0.15f),
                    MotionProfile(PresentationRole.HaetaeMeleePreview, "haetae.melee",
                        0.9f, 0.016f, 0.06f, 3.2f, 4f, 22f, 58f, 0.22f, 14f, 72f, 0.3f, 0.14f),
                    MotionProfile(PresentationRole.HaetaeRangedPreview, "haetae.ranged",
                        0.72f, 0.012f, 0.038f, 1.8f, -4f, 14f, 38f, 0.2f, 10f, 58f, 0.38f, 0.16f),
                    MotionProfile(PresentationRole.HaetaeBalancedPreview, "haetae.balanced",
                        0.84f, 0.016f, 0.05f, 2.6f, 1f, 19f, 45f, 0.16f, 12f, 66f, 0.34f, 0.15f),
                    MotionProfile(PresentationRole.Runner, "zombie.runner",
                        2.35f, 0.022f, 0.1f, 7f, 18f, 46f, 56f, 0.2f, 18f, 86f, 0.26f, 0.12f),
                    MotionProfile(PresentationRole.Bruiser, "zombie.bruiser",
                        1.08f, 0.035f, 0.085f, 5f, 5f, 25f, 48f, 0.16f, 15f, 78f, 0.42f, 0.18f),
                    MotionProfile(PresentationRole.Ripper, "zombie.ripper",
                        1.62f, 0.025f, 0.09f, 8f, 12f, 37f, 72f, 0.25f, 21f, 96f, 0.32f, 0.13f)
                };
                item.effects = new[]
                {
                    Effect("combat.hit", "enemy.corruption", 0.12f, 0.22f, 32),
                    Effect("combat.headshot", "state.caution", 0.18f, 0.28f, 16),
                    Effect("combat.explosion", "state.caution", 0.35f, 5f, 8),
                    Effect("robot.attack", "ally.energy", 0.12f, 0.42f, 24),
                    Effect("robot.heal", "ally.medical", 0.3f, 1f, 8),
                    Effect("robot.level", "ally.haetae", 0.8f, 1.6f, 4),
                    Effect("enemy.death", "enemy.corruption", 0.42f, 0.9f, 32)
                };
            });
        }

        private static DesignAssetCatalogAsset BuildDesignAssetCatalog(VisualThemeDefinitionAsset theme)
        {
            return Asset<DesignAssetCatalogAsset>("DesignAssetCatalog", item =>
            {
                item.catalogVersion = "design-assets-1.0.0";
                var entries = new List<DesignAssetItemDefinition>();
                foreach (var id in DesignAssetCatalogAsset.RequiredAssetIds)
                {
                    var isFont = id.StartsWith("font.", StringComparison.Ordinal);
                    var adoptedFont = isFont && theme.bodyFont != null;
                    var implementedAnimation = id == "animation.zombie" || id == "animation.haetae";
                    var deferred = (id.StartsWith("animation.", StringComparison.Ordinal) && !implementedAnimation) ||
                                   id.StartsWith("audio.", StringComparison.Ordinal) ||
                                   (isFont && !adoptedFont);
                    var fallback = id.StartsWith("animation.", StringComparison.Ordinal) && !implementedAnimation
                        ? "fallback.animation.transform" :
                        id.StartsWith("audio.", StringComparison.Ordinal) ? "fallback.audio.procedural" :
                        id.StartsWith("font.", StringComparison.Ordinal) ? "fallback.font.system" : null;
                    entries.Add(new DesignAssetItemDefinition
                    {
                        id = id,
                        displayName = id,
                        category = CategoryFor(id),
                        priority = id.StartsWith("ui.", StringComparison.Ordinal) ? DesignAssetPriority.P2 : DesignAssetPriority.P1,
                        decision = adoptedFont ? DesignAssetDecision.Adopt :
                            deferred ? DesignAssetDecision.Defer : DesignAssetDecision.Make,
                        status = deferred ? DesignAssetStatus.Deferred : DesignAssetStatus.Integrated,
                        usageRoles = new[] { id },
                        assetReferences = adoptedFont ? new UnityEngine.Object[] { theme.bodyFont } : Array.Empty<UnityEngine.Object>(),
                        generatedRecipe = deferred || adoptedFont ? null : RecipeFor(id),
                        sourceId = adoptedFont ? "google-noto-sans-kr" : null,
                        fallbackId = fallback,
                        validationTags = new[] { deferred ? "catalog" : "automated", "manual-visual" },
                        notes = id.Contains("haetae.melee") || id.Contains("haetae.ranged") || id.Contains("haetae.balanced")
                            ? "Authored upgrade model is mapped live with a role-local procedural fallback."
                            : id == "enemy.runner" || id == "enemy.bruiser" || id == "enemy.ripper"
                            ? "Authored zombie model is mapped live with a role-local procedural fallback."
                            : string.Empty
                    });
                }
                entries.Add(Fallback("fallback.animation.transform", DesignAssetCategory.Animation,
                    "Transform movement remains playable without skeletal clips."));
                entries.Add(Fallback("fallback.audio.procedural", DesignAssetCategory.Audio,
                    "Procedural runtime tones remain playable."));
                entries.Add(Fallback("fallback.font.system", DesignAssetCategory.Font,
                    "Verified operating-system Korean font chain."));

                item.items = entries.ToArray();
                item.sources = CandidateSources();
                item.fallbackTheme = theme;
            });
        }

        private static SemanticColorDefinition ColorEntry(string key, Color value)
        {
            return new SemanticColorDefinition { key = key, value = value };
        }

        private static MaterialRoleDefinition MaterialRole(string key, Color color, float metallic, float smoothness,
            float emission = 0f)
        {
            return new MaterialRoleDefinition
            {
                key = key,
                baseColor = color,
                metallic = metallic,
                smoothness = smoothness,
                emissionColor = color,
                emissionIntensity = emission,
                material = VisualMaterialAsset(key.Replace('.', '-'), color, metallic, smoothness, emission)
            };
        }

        private static AuthoredHaetaeModelDefinition AuthoredHaetaeModel(
            PresentationRole role,
            string assetId,
            string silhouetteSignature,
            string lod0Path,
            string lod1Path)
        {
            return new AuthoredHaetaeModelDefinition
            {
                role = role,
                assetId = assetId,
                silhouetteSignature = silhouetteSignature,
                lod0 = AssetDatabase.LoadAssetAtPath<GameObject>(lod0Path),
                lod1 = AssetDatabase.LoadAssetAtPath<GameObject>(lod1Path)
            };
        }

        private static AuthoredZombieModelDefinition AuthoredZombieModel(
            PresentationRole role,
            string assetId,
            string silhouetteSignature,
            string lod0Path,
            string lod1Path)
        {
            return new AuthoredZombieModelDefinition
            {
                role = role,
                assetId = assetId,
                silhouetteSignature = silhouetteSignature,
                lod0 = AssetDatabase.LoadAssetAtPath<GameObject>(lod0Path),
                lod1 = AssetDatabase.LoadAssetAtPath<GameObject>(lod1Path)
            };
        }

        private static CharacterMotionProfileDefinition MotionProfile(
            PresentationRole role, string id, float cycleHz, float idleBob, float locomotionBob,
            float swayDegrees, float forwardLeanDegrees, float strideDegrees, float attackDegrees,
            float attackRecoil, float hitDegrees, float deathDegrees, float attackDuration,
            float hitDuration)
        {
            return new CharacterMotionProfileDefinition
            {
                role = role,
                profileId = id,
                cycleHz = cycleHz,
                idleBob = idleBob,
                locomotionBob = locomotionBob,
                swayDegrees = swayDegrees,
                forwardLeanDegrees = forwardLeanDegrees,
                strideDegrees = strideDegrees,
                attackDegrees = attackDegrees,
                attackRecoil = attackRecoil,
                hitDegrees = hitDegrees,
                deathDegrees = deathDegrees,
                attackDuration = attackDuration,
                hitDuration = hitDuration
            };
        }

        private static EffectStyleDefinition Effect(string key, string colorKey, float duration, float size, int maximum)
        {
            return new EffectStyleDefinition
            {
                key = key,
                colorKey = colorKey,
                duration = duration,
                size = size,
                maximumConcurrent = maximum
            };
        }

        private static Material VisualMaterialAsset(string name, Color color, float metallic, float smoothness, float emission)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Universal Render Pipeline/Lit shader is unavailable.");
            var path = "Assets/Game/Art/Materials/" + name + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null)
            {
                result = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(result, path);
            }
            result.shader = shader;
            result.color = color;
            if (result.HasProperty("_BaseColor")) result.SetColor("_BaseColor", color);
            if (result.HasProperty("_Metallic")) result.SetFloat("_Metallic", metallic);
            if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", smoothness);
            if (emission > 0f && result.HasProperty("_EmissionColor"))
            {
                result.EnableKeyword("_EMISSION");
                result.SetColor("_EmissionColor", color * emission);
            }
            else result.DisableKeyword("_EMISSION");
            result.enableInstancing = true;
            EditorUtility.SetDirty(result);
            return result;
        }

        private static DesignAssetCategory CategoryFor(string id)
        {
            if (id.StartsWith("character.", StringComparison.Ordinal)) return DesignAssetCategory.Character;
            if (id.StartsWith("enemy.", StringComparison.Ordinal)) return DesignAssetCategory.Enemy;
            if (id.StartsWith("environment.", StringComparison.Ordinal) || id.StartsWith("interactable.", StringComparison.Ordinal))
                return DesignAssetCategory.Environment;
            if (id.StartsWith("ui.", StringComparison.Ordinal)) return DesignAssetCategory.UI;
            if (id.StartsWith("vfx.", StringComparison.Ordinal)) return DesignAssetCategory.VFX;
            if (id.StartsWith("animation.", StringComparison.Ordinal)) return DesignAssetCategory.Animation;
            if (id.StartsWith("audio.", StringComparison.Ordinal)) return DesignAssetCategory.Audio;
            if (id.StartsWith("font.", StringComparison.Ordinal)) return DesignAssetCategory.Font;
            return DesignAssetCategory.Environment;
        }

        private static string RecipeFor(string id)
        {
            if (id == "character.haetae.unit-1" || id == "character.haetae.unit-2")
                return "ArtSource/Haetae/create_haetae_general.py";
            if (id == "enemy.runner" || id == "enemy.bruiser" || id == "enemy.ripper")
                return "ArtSource/Zombies/create_zombie_models.py";
            if (id.StartsWith("ui.icon.", StringComparison.Ordinal))
                return "Assets/Game/Runtime/Presentation/RuntimeIconLibrary.cs";
            if (id.StartsWith("ui.", StringComparison.Ordinal))
                return "Assets/Game/Runtime/HUD";
            if (id.StartsWith("vfx.", StringComparison.Ordinal))
                return "Assets/Game/Runtime/Presentation/VisualEffectFactory.cs";
            if (id == "animation.zombie" || id == "animation.haetae")
                return "Assets/Game/Runtime/Presentation/CharacterMotionDriver.cs";
            if (id.StartsWith("environment.", StringComparison.Ordinal) || id.StartsWith("interactable.", StringComparison.Ordinal))
                return "Assets/Game/Runtime/Presentation/WorldArtBuilder.cs";
            return "Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs";
        }

        private static DesignAssetItemDefinition Fallback(string id, DesignAssetCategory category, string notes)
        {
            return new DesignAssetItemDefinition
            {
                id = id,
                displayName = id,
                category = category,
                priority = DesignAssetPriority.P1,
                decision = DesignAssetDecision.Make,
                status = DesignAssetStatus.Integrated,
                usageRoles = new[] { "runtime-fallback" },
                generatedRecipe = notes,
                validationTags = new[] { "automated", "fallback" },
                notes = notes
            };
        }

        private static DesignAssetSourceRecord[] CandidateSources()
        {
            var sources = new[]
            {
                Source("kenney-ui-scifi", "UI Pack - Sci-Fi", "Kenney",
                    "https://kenney.nl/assets/ui-pack-sci-fi", "CC0-1.0"),
                Source("kenney-city-roads", "City Kit (Roads)", "Kenney",
                    "https://kenney.nl/assets/city-kit-roads", "CC0-1.0"),
                Source("kenney-scifi-sounds", "Sci-fi Sounds", "Kenney",
                    "https://kenney.nl/assets/sci-fi-sounds", "CC0-1.0"),
                Source("kenney-digital-audio", "Digital Audio", "Kenney",
                    "https://www.kenney.nl/assets/digital-audio", "CC0-1.0"),
                Source("quaternius-scifi-megakit", "Modular Sci-Fi Megakit", "Quaternius",
                    "https://quaternius.com/packs/modularscifimegakit.html", "CC0-1.0"),
                Source("quaternius-zombie-apocalypse", "Zombie Apocalypse Kit", "Quaternius",
                    "https://quaternius.com/packs/zombieapocalypsekit.html", "CC0-1.0"),
                Source("quaternius-animation-library", "Universal Animation Library", "Quaternius",
                    "https://quaternius.com/packs/universalanimationlibrary.html", "CC0-1.0"),
                Source("adobe-mixamo", "Mixamo", "Adobe",
                    "https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html", "ROYALTY-FREE-SERVICE"),
                Source("google-noto-sans-kr", "Noto Sans KR", "Google and Adobe",
                    "https://notofonts.github.io/noto-docs/website/use/", "OFL-1.1")
            };
            sources[sources.Length - 1].originalFiles = new[] { "NotoSansKR-VF.ttf" };
            sources[sources.Length - 1].modifications = new[] { "Copied from the locally installed official Noto Sans KR family; no glyph modification." };
            sources[sources.Length - 1].noticeText = "Noto Sans KR is licensed under the SIL Open Font License 1.1.";
            return sources;
        }

        private static DesignAssetSourceRecord Source(string id, string title, string creator, string url, string license)
        {
            return new DesignAssetSourceRecord
            {
                id = id,
                title = title,
                creator = creator,
                officialUrl = url,
                licenseId = license,
                licenseEvidence = url,
                retrievedOn = "2026-07-26",
                originalFiles = Array.Empty<string>(),
                modifications = Array.Empty<string>(),
                attributionRequired = license == "OFL-1.1",
                noticeText = "Candidate only; no runtime file imported.",
                redistributionNotes = "Review exact downloaded files before adoption."
            };
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }
}
