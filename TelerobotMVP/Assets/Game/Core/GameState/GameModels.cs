using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    [Serializable]
    public sealed class HealthState
    {
        public float Current;
        public float Maximum;
        public bool IsDead { get { return Current <= 0f; } }

        public HealthState(float maximum)
        {
            Maximum = Math.Max(1f, maximum);
            Current = Maximum;
        }
    }

    [Serializable]
    public sealed class AmmoState
    {
        public int Loaded;
        public int Reserve;
        public int MagazineCapacity;
        public bool IsReloading;
        public float ReloadRemaining;

        public AmmoState(int capacity, int reserve)
        {
            MagazineCapacity = Math.Max(1, capacity);
            Loaded = MagazineCapacity;
            Reserve = Math.Max(0, reserve);
        }
    }

    [Serializable]
    public sealed class PlayerState
    {
        public HealthState Health;
        public AmmoState Ammo;
        public int Grenades;

        public PlayerState(float health, int magazine, int reserve, int grenades)
        {
            Health = new HealthState(health);
            Ammo = new AmmoState(magazine, reserve);
            Grenades = grenades;
        }
    }

    [Serializable]
    public sealed class BaseState
    {
        public HealthState Health;
        public bool WarningActive;

        public BaseState(float health)
        {
            Health = new HealthState(health);
        }
    }

    [Serializable]
    public sealed class RobotState
    {
        public string Id;
        public HealthState Health;
        public float Battery;
        public float MaximumBattery;
        public BatteryBand BatteryBand;
        public RobotMode Mode;
        public RobotCommand Command;
        public RouteId AssignedRoute;
        public float DisabledElapsed;
        public bool FirstDashUsed;
        public string CurrentTargetId;
        public float AttackCooldownRemaining;
        public float DashCooldownRemaining;

        public RobotState(string id, float health, float battery)
        {
            Id = id;
            Health = new HealthState(health);
            Battery = battery;
            MaximumBattery = battery;
            BatteryBand = BatteryBand.Normal;
            Mode = RobotMode.Standby;
            Command = RobotCommand.DefendPosition;
            AssignedRoute = RouteId.NorthRoad;
        }

        public bool IsDestroyed { get { return Mode == RobotMode.Destroyed || Health.IsDead; } }
        public bool CanMove { get { return !IsDestroyed && Mode != RobotMode.Disabled && Mode != RobotMode.Recovery; } }
        public bool CanAttack { get { return CanMove && Mode != RobotMode.ReturnToCharge && Mode != RobotMode.Charging; } }
        public bool CanCharge { get { return !IsDestroyed && Mode != RobotMode.Disabled && Mode != RobotMode.Recovery; } }
    }

    [Serializable]
    public sealed class ZombieState
    {
        public string Id;
        public ZombieType Type;
        public RouteId Route;
        public HealthState Health;
        public float Progress;

        public ZombieState(string id, ZombieType type, RouteId route, float health)
        {
            Id = id;
            Type = type;
            Route = route;
            Health = new HealthState(health);
        }
    }

    [Serializable]
    public sealed class PhaseState
    {
        public int Number;
        public bool AllSpawned;
        public int AliveCount;
        public bool Cleared;
        public List<RouteId> OpenRoutes = new List<RouteId>();

        public PhaseState(int number, IEnumerable<RouteId> routes)
        {
            Number = number;
            OpenRoutes.AddRange(routes);
        }
    }

    [Serializable]
    public sealed class SessionState
    {
        public int Seed;
        public float ElapsedTime;
        public int CurrentPhase;
        public GameResult Result = GameResult.InProgress;
        public DefeatReason DefeatReason = DefeatReason.None;
        public List<string> SelectedUpgrades = new List<string>();

        public SessionState(int seed)
        {
            Seed = seed;
            CurrentPhase = 1;
        }
    }

    [Serializable]
    public sealed class RuntimeModifiers
    {
        public float CombatDrainMultiplier = 1f;
        public float FirstDashDamageMultiplier = 1f;
        public float ChargeRateMultiplier = 1f;
        public float MedicalHealMultiplier = 1f;
        public bool EmergencyBarrier;
        public bool PiercingRounds;
    }

    [Serializable]
    public sealed class SpawnEntry
    {
        public ZombieType Type;
        public RouteId Route;

        public SpawnEntry(ZombieType type, RouteId route)
        {
            Type = type;
            Route = route;
        }
    }
}
