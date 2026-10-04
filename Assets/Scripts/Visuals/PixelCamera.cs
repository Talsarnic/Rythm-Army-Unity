using System;

namespace RhythmArmy.Visuals
{
    public class PixelCamera
    {
        public float TargetX = 0f;
        public float TargetY = 0f;
        public float CurrentX = 0f;
        public float CurrentY = 0f;
        public float FollowSpeed = 5.0f;
        public float LookAhead = 120f;
        public float PixelsPerUnit = 16f;

        // Trauma-based screen shake
        public float Trauma = 0f;
        public float TraumaDecayRate = 1.8f;
        public float MaxShakeTranslation = 12f;
        public float MaxShakeAngleDeg = 4f;

        private readonly Random _rng = new Random();

        public float ShakeOffsetX { get; private set; }
        public float ShakeOffsetY { get; private set; }
        public float ShakeRotation { get; private set; }

        public float SnappedX
        {
            get { return (float)Math.Floor((CurrentX + ShakeOffsetX) * PixelsPerUnit) / PixelsPerUnit; }
        }

        public float SnappedY
        {
            get { return (float)Math.Floor((CurrentY + ShakeOffsetY) * PixelsPerUnit) / PixelsPerUnit; }
        }

        public void AddTrauma(float amount)
        {
            Trauma = Math.Min(1.0f, Trauma + Math.Max(0f, amount));
        }

        public void SnapTo(float x, float y)
        {
            CurrentX = x;
            CurrentY = y;
            TargetX = x;
            TargetY = y;
        }

        public void FollowTarget(float targetX, float targetY, float deltaTime)
        {
            TargetX = targetX;
            TargetY = targetY;
            CurrentX += (TargetX - CurrentX) * Math.Min(1f, FollowSpeed * deltaTime);
            CurrentY += (TargetY - CurrentY) * Math.Min(1f, FollowSpeed * deltaTime);
        }

        public void UpdateShake(float deltaTime)
        {
            if (Trauma > 0f)
            {
                float shakeMagnitude = Trauma * Trauma;
                ShakeOffsetX = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeTranslation * shakeMagnitude);
                ShakeOffsetY = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeTranslation * shakeMagnitude);
                ShakeRotation = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeAngleDeg * shakeMagnitude);

                Trauma = Math.Max(0f, Trauma - (TraumaDecayRate * deltaTime));
            }
            else
            {
                ShakeOffsetX = 0f;
                ShakeOffsetY = 0f;
                ShakeRotation = 0f;
            }
        }

        public void Update(float bannerX, float deltaTime)
        {
            TargetX = bannerX + LookAhead;

            // Smooth damping
            CurrentX += (TargetX - CurrentX) * Math.Min(1f, FollowSpeed * deltaTime);
            CurrentY += (TargetY - CurrentY) * Math.Min(1f, FollowSpeed * deltaTime);

            // Calculate Shake
            if (Trauma > 0f)
            {
                float shakeMagnitude = Trauma * Trauma; // Non-linear response (quadratic)
                ShakeOffsetX = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeTranslation * shakeMagnitude);
                ShakeOffsetY = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeTranslation * shakeMagnitude);
                ShakeRotation = (float)((_rng.NextDouble() * 2.0 - 1.0) * MaxShakeAngleDeg * shakeMagnitude);

                Trauma = Math.Max(0f, Trauma - (TraumaDecayRate * deltaTime));
            }
            else
            {
                ShakeOffsetX = 0f;
                ShakeOffsetY = 0f;
                ShakeRotation = 0f;
            }
        }
    }
}
