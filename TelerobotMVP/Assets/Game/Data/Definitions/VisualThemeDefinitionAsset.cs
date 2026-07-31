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

    [Serializable]
    public sealed class AuthoredZombieModelDefinition
    {
        public PresentationRole role;
        public string assetId;
        public GameObject lod0;
        public GameObject lod1;
        public string silhouetteSignature;
    }

    [Serializable]
    public sealed class CharacterMotionProfileDefinition
    {
        public PresentationRole role;
        public string profileId;
        [Min(0.05f)] public float cycleHz = 1f;
        [Range(0f, 0.2f)] public float idleBob = 0.015f;
        [Range(0f, 0.3f)] public float locomotionBob = 0.06f;
        [Range(0f, 20f)] public float swayDegrees = 3f;
        [Range(-30f, 30f)] public float forwardLeanDegrees;
        [Range(0f, 70f)] public float strideDegrees = 18f;
        [Range(0f, 90f)] public float attackDegrees = 35f;
        [Range(0f, 0.5f)] public float attackRecoil = 0.12f;
        [Range(0f, 45f)] public float hitDegrees = 14f;
        [Range(0f, 120f)] public float deathDegrees = 75f;
        [Min(0.05f)] public float attackDuration = 0.34f;
        [Min(0.05f)] public float hitDuration = 0.16f;
    }

    [Serializable]
    public sealed class HaetaeOcclusionFadeDefinition
    {
        public bool enabled = true;
        [Range(0.05f, 0.95f)] public float obstructingOpacity = 0.10f;
        [Range(0.01f, 2f)] public float fadeSeconds = 0.15f;
        [Range(0.01f, 2f)] public float restoreSeconds = 0.25f;
        [Range(0.01f, 3f)] public float aimCorridorRadius = 0.45f;
        [Range(1f, 200f)] public float maxDistance = 35f;

        public void Validate()
        {
            if (!IsFiniteInRange(obstructingOpacity, 0.05f, 0.95f) ||
                !IsFiniteInRange(fadeSeconds, 0.01f, 2f) ||
                !IsFiniteInRange(restoreSeconds, 0.01f, 2f) ||
                !IsFiniteInRange(aimCorridorRadius, 0.01f, 3f) ||
                !IsFiniteInRange(maxDistance, 1f, 200f))
                throw new InvalidOperationException(
                    "Haetae occlusion fade values must be finite and within their supported ranges.");
        }

        private static bool IsFiniteInRange(float value, float minimum, float maximum)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) &&
                   value >= minimum && value <= maximum;
        }
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
        [Header("Character Scale")]
        [Range(0.01f, 2f)]
        public float haetaeVisualScale = 0.80f;
        [Header("Haetae Camera Occlusion")]
        public HaetaeOcclusionFadeDefinition haetaeOcclusionFade =
            new HaetaeOcclusionFadeDefinition();
        public Material haetaeOcclusionMaterialTemplate;
        [Header("Authored Character Models")]
        public GameObject haetaeGeneralModel;
        public GameObject haetaeGeneralLod1;
        public AuthoredHaetaeModelDefinition[] haetaeUpgradeModels =
            Array.Empty<AuthoredHaetaeModelDefinition>();
        public AuthoredZombieModelDefinition[] authoredZombieModels =
            Array.Empty<AuthoredZombieModelDefinition>();
        [Header("Character Motion")]
        public CharacterMotionProfileDefinition[] characterMotionProfiles =
            Array.Empty<CharacterMotionProfileDefinition>();

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

        public AuthoredZombieModelDefinition AuthoredZombieFor(PresentationRole role)
        {
            if (authoredZombieModels == null) return null;
            for (var index = 0; index < authoredZombieModels.Length; index++)
            {
                var item = authoredZombieModels[index];
                if (item != null && item.role == role) return item;
            }
            return null;
        }

        public CharacterMotionProfileDefinition MotionProfileFor(PresentationRole role)
        {
            if (characterMotionProfiles == null) return null;
            for (var index = 0; index < characterMotionProfiles.Length; index++)
            {
                var item = characterMotionProfiles[index];
                if (item != null && item.role == role) return item;
            }
            return null;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(themeId)) throw new InvalidOperationException("Visual theme ID is required.");
            if (float.IsNaN(haetaeVisualScale) || float.IsInfinity(haetaeVisualScale) ||
                haetaeVisualScale <= 0f || haetaeVisualScale > 2f)
                throw new InvalidOperationException(
                    "Haetae visual scale must be finite, greater than zero, and at most two.");
            if (haetaeOcclusionFade == null)
                throw new InvalidOperationException("Haetae occlusion fade definition is required.");
            haetaeOcclusionFade.Validate();
            if (haetaeOcclusionMaterialTemplate == null ||
                haetaeOcclusionMaterialTemplate.shader == null)
                throw new InvalidOperationException(
                    "Haetae occlusion requires a transparent material template retained by player builds.");
            ValidateUniqueKeys(colors, item => item == null ? null : item.key, RequiredColorKeys, "color");
            ValidateUniqueKeys(materials, item => item == null ? null : item.key, RequiredMaterialKeys, "material");
            ValidateUniqueKeys(effects, item => item == null ? null : item.key, Array.Empty<string>(), "effect");
            ValidateAuthoredHaetaeModels();
            ValidateAuthoredZombieModels();
            ValidateCharacterMotionProfiles();
        }

        private void ValidateCharacterMotionProfiles()
        {
            if (characterMotionProfiles == null)
                throw new InvalidOperationException("Character motion profiles cannot be null.");
            var roles = new HashSet<PresentationRole>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < characterMotionProfiles.Length; index++)
            {
                var item = characterMotionProfiles[index];
                if (item == null || !SupportsCharacterMotion(item.role) ||
                    !roles.Add(item.role) || string.IsNullOrWhiteSpace(item.profileId) ||
                    !ids.Add(item.profileId) || item.cycleHz <= 0f ||
                    item.attackDuration <= 0f || item.hitDuration <= 0f)
                    throw new InvalidOperationException(
                        "Character motion profiles require unique supported roles, IDs and positive timing.");
            }
        }

        public static bool SupportsCharacterMotion(PresentationRole role)
        {
            return role == PresentationRole.HaetaeGeneralUnit1 ||
                   role == PresentationRole.HaetaeGeneralUnit2 ||
                   role == PresentationRole.HaetaeMeleePreview ||
                   role == PresentationRole.HaetaeRangedPreview ||
                   role == PresentationRole.HaetaeBalancedPreview ||
                   role == PresentationRole.Runner ||
                   role == PresentationRole.Bruiser ||
                   role == PresentationRole.Ripper;
        }

        private void ValidateAuthoredZombieModels()
        {
            if (authoredZombieModels == null)
                throw new InvalidOperationException("Authored zombie model entries cannot be null.");
            var roles = new HashSet<PresentationRole>();
            var assetIds = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < authoredZombieModels.Length; index++)
            {
                var item = authoredZombieModels[index];
                if (item == null ||
                    (item.role != PresentationRole.Runner &&
                     item.role != PresentationRole.Bruiser &&
                     item.role != PresentationRole.Ripper) ||
                    !roles.Add(item.role) ||
                    string.IsNullOrWhiteSpace(item.assetId) ||
                    !assetIds.Add(item.assetId) ||
                    string.IsNullOrWhiteSpace(item.silhouetteSignature) ||
                    !signatures.Add(item.silhouetteSignature))
                    throw new InvalidOperationException(
                        "Authored zombie models require unique enemy roles, asset IDs and signatures.");
            }
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
