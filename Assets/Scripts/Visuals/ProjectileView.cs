using System;
using RhythmArmy.Core.Combat;

namespace RhythmArmy.Visuals
{
    public enum ProjectileVisualType
    {
        Spear,
        Arrow,
        Rock,
        SonicWave,
        ArcaneOrb
    }

    /// <summary>
    /// Presentation model for flying ranged projectiles in the Moonlighter 2D pixel art style.
    /// Handles parabolic rotation alignment, pixel-grid snapping, trail particles, and impact sparks.
    /// </summary>
    public class ProjectileView
    {
        public string Id { get; private set; }
        public ProjectileVisualType VisualType { get; private set; }

        public float StartX { get; private set; }
        public float StartY { get; private set; }
        public float TargetX { get; private set; }
        public float TargetY { get; private set; }
        public float ArcHeight { get; private set; }
        public float WindDriftX { get; private set; }
        public float TotalDuration { get; private set; }

        public float ElapsedTime { get; private set; }
        public float Progress
        {
            get { return TotalDuration > 0f ? Math.Min(1.0f, ElapsedTime / TotalDuration) : 1.0f; }
        }

        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }
        public float RotationDegrees { get; private set; }
        public bool IsFinished
        {
            get { return Progress >= 1.0f; }
        }

        public string TargetId { get; private set; }
        public float TrailTimer { get; private set; }
        public string SpriteId { get; private set; }

        public ProjectileView(string id, ProjectileVisualType type, float startX, float startY, float targetX, float targetY, float speed = 450f, float arcHeight = 60f, string targetId = null, float windDriftX = 0f)
        {
            Id = id != null ? id : Guid.NewGuid().ToString();
            VisualType = type;
            StartX = startX;
            StartY = startY;
            TargetX = targetX;
            TargetY = targetY;
            ArcHeight = arcHeight;
            WindDriftX = windDriftX;
            TargetId = targetId;

            float distance = (float)Math.Sqrt(Math.Pow(targetX - startX, 2) + Math.Pow(targetY - startY, 2));
            TotalDuration = BallisticsCalculator.CalculateFlightDuration(distance, speed);
            ElapsedTime = 0f;

            DetermineSpriteId();
            UpdatePosition();
        }

        private void DetermineSpriteId()
        {
            switch (VisualType)
            {
                case ProjectileVisualType.Spear:
                    SpriteId = "projectile_spear_iron";
                    break;
                case ProjectileVisualType.Arrow:
                    SpriteId = "projectile_arrow_flint";
                    break;
                case ProjectileVisualType.Rock:
                    SpriteId = "projectile_boulder";
                    break;
                case ProjectileVisualType.SonicWave:
                    SpriteId = "projectile_sonic_ring";
                    break;
                case ProjectileVisualType.ArcaneOrb:
                    SpriteId = "projectile_arcane_orb";
                    break;
                default:
                    SpriteId = "projectile_arrow_flint";
                    break;
            }
        }

        public void Update(float deltaTime)
        {
            if (IsFinished) return;

            ElapsedTime += deltaTime;
            UpdatePosition();
            TrailTimer += deltaTime;
        }

        private void UpdatePosition()
        {
            var point = BallisticsCalculator.SampleParabolicArc(StartX, StartY, TargetX, TargetY, Progress, ArcHeight, WindDriftX);
            CurrentX = point.X;
            CurrentY = point.Y;
            RotationDegrees = point.AngleDegrees;
        }
    }
}
