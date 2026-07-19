using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class UpgradeDefinitionAsset : ScriptableObject
    {
        public string id;
        public string displayNameKey;
        public UpgradeEffectType effectType;
        public float amount;
    }
}
