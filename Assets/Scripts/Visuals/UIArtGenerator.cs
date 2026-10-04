using System;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Visuals
{
    public enum DrumButtonState
    {
        Normal,
        Pressed,
        FeverGlow
    }

    public enum CurrencyCoinType
    {
        Bronze,
        Silver,
        Gold,
        Platinum
    }

    public enum ChestTierType
    {
        Wood,
        Iron,
        Golden,
        Relic
    }

    /// <summary>
    /// Generates Moonlighter-style UI frames, rhythmic chant drum buttons, health/fever bars,
    /// currency coins, and treasure chest pixel art.
    /// </summary>
    public static class UIArtGenerator
    {
        /// <summary>
        /// Generates 32x32 px interactive Drum Chant Buttons for Boom, Tak, Rat, and Ting.
        /// </summary>
        public static PixelBitmapBuffer GenerateDrumButton(DrumId drum, DrumButtonState state)
        {
            var buffer = new PixelBitmapBuffer(32, 32);
            PixelColor32 baseColor;
            PixelColor32 symbolColor = PixelColor32.White;

            switch (drum)
            {
                case DrumId.Boom:
                    baseColor = (state == DrumButtonState.FeverGlow) ? PixelColor32.FeverYellow : PixelColor32.AmberGold;
                    break;
                case DrumId.Tak:
                    baseColor = (state == DrumButtonState.FeverGlow) ? PixelColor32.DragonRuby : PixelColor32.Crimson;
                    break;
                case DrumId.Rat:
                    baseColor = (state == DrumButtonState.FeverGlow) ? PixelColor32.FromHex("#AEEA00") : PixelColor32.VerdantMoss;
                    break;
                case DrumId.Ting:
                    baseColor = (state == DrumButtonState.FeverGlow) ? PixelColor32.White : PixelColor32.SpiritCyan;
                    break;
                default:
                    baseColor = PixelColor32.IronGrey;
                    break;
            }

            int inset = (state == DrumButtonState.Pressed) ? 2 : 0;
            int cx = 16 + inset;
            int cy = 16 + inset;

            // 1. Button Base Frame / Plate
            buffer.FillRect(2 + inset, 2 + inset, 28 - inset, 28 - inset, PixelColor32.FromHex("#1A202C"));
            buffer.FillRect(4 + inset, 4 + inset, 24 - inset, 24 - inset, baseColor);

            if (state == DrumButtonState.FeverGlow)
            {
                buffer.DrawRectOutline(1 + inset, 1 + inset, 30 - inset, 30 - inset, PixelColor32.White);
            }

            // 2. Drum Geometric Symbol
            switch (drum)
            {
                case DrumId.Boom:
                    // Square Rune
                    buffer.FillRect(cx - 5, cy - 5, 10, 10, symbolColor);
                    buffer.FillRect(cx - 3, cy - 3, 6, 6, baseColor);
                    break;

                case DrumId.Tak:
                    // Circle Rune
                    buffer.DrawCircle(cx, cy, 5, symbolColor, true);
                    buffer.DrawCircle(cx, cy, 3, baseColor, true);
                    break;

                case DrumId.Rat:
                    // Triangle Rune
                    buffer.DrawLine(cx - 6, cy + 5, cx, cy - 6, symbolColor);
                    buffer.DrawLine(cx + 6, cy + 5, cx, cy - 6, symbolColor);
                    buffer.DrawLine(cx - 6, cy + 5, cx + 6, cy + 5, symbolColor);
                    break;

                case DrumId.Ting:
                    // Cross / Diamond Rune
                    buffer.DrawLine(cx - 5, cy - 5, cx + 5, cy + 5, symbolColor);
                    buffer.DrawLine(cx - 5, cy + 5, cx + 5, cy - 5, symbolColor);
                    buffer.SetPixel(cx, cy, baseColor);
                    break;
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates a 96x48 Moonlighter-style parchment dialogue frame with 9-slice stone/gold borders.
        /// </summary>
        public static PixelBitmapBuffer GenerateDialogueFrame()
        {
            var buffer = new PixelBitmapBuffer(96, 48);

            // Aged parchment background
            buffer.FillRect(0, 0, 96, 48, PixelColor32.FromHex("#212121")); // Outer border shadow
            buffer.FillRect(2, 2, 92, 44, PixelColor32.FromHex("#F5E6CA")); // Warm parchment ivory
            buffer.FillRect(4, 4, 88, 40, PixelColor32.FromHex("#FFF8E7")); // Light parchment inner

            // Engraved corner crests
            buffer.FillRect(2, 2, 6, 6, PixelColor32.AmberGold);
            buffer.FillRect(88, 2, 6, 6, PixelColor32.AmberGold);
            buffer.FillRect(2, 40, 6, 6, PixelColor32.AmberGold);
            buffer.FillRect(88, 40, 6, 6, PixelColor32.AmberGold);

            // Border piping
            buffer.DrawRectOutline(3, 3, 90, 42, PixelColor32.WoodBrown);

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates 128x16 px Rhythm Beat Track HUD bar.
        /// </summary>
        public static PixelBitmapBuffer GenerateRhythmBeatTrack()
        {
            var buffer = new PixelBitmapBuffer(128, 16);

            // Dark stone slate track
            buffer.FillRect(0, 0, 128, 16, PixelColor32.FromHex("#1A202C"));
            buffer.FillRect(2, 2, 124, 12, PixelColor32.FromHex("#2D3748"));

            // 4 Beat Window Indicator Pips (Quarter notes)
            for (int b = 0; b < 4; b++)
            {
                int beatX = 16 + b * 32;
                buffer.DrawCircle(beatX, 8, 4, PixelColor32.AmberGold, true);
                buffer.DrawCircle(beatX, 8, 2, PixelColor32.White, true);
            }

            // Target judgment bracket at center-left
            buffer.DrawRectOutline(12, 1, 10, 14, PixelColor32.SpiritCyan);

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates 16x16 px Currency Coin sprites (Bronze, Silver, Gold, Platinum).
        /// </summary>
        public static PixelBitmapBuffer GenerateCurrencyCoin(CurrencyCoinType coin)
        {
            var buffer = new PixelBitmapBuffer(16, 16);
            PixelColor32 rimColor;
            PixelColor32 innerColor;

            switch (coin)
            {
                case CurrencyCoinType.Bronze:
                    rimColor = PixelColor32.LeatherBrown;
                    innerColor = PixelColor32.FromHex("#D78B45");
                    break;
                case CurrencyCoinType.Silver:
                    rimColor = PixelColor32.IronGrey;
                    innerColor = PixelColor32.FromHex("#ECEFF1");
                    break;
                case CurrencyCoinType.Gold:
                    rimColor = PixelColor32.AmberGold;
                    innerColor = PixelColor32.StarGold;
                    break;
                default: // Platinum
                    rimColor = PixelColor32.OceanicTeal;
                    innerColor = PixelColor32.SpiritCyan;
                    break;
            }

            buffer.DrawCircle(8, 8, 6, rimColor, true);
            buffer.DrawCircle(8, 8, 4, innerColor, true);
            buffer.SetPixel(6, 6, PixelColor32.White); // Glint
            buffer.SetPixel(8, 8, rimColor); // Center stamp

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates 24x24 px Loot Chest sprite (Closed, Shaking, Opened).
        /// </summary>
        public static PixelBitmapBuffer GenerateLootChest(ChestTierType tier, bool opened = false)
        {
            var buffer = new PixelBitmapBuffer(24, 24);
            PixelColor32 bodyCol = (tier == ChestTierType.Wood) ? PixelColor32.WoodBrown :
                                  (tier == ChestTierType.Iron) ? PixelColor32.IronGrey :
                                  (tier == ChestTierType.Golden) ? PixelColor32.StarGold : PixelColor32.SpiritCyan;

            PixelColor32 trimCol = (tier == ChestTierType.Golden || tier == ChestTierType.Relic) ? PixelColor32.AmberGold : PixelColor32.ObsidianSlate;

            // Chest Base
            buffer.FillRect(3, 10, 18, 11, bodyCol);
            buffer.DrawRectOutline(3, 10, 18, 11, trimCol);

            if (!opened)
            {
                // Lid Closed
                buffer.FillRect(2, 4, 20, 7, bodyCol);
                buffer.DrawRectOutline(2, 4, 20, 7, trimCol);
                // Keyhole Lock
                buffer.FillRect(11, 9, 2, 4, PixelColor32.AmberGold);
            }
            else
            {
                // Lid Opened (Flipped Up)
                buffer.FillRect(2, 1, 20, 6, bodyCol);
                buffer.DrawRectOutline(2, 1, 20, 6, trimCol);
                // Golden Treasure Glow Radiating from Inside
                buffer.FillRect(5, 7, 14, 4, PixelColor32.FeverYellow);
                buffer.SetPixel(8, 6, PixelColor32.White);
                buffer.SetPixel(15, 6, PixelColor32.White);
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }
    }
}
