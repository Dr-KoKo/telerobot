using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseThreePlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator SecondRewardDeploysMedicalAndRippersOnThreeRoutesThenVictory()
        {
            yield return ClearAndChooseFirstUpgrade();
            yield return ClearAndChooseFirstUpgrade();
            Assert.That(Game.CurrentPhase, Is.EqualTo(3));
            Assert.That(Game.OpenRoutes.Count, Is.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<MedicalRobotActor>(), Is.Not.Null);
            Game.SpawnAllNowForTests();
            Assert.That(Game.AliveZombies.Exists(item => item.Type == ZombieType.Ripper), Is.True);
            Game.ClearCurrentWaveForTests();
            yield return null;
            yield return null;
            Assert.That(Game.Session.Result, Is.EqualTo(GameResult.Victory));
        }

        [UnityTest]
        public IEnumerator DestroyedMedicalRobotDisablesZoneAndDoesNotRegenerate()
        {
            yield return ClearAndChooseFirstUpgrade();
            yield return ClearAndChooseFirstUpgrade();
            var medical = Game.MedicalActor;
            Assert.That(medical, Is.Not.Null);
            medical.ReceiveDamage(medical.CurrentHealth);
            yield return null;

            Assert.That(medical == null || !medical.IsZoneActive, Is.True);
            var destroyed = 0;
            var disabled = 0;
            foreach (var gameEvent in Game.EventHistory)
            {
                if (gameEvent.Name == "medical_robot_destroyed") destroyed++;
                if (gameEvent.Name == "medical_zone_disabled") disabled++;
            }
            Assert.That(destroyed, Is.EqualTo(1));
            Assert.That(disabled, Is.EqualTo(1));
            yield return null;
            Assert.That(Game.MedicalActor == null, Is.True);
        }

        [UnityTest]
        public IEnumerator SideRouteEmergencyBarriersRotateAcrossTheirApproachDirections()
        {
            Game.Modifiers.EmergencyBarrier = true;
            yield return ClearAndChooseFirstUpgrade();
            yield return ClearAndChooseFirstUpgrade();

            foreach (var routeId in new[] { RouteId.EastAlley, RouteId.SouthTunnel })
            {
                Assert.That(Game.TryGetBarrier(routeId, out var barrier), Is.True);
                var route = Game.Catalog.Route(routeId);
                var position = route.waypoints[route.waypoints.Length - 2];
                var inward = route.waypoints[route.waypoints.Length - 1] - position;
                inward.y = 0f;
                inward.Normalize();

                Assert.That(Vector3.Dot(barrier.transform.forward, inward), Is.GreaterThan(0.999f));
                Assert.That(Mathf.Abs(Vector3.Dot(barrier.transform.right, inward)), Is.LessThan(0.001f));
            }
        }
    }
}
