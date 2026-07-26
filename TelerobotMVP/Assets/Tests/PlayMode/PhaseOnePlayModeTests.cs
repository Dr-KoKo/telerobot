using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseOnePlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator BootProvidesNorthRoadTwoHaetaeAndExactRadioEvent()
        {
            Assert.That(Game.CurrentPhase, Is.EqualTo(1));
            Assert.That(Game.OpenRoutes, Is.EqualTo(new[] { RouteId.NorthRoad }));
            Assert.That(Game.Robots.Count, Is.EqualTo(2));
            Assert.That(Game.EventHistory.Any(item => item.Name == "radio_event" && item.Payload.ContainsKey("key") && item.Payload["key"] == "radio.game_start"), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuntimeWorldUsesBuildIncludedMaterialTemplate()
        {
            Assert.That(Game.Catalog.runtimeMaterialTemplate, Is.Not.Null);
            Assert.That(Game.Catalog.runtimeMaterialTemplate.shader, Is.Not.Null);

            var ground = GameObject.Find("Ground");
            Assert.That(ground, Is.Not.Null);
            var renderer = ground.GetComponent<Renderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.sharedMaterial, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.shader, Is.EqualTo(Game.Catalog.runtimeMaterialTemplate.shader));
            yield return null;
        }

        [UnityTest]
        public IEnumerator EmptyCompletedWaveImmediatelyStartsPhaseTwoWithoutUpgradeView()
        {
            Game.ClearCurrentWaveForTests();
            yield return null;
            yield return null;
            Assert.That(Game.CurrentPhase, Is.EqualTo(2));
            Assert.That(Game.OpenRoutes, Does.Contain(RouteId.EastAlley));
            Assert.That(GameObject.Find("MVP HUD").GetComponents<MonoBehaviour>()
                .Any(item => item.GetType().Name == "UpgradeSelectionView"), Is.False);
            Assert.That(Game.EventHistory.Any(item => item.Name == "radio_event" &&
                item.Payload.ContainsKey("key") && item.Payload["key"] == "radio.phase_clear"), Is.True);
            Assert.That(Game.EventHistory.Any(item => item.Name == "upgrade_selected"), Is.False);
        }

        [UnityTest]
        public IEnumerator ContinuousSpawningStopsAtAliveCapAndResumesAfterKill()
        {
            foreach (var robot in Game.Robots) robot.enabled = false;
            for (var frame = 0; frame < 20 && Game.AliveZombies.Count < Game.MaxAliveConcurrent; frame++)
            {
                yield return null;
                foreach (var zombie in Game.AliveZombies) zombie.enabled = false;
                Assert.That(Game.AliveZombies.Count, Is.LessThanOrEqualTo(Game.MaxAliveConcurrent));
            }
            Assert.That(Game.AliveZombies.Count, Is.EqualTo(Game.MaxAliveConcurrent));
            Assert.That(Game.SpawnedCount, Is.LessThan(Game.TotalSpawnCount));

            var spawnedBeforeKill = Game.SpawnedCount;
            Game.AliveZombies[0].ReceiveDamage(99999f, "test");
            yield return null;
            foreach (var zombie in Game.AliveZombies) zombie.enabled = false;
            Assert.That(Game.SpawnedCount, Is.GreaterThan(spawnedBeforeKill));
            Assert.That(Game.AliveZombies.Count, Is.LessThanOrEqualTo(Game.MaxAliveConcurrent));
        }

        [UnityTest]
        public IEnumerator SpawnGroupStartsOnDistinctApproachPositions()
        {
            for (var frame = 0; frame < 10 && Game.AliveZombies.Count < 3; frame++) yield return null;
            Assert.That(Game.AliveZombies.Count, Is.GreaterThanOrEqualTo(3));
            var group = Game.AliveZombies.Take(3).ToArray();
            for (var left = 0; left < group.Length; left++)
            for (var right = left + 1; right < group.Length; right++)
            {
                var delta = group[left].transform.position - group[right].transform.position;
                delta.y = 0f;
                Assert.That(delta.magnitude, Is.GreaterThan(0.25f));
            }
            Assert.That(group.Select(item => item.CurrentNavigationPoint.x).Distinct().Count(), Is.GreaterThan(1));
        }

        [UnityTest]
        public IEnumerator NavigationWaypointIsTraversedWithoutRemoteBaseDamage()
        {
            Game.SpawnAllNowForTests();
            foreach (var robot in Game.Robots) robot.enabled = false;
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");

            var waypoint = target.CurrentNavigationPoint;
            target.transform.position = waypoint + Vector3.forward * 1.2f;
            var distanceBefore = Vector3.Distance(target.transform.position, waypoint);
            var baseHealthBefore = Game.BaseState.Health.Current;
            yield return null;

            Assert.That(Game.BaseState.Health.Current, Is.EqualTo(baseHealthBefore));
            Assert.That(Vector3.Distance(target.transform.position, waypoint), Is.LessThan(distanceBefore));
        }

        [UnityTest]
        public IEnumerator HaetaeAutonomouslyDamagesNearbyZombie()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");
            target.enabled = false;

            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            robot.transform.position = Vector3.zero;
            target.transform.position = Vector3.forward * 1.5f;
            Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);
            var healthBefore = target.State.Health.Current;
            for (var frame = 0; frame < 5 && target.State.Health.Current >= healthBefore; frame++) yield return null;

            Assert.That(target.State.Health.Current, Is.LessThan(healthBefore));
        }

        [UnityTest]
        public IEnumerator RifleSkipsPlayerColliderAndDamagesFirstVisibleZombie()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            target.enabled = false;
            Game.PlayerActor.transform.position = new Vector3(10f, 1f, 10f);
            Game.PlayerActor.transform.rotation = Quaternion.identity;
            target.transform.position = new Vector3(10f, 1f, 20f);
            Game.PlayerActor.SnapCameraForTests();
            yield return null;

            var beforeHealth = target.State.Health.Current;
            var beforeAmmo = Game.PlayerState.Ammo.Loaded;
            Game.PlayerActor.FireForTests();

            Assert.That(Game.PlayerState.Ammo.Loaded, Is.EqualTo(beforeAmmo - 1));
            Assert.That(target.State.Health.Current, Is.LessThan(beforeHealth));
            Assert.That(Game.EventHistory.Any(item => item.Name == "player_hit_confirmed"), Is.True);
            Assert.That(Game.PlayerActor.ShotFeedbackCount, Is.EqualTo(1));
            Assert.That(Game.PlayerActor.RecoilMagnitude, Is.GreaterThan(0f));
            Assert.That(Game.PlayerActor.HasCombatAudio, Is.True);
            Assert.That(Game.PlayerActor.LastMuzzleFlash, Is.Not.Null);
            Assert.That(Game.PlayerActor.LastImpactEffect, Is.Not.Null);
            Assert.That(Game.PlayerActor.HitSoundCount, Is.EqualTo(1));
            Assert.That(target.HitFlashActive, Is.True);
            var hud = Object.FindFirstObjectByType<Telerobot.Game.Runtime.CombatHud>();
            Assert.That(hud.HitFeedbackActive, Is.True);
        }

        [UnityTest]
        public IEnumerator BaseDestroyedImmediatelyEndsInDefeat()
        {
            Game.DamageBase(5000f);
            yield return null;
            Assert.That(Game.Session.Result, Is.EqualTo(GameResult.Defeat));
            Assert.That(Game.Session.DefeatReason, Is.EqualTo(DefeatReason.BaseDestroyed));
        }
    }
}
