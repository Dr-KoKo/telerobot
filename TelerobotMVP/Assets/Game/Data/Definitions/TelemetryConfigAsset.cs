using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class TelemetryConfigAsset : ScriptableObject
    {
        public string[] enabledEvents;
        public string sinkFolder;
    }
}
