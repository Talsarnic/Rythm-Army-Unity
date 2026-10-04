using System;

namespace RhythmArmy.Core.Combat
{
    public struct TrajectoryPoint
    {
        public float X;
        public float Y;
        public float AngleDegrees;

        public TrajectoryPoint(float x, float y, float angleDegrees)
        {
            X = x;
            Y = y;
            AngleDegrees = angleDegrees;
        }
    }

    public static class BallisticsCalculator
    {
        public const float Gravity = 980f; // Pixels / second^2 in standard 2D space

        public static TrajectoryPoint SampleParabolicArc(
            float startX,
            float startY,
            float targetX,
            float targetY,
            float progress, // 0.0 to 1.0
            float peakHeight = 120f,
            float windDriftX = 0f)
        {
            float t = Math.Max(0f, Math.Min(1f, progress));

            // Horizontal linear interpolation + progressive wind drift
            float currentX = startX + (targetX - startX) * t + windDriftX * t;

            // Parabolic vertical arc: 4 * peakHeight * t * (1 - t)
            float baseLinearY = startY + (targetY - startY) * t;
            float arcY = 4f * peakHeight * t * (1f - t);
            float currentY = baseLinearY + arcY;

            // Calculate instantaneous angle
            float dx = (targetX - startX) + windDriftX;
            float dyLinear = targetY - startY;
            float dyArcDerivative = 4f * peakHeight * (1f - 2f * t);
            float dy = dyLinear + dyArcDerivative;

            float angleRad = (float)Math.Atan2(dy, dx);
            float angleDeg = angleRad * (180f / (float)Math.PI);

            return new TrajectoryPoint(currentX, currentY, angleDeg);
        }

        public static float CalculateFlightDuration(float distance, float projectileSpeed = 650f)
        {
            if (projectileSpeed <= 0f) return 0.5f;
            return Math.Max(0.25f, distance / projectileSpeed);
        }
    }
}
