using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeCombatRolePlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator RangedRoleFiresTracerFromPreferredBandWithoutDash()
        {
            Game.SpawnAllNowForTests();
            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            ReadyAndSelect(robot, HaetaeSpecialization.Ranged);
            var target = IsolateRunner();
            robot.transform.position = Vector3.zero;
            target.transform.position = Vector3.forward * 8f;
            target.State.Health.Maximum = 500f;
            target.State.Health.Current = 500f;
            Physics.SyncTransforms();

            for (var frame = 0; frame < 5 && robot.LastAttackKind == RobotAttackKind.None; frame++)
                yield return null;

            Assert.That(robot.LastMovementIntent, Is.EqualTo(RobotMovementIntent.Hold));
            Assert.That(robot.LastAttackKind, Is.EqualTo(RobotAttackKind.Ranged));
            Assert.That(robot.State.FirstDashUsed, Is.False);
            Assert.That(robot.LastAttackCue, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MeleeRoleCleavesAtMostThreeSameRouteTargets()
        {
            Game.SpawnAllNowForTests();
            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            ReadyAndSelect(robot, HaetaeSpecialization.Melee);
            var targets = Game.AliveZombies.Where(item => item.Type == ZombieType.Runner).Take(4).ToArray();
            foreach (var other in Game.AliveZombies.ToArray())
                if (!targets.Contains(other)) other.ReceiveDamage(99999f, "test");
            robot.transform.position = Vector3.zero;
            for (var index = 0; index < targets.Length; index++)
            {
                targets[index].enabled = false;
                targets[index].State.Health.Maximum = 500f;
                targets[index].State.Health.Current = 500f;
                targets[index].transform.position = Vector3.forward * 1.4f + Vector3.right * (index * 0.35f);
            }
            Physics.SyncTransforms();
            yield return null;

            Assert.That(targets.Count(item => item.State.Health.Current < 500f), Is.EqualTo(3));
            Assert.That(robot.LastAttackKind, Is.EqualTo(RobotAttackKind.Dash));
        }

        [UnityTest]
        public IEnumerator RoleMultipliersPreserveRipperDrainDestroyAndRestore()
        {
            var robot = Game.Robots[0];
            ReadyAndSelect(robot, HaetaeSpecialization.Melee);
            var healthBefore = robot.State.Health.Current;
            var batteryBefore = robot.State.Battery;

            robot.ReceiveZombieHit(100f, true);
            var incomingMultiplier = Game.Config.GetHaetaeSpecialization(HaetaeSpecialization.Melee)
                .Combat.IncomingDamageMultiplier;
            Assert.That(robot.State.Health.Current,
                Is.EqualTo(healthBefore - 100f * incomingMultiplier).Within(0.01f));
            Assert.That(robot.State.Battery, Is.EqualTo(batteryBefore - Game.Config.Battery.RipperHitDrain).Within(0.01f));

            robot.ReceiveZombieHit(99999f, false);
            Assert.That(robot.State.IsDestroyed, Is.True);
            robot.RestoreForPhaseStart();
            yield return null;

            Assert.That(robot.State.IsDestroyed, Is.False);
            Assert.That(robot.State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Melee));
        }

        private ZombieActor IsolateRunner()
        {
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");
            target.enabled = false;
            return target;
        }

        private void ReadyAndSelect(HaetaeRobotActor robot, HaetaeSpecialization role)
        {
            robot.State.Progression.Level = 2;
            robot.State.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel;
            Assert.That(Game.SelectHaetaeSpecialization(robot.State.Id, role),
                Is.EqualTo(SpecializationSelectionResult.Selected));
        }
    }
}
