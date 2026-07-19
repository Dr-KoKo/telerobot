using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class ValidationConfigAsset : ScriptableObject
    {
        public int[] seeds;
        public float fixedStepSeconds;
    }
}
