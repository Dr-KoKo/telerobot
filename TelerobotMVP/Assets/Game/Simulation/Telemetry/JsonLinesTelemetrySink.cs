using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Telerobot.Game.Core;

namespace Telerobot.Game.Simulation
{
    public static class TelemetryEventNames
    {
        public static readonly string[] ConstitutionMinimum =
        {
            "session_started", "session_ended", "phase_started", "phase_cleared", "phase_failed",
            "zombie_spawned", "zombie_killed", "base_damaged", "player_damaged", "player_died",
            "robot_battery_changed", "robot_auto_charge_started", "robot_disabled", "ripper_attacked_robot",
            "route_pressure_sampled", "simulation_run_completed",
            "haetae_xp_gained", "haetae_level_reached", "haetae_specialization_ready",
            "haetae_specialization_selected"
        };
    }

    public sealed class JsonLinesTelemetrySink : ITelemetrySink, IDisposable
    {
        private readonly StreamWriter writer;
        private readonly List<TelemetryRecord> pending = new List<TelemetryRecord>();

        public JsonLinesTelemetrySink(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            writer = new StreamWriter(path, false, new UTF8Encoding(false));
        }

        public void Write(TelemetryRecord record)
        {
            pending.Add(record);
        }

        public void Flush()
        {
            foreach (var record in pending) writer.WriteLine(ToJson(record));
            pending.Clear();
            writer.Flush();
        }

        public void Dispose()
        {
            Flush();
            writer.Dispose();
        }

        public static string ToJson(TelemetryRecord record)
        {
            var builder = new StringBuilder();
            builder.Append('{');
            Append(builder, "buildVersion", record.BuildVersion, true);
            Append(builder, "dataVersion", record.DataVersion, false);
            Append(builder, "sessionId", record.SessionId, false);
            Append(builder, "seed", record.Seed.ToString(CultureInfo.InvariantCulture), false, false);
            if (record.SimProfileId == null) builder.Append(",\"simProfileId\":null");
            else Append(builder, "simProfileId", record.SimProfileId, false);
            Append(builder, "phase", record.Phase.ToString(CultureInfo.InvariantCulture), false, false);
            Append(builder, "simTime", record.SimTime.ToString("0.000", CultureInfo.InvariantCulture), false, false);
            Append(builder, "event", record.EventName, false);
            builder.Append(",\"payload\":{");
            var first = true;
            foreach (var pair in record.Payload)
            {
                Append(builder, pair.Key, pair.Value, first);
                first = false;
            }
            builder.Append("}}");
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string key, string value, bool first, bool quote = true)
        {
            if (!first) builder.Append(',');
            builder.Append('\"').Append(Escape(key)).Append("\":");
            if (quote) builder.Append('\"').Append(Escape(value ?? string.Empty)).Append('\"');
            else builder.Append(value);
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }

    public sealed class InMemoryTelemetrySink : ITelemetrySink
    {
        public readonly List<TelemetryRecord> Records = new List<TelemetryRecord>();

        public void Write(TelemetryRecord record)
        {
            Records.Add(record);
        }

        public void Flush() { }

        public string CanonicalText()
        {
            var builder = new StringBuilder();
            foreach (var record in Records) builder.AppendLine(JsonLinesTelemetrySink.ToJson(record));
            return builder.ToString();
        }
    }
}
