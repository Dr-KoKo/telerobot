using UnityEngine;

namespace Telerobot.Game.Data
{
    public sealed class TelemetryConfigAsset : ScriptableObject
    {
        public string[] enabledEvents;
        public string sinkFolder;
        public string[] requiredFields;
        public float sampleIntervalSeconds;
        public float routePressureSampleIntervalSeconds;
        public Telerobot.Game.Core.BatteryEmitPolicy batteryEmitPolicy;
        public float batteryEmitIntervalSeconds;
    }
}
