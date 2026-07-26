using System;
using System.Collections.Generic;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class PresentationMaterialLibrary : IDisposable
    {
        private readonly VisualThemeDefinitionAsset theme;
        private readonly Material template;
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>(StringComparer.Ordinal);

        public PresentationMaterialLibrary(VisualThemeDefinitionAsset visualTheme, Material fallbackTemplate)
        {
            theme = visualTheme;
            template = fallbackTemplate;
        }

        public VisualThemeDefinitionAsset Theme { get { return theme; } }
        public int MaterialCount { get { return materials.Count; } }

        public Material Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) key = "world.structure";
            if (materials.TryGetValue(key, out var existing)) return existing;

            var definition = theme == null ? null : theme.MaterialFor(key);
            Material result;
            if (definition != null && definition.material != null) result = new Material(definition.material);
            else if (template != null) result = new Material(template);
            else
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null) throw new InvalidOperationException("No supported presentation shader is available.");
                result = new Material(shader);
            }

            result.name = "Runtime Visual " + key;
            result.enableInstancing = true;
            var baseColor = definition == null
                ? theme == null ? Color.gray : theme.ColorFor(key, Color.gray)
                : definition.baseColor;
            result.color = baseColor;
            if (result.HasProperty("_BaseColor")) result.SetColor("_BaseColor", baseColor);
            if (definition != null)
            {
                if (result.HasProperty("_Metallic")) result.SetFloat("_Metallic", definition.metallic);
                if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", definition.smoothness);
                if (definition.emissionIntensity > 0f && result.HasProperty("_EmissionColor"))
                {
                    result.EnableKeyword("_EMISSION");
                    result.SetColor("_EmissionColor", definition.emissionColor * definition.emissionIntensity);
                }
            }

            materials.Add(key, result);
            return result;
        }

        public void Apply(Renderer renderer, string key)
        {
            if (renderer != null) renderer.sharedMaterial = Get(key);
        }

        public void ApplyAccent(Renderer renderer, string key, Color accent)
        {
            if (renderer == null) return;
            renderer.sharedMaterial = Get(key);
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", accent);
            block.SetColor("_Color", accent);
            renderer.SetPropertyBlock(block);
        }

        public void Dispose()
        {
            foreach (var pair in materials)
            {
                if (pair.Value == null) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(pair.Value);
                else UnityEngine.Object.DestroyImmediate(pair.Value);
            }
            materials.Clear();
        }
    }
}
