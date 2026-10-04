using System;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay.Battle
{
    public enum WindDirection
    {
        Calm,       // Neutral / near-zero wind
        Tailwind,   // Blowing West to East (assisting player army arrows/spears)
        Headwind    // Blowing East to West (resisting player army, assisting enemies)
    }

    public class WeatherState
    {
        public WeatherType CurrentWeather { get; set; }
        public float WeatherIntensity { get; set; } // 0.0 to 1.0
        public float WindSpeed { get; set; } // -100f (strong headwind) to +100f (strong tailwind)
        public WindDirection Direction { get; set; }
        public float GustTimer { get; set; }
        public float BaseWindSpeed { get; set; }
        public bool IsRainDousingFire { get; set; }
        public float MovementModifier { get; set; } // Speed multiplier (e.g. 0.85 in blizzard)

        public WeatherState()
        {
            CurrentWeather = WeatherType.Clear;
            WeatherIntensity = 0.5f;
            WindSpeed = 0f;
            Direction = WindDirection.Calm;
            GustTimer = 0f;
            BaseWindSpeed = 0f;
            IsRainDousingFire = false;
            MovementModifier = 1.0f;
        }
    }

    /// <summary>
    /// Simulates dynamic stage weather and wind physics during battle.
    /// Wind dynamically drifts ballistic projectiles (arrows, spears) and modifies projectile range.
    /// Inclement weather (Rain, Blizzard, Sandstorm) influences unit movement and status effects (dousing burns, fog evasion).
    /// </summary>
    public class WeatherSimulationSystem
    {
        public WeatherState State { get; private set; }

        public event Action<WeatherType, float> OnWeatherChanged;
        public event Action<WindDirection, float> OnWindShifted;

        public WeatherSimulationSystem()
        {
            State = new WeatherState();
        }

        public void InitializeForBiome(BiomeType biome, WeatherType defaultWeather = WeatherType.Clear, float initialWind = 0f)
        {
            State.CurrentWeather = defaultWeather;
            State.BaseWindSpeed = initialWind;
            State.WindSpeed = initialWind;
            State.WeatherIntensity = defaultWeather == WeatherType.Clear ? 0.2f : 0.7f;
            UpdateWindDirection();
            UpdateWeatherProperties();
        }

        public void SetWeather(WeatherType weather, float intensity = 0.8f, float windSpeed = 0f)
        {
            State.CurrentWeather = weather;
            State.WeatherIntensity = Math.Max(0f, Math.Min(1f, intensity));
            State.BaseWindSpeed = windSpeed;
            State.WindSpeed = windSpeed;
            UpdateWindDirection();
            UpdateWeatherProperties();

            if (OnWeatherChanged != null)
            {
                OnWeatherChanged.Invoke(State.CurrentWeather, State.WeatherIntensity);
            }
        }

        public void ApplyWindGust(float gustSpeed, float duration = 4.0f)
        {
            State.WindSpeed = State.BaseWindSpeed + gustSpeed;
            State.GustTimer = duration;
            UpdateWindDirection();
        }

        public void Update(float deltaTime)
        {
            if (State.GustTimer > 0f)
            {
                State.GustTimer -= deltaTime;
                if (State.GustTimer <= 0f)
                {
                    State.GustTimer = 0f;
                    State.WindSpeed = State.BaseWindSpeed;
                    UpdateWindDirection();
                }
            }
        }

        private void UpdateWindDirection()
        {
            var oldDirection = State.Direction;
            if (State.WindSpeed > 10f)
            {
                State.Direction = WindDirection.Tailwind;
            }
            else if (State.WindSpeed < -10f)
            {
                State.Direction = WindDirection.Headwind;
            }
            else
            {
                State.Direction = WindDirection.Calm;
            }

            if (oldDirection != State.Direction && OnWindShifted != null)
            {
                OnWindShifted.Invoke(State.Direction, State.WindSpeed);
            }
        }

        private void UpdateWeatherProperties()
        {
            switch (State.CurrentWeather)
            {
                case WeatherType.Rain:
                    State.IsRainDousingFire = true;
                    State.MovementModifier = 0.95f;
                    break;

                case WeatherType.Blizzard:
                    State.IsRainDousingFire = true;
                    State.MovementModifier = 0.80f;
                    break;

                case WeatherType.Sandstorm:
                    State.IsRainDousingFire = false;
                    State.MovementModifier = 0.85f;
                    break;

                case WeatherType.Embers:
                case WeatherType.Spores:
                case WeatherType.Sunbeams:
                case WeatherType.Clear:
                default:
                    State.IsRainDousingFire = false;
                    State.MovementModifier = 1.0f;
                    break;
            }
        }

        public float CalculateWindDriftOffset(float flightDuration, bool isPlayerProjectile)
        {
            // Tailwind (+WindSpeed) pushes player projectile further forward (+X)
            // Headwind (-WindSpeed) pushes player projectile backward (-X)
            // If enemy projectile (flying East to West), opposite direction applies
            float projectileMultiplier = isPlayerProjectile ? 1.0f : -1.0f;
            return State.WindSpeed * flightDuration * 1.5f * projectileMultiplier;
        }
    }
}
