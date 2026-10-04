using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    [Serializable]
    public struct ColorRgb
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ColorRgb(float r, float g, float b, float a = 1.0f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static ColorRgb FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                float r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
                float g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
                float b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
                return new ColorRgb(r, g, b, 1.0f);
            }
            return new ColorRgb(1f, 1f, 1f, 1f);
        }
    }

    [Serializable]
    public class BiomeLightingProfile
    {
        public string BiomeName { get; set; }
        public ColorRgb AmbientGlobalLight { get; set; }
        public ColorRgb KeySunLight { get; set; }
        public ColorRgb RimLight { get; set; }
        public float LightIntensity { get; set; }
        public float NormalMapStrength { get; set; }

        public static readonly Dictionary<string, BiomeLightingProfile> Profiles = new Dictionary<string, BiomeLightingProfile>
        {
            {
                "coral-coast", new BiomeLightingProfile
                {
                    BiomeName = "Coral Coast",
                    AmbientGlobalLight = ColorRgb.FromHex("87CEEB"),
                    KeySunLight = ColorRgb.FromHex("FFF8DC"),
                    RimLight = ColorRgb.FromHex("00BFFF"),
                    LightIntensity = 1.1f,
                    NormalMapStrength = 0.8f
                }
            },
            {
                "jungle", new BiomeLightingProfile
                {
                    BiomeName = "Lush Jungle",
                    AmbientGlobalLight = ColorRgb.FromHex("2E8B57"),
                    KeySunLight = ColorRgb.FromHex("98FB98"),
                    RimLight = ColorRgb.FromHex("3CB371"),
                    LightIntensity = 0.95f,
                    NormalMapStrength = 1.0f
                }
            },
            {
                "caldera", new BiomeLightingProfile
                {
                    BiomeName = "Volcanic Caldera",
                    AmbientGlobalLight = ColorRgb.FromHex("8B0000"),
                    KeySunLight = ColorRgb.FromHex("FF4500"),
                    RimLight = ColorRgb.FromHex("FFA500"),
                    LightIntensity = 1.2f,
                    NormalMapStrength = 1.2f
                }
            },
            {
                "swamp", new BiomeLightingProfile
                {
                    BiomeName = "Misty Swamps",
                    AmbientGlobalLight = ColorRgb.FromHex("4F5D54"),
                    KeySunLight = ColorRgb.FromHex("708090"),
                    RimLight = ColorRgb.FromHex("556B2F"),
                    LightIntensity = 0.85f,
                    NormalMapStrength = 0.9f
                }
            },
            {
                "ruins", new BiomeLightingProfile
                {
                    BiomeName = "Ancient Ruin Altar",
                    AmbientGlobalLight = ColorRgb.FromHex("483D8B"),
                    KeySunLight = ColorRgb.FromHex("9370DB"),
                    RimLight = ColorRgb.FromHex("BA55D3"),
                    LightIntensity = 1.0f,
                    NormalMapStrength = 1.1f
                }
            }
        };

        public static BiomeLightingProfile GetProfile(string biomeKey)
        {
            BiomeLightingProfile profile;
            if (Profiles.TryGetValue(biomeKey, out profile))
            {
                return profile;
            }
            return Profiles["coral-coast"];
        }
    }
}
