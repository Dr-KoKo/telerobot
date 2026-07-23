using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public interface IDeterministicRng
    {
        uint NextUInt();
        int NextInt(int exclusiveMaximum);
        float NextFloat();
    }

    public interface ISimClock
    {
        float Time { get; }
        float Step { get; }
        void Advance();
        void Reset();
    }

    public interface IMovementModel
    {
        float Advance(float currentProgress, float speed, float deltaTime, float pathLength);
    }

    public interface ICommandInput
    {
        bool IssueCommand(RobotState robot, RobotCommand command, RouteId route);
    }

    public interface IPlayerInput
    {
        PlayerInputFrame ReadFrame();
    }

    public struct PlayerInputFrame
    {
        public Float2 Move;
        public Float2 Look;
        public bool FirePressed;
        public bool FireHeld;
        public bool ReloadPressed;
        public bool GrenadePressed;
        public bool InteractPressed;
        public bool JumpPressed;
        public bool SprintHeld;
        public bool TogglePerspectivePressed;
        public bool PausePressed;
    }

    public interface IDomainEventSink
    {
        void Publish(DomainEvent gameEvent);
    }

    public interface ITelemetrySink
    {
        void Write(TelemetryRecord record);
        void Flush();
    }

    [Serializable]
    public sealed class DomainEvent
    {
        public string Name;
        public float SimTime;
        public int Phase;
        public Dictionary<string, string> Payload = new Dictionary<string, string>();

        public DomainEvent(string name, float simTime, int phase)
        {
            Name = name;
            SimTime = simTime;
            Phase = phase;
        }

        public DomainEvent With(string key, object value)
        {
            Payload[key] = value == null ? string.Empty : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            return this;
        }
    }

    [Serializable]
    public sealed class TelemetryRecord
    {
        public string BuildVersion;
        public string DataVersion;
        public string SessionId;
        public int Seed;
        public string SimProfileId;
        public int Phase;
        public float SimTime;
        public string EventName;
        public Dictionary<string, string> Payload = new Dictionary<string, string>();
    }
}
