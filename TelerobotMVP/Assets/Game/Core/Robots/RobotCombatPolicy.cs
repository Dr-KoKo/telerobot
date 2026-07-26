using System;

namespace Telerobot.Game.Core
{
    public sealed class RobotCombatPolicy
    {
        private readonly GameplayConfig config;
        private readonly RobotCombatProfileConfig generalProfile;

        public RobotCombatPolicy(GameplayConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
            generalProfile = new RobotCombatProfileConfig
            {
                PreferredMinRange = 0f,
                PreferredMaxRange = config.Robot.EngageRange,
                DashDamageMultiplier = 1f,
                BiteDamageMultiplier = 1f,
                MaximumTargets = 1,
                IncomingDamageMultiplier = 1f,
                CombatBatteryMultiplier = 1f
            };
        }

        public RobotCombatProfileConfig ActiveProfile(RobotState robot)
        {
            if (robot == null || robot.Progression.Specialization == HaetaeSpecialization.Unselected)
                return generalProfile;
            return config.GetHaetaeSpecialization(robot.Progression.Specialization).Combat;
        }

        public RobotCombatDecision Decide(RobotState robot, float targetDistance)
        {
            if (robot == null || robot.IsDestroyed)
                return new RobotCombatDecision
                {
                    Movement = RobotMovementIntent.None,
                    Attack = RobotAttackResult.None
                };

            var profile = ActiveProfile(robot);
            var distance = Math.Max(0f, targetDistance);
            var role = robot.Progression.Specialization;
            var movement = MovementFor(role, profile, distance);
            var attack = RobotAttackResult.None;
            if (!robot.CanAttack || robot.AttackCooldownRemaining > 0f) return new RobotCombatDecision
            {
                Movement = movement,
                Attack = attack
            };

            var kind = RobotAttackKind.None;
            if (role == HaetaeSpecialization.Ranged)
            {
                if (distance <= config.Robot.DetectionRadius) kind = RobotAttackKind.Ranged;
            }
            else if (role == HaetaeSpecialization.Balanced && distance > config.Robot.EngageRange)
            {
                if (distance <= profile.PreferredMaxRange) kind = RobotAttackKind.Ranged;
            }
            else if (distance <= config.Robot.EngageRange)
            {
                kind = !robot.FirstDashUsed && robot.DashCooldownRemaining <= 0f
                    ? RobotAttackKind.Dash
                    : RobotAttackKind.Bite;
            }

            if (kind != RobotAttackKind.None)
            {
                attack = new RobotAttackResult
                {
                    Kind = kind,
                    Range = kind == RobotAttackKind.Ranged
                        ? Math.Max(profile.PreferredMaxRange, config.Robot.EngageRange)
                        : config.Robot.EngageRange,
                    AreaRadius = profile.CleaveRadius,
                    MaximumTargets = Math.Max(1, profile.MaximumTargets),
                    CooldownSeconds = kind == RobotAttackKind.Ranged
                        ? profile.RangedCooldownSeconds
                        : kind == RobotAttackKind.Dash
                            ? config.Robot.BiteCooldownSeconds
                            : config.Robot.BiteCooldownSeconds
                };
            }

            return new RobotCombatDecision { Movement = movement, Attack = attack };
        }

        private RobotMovementIntent MovementFor(
            HaetaeSpecialization role,
            RobotCombatProfileConfig profile,
            float distance)
        {
            if (role == HaetaeSpecialization.Ranged)
            {
                if (distance > profile.PreferredMaxRange) return RobotMovementIntent.Approach;
                if (distance < profile.PreferredMinRange) return RobotMovementIntent.Retreat;
                return RobotMovementIntent.Hold;
            }
            return distance > config.Robot.EngageRange
                ? RobotMovementIntent.Approach
                : RobotMovementIntent.Hold;
        }
    }
}
