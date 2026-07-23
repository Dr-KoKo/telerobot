using System;

namespace Telerobot.Game.Core
{
    public sealed class XorShiftRng : IDeterministicRng
    {
        private uint state;

        public XorShiftRng(int seed)
        {
            state = seed == 0 ? 0x9E3779B9u : unchecked((uint)seed);
        }

        public uint NextUInt()
        {
            var value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;
            return value;
        }

        public int NextInt(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0) throw new ArgumentOutOfRangeException("exclusiveMaximum");
            return (int)(NextUInt() % (uint)exclusiveMaximum);
        }

        public float NextFloat()
        {
            return (NextUInt() & 0x00FFFFFFu) / 16777216f;
        }
    }

    public sealed class FixedSimClock : ISimClock
    {
        public float Time { get; private set; }
        public float Step { get; private set; }

        public FixedSimClock(float step)
        {
            if (step <= 0f) throw new ArgumentOutOfRangeException("step");
            Step = step;
        }

        public void Advance()
        {
            Time += Step;
        }

        public void Reset()
        {
            Time = 0f;
        }
    }

    public sealed class WaypointMovement : IMovementModel
    {
        public float Advance(float currentProgress, float speed, float deltaTime, float pathLength)
        {
            if (pathLength <= 0f) return 1f;
            return Math.Min(1f, currentProgress + Math.Max(0f, speed) * Math.Max(0f, deltaTime) / pathLength);
        }
    }
}
