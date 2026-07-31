using System.Collections.Generic;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Telerobot.Game.Runtime
{
    [DefaultExecutionOrder(1100)]
    public sealed class HaetaeCameraOcclusionFader : MonoBehaviour
    {
        private sealed class RendererBinding
        {
            public Renderer renderer;
            public Material[] opaqueMaterials;
            public Material[] transparentMaterials;
            public readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
        private readonly List<RendererBinding> bindings = new List<RendererBinding>(24);
        private readonly Dictionary<Material, Material> transparentByOpaque =
            new Dictionary<Material, Material>();
        private ThirdPersonPlayerController player;
        private HaetaeOcclusionFadeDefinition tuning;
        private Transform presentationRoot;
        private bool usesTransparentMaterials;

        public bool IsObstructing { get; private set; }
        public float CurrentOpacity { get; private set; } = 1f;
        public int TrackedRendererCount { get { return bindings.Count; } }
        public int OwnedMaterialCount { get { return transparentByOpaque.Count; } }
        public IEnumerable<Material> OwnedMaterials { get { return transparentByOpaque.Values; } }
        public bool UsesTransparentMaterials { get { return usesTransparentMaterials; } }
        public Transform PresentationRoot { get { return presentationRoot; } }

        public void Initialize(
            ThirdPersonPlayerController playerController,
            VisualThemeDefinitionAsset visualTheme)
        {
            RestoreAndReleaseBindings();
            player = playerController;
            tuning = visualTheme == null ? null : visualTheme.haetaeOcclusionFade;
            IsObstructing = false;
            CurrentOpacity = 1f;
            RefreshPresentationBindings();
            enabled = player != null && tuning != null && tuning.enabled;
        }

        private void LateUpdate()
        {
            RefreshPresentationBindings();
            Tick(EvaluateOcclusion(), Time.deltaTime);
        }

        public bool EvaluateOcclusionForTests()
        {
            RefreshPresentationBindings();
            return EvaluateOcclusion();
        }

        public void TickForTests(bool obstructing, float deltaTime)
        {
            RefreshPresentationBindings();
            Tick(obstructing, deltaTime);
        }

        private bool EvaluateOcclusion()
        {
            if (tuning == null || !tuning.enabled || player == null ||
                player.Perspective != CameraPerspective.ThirdPerson || presentationRoot == null)
                return false;
            var camera = player.ViewCamera;
            if (camera == null) return false;
            var aimRay = new Ray(camera.transform.position, camera.transform.forward);
            for (var index = 0; index < bindings.Count; index++)
            {
                var renderer = bindings[index].renderer;
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;
                var expandedBounds = renderer.bounds;
                expandedBounds.Expand(tuning.aimCorridorRadius * 2f);
                if (expandedBounds.IntersectRay(aimRay, out var distance) &&
                    distance >= 0f && distance <= tuning.maxDistance)
                    return true;
            }
            return false;
        }

        private void Tick(bool obstructing, float deltaTime)
        {
            IsObstructing = tuning != null && tuning.enabled && obstructing;
            var target = IsObstructing ? tuning.obstructingOpacity : 1f;
            var duration = IsObstructing ? tuning.fadeSeconds : tuning.restoreSeconds;
            var maximumDelta = duration <= 0f ? 1f : Mathf.Max(0f, deltaTime) / duration;
            CurrentOpacity = Mathf.MoveTowards(CurrentOpacity, target, maximumDelta);
            ApplyOpacity(CurrentOpacity);
        }

        private void RefreshPresentationBindings()
        {
            var current = transform.Find(LowPolyModelFactory.VisualRootName);
            if (current == presentationRoot) return;
            var retainedOpacity = CurrentOpacity;
            RestoreAndReleaseBindings();
            presentationRoot = current;
            if (presentationRoot == null)
            {
                CurrentOpacity = 1f;
                return;
            }

            var renderers = presentationRoot.GetComponentsInChildren<Renderer>(true);
            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var opaqueMaterials = renderer.sharedMaterials;
                var transparentMaterials = new Material[opaqueMaterials.Length];
                for (var materialIndex = 0; materialIndex < opaqueMaterials.Length; materialIndex++)
                    transparentMaterials[materialIndex] = TransparentVariant(opaqueMaterials[materialIndex]);
                bindings.Add(new RendererBinding
                {
                    renderer = renderer,
                    opaqueMaterials = opaqueMaterials,
                    transparentMaterials = transparentMaterials
                });
            }

            CurrentOpacity = Mathf.Clamp(retainedOpacity,
                tuning == null ? 0.05f : tuning.obstructingOpacity, 1f);
            ApplyOpacity(CurrentOpacity, true);
        }

        private Material TransparentVariant(Material opaque)
        {
            if (opaque == null) return null;
            if (transparentByOpaque.TryGetValue(opaque, out var existing)) return existing;
            var result = new Material(opaque)
            {
                name = opaque.name + " (Haetae Occlusion)",
                renderQueue = (int)RenderQueue.Transparent
            };
            result.SetOverrideTag("RenderType", "Transparent");
            SetFloatIfPresent(result, "_Surface", 1f);
            SetFloatIfPresent(result, "_Blend", 0f);
            SetFloatIfPresent(result, "_BlendModePreserveSpecular", 0f);
            SetFloatIfPresent(result, "_AlphaClip", 0f);
            SetFloatIfPresent(result, "_Mode", 3f);
            SetFloatIfPresent(result, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(result, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(result, "_SrcBlendAlpha", (float)BlendMode.One);
            SetFloatIfPresent(result, "_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(result, "_ZWrite", 0f);
            result.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            result.EnableKeyword("_ALPHABLEND_ON");
            result.DisableKeyword("_ALPHATEST_ON");
            result.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            result.SetShaderPassEnabled("ShadowCaster", false);
            transparentByOpaque.Add(opaque, result);
            return result;
        }

        private void ApplyOpacity(float opacity, bool forceMaterialAssignment = false)
        {
            var shouldUseTransparent = opacity < 0.999f;
            if (forceMaterialAssignment || shouldUseTransparent != usesTransparentMaterials)
            {
                for (var index = 0; index < bindings.Count; index++)
                {
                    var binding = bindings[index];
                    if (binding.renderer == null) continue;
                    binding.renderer.sharedMaterials = shouldUseTransparent
                        ? binding.transparentMaterials
                        : binding.opaqueMaterials;
                }
                usesTransparentMaterials = shouldUseTransparent;
            }

            foreach (var pair in transparentByOpaque)
                SetMaterialAlpha(pair.Value, opacity);

            for (var index = 0; index < bindings.Count; index++)
            {
                var binding = bindings[index];
                if (binding.renderer == null) continue;
                binding.renderer.GetPropertyBlock(binding.propertyBlock);
                if (binding.propertyBlock.isEmpty) continue;
                var color = binding.propertyBlock.GetColor(BaseColorId);
                if (color == Color.clear)
                    color = binding.propertyBlock.GetColor(LegacyColorId);
                if (color == Color.clear) continue;
                color.a = shouldUseTransparent ? opacity : 1f;
                binding.propertyBlock.SetColor(BaseColorId, color);
                binding.propertyBlock.SetColor(LegacyColorId, color);
                binding.renderer.SetPropertyBlock(binding.propertyBlock);
            }
        }

        private static void SetMaterialAlpha(Material material, float opacity)
        {
            if (material == null) return;
            var color = material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : material.color;
            color.a = opacity;
            material.color = color;
            if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, color);
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material.HasProperty(property)) material.SetFloat(property, value);
        }

        private void RestoreAndReleaseBindings()
        {
            if (bindings.Count > 0) ApplyOpacity(1f, true);
            bindings.Clear();
            foreach (var pair in transparentByOpaque)
            {
                if (pair.Value == null) continue;
                if (Application.isPlaying) Destroy(pair.Value);
                else DestroyImmediate(pair.Value);
            }
            transparentByOpaque.Clear();
            presentationRoot = null;
            usesTransparentMaterials = false;
        }

        private void OnDestroy()
        {
            RestoreAndReleaseBindings();
        }
    }
}
