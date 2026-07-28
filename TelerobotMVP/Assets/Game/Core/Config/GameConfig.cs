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
    public enum RobotMode { Standby, Patrol, Engage, LowBattery, ReturnToCharge, Charging, Disabled, Recovery, Destroyed }
    public enum RobotCommand { DefendPosition, PatrolRoute, ReturnToBase }
    public enum GameResult { InProgress, Victory, Defeat }
    public enum DefeatReason { None, BaseDestroyed, PlayerDeath }
    public enum SupplyKind { Safe, Risky }
    public enum WarningSeverity { None, Yellow, Red }
    public enum CameraPerspective { ThirdPerson, FirstPerson }
    public enum SpawnTrimTarget { Runner, Bruiser, Ripper }
    public enum ResupplyPolicy { FullReserve, FixedAmount }
    public enum GrenadeResupplyPolicy { None, PhaseResetOnly }
    [Flags] public enum BatteryEmitPolicy { None = 0, OnThresholdCrossing = 1, EveryNSeconds = 2 }
    public enum SimProfileId { Novice, Baseline, Skilled }
    public enum SimRoutePriorityPolicy { LateReactive, BalancedCoverage, HighestPressure }
    public enum SimGrenadeUsePolicy { Rarely, DenseClusters, DenseClustersAndBruisers }
    public enum HaetaeSpecialization { Unselected, Melee, Ranged, Balanced }
    public enum HaetaeMasteryUpgrade { Power, Armor, Efficiency, AttackSpeed }
    public enum DamageSourceKind { Player, Haetae, Environment, Debug, Other }
    public enum RobotMovementIntent { Approach, Hold, Retreat, ReturnToCommandAnchor, None }
    public enum RobotAttackKind { None, Dash, Bite, Ranged }
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
        public float TargetSessionMinimumSeconds;
        public float TargetSessionMaximumSeconds;
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
        public string DataVersion;
    }

    [Serializable]
    public sealed class WeaponConfig
    {
        public float BaseDamage;
        public float HeadshotMultiplier;
        public int MagazineSize;
        public float ReloadSeconds;
        public float FireIntervalSeconds;
        public int GrenadesPerPhase;
        public float Range;
    }

    [Serializable]
    public sealed class BaseConfig
    {
        public float MaxHealth;
        public float PhaseRecoveryFraction;
        public float WarningFraction;
        public bool AllowPlayerRepair;
    }

    [Serializable]
    public sealed class AmmoConfig
    {
        public int StartReserveAmmo;
        public int ReserveAmmoMax;
        public ResupplyPolicy ResupplyPolicy;
        public int ResupplyAmount;
        public float ResupplyUseSeconds;
        public float ResupplyCooldownSeconds;
        public GrenadeResupplyPolicy GrenadeResupplyPolicy;
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
        public float DashDamage;
        public float BiteDamage;
        public float BiteCooldownSeconds;
        public float DashCooldownSeconds;
        public float DetectionRadius;
        public float EngageRange;
        public float SeparationRadius;
        public float SeparationStrength;
        public float FormationSpacing;
        public float DefendLeashRadius;
        public float RunnerKillTargetMinimumSeconds;
        public float RunnerKillTargetMaximumSeconds;
        public float BruiserKillTargetMinimumSeconds;
        public float BruiserKillTargetMaximumSeconds;
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
        public int HaetaeExperienceReward;
        public float MaxHealth;
        public float MoveSpeed;
        public float BaseDamage;
        public float PlayerDamage;
        public float RobotDamage;
        public float AttackInterval;
        public float AttackRange;
        public float PathVariationFraction;
        public float SeparationRadius;
        public float SeparationStrength;
        public int ThreatCost;
        public int FirstPhase;
        public TargetKind[] TargetPriority;
    }

    [Serializable]
    public sealed class IntRangeConfig
    {
        public int Min;
        public int Max;

        public int Sample(IDeterministicRng rng)
        {
            if (rng == null) throw new ArgumentNullException("rng");
            if (Max < Min) throw new InvalidOperationException("Range maximum must be greater than or equal to minimum.");
            return Min + (Max == Min ? 0 : rng.NextInt(Max - Min + 1));
        }
    }

    [Serializable]
    public sealed class RouteWeightConfig
    {
        public RouteId Route;
        public float Weight;
    }

    [Serializable]
    public sealed class ZombieRouteWeightConfig
    {
        public ZombieType Type;
        public RouteWeightConfig[] Routes;
    }

    [Serializable]
    public sealed class PhaseConfig
    {
        public int Number;
        public int ThreatBudget;
        public float TargetDurationSeconds;
        public RouteId[] OpenRoutes;
        public bool OpensNewRoute;
        public RouteId NewlyOpenedRoute;
        public IntRangeConfig RunnerCount;
        public IntRangeConfig BruiserCount;
        public IntRangeConfig RipperCount;
        public IntRangeConfig LearningTotal;
        public int RunnerMinimum;
        public int BruiserMinimum;
        public int RipperMinimum;
        public SpawnTrimTarget[] TrimOrder;
        public float PhaseStartDelaySeconds;
        public float GroupIntervalSeconds;
        public IntRangeConfig GroupSize;
        public int MaxAliveConcurrent;
        public RouteWeightConfig[] RouteWeights;
        public ZombieRouteWeightConfig[] ZombieTypeRouteWeights;
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
    public sealed class BarrierConfig
    {
        public float MaxHealth;
    }

    [Serializable]
    public sealed class WarningConfig
    {
        public float BatteryYellowFraction;
        public float BatteryRedFraction;
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
        public float SupplyExitTolerance;
        public float BaseChargingRadius;
        public float ChargingArrivalRadius;
        public float BaseOuterRadius;
        public int BaseTerraceCount;
        public float BaseTerraceRise;
        public float BaseTerraceDepth;
        public float BaseTerraceSlopeRun;
        public float BaseBeaconDiameter;
        public float BaseAttackEdgePadding;
        public float BaseAttackRowSpacing;
        public float BaseAttackLateralSpacing;
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
        public string[] RequiredFields;
        public float SampleIntervalSeconds;
        public float RoutePressureSampleIntervalSeconds;
        public BatteryEmitPolicy BatteryEmitPolicy;
        public float BatteryEmitIntervalSeconds;
    }

    [Serializable]
    public sealed class SimPlayerProfileConfig
    {
        public SimProfileId Id;
        public float AimAccuracy;
        public float HeadshotRate;
        public float ReactionDelaySeconds;
        public float FireIntervalSeconds;
        public SimRoutePriorityPolicy RoutePriorityPolicy;
        public float RipperFocus;
        public float RobotChargeThresholdFraction;
        public SimGrenadeUsePolicy GrenadeUsePolicy;
        public int GrenadeClusterThreshold;
        public HaetaeSpecializationPair DefaultSpecializationLoadout;
    }

    [Serializable]
    public sealed class HaetaeSpecializationPair
    {
        public HaetaeSpecialization Haetae1;
        public HaetaeSpecialization Haetae2;

        public HaetaeSpecializationPair()
            : this(HaetaeSpecialization.Balanced, HaetaeSpecialization.Balanced)
        {
        }

        public HaetaeSpecializationPair(HaetaeSpecialization haetae1, HaetaeSpecialization haetae2)
        {
            Haetae1 = haetae1;
            Haetae2 = haetae2;
        }

        public HaetaeSpecialization ForIndex(int index)
        {
            if (index == 0) return Haetae1;
            if (index == 1) return Haetae2;
            throw new ArgumentOutOfRangeException("index");
        }
    }

    [Serializable]
    public sealed class SimulationRunOptions
    {
        public HaetaeSpecializationPair SpecializationLoadout;
    }

    [Serializable]
    public sealed class RobotCombatProfileConfig
    {
        public float PreferredMinRange;
        public float PreferredMaxRange;
        public float DashDamageMultiplier;
        public float BiteDamageMultiplier;
        public float RangedDamage;
        public float RangedCooldownSeconds;
        public float CleaveRadius;
        public int MaximumTargets;
        public float IncomingDamageMultiplier;
        public float CombatBatteryMultiplier;
    }

    [Serializable]
    public sealed class HaetaeSpecializationConfig
    {
        public HaetaeSpecialization Id;
        public string DisplayNameKey;
        public string DescriptionKey;
        public RobotCombatProfileConfig Combat;
    }

    [Serializable]
    public sealed class HaetaeProgressionConfig
    {
        public int ExperiencePerLevel;
        public float ReadyAlertSeconds;
        public float PowerDamageBonusPerRank;
        public float ArmorDamageReductionPerRank;
        public float EfficiencyBatteryReductionPerRank;
        public float AttackSpeedBonusPerRank;
        public float MinimumReductionMultiplier;

        public int LevelForExperience(int experience)
        {
            if (ExperiencePerLevel <= 0)
                throw new InvalidOperationException("Experience per level must be positive.");
            var level = 1L + Math.Max(0, experience) / (long)ExperiencePerLevel;
            return (int)Math.Min(int.MaxValue, level);
        }

        public int ExperienceRequiredForNextLevel(int currentLevel)
        {
            if (ExperiencePerLevel <= 0)
                throw new InvalidOperationException("Experience per level must be positive.");
            var required = Math.Max(1L, currentLevel) * ExperiencePerLevel;
            return (int)Math.Min(int.MaxValue, required);
        }

        public float DamageMultiplier(HaetaeProgressionState progression)
        {
            return 1f + Math.Max(0, progression == null ? 0 : progression.PowerRank) *
                Math.Max(0f, PowerDamageBonusPerRank);
        }

        public float IncomingDamageMultiplier(HaetaeProgressionState progression)
        {
            var multiplier = 1f - Math.Max(0, progression == null ? 0 : progression.ArmorRank) *
                Math.Max(0f, ArmorDamageReductionPerRank);
            return Math.Max(MinimumReductionMultiplier, multiplier);
        }

        public float CombatBatteryMultiplier(HaetaeProgressionState progression)
        {
            var multiplier = 1f - Math.Max(0, progression == null ? 0 : progression.EfficiencyRank) *
                Math.Max(0f, EfficiencyBatteryReductionPerRank);
            return Math.Max(MinimumReductionMultiplier, multiplier);
        }

        public float AttackCooldownMultiplier(HaetaeProgressionState progression)
        {
            var multiplier = 1f - Math.Max(0, progression == null ? 0 : progression.AttackSpeedRank) *
                Math.Max(0f, AttackSpeedBonusPerRank);
            return Math.Max(MinimumReductionMultiplier, multiplier);
        }
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
        public BaseConfig Base;
        public AmmoConfig Ammo;
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
        public HaetaeProgressionConfig HaetaeProgression;
        public List<SimPlayerProfileConfig> SimPlayerProfiles = new List<SimPlayerProfileConfig>();
        public List<HaetaeSpecializationConfig> HaetaeSpecializations = new List<HaetaeSpecializationConfig>();
        public List<ZombieConfig> Zombies = new List<ZombieConfig>();
        public List<PhaseConfig> Phases = new List<PhaseConfig>();
        public List<RouteConfig> Routes = new List<RouteConfig>();
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

        public SimPlayerProfileConfig GetSimPlayerProfile(SimProfileId id)
        {
            var result = SimPlayerProfiles.Find(item => item.Id == id);
            if (result == null) throw new InvalidOperationException("Missing simulation player profile: " + id);
            return result;
        }

        public HaetaeSpecializationConfig GetHaetaeSpecialization(HaetaeSpecialization id)
        {
            var result = HaetaeSpecializations.Find(item => item.Id == id);
            if (result == null) throw new InvalidOperationException("Missing Haetae specialization config: " + id);
            return result;
        }
    }
}
