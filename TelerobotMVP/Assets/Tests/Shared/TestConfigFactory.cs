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
                    PlayerMaxHealth = 100f, BaseMaxHealth = 1000f, BasePhaseRecoveryFraction = 0.15f,
                    BaseWarningFraction = 0.30f, FixedStepSeconds = 1f / 60f, PlayerMoveSpeed = 8f,
                    SprintMultiplier = 1.5f, Gravity = 24f, MouseSensitivity = 0.12f, CameraDistance = 6f,
                    ThirdPersonFieldOfView = 65f, FirstPersonFieldOfView = 75f,
                    FirstPersonEyeHeight = 0.65f, CameraCollisionRadius = 0.22f,
                    CameraCollisionPadding = 0.08f, JumpHeight = 1.35f, GroundedVelocity = -2f,
                    MinimumSpawnInterval = 1.2f, DataVersion = "test-v1"
                },
                Weapon = new WeaponConfig
                {
                    BaseDamage = 30f, HeadshotMultiplier = 2.5f, MagazineSize = 30,
                    ReserveAmmo = 180, ReloadSeconds = 2f, GrenadesPerPhase = 2, Range = 200f
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
                    MaxHealth = 300f, MoveSpeed = 10f, AttackDamage = 75f, AttackInterval = 1.1f,
                    DetectionRadius = 18f, AttackRange = 1.7f
                },
                Medical = new MedicalConfig { MaxHealth = 150f, HealPerSecond = 8f, Radius = 6f },
                Barrier = new BarrierConfig { MaxHealth = 300f },
                Warnings = new WarningConfig { BatteryYellowFraction = 0.25f, BatteryRedFraction = 0.10f, BaseWarningFraction = 0.30f },
                World = new WorldLayoutConfig
                {
                    BasePosition = new Float3(0, 0, 0), PlayerStart = new Float3(0, 1, -7),
                    RobotStarts = new[] { new Float3(-2.4f, 0.8f, -3.8f), new Float3(2.4f, 0.8f, -3.8f) },
                    BaseRally = new Float3(0, 0.8f, -4), ChargingStation = new Float3(4.5f, 0.5f, -4.5f),
                    SafeSupply = new Float3(-3, 0.5f, -2), RiskySupply = new Float3(0, 0.5f, 18),
                    MedicalAnchor = new Float3(-4.5f, 0.8f, -4.5f), SupplyInteractionRadius = 2.5f,
                    ChargingArrivalRadius = 1.2f
                },
                Commands = new CommandConfig { Commands = (RobotCommand[])RobotCommandSystem.AllowedCommands.Clone() },
                Telemetry = new TelemetryConfig
                {
                    EnabledEvents = new[] { "session_started", "session_ended", "phase_started", "phase_cleared", "phase_failed" },
                    SinkFolder = "Telerobot/Telemetry"
                },
                Validation = new ValidationConfig { Seeds = new[] { 1001, 1002, 1003 }, FixedStepSeconds = 1f / 60f }
            };
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Runner, MaxHealth = 90f, MoveSpeed = 6.5f, BaseDamage = 8f,
                PlayerDamage = 12f, RobotDamage = 8f, AttackInterval = 1f, AttackRange = 1.8f, ThreatCost = 1, FirstPhase = 1,
                TargetPriority = new[] { TargetKind.Base, TargetKind.Player, TargetKind.Robot }
            });
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Bruiser, MaxHealth = 500f, MoveSpeed = 2.6f, BaseDamage = 60f,
                PlayerDamage = 30f, RobotDamage = 25f, AttackInterval = 2f, AttackRange = 1.8f, ThreatCost = 5, FirstPhase = 2,
                TargetPriority = new[] { TargetKind.Base, TargetKind.Robot, TargetKind.Player }
            });
            config.Zombies.Add(new ZombieConfig
            {
                Type = ZombieType.Ripper, MaxHealth = 180f, MoveSpeed = 7.2f, BaseDamage = 10f,
                PlayerDamage = 18f, RobotDamage = 18f, AttackInterval = 0.9f, AttackRange = 1.8f, ThreatCost = 4, FirstPhase = 3,
                TargetPriority = new[] { TargetKind.Robot, TargetKind.Player, TargetKind.Base }
            });
            config.Phases.Add(new PhaseConfig { Number = 1, ThreatBudget = 40, TargetDurationSeconds = 150f, OpenRoutes = new[] { RouteId.NorthRoad }, RunnerTarget = 30 });
            config.Phases.Add(new PhaseConfig { Number = 2, ThreatBudget = 60, TargetDurationSeconds = 210f, OpenRoutes = new[] { RouteId.NorthRoad, RouteId.EastAlley }, RunnerTarget = 45, BruiserTarget = 3 });
            config.Phases.Add(new PhaseConfig { Number = 3, ThreatBudget = 80, TargetDurationSeconds = 270f, OpenRoutes = new[] { RouteId.NorthRoad, RouteId.EastAlley, RouteId.SouthTunnel }, RunnerTarget = 60, RipperTarget = 5 });
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
            return config;
        }

        private static UpgradeConfig Upgrade(string id, UpgradeEffectType type, float amount)
        {
            return new UpgradeConfig { Id = id, DisplayNameKey = id, EffectType = type, Amount = amount };
        }
    }
}
