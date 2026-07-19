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
        public IEnumerator EmptyCompletedWaveOpensUpgradeReward()
        {
            Game.ClearCurrentWaveForTests();
            yield return null;
            yield return null;
            Assert.That(Game.UpgradeOpen, Is.True);
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
