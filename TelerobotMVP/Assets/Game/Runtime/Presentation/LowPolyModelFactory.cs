using System;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class VisualIdentityMarker : MonoBehaviour
    {
        public PresentationRole role;
        public string silhouetteSignature;
        public int markerCount;
        public int partCount;
    }

    public sealed class AuthoredModelMarker : MonoBehaviour
    {
        public string assetId;
        public int sourceVertexCount;
        public int lodCount;
    }

    public sealed class LowPolyModelFactory
    {
        public const string VisualRootName = "Presentation Visual";
        private readonly PresentationMaterialLibrary materials;

        public LowPolyModelFactory(PresentationMaterialLibrary materialLibrary)
        {
            materials = materialLibrary;
        }

        public GameObject Attach(GameObject gameplayRoot, PresentationRole role, int unitMarkerCount = 0)
        {
            if (gameplayRoot == null) return null;
            var old = gameplayRoot.transform.Find(VisualRootName);
            if (old != null)
            {
                if (Application.isPlaying)
                {
                    old.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(old.gameObject);
                }
                else UnityEngine.Object.DestroyImmediate(old.gameObject);
            }

            var visual = new GameObject(VisualRootName);
            visual.transform.SetParent(gameplayRoot.transform, false);
            try
            {
                var marker = visual.AddComponent<VisualIdentityMarker>();
                marker.role = role;
                BuildRole(visual.transform, role, marker, unitMarkerCount);
                marker.partCount = visual.GetComponentsInChildren<Renderer>(true).Length;
                var rootRenderer = gameplayRoot.GetComponent<Renderer>();
                if (rootRenderer != null) rootRenderer.enabled = false;
                return visual;
            }
            catch (Exception exception)
            {
                var rootRenderer = gameplayRoot.GetComponent<Renderer>();
                if (rootRenderer != null) rootRenderer.enabled = true;
                if (Application.isPlaying) UnityEngine.Object.Destroy(visual);
                else UnityEngine.Object.DestroyImmediate(visual);
                Debug.LogWarning("Presentation fallback retained for " + gameplayRoot.name + ": " + exception.Message);
                return null;
            }
        }

        public GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 localPosition,
            Vector3 localScale, Vector3 localEuler, string materialRole)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = Quaternion.Euler(localEuler);
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(collider);
                else UnityEngine.Object.DestroyImmediate(collider);
            }
            materials.Apply(part.GetComponent<Renderer>(), materialRole);
            return part;
        }

        private void BuildRole(Transform root, PresentationRole role, VisualIdentityMarker marker, int unitMarkerCount)
        {
            switch (role)
            {
                case PresentationRole.PlayerCommander:
                    BuildPlayer(root, marker);
                    return;
                case PresentationRole.AssaultRifle:
                    BuildRifle(root, marker);
                    return;
                case PresentationRole.HaetaeGeneralUnit1:
                case PresentationRole.HaetaeGeneralUnit2:
                case PresentationRole.HaetaeMeleePreview:
                case PresentationRole.HaetaeRangedPreview:
                case PresentationRole.HaetaeBalancedPreview:
                    BuildHaetae(root, role, marker, unitMarkerCount);
                    return;
                case PresentationRole.MedicalRobot:
                    BuildMedical(root, marker);
                    return;
                case PresentationRole.Runner:
                case PresentationRole.Bruiser:
                case PresentationRole.Ripper:
                    BuildEnemy(root, role, marker);
                    return;
                default:
                    throw new InvalidOperationException("Unsupported compound visual role: " + role);
            }
        }

        private void BuildPlayer(Transform root, VisualIdentityMarker marker)
        {
            marker.silhouetteSignature = "commander.shoulders.backpack";
            marker.markerCount = 1;
            CreatePart(root, "Armored Torso", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f),
                new Vector3(0.7f, 0.72f, 0.45f), Vector3.zero, "ally.armor");
            CreatePart(root, "Helmet", PrimitiveType.Sphere, new Vector3(0f, 0.82f, 0.02f),
                new Vector3(0.58f, 0.48f, 0.55f), Vector3.zero, "ally.armor");
            CreatePart(root, "Visor", PrimitiveType.Cube, new Vector3(0f, 0.84f, 0.29f),
                new Vector3(0.42f, 0.11f, 0.08f), Vector3.zero, "ally.energy");
            CreatePart(root, "Left Shoulder", PrimitiveType.Cube, new Vector3(-0.48f, 0.28f, 0f),
                new Vector3(0.28f, 0.22f, 0.45f), new Vector3(0f, 0f, -12f), "world.trim");
            CreatePart(root, "Right Shoulder", PrimitiveType.Cube, new Vector3(0.48f, 0.28f, 0f),
                new Vector3(0.28f, 0.22f, 0.45f), new Vector3(0f, 0f, 12f), "world.trim");
            CreatePart(root, "Backpack", PrimitiveType.Cube, new Vector3(0f, 0.1f, -0.38f),
                new Vector3(0.48f, 0.58f, 0.2f), Vector3.zero, "world.structure");
        }

        private void BuildRifle(Transform root, VisualIdentityMarker marker)
        {
            marker.silhouetteSignature = "rifle.compact.guardian";
            marker.markerCount = 1;
            CreatePart(root, "Rifle Body", PrimitiveType.Cube, Vector3.zero,
                new Vector3(0.18f, 0.16f, 0.8f), Vector3.zero, "ally.armor");
            CreatePart(root, "Rifle Barrel", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.52f),
                new Vector3(0.05f, 0.38f, 0.05f), new Vector3(90f, 0f, 0f), "world.trim");
            CreatePart(root, "Rifle Energy", PrimitiveType.Cube, new Vector3(0f, 0.1f, 0f),
                new Vector3(0.08f, 0.05f, 0.34f), Vector3.zero, "ally.energy");
        }

        private void BuildHaetae(Transform root, PresentationRole role, VisualIdentityMarker marker, int unitMarkerCount)
        {
            if (TryBuildAuthoredHaetae(root, role, marker, unitMarkerCount)) return;

            marker.silhouetteSignature = role == PresentationRole.HaetaeMeleePreview ? "haetae.ram.heavy" :
                role == PresentationRole.HaetaeRangedPreview ? "haetae.turret.long" :
                role == PresentationRole.HaetaeBalancedPreview ? "haetae.mixed.asymmetric" :
                "haetae.guardian.quadruped";
            marker.markerCount = unitMarkerCount > 0
                ? unitMarkerCount
                : role == PresentationRole.HaetaeGeneralUnit2 ? 2 : 1;

            CreatePart(root, "Guardian Chassis", PrimitiveType.Cube, new Vector3(0f, 0f, -0.06f),
                new Vector3(0.82f, 0.55f, 1.08f), Vector3.zero, "ally.armor");
            CreatePart(root, "Guardian Head", PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.68f),
                new Vector3(0.72f, 0.55f, 0.52f), new Vector3(-7f, 0f, 0f), "ally.armor");
            CreatePart(root, "Energy Jaw", PrimitiveType.Cube, new Vector3(0f, -0.03f, 0.94f),
                new Vector3(0.42f, 0.13f, 0.18f), Vector3.zero, "ally.energy");
            CreatePart(root, "Left Horn", PrimitiveType.Cylinder, new Vector3(-0.29f, 0.57f, 0.68f),
                new Vector3(0.09f, 0.36f, 0.09f), new Vector3(-28f, 0f, -18f), "ally.haetae");
            CreatePart(root, "Right Horn", PrimitiveType.Cylinder, new Vector3(0.29f, 0.57f, 0.68f),
                new Vector3(0.09f, 0.36f, 0.09f), new Vector3(-28f, 0f, 18f), "ally.haetae");
            for (var side = -1; side <= 1; side += 2)
                for (var forward = -1; forward <= 1; forward += 2)
                    CreatePart(root, "Guardian Leg", PrimitiveType.Cube,
                        new Vector3(side * 0.36f, -0.49f, forward * 0.38f),
                        new Vector3(0.18f, 0.54f, 0.22f), new Vector3(forward * 5f, 0f, side * 4f), "world.trim");
            CreatePart(root, "Tail", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, -0.82f),
                new Vector3(0.08f, 0.42f, 0.08f), new Vector3(62f, 0f, 0f), "ally.haetae");

            var markerRole = marker.markerCount == 2 ? "ally.unit2" : "ally.energy";
            CreatePart(root, "Crest Marker 1", PrimitiveType.Cube, new Vector3(0f, 0.58f, 0.15f),
                new Vector3(0.14f, 0.25f, 0.42f), new Vector3(-12f, 0f, 0f), markerRole);
            if (marker.markerCount == 2)
                CreatePart(root, "Crest Marker 2", PrimitiveType.Cube, new Vector3(0.22f, 0.55f, 0.12f),
                    new Vector3(0.09f, 0.2f, 0.32f), new Vector3(-12f, 0f, 0f), markerRole);

            if (role == PresentationRole.HaetaeMeleePreview)
            {
                CreatePart(root, "Melee Ram", PrimitiveType.Cube, new Vector3(0f, 0.02f, 1.15f),
                    new Vector3(0.86f, 0.36f, 0.38f), new Vector3(-8f, 0f, 0f), "ally.haetae");
                CreatePart(root, "Left Shoulder Armor", PrimitiveType.Cube, new Vector3(-0.54f, 0.18f, 0.22f),
                    new Vector3(0.28f, 0.48f, 0.62f), Vector3.zero, "ally.haetae");
                CreatePart(root, "Right Shoulder Armor", PrimitiveType.Cube, new Vector3(0.54f, 0.18f, 0.22f),
                    new Vector3(0.28f, 0.48f, 0.62f), Vector3.zero, "ally.haetae");
            }
            else if (role == PresentationRole.HaetaeRangedPreview)
            {
                CreatePart(root, "Ranged Turret", PrimitiveType.Cylinder, new Vector3(0f, 0.56f, -0.12f),
                    new Vector3(0.34f, 0.18f, 0.34f), Vector3.zero, "ally.armor");
                CreatePart(root, "Ranged Barrel", PrimitiveType.Cylinder, new Vector3(0f, 0.65f, 0.44f),
                    new Vector3(0.09f, 0.58f, 0.09f), new Vector3(90f, 0f, 0f), "ally.energy");
            }
            else if (role == PresentationRole.HaetaeBalancedPreview)
            {
                CreatePart(root, "Balanced Turret", PrimitiveType.Cylinder, new Vector3(-0.2f, 0.55f, -0.05f),
                    new Vector3(0.22f, 0.14f, 0.22f), Vector3.zero, "ally.armor");
                CreatePart(root, "Balanced Barrel", PrimitiveType.Cylinder, new Vector3(-0.2f, 0.61f, 0.38f),
                    new Vector3(0.06f, 0.42f, 0.06f), new Vector3(90f, 0f, 0f), "ally.energy");
                CreatePart(root, "Balanced Fang", PrimitiveType.Cube, new Vector3(0.24f, -0.12f, 1.01f),
                    new Vector3(0.12f, 0.26f, 0.1f), new Vector3(18f, 0f, 0f), "ally.haetae");
            }
        }

        private bool TryBuildAuthoredHaetae(
            Transform root, PresentationRole role, VisualIdentityMarker marker, int unitMarkerCount)
        {
            var theme = materials.Theme;
            if (!TryResolveAuthoredHaetae(
                    theme,
                    role,
                    out var lod0Prefab,
                    out var lod1Prefab,
                    out var assetId,
                    out var silhouetteSignature,
                    out var modelName)) return false;

            GameObject lod0 = null;
            GameObject lod1 = null;
            try
            {
                lod0 = UnityEngine.Object.Instantiate(lod0Prefab, root, false);
                lod0.name = "Haetae " + modelName + " Authored LOD0";
                ResetLocalTransform(lod0.transform);
                ApplyAuthoredMaterials(lod0);

                var requestedMarkers = unitMarkerCount > 0
                    ? unitMarkerCount
                    : role == PresentationRole.HaetaeGeneralUnit2 ? 2 : 1;
                ConfigureAuthoredUnitMarkers(lod0, requestedMarkers);

                var lod0Renderers = lod0.GetComponentsInChildren<Renderer>(true);
                var lodCount = 1;
                if (lod1Prefab != null)
                {
                    lod1 = UnityEngine.Object.Instantiate(lod1Prefab, root, false);
                    lod1.name = "Haetae " + modelName + " Authored LOD1";
                    ResetLocalTransform(lod1.transform);
                    ApplyAuthoredMaterials(lod1);
                    ConfigureAuthoredUnitMarkers(lod1, requestedMarkers);

                    var lodGroup = root.gameObject.AddComponent<LODGroup>();
                    lodGroup.fadeMode = LODFadeMode.CrossFade;
                    lodGroup.animateCrossFading = false;
                    lodGroup.SetLODs(new[]
                    {
                        new LOD(0.3f, lod0Renderers),
                        new LOD(0.07f, lod1.GetComponentsInChildren<Renderer>(true))
                    });
                    lodGroup.RecalculateBounds();
                    lodCount = 2;
                }

                marker.silhouetteSignature = silhouetteSignature;
                marker.markerCount = requestedMarkers;
                var authored = root.gameObject.AddComponent<AuthoredModelMarker>();
                authored.assetId = assetId;
                authored.sourceVertexCount = CountVertices(lod0);
                authored.lodCount = lodCount;
                return true;
            }
            catch (Exception exception)
            {
                DestroyPresentationObject(lod0);
                DestroyPresentationObject(lod1);
                Debug.LogWarning("Authored haetae model fallback: " + exception.Message);
                return false;
            }
        }

        private static bool TryResolveAuthoredHaetae(
            VisualThemeDefinitionAsset theme,
            PresentationRole role,
            out GameObject lod0,
            out GameObject lod1,
            out string assetId,
            out string silhouetteSignature,
            out string modelName)
        {
            lod0 = null;
            lod1 = null;
            assetId = null;
            silhouetteSignature = null;
            modelName = null;
            if (theme == null) return false;

            if (role == PresentationRole.HaetaeGeneralUnit1 ||
                role == PresentationRole.HaetaeGeneralUnit2)
            {
                lod0 = theme.haetaeGeneralModel;
                lod1 = theme.haetaeGeneralLod1;
                assetId = "character.haetae.general";
                silhouetteSignature = "haetae.authored.guardian.quadruped";
                modelName = "General";
                return lod0 != null;
            }

            var definition = theme.AuthoredHaetaeFor(role);
            if (definition == null || definition.lod0 == null) return false;
            lod0 = definition.lod0;
            lod1 = definition.lod1;
            assetId = definition.assetId;
            silhouetteSignature = definition.silhouetteSignature;
            modelName = role == PresentationRole.HaetaeMeleePreview ? "Melee" :
                role == PresentationRole.HaetaeRangedPreview ? "Ranged" : "Balanced";
            return true;
        }

        private void ApplyAuthoredMaterials(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var source = renderer.sharedMaterials;
                var remapped = new Material[source.Length];
                for (var materialIndex = 0; materialIndex < source.Length; materialIndex++)
                {
                    var name = source[materialIndex] == null ? string.Empty : source[materialIndex].name;
                    remapped[materialIndex] = materials.Get(AuthoredMaterialRole(name));
                }
                renderer.sharedMaterials = remapped;
            }
        }

        private void ConfigureAuthoredUnitMarkers(GameObject model, int markerCount)
        {
            var transforms = model.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < transforms.Length; index++)
            {
                var item = transforms[index];
                var isFirst = item.name.IndexOf("UnitMarker_1", StringComparison.OrdinalIgnoreCase) >= 0;
                var isSecond = item.name.IndexOf("UnitMarker_2", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!isFirst && !isSecond) continue;
                item.gameObject.SetActive(isFirst || markerCount >= 2);
                if (!item.gameObject.activeSelf) continue;
                var accentRole = markerCount >= 2 ? "ally.unit2" : "ally.energy";
                var renderers = item.GetComponentsInChildren<Renderer>(true);
                for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    var accent = materials.Theme == null
                        ? Color.cyan
                        : materials.Theme.ColorFor(accentRole, Color.cyan);
                    materials.ApplyAccent(renderers[rendererIndex], accentRole, accent);
                }
            }
        }

        private static int CountVertices(GameObject model)
        {
            var total = 0;
            var filters = model.GetComponentsInChildren<MeshFilter>(true);
            for (var index = 0; index < filters.Length; index++)
                if (filters[index].sharedMesh != null) total += filters[index].sharedMesh.vertexCount;
            var skinned = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (var index = 0; index < skinned.Length; index++)
                if (skinned[index].sharedMesh != null) total += skinned[index].sharedMesh.vertexCount;
            return total;
        }

        private static string AuthoredMaterialRole(string materialName)
        {
            if (materialName.IndexOf("ZombieFlesh", StringComparison.OrdinalIgnoreCase) >= 0)
                return "enemy.body";
            if (materialName.IndexOf("ZombieArmor", StringComparison.OrdinalIgnoreCase) >= 0)
                return "enemy.armor";
            if (materialName.IndexOf("ZombieTissue", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ally.joint";
            if (materialName.IndexOf("ZombieCorruption", StringComparison.OrdinalIgnoreCase) >= 0)
                return "enemy.corruption";
            if (materialName.IndexOf("ZombieBone", StringComparison.OrdinalIgnoreCase) >= 0)
                return "enemy.ripper";
            if (materialName.IndexOf("IvoryArmor", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ally.ceramic";
            if (materialName.IndexOf("GoldTrim", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ally.haetae";
            if (materialName.IndexOf("CyanEnergy", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ally.energy";
            if (materialName.IndexOf("DarkJoint", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ally.joint";
            return "ally.frame";
        }

        private static void ResetLocalTransform(Transform item)
        {
            item.localPosition = Vector3.zero;
            item.localRotation = Quaternion.identity;
            item.localScale = Vector3.one;
        }

        private static void DestroyPresentationObject(GameObject item)
        {
            if (item == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(item);
            else UnityEngine.Object.DestroyImmediate(item);
        }

        private void BuildMedical(Transform root, VisualIdentityMarker marker)
        {
            marker.silhouetteSignature = "medical.halo.unarmed";
            marker.markerCount = 4;
            CreatePart(root, "Medical Core", PrimitiveType.Cylinder, Vector3.zero,
                new Vector3(0.55f, 0.35f, 0.55f), Vector3.zero, "ally.armor");
            CreatePart(root, "Medical Light", PrimitiveType.Sphere, new Vector3(0f, 0.38f, 0f),
                new Vector3(0.42f, 0.26f, 0.42f), Vector3.zero, "ally.medical");
            for (var index = 0; index < 4; index++)
            {
                var angle = index * 90f;
                var radians = angle * Mathf.Deg2Rad;
                CreatePart(root, "Halo Segment", PrimitiveType.Cube,
                    new Vector3(Mathf.Sin(radians) * 0.64f, 0.32f, Mathf.Cos(radians) * 0.64f),
                    new Vector3(0.38f, 0.06f, 0.12f), new Vector3(0f, angle, 0f), "ally.medical");
            }
        }

        private void BuildEnemy(Transform root, PresentationRole role, VisualIdentityMarker marker)
        {
            if (TryBuildAuthoredZombie(root, role, marker)) return;

            var runner = role == PresentationRole.Runner;
            var bruiser = role == PresentationRole.Bruiser;
            marker.silhouetteSignature = runner ? "runner.lean.fins" : bruiser ? "bruiser.wide.armor" : "ripper.tall.blades";
            marker.markerCount = runner ? 1 : bruiser ? 2 : 3;
            var corruptionRole = role == PresentationRole.Ripper ? "enemy.ripper" : "enemy.corruption";

            CreatePart(root, "Enemy Torso", PrimitiveType.Capsule,
                runner ? new Vector3(0f, 0f, 0.1f) : Vector3.zero,
                runner ? new Vector3(0.48f, 0.74f, 0.38f) :
                bruiser ? new Vector3(0.9f, 0.75f, 0.55f) : new Vector3(0.58f, 0.92f, 0.46f),
                runner ? new Vector3(18f, 0f, 0f) : Vector3.zero, "enemy.body");
            CreatePart(root, "Enemy Head", PrimitiveType.Sphere,
                runner ? new Vector3(0f, 0.78f, 0.25f) : bruiser ? new Vector3(0f, 0.7f, 0.12f) : new Vector3(0f, 0.98f, 0.08f),
                runner ? new Vector3(0.42f, 0.38f, 0.42f) :
                bruiser ? new Vector3(0.58f, 0.5f, 0.55f) : new Vector3(0.45f, 0.42f, 0.45f),
                Vector3.zero, corruptionRole);
            CreatePart(root, "Corruption Core", PrimitiveType.Sphere, new Vector3(0f, 0.2f, 0.42f),
                bruiser ? new Vector3(0.28f, 0.24f, 0.16f) : new Vector3(0.22f, 0.18f, 0.12f),
                Vector3.zero, corruptionRole);

            if (runner)
            {
                CreatePart(root, "Runner Left Fin", PrimitiveType.Cube, new Vector3(-0.38f, 0.25f, -0.05f),
                    new Vector3(0.12f, 0.5f, 0.28f), new Vector3(20f, 0f, -22f), corruptionRole);
                CreatePart(root, "Runner Right Fin", PrimitiveType.Cube, new Vector3(0.38f, 0.25f, -0.05f),
                    new Vector3(0.12f, 0.5f, 0.28f), new Vector3(20f, 0f, 22f), corruptionRole);
                CreatePart(root, "Runner Left Leg", PrimitiveType.Cube, new Vector3(-0.2f, -0.72f, 0f),
                    new Vector3(0.16f, 0.7f, 0.18f), new Vector3(-12f, 0f, 0f), "enemy.body");
                CreatePart(root, "Runner Right Leg", PrimitiveType.Cube, new Vector3(0.2f, -0.72f, 0f),
                    new Vector3(0.16f, 0.7f, 0.18f), new Vector3(-12f, 0f, 0f), "enemy.body");
            }
            else if (bruiser)
            {
                CreatePart(root, "Bruiser Left Shoulder", PrimitiveType.Cube, new Vector3(-0.72f, 0.34f, 0f),
                    new Vector3(0.45f, 0.52f, 0.62f), new Vector3(0f, 0f, -10f), "enemy.armor");
                CreatePart(root, "Bruiser Right Shoulder", PrimitiveType.Cube, new Vector3(0.72f, 0.34f, 0f),
                    new Vector3(0.45f, 0.52f, 0.62f), new Vector3(0f, 0f, 10f), "enemy.armor");
                CreatePart(root, "Bruiser Lower Armor", PrimitiveType.Cube, new Vector3(0f, -0.55f, 0f),
                    new Vector3(0.9f, 0.38f, 0.55f), Vector3.zero, "enemy.armor");
            }
            else
            {
                CreatePart(root, "Ripper Left Blade", PrimitiveType.Cube, new Vector3(-0.56f, 0f, 0.22f),
                    new Vector3(0.13f, 1.05f, 0.28f), new Vector3(12f, 0f, -16f), "enemy.ripper");
                CreatePart(root, "Ripper Right Blade", PrimitiveType.Cube, new Vector3(0.56f, 0f, 0.22f),
                    new Vector3(0.13f, 1.05f, 0.28f), new Vector3(12f, 0f, 16f), "enemy.ripper");
                CreatePart(root, "Ripper Crest", PrimitiveType.Cube, new Vector3(0f, 1.3f, -0.02f),
                    new Vector3(0.12f, 0.42f, 0.32f), new Vector3(-8f, 0f, 0f), "enemy.ripper");
            }
        }

        private bool TryBuildAuthoredZombie(
            Transform root, PresentationRole role, VisualIdentityMarker marker)
        {
            var definition = materials.Theme == null
                ? null
                : materials.Theme.AuthoredZombieFor(role);
            if (definition == null || definition.lod0 == null) return false;

            GameObject lod0 = null;
            GameObject lod1 = null;
            LODGroup lodGroup = null;
            try
            {
                lod0 = UnityEngine.Object.Instantiate(definition.lod0, root, false);
                lod0.name = "Zombie " + role + " Authored LOD0";
                ResetLocalTransform(lod0.transform);
                DisablePresentationColliders(lod0);
                ApplyAuthoredMaterials(lod0);

                var lodCount = 1;
                if (definition.lod1 != null)
                {
                    lod1 = UnityEngine.Object.Instantiate(definition.lod1, root, false);
                    lod1.name = "Zombie " + role + " Authored LOD1";
                    ResetLocalTransform(lod1.transform);
                    DisablePresentationColliders(lod1);
                    ApplyAuthoredMaterials(lod1);

                    lodGroup = root.gameObject.AddComponent<LODGroup>();
                    lodGroup.fadeMode = LODFadeMode.CrossFade;
                    lodGroup.animateCrossFading = false;
                    lodGroup.SetLODs(new[]
                    {
                        new LOD(0.32f, lod0.GetComponentsInChildren<Renderer>(true)),
                        new LOD(0.08f, lod1.GetComponentsInChildren<Renderer>(true))
                    });
                    lodGroup.RecalculateBounds();
                    lodCount = 2;
                }

                marker.silhouetteSignature = definition.silhouetteSignature;
                marker.markerCount = role == PresentationRole.Runner
                    ? 1
                    : role == PresentationRole.Bruiser ? 2 : 3;
                var authored = root.gameObject.AddComponent<AuthoredModelMarker>();
                authored.assetId = definition.assetId;
                authored.sourceVertexCount = CountVertices(lod0);
                authored.lodCount = lodCount;
                return true;
            }
            catch (Exception exception)
            {
                DestroyPresentationObject(lod0);
                DestroyPresentationObject(lod1);
                if (lodGroup != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(lodGroup);
                    else UnityEngine.Object.DestroyImmediate(lodGroup);
                }
                Debug.LogWarning("Authored zombie model fallback: " + exception.Message);
                return false;
            }
        }

        private static void DisablePresentationColliders(GameObject model)
        {
            var colliders = model.GetComponentsInChildren<Collider>(true);
            for (var index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(colliders[index]);
                else UnityEngine.Object.DestroyImmediate(colliders[index]);
            }
        }
    }
}
