using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public class WeatherParticle
    {
        public float X;
        public float Y;
        public float VelocityX;
        public float VelocityY;
        public float Lifetime;
        public float MaxLifetime;
        public float Size;
        public float Alpha;
        public bool IsAlive
        {
            get { return Lifetime < MaxLifetime; }
        }
    }

    /// <summary>
    /// Presentation model for the 8-biome parallax environment in Moonlighter pixel style.
    /// Manages multi-layered parallax scrolling offsets, ambient 2D lighting tint, and weather particle simulation.
    /// </summary>
    public class EnvironmentView
    {
        public BiomeTheme CurrentTheme { get; private set; }
        public BiomeType Biome
        {
            get { return CurrentTheme != null ? CurrentTheme.Biome : BiomeType.CoralCoast; }
        }

        public List<WeatherParticle> ActiveParticles { get; private set; }
        public float ParticleSpawnAccumulator { get; private set; }

        private readonly Random _rng = new Random();

        public EnvironmentView(BiomeType biome = BiomeType.CoralCoast)
        {
            ActiveParticles = new List<WeatherParticle>();
            SetBiome(biome);
        }

        public void SetBiome(BiomeType biome)
        {
            CurrentTheme = EnvironmentSystem.CreateBiomeTheme(biome);
            if (ActiveParticles != null)
            {
                ActiveParticles.Clear();
            }
            ParticleSpawnAccumulator = 0f;
        }

        public void Update(float cameraX, float cameraY, float deltaTime, float viewportWidth = 480f, float viewportHeight = 270f)
        {
            if (CurrentTheme == null) return;

            // Update Weather Particles
            var weather = CurrentTheme.Weather;
            if (weather != null && weather.SpawnRate > 0f)
            {
                ParticleSpawnAccumulator += weather.SpawnRate * deltaTime;
                while (ParticleSpawnAccumulator >= 1.0f)
                {
                    ParticleSpawnAccumulator -= 1.0f;
                    SpawnParticle(cameraX, cameraY, viewportWidth, viewportHeight, weather);
                }
            }

            for (int i = ActiveParticles.Count - 1; i >= 0; i--)
            {
                var p = ActiveParticles[i];
                p.X += p.VelocityX * deltaTime;
                p.Y += p.VelocityY * deltaTime;
                p.Lifetime += deltaTime;

                // Fade particle over lifetime
                float normalizedLife = p.Lifetime / p.MaxLifetime;
                p.Alpha = (1.0f - normalizedLife) * weather.Intensity;

                if (!p.IsAlive)
                {
                    ActiveParticles.RemoveAt(i);
                }
            }
        }

        private void SpawnParticle(float cameraX, float cameraY, float vpWidth, float vpHeight, WeatherEffect weather)
        {
            float spawnX = cameraX - (vpWidth * 0.5f) + (float)(_rng.NextDouble() * vpWidth * 1.5);
            float spawnY = cameraY + (vpHeight * 0.5f) + (float)(_rng.NextDouble() * 50.0);

            float jitterVx = (float)((_rng.NextDouble() * 2.0 - 1.0) * 10.0);
            float jitterVy = (float)((_rng.NextDouble() * 2.0 - 1.0) * 10.0);

            var p = new WeatherParticle
            {
                X = spawnX,
                Y = spawnY,
                VelocityX = weather.WindSpeedX + jitterVx,
                VelocityY = weather.FallSpeedY + jitterVy,
                Lifetime = 0f,
                MaxLifetime = 2.0f + (float)(_rng.NextDouble() * 2.5),
                Size = 1.0f + (float)(_rng.NextDouble() * 2.0),
                Alpha = weather.Intensity
            };

            ActiveParticles.Add(p);
        }

        public float GetLayerRenderX(ParallaxLayer layer, float cameraX)
        {
            if (layer == null) return 0f;
            return layer.ComputePositionX(cameraX);
        }
    }
}
