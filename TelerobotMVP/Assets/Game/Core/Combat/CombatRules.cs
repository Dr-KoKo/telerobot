using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public sealed class GrenadeTarget
    {
        public string Id;
        public float Distance;
        public HealthState Health;

        public GrenadeTarget(string id, float distance, HealthState health)
        {
            Id = id;
            Distance = distance;
            Health = health;
        }
    }

    public static class CombatRules
    {
        public static float CalculateBulletDamage(WeaponConfig config, HitRegion region)
        {
            if (config == null) throw new ArgumentNullException("config");
            return config.BaseDamage * (region == HitRegion.Head ? config.HeadshotMultiplier : 1f);
        }

        public static float ApplyDamage(HealthState target, float amount)
        {
            if (target == null) throw new ArgumentNullException("target");
            var before = target.Current;
            target.Current = Math.Max(0f, target.Current - Math.Max(0f, amount));
            return before - target.Current;
        }

        public static float Heal(HealthState target, float amount)
        {
            if (target == null) throw new ArgumentNullException("target");
            var before = target.Current;
            target.Current = Math.Min(target.Maximum, target.Current + Math.Max(0f, amount));
            return target.Current - before;
        }

        public static float RecoverBase(BaseState target, float fraction)
        {
            return Heal(target.Health, target.Health.Maximum * Math.Max(0f, fraction));
        }

        public static bool TryFire(AmmoState ammo)
        {
            if (ammo == null || ammo.IsReloading || ammo.Loaded <= 0) return false;
            ammo.Loaded--;
            return true;
        }

        public static bool BeginReload(AmmoState ammo, float reloadSeconds)
        {
            if (ammo == null || ammo.IsReloading || ammo.Loaded >= ammo.MagazineCapacity || ammo.Reserve <= 0) return false;
            ammo.IsReloading = true;
            ammo.ReloadRemaining = Math.Max(0f, reloadSeconds);
            return true;
        }

        public static bool TickReload(AmmoState ammo, float deltaTime)
        {
            if (ammo == null || !ammo.IsReloading) return false;
            ammo.ReloadRemaining -= Math.Max(0f, deltaTime);
            if (ammo.ReloadRemaining > 0f) return false;

            var needed = ammo.MagazineCapacity - ammo.Loaded;
            var moved = Math.Min(needed, ammo.Reserve);
            ammo.Loaded += moved;
            ammo.Reserve -= moved;
            ammo.ReloadRemaining = 0f;
            ammo.IsReloading = false;
            return true;
        }

        public static void Resupply(AmmoState ammo, AmmoConfig config)
        {
            if (ammo == null) throw new ArgumentNullException("ammo");
            if (config == null) throw new ArgumentNullException("config");
            ammo.Reserve = config.ResupplyPolicy == ResupplyPolicy.FullReserve
                ? Math.Max(ammo.Reserve, config.ReserveAmmoMax)
                : Math.Min(config.ReserveAmmoMax, ammo.Reserve + Math.Max(0, config.ResupplyAmount));
        }

        public static float GrenadeDamage(GrenadeConfig config, float distance)
        {
            if (config == null || distance < 0f || distance > config.Radius) return 0f;
            if (distance <= config.InnerRadius) return config.CenterDamage;
            var range = Math.Max(0.0001f, config.Radius - config.InnerRadius);
            var t = Math.Min(1f, (distance - config.InnerRadius) / range);
            return config.CenterDamage + (config.EdgeDamage - config.CenterDamage) * t;
        }

        public static List<string> ApplyGrenade(GrenadeConfig config, IEnumerable<GrenadeTarget> candidates)
        {
            if (config == null) throw new ArgumentNullException("config");
            var sorted = new List<GrenadeTarget>();
            foreach (var candidate in candidates)
            {
                if (candidate != null && candidate.Distance <= config.Radius && !candidate.Health.IsDead) sorted.Add(candidate);
            }
            sorted.Sort((a, b) =>
            {
                var distanceOrder = a.Distance.CompareTo(b.Distance);
                return distanceOrder != 0 ? distanceOrder : string.CompareOrdinal(a.Id, b.Id);
            });

            var affected = new List<string>();
            var count = Math.Min(config.MaxTargets, sorted.Count);
            for (var index = 0; index < count; index++)
            {
                var target = sorted[index];
                ApplyDamage(target.Health, GrenadeDamage(config, target.Distance));
                affected.Add(target.Id);
            }
            return affected;
        }
    }
}
