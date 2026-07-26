using System;

namespace Telerobot.Game.Core
{
    public sealed class RobotAttackSystem
    {
        private readonly RobotConfig config;

        public RobotAttackSystem(RobotConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public float FirstDashDamage(float multiplier)
        {
            return config.DashDamage * Math.Max(0f, multiplier);
        }

        public float EstimateKillTime(float targetHealth, float dashMultiplier)
        {
            var remaining = Math.Max(0f, targetHealth - FirstDashDamage(dashMultiplier));
            var bites = remaining <= 0f ? 0 : (int)Math.Ceiling(remaining / config.BiteDamage);
            return config.BiteCooldownSeconds * (bites + 1);
        }

        public float Advance(RobotState robot, string targetId, float deltaTime, bool inRange, float firstDashMultiplier)
        {
            if (robot == null || robot.IsDestroyed || !robot.CanAttack || string.IsNullOrEmpty(targetId)) return 0f;
            Tick(robot, deltaTime);
            BeginEngagement(robot, targetId);
            if (!inRange) return 0f;
            var profile = GeneralProfile();
            var decision = new RobotCombatDecision
            {
                Movement = RobotMovementIntent.Hold,
                Attack = new RobotAttackResult
                {
                    Kind = !robot.FirstDashUsed && robot.DashCooldownRemaining <= 0f
                        ? RobotAttackKind.Dash
                        : RobotAttackKind.Bite,
                    Range = config.EngageRange,
                    MaximumTargets = 1
                }
            };
            return Advance(robot, targetId, decision, profile, firstDashMultiplier).Damage;
        }

        public void Tick(RobotState robot, float deltaTime)
        {
            if (robot == null) return;
            var delta = Math.Max(0f, deltaTime);
            robot.AttackCooldownRemaining = Math.Max(0f, robot.AttackCooldownRemaining - delta);
            robot.DashCooldownRemaining = Math.Max(0f, robot.DashCooldownRemaining - delta);
        }

        public RobotAttackResult Advance(
            RobotState robot,
            string targetId,
            RobotCombatDecision decision,
            RobotCombatProfileConfig profile,
            float firstDashMultiplier,
            float attackCooldownMultiplier = 1f)
        {
            if (robot == null || robot.IsDestroyed || !robot.CanAttack || string.IsNullOrEmpty(targetId) ||
                profile == null || decision.Attack.Kind == RobotAttackKind.None ||
                robot.AttackCooldownRemaining > 0f)
                return RobotAttackResult.None;

            BeginEngagement(robot, targetId);
            var result = decision.Attack;
            var cooldownMultiplier = Math.Max(0.01f, attackCooldownMultiplier);
            result.AreaRadius = Math.Max(0f, profile.CleaveRadius);
            result.MaximumTargets = Math.Max(1, profile.MaximumTargets);
            if (result.Kind == RobotAttackKind.Dash)
            {
                if (robot.FirstDashUsed || robot.DashCooldownRemaining > 0f) return RobotAttackResult.None;
                robot.FirstDashUsed = true;
                robot.DashCooldownRemaining = Cooldown(config.DashCooldownSeconds, cooldownMultiplier);
                robot.AttackCooldownRemaining = Cooldown(config.BiteCooldownSeconds, cooldownMultiplier);
                result.Damage = config.DashDamage * Math.Max(0f, profile.DashDamageMultiplier) *
                    Math.Max(0f, firstDashMultiplier);
                result.CooldownSeconds = robot.AttackCooldownRemaining;
                return result;
            }
            if (result.Kind == RobotAttackKind.Bite)
            {
                robot.AttackCooldownRemaining = Cooldown(config.BiteCooldownSeconds, cooldownMultiplier);
                result.Damage = config.BiteDamage * Math.Max(0f, profile.BiteDamageMultiplier);
                result.CooldownSeconds = robot.AttackCooldownRemaining;
                return result;
            }
            if (result.Kind == RobotAttackKind.Ranged)
            {
                robot.AttackCooldownRemaining = Cooldown(profile.RangedCooldownSeconds, cooldownMultiplier);
                result.Damage = Math.Max(0f, profile.RangedDamage);
                result.CooldownSeconds = robot.AttackCooldownRemaining;
                return result;
            }
            return RobotAttackResult.None;
        }

        private static float Cooldown(float seconds, float multiplier)
        {
            return Math.Max(0.01f, Math.Max(0f, seconds) * multiplier);
        }

        public void BeginEngagement(RobotState robot, string targetId)
        {
            if (robot == null || robot.IsDestroyed || !robot.CanAttack || string.IsNullOrEmpty(targetId)) return;
            if (string.Equals(robot.CurrentTargetId, targetId, StringComparison.Ordinal)) return;
            robot.CurrentTargetId = targetId;
            robot.FirstDashUsed = false;
        }

        public void EndEngagement(RobotState robot)
        {
            if (robot == null) return;
            robot.CurrentTargetId = null;
            robot.FirstDashUsed = false;
        }

        private RobotCombatProfileConfig GeneralProfile()
        {
            return new RobotCombatProfileConfig
            {
                PreferredMinRange = 0f,
                PreferredMaxRange = config.EngageRange,
                DashDamageMultiplier = 1f,
                BiteDamageMultiplier = 1f,
                MaximumTargets = 1,
                IncomingDamageMultiplier = 1f,
                CombatBatteryMultiplier = 1f
            };
        }
    }
}
