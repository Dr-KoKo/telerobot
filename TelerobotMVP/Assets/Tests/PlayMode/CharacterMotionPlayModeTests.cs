using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class CharacterMotionPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator LiveCharacters_HaveOneBoundRoleProfile()
        {
            var zombies = new[]
            {
                Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad),
                Game.SpawnZombieForTests(ZombieType.Bruiser, RouteId.EastAlley),
                Game.SpawnZombieForTests(ZombieType.Ripper, RouteId.SouthTunnel)
            };
            yield return null;

            var actors = Game.Robots.Select(item => item.gameObject)
                .Concat(zombies.Select(item => item.gameObject)).ToArray();
            Assert.That(actors.All(actor =>
                actor.GetComponents<CharacterMotionDriver>().Length == 1), Is.True);
            Assert.That(actors.All(actor =>
                actor.GetComponent<CharacterMotionDriver>().VisualRoot.name ==
                LowPolyModelFactory.VisualRootName), Is.True);
            Assert.That(zombies.Select(item =>
                    item.GetComponent<CharacterMotionDriver>().ProfileId)
                .Distinct().Count(), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator DisplacementSelectsLocomotionWithoutColliderMutation()
        {
            var zombie = Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad);
            yield return null;
            var collider = zombie.GetComponent<CapsuleCollider>();
            var center = collider.center;
            var radius = collider.radius;
            var height = collider.height;
            zombie.transform.position += Vector3.forward * 0.25f;
            yield return null;

            Assert.That(zombie.GetComponent<CharacterMotionDriver>().State,
                Is.EqualTo(CharacterMotionState.Locomotion));
            Assert.That(collider.center, Is.EqualTo(center));
            Assert.That(collider.radius, Is.EqualTo(radius));
            Assert.That(collider.height, Is.EqualTo(height));
        }

        [UnityTest]
        public IEnumerator ZombieHitAndDeathTriggersDrivePresentationStates()
        {
            var zombie = Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad);
            yield return null;
            var driver = zombie.GetComponent<CharacterMotionDriver>();

            zombie.ReceiveDamage(1f, DamageSource.Player("motion-test"));
            yield return null;
            yield return null;
            Assert.That(driver.State, Is.EqualTo(CharacterMotionState.Hit));

            zombie.ReceiveDamage(zombie.State.Health.Current + 1f,
                DamageSource.Player("motion-test"));
            yield return null;
            yield return null;
            Assert.That(driver.State, Is.EqualTo(CharacterMotionState.Death));
            Assert.That(driver.NormalizedPhase, Is.GreaterThanOrEqualTo(0f));
        }

        [UnityTest]
        public IEnumerator HaetaeAttackEventTriggersMotionAndKeepsDamageFlow()
        {
            var robot = Game.Robots[0];
            var zombie = Game.SpawnZombieForTests(ZombieType.Bruiser, RouteId.NorthRoad);
            zombie.CompleteNavigationForTests();
            zombie.transform.position = robot.transform.position + robot.transform.forward * 1.2f;
            var healthBefore = zombie.State.Health.Current;
            var driver = robot.GetComponent<CharacterMotionDriver>();
            var deadline = Time.time + 4f;

            while (Time.time < deadline &&
                   (robot.LastAttackKind == RobotAttackKind.None ||
                    driver.State != CharacterMotionState.Attack))
                yield return null;

            Assert.That(robot.LastAttackKind, Is.Not.EqualTo(RobotAttackKind.None));
            Assert.That(driver.State, Is.EqualTo(CharacterMotionState.Attack));
            Assert.That(zombie.State.Health.Current, Is.LessThan(healthBefore));
        }

        [UnityTest]
        public IEnumerator AttackPhasePausesWithGameTimeAndInstancesRemainIndependent()
        {
            var first = Game.Robots[0].GetComponent<CharacterMotionDriver>();
            var second = Game.Robots[1].GetComponent<CharacterMotionDriver>();
            first.TriggerAttack(CharacterAttackMotion.Melee);
            Time.timeScale = 0f;
            yield return null;
            var frozenPhase = first.NormalizedPhase;
            yield return null;
            yield return null;

            Assert.That(first.NormalizedPhase, Is.EqualTo(frozenPhase));
            Assert.That(first.State, Is.EqualTo(CharacterMotionState.Attack));
            Assert.That(second.State, Is.Not.EqualTo(CharacterMotionState.Attack));
            Time.timeScale = 1f;
            yield return null;
            yield return null;
            Assert.That(first.NormalizedPhase, Is.GreaterThan(frozenPhase));
        }

        [UnityTearDown]
        public IEnumerator RestoreTimeScale()
        {
            Time.timeScale = 1f;
            yield return null;
        }
    }
}
