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
            PixelColor32 farCol = GetBiomeFarColor(biome);

            if (biome == BiomeType.CoralCoast)
            {
                var oceanDeep = PixelColor32.FromHex("#006B73");
                var oceanLight = PixelColor32.FromHex("#1A8A8A");
                var islandDark = PixelColor32.FromHex("#07545A");
                var islandLight = PixelColor32.FromHex("#2A7470");

                // Distant ocean shelf.
                buffer.FillRect(0, 118, NativeWidth, 58, oceanDeep);
                for (int x = 0; x < NativeWidth; x += 24)
                {
                    buffer.DrawLine(x, 126 + (x % 7), x + 10, 126 + (x % 7), oceanLight);
                    buffer.DrawLine(x + 8, 141, x + 18, 141, oceanLight.Darken(0.2f));
                }

                // Low island silhouettes create depth behind the playable route.
                for (int x = 0; x < NativeWidth; x++)
                {
                    int islandY = 126
                        - (int)(Math.Sin(x * 0.028) * 10)
                        - (int)(Math.Cos(x * 0.061) * 5);

                    if (x < 55) islandY -= 13;
                    if (x > 235 && x < 330) islandY -= 20;

                    for (int y = islandY; y < 150; y++)
                        buffer.SetPixel(x, y, islandDark);
                }

                // Chunky foliage/highlights break up the silhouette.
                for (int x = 18; x < NativeWidth; x += 31)
                {
                    int y = 112 + (x % 19);
                    buffer.FillRect(x, y, 7, 13, islandLight);
                    buffer.FillRect(x - 3, y + 4, 13, 5, islandLight);
                }

                return;
            }

            // Generic distant ridgeline for the remaining biomes.
            int baseY = 130;
            for (int x = 0; x < NativeWidth; x++)
            {
                int wave1 = (int)(Math.Sin(x * 0.02) * 35);
                int wave2 = (int)(Math.Cos(x * 0.05) * 15);
                int peakY = baseY - wave1 - wave2;

                for (int y = peakY; y < NativeHeight; y++)
                    buffer.SetPixel(x, y, farCol);
            }
        }

        private static void DrawMidTerrain(PixelBitmapBuffer buffer, BiomeType biome)
        {
            PixelColor32 midCol = GetBiomeMidColor(biome);

            if (biome == BiomeType.CoralCoast)
            {
                var cliffDark = PixelColor32.FromHex("#07545A");
                var cliffMid = PixelColor32.FromHex("#0B6E68");
                var cliffLight = PixelColor32.FromHex("#24937D");
                var foliage = PixelColor32.FromHex("#3E8F63");
                var coral = PixelColor32.FromHex("#D37B61");

                // Layered coastal cliffs with stepped pixel contours.
                int baseY = 178;
                for (int x = 0; x < NativeWidth; x++)
                {
                    int hillY = baseY
                        - (int)(Math.Sin(x * 0.033) * 18)
                        - (int)(Math.Cos(x * 0.071) * 8);

                    if (x > 75 && x < 155) hillY -= 13;
                    if (x > 255 && x < 330) hillY -= 20;

                    for (int y = hillY; y < NativeHeight; y++)
                        buffer.SetPixel(x, y, cliffDark);
                }

                // Broad material bands make the cliffs read as pixel-painted terrain.
                for (int x = 0; x < NativeWidth; x += 4)
                {
                    int y = 154 + (int)(Math.Sin(x * 0.045) * 12);
                    buffer.FillRect(x, y, 4, 18, cliffMid);
                    if (x % 13 < 7)
                        buffer.FillRect(x, y + 2, 3, 7, cliffLight);
                }

                // Vegetation clusters and small coral outcrops.
                for (int x = 22; x < NativeWidth - 12; x += 37)
                {
                    int y = 142 + (x % 24);
                    buffer.FillRect(x, y, 5, 16, foliage);
                    buffer.FillRect(x - 4, y + 3, 13, 4, foliage);
                    buffer.FillRect(x - 2, y - 1, 9, 3, foliage);
                }

                for (int x = 42; x < NativeWidth; x += 67)
                {
                    int y = 168 + (x % 8);
                    buffer.FillRect(x, y, 9, 7, coral);
                    buffer.FillRect(x + 3, y - 4, 3, 5, coral);
                }

                return;
            }

            // Midground rolling hills and structural silhouettes for other biomes.
            int genericBaseY = 160;
            for (int x = 0; x < NativeWidth; x++)
            {
                int wave = (int)(Math.Sin(x * 0.035) * 20);
                int hillY = genericBaseY - wave;
                for (int y = hillY; y < NativeHeight; y++)
                    buffer.SetPixel(x, y, midCol);
            }

            // Distinct Biome Landmarks
            if (biome == BiomeType.IronBastion)
            {
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
            PixelColor32 underCol = PixelColor32.FromHex("#3B3029");

            if (biome == BiomeType.CoralCoast)
            {
                var sandShadow = PixelColor32.FromHex("#B78355");
                var sandMid = PixelColor32.FromHex("#D6A66A");
                var sandLight = PixelColor32.FromHex("#F2CF88");
                var wetSand = PixelColor32.FromHex("#7B7660");
                var shell = PixelColor32.FromHex("#E5D2B8");

                int groundTopY = 170;

                // Deep foreground gives the characters a substantial surface to stand on.
                buffer.FillRect(0, groundTopY, NativeWidth, NativeHeight - groundTopY, sandShadow);
                buffer.FillRect(0, groundTopY + 7, NativeWidth, NativeHeight - groundTopY - 7, groundCol);

                // Uneven shoreline edge.
                for (int x = 0; x < NativeWidth; x += 3)
                {
                    int y = groundTopY + (int)(Math.Sin(x * 0.11) * 2);
                    buffer.FillRect(x, y, 3, 3, sandLight);
                }

                // Painterly sand clusters.
                for (int x = 7; x < NativeWidth; x += 19)
                {
                    int y = 184 + (x % 17);
                    buffer.FillRect(x, y, 6, 2, sandShadow);
                    buffer.FillRect(x + 2, y + 2, 3, 2, sandLight);
                    if (x % 38 == 0)
                        buffer.FillRect(x + 9, y - 3, 4, 3, wetSand);
                }

                // Small shells/pebbles provide scale.
                for (int x = 13; x < NativeWidth; x += 47)
                {
                    int y = 198 + (x % 9);
                    buffer.FillRect(x, y, 3, 2, shell);
                    buffer.SetPixel(x + 1, y - 1, shell);
                }

                // Tufts and coastal grass along the upper edge.
                for (int x = 5; x < NativeWidth; x += 16)
                {
                    buffer.DrawLine(x, groundTopY, x + 2, groundTopY - 5, sandLight);
                    buffer.DrawLine(x + 3, groundTopY + 1, x + 5, groundTopY - 4, sandLight);
                }

                return;
            }

            int genericGroundTopY = 170;
            buffer.FillRect(0, genericGroundTopY, NativeWidth, NativeHeight - genericGroundTopY, groundCol);
            buffer.FillRect(0, genericGroundTopY + 10, NativeWidth, NativeHeight - genericGroundTopY - 10, underCol);

            for (int x = 4; x < NativeWidth; x += 16)
            {
                buffer.DrawLine(x, genericGroundTopY - 2, x + 2, genericGroundTopY, groundCol);
                buffer.DrawLine(x + 3, genericGroundTopY - 3, x + 5, genericGroundTopY, groundCol);
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
