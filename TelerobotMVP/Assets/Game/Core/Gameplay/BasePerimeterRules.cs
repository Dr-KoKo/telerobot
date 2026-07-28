using System;

namespace Telerobot.Game.Core
{
    public static class BasePerimeterRules
    {
        private const int SlotsPerRow = 7;
        private const int RowCount = 3;

        public static Float3 AttackSlot(Float3 center, Float3 approach, float outerRadius,
            int ordinal, float edgePadding, float rowSpacing, float lateralSpacing)
        {
            Validate(outerRadius, edgePadding, rowSpacing, lateralSpacing);

            var approachX = approach.X;
            var approachZ = approach.Z;
            var magnitude = (float)Math.Sqrt(approachX * approachX + approachZ * approachZ);
            if (magnitude < 0.001f)
            {
                approachX = 0f;
                approachZ = 1f;
            }
            else
            {
                approachX /= magnitude;
                approachZ /= magnitude;
            }

            ordinal = Math.Max(0, ordinal);
            var column = ordinal % SlotsPerRow - SlotsPerRow / 2;
            var row = ordinal / SlotsPerRow % RowCount;
            var radialDistance = outerRadius + edgePadding + row * rowSpacing;
            var tangentX = -approachZ;
            var tangentZ = approachX;

            return new Float3(
                center.X + approachX * radialDistance + tangentX * column * lateralSpacing,
                center.Y,
                center.Z + approachZ * radialDistance + tangentZ * column * lateralSpacing);
        }

        private static void Validate(float outerRadius, float edgePadding,
            float rowSpacing, float lateralSpacing)
        {
            if (!IsFinite(outerRadius) || outerRadius <= 0f ||
                !IsFinite(edgePadding) || edgePadding < 0f ||
                !IsFinite(rowSpacing) || rowSpacing <= 0f ||
                !IsFinite(lateralSpacing) || lateralSpacing <= 0f)
                throw new ArgumentOutOfRangeException("Base perimeter dimensions must be finite and valid.");
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
