using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public sealed class DomainEventBus : IDomainEventSink
    {
        private readonly List<DomainEvent> history = new List<DomainEvent>();
        public event Action<DomainEvent> EventPublished;
        public IReadOnlyList<DomainEvent> History { get { return history; } }

        public void Publish(DomainEvent gameEvent)
        {
            if (gameEvent == null) throw new ArgumentNullException("gameEvent");
            history.Add(gameEvent);
            var handler = EventPublished;
            if (handler != null) handler(gameEvent);
        }

        public void Clear()
        {
            history.Clear();
        }
    }

    public sealed class TelemetryBridge
    {
        private readonly ITelemetrySink sink;
        private readonly string buildVersion;
        private readonly string dataVersion;
        private readonly string sessionId;
        private readonly int seed;
        private readonly string simProfileId;

        public TelemetryBridge(DomainEventBus bus, ITelemetrySink sink, string buildVersion, string dataVersion, string sessionId, int seed,
            string simProfileId = null)
        {
            this.sink = sink;
            this.buildVersion = buildVersion;
            this.dataVersion = dataVersion;
            this.sessionId = sessionId;
            this.seed = seed;
            this.simProfileId = simProfileId;
            bus.EventPublished += OnEvent;
        }

        private void OnEvent(DomainEvent gameEvent)
        {
            sink.Write(new TelemetryRecord
            {
                BuildVersion = buildVersion,
                DataVersion = dataVersion,
                SessionId = sessionId,
                Seed = seed,
                SimProfileId = simProfileId,
                Phase = gameEvent.Phase,
                SimTime = gameEvent.SimTime,
                EventName = gameEvent.Name,
                Payload = new Dictionary<string, string>(gameEvent.Payload)
            });
        }
    }
}
