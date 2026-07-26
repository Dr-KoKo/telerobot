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

            Assert.DoesNotThrow(theme.Validate);
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
    }
}
