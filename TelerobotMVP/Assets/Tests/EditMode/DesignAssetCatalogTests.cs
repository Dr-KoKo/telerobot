using System;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Data;
using UnityEditor;
using UnityEngine;

namespace Telerobot.Game.Tests
{
    public sealed class DesignAssetCatalogTests
    {
        [Test]
        public void VisualTheme_RequiresEverySemanticColor()
        {
            var theme = ScriptableObject.CreateInstance<VisualThemeDefinitionAsset>();
            theme.themeId = "test";
            theme.colors = VisualThemeDefinitionAsset.RequiredColorKeys
                .Select(key => new SemanticColorDefinition { key = key, value = Color.white })
                .ToArray();
            theme.materials = VisualThemeDefinitionAsset.RequiredMaterialKeys
                .Select(key => new MaterialRoleDefinition { key = key, baseColor = Color.white })
                .ToArray();

            Assert.That(theme.haetaeVisualScale, Is.EqualTo(0.85f));
            Assert.That(theme.haetaeOcclusionFade, Is.Not.Null);
            Assert.That(theme.haetaeOcclusionFade.enabled, Is.True);
            Assert.That(theme.haetaeOcclusionFade.obstructingOpacity, Is.EqualTo(0.16f));
            Assert.That(theme.haetaeOcclusionFade.fadeSeconds, Is.EqualTo(0.15f));
            Assert.That(theme.haetaeOcclusionFade.restoreSeconds, Is.EqualTo(0.25f));
            Assert.That(theme.haetaeOcclusionFade.aimCorridorRadius, Is.EqualTo(0.45f));
            Assert.That(theme.haetaeOcclusionFade.maxDistance, Is.EqualTo(35f));
            Assert.DoesNotThrow(theme.Validate);
            theme.haetaeVisualScale = 0f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeVisualScale = 2.01f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeVisualScale = float.NaN;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeVisualScale = 0.85f;
            theme.haetaeOcclusionFade.obstructingOpacity = 1f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeOcclusionFade.obstructingOpacity = 0.16f;
            theme.haetaeOcclusionFade.fadeSeconds = 0f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeOcclusionFade.fadeSeconds = 0.15f;
            theme.haetaeOcclusionFade.restoreSeconds = float.PositiveInfinity;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeOcclusionFade.restoreSeconds = 0.25f;
            theme.haetaeOcclusionFade.aimCorridorRadius = 3.01f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeOcclusionFade.aimCorridorRadius = 0.45f;
            theme.haetaeOcclusionFade.maxDistance = 0f;
            Assert.Throws<InvalidOperationException>(theme.Validate);
            theme.haetaeOcclusionFade.maxDistance = 35f;
            theme.colors[0].key = theme.colors[1].key;
            Assert.Throws<InvalidOperationException>(theme.Validate);
        }

        [Test]
        public void Catalog_RequiresAllRolesAndPlayableFallbacks()
        {
            var catalog = ScriptableObject.CreateInstance<DesignAssetCatalogAsset>();
            catalog.catalogVersion = "test";
            catalog.items = DesignAssetCatalogAsset.RequiredAssetIds
                .Select(id => new DesignAssetItemDefinition
                {
                    id = id,
                    category = DesignAssetCategory.Environment,
                    priority = DesignAssetPriority.P1,
                    decision = DesignAssetDecision.Make,
                    status = DesignAssetStatus.Integrated,
                    generatedRecipe = "Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs",
                    validationTags = new[] { "automated" }
                })
                .ToArray();

            Assert.DoesNotThrow(catalog.Validate);
            catalog.items[0].status = DesignAssetStatus.Deferred;
            Assert.Throws<InvalidOperationException>(catalog.Validate);
            catalog.items[0].fallbackId = catalog.items[1].id;
            Assert.DoesNotThrow(catalog.Validate);
        }

        [Test]
        public void Catalog_RejectsAdoptedAssetWithoutProvenance()
        {
            var catalog = ScriptableObject.CreateInstance<DesignAssetCatalogAsset>();
            catalog.catalogVersion = "test";
            catalog.items = DesignAssetCatalogAsset.RequiredAssetIds
                .Select(id => new DesignAssetItemDefinition
                {
                    id = id,
                    category = DesignAssetCategory.UI,
                    priority = DesignAssetPriority.P2,
                    decision = DesignAssetDecision.Make,
                    status = DesignAssetStatus.Integrated,
                    generatedRecipe = "code",
                    validationTags = new[] { "automated" }
                })
                .ToArray();
            catalog.items[0].decision = DesignAssetDecision.Adopt;
            catalog.items[0].sourceId = "missing";

            Assert.Throws<InvalidOperationException>(catalog.Validate);
            catalog.sources = new[]
            {
                new DesignAssetSourceRecord
                {
                    id = "missing",
                    title = "Test",
                    creator = "Test",
                    officialUrl = "https://example.com",
                    licenseId = "CC0-1.0",
                    licenseEvidence = "https://example.com/license",
                    retrievedOn = "2026-07-26"
                }
            };
            Assert.DoesNotThrow(catalog.Validate);

            catalog.sources[0].licenseEvidence = string.Empty;
            Assert.Throws<InvalidOperationException>(catalog.Validate);
            catalog.sources[0].licenseEvidence = "https://example.com/license";

            catalog.items[0].status = DesignAssetStatus.Rejected;
            Assert.Throws<InvalidOperationException>(catalog.Validate);
            catalog.items[0].generatedRecipe = null;
            catalog.items[0].assetReferences = System.Array.Empty<UnityEngine.Object>();
            Assert.DoesNotThrow(catalog.Validate);
        }

        [Test]
        public void GeneratedThemeAndCatalog_PassContracts()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(
                "Assets/Game/Data/Assets/VisualTheme.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<DesignAssetCatalogAsset>(
                "Assets/Game/Data/Assets/DesignAssetCatalog.asset");

            Assert.That(theme, Is.Not.Null, "Run Tools/Telerobot/Build MVP Project.");
            Assert.That(catalog, Is.Not.Null, "Run Tools/Telerobot/Build MVP Project.");
            Assert.DoesNotThrow(theme.Validate);
            Assert.DoesNotThrow(catalog.Validate);
            Assert.That(catalog.fallbackTheme, Is.SameAs(theme));
            Assert.That(theme.haetaeVisualScale, Is.EqualTo(0.85f));
            Assert.That(theme.haetaeOcclusionFade, Is.Not.Null);
            Assert.That(theme.haetaeOcclusionFade.enabled, Is.True);
            Assert.That(theme.haetaeOcclusionFade.obstructingOpacity, Is.EqualTo(0.16f));
            Assert.That(theme.haetaeOcclusionFade.fadeSeconds, Is.EqualTo(0.15f));
            Assert.That(theme.haetaeOcclusionFade.restoreSeconds, Is.EqualTo(0.25f));
            Assert.That(theme.haetaeOcclusionFade.aimCorridorRadius, Is.EqualTo(0.45f));
            Assert.That(theme.haetaeOcclusionFade.maxDistance, Is.EqualTo(35f));
            Assert.That(catalog.items.Select(item => item.id).Distinct().Count(), Is.EqualTo(catalog.items.Length));
            Assert.That(catalog.sources.All(source => source.officialUrl.StartsWith("https://", StringComparison.Ordinal)), Is.True);
        }

        [Test]
        public void AuthoredHaetae_HasProductionMeshAndTwoLods()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(
                "Assets/Game/Data/Assets/VisualTheme.asset");
            Assert.That(theme, Is.Not.Null, "Run Tools/Telerobot/Build MVP Project.");
            Assert.That(theme.haetaeGeneralModel, Is.Not.Null,
                "Generate the Blender outputs before rebuilding the visual theme.");
            Assert.That(theme.haetaeGeneralLod1, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(theme.haetaeGeneralModel),
                Does.EndWith("Haetae_General_LOD0.fbx"));
            Assert.That(AssetDatabase.GetAssetPath(theme.haetaeGeneralLod1),
                Does.EndWith("Haetae_General_LOD1.fbx"));

            var vertexCount = MeshVertexCount(theme.haetaeGeneralModel);
            var lod1VertexCount = MeshVertexCount(theme.haetaeGeneralLod1);
            var materialNames = theme.haetaeGeneralModel
                .GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct()
                .ToArray();

            Assert.That(vertexCount, Is.GreaterThan(15000));
            Assert.That(lod1VertexCount, Is.GreaterThan(500));
            Assert.That(lod1VertexCount, Is.LessThan(vertexCount * 0.7f));
            Assert.That(materialNames.Length, Is.EqualTo(5));
            var authoredBody = theme.haetaeGeneralModel
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(renderer => renderer.sharedMesh != null
                    && renderer.sharedMesh.vertexCount > 15000);
            Assert.That(authoredBody, Is.Not.Null);
            Assert.That(authoredBody.sharedMesh.subMeshCount, Is.EqualTo(5));
            Assert.That(
                Enumerable.Range(0, authoredBody.sharedMesh.subMeshCount)
                    .All(index => authoredBody.sharedMesh.GetIndexCount(index) > 0),
                Is.True,
                "Every semantic material must be used by visible geometry.");
            var hierarchyNames = theme.haetaeGeneralModel
                .GetComponentsInChildren<Transform>(true)
                .Select(item => item.name)
                .ToArray();
            Assert.That(hierarchyNames, Does.Contain("head"));
            Assert.That(hierarchyNames, Does.Contain("leg_lf"));
            Assert.That(hierarchyNames, Does.Contain("leg_rf"));
            Assert.That(hierarchyNames, Does.Contain("tail_06"));
        }

        [Test]
        public void AuthoredHaetaeUpgrades_HaveProductionMeshesAndRoleMappings()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(
                "Assets/Game/Data/Assets/VisualTheme.asset");
            Assert.That(theme, Is.Not.Null, "Run Tools/Telerobot/Build MVP Project.");
            Assert.That(theme.haetaeUpgradeModels, Is.Not.Null);
            Assert.That(theme.haetaeUpgradeModels.Length, Is.EqualTo(3));
            Assert.That(theme.haetaeUpgradeModels.Select(item => item.role).Distinct().Count(),
                Is.EqualTo(3));
            Assert.That(theme.haetaeUpgradeModels.Select(item => item.assetId).Distinct().Count(),
                Is.EqualTo(3));

            var expected = new[]
            {
                new
                {
                    Role = PresentationRole.HaetaeMeleePreview,
                    AssetId = "character.haetae.melee",
                    Stem = "Haetae_Melee"
                },
                new
                {
                    Role = PresentationRole.HaetaeRangedPreview,
                    AssetId = "character.haetae.ranged",
                    Stem = "Haetae_Ranged"
                },
                new
                {
                    Role = PresentationRole.HaetaeBalancedPreview,
                    AssetId = "character.haetae.balanced",
                    Stem = "Haetae_Balanced"
                }
            };

            foreach (var item in expected)
            {
                var definition = theme.AuthoredHaetaeFor(item.Role);
                Assert.That(definition, Is.Not.Null, item.Role.ToString());
                Assert.That(definition.assetId, Is.EqualTo(item.AssetId));
                Assert.That(definition.silhouetteSignature, Is.Not.Empty);
                Assert.That(definition.lod0, Is.Not.Null);
                Assert.That(definition.lod1, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(definition.lod0),
                    Does.EndWith(item.Stem + "_LOD0.fbx"));
                Assert.That(AssetDatabase.GetAssetPath(definition.lod1),
                    Does.EndWith(item.Stem + "_LOD1.fbx"));
                AssertAuthoredUpgradeContract(definition.lod0, definition.lod1);
            }
        }

        [Test]
        public void AuthoredZombies_HaveProductionMeshesAndRoleMappings()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(
                "Assets/Game/Data/Assets/VisualTheme.asset");
            Assert.That(theme, Is.Not.Null, "Run Tools/Telerobot/Build MVP Project.");
            Assert.That(theme.authoredZombieModels, Is.Not.Null);
            Assert.That(theme.authoredZombieModels.Length, Is.EqualTo(3));
            Assert.That(theme.authoredZombieModels.Select(item => item.role).Distinct().Count(),
                Is.EqualTo(3));
            Assert.That(theme.authoredZombieModels.Select(item => item.assetId).Distinct().Count(),
                Is.EqualTo(3));
            Assert.That(theme.authoredZombieModels
                .Select(item => item.silhouetteSignature).Distinct().Count(), Is.EqualTo(3));

            var expected = new[]
            {
                new { Role = PresentationRole.Runner, AssetId = "enemy.runner", Stem = "Zombie_Runner" },
                new { Role = PresentationRole.Bruiser, AssetId = "enemy.bruiser", Stem = "Zombie_Bruiser" },
                new { Role = PresentationRole.Ripper, AssetId = "enemy.ripper", Stem = "Zombie_Ripper" }
            };

            foreach (var item in expected)
            {
                var definition = theme.AuthoredZombieFor(item.Role);
                Assert.That(definition, Is.Not.Null, item.Role.ToString());
                Assert.That(definition.assetId, Is.EqualTo(item.AssetId));
                Assert.That(definition.silhouetteSignature, Is.Not.Empty);
                Assert.That(definition.lod0, Is.Not.Null);
                Assert.That(definition.lod1, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(definition.lod0),
                    Does.EndWith(item.Stem + "_LOD0.fbx"));
                Assert.That(AssetDatabase.GetAssetPath(definition.lod1),
                    Does.EndWith(item.Stem + "_LOD1.fbx"));
                AssertAuthoredZombieContract(definition.lod0, definition.lod1);
            }
        }

        [Test]
        public void VisualDefinitions_DoNotOwnGameplayBalance()
        {
            var forbidden = new[] { "damage", "health", "battery", "spawn", "attackrange", "movespeed", "experience" };
            var fields = typeof(VisualThemeDefinitionAsset).GetFields()
                .Concat(typeof(DesignAssetCatalogAsset).GetFields())
                .Select(field => field.Name.ToLowerInvariant()).ToArray();

            Assert.That(fields.Any(field => forbidden.Any(field.Contains)), Is.False);
        }

        private static int MeshVertexCount(GameObject model)
        {
            return model.GetComponentsInChildren<MeshFilter>(true)
                       .Where(filter => filter.sharedMesh != null)
                       .Sum(filter => filter.sharedMesh.vertexCount) +
                   model.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                       .Where(renderer => renderer.sharedMesh != null)
                       .Sum(renderer => renderer.sharedMesh.vertexCount);
        }

        private static void AssertAuthoredUpgradeContract(GameObject lod0, GameObject lod1)
        {
            var vertexCount = MeshVertexCount(lod0);
            var lod1VertexCount = MeshVertexCount(lod1);
            Assert.That(vertexCount, Is.GreaterThan(18000));
            Assert.That(lod1VertexCount, Is.GreaterThan(500));
            Assert.That(lod1VertexCount, Is.LessThan(vertexCount * 0.7f));

            var materialNames = lod0.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct()
                .ToArray();
            Assert.That(materialNames.Length, Is.EqualTo(5));

            var body = lod0.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(renderer => renderer.sharedMesh != null
                    && renderer.sharedMesh.vertexCount > 18000);
            Assert.That(body, Is.Not.Null);
            Assert.That(body.sharedMesh.subMeshCount, Is.EqualTo(5));
            Assert.That(Enumerable.Range(0, body.sharedMesh.subMeshCount)
                .All(index => body.sharedMesh.GetIndexCount(index) > 0), Is.True);

            var hierarchy = lod0.GetComponentsInChildren<Transform>(true)
                .Select(item => item.name)
                .ToArray();
            foreach (var required in new[]
                     {
                         "head", "leg_lf", "leg_rf", "leg_lb", "leg_rb",
                         "tail_06", "UnitMarker_1", "UnitMarker_2"
                     })
                Assert.That(hierarchy, Does.Contain(required));
        }

        private static void AssertAuthoredZombieContract(GameObject lod0, GameObject lod1)
        {
            var vertexCount = MeshVertexCount(lod0);
            var lod1VertexCount = MeshVertexCount(lod1);
            Assert.That(vertexCount, Is.GreaterThan(16000));
            Assert.That(lod1VertexCount, Is.GreaterThan(500));
            Assert.That(lod1VertexCount, Is.LessThan(vertexCount * 0.7f));

            var body = lod0.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(renderer => renderer.sharedMesh != null &&
                                            renderer.sharedMesh.vertexCount > 16000);
            Assert.That(body, Is.Not.Null);
            Assert.That(body.sharedMesh.subMeshCount, Is.EqualTo(5));
            Assert.That(Enumerable.Range(0, body.sharedMesh.subMeshCount)
                .All(index => body.sharedMesh.GetIndexCount(index) > 0), Is.True);

            var materialNames = body.sharedMaterials
                .Where(material => material != null)
                .Select(material => material.name)
                .ToArray();
            foreach (var required in new[]
                     {
                         "MAT_ZombieFlesh", "MAT_ZombieArmor", "MAT_ZombieTissue",
                         "MAT_ZombieCorruption", "MAT_ZombieBone"
                     })
                Assert.That(materialNames.Any(name => name.StartsWith(required, StringComparison.Ordinal)),
                    Is.True, required);

            var hierarchy = lod0.GetComponentsInChildren<Transform>(true)
                .Select(item => item.name)
                .ToArray();
            foreach (var required in new[]
                     {
                         "hips", "spine", "chest", "neck", "head",
                         "upper_arm_l", "lower_arm_l", "hand_l",
                         "upper_arm_r", "lower_arm_r", "hand_r",
                         "thigh_l", "shin_l", "foot_l",
                         "thigh_r", "shin_r", "foot_r"
                     })
                Assert.That(hierarchy, Does.Contain(required));

            Assert.That(lod0.GetComponentsInChildren<Collider>(true)
                .All(collider => !collider.enabled), Is.True);
        }
    }
}
