namespace Telerobot.Game.Core
{
    public sealed class WarningSystem
    {
        private readonly WarningConfig config;
        private readonly System.Collections.Generic.Dictionary<string, WarningSeverity> previousBattery =
            new System.Collections.Generic.Dictionary<string, WarningSeverity>();
        private bool previousBase;

        public WarningSystem(WarningConfig config)
        {
            this.config = config;
        }

        public WarningSeverity BatterySeverity(float current, float maximum)
        {
            if (maximum <= 0f) return WarningSeverity.Red;
            var fraction = current / maximum;
            if (fraction < config.BatteryRedFraction) return WarningSeverity.Red;
            if (fraction < config.BatteryYellowFraction) return WarningSeverity.Yellow;
            return WarningSeverity.None;
        }

        public bool TryBatteryTransition(float current, float maximum, out WarningSeverity severity)
        {
            return TryBatteryTransition("default", current, maximum, out severity);
        }

        public bool TryBatteryTransition(string robotId, float current, float maximum, out WarningSeverity severity)
        {
            severity = BatterySeverity(current, maximum);
            WarningSeverity previous;
            previousBattery.TryGetValue(robotId, out previous);
            if (severity == previous) return false;
            previousBattery[robotId] = severity;
            return true;
        }

        public bool IsBaseWarning(float current, float maximum)
        {
            return maximum > 0f && current / maximum <= config.BaseWarningFraction;
        }

        public bool TryBaseTransition(float current, float maximum, out bool active)
        {
            active = IsBaseWarning(current, maximum);
            if (active == previousBase) return false;
            previousBase = active;
            return true;
        }
    }
}
