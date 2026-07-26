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
    public sealed class VisualPresentationPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator WorldBuildsThreeDistinctRouteLandmarksAndInteractables()
        {
            yield return null;
            Assert.That(Game.Catalog.visualTheme, Is.Not.Null);
            Assert.That(Game.Catalog.designAssets, Is.Not.Null);
            var landmarks = Object.FindObjectsByType<WorldLandmarkMarker>(FindObjectsSortMode.None);
            var routeLandmarks = landmarks.Where(item =>
                item.role == PresentationRole.NorthRoute ||
                item.role == PresentationRole.EastRoute ||
                item.role == PresentationRole.SouthRoute).ToArray();

            Assert.That(routeLandmarks.Select(item => item.shapeSignature).Distinct().Count(), Is.EqualTo(3));
            Assert.That(landmarks.Any(item => item.role == PresentationRole.CentralBase), Is.True);
            Assert.That(landmarks.Any(item => item.role == PresentationRole.ChargingStation), Is.True);
            Assert.That(landmarks.Any(item => item.role == PresentationRole.SafeSupply), Is.True);
            Assert.That(landmarks.Any(item => item.role == PresentationRole.RiskySupply), Is.True);
            Assert.That(Game.BaseTransform.GetComponent<Collider>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CentralBaseBlocksPlayerMovement()
        {
            yield return null;
            var blocker = Game.BaseTransform.GetComponent<BoxCollider>();
            Assert.That(blocker, Is.Not.Null);
            Assert.That(blocker.enabled, Is.True);
            Assert.That(blocker.isTrigger, Is.False);

            var player = Game.PlayerActor;
            var start = Game.BaseTransform.position + new Vector3(0f, 1f, -5.2f);
            player.transform.position = start;
            Physics.SyncTransforms();
            player.CharacterForTests.Move(Vector3.forward * 3f);
            Physics.SyncTransforms();

            Assert.That(player.transform.position.z,
                Is.LessThanOrEqualTo(blocker.bounds.min.z - player.CharacterForTests.radius + 0.08f));
        }

        [UnityTest]
        public IEnumerator ZombiesAttackFromDistributedPositionsOutsideCentralBase()
        {
            Game.SetAcceleratedSpawningForTests(false);
            var attackers = Enumerable.Range(0, 6)
                .Select(_ => Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad))
                .ToArray();
            for (var index = 0; index < attackers.Length; index++)
            {
                attackers[index].CompleteNavigationForTests();
                attackers[index].transform.position = Game.BaseTransform.position +
                    new Vector3((index - 2.5f) * 0.35f, 1f, 8f);
            }
            Physics.SyncTransforms();

            var healthBefore = Game.BaseState.Health.Current;
            var deadline = Time.time + 3f;
            while (Time.time < deadline && Game.BaseState.Health.Current >= healthBefore)
                yield return null;

            var blocker = Game.BaseTransform.GetComponent<BoxCollider>();
            Assert.That(Game.BaseState.Health.Current, Is.LessThan(healthBefore),
                "Perimeter attackers must still damage the base.");
            Assert.That(attackers.All(item => item != null && !blocker.bounds.Contains(item.transform.position)),
                Is.True, "Zombies must remain outside the base collision volume.");
            Assert.That(attackers.Select(item =>
                    new Vector2(item.transform.position.x, item.transform.position.z).ToString("F1"))
                .Distinct().Count(), Is.GreaterThanOrEqualTo(4),
                "Attackers must occupy multiple visible perimeter slots.");
        }

        [UnityTest]
        public IEnumerator HaetaeAndPreviewRolesHaveDistinctSignatures()
        {
            yield return null;
            var liveMarkers = Game.Robots.Select(item => item.GetComponentInChildren<VisualIdentityMarker>()).ToArray();
            Assert.That(liveMarkers.All(item => item != null), Is.True);
            Assert.That(liveMarkers[0].markerCount, Is.EqualTo(1));
            Assert.That(liveMarkers[1].markerCount, Is.EqualTo(2));

            var roles = new[]
            {
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview,
                PresentationRole.MedicalRobot
            };
            var signatures = roles.Select(role =>
            {
                var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.name = "Preview " + role;
                Game.PresentationModels.Attach(root, role);
                var marker = root.GetComponentInChildren<VisualIdentityMarker>();
                var signature = marker.silhouetteSignature;
                Object.Destroy(root);
                return signature;
            }).ToArray();
            Assert.That(signatures.Distinct().Count(), Is.EqualTo(roles.Length));
        }

        [UnityTest]
        public IEnumerator LiveGeneralHaetaeUsesAuthoredLodModel()
        {
            yield return null;
            var authored = Game.Robots
                .Select(item => item.GetComponentInChildren<AuthoredModelMarker>())
                .ToArray();

            Assert.That(authored.All(item => item != null), Is.True);
            Assert.That(authored.All(item => item.assetId == "character.haetae.general"), Is.True);
            Assert.That(authored.All(item => item.sourceVertexCount > 1000), Is.True);
            Assert.That(authored.All(item => item.lodCount == 2), Is.True);
            Assert.That(Game.Robots.All(item => item.GetComponentInChildren<LODGroup>() != null), Is.True);
        }

        [UnityTest]
        public IEnumerator AuthoredUpgradeRolesUseRoleSpecificLodModelsAndMarkers()
        {
            var roles = new[]
            {
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview
            };
            var expectedIds = new[]
            {
                "character.haetae.melee",
                "character.haetae.ranged",
                "character.haetae.balanced"
            };
            var signatures = new string[roles.Length];

            for (var index = 0; index < roles.Length; index++)
            {
                var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.name = "Authored Upgrade " + roles[index];
                var collider = root.GetComponent<Collider>();
                var centerBefore = collider.bounds.center;
                var sizeBefore = collider.bounds.size;

                Game.PresentationModels.Attach(root, roles[index], 2);
                yield return null;

                var authored = root.GetComponentInChildren<AuthoredModelMarker>();
                var identity = root.GetComponentInChildren<VisualIdentityMarker>();
                Assert.That(authored, Is.Not.Null, roles[index].ToString());
                Assert.That(authored.assetId, Is.EqualTo(expectedIds[index]));
                Assert.That(authored.sourceVertexCount, Is.GreaterThan(18000));
                Assert.That(authored.lodCount, Is.EqualTo(2));
                Assert.That(root.GetComponentsInChildren<LODGroup>(true).Length, Is.EqualTo(1));
                Assert.That(identity.markerCount, Is.EqualTo(2));
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name.Contains("UnitMarker_2"))
                    .All(item => item.gameObject.activeSelf), Is.True);
                Assert.That(collider.bounds.center, Is.EqualTo(centerBefore));
                Assert.That(collider.bounds.size, Is.EqualTo(sizeBefore));
                signatures[index] = identity.silhouetteSignature;
                Object.Destroy(root);
            }

            Assert.That(signatures.Distinct().Count(), Is.EqualTo(roles.Length));
        }

        [UnityTest]
        public IEnumerator MissingAuthoredUpgradeUsesOnlyItsProceduralFallback()
        {
            var roles = new[]
            {
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview
            };
            var fallbackSignatures = new[]
            {
                "haetae.ram.heavy",
                "haetae.turret.long",
                "haetae.mixed.asymmetric"
            };

            for (var missingIndex = 0; missingIndex < roles.Length; missingIndex++)
            {
                var fallbackTheme = Object.Instantiate(Game.Catalog.visualTheme);
                fallbackTheme.haetaeUpgradeModels = Game.Catalog.visualTheme.haetaeUpgradeModels
                    .Select(item => new AuthoredHaetaeModelDefinition
                    {
                        role = item.role,
                        assetId = item.assetId,
                        lod0 = item.role == roles[missingIndex] ? null : item.lod0,
                        lod1 = item.lod1,
                        silhouetteSignature = item.silhouetteSignature
                    })
                    .ToArray();
                var library = new PresentationMaterialLibrary(
                    fallbackTheme, Game.Catalog.runtimeMaterialTemplate);
                var factory = new LowPolyModelFactory(library);
                var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                root.name = "Missing Upgrade " + roles[missingIndex];

                factory.Attach(root, roles[missingIndex], 1);
                factory.Attach(root, roles[missingIndex], 1);
                yield return null;

                var identity = root.GetComponentInChildren<VisualIdentityMarker>();
                Assert.That(identity.silhouetteSignature,
                    Is.EqualTo(fallbackSignatures[missingIndex]));
                Assert.That(root.GetComponentInChildren<AuthoredModelMarker>(), Is.Null);
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Count(item => item.name == LowPolyModelFactory.VisualRootName),
                    Is.EqualTo(1));
                Assert.That(root.GetComponentsInChildren<LODGroup>(true).Length, Is.EqualTo(0));

                var availableIndex = (missingIndex + 1) % roles.Length;
                var availableRoot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                factory.Attach(availableRoot, roles[availableIndex], 1);
                yield return null;
                var availableAuthored = availableRoot.GetComponentInChildren<AuthoredModelMarker>();
                Assert.That(availableAuthored, Is.Not.Null);
                Assert.That(availableAuthored.assetId,
                    Is.EqualTo(fallbackTheme.AuthoredHaetaeFor(roles[availableIndex]).assetId));

                Object.Destroy(root);
                Object.Destroy(availableRoot);
                library.Dispose();
                Object.Destroy(fallbackTheme);
            }
        }

        [UnityTest]
        public IEnumerator MissingAuthoredHaetaeUsesProceduralFallback()
        {
            var fallbackTheme = Object.Instantiate(Game.Catalog.visualTheme);
            fallbackTheme.haetaeGeneralModel = null;
            fallbackTheme.haetaeGeneralLod1 = null;
            var library = new PresentationMaterialLibrary(
                fallbackTheme, Game.Catalog.runtimeMaterialTemplate);
            var factory = new LowPolyModelFactory(library);
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Authored Haetae Fallback Test";

            factory.Attach(root, PresentationRole.HaetaeGeneralUnit1);
            yield return null;

            var marker = root.GetComponentInChildren<VisualIdentityMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.silhouetteSignature, Is.EqualTo("haetae.guardian.quadruped"));
            Assert.That(root.GetComponentInChildren<AuthoredModelMarker>(), Is.Null);

            Object.Destroy(root);
            library.Dispose();
            Object.Destroy(fallbackTheme);
        }

        [UnityTest]
        public IEnumerator EnemyTypesUseDistinctSilhouettesAndEffectsExpire()
        {
            var spawned = new[]
            {
                Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad),
                Game.SpawnZombieForTests(ZombieType.Bruiser, RouteId.NorthRoad),
                Game.SpawnZombieForTests(ZombieType.Ripper, RouteId.NorthRoad)
            };
            yield return null;
            var signatures = spawned
                .Select(item => item.GetComponentInChildren<VisualIdentityMarker>())
                .Where(item => item != null)
                .GroupBy(item => item.role)
                .ToDictionary(group => group.Key, group => group.First().silhouetteSignature);

            Assert.That(signatures.ContainsKey(PresentationRole.Runner), Is.True);
            Assert.That(signatures.ContainsKey(PresentationRole.Bruiser), Is.True);
            Assert.That(signatures.ContainsKey(PresentationRole.Ripper), Is.True);
            Assert.That(signatures.Values.Distinct().Count(), Is.EqualTo(3));

            Game.SpawnPulse(Vector3.zero, 0.2f, Color.white, 0.05f, "Presentation Test");
            Assert.That(Game.ActivePresentationEffectCount, Is.GreaterThan(0));
            for (var index = 0; index < 104; index++)
                Game.SpawnPulse(Vector3.zero, 0.02f, Color.white, 0.05f, "Bounded Presentation Test");
            Assert.That(Game.ActivePresentationEffectCount, Is.LessThanOrEqualTo(96));
            yield return new WaitForSeconds(0.12f);
            yield return null;
            Assert.That(Object.FindObjectsByType<PresentationEffectLifetime>(FindObjectsSortMode.None).Length, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator RuntimeIconsAreCachedAndThemeHasEditableTextAssets()
        {
            var first = Game.PresentationIcons.Get("ui.icon.health");
            var second = Game.PresentationIcons.Get("ui.icon.health");
            Assert.That(first, Is.SameAs(second));
            Assert.That(Game.Catalog.visualTheme.menuBackdrop, Is.Not.Null);
            Assert.That(Game.Catalog.visualTheme.bodyFont, Is.Not.Null);
            Assert.That(Game.Catalog.strings.Get("menu.title"), Is.EqualTo("텔레 로봇팀, 출격하라"));
            yield return null;
        }
    }
}
