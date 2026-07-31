using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

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
            Assert.That(Game.BaseTransform.GetComponent<CentralBasePlatform>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CentralBaseUsesLowVisibleTerracesWithMatchingColliders()
        {
            yield return null;
            var platform = Game.BaseTransform.GetComponent<CentralBasePlatform>();
            var activeColliders = Game.BaseTransform.GetComponentsInChildren<Collider>(true)
                .Where(item => item.enabled).ToArray();
            var marker = Game.BaseTransform.GetComponentInChildren<WorldLandmarkMarker>(true);
            var visualRoots = Game.BaseTransform.GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == LowPolyModelFactory.VisualRootName);
            var beaconRenderers = Game.BaseTransform.GetComponentsInChildren<Renderer>(true)
                .Where(item => item.name.StartsWith("Guardian Beacon") ||
                    item.name == "Beacon Brace").ToArray();

            Assert.That(Game.BaseTransform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(platform, Is.Not.Null);
            Assert.That(platform.TerraceCount, Is.EqualTo(Game.Config.World.BaseTerraceCount));
            Assert.That(platform.TopHeight, Is.LessThanOrEqualTo(0.75f));
            Assert.That(platform.BeaconDiameter, Is.LessThanOrEqualTo(1f));
            Assert.That(platform.SurfaceColliders.Count, Is.EqualTo(1));
            Assert.That(activeColliders.Length, Is.EqualTo(1));
            Assert.That(activeColliders.All(item => item is MeshCollider && !item.isTrigger), Is.True);
            Assert.That(platform.SurfaceColliders.Select(item => item.bounds.max.y).Max() -
                Game.BaseTransform.position.y, Is.EqualTo(platform.TopHeight).Within(0.02f));
            Assert.That(marker.shapeSignature, Is.EqualTo("base.terraced.guardian"));
            Assert.That(visualRoots, Is.EqualTo(1));
            Assert.That(beaconRenderers.Length, Is.GreaterThan(0));
            Assert.That(beaconRenderers.All(item =>
                Mathf.Max(item.bounds.size.x, item.bounds.size.z) <= 1.01f), Is.True);
        }

        [UnityTest]
        public IEnumerator PlayerTraversesBaseFromEveryCardinalDirectionWithoutJump()
        {
            Game.SetAcceleratedSpawningForTests(false);
            var directions = new[]
            {
                Vector3.forward, Vector3.right, Vector3.back, Vector3.left
            };
            for (var repeat = 0; repeat < 3; repeat++)
                foreach (var direction in directions)
                    yield return TraverseBase(direction, false);
        }

        [UnityTest]
        public IEnumerator PlayerTraversesBaseDiagonallyWithoutBeingTrappedOrEjected()
        {
            Game.SetAcceleratedSpawningForTests(false);
            var directions = new[]
            {
                new Vector3(1f, 0f, 1f).normalized,
                new Vector3(-1f, 0f, 1f).normalized
            };
            for (var repeat = 0; repeat < 4; repeat++)
                foreach (var direction in directions)
                    yield return TraverseBase(direction, true);
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

            var platform = Game.BaseTransform.GetComponent<CentralBasePlatform>();
            Assert.That(Game.BaseState.Health.Current, Is.LessThan(healthBefore),
                "Perimeter attackers must still damage the base.");
            Assert.That(attackers.All(item => item != null &&
                PlanarDistance(item.transform.position, Game.BaseTransform.position) >=
                platform.OuterRadius - 0.05f),
                Is.True, "Zombies must remain outside the base collision volume.");
            Assert.That(attackers.Select(item =>
                    new Vector2(item.transform.position.x, item.transform.position.z).ToString("F1"))
                .Distinct().Count(), Is.GreaterThanOrEqualTo(4),
                "Attackers must occupy multiple visible perimeter slots.");
            var hud = Object.FindFirstObjectByType<CombatHud>();
            Assert.That(hud, Is.Not.Null);
            Assert.That(Game.Robots.All(item =>
                !string.IsNullOrWhiteSpace(hud.GetRobotHealthBarText(item.State.Id)) &&
                !string.IsNullOrWhiteSpace(hud.GetRobotBatteryBarText(item.State.Id))), Is.True,
                "Existing robot status bars must remain populated.");
            Assert.That(Game.Config.World.BaseChargingRadius, Is.EqualTo(6f));
        }

        private IEnumerator TraverseBase(Vector3 direction, bool diagonal)
        {
            var platform = Game.BaseTransform.GetComponent<CentralBasePlatform>();
            var player = Game.PlayerActor;
            var unrelatedColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)
                .Where(item => item.enabled &&
                    item.transform != player.transform &&
                    !item.transform.IsChildOf(player.transform) &&
                    item.transform != Game.BaseTransform &&
                    !item.transform.IsChildOf(Game.BaseTransform) &&
                    item.gameObject.name != "Ground")
                .ToArray();
            foreach (var collider in unrelatedColliders) collider.enabled = false;
            var startDistance = platform.OuterRadius + 1.2f;
            var start = Game.BaseTransform.position - direction * startDistance + Vector3.up;
            player.transform.position = start;
            Physics.SyncTransforms();
            player.CharacterForTests.Move(Vector3.down * 0.12f);
            Physics.SyncTransforms();

            var highestY = player.transform.position.y;
            const int steps = 132;
            var travel = startDistance * 2f + 1.6f;
            for (var step = 0; step < steps; step++)
            {
                player.CharacterForTests.Move(direction * (travel / steps) + Vector3.down * 0.035f);
                Physics.SyncTransforms();
                highestY = Mathf.Max(highestY, player.transform.position.y);
                yield return null;
            }
            for (var settle = 0; settle < 30; settle++)
            {
                player.CharacterForTests.Move(direction * 0.05f + Vector3.down * 0.08f);
                Physics.SyncTransforms();
                yield return null;
            }

            var signedExit = Vector3.Dot(player.transform.position - Game.BaseTransform.position, direction);
            foreach (var collider in unrelatedColliders)
                if (collider != null) collider.enabled = true;
            Physics.SyncTransforms();

            Assert.That(signedExit, Is.GreaterThan(platform.OuterRadius + 0.05f),
                (diagonal ? "Diagonal" : "Cardinal") + " traversal must exit the opposite side.");
            Assert.That(highestY, Is.GreaterThanOrEqualTo(
                Game.BaseTransform.position.y + platform.TopHeight + 0.75f),
                "Traversal must reach the top terrace.");
            Assert.That(Mathf.Abs(player.transform.position.y - start.y), Is.LessThan(0.35f),
                "Traversal must return to surrounding ground without ejection or fall-through.");
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            var delta = first - second;
            return new Vector2(delta.x, delta.z).magnitude;
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
            Assert.That(Game.Robots.All(item =>
                item.transform.Find(LowPolyModelFactory.VisualRootName).localScale ==
                Vector3.one * Game.Catalog.visualTheme.haetaeVisualScale), Is.True);
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
                Assert.That(root.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                    Is.EqualTo(Vector3.one * Game.Catalog.visualTheme.haetaeVisualScale));
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
                Assert.That(root.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                    Is.EqualTo(Vector3.one * fallbackTheme.haetaeVisualScale));

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
            var fader = root.AddComponent<HaetaeCameraOcclusionFader>();
            fader.Initialize(Game.PlayerActor, fallbackTheme);
            fader.enabled = false;
            yield return null;

            var marker = root.GetComponentInChildren<VisualIdentityMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.silhouetteSignature, Is.EqualTo("haetae.guardian.quadruped"));
            Assert.That(root.GetComponentInChildren<AuthoredModelMarker>(), Is.Null);
            Assert.That(root.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                Is.EqualTo(Vector3.one * fallbackTheme.haetaeVisualScale));
            fader.TickForTests(true, fallbackTheme.haetaeOcclusionFade.fadeSeconds);
            Assert.That(fader.CurrentOpacity,
                Is.EqualTo(fallbackTheme.haetaeOcclusionFade.obstructingOpacity).Within(0.001f));
            Assert.That(fader.UsesTransparentMaterials, Is.True);
            fader.TickForTests(false, fallbackTheme.haetaeOcclusionFade.restoreSeconds);
            Assert.That(fader.CurrentOpacity, Is.EqualTo(1f).Within(0.001f));
            Assert.That(fader.UsesTransparentMaterials, Is.False);

            Object.Destroy(root);
            library.Dispose();
            Object.Destroy(fallbackTheme);
        }

        [UnityTest]
        public IEnumerator HaetaeScaleStaysAbsoluteAcrossRolesRefreshMotionAndPhysics()
        {
            var theme = Game.Catalog.visualTheme;
            var expectedScale = Vector3.one * theme.haetaeVisualScale;
            var roles = new[]
            {
                PresentationRole.HaetaeGeneralUnit1,
                PresentationRole.HaetaeGeneralUnit2,
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview
            };
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Haetae Scale Boundary Test";
            root.transform.localScale = new Vector3(1.1f, 0.75f, 1.5f);
            var collider = root.GetComponent<Collider>();
            var gameplayScaleBefore = root.transform.localScale;
            Physics.SyncTransforms();
            var colliderCenterBefore = collider.bounds.center;
            var colliderSizeBefore = collider.bounds.size;

            foreach (var role in roles)
            {
                Game.PresentationModels.Attach(root, role, 2);
                yield return null;

                var visual = root.transform.Find(LowPolyModelFactory.VisualRootName);
                Assert.That(visual, Is.Not.Null, role.ToString());
                Assert.That(visual.localScale, Is.EqualTo(expectedScale), role.ToString());
                var motion = root.GetComponent<CharacterMotionDriver>();
                Assert.That(motion, Is.Not.Null, role.ToString());
                motion.SampleForTests(CharacterMotionState.Attack, 0.5f);
                Assert.That(visual.localScale, Is.EqualTo(expectedScale), role.ToString());
            }

            for (var iteration = 0; iteration < 10; iteration++)
            {
                Game.PresentationModels.Attach(root, PresentationRole.HaetaeGeneralUnit1, 1);
                yield return null;
                Assert.That(root.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                    Is.EqualTo(expectedScale), "Refresh " + iteration);
            }

            Physics.SyncTransforms();
            Assert.That(root.transform.localScale, Is.EqualTo(gameplayScaleBefore));
            Assert.That(collider.bounds.center, Is.EqualTo(colliderCenterBefore));
            Assert.That(collider.bounds.size, Is.EqualTo(colliderSizeBefore));

            var runner = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            runner.name = "Non-Haetae Scale Control";
            Game.PresentationModels.Attach(runner, PresentationRole.Runner);
            yield return null;
            Assert.That(runner.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                Is.EqualTo(Vector3.one));

            Object.Destroy(root);
            Object.Destroy(runner);
        }

        [UnityTest]
        public IEnumerator LiveHaetaeUsesOneVisualScaleAndPreservesLegacyPhysicalBounds()
        {
            yield return null;
            Assert.That(Game.Catalog.visualTheme.haetaeVisualScale, Is.EqualTo(0.85f));
            var expectedVisualScale = Vector3.one * Game.Catalog.visualTheme.haetaeVisualScale;
            foreach (var robot in Game.Robots)
            {
                Assert.That(robot.transform.localScale, Is.EqualTo(Vector3.one), robot.name);
                var visual = robot.transform.Find(LowPolyModelFactory.VisualRootName);
                Assert.That(visual, Is.Not.Null, robot.name);
                Assert.That(visual.localScale, Is.EqualTo(expectedVisualScale), robot.name);

                var capsule = robot.GetComponent<CapsuleCollider>();
                Assert.That(capsule.radius, Is.EqualTo(Game.Config.Robot.BodyColliderRadius));
                Assert.That(capsule.height, Is.EqualTo(Game.Config.Robot.BodyColliderHeight));
                Assert.That(capsule.center.y, Is.EqualTo(Game.Config.Robot.BodyColliderCenterY));

                var legacy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                legacy.transform.position = robot.transform.position;
                legacy.transform.localScale = new Vector3(1.1f, 0.75f, 1.5f);
                Physics.SyncTransforms();
                Assert.That(capsule.bounds.center,
                    Is.EqualTo(legacy.GetComponent<Collider>().bounds.center).Using(Vector3ComparerWithEqualsOperator.Instance));
                Assert.That(capsule.bounds.size,
                    Is.EqualTo(legacy.GetComponent<Collider>().bounds.size).Using(Vector3ComparerWithEqualsOperator.Instance));
                Object.Destroy(legacy);
            }

            var specializationRobot = Game.Robots[0];
            var specializations = new[]
            {
                HaetaeSpecialization.Melee,
                HaetaeSpecialization.Ranged,
                HaetaeSpecialization.Balanced
            };
            foreach (var specialization in specializations)
            {
                specializationRobot.State.Progression.Specialization = specialization;
                yield return null;
                Assert.That(specializationRobot.transform.localScale, Is.EqualTo(Vector3.one),
                    specialization.ToString());
                Assert.That(specializationRobot.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                    Is.EqualTo(expectedVisualScale), specialization.ToString());
            }
        }

        [UnityTest]
        public IEnumerator HaetaeOcclusionFadesOnlyTheThirdPersonAimCorridorAndRestores()
        {
            yield return null;
            var tuning = Game.Catalog.visualTheme.haetaeOcclusionFade;
            Assert.That(tuning.obstructingOpacity, Is.EqualTo(0.16f));
            var player = Game.PlayerActor;
            var robot = Game.Robots[0];
            var fader = robot.GetComponent<HaetaeCameraOcclusionFader>();
            Assert.That(fader, Is.Not.Null);
            Assert.That(robot.GetComponents<HaetaeCameraOcclusionFader>().Length, Is.EqualTo(1));
            fader.enabled = false;
            fader.TickForTests(false, tuning.restoreSeconds);

            var renderers = fader.PresentationRoot.GetComponentsInChildren<Renderer>(true);
            var opaqueMaterials = renderers.Select(item => item.sharedMaterials.ToArray()).ToArray();
            var camera = player.ViewCamera;
            var centeredPosition = camera.transform.position + camera.transform.forward * 5f;
            robot.transform.position = centeredPosition + camera.transform.right * 6f;
            fader.PresentationRoot.position = centeredPosition;
            Physics.SyncTransforms();
            var collider = robot.GetComponent<Collider>();
            var colliderSize = collider.bounds.size;
            var gameplayScale = robot.transform.localScale;

            Assert.That(Vector3.Distance(collider.bounds.center, centeredPosition), Is.GreaterThan(4f));
            Assert.That(fader.EvaluateOcclusionForTests(), Is.True);
            fader.TickForTests(true, tuning.fadeSeconds);
            Assert.That(fader.IsObstructing, Is.True);
            Assert.That(fader.CurrentOpacity,
                Is.EqualTo(tuning.obstructingOpacity).Within(0.001f));
            Assert.That(fader.UsesTransparentMaterials, Is.True);
            Assert.That(renderers.SelectMany(item => item.sharedMaterials)
                .All(item => item.renderQueue >= 3000), Is.True);
            var specularMaterials = renderers.SelectMany(item => item.sharedMaterials)
                .Where(item => item.HasProperty("_BlendModePreserveSpecular"))
                .ToArray();
            Assert.That(specularMaterials, Is.Not.Empty);
            Assert.That(specularMaterials
                .All(item => item.GetFloat("_BlendModePreserveSpecular") == 0f), Is.True);
            Assert.That(renderers.SelectMany(item => item.sharedMaterials)
                .Where(item => item.HasProperty("_BaseColor"))
                .All(item => Mathf.Approximately(
                    item.GetColor("_BaseColor").a, tuning.obstructingOpacity)), Is.True);
            Assert.That(collider.bounds.size, Is.EqualTo(colliderSize));
            Assert.That(robot.transform.localScale, Is.EqualTo(gameplayScale));
            Assert.That(robot.transform.Find(LowPolyModelFactory.VisualRootName).localScale,
                Is.EqualTo(Vector3.one * Game.Catalog.visualTheme.haetaeVisualScale));

            robot.transform.position = centeredPosition;
            fader.PresentationRoot.position = centeredPosition + camera.transform.right * 6f;
            Physics.SyncTransforms();
            Assert.That(fader.EvaluateOcclusionForTests(), Is.False);
            fader.TickForTests(false, tuning.restoreSeconds);
            Assert.That(fader.CurrentOpacity, Is.EqualTo(1f).Within(0.001f));
            Assert.That(fader.UsesTransparentMaterials, Is.False);
            for (var index = 0; index < renderers.Length; index++)
                Assert.That(renderers[index].sharedMaterials, Is.EqualTo(opaqueMaterials[index]));

            robot.transform.position = centeredPosition + camera.transform.right * 6f;
            fader.PresentationRoot.position = centeredPosition;
            Physics.SyncTransforms();
            player.TogglePerspective();
            Assert.That(player.Perspective, Is.EqualTo(CameraPerspective.FirstPerson));
            Assert.That(fader.EvaluateOcclusionForTests(), Is.False);
            fader.TickForTests(false, tuning.restoreSeconds);
            Assert.That(fader.CurrentOpacity, Is.EqualTo(1f).Within(0.001f));
            player.TogglePerspective();
        }

        [UnityTest]
        public IEnumerator HaetaeOcclusionRemainsIndependentAcrossReplacementMotionAndCycles()
        {
            yield return null;
            var tuning = Game.Catalog.visualTheme.haetaeOcclusionFade;
            var firstRobot = Game.Robots[0];
            var secondRobot = Game.Robots[1];
            var first = firstRobot.GetComponent<HaetaeCameraOcclusionFader>();
            var second = secondRobot.GetComponent<HaetaeCameraOcclusionFader>();
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            first.enabled = false;
            second.enabled = false;
            first.TickForTests(false, tuning.restoreSeconds);
            second.TickForTests(false, tuning.restoreSeconds);

            first.TickForTests(true, tuning.fadeSeconds);
            second.TickForTests(false, tuning.fadeSeconds);
            Assert.That(first.CurrentOpacity,
                Is.EqualTo(tuning.obstructingOpacity).Within(0.001f));
            Assert.That(second.CurrentOpacity, Is.EqualTo(1f).Within(0.001f));
            second.TickForTests(true, tuning.fadeSeconds);
            Assert.That(second.CurrentOpacity,
                Is.EqualTo(tuning.obstructingOpacity).Within(0.001f));
            Assert.That(first.OwnedMaterialCount, Is.GreaterThan(0));
            Assert.That(second.OwnedMaterialCount, Is.GreaterThan(0));
            Assert.That(first.OwnedMaterials.Intersect(second.OwnedMaterials).Any(), Is.False);

            var oldRoot = first.PresentationRoot;
            firstRobot.State.Progression.Specialization = HaetaeSpecialization.Melee;
            for (var frame = 0; frame < 3 && first.PresentationRoot == oldRoot; frame++)
            {
                yield return null;
                first.TickForTests(true, tuning.fadeSeconds);
            }
            Assert.That(first.PresentationRoot, Is.Not.SameAs(oldRoot));
            Assert.That(first.PresentationRoot.GetComponent<AuthoredModelMarker>().assetId,
                Is.EqualTo("character.haetae.melee"));
            var ownedAfterReplacement = first.OwnedMaterialCount;
            Assert.That(ownedAfterReplacement, Is.GreaterThan(0));

            var tinted = first.PresentationRoot.GetComponentsInChildren<Renderer>(true).First();
            var tint = new Color(0.18f, 0.27f, 0.39f, 1f);
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            tinted.SetPropertyBlock(block);
            first.TickForTests(true, tuning.fadeSeconds);
            var sampled = new MaterialPropertyBlock();
            tinted.GetPropertyBlock(sampled);
            var sampledColor = sampled.GetColor("_BaseColor");
            Assert.That(sampledColor.r, Is.EqualTo(tint.r).Within(0.001f));
            Assert.That(sampledColor.g, Is.EqualTo(tint.g).Within(0.001f));
            Assert.That(sampledColor.b, Is.EqualTo(tint.b).Within(0.001f));
            Assert.That(sampledColor.a, Is.EqualTo(tuning.obstructingOpacity).Within(0.001f));

            firstRobot.GetComponent<CharacterMotionDriver>()
                .SampleForTests(CharacterMotionState.Attack, 0.5f, CharacterAttackMotion.Melee);
            Assert.That(first.CurrentOpacity,
                Is.EqualTo(tuning.obstructingOpacity).Within(0.001f));
            for (var cycle = 0; cycle < 10; cycle++)
            {
                first.TickForTests(false, tuning.restoreSeconds);
                Assert.That(first.CurrentOpacity, Is.EqualTo(1f).Within(0.001f), "Restore " + cycle);
                first.TickForTests(true, tuning.fadeSeconds);
                Assert.That(first.CurrentOpacity,
                    Is.EqualTo(tuning.obstructingOpacity).Within(0.001f), "Fade " + cycle);
                Assert.That(first.OwnedMaterialCount, Is.EqualTo(ownedAfterReplacement));
            }
            first.TickForTests(false, tuning.restoreSeconds);
            tinted.GetPropertyBlock(sampled);
            Assert.That(sampled.GetColor("_BaseColor").a, Is.EqualTo(1f).Within(0.001f));
            Assert.That(first.UsesTransparentMaterials, Is.False);
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
        public IEnumerator AuthoredZombieRolesUseDetailedLodModelsWithoutChangingColliders()
        {
            var types = new[] { ZombieType.Runner, ZombieType.Bruiser, ZombieType.Ripper };
            var roles = new[] { PresentationRole.Runner, PresentationRole.Bruiser, PresentationRole.Ripper };
            var expectedIds = new[] { "enemy.runner", "enemy.bruiser", "enemy.ripper" };
            var signatures = new string[types.Length];

            for (var index = 0; index < types.Length; index++)
            {
                var zombie = Game.SpawnZombieForTests(types[index], RouteId.NorthRoad);
                var rootCollider = zombie.GetComponent<CapsuleCollider>();
                var centerBefore = rootCollider.center;
                var radiusBefore = rootCollider.radius;
                var heightBefore = rootCollider.height;
                yield return null;

                var authored = zombie.GetComponentInChildren<AuthoredModelMarker>(true);
                var identity = zombie.GetComponentInChildren<VisualIdentityMarker>(true);
                Assert.That(authored, Is.Not.Null, roles[index].ToString());
                Assert.That(authored.assetId, Is.EqualTo(expectedIds[index]));
                Assert.That(authored.sourceVertexCount, Is.GreaterThan(16000));
                Assert.That(authored.lodCount, Is.EqualTo(2));
                Assert.That(zombie.GetComponentsInChildren<LODGroup>(true).Length, Is.EqualTo(1));
                Assert.That(rootCollider.center, Is.EqualTo(centerBefore));
                Assert.That(rootCollider.radius, Is.EqualTo(radiusBefore));
                Assert.That(rootCollider.height, Is.EqualTo(heightBefore));
                Assert.That(zombie.GetComponentsInChildren<Collider>(true)
                    .Where(collider => collider != rootCollider)
                    .All(collider => !collider.enabled), Is.True);
                signatures[index] = identity.silhouetteSignature;
            }

            Assert.That(signatures.Distinct().Count(), Is.EqualTo(types.Length));
        }

        [UnityTest]
        public IEnumerator MissingZombieLod0FallsBackPerRoleAndMissingLod1UsesLod0Only()
        {
            var roles = new[] { PresentationRole.Runner, PresentationRole.Bruiser, PresentationRole.Ripper };
            var fallbackSignatures = new[]
            {
                "runner.lean.fins", "bruiser.wide.armor", "ripper.tall.blades"
            };

            for (var missingIndex = 0; missingIndex < roles.Length; missingIndex++)
            {
                var fallbackTheme = Object.Instantiate(Game.Catalog.visualTheme);
                fallbackTheme.authoredZombieModels = Game.Catalog.visualTheme.authoredZombieModels
                    .Select(item => new AuthoredZombieModelDefinition
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

                factory.Attach(root, roles[missingIndex]);
                factory.Attach(root, roles[missingIndex]);
                yield return null;
                Assert.That(root.GetComponentInChildren<VisualIdentityMarker>()
                    .silhouetteSignature, Is.EqualTo(fallbackSignatures[missingIndex]));
                Assert.That(root.GetComponentInChildren<AuthoredModelMarker>(), Is.Null);
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Count(item => item.name == LowPolyModelFactory.VisualRootName), Is.EqualTo(1));

                var availableIndex = (missingIndex + 1) % roles.Length;
                var availableRoot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                factory.Attach(availableRoot, roles[availableIndex]);
                yield return null;
                Assert.That(availableRoot.GetComponentInChildren<AuthoredModelMarker>(), Is.Not.Null);

                Object.Destroy(root);
                Object.Destroy(availableRoot);
                library.Dispose();
                Object.Destroy(fallbackTheme);
            }

            var lod0OnlyTheme = Object.Instantiate(Game.Catalog.visualTheme);
            lod0OnlyTheme.authoredZombieModels = Game.Catalog.visualTheme.authoredZombieModels
                .Select(item => new AuthoredZombieModelDefinition
                {
                    role = item.role,
                    assetId = item.assetId,
                    lod0 = item.lod0,
                    lod1 = item.role == PresentationRole.Runner ? null : item.lod1,
                    silhouetteSignature = item.silhouetteSignature
                })
                .ToArray();
            var lod0OnlyLibrary = new PresentationMaterialLibrary(
                lod0OnlyTheme, Game.Catalog.runtimeMaterialTemplate);
            var lod0OnlyFactory = new LowPolyModelFactory(lod0OnlyLibrary);
            var lod0OnlyRoot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lod0OnlyFactory.Attach(lod0OnlyRoot, PresentationRole.Runner);
            yield return null;
            Assert.That(lod0OnlyRoot.GetComponentInChildren<AuthoredModelMarker>().lodCount,
                Is.EqualTo(1));
            Assert.That(lod0OnlyRoot.GetComponentInChildren<LODGroup>(true), Is.Null);

            Object.Destroy(lod0OnlyRoot);
            lod0OnlyLibrary.Dispose();
            Object.Destroy(lod0OnlyTheme);
        }

        [UnityTest]
        public IEnumerator AuthoredZombieHitFeedbackReachesEveryRenderer()
        {
            var zombie = Game.SpawnZombieForTests(ZombieType.Runner, RouteId.NorthRoad);
            yield return null;
            var renderers = zombie.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled).ToArray();
            Assert.That(renderers.Length, Is.GreaterThan(0));

            zombie.ReceiveDamage(1, DamageSource.Player("presentation-test"));
            yield return null;
            var block = new MaterialPropertyBlock();
            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                Assert.That(block.GetColor("_BaseColor"), Is.Not.EqualTo(default(Color)));
            }
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
