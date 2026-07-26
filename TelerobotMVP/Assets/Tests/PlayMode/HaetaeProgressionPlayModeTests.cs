using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeProgressionPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator TypedDamageAwardsOnlyTheContributingHaetaeBeforeKillEvent()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            target.enabled = false;
            var first = Game.Robots[0];
            var second = Game.Robots[1];

            target.ReceiveDamage(1f, DamageSource.Haetae(first.State.Id));
            target.ReceiveDamage(99999f, DamageSource.Player("player"));
            yield return null;

            Assert.That(first.State.Progression.Experience, Is.EqualTo(5));
            Assert.That(second.State.Progression.Experience, Is.Zero);
            var progressionIndex = Game.EventHistory.ToList().FindIndex(item => item.Name == "haetae_xp_gained");
            var killIndex = Game.EventHistory.ToList().FindIndex(item => item.Name == "zombie_killed" &&
                item.Payload["zombieId"] == target.State.Id);
            Assert.That(progressionIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(killIndex, Is.GreaterThan(progressionIndex));
        }

        [UnityTest]
        public IEnumerator SharedTypedDamageAwardsBothRobotsOnce()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            target.enabled = false;

            target.ReceiveDamage(1f, DamageSource.Haetae(Game.Robots[1].State.Id));
            target.ReceiveDamage(1f, DamageSource.Haetae(Game.Robots[0].State.Id));
            target.ReceiveDamage(1f, DamageSource.Haetae(Game.Robots[0].State.Id));
            target.ReceiveDamage(99999f, DamageSource.Player("player"));
            yield return null;

            Assert.That(Game.Robots.Select(item => item.State.Progression.Experience),
                Is.EqualTo(new[] { 5, 5 }));
            var awards = Game.EventHistory.Where(item => item.Name == "haetae_xp_gained" &&
                item.Payload["zombieId"] == target.State.Id).ToArray();
            Assert.That(awards.Select(item => item.Payload["robotId"]),
                Is.EqualTo(new[] { Game.Robots[0].State.Id, Game.Robots[1].State.Id }));
        }

        [UnityTest]
        public IEnumerator LevelThreeKeepsExperienceAndDoesNotRepeatSpecializationReady()
        {
            Game.SpawnAllNowForTests();
            var targets = Game.AliveZombies.Where(item => item.Type == ZombieType.Runner).Take(2).ToArray();
            Assert.That(targets, Has.Length.EqualTo(2));
            foreach (var target in targets) target.enabled = false;
            var robot = Game.Robots[0].State;
            robot.Progression.Experience = 70;

            targets[0].ReceiveDamage(1f, DamageSource.Haetae(robot.Id));
            targets[0].ReceiveDamage(99999f, DamageSource.Player("player"));
            yield return null;

            Assert.That(robot.Progression.Level, Is.EqualTo(2));
            Assert.That(robot.Progression.Experience, Is.EqualTo(75));
            Assert.That(Game.SelectHaetaeSpecialization(robot.Id, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.Selected));

            robot.Progression.Experience = 145;
            targets[1].ReceiveDamage(1f, DamageSource.Haetae(robot.Id));
            targets[1].ReceiveDamage(99999f, DamageSource.Player("player"));
            yield return null;

            Assert.That(robot.Progression.Level, Is.EqualTo(3));
            Assert.That(robot.Progression.Experience, Is.EqualTo(150));
            Assert.That(robot.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Melee));
            Assert.That(Game.EventHistory.Count(item => item.Name == "haetae_specialization_ready" &&
                item.Payload["robotId"] == robot.Id), Is.EqualTo(1));
            Assert.That(Game.EventHistory.Count(item => item.Name == "haetae_level_reached" &&
                item.Payload["robotId"] == robot.Id), Is.EqualTo(2));
        }
    }
}
