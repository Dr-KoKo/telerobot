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
            var delta = Math.Max(0f, deltaTime);
            robot.AttackCooldownRemaining = Math.Max(0f, robot.AttackCooldownRemaining - delta);
            robot.DashCooldownRemaining = Math.Max(0f, robot.DashCooldownRemaining - delta);
            BeginEngagement(robot, targetId);
            if (!inRange) return 0f;
            if (!robot.FirstDashUsed && robot.DashCooldownRemaining <= 0f)
            {
                robot.FirstDashUsed = true;
                robot.DashCooldownRemaining = config.DashCooldownSeconds;
                robot.AttackCooldownRemaining = config.BiteCooldownSeconds;
                return FirstDashDamage(firstDashMultiplier);
            }
            if (robot.AttackCooldownRemaining > 0f) return 0f;
            robot.AttackCooldownRemaining = config.BiteCooldownSeconds;
            return config.BiteDamage;
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
    }
}
