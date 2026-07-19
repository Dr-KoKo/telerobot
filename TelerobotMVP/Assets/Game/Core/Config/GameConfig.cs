using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public enum RouteId { NorthRoad, EastAlley, SouthTunnel }
    public enum ZombieType { Runner, Bruiser, Ripper }
    public enum TargetKind { Base, Player, Robot }
    public enum HitRegion { Body, Head }
    public enum RobotActivity { Idle, Patrol, Combat }
    public enum BatteryBand { Normal, LowPower, Critical, Depleted, Charging }
    public enum RobotMode { Standby, Patrol, Engage, LowBattery, ReturnToCharge, Charging, Disabled, Recovery }
    public enum RobotCommand { DefendPosition, PatrolRoute, ReturnToBase, Charge }
    public enum GameResult { InProgress, Victory, Defeat }
    public enum DefeatReason { None, BaseDestroyed, PlayerDeath }
    public enum SupplyKind { Safe, Risky }
    public enum WarningSeverity { None, Yellow, Red }
    public enum CameraPerspective { ThirdPerson, FirstPerson }
    public enum UpgradeEffectType
    {
        MaxBattery,
        CombatDrainMultiplier,
        FirstDashDamageMultiplier,
        ChargeRateMultiplier,
        BaseMaxHealth,
        EmergencyBarrier,
        PiercingRounds,
        MagazineCapacity,
        MedicalHealMultiplier
    }

    [Serializable]
    public struct Float3
    {
        public float X;
        public float Y;
        public float Z;

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static float Distance(Float3 a, Float3 b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            var z = a.Z - b.Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }

    [Serializable]
    public struct Float2
    {
        public float X;
        public float Y;

        public Float2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public sealed class GameRulesConfig
    {
        public float PlayerMaxHealth;
        public float BaseMaxHealth;
        public float BasePhaseRecoveryFraction;
        public float BaseWarningFraction;
        public float FixedStepSeconds;
        public float PlayerMoveSpeed;
        public float SprintMultiplier;
        public float Gravity;
        public float MouseSensitivity;
        public float CameraDistance;
        public float ThirdPersonFieldOfView;
        public float FirstPersonFieldOfView;
        public float FirstPersonEyeHeight;
        public float CameraCollisionRadius;
        public float CameraCollisionPadding;
        public float JumpHeight;
        public float GroundedVelocity;
        public float MinimumSpawnInterval;
        public string DataVersion;
    }

    [Serializable]
    public sealed class WeaponConfig
    {
        public float BaseDamage;
        public float HeadshotMultiplier;
        public int MagazineSize;
        public int ReserveAmmo;
        public float ReloadSeconds;
        public int GrenadesPerPhase;
        public float Range;
    }

    [Serializable]
    public sealed class GrenadeConfig
    {
        public float Radius;
        public float InnerRadius;
        public float CenterDamage;
        public float EdgeDamage;
        public int MaxTargets;
        public float ThrowDistance;
    }

    [Serializable]
    public sealed class BatteryConfig
    {
        public float Maximum;
        public float LowPowerMaximum;
        public float CriticalMaximum;
        public float IdleDrainPerSecond;
        public float PatrolDrainPerSecond;
        public float CombatDrainPerSecond;
        public float RipperHitDrain;
        public float ChargePerSecond;
        public float LowPowerMoveMultiplier;
        public float LowPowerAttackMultiplier;
        public float DisabledHoldSeconds;
        public float RecoveryPerSecond;
        public float MoveEnableThreshold;
        public float YellowWarningFraction;
        public float RedWarningFraction;
    }

    [Serializable]
    public sealed class RobotConfig
    {
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackInterval;
        public float DetectionRadius;
        public float AttackRange;
    }

    [Serializable]
    public sealed class MedicalConfig
    {
        public float MaxHealth;
        public float HealPerSecond;
        public float Radius;
    }

    [Serializable]
    public sealed class ZombieConfig
    {
        public ZombieType Type;
        public float MaxHealth;
        public float MoveSpeed;
        public float BaseDamage;
        public float PlayerDamage;
        public float RobotDamage;
        public float AttackInterval;
        public float AttackRange;
        public int ThreatCost;
        public int FirstPhase;
        public TargetKind[] TargetPriority;
    }

    [Serializable]
    public sealed class PhaseConfig
    {
        public int Number;
        public int ThreatBudget;
        public float TargetDurationSeconds;
        public RouteId[] OpenRoutes;
        public int RunnerTarget;
        public int BruiserTarget;
        public int RipperTarget;
    }

    [Serializable]
    public sealed class RouteConfig
    {
        public RouteId Id;
        public int OpenPhase;
        public string DisplayNameKey;
        public Float3[] Waypoints;
        public float Width;
    }

    [Serializable]
    public sealed class UpgradeConfig
    {
        public string Id;
        public string DisplayNameKey;
        public UpgradeEffectType EffectType;
        public float Amount;
    }

    [Serializable]
    public sealed class BarrierConfig
    {
        public float MaxHealth;
    }

    [Serializable]
    public sealed class WarningConfig
    {
        public float BatteryYellowFraction;
        public float BatteryRedFraction;
        public float BaseWarningFraction;
    }

    [Serializable]
    public sealed class WorldLayoutConfig
    {
        public Float3 BasePosition;
        public Float3 PlayerStart;
        public Float3[] RobotStarts;
        public Float3 BaseRally;
        public Float3 ChargingStation;
        public Float3 SafeSupply;
        public Float3 RiskySupply;
        public Float3 MedicalAnchor;
        public float SupplyInteractionRadius;
        public float ChargingArrivalRadius;
    }

    [Serializable]
    public sealed class CommandConfig
    {
        public RobotCommand[] Commands;
    }

    [Serializable]
    public sealed class TelemetryConfig
    {
        public string[] EnabledEvents;
        public string SinkFolder;
    }

    [Serializable]
    public sealed class ValidationConfig
    {
        public int[] Seeds;
        public float FixedStepSeconds;
    }

    [Serializable]
    public sealed class GameplayConfig
    {
        public GameRulesConfig Game;
        public WeaponConfig Weapon;
        public GrenadeConfig Grenade;
        public BatteryConfig Battery;
        public RobotConfig Robot;
        public MedicalConfig Medical;
        public BarrierConfig Barrier;
        public WarningConfig Warnings;
        public WorldLayoutConfig World;
        public CommandConfig Commands;
        public TelemetryConfig Telemetry;
        public ValidationConfig Validation;
        public List<ZombieConfig> Zombies = new List<ZombieConfig>();
        public List<PhaseConfig> Phases = new List<PhaseConfig>();
        public List<RouteConfig> Routes = new List<RouteConfig>();
        public List<UpgradeConfig> Upgrades = new List<UpgradeConfig>();

        public ZombieConfig GetZombie(ZombieType type)
        {
            var result = Zombies.Find(item => item.Type == type);
            if (result == null) throw new InvalidOperationException("Missing zombie config: " + type);
            return result;
        }

        public PhaseConfig GetPhase(int number)
        {
            var result = Phases.Find(item => item.Number == number);
            if (result == null) throw new InvalidOperationException("Missing phase config: " + number);
            return result;
        }

        public RouteConfig GetRoute(RouteId id)
        {
            var result = Routes.Find(item => item.Id == id);
            if (result == null) throw new InvalidOperationException("Missing route config: " + id);
            return result;
        }
    }
}
