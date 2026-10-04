using System;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Generates high-fidelity Moonlighter-style screen plates:
    /// Title Screen Splash Banner and Campaign World Map Navigator.
    /// </summary>
    public static class TitleAndMapArtGenerator
    {
        public const int ScreenWidth = 384;
        public const int ScreenHeight = 216;

        public static PixelBitmapBuffer GenerateTitleScreenSplash()
        {
            var buffer = new PixelBitmapBuffer(ScreenWidth, ScreenHeight);

            // 1. Sky twilight gradient (Deep Indigo to Warm Sunset Gold)
            for (int y = 0; y < ScreenHeight; y++)
            {
                float t = (float)y / ScreenHeight;
                PixelColor32 skyCol;
                if (t < 0.4f)
                {
                    skyCol = PixelColor32.Lerp(PixelColor32.FromHex("#0B132B"), PixelColor32.FromHex("#1C2541"), t / 0.4f);
                }
                else if (t < 0.75f)
                {
                    skyCol = PixelColor32.Lerp(PixelColor32.FromHex("#1C2541"), PixelColor32.FromHex("#5BC0BE"), (t - 0.4f) / 0.35f);
                }
                else
                {
                    skyCol = PixelColor32.Lerp(PixelColor32.FromHex("#5BC0BE"), PixelColor32.FromHex("#F77F00"), (t - 0.75f) / 0.25f);
                }

                for (int x = 0; x < ScreenWidth; x++)
                {
                    buffer.SetPixel(x, y, skyCol);
                }
            }

            // 2. Distant mountain silhouette
            var mountainCol = PixelColor32.FromHex("#1F2421");
            for (int x = 0; x < ScreenWidth; x++)
            {
                int peak1 = (int)(40.0 * Math.Sin(x * 0.015) + 20.0 * Math.Cos(x * 0.035) + 130.0);
                for (int y = peak1; y < ScreenHeight; y++)
                {
                    buffer.SetPixel(x, y, mountainCol);
                }
            }

            // 3. Foreground hill silhouette & Tree of Life branches
            var hillCol = PixelColor32.FromHex("#0B090A");
            for (int x = 0; x < ScreenWidth; x++)
            {
                int peak2 = (int)(25.0 * Math.Sin(x * 0.02 + 1.2) + 155.0);
                for (int y = peak2; y < ScreenHeight; y++)
                {
                    buffer.SetPixel(x, y, hillCol);
                }
            }

            // Tree of Life silhouette on the right
            buffer.FillRect(290, 80, 16, 100, hillCol);
            buffer.FillCircle(298, 75, 45, hillCol);
            buffer.FillCircle(270, 90, 30, hillCol);
            buffer.FillCircle(325, 95, 30, hillCol);

            // Glowing golden spirit wisps floating around tree
            var glowGold = PixelColor32.FromHex("#FCBF49");
            buffer.FillCircle(285, 70, 3, glowGold);
            buffer.FillCircle(315, 65, 4, glowGold);
            buffer.FillCircle(330, 85, 3, glowGold);
            buffer.FillCircle(260, 95, 3, glowGold);

            // 4. Logo Plate: "RHYTHM ARMY" Banner
            int logoCenterX = ScreenWidth / 2;
            int logoTopY = 35;
            int logoWidth = 240;
            int logoHeight = 48;

            // Ornate crest border
            var crestBg = PixelColor32.FromHex("#161A1D");
            var crestBorder = PixelColor32.FromHex("#D4AF37"); // Gold
            var crestHighlight = PixelColor32.FromHex("#FFE6A7");

            buffer.FillRect(logoCenterX - logoWidth / 2, logoTopY, logoWidth, logoHeight, crestBg);
            buffer.DrawRect(logoCenterX - logoWidth / 2, logoTopY, logoWidth, logoHeight, crestBorder);
            buffer.DrawRect(logoCenterX - logoWidth / 2 + 2, logoTopY + 2, logoWidth - 4, logoHeight - 4, crestHighlight);

            // 4 Drum Gems embedded in logo crest
            buffer.FillCircle(logoCenterX - 100, logoTopY + 24, 8, PixelColor32.FromHex("#E63946")); // Boom Red
            buffer.FillCircle(logoCenterX - 75,  logoTopY + 24, 8, PixelColor32.FromHex("#457B9D")); // Tak Blue
            buffer.FillCircle(logoCenterX + 75,  logoTopY + 24, 8, PixelColor32.FromHex("#2A9D8F")); // Rat Green
            buffer.FillCircle(logoCenterX + 100, logoTopY + 24, 8, PixelColor32.FromHex("#E76F51")); // Ting Orange

            // Title center badge
            buffer.FillRect(logoCenterX - 55, logoTopY + 12, 110, 24, PixelColor32.FromHex("#2B2D42"));
            buffer.DrawRect(logoCenterX - 55, logoTopY + 12, 110, 24, crestBorder);

            // 5. Prompt Banner: "PRESS ANY DRUM TO MARCH"
            int promptY = 185;
            buffer.FillRect(ScreenWidth / 2 - 90, promptY, 180, 18, PixelColor32.FromHex("#0F1416"));
            buffer.DrawRect(ScreenWidth / 2 - 90, promptY, 180, 18, PixelColor32.FromHex("#E0E1DD"));

            return buffer;
        }

        public static PixelBitmapBuffer GenerateCampaignWorldMap()
        {
            var buffer = new PixelBitmapBuffer(ScreenWidth, ScreenHeight);

            // 1. Ancient Parchment Map Texture Base
            var parchmentBase = PixelColor32.FromHex("#DDB892");
            var parchmentDark = PixelColor32.FromHex("#B08968");
            var parchmentEdge = PixelColor32.FromHex("#7F5539");

            buffer.FillRect(0, 0, ScreenWidth, ScreenHeight, parchmentBase);

            // Border vignette
            for (int x = 0; x < ScreenWidth; x++)
            {
                buffer.SetPixel(x, 0, parchmentEdge);
                buffer.SetPixel(x, 1, parchmentDark);
                buffer.SetPixel(x, ScreenHeight - 2, parchmentDark);
                buffer.SetPixel(x, ScreenHeight - 1, parchmentEdge);
            }
            for (int y = 0; y < ScreenHeight; y++)
            {
                buffer.SetPixel(0, y, parchmentEdge);
                buffer.SetPixel(1, y, parchmentDark);
                buffer.SetPixel(ScreenWidth - 2, y, parchmentDark);
                buffer.SetPixel(ScreenWidth - 1, y, parchmentEdge);
            }

            // Grid coordinate lines (cartography style)
            var gridLineCol = PixelColor32.FromHex("#CDB498");
            for (int x = 40; x < ScreenWidth; x += 48)
            {
                for (int y = 0; y < ScreenHeight; y += 3)
                {
                    buffer.SetPixel(x, y, gridLineCol);
                }
            }
            for (int y = 30; y < ScreenHeight; y += 40)
            {
                for (int x = 0; x < ScreenWidth; x += 3)
                {
                    buffer.SetPixel(x, y, gridLineCol);
                }
            }

            // 2. Biome Terrain Zones on Map
            // Coral Coast (Southwest Ocean)
            buffer.FillCircle(50, 170, 35, PixelColor32.FromHex("#70A288"));
            // Jungle Fort (South)
            buffer.FillCircle(120, 160, 30, PixelColor32.FromHex("#588157"));
            // Misty Swamp (West)
            buffer.FillCircle(60, 95, 28, PixelColor32.FromHex("#6B705C"));
            // Volcanic Caldera (Northwest)
            buffer.FillCircle(110, 50, 32, PixelColor32.FromHex("#C97064"));
            // Camp Hub (Central Sanctuary)
            buffer.FillCircle(192, 120, 35, PixelColor32.FromHex("#E6CCB2"));
            buffer.DrawCircle(192, 120, 36, PixelColor32.FromHex("#9C6644"));
            // Desert Dunes (Southeast)
            buffer.FillCircle(270, 160, 32, PixelColor32.FromHex("#D4A373"));
            // Iron Bastion (East)
            buffer.FillCircle(290, 100, 30, PixelColor32.FromHex("#8D99AE"));
            // Ruin Altar (Northeast)
            buffer.FillCircle(260, 50, 28, PixelColor32.FromHex("#83C5BE"));
            // Frozen Peaks / World's Edge (North Summit)
            buffer.FillCircle(340, 45, 34, PixelColor32.FromHex("#EDF2F4"));

            // 3. Campaign Expedition Road connecting biomes
            int[] pathX = { 192, 120, 50, 60, 110, 192, 270, 290, 260, 340 };
            int[] pathY = { 120, 160, 170, 95, 50,  120, 160, 100, 50,  45  };

            var roadCol = PixelColor32.FromHex("#6F1D1B");
            for (int i = 0; i < pathX.Length - 1; i++)
            {
                int x0 = pathX[i];
                int y0 = pathY[i];
                int x1 = pathX[i + 1];
                int y1 = pathY[i + 1];

                int steps = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
                for (int s = 0; s <= steps; s += 2) // Dotted trail
                {
                    float ratio = (float)s / steps;
                    int px = (int)(x0 + (x1 - x0) * ratio);
                    int py = (int)(y0 + (y1 - y0) * ratio);
                    buffer.SetPixel(px, py, roadCol);
                }
            }

            // 4. Mission Node Badges
            for (int i = 0; i < pathX.Length; i++)
            {
                int nx = pathX[i];
                int ny = pathY[i];

                buffer.FillCircle(nx, ny, 6, PixelColor32.FromHex("#2B2D42"));
                buffer.FillCircle(nx, ny, 4, i == 0 ? PixelColor32.FromHex("#FFD166") : PixelColor32.FromHex("#EF233C"));
                buffer.DrawCircle(nx, ny, 6, PixelColor32.FromHex("#D8F3DC"));
            }

            // 5. Compass Rose in bottom left
            int cx = 35;
            int cy = 35;
            buffer.FillCircle(cx, cy, 14, PixelColor32.FromHex("#EDE0D4"));
            buffer.DrawCircle(cx, cy, 14, parchmentEdge);
            buffer.FillRect(cx - 1, cy - 12, 3, 24, parchmentEdge);
            buffer.FillRect(cx - 12, cy - 1, 24, 3, parchmentEdge);
            buffer.FillCircle(cx, cy - 8, 3, PixelColor32.FromHex("#9B2226")); // North pointer

            return buffer;
        }
    }
}
