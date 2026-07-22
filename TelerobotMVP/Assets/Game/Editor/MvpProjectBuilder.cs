using System;
using System.Collections.Generic;
using System.IO;
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

        [InitializeOnLoadMethod]
        private static void ScheduleFirstImportBuild()
        {
            if (File.Exists(MainMenuScenePath) && File.Exists(MvpScenePath)) return;
            EditorApplication.delayCall += () =>
            {
                if ((!File.Exists(MainMenuScenePath) || !File.Exists(MvpScenePath)) && !EditorApplication.isCompiling) BuildAll();
            };
        }

        [MenuItem("Tools/Telerobot/Build MVP Project")]
        public static void BuildAll()
        {
            EnsureFolder("Assets/Game/Data/Assets");
            EnsureFolder("Assets/Game/Scenes");

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
                    "upgrade_selected", "route_pressure_sampled", "simulation_run_completed", "base_hp_sampled",
                    "player_hp_at_phase_end", "grenade_used", "ammo_resupplied", "barrier_damaged", "barrier_destroyed",
                    "medical_heal_applied", "medical_robot_destroyed", "robot_damaged", "robot_destroyed", "base_warning", "battery_warning",
                    "radio_event", "ripper_spawned", "route_opened", "camera_perspective_changed",
                    "player_jumped", "player_hit_confirmed", "game_paused", "session_restarted",
                    "returned_to_main_menu"
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
                SimRoutePriorityPolicy.LateReactive, 0.2f, 0.10f, SimUpgradeSelectionPolicy.RandomOfThree,
                SimGrenadeUsePolicy.Rarely, 8);
            var baselineProfile = SimProfile("SimPlayerBaseline", SimProfileId.Baseline, 0.75f, 0.25f, 0.6f, 1.8f,
                SimRoutePriorityPolicy.BalancedCoverage, 0.6f, 0.25f, SimUpgradeSelectionPolicy.IntendedMeta,
                SimGrenadeUsePolicy.DenseClusters, 4);
            var skilledProfile = SimProfile("SimPlayerSkilled", SimProfileId.Skilled, 0.92f, 0.45f, 0.25f, 1.0f,
                SimRoutePriorityPolicy.HighestPressure, 1f, 0.40f, SimUpgradeSelectionPolicy.RiskAwareOptimal,
                SimGrenadeUsePolicy.DenseClustersAndBruisers, 3);

            var runner = Zombie("Runner", ZombieType.Runner, 90f, 6.5f, 8f, 12f, 8f, 1f, 1, 1,
                new[] { TargetKind.Base, TargetKind.Player, TargetKind.Robot }, new Color(0.38f, 0.9f, 0.28f), new Vector3(0.72f, 0.86f, 0.72f));
            var bruiser = Zombie("Bruiser", ZombieType.Bruiser, 500f, 2.6f, 60f, 30f, 25f, 2f, 5, 2,
                new[] { TargetKind.Base, TargetKind.Robot, TargetKind.Player }, new Color(0.52f, 0.24f, 0.12f), new Vector3(1.45f, 1.4f, 1.45f));
            var ripper = Zombie("Ripper", ZombieType.Ripper, 180f, 7.2f, 10f, 18f, 18f, 0.9f, 4, 3,
                new[] { TargetKind.Robot, TargetKind.Player, TargetKind.Base }, new Color(0.92f, 0.05f, 0.45f), new Vector3(0.9f, 1.05f, 0.9f));

            var north = Route("NorthRoad", RouteId.NorthRoad, 1, "route.north", new Color(0.15f, 0.55f, 0.95f), 9f,
                new[] { new Vector3(0f, 1f, 50f), new Vector3(0f, 1f, 34f), new Vector3(0f, 1f, 20f), new Vector3(0f, 1f, 10f), new Vector3(0f, 1f, 4.5f) });
            var east = Route("EastAlley", RouteId.EastAlley, 2, "route.east", new Color(0.95f, 0.62f, 0.12f), 5f,
                new[] { new Vector3(40f, 1f, 13f), new Vector3(27f, 1f, 13f), new Vector3(16f, 1f, 9f), new Vector3(8f, 1f, 6f), new Vector3(4.5f, 1f, 2.5f) });
            var south = Route("SouthTunnel", RouteId.SouthTunnel, 3, "route.south", new Color(0.65f, 0.18f, 0.75f), 6f,
                new[] { new Vector3(-40f, 1f, -10f), new Vector3(-29f, 1f, -8f), new Vector3(-19f, 1f, -4f), new Vector3(-10f, 1f, 1f), new Vector3(-4.5f, 1f, 2.5f) });

            var phase1 = Phase("Phase1", 1, 40, 150f, new[] { RouteId.NorthRoad });
            var phase2 = Phase("Phase2", 2, 60, 210f, new[] { RouteId.NorthRoad, RouteId.EastAlley });
            var phase3 = Phase("Phase3", 3, 80, 270f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel });

            var upgrades = new[]
            {
                Upgrade("HighEfficiencyBattery", "high_efficiency_battery", "upg.battery", UpgradeEffectType.MaxBattery, 20f),
                Upgrade("CombatPowerSave", "combat_power_save", "upg.powersave", UpgradeEffectType.CombatDrainMultiplier, 0.8f),
                Upgrade("HaetaeChargeBoost", "haetae_charge_boost", "upg.dash", UpgradeEffectType.FirstDashDamageMultiplier, 1.4f),
                Upgrade("ChargeStationSpeedup", "charge_station_speedup", "upg.chargefast", UpgradeEffectType.ChargeRateMultiplier, 1.3f),
                Upgrade("BaseArmor", "base_armor", "upg.armor", UpgradeEffectType.BaseMaxHealth, 200f),
                Upgrade("EmergencyBarrier", "emergency_barrier", "upg.barrier", UpgradeEffectType.EmergencyBarrier, 1f),
                Upgrade("PiercingRounds", "piercing_rounds", "upg.pierce", UpgradeEffectType.PiercingRounds, 1f),
                Upgrade("ExtendedMagazine", "extended_magazine", "upg.mag", UpgradeEffectType.MagazineCapacity, 15f),
                Upgrade("EmergencyRecoveryProtocol", "emergency_recovery_protocol", "upg.recovery", UpgradeEffectType.MedicalHealMultiplier, 1.3f)
            };
            var strings = BuildStringTable();
            var runtimeMaterial = MaterialAsset("RuntimeGreyboxMaterial");

            var catalog = Asset<MvpContentCatalog>("MvpBalanceCatalog", item =>
            {
                item.dataVersion = "mvp-1.4.5";
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
                item.simPlayerProfiles = new[] { noviceProfile, baselineProfile, skilledProfile };
                item.supplyPoints = new[] { safeSupply, riskySupply };
                item.zombies = new[] { runner, bruiser, ripper };
                item.phases = new[] { phase1, phase2, phase3 };
                item.routes = new[] { north, east, south };
                item.upgrades = upgrades;
                item.strings = strings;
                item.runtimeMaterialTemplate = runtimeMaterial;
            });

            MvpDataMapper.Validate(catalog);
            BuildScenes(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlayerSettings.companyName = "Telerobot Team";
            PlayerSettings.productName = "TelerobotMVP";
            PlayerSettings.bundleVersion = "0.2.2";
            Debug.Log("Telerobot MVP assets and scenes built successfully: " + MainMenuScenePath + ", " + MvpScenePath);
        }

        private static ZombieDefinitionAsset Zombie(string name, ZombieType type, float hp, float speed, float baseDamage,
            float playerDamage, float robotDamage, float attackInterval, int cost, int firstPhase, TargetKind[] priority,
            Color color, Vector3 scale)
        {
            return Asset<ZombieDefinitionAsset>(name, item =>
            {
                item.type = type;
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
                else
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
            });
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

        private static UpgradeDefinitionAsset Upgrade(string name, string id, string key, UpgradeEffectType type, float amount)
        {
            return Asset<UpgradeDefinitionAsset>(name, item =>
            {
                item.id = id;
                item.displayNameKey = key;
                item.effectType = type;
                item.amount = amount;
            });
        }

        private static SimPlayerProfileAsset SimProfile(string name, SimProfileId id, float accuracy, float headshot,
            float reaction, float fireInterval, SimRoutePriorityPolicy routePolicy, float ripperFocus,
            float chargeThreshold, SimUpgradeSelectionPolicy upgradePolicy, SimGrenadeUsePolicy grenadePolicy,
            int grenadeClusterThreshold)
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
                item.upgradeSelectionPolicy = upgradePolicy;
                item.grenadeUsePolicy = grenadePolicy;
                item.grenadeClusterThreshold = grenadeClusterThreshold;
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
                    Entry("radio.battery_warning", "해태 1호, 배터리 위험."),
                    Entry("radio.base_danger", "거점 방어선 붕괴 임박."),
                    Entry("radio.phase_clear", "위협 제거. 재정비 단계 진입."),
                    Entry("radio.victory", "거점 생존 확인. 작전 성공."),
                    Entry("cmd.defend", "거점 사수"), Entry("cmd.patrol", "경로 순찰"),
                    Entry("cmd.return", "기지 복귀"),
                    Entry("route.north", "북쪽 도로"), Entry("route.east", "동쪽 골목"), Entry("route.south", "남쪽 터널"),
                    Entry("upg.battery", "고효율 배터리"), Entry("upg.powersave", "전투 절전 모드"),
                    Entry("upg.dash", "해태 돌진 강화"), Entry("upg.chargefast", "충전소 고속화"),
                    Entry("upg.armor", "거점 장갑 보강"), Entry("upg.barrier", "긴급 방벽"),
                    Entry("upg.pierce", "관통탄"), Entry("upg.mag", "확장 탄창"), Entry("upg.recovery", "응급 회복 프로토콜"),
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
