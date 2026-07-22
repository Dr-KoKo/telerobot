using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PlayerExperiencePlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator PerspectiveToggleSwitchesFovBodyAndEyePosition()
        {
            var player = Game.PlayerActor;
            Assert.That(player.Perspective, Is.EqualTo(CameraPerspective.ThirdPerson));
            Assert.That(player.IsBodyVisible, Is.True);

            player.TogglePerspective();
            yield return null;

            Assert.That(player.Perspective, Is.EqualTo(CameraPerspective.FirstPerson));
            Assert.That(player.IsBodyVisible, Is.False);
            Assert.That(player.ViewCamera.fieldOfView, Is.EqualTo(Game.Config.Game.FirstPersonFieldOfView));
            var expectedEye = player.transform.position + Vector3.up * Game.Config.Game.FirstPersonEyeHeight;
            Assert.That(Vector3.Distance(player.ViewCamera.transform.position, expectedEye), Is.LessThan(0.01f));
            Assert.That(Game.EventHistory.Any(item => item.Name == "camera_perspective_changed"), Is.True);
        }

        [UnityTest]
        public IEnumerator ThirdPersonCameraStopsInFrontOfWall()
        {
            var player = Game.PlayerActor;
            player.transform.position = new Vector3(10f, 1f, 10f);
            var anchor = player.transform.position + Vector3.up * 0.55f;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Camera Collision Test Wall";
            wall.transform.position = anchor + Vector3.back * 2f;
            wall.transform.localScale = new Vector3(4f, 4f, 0.4f);
            Physics.SyncTransforms();

            player.SnapCameraForTests();
            var distance = Vector3.Distance(anchor, player.ViewCamera.transform.position);
            Object.Destroy(wall);
            yield return null;

            Assert.That(distance, Is.LessThan(3f));
        }

        [UnityTest]
        public IEnumerator GroundedPlayerCanJumpAndEmitsTelemetry()
        {
            var player = Game.PlayerActor;
            player.transform.position = new Vector3(10f, 0.85f, 10f);
            Physics.SyncTransforms();
            player.CharacterForTests.Move(Vector3.down * 0.1f);
            Assert.That(player.IsGrounded, Is.True);

            player.RequestJumpForTests();
            yield return null;

            Assert.That(player.VerticalVelocity, Is.GreaterThan(0f));
            Assert.That(Game.EventHistory.Any(item => item.Name == "player_jumped"), Is.True);
        }

        [UnityTest]
        public IEnumerator PauseFreezesSessionAndRestoresTimeScale()
        {
            var elapsed = Game.Session.ElapsedTime;
            Game.SetPaused(true);
            var blockedWhilePaused = Game.InputBlocked;
            var pausedScale = Time.timeScale;
            yield return null;
            var remainedFrozen = Game.Session.ElapsedTime;
            Game.SetPaused(false);

            Assert.That(blockedWhilePaused, Is.True);
            Assert.That(pausedScale, Is.EqualTo(0f));
            Assert.That(remainedFrozen, Is.EqualTo(elapsed));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator PauseSettingsCloseBackToPauseBeforeSessionResumes()
        {
            Game.SetPaused(true);
            Game.OpenSettings();

            Assert.That(Game.IsPaused, Is.True);
            Assert.That(Game.SettingsOpen, Is.True);

            Game.TogglePause();
            yield return null;

            Assert.That(Game.IsPaused, Is.True);
            Assert.That(Game.SettingsOpen, Is.False);
            Game.SetPaused(false);
        }

        [UnityTest]
        public IEnumerator SprintInputUsesConfiguredMovementMultiplier()
        {
            var player = Game.PlayerActor;
            player.SetInputForTests(new StubPlayerInput
            {
                Frame = new PlayerInputFrame { Move = new Float2(0f, 1f), SprintHeld = true }
            });

            yield return null;

            Assert.That(player.CurrentPlanarSpeed,
                Is.EqualTo(Game.Config.Game.PlayerMoveSpeed * Game.Config.Game.SprintMultiplier).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator HeldFireUsesConfiguredCadenceAndBoundedRandomRecoil()
        {
            var player = Game.PlayerActor;
            var input = new StubPlayerInput
            {
                Frame = new PlayerInputFrame { FireHeld = true }
            };
            player.SetInputForTests(input);
            var ammoBefore = player.State.Ammo.Loaded;
            var observedFeedback = player.ShotFeedbackCount;
            var pitchSamples = new List<float>();
            var yawSamples = new List<float>();
            var endTime = Time.time + Game.Config.Weapon.FireIntervalSeconds * 3.5f;

            while (Time.time < endTime)
            {
                yield return null;
                if (player.ShotFeedbackCount == observedFeedback) continue;
                observedFeedback = player.ShotFeedbackCount;
                pitchSamples.Add(player.LastRecoilPitchDegrees);
                yawSamples.Add(player.LastRecoilYawDegrees);
            }
            input.Frame = default;

            var roundsFired = ammoBefore - player.State.Ammo.Loaded;
            Assert.That(roundsFired, Is.InRange(3, 5));
            Assert.That(pitchSamples.Count, Is.EqualTo(roundsFired));
            Assert.That(pitchSamples.All(value => value >= Game.Catalog.weapon.recoilPitchMinimumDegrees &&
                value <= Game.Catalog.weapon.recoilPitchMaximumDegrees), Is.True);
            Assert.That(yawSamples.All(value => Mathf.Abs(value) <=
                Game.Catalog.weapon.recoilYawMaximumDegrees), Is.True);
            Assert.That(pitchSamples.Select(value => Mathf.RoundToInt(value * 1000f)).Distinct().Count(),
                Is.GreaterThan(1));
            Assert.That(yawSamples.Select(value => Mathf.RoundToInt(value * 1000f)).Distinct().Count(),
                Is.GreaterThan(1));
        }

        [UnityTest]
        public IEnumerator PlayerDamageReportsDirectionAndActivatesHudFeedback()
        {
            var player = Game.PlayerActor;
            var hud = Object.FindFirstObjectByType<Telerobot.Game.Runtime.CombatHud>();
            var healthBefore = player.State.Health.Current;
            var source = player.transform.position + player.transform.right * 3f;

            player.ReceiveDamage(5f, source);
            yield return null;

            Assert.That(player.State.Health.Current, Is.EqualTo(healthBefore - 5f));
            Assert.That(hud.DamageFeedbackActive, Is.True);
            Assert.That(hud.LastDamageAngle, Is.EqualTo(90f).Within(0.2f));
            Assert.That(Game.EventHistory.Any(item => item.Name == "player_damaged" &&
                item.Payload.ContainsKey("directionAngle")), Is.True);
        }

        [UnityTest]
        public IEnumerator NearbySupplyAndLowAmmoExposeActionableHudState()
        {
            var player = Game.PlayerActor;
            var hud = Object.FindFirstObjectByType<Telerobot.Game.Runtime.CombatHud>();
            player.transform.position = Game.ToVector(Game.Config.World.SafeSupply);
            Game.PlayerState.Ammo.Loaded = Game.Catalog.hud.lowAmmoThreshold;
            Game.PlayerState.Ammo.Reserve = 0;
            Physics.SyncTransforms();

            Assert.That(hud.SupplyPromptActive, Is.True);
            Assert.That(hud.LowAmmoWarningActive, Is.True);
            Assert.That(Game.TryGetNearbySupply(player.transform.position, out var kind), Is.True);
            Assert.That(kind, Is.EqualTo(SupplyKind.Safe));
            Assert.That(Game.TryResupply(player.transform.position), Is.True);
            Assert.That(Game.IsResupplying, Is.True);
            Assert.That(Game.PlayerState.Ammo.Reserve, Is.Zero);
            yield return new WaitForSeconds(Game.Config.Ammo.ResupplyUseSeconds + 0.1f);
            Assert.That(Game.PlayerState.Ammo.Reserve, Is.EqualTo(Game.Config.Ammo.ReserveAmmoMax));
            Assert.That(Game.EventHistory.Any(item => item.Name == "ammo_resupplied" &&
                item.Payload["supplyKind"] == SupplyKind.Safe.ToString()), Is.True);

            Game.PlayerState.Ammo.Reserve = 0;
            player.transform.position = Game.ToVector(Game.Config.World.RiskySupply);
            player.enabled = false;
            Game.PlayerState.Health.Maximum = 10000f;
            Game.PlayerState.Health.Current = 10000f;
            Game.BaseState.Health.Maximum = 10000f;
            Game.BaseState.Health.Current = 10000f;
            Physics.SyncTransforms();
            Assert.That(Game.TryResupply(player.transform.position), Is.True);
            yield return new WaitForSeconds(Game.Config.Ammo.ResupplyUseSeconds + 0.1f);
            Assert.That(Game.EventHistory.Any(item => item.Name == "ammo_resupplied" &&
                item.Payload["supplyKind"] == SupplyKind.Risky.ToString()), Is.True);
        }

        [UnityTest]
        public IEnumerator SafeSupplyUsesPlanarRangeAndToleratesSmallBoundaryDrift()
        {
            var player = Game.PlayerActor;
            player.enabled = false;
            Game.PlayerState.Ammo.Reserve = 0;
            var supply = Game.ToVector(Game.Config.World.SafeSupply);
            player.transform.position = supply + new Vector3(Game.Config.World.SupplyInteractionRadius - 0.05f, 5f, 0f);
            Physics.SyncTransforms();

            Assert.That(Game.TryGetNearbySupply(player.transform.position, out var kind), Is.True);
            Assert.That(kind, Is.EqualTo(SupplyKind.Safe));
            Assert.That(Game.TryResupply(player.transform.position), Is.True);

            player.transform.position = supply + new Vector3(
                Game.Config.World.SupplyInteractionRadius + Game.Config.World.SupplyExitTolerance * 0.5f, 5f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(Game.Config.Ammo.ResupplyUseSeconds + 0.1f);

            Assert.That(Game.PlayerState.Ammo.Reserve, Is.EqualTo(Game.Config.Ammo.ReserveAmmoMax));
            Assert.That(Game.EventHistory.Any(item => item.Name == "ammo_resupplied" &&
                item.Payload["supplyKind"] == SupplyKind.Safe.ToString()), Is.True);
        }

        [UnityTest]
        public IEnumerator HeadshotUsesDistinctFeedbackAndLethalHitShowsDeathEffect()
        {
            Game.SpawnAllNowForTests();
            var player = Game.PlayerActor;
            var target = Game.AliveZombies.First(item => item.Type == ZombieType.Runner);
            target.enabled = false;
            player.transform.position = new Vector3(12f, 1f, 12f);
            player.TogglePerspective();
            player.SnapCameraForTests();
            var aimPoint = player.ViewCamera.transform.position + player.ViewCamera.transform.forward * 8f;
            target.transform.position = aimPoint - Vector3.up * (target.VisualHeight * 0.35f);
            Physics.SyncTransforms();
            yield return null;

            player.FireForTests();
            Assert.That(player.LastHitSoundRegion, Is.EqualTo(HitRegion.Head));
            Assert.That(target.HitFlashActive, Is.True);

            player.FireForTests();
            Assert.That(target.DeathFeedbackActive, Is.True);
            Assert.That(Game.AliveZombies.Contains(target), Is.False);
            Assert.That(GameObject.Find("Zombie Death"), Is.Not.Null);
            Assert.That(player.HitSoundCount, Is.EqualTo(2));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReloadingExposesAdvancingHudProgress()
        {
            var hud = Object.FindFirstObjectByType<Telerobot.Game.Runtime.CombatHud>();
            Game.PlayerState.Ammo.Loaded = 0;
            Game.PlayerState.Ammo.Reserve = 30;
            Assert.That(CombatRules.BeginReload(Game.PlayerState.Ammo, Game.Config.Weapon.ReloadSeconds), Is.True);
            Assert.That(hud.ReloadProgressVisible, Is.True);
            var initialProgress = hud.ReloadProgress;

            yield return new WaitForSeconds(0.2f);

            Assert.That(hud.ReloadProgressVisible, Is.True);
            Assert.That(hud.ReloadProgress, Is.GreaterThan(initialProgress));
            Assert.That(hud.ReloadProgress, Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator KoreanHudRowsProvideVerticalGlyphPadding()
        {
            var hud = Object.FindFirstObjectByType<Telerobot.Game.Runtime.CombatHud>();

            yield return null;

            Assert.That(hud.TextClippingMode, Is.EqualTo(TextClipping.Overflow));
            Assert.That(hud.BodyLineHeight, Is.GreaterThanOrEqualTo(31f));
            Assert.That(hud.HeaderLineHeight, Is.GreaterThanOrEqualTo(39f));
            Assert.That(hud.StatusPanelHeight,
                Is.GreaterThanOrEqualTo(hud.HeaderLineHeight + hud.BodyLineHeight *
                    (3f + Game.Robots.Count) + 20f));
        }

        private sealed class StubPlayerInput : IPlayerInput
        {
            public PlayerInputFrame Frame;

            public PlayerInputFrame ReadFrame()
            {
                return Frame;
            }
        }
    }
}
