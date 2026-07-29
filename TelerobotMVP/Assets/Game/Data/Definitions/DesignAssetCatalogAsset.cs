using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public enum DesignAssetCategory
    {
        Character,
        Enemy,
        Environment,
        Equipment,
        UI,
        VFX,
        Animation,
        Audio,
        Font
    }

    public enum DesignAssetPriority { P1, P2, P3 }
    public enum DesignAssetDecision { Make, Find, Adopt, Defer }
    public enum DesignAssetStatus { Missing, Candidate, InProduction, Integrated, Validated, Deferred, Rejected }

    [Serializable]
    public sealed class DesignAssetItemDefinition
    {
        public string id;
        public string displayName;
        public DesignAssetCategory category;
        public DesignAssetPriority priority;
        public DesignAssetDecision decision;
        public DesignAssetStatus status;
        public string[] usageRoles = Array.Empty<string>();
        public UnityEngine.Object[] assetReferences = Array.Empty<UnityEngine.Object>();
        public string generatedRecipe;
        public string sourceId;
        public string fallbackId;
        public string[] validationTags = Array.Empty<string>();
        [TextArea] public string notes;
    }

    [Serializable]
    public sealed class DesignAssetSourceRecord
    {
        public string id;
        public string title;
        public string creator;
        public string officialUrl;
        public string licenseId;
        public string licenseEvidence;
        public string retrievedOn;
        public string[] originalFiles = Array.Empty<string>();
        public string[] modifications = Array.Empty<string>();
        public bool attributionRequired;
        [TextArea] public string noticeText;
        [TextArea] public string redistributionNotes;
    }

    [CreateAssetMenu(menuName = "Telerobot/Design Asset Catalog")]
    public sealed class DesignAssetCatalogAsset : ScriptableObject
    {
        public static readonly string[] RequiredAssetIds =
        {
            "character.player.commander", "character.player.assault-rifle",
            "character.haetae.unit-1", "character.haetae.unit-2",
            "character.haetae.melee", "character.haetae.ranged", "character.haetae.balanced",
            "character.medical.robot",
            "enemy.runner", "enemy.bruiser", "enemy.ripper",
            "environment.base.central", "environment.route.north", "environment.route.east", "environment.route.south",
            "interactable.charging", "interactable.supply.safe", "interactable.supply.risky", "interactable.barrier",
            "ui.surface.menu", "ui.surface.settings", "ui.surface.combat", "ui.surface.command",
            "ui.surface.specialization", "ui.surface.result",
            "ui.icon.health", "ui.icon.ammo", "ui.icon.grenade", "ui.icon.base", "ui.icon.battery",
            "ui.icon.xp", "ui.icon.melee", "ui.icon.ranged", "ui.icon.balanced",
            "ui.icon.defend", "ui.icon.patrol", "ui.icon.return",
            "ui.icon.route-north", "ui.icon.route-east", "ui.icon.route-south", "ui.icon.warning",
            "vfx.combat", "vfx.robot-state", "vfx.enemy-state",
            "animation.player", "animation.zombie", "animation.haetae", "animation.medical",
            "audio.weapon", "audio.robot", "audio.enemy", "audio.ui", "audio.ambience",
            "font.korean.body", "font.korean.heading"
        };

        public string catalogVersion = "design-assets-1.0.0";
        public DesignAssetItemDefinition[] items = Array.Empty<DesignAssetItemDefinition>();
        public DesignAssetSourceRecord[] sources = Array.Empty<DesignAssetSourceRecord>();
        public VisualThemeDefinitionAsset fallbackTheme;

        public DesignAssetItemDefinition Find(string id)
        {
            if (items == null) return null;
            for (var index = 0; index < items.Length; index++)
                if (items[index] != null && string.Equals(items[index].id, id, StringComparison.Ordinal)) return items[index];
            return null;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(catalogVersion)) throw new InvalidOperationException("Design asset catalog version is required.");
            if (items == null) throw new InvalidOperationException("Design asset items are required.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                if (item == null || string.IsNullOrWhiteSpace(item.id) || !ids.Add(item.id))
                    throw new InvalidOperationException("Design asset catalog has an empty or duplicate item ID.");
                if (item.validationTags == null || item.validationTags.Length == 0)
                    throw new InvalidOperationException("Design asset item requires validation tags: " + item.id);
                if ((item.status == DesignAssetStatus.Deferred || item.status == DesignAssetStatus.Missing) &&
                    string.IsNullOrWhiteSpace(item.fallbackId))
                    throw new InvalidOperationException("Deferred or missing design asset requires a fallback: " + item.id);
                if (item.status == DesignAssetStatus.Integrated || item.status == DesignAssetStatus.Validated)
                {
                    var hasReference = item.assetReferences != null && item.assetReferences.Length > 0;
                    if (!hasReference && string.IsNullOrWhiteSpace(item.generatedRecipe))
                        throw new InvalidOperationException("Integrated design asset needs a local reference or generated recipe: " + item.id);
                }
                if (item.status == DesignAssetStatus.Rejected &&
                    ((item.assetReferences != null && item.assetReferences.Length > 0) ||
                     !string.IsNullOrWhiteSpace(item.generatedRecipe)))
                    throw new InvalidOperationException("Rejected design asset cannot retain an active runtime reference: " + item.id);
            }

            for (var index = 0; index < RequiredAssetIds.Length; index++)
                if (!ids.Contains(RequiredAssetIds[index]))
                    throw new InvalidOperationException("Design asset catalog is missing required item: " + RequiredAssetIds[index]);

            var sourceIds = new HashSet<string>(StringComparer.Ordinal);
            if (sources != null)
            {
                for (var index = 0; index < sources.Length; index++)
                {
                    var source = sources[index];
                    if (source == null || string.IsNullOrWhiteSpace(source.id) || !sourceIds.Add(source.id))
                        throw new InvalidOperationException("Design asset catalog has an empty or duplicate source ID.");
                    if (string.IsNullOrWhiteSpace(source.title) || string.IsNullOrWhiteSpace(source.creator) ||
                        string.IsNullOrWhiteSpace(source.officialUrl) || string.IsNullOrWhiteSpace(source.licenseId) ||
                        string.IsNullOrWhiteSpace(source.licenseEvidence) || string.IsNullOrWhiteSpace(source.retrievedOn))
                        throw new InvalidOperationException("Design asset source is incomplete: " + source.id);
                }
            }

            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                if (item.decision == DesignAssetDecision.Adopt && !sourceIds.Contains(item.sourceId ?? string.Empty))
                    throw new InvalidOperationException("Adopted design asset lacks provenance: " + item.id);
                if (!string.IsNullOrWhiteSpace(item.fallbackId) && !ids.Contains(item.fallbackId))
                    throw new InvalidOperationException("Design asset fallback does not exist: " + item.id);
            }
        }
    }
}
