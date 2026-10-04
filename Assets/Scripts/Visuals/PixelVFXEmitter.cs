using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmArmy.Visuals
{
    public enum VFXType
    {
        DustPuff,
        HitSparks,
        BloodSplatter,
        FeverSparkle,
        TelegraphDecal
    }

    public class PixelParticle
    {
        public VFXType Type;
        public float X;
        public float Y;
        public float Vx;
        public float Vy;
        public float Gravity;
        public float Age;
        public float MaxLife;
        public float Size;
        public string ColorHex; // Moonlighter palette color

        public bool IsAlive
        {
            get { return Age < MaxLife; }
        }

        public float NormalizedLife
        {
            get { return MaxLife > 0 ? Math.Min(1f, Age / MaxLife) : 1f; }
        }
    }

    public class PixelVFXEmitter
    {
        public List<PixelParticle> ActiveParticles = new List<PixelParticle>();
        private readonly Random _rng = new Random();

        public void SpawnDustPuff(float x, float y, int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (float)(_rng.NextDouble() * Math.PI);
                float speed = (float)(20.0 + (_rng.NextDouble() * 40.0));
                ActiveParticles.Add(new PixelParticle
                {
                    Type = VFXType.DustPuff,
                    X = x + (float)((_rng.NextDouble() - 0.5) * 8.0),
                    Y = y,
                    Vx = (float)Math.Cos(angle) * speed * 0.5f,
                    Vy = (float)Math.Sin(angle) * speed,
                    Gravity = -40f,
                    Age = 0f,
                    MaxLife = (float)(0.25 + (_rng.NextDouble() * 0.25)),
                    Size = 2f,
                    ColorHex = "#D7C4A5" // Warm sand/dust
                });
            }
        }

        public void SpawnHitSparks(float x, float y, int count = 8)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = _rng.NextDouble() * Math.PI * 2.0;
                double speed = 60.0 + (_rng.NextDouble() * 100.0);
                ActiveParticles.Add(new PixelParticle
                {
                    Type = VFXType.HitSparks,
                    X = x,
                    Y = y,
                    Vx = (float)(Math.Cos(angle) * speed),
                    Vy = (float)(Math.Sin(angle) * speed),
                    Gravity = -120f,
                    Age = 0f,
                    MaxLife = (float)(0.15 + (_rng.NextDouble() * 0.2)),
                    Size = 2f,
                    ColorHex = "#FFF176" // Bright golden spark
                });
            }
        }

        public void SpawnFeverSparkles(float x, float y, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                ActiveParticles.Add(new PixelParticle
                {
                    Type = VFXType.FeverSparkle,
                    X = x + (float)((_rng.NextDouble() - 0.5) * 24.0),
                    Y = y + (float)(_rng.NextDouble() * 30.0),
                    Vx = (float)((_rng.NextDouble() - 0.5) * 15.0),
                    Vy = (float)(30.0 + (_rng.NextDouble() * 40.0)),
                    Gravity = 0f,
                    Age = 0f,
                    MaxLife = (float)(0.4 + (_rng.NextDouble() * 0.3)),
                    Size = 3f,
                    ColorHex = "#FFD700" // Vibrant Fever gold
                });
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = ActiveParticles.Count - 1; i >= 0; i--)
            {
                var p = ActiveParticles[i];
                p.Age += deltaTime;
                p.X += p.Vx * deltaTime;
                p.Y += p.Vy * deltaTime;
                p.Vy += p.Gravity * deltaTime;

                if (!p.IsAlive)
                {
                    ActiveParticles.RemoveAt(i);
                }
            }
        }
    }
}
