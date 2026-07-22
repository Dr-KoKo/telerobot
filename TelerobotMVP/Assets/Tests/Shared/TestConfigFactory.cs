using System.Collections.Generic;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public static class TestConfigFactory
    {
        public static GameplayConfig Create()
        {
            var config = new GameplayConfig
            {
                Game = new GameRulesConfig
                {
                    PlayerMaxHealth = 100f, TargetSessionMinimumSeconds = 600f, TargetSessionMaximumSeconds = 900f,
                    FixedStepSeconds = 1f / 60f, PlayerMoveSpeed = 8f,
                    SprintMultiplier = 1.5f, Gravity = 24f, MouseSensitivity = 0.12f, CameraDistance = 6f,
                    ThirdPersonFieldOfView = 65f, FirstPersonFieldOfView = 75f,
                    FirstPersonEyeHeight = 0.65f, CameraCollisionRadius = 0.22f,
                    CameraCollisionPadding = 0.08f, JumpHeight = 1.35f, GroundedVelocity = -2f,
                    DataVersion = "test-v1"
                },
                Base = new BaseConfig { MaxHealth = 1000f, PhaseRecoveryFraction = 0.15f, WarningFraction = 0.30f },
                Ammo = new AmmoConfig
                {
                    StartReserveAmmo = 120, ReserveAmmoMax = 240, ResupplyPolicy = ResupplyPolicy.FullReserve,
                    ResupplyUseSeconds = 1.5f, GrenadeResupplyPolicy = GrenadeResupplyPolicy.PhaseResetOnly
                },
                Weapon = new WeaponConfig
                {
                    BaseDamage = 30f, HeadshotMultiplier = 2.5f, MagazineSize = 30,
                    ReloadSeconds = 2f, FireIntervalSeconds = 0.12f, GrenadesPerPhase = 2, Range = 200f
                },
                Grenade = new GrenadeConfig
                {
                    Radius = 5f, InnerRadius = 2f, CenterDamage = 150f, EdgeDamage = 60f,
                    MaxTargets = 10, ThrowDistance = 8f
                },
                Battery = new BatteryConfig
                {
                    Maximum = 100f, LowPowerMaximum = 30f, CriticalMaximum = 10f,
                    IdleDrainPerSecond = 0.3f, PatrolDrainPerSecond = 0.8f, CombatDrainPerSecond = 2.5f,
                    RipperHitDrain = 5f, ChargePerSecond = 4f, LowPowerMoveMultiplier = 0.85f,
                    LowPowerAttackMultiplier = 0.9f, DisabledHoldSeconds = 5f, RecoveryPerSecond = 0.5f,
                    MoveEnableThreshold = 5f, YellowWarningFraction = 0.25f, RedWarningFraction = 0.10f
                },
                Robot = new RobotConfig
                {
                    MaxHealth = 300f, MoveSpeed = 10f, DashDamage = 60f, BiteDamage = 40f,
                    BiteCooldownSeconds = 0.6f, DashCooldownSeconds = 3f, DetectionRadius = 15f, EngageRange = 2f,
                    SeparationRadius = 2.2f, SeparationStrength = 1.8f, FormationSpacing = 3f, DefendLeashRadius = 14f,
                    RunnerKillTargetMinimumSeconds = 1f, RunnerKillTargetMaximumSeconds = 2f,
                    BruiserKillTargetMinimumSeconds = 6f, BruiserKillTargetMaximumSeconds = 10f
                },
                Medical = new MedicalConfig { MaxHealth = 150f, HealPerSecond = 8f, Radius = 6f },
                Barrier = new BarrierConfig { MaxHealth = 300f },
                Warnings = new WarningConfig { BatteryYellowFraction = 0.25f, BatteryRedFraction = 0.10f },
                World = new WorldLayoutConfig
                {
                    BasePosition = new Float3(0, 0, 0), PlayerStart = new Float3(0, 1, -7),
                    RobotStarts = new[] { new Float3(-2.4f, 0.8f, -3.8f), new Float3(2.4f, 0.8f, -3.8f) },
                    BaseRally = new Float3(0, 0.8f, -4), ChargingStation = new Float3(4.5f, 0.5f, -4.5f),
                    SafeSupply = new Float3(-3, 0.5f, -2), RiskySupply = new Float3(0, 0.5f, 18),
                    MedicalAnchor = new Float3(-4.5f, 0.8f, -4.5f), SupplyInteractionRadius = 2.5f,
                    SupplyExitTolerance = 0.75f, BaseChargingRadius = 6f,
                    ChargingArrivalRadius = 1.2f
                },
                Commands = new CommandConfig { Commands = (RobotCommand[])RobotCommandSystem.AllowedCommands.Clone() },
                Telemetry = new TelemetryConfig
                {
                    EnabledEvents = new[] { "session_started", "session_ended", "phase_started", "phase_cleared", "phase_failed", "robot_auto_charge_started" },
                    SinkFolder = "Telerobot/Telemetry",
                    RequiredFields = new[] { "buildVersion", "dataVersion", "sessionId", "seed", "simProfileId", "phase", "simTime" },
                    SampleIntervalSeconds = 1f, RoutePressureSampleIntervalSeconds = 2f,
                    BatteryEmitPolicy = BatteryEmitPolicy.OnThresholdCrossing | BatteryEmitPolicy.EveryNSeconds,
                    BatteryEmitIntervalSeconds = 1f
                },
                Validation = new ValidationConfig { Seeds = new[] { 1001, 1002, 1003 }, FixedStepSeconds = 1f / 60f }
            };
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Runner, MaxHealth = 90f, MoveSpeed = 6.5f, BaseDamage = 8f,
                PlayerDamage = 12f, RobotDamage = 8f, AttackInterval = 1f, AttackRange = 1.8f, ThreatCost = 1, FirstPhase = 1,
                PathVariationFraction = 0.4f, SeparationRadius = 1.1f, SeparationStrength = 1.6f,
                TargetPriority = new[] { TargetKind.Base, TargetKind.Player, TargetKind.Robot }
            });
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Bruiser, MaxHealth = 500f, MoveSpeed = 2.6f, BaseDamage = 60f,
                PlayerDamage = 30f, RobotDamage = 25f, AttackInterval = 2f, AttackRange = 1.8f, ThreatCost = 5, FirstPhase = 2,
                PathVariationFraction = 0.4f, SeparationRadius = 1.9f, SeparationStrength = 1.6f,
                TargetPriority = new[] { TargetKind.Base, TargetKind.Robot, TargetKind.Player }
            });
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Ripper, MaxHealth = 180f, MoveSpeed = 7.2f, BaseDamage = 10f,
                PlayerDamage = 18f, RobotDamage = 18f, AttackInterval = 0.9f, AttackRange = 1.8f, ThreatCost = 4, FirstPhase = 3,
                PathVariationFraction = 0.4f, SeparationRadius = 1.3f, SeparationStrength = 1.6f,
                TargetPriority = new[] { TargetKind.Robot, TargetKind.Player, TargetKind.Base }
            });
            config.Phases.Add(Phase(1, 40, 150f, new[] { RouteId.NorthRoad },
                Range(18, 24), Range(0, 0), Range(0, 0), Range(18, 24), 0, 0,
                4f, Range(3, 4), 15, Weights(RouteId.NorthRoad, 1f),
                new[] { TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 1f)) }));
            config.Phases.Add(Phase(2, 60, 210f, new[] { RouteId.NorthRoad, RouteId.EastAlley },
                Range(28, 36), Range(2, 3), Range(0, 0), Range(30, 39), 2, 0,
                3.5f, Range(3, 5), 20, Weights(RouteId.NorthRoad, 0.55f, RouteId.EastAlley, 0.45f),
                new[]
                {
                    TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 0.6f, RouteId.EastAlley, 0.4f)),
                    TypeWeights(ZombieType.Bruiser, Weights(RouteId.NorthRoad, 0.65f, RouteId.EastAlley, 0.35f))
                }));
            config.Phases.Add(Phase(3, 80, 270f, new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel },
                Range(42, 48), Range(2, 3), Range(3, 4), Range(47, 55), 2, 3,
                3f, Range(4, 6), 24, Weights(RouteId.NorthRoad, 0.4f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.3f),
                new[]
                {
                    TypeWeights(ZombieType.Runner, Weights(RouteId.NorthRoad, 0.4f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.3f)),
                    TypeWeights(ZombieType.Bruiser, Weights(RouteId.NorthRoad, 0.5f, RouteId.EastAlley, 0.3f, RouteId.SouthTunnel, 0.2f)),
                    TypeWeights(ZombieType.Ripper, Weights(RouteId.NorthRoad, 0.15f, RouteId.EastAlley, 0.2f, RouteId.SouthTunnel, 0.65f))
                }));
            config.Routes.Add(new RouteConfig { Id = RouteId.NorthRoad, OpenPhase = 1, DisplayNameKey = "route.north", Width = 9f, Waypoints = new[] { new Float3(0, 0, 20), new Float3(0, 0, 0) } });
            config.Routes.Add(new RouteConfig { Id = RouteId.EastAlley, OpenPhase = 2, DisplayNameKey = "route.east", Width = 5f, Waypoints = new[] { new Float3(20, 0, 0), new Float3(0, 0, 0) } });
            config.Routes.Add(new RouteConfig { Id = RouteId.SouthTunnel, OpenPhase = 3, DisplayNameKey = "route.south", Width = 6f, Waypoints = new[] { new Float3(-20, 0, 0), new Float3(0, 0, 0) } });
            config.Upgrades.AddRange(new[]
            {
                Upgrade("high_efficiency_battery", UpgradeEffectType.MaxBattery, 20f),
                Upgrade("combat_power_save", UpgradeEffectType.CombatDrainMultiplier, 0.8f),
                Upgrade("haetae_charge_boost", UpgradeEffectType.FirstDashDamageMultiplier, 1.4f),
                Upgrade("charge_station_speedup", UpgradeEffectType.ChargeRateMultiplier, 1.3f),
                Upgrade("base_armor", UpgradeEffectType.BaseMaxHealth, 200f),
                Upgrade("emergency_barrier", UpgradeEffectType.EmergencyBarrier, 1f),
                Upgrade("piercing_rounds", UpgradeEffectType.PiercingRounds, 1f),
                Upgrade("extended_magazine", UpgradeEffectType.MagazineCapacity, 15f),
                Upgrade("emergency_recovery_protocol", UpgradeEffectType.MedicalHealMultiplier, 1.3f)
            });
            config.SimPlayerProfiles.AddRange(new[]
            {
                Profile(SimProfileId.Novice, 0.55f, 0.10f, 1.2f, 2.6f, 0.2f, 0.10f),
                Profile(SimProfileId.Baseline, 0.75f, 0.25f, 0.6f, 1.8f, 0.6f, 0.25f),
                Profile(SimProfileId.Skilled, 0.92f, 0.45f, 0.25f, 1f, 1f, 0.40f)
            });
            return config;
        }

        private static UpgradeConfig Upgrade(string id, UpgradeEffectType type, float amount)
        {
            return new UpgradeConfig { Id = id, DisplayNameKey = id, EffectType = type, Amount = amount };
        }

        private static PhaseConfig Phase(int number, int budget, float duration, RouteId[] routes,
            IntRangeConfig runners, IntRangeConfig bruisers, IntRangeConfig rippers, IntRangeConfig learningTotal,
            int bruiserMinimum, int ripperMinimum, float groupInterval, IntRangeConfig groupSize, int cap,
            RouteWeightConfig[] routeWeights, ZombieRouteWeightConfig[] typeWeights)
        {
            return new PhaseConfig
            {
                Number = number,
                ThreatBudget = budget,
                TargetDurationSeconds = duration,
                OpenRoutes = routes,
                NewlyOpenedRoute = routes[routes.Length - 1],
                RunnerCount = runners,
                BruiserCount = bruisers,
                RipperCount = rippers,
                LearningTotal = learningTotal,
                BruiserMinimum = bruiserMinimum,
                RipperMinimum = ripperMinimum,
                TrimOrder = new[] { SpawnTrimTarget.Runner, SpawnTrimTarget.Bruiser },
                PhaseStartDelaySeconds = 2f,
                GroupIntervalSeconds = groupInterval,
                GroupSize = groupSize,
                MaxAliveConcurrent = cap,
                RouteWeights = routeWeights,
                ZombieTypeRouteWeights = typeWeights
            };
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
                    Weight = System.Convert.ToSingle(values[index * 2 + 1])
                };
            return result;
        }

        private static ZombieRouteWeightConfig TypeWeights(ZombieType type, RouteWeightConfig[] routes)
        {
            return new ZombieRouteWeightConfig { Type = type, Routes = routes };
        }

        private static SimPlayerProfileConfig Profile(SimProfileId id, float accuracy, float headshot, float reaction,
            float fireInterval, float ripperFocus, float chargeThreshold)
        {
            return new SimPlayerProfileConfig
            {
                Id = id,
                AimAccuracy = accuracy,
                HeadshotRate = headshot,
                ReactionDelaySeconds = reaction,
                FireIntervalSeconds = fireInterval,
                RoutePriorityPolicy = id == SimProfileId.Novice ? SimRoutePriorityPolicy.LateReactive
                    : id == SimProfileId.Baseline ? SimRoutePriorityPolicy.BalancedCoverage : SimRoutePriorityPolicy.HighestPressure,
                RipperFocus = ripperFocus,
                RobotChargeThresholdFraction = chargeThreshold,
                UpgradeSelectionPolicy = id == SimProfileId.Novice ? SimUpgradeSelectionPolicy.RandomOfThree
                    : id == SimProfileId.Baseline ? SimUpgradeSelectionPolicy.IntendedMeta : SimUpgradeSelectionPolicy.RiskAwareOptimal,
                GrenadeUsePolicy = id == SimProfileId.Novice ? SimGrenadeUsePolicy.Rarely
                    : id == SimProfileId.Baseline ? SimGrenadeUsePolicy.DenseClusters : SimGrenadeUsePolicy.DenseClustersAndBruisers,
                GrenadeClusterThreshold = id == SimProfileId.Novice ? 8 : id == SimProfileId.Baseline ? 4 : 3
            };
        }
    }
}
