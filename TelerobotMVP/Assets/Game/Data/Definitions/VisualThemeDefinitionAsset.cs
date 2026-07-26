using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public enum PresentationRole
    {
        PlayerCommander,
        AssaultRifle,
        HaetaeGeneralUnit1,
        HaetaeGeneralUnit2,
        HaetaeMeleePreview,
        HaetaeRangedPreview,
        HaetaeBalancedPreview,
        MedicalRobot,
        Runner,
        Bruiser,
        Ripper,
        CentralBase,
        ChargingStation,
        SafeSupply,
        RiskySupply,
        EmergencyBarrier,
        NorthRoute,
        EastRoute,
        SouthRoute
    }

    [Serializable]
    public sealed class SemanticColorDefinition
    {
        public string key;
        public Color value = Color.white;
    }

    [Serializable]
    public sealed class MaterialRoleDefinition
    {
        public string key;
        public Color baseColor = Color.white;
        public Color emissionColor = Color.black;
        [Range(0f, 8f)] public float emissionIntensity;
        [Range(0f, 1f)] public float metallic;
        [Range(0f, 1f)] public float smoothness = 0.35f;
        public Material material;
    }

    [Serializable]
    public sealed class EffectStyleDefinition
    {
        public string key;
        public string colorKey;
        [Min(0.02f)] public float duration = 0.2f;
        [Min(0.02f)] public float size = 0.5f;
        [Range(1, 128)] public int maximumConcurrent = 24;
    }

    [Serializable]
    public sealed class AuthoredHaetaeModelDefinition
    {
        public PresentationRole role;
        public string assetId;
        public GameObject lod0;
        public GameObject lod1;
        public string silhouetteSignature;
    }

    [CreateAssetMenu(menuName = "Telerobot/Visual Theme")]
    public sealed class VisualThemeDefinitionAsset : ScriptableObject
    {
        public static readonly string[] RequiredColorKeys =
        {
            "world.ground", "world.structure", "world.trim",
            "ally.frame", "ally.ceramic", "ally.joint",
            "ally.energy", "ally.haetae", "ally.unit2", "ally.medical",
            "enemy.body", "enemy.corruption", "enemy.ripper",
            "route.north", "route.east", "route.south",
            "state.safe", "state.caution", "state.danger",
            "ui.panel", "ui.line", "ui.text", "ui.muted"
        };

        public static readonly string[] RequiredMaterialKeys =
        {
            "world.ground", "world.structure", "world.trim",
            "ally.armor", "ally.frame", "ally.ceramic", "ally.joint",
            "ally.energy", "ally.unit2", "ally.medical",
            "enemy.body", "enemy.armor", "enemy.corruption", "enemy.ripper",
            "state.safe", "state.caution", "state.danger", "ui.panel"
        };

        public string themeId = "guardian-night-v1";
        public SemanticColorDefinition[] colors = Array.Empty<SemanticColorDefinition>();
        public MaterialRoleDefinition[] materials = Array.Empty<MaterialRoleDefinition>();
        public EffectStyleDefinition[] effects = Array.Empty<EffectStyleDefinition>();
        public Texture2D menuBackdrop;
        public Font bodyFont;
        public Font headingFont;
        [Header("Authored Character Models")]
        public GameObject haetaeGeneralModel;
        public GameObject haetaeGeneralLod1;
        public AuthoredHaetaeModelDefinition[] haetaeUpgradeModels =
            Array.Empty<AuthoredHaetaeModelDefinition>();

        public Color ColorFor(string key, Color fallback)
        {
            if (colors == null) return fallback;
            for (var index = 0; index < colors.Length; index++)
            {
                var item = colors[index];
                if (item != null && string.Equals(item.key, key, StringComparison.Ordinal)) return item.value;
            }
            return fallback;
        }

        public MaterialRoleDefinition MaterialFor(string key)
        {
            if (materials == null) return null;
            for (var index = 0; index < materials.Length; index++)
            {
                var item = materials[index];
                if (item != null && string.Equals(item.key, key, StringComparison.Ordinal)) return item;
            }
            return null;
        }

        public EffectStyleDefinition EffectFor(string key)
        {
            if (effects == null) return null;
            for (var index = 0; index < effects.Length; index++)
            {
                var item = effects[index];
                if (item != null && string.Equals(item.key, key, StringComparison.Ordinal)) return item;
            }
            return null;
        }

        public AuthoredHaetaeModelDefinition AuthoredHaetaeFor(PresentationRole role)
        {
            if (haetaeUpgradeModels == null) return null;
            for (var index = 0; index < haetaeUpgradeModels.Length; index++)
            {
                var item = haetaeUpgradeModels[index];
                if (item != null && item.role == role) return item;
            }
            return null;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(themeId)) throw new InvalidOperationException("Visual theme ID is required.");
            ValidateUniqueKeys(colors, item => item == null ? null : item.key, RequiredColorKeys, "color");
            ValidateUniqueKeys(materials, item => item == null ? null : item.key, RequiredMaterialKeys, "material");
            ValidateUniqueKeys(effects, item => item == null ? null : item.key, Array.Empty<string>(), "effect");
            ValidateAuthoredHaetaeModels();
        }

        private void ValidateAuthoredHaetaeModels()
        {
            if (haetaeUpgradeModels == null)
                throw new InvalidOperationException("Authored haetae model entries cannot be null.");
            var roles = new HashSet<PresentationRole>();
            var assetIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < haetaeUpgradeModels.Length; index++)
            {
                var item = haetaeUpgradeModels[index];
                if (item == null ||
                    (item.role != PresentationRole.HaetaeMeleePreview &&
                     item.role != PresentationRole.HaetaeRangedPreview &&
                     item.role != PresentationRole.HaetaeBalancedPreview) ||
                    !roles.Add(item.role) ||
                    string.IsNullOrWhiteSpace(item.assetId) ||
                    !assetIds.Add(item.assetId) ||
                    string.IsNullOrWhiteSpace(item.silhouetteSignature))
                    throw new InvalidOperationException(
                        "Authored haetae models require unique upgrade roles, asset IDs and signatures.");
            }
        }

        private static void ValidateUniqueKeys<T>(T[] entries, Func<T, string> keyOf, IEnumerable<string> required, string label)
        {
            if (entries == null) throw new InvalidOperationException("Visual theme " + label + " entries are required.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < entries.Length; index++)
            {
                var key = keyOf(entries[index]);
                if (string.IsNullOrWhiteSpace(key) || !keys.Add(key))
                    throw new InvalidOperationException("Visual theme has an empty or duplicate " + label + " key.");
            }
            foreach (var key in required)
                if (!keys.Contains(key)) throw new InvalidOperationException("Visual theme is missing " + label + " key: " + key);
        }
    }
}
