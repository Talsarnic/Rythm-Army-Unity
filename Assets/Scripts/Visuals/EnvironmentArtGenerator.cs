using System;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum ParallaxLayerRole
    {
        SkyAtmosphere,
        FarScenery,
        MidTerrain,
        ForegroundGround
    }

    /// <summary>
    /// Generates Moonlighter-style high-fidelity 384x216 pixel parallax background plates
    /// and scenery layers for all 8 campaign biomes.
    /// </summary>
    public static class EnvironmentArtGenerator
    {
        public const int NativeWidth = 384;
        public const int NativeHeight = 216;

        /// <summary>
        /// Generates a composite or layer-specific 384x216 pixel environment backdrop.
        /// </summary>
        public static PixelBitmapBuffer GenerateBiomeBackdrop(BiomeType biome, ParallaxLayerRole layer)
        {
            var buffer = new PixelBitmapBuffer(NativeWidth, NativeHeight);

            switch (layer)
            {
                case ParallaxLayerRole.SkyAtmosphere:
                    DrawSkyAtmosphere(buffer, biome);
                    break;

                case ParallaxLayerRole.FarScenery:
                    DrawFarScenery(buffer, biome);
                    break;

                case ParallaxLayerRole.MidTerrain:
                    DrawMidTerrain(buffer, biome);
                    break;

                case ParallaxLayerRole.ForegroundGround:
                    DrawForegroundGround(buffer, biome);
                    break;
            }

            return buffer;
        }

        private static void DrawSkyAtmosphere(PixelBitmapBuffer buffer, BiomeType biome)
        {
            PixelColor32 skyTop;
            PixelColor32 skyBottom;

            switch (biome)
            {
                case BiomeType.CoralCoast:
                    skyTop = PixelColor32.FromHex("#0288D1"); // Ocean azure
                    skyBottom = PixelColor32.FromHex("#80DEEA"); // Warm sea mist
                    break;

                case BiomeType.JungleFort:
                    skyTop = PixelColor32.FromHex("#2E7D32"); // Canopy emerald
                    skyBottom = PixelColor32.FromHex("#C8E6C9"); // Humid mist
                    break;

                case BiomeType.MistySwamp:
                    skyTop = PixelColor32.FromHex("#311B92"); // Deep indigo
                    skyBottom = PixelColor32.FromHex("#9575CD"); // Purple marsh fog
                    break;

                case BiomeType.VolcanicCaldera:
                    skyTop = PixelColor32.FromHex("#212121"); // Ash black
                    skyBottom = PixelColor32.FromHex("#BF360C"); // Deep magma orange
                    break;

                case BiomeType.IronBastion:
                    skyTop = PixelColor32.FromHex("#263238"); // Slate storm cloud
                    skyBottom = PixelColor32.FromHex("#78909C"); // Cold silver fog
                    break;

                case BiomeType.RuinAltar:
                    skyTop = PixelColor32.FromHex("#004D40"); // Deep spiritual teal
                    skyBottom = PixelColor32.FromHex("#80CBC4"); // Ethereal aurora
                    break;

                case BiomeType.DesertDunes:
                    skyTop = PixelColor32.FromHex("#E65100"); // Fiery desert dusk
                    skyBottom = PixelColor32.FromHex("#FFE082"); // Golden sand glow
                    break;

                case BiomeType.FrozenPeaks:
                default:
                    skyTop = PixelColor32.FromHex("#0D1B2A"); // Frost starfield
                    skyBottom = PixelColor32.FromHex("#90CAF9"); // Icy mountain dawn
                    break;
            }

            buffer.DrawGradientV(0, 0, NativeWidth, NativeHeight, skyTop, skyBottom);

            // Celestial Stars / Sun / Moon
            if (biome == BiomeType.FrozenPeaks)
            {
                var rng = new Random(1337);
                for (int i = 0; i < 60; i++)
                {
                    int sx = rng.Next(0, NativeWidth);
                    int sy = rng.Next(0, NativeHeight / 2);
                    buffer.SetPixel(sx, sy, (i % 3 == 0) ? PixelColor32.FeverYellow : PixelColor32.White);
                }
                // Big Radiant Moon
                buffer.DrawCircle(320, 50, 20, PixelColor32.White, true);
                buffer.DrawCircle(320, 50, 18, PixelColor32.FromHex("#FFF9C4"), true);
            }
            else if (biome == BiomeType.CoralCoast || biome == BiomeType.DesertDunes)
            {
                // Radiant Warm Sun
                buffer.DrawCircle(60, 60, 22, PixelColor32.StarGold, true);
                buffer.DrawCircle(60, 60, 18, PixelColor32.White, true);
            }
        }

        private static void DrawFarScenery(PixelBitmapBuffer buffer, BiomeType biome)
        {
            PixelColor32 mountainCol = GetBiomeFarColor(biome);

            // Distant mountain ridgeline
            int baseY = 130;
            for (int x = 0; x < NativeWidth; x++)
            {
                int wave1 = (int)(Math.Sin(x * 0.02) * 35);
                int wave2 = (int)(Math.Cos(x * 0.05) * 15);
                int peakY = baseY - wave1 - wave2;

                for (int y = peakY; y < NativeHeight; y++)
                {
                    buffer.SetPixel(x, y, mountainCol);
                }
            }
        }

        private static void DrawMidTerrain(PixelBitmapBuffer buffer, BiomeType biome)
        {
            PixelColor32 midCol = GetBiomeMidColor(biome);

            // Midground rolling hills and structural silhouettes
            int baseY = 160;
            for (int x = 0; x < NativeWidth; x++)
            {
                int wave = (int)(Math.Sin(x * 0.035) * 20);
                int hillY = baseY - wave;

                for (int y = hillY; y < NativeHeight; y++)
                {
                    buffer.SetPixel(x, y, midCol);
                }
            }

            // Distinct Biome Landmarks
            if (biome == BiomeType.IronBastion)
            {
                // Fortress Watchtowers
                buffer.FillRect(80, 100, 24, 80, PixelColor32.ObsidianSlate);
                buffer.FillRect(280, 110, 20, 70, PixelColor32.ObsidianSlate);
            }
            else if (biome == BiomeType.RuinAltar)
            {
                // Floating Ancient Monolith Pillars
                buffer.FillRect(120, 70, 12, 40, PixelColor32.ObsidianSlate);
                buffer.FillRect(220, 55, 14, 50, PixelColor32.ObsidianSlate);
                buffer.SetPixel(126, 90, PixelColor32.SpiritCyan);
                buffer.SetPixel(227, 80, PixelColor32.SpiritCyan);
            }
            else if (biome == BiomeType.VolcanicCaldera)
            {
                // Magma Vein Glows
                for (int x = 0; x < NativeWidth; x += 30)
                {
                    buffer.DrawLine(x, 160, x + 15, 180, PixelColor32.Crimson);
                    buffer.DrawLine(x + 1, 161, x + 14, 179, PixelColor32.AmberGold);
                }
            }
        }

        private static void DrawForegroundGround(PixelBitmapBuffer buffer, BiomeType biome)
        {
            PixelColor32 groundCol = GetBiomeGroundColor(biome);
            PixelColor32 underCol = PixelColor32.FromHex("#1A202C");

            int groundTopY = 175;

            // Flat battle path lane for marching squad and enemies
            buffer.FillRect(0, groundTopY, NativeWidth, 8, groundCol);
            buffer.FillRect(0, groundTopY + 8, NativeWidth, NativeHeight - (groundTopY + 8), underCol);

            // Grass / Stone / Sand Tufts
            for (int x = 4; x < NativeWidth; x += 16)
            {
                buffer.DrawLine(x, groundTopY - 2, x + 2, groundTopY, groundCol);
                buffer.DrawLine(x + 3, groundTopY - 3, x + 5, groundTopY, groundCol);
            }
        }

        private static PixelColor32 GetBiomeFarColor(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.CoralCoast: return PixelColor32.FromHex("#00695C");
                case BiomeType.JungleFort: return PixelColor32.FromHex("#1B5E20");
                case BiomeType.MistySwamp: return PixelColor32.FromHex("#4A148C");
                case BiomeType.VolcanicCaldera: return PixelColor32.FromHex("#3E2723");
                case BiomeType.IronBastion: return PixelColor32.FromHex("#37474F");
                case BiomeType.RuinAltar: return PixelColor32.FromHex("#004D40");
                case BiomeType.DesertDunes: return PixelColor32.FromHex("#BF360C");
                default: return PixelColor32.FromHex("#1B263B");
            }
        }

        private static PixelColor32 GetBiomeMidColor(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.CoralCoast: return PixelColor32.FromHex("#00897B");
                case BiomeType.JungleFort: return PixelColor32.FromHex("#2E7D32");
                case BiomeType.MistySwamp: return PixelColor32.FromHex("#6A1B9A");
                case BiomeType.VolcanicCaldera: return PixelColor32.FromHex("#4E342E");
                case BiomeType.IronBastion: return PixelColor32.FromHex("#455A64");
                case BiomeType.RuinAltar: return PixelColor32.FromHex("#00796B");
                case BiomeType.DesertDunes: return PixelColor32.FromHex("#E65100");
                default: return PixelColor32.FromHex("#415A77");
            }
        }

        private static PixelColor32 GetBiomeGroundColor(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.CoralCoast: return PixelColor32.FromHex("#FFE082"); // Sand
                case BiomeType.JungleFort: return PixelColor32.FromHex("#4CAF50"); // Lush grass
                case BiomeType.MistySwamp: return PixelColor32.FromHex("#558B2F"); // Murky moss
                case BiomeType.VolcanicCaldera: return PixelColor32.FromHex("#37474F"); // Basalt rock
                case BiomeType.IronBastion: return PixelColor32.FromHex("#90A4AE"); // Cobblestone
                case BiomeType.RuinAltar: return PixelColor32.FromHex("#80CBC4"); // Ancient stone
                case BiomeType.DesertDunes: return PixelColor32.FromHex("#FFB74D"); // Dune sand
                default: return PixelColor32.FromHex("#ECEFF1"); // Snow/Ice
            }
        }
    }
}
