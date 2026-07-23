using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public sealed class UpgradeSystem
    {
        private readonly GameplayConfig config;

        public UpgradeSystem(GameplayConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public List<UpgradeConfig> Offer(IDeterministicRng rng)
        {
            return Offer(rng, Array.Empty<string>());
        }

        public List<UpgradeConfig> Offer(IDeterministicRng rng, IEnumerable<string> selectedUpgradeIds)
        {
            if (rng == null) throw new ArgumentNullException("rng");
            if (config.Upgrades.Count != 9) throw new InvalidOperationException("Exactly nine upgrade definitions are required.");
            var selected = selectedUpgradeIds == null ? new HashSet<string>() : new HashSet<string>(selectedUpgradeIds);
            var pool = config.Upgrades.FindAll(item => !selected.Contains(item.Id));
            if (pool.Count < 3) throw new InvalidOperationException("At least three unselected upgrades are required for an offer.");
            var result = new List<UpgradeConfig>(3);
            while (result.Count < 3)
            {
                var index = rng.NextInt(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return result;
        }

        public bool Apply(UpgradeConfig upgrade, SessionState session, BaseState baseState, IList<RobotState> robots,
            PlayerState player, RuntimeModifiers modifiers)
        {
            if (upgrade == null || session.SelectedUpgrades.Count >= 2 || session.SelectedUpgrades.Contains(upgrade.Id)) return false;
            session.SelectedUpgrades.Add(upgrade.Id);
            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.MaxBattery:
                    foreach (var robot in robots)
                    {
                        robot.MaximumBattery += upgrade.Amount;
                        robot.Battery += upgrade.Amount;
                    }
                    break;
                case UpgradeEffectType.CombatDrainMultiplier:
                    modifiers.CombatDrainMultiplier *= upgrade.Amount;
                    break;
                case UpgradeEffectType.FirstDashDamageMultiplier:
                    modifiers.FirstDashDamageMultiplier *= upgrade.Amount;
                    break;
                case UpgradeEffectType.ChargeRateMultiplier:
                    modifiers.ChargeRateMultiplier *= upgrade.Amount;
                    break;
                case UpgradeEffectType.BaseMaxHealth:
                    baseState.Health.Maximum += upgrade.Amount;
                    baseState.Health.Current += upgrade.Amount;
                    break;
                case UpgradeEffectType.EmergencyBarrier:
                    modifiers.EmergencyBarrier = true;
                    break;
                case UpgradeEffectType.PiercingRounds:
                    modifiers.PiercingRounds = true;
                    break;
                case UpgradeEffectType.MagazineCapacity:
                    player.Ammo.MagazineCapacity += (int)upgrade.Amount;
                    break;
                case UpgradeEffectType.MedicalHealMultiplier:
                    modifiers.MedicalHealMultiplier *= upgrade.Amount;
                    break;
            }
            return true;
        }
    }

    public sealed class BarrierState
    {
        public RouteId Route;
        public HealthState Health;

        public BarrierState(RouteId route, float health)
        {
            Route = route;
            Health = new HealthState(health);
        }
    }
}
