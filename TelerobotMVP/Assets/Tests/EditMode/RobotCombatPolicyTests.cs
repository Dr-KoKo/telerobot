using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class RobotCombatPolicyTests
    {
        private GameplayConfig config;
        private RobotCombatPolicy policy;
        private RobotState robot;

        [SetUp]
        public void SetUp()
        {
            config = TestConfigFactory.Create();
            policy = new RobotCombatPolicy(config);
            robot = new RobotState("haetae-1", config.Robot.MaxHealth, config.Battery.Maximum);
        }

        [Test]
        public void GeneralPreservesApproachDashAndBiteBaseline()
        {
            var far = policy.Decide(robot, 5f);
            Assert.That(far.Movement, Is.EqualTo(RobotMovementIntent.Approach));
            Assert.That(far.Attack.Kind, Is.EqualTo(RobotAttackKind.None));

            var close = policy.Decide(robot, config.Robot.EngageRange);
            Assert.That(close.Movement, Is.EqualTo(RobotMovementIntent.Hold));
            Assert.That(close.Attack.Kind, Is.EqualTo(RobotAttackKind.Dash));
            robot.FirstDashUsed = true;
            Assert.That(policy.Decide(robot, 1f).Attack.Kind, Is.EqualTo(RobotAttackKind.Bite));
        }

        [Test]
        public void MeleeApproachesAndUsesThreeTargetCleaveProfile()
        {
            Select(HaetaeSpecialization.Melee);
            var far = policy.Decide(robot, 4f);
            var close = policy.Decide(robot, 1.5f);
            var profile = policy.ActiveProfile(robot);

            Assert.That(far.Movement, Is.EqualTo(RobotMovementIntent.Approach));
            Assert.That(close.Attack.Kind, Is.EqualTo(RobotAttackKind.Dash));
            Assert.That(profile.CleaveRadius, Is.EqualTo(2.5f));
            Assert.That(profile.MaximumTargets, Is.EqualTo(3));
            Assert.That(profile.IncomingDamageMultiplier, Is.EqualTo(0.7f));
            Assert.That(profile.CombatBatteryMultiplier, Is.EqualTo(1.2f));
        }

        [Test]
        public void RangedMaintainsSixToTwelveMetersAndNeverRequestsDash()
        {
            Select(HaetaeSpecialization.Ranged);
            Assert.That(policy.Decide(robot, 14f).Movement, Is.EqualTo(RobotMovementIntent.Approach));
            Assert.That(policy.Decide(robot, 8f).Movement, Is.EqualTo(RobotMovementIntent.Hold));
            var close = policy.Decide(robot, 3f);
            Assert.That(close.Movement, Is.EqualTo(RobotMovementIntent.Retreat));
            Assert.That(close.Attack.Kind, Is.EqualTo(RobotAttackKind.Ranged));
            Assert.That(policy.ActiveProfile(robot).RangedDamage, Is.EqualTo(200f));
            Assert.That(policy.ActiveProfile(robot).RangedCooldownSeconds, Is.EqualTo(0.35f));
        }

        [Test]
        public void BalancedFiresWhileApproachingThenSwitchesAtChassisMeleeRange()
        {
            Select(HaetaeSpecialization.Balanced);
            var far = policy.Decide(robot, 6f);
            var close = policy.Decide(robot, config.Robot.EngageRange);

            Assert.That(far.Movement, Is.EqualTo(RobotMovementIntent.Approach));
            Assert.That(far.Attack.Kind, Is.EqualTo(RobotAttackKind.Ranged));
            Assert.That(close.Attack.Kind, Is.EqualTo(RobotAttackKind.Dash));
            Assert.That(policy.ActiveProfile(robot).DashDamageMultiplier, Is.EqualTo(2.5f));
            Assert.That(policy.ActiveProfile(robot).BiteDamageMultiplier, Is.EqualTo(2.5f));
        }

        [Test]
        public void DisabledDestroyedAndCooldownStatesCannotAttack()
        {
            Select(HaetaeSpecialization.Ranged);
            robot.AttackCooldownRemaining = 0.2f;
            Assert.That(policy.Decide(robot, 8f).Attack.Kind, Is.EqualTo(RobotAttackKind.None));
            robot.AttackCooldownRemaining = 0f;
            robot.Mode = RobotMode.Disabled;
            Assert.That(policy.Decide(robot, 8f).Attack.Kind, Is.EqualTo(RobotAttackKind.None));
            robot.Mode = RobotMode.Destroyed;
            Assert.That(policy.Decide(robot, 8f).Movement, Is.EqualTo(RobotMovementIntent.None));
        }

        [Test]
        public void AttackSpeedRankReducesEveryAttackCooldownWithTheSameFloor()
        {
            Select(HaetaeSpecialization.Ranged);
            robot.Progression.AttackSpeedRank = 1;
            var attacks = new RobotAttackSystem(config.Robot);
            var decision = policy.Decide(robot, 8f);

            var attack = attacks.Advance(robot, "zombie-1", decision, policy.ActiveProfile(robot), 1f,
                config.HaetaeProgression.AttackCooldownMultiplier(robot.Progression));

            Assert.That(attack.Kind, Is.EqualTo(RobotAttackKind.Ranged));
            Assert.That(attack.CooldownSeconds, Is.EqualTo(0.315f).Within(0.0001f));
            Assert.That(robot.AttackCooldownRemaining, Is.EqualTo(0.315f).Within(0.0001f));

            robot.AttackCooldownRemaining = 0f;
            robot.DashCooldownRemaining = 0f;
            robot.Progression.Specialization = HaetaeSpecialization.Melee;
            robot.FirstDashUsed = false;
            robot.CurrentTargetId = null;
            decision = policy.Decide(robot, 1f);
            attack = attacks.Advance(robot, "zombie-dash", decision, policy.ActiveProfile(robot), 1f,
                config.HaetaeProgression.AttackCooldownMultiplier(robot.Progression));

            Assert.That(attack.Kind, Is.EqualTo(RobotAttackKind.Dash));
            Assert.That(robot.DashCooldownRemaining, Is.EqualTo(2.7f).Within(0.0001f));

            robot.AttackCooldownRemaining = 0f;
            robot.FirstDashUsed = true;
            robot.Progression.AttackSpeedRank = 10;
            decision = policy.Decide(robot, 1f);
            attack = attacks.Advance(robot, "zombie-2", decision, policy.ActiveProfile(robot), 1f,
                config.HaetaeProgression.AttackCooldownMultiplier(robot.Progression));

            Assert.That(attack.Kind, Is.EqualTo(RobotAttackKind.Bite));
            Assert.That(attack.CooldownSeconds, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(robot.AttackCooldownRemaining, Is.EqualTo(0.3f).Within(0.0001f));
        }

        private void Select(HaetaeSpecialization specialization)
        {
            robot.Progression.Level = 2;
            robot.Progression.Experience = config.HaetaeProgression.ExperiencePerLevel;
            robot.Progression.Specialization = specialization;
        }
    }
}
