using System;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum CampNPCType
    {
        PriestessLeah,
        MerchantLeo,
        BlacksmithVulcan,
        ChefGourmet
    }

    public enum CampStructureType
    {
        TreeOfLifeAltar,
        BlacksmithForge,
        MerchantStall,
        ChefStewpot,
        BarracksPavilion,
        AncientRhythmMonolith
    }

    /// <summary>
    /// Generates Moonlighter-style pixel art spritesheets and tiles for Camp Hub NPCs and structures.
    /// </summary>
    public static class CampArtGenerator
    {
        /// <summary>
        /// Generates an animated NPC spritesheet (48x48 per frame, 4 frames Idle, 4 frames Action/Chant).
        /// </summary>
        public static PixelBitmapBuffer GenerateNPCSpriteSheet(CampNPCType npc)
        {
            int frameSize = 48;
            int width = frameSize * 4;
            int height = frameSize * 2;
            var buffer = new PixelBitmapBuffer(width, height);

            PixelColor32 robeColor;
            PixelColor32 accentColor;
            PixelColor32 hairColor;

            switch (npc)
            {
                case CampNPCType.PriestessLeah:
                    robeColor = PixelColor32.White;
                    accentColor = PixelColor32.SpiritCyan;
                    hairColor = PixelColor32.FromHex("#FFD54F"); // Golden blonde
                    break;
                case CampNPCType.MerchantLeo:
                    robeColor = PixelColor32.OceanicTeal;
                    accentColor = PixelColor32.AmberGold;
                    hairColor = PixelColor32.WoodBrown;
                    break;
                case CampNPCType.BlacksmithVulcan:
                    robeColor = PixelColor32.LeatherBrown;
                    accentColor = PixelColor32.Crimson;
                    hairColor = PixelColor32.ObsidianSlate;
                    break;
                default: // Chef
                    robeColor = PixelColor32.White;
                    accentColor = PixelColor32.Crimson;
                    hairColor = PixelColor32.WoodBrown;
                    break;
            }

            // Row 0: Idle (4 frames, subtle breathing bob)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * frameSize;
                int oy = 0;
                int bob = (f == 1 || f == 2) ? 1 : 0;
                DrawNPCBody(buffer, ox, oy + bob, frameSize, robeColor, accentColor, hairColor, npc);
            }

            // Row 1: Action (4 frames: Priestess blessing, Merchant showing wares, Blacksmith hammering, Chef stirring)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * frameSize;
                int oy = frameSize;
                DrawNPCBody(buffer, ox, oy, frameSize, robeColor, accentColor, hairColor, npc);
                DrawNPCActionOverlay(buffer, ox, oy, frameSize, npc, f);
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates large Moonlighter-style camp hub buildings and interactables.
        /// </summary>
        public static PixelBitmapBuffer GenerateCampStructure(CampStructureType structure)
        {
            int w = 64;
            int h = 64;
            var buffer = new PixelBitmapBuffer(w, h);

            switch (structure)
            {
                case CampStructureType.TreeOfLifeAltar:
                    // Massive spiritual bonsai tree with glowing cyan leaves and spirit altar pedestal
                    // Stone altar pedestal
                    buffer.FillRect(12, 44, 40, 16, PixelColor32.ObsidianSlate);
                    buffer.FillRect(16, 40, 32, 4, PixelColor32.IronGrey);
                    buffer.DrawRectOutline(12, 44, 40, 16, PixelColor32.Black);

                    // Wooden trunk
                    buffer.FillRect(28, 20, 8, 20, PixelColor32.WoodBrown);
                    buffer.DrawLine(28, 30, 20, 24, PixelColor32.WoodBrown);
                    buffer.DrawLine(36, 30, 44, 24, PixelColor32.WoodBrown);

                    // Glowing Foliage Canopy
                    buffer.DrawCircle(32, 16, 14, PixelColor32.VerdantMoss, true);
                    buffer.DrawCircle(22, 18, 10, PixelColor32.SpiritCyan, true);
                    buffer.DrawCircle(42, 18, 10, PixelColor32.SpiritCyan, true);
                    buffer.DrawCircle(32, 10, 8, PixelColor32.FromHex("#80CBC4"), true);

                    // Ancient Spirit Eye in Altar Base
                    buffer.DrawCircle(32, 52, 4, PixelColor32.SpiritCyan, true);
                    buffer.SetPixel(32, 52, PixelColor32.White);
                    break;

                case CampStructureType.BlacksmithForge:
                    // Stone furnace, chimney with smoke, and heavy steel anvil
                    buffer.FillRect(8, 20, 32, 40, PixelColor32.FromHex("#37474F"));
                    buffer.FillRect(14, 4, 12, 16, PixelColor32.FromHex("#263238")); // Chimney
                    // Fire opening
                    buffer.FillRect(16, 38, 16, 18, PixelColor32.Crimson);
                    buffer.FillRect(19, 42, 10, 12, PixelColor32.AmberGold);
                    buffer.FillRect(22, 46, 4, 6, PixelColor32.FeverYellow);
                    // Anvil to the right
                    buffer.FillRect(44, 42, 16, 18, PixelColor32.IronGrey);
                    buffer.FillRect(42, 38, 20, 6, PixelColor32.IronGrey);
                    buffer.DrawLine(40, 38, 44, 44, PixelColor32.IronGrey); // Horn
                    break;

                case CampStructureType.MerchantStall:
                    // Striped awning tent with wooden counter and crates
                    buffer.FillRect(8, 36, 48, 24, PixelColor32.WoodBrown); // Wooden counter
                    // Striped awning canopy
                    for (int x = 4; x < 60; x += 8)
                    {
                        var col = ((x / 8) % 2 == 0) ? PixelColor32.Crimson : PixelColor32.White;
                        buffer.FillRect(x, 12, 8, 16, col);
                    }
                    // Support posts
                    buffer.DrawLine(6, 12, 6, 60, PixelColor32.WoodBrown);
                    buffer.DrawLine(57, 12, 57, 60, PixelColor32.WoodBrown);
                    // Barrels & sacks on counter
                    buffer.DrawCircle(16, 44, 6, PixelColor32.AmberGold, true);
                    buffer.DrawCircle(32, 44, 5, PixelColor32.OceanicTeal, true);
                    buffer.DrawCircle(46, 44, 6, PixelColor32.LeatherBrown, true);
                    break;

                case CampStructureType.ChefStewpot:
                    // Roaring campfire with hanging black iron cauldron
                    buffer.FillRect(20, 52, 24, 8, PixelColor32.LeatherBrown); // Firewood
                    buffer.FillRect(24, 46, 16, 8, PixelColor32.Crimson); // Fire
                    buffer.FillRect(28, 44, 8, 6, PixelColor32.FeverYellow);
                    // Hanging iron pot
                    buffer.DrawCircle(32, 32, 12, PixelColor32.ObsidianSlate, true);
                    buffer.DrawCircle(32, 28, 9, PixelColor32.FromHex("#43A047"), true); // Green savory stew
                    // Pot tripod
                    buffer.DrawLine(16, 56, 32, 12, PixelColor32.IronGrey);
                    buffer.DrawLine(48, 56, 32, 12, PixelColor32.IronGrey);
                    break;

                case CampStructureType.BarracksPavilion:
                    // Large warrior marquee tent with army heraldry banner
                    buffer.FillRect(6, 24, 52, 36, PixelColor32.FromHex("#ECEFF1")); // White tent canvas
                    buffer.FillRect(24, 38, 16, 22, PixelColor32.ObsidianSlate); // Tent flap doorway
                    // Crest emblem
                    buffer.DrawCircle(32, 18, 10, PixelColor32.AmberGold, true);
                    buffer.DrawLine(32, 4, 32, 18, PixelColor32.IronGrey); // Center pole
                    buffer.FillRect(32, 4, 14, 8, PixelColor32.Crimson); // Pennant flag
                    break;

                case CampStructureType.AncientRhythmMonolith:
                    // Weathered ancient stone totem engraved with glowing rhythm runes
                    buffer.FillRect(20, 8, 24, 52, PixelColor32.FromHex("#455A64"));
                    buffer.FillRect(14, 54, 36, 8, PixelColor32.FromHex("#37474F"));
                    // Engraved glowing runes (Boom, Tak, Rat, Ting symbols)
                    buffer.DrawRectOutline(26, 14, 12, 6, PixelColor32.AmberGold); // Boom Square
                    buffer.DrawCircle(32, 28, 3, PixelColor32.Crimson, false); // Tak Circle
                    buffer.DrawLine(27, 40, 32, 35, PixelColor32.VerdantMoss); // Rat Triangle
                    buffer.DrawLine(37, 40, 32, 35, PixelColor32.VerdantMoss);
                    buffer.DrawLine(27, 40, 37, 40, PixelColor32.VerdantMoss);
                    buffer.DrawLine(30, 46, 34, 50, PixelColor32.SpiritCyan); // Ting Cross
                    buffer.DrawLine(34, 46, 30, 50, PixelColor32.SpiritCyan);
                    break;
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        private static void DrawNPCBody(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 robe, PixelColor32 accent, PixelColor32 hair, CampNPCType npc)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2 + 2;

            // Robe / Body
            buffer.FillRect(cx - 8, cy - 2, 16, 16, robe);
            buffer.FillRect(cx - 6, cy + 12, 12, 4, PixelColor32.LeatherBrown); // Boots

            // Face & Large Eyes
            buffer.DrawCircle(cx, cy - 8, 8, PixelColor32.FromHex("#FFE0B2"), true); // Skin tone
            buffer.DrawCircle(cx + 2, cy - 8, 3, PixelColor32.White, true); // Eye
            buffer.DrawCircle(cx + 3, cy - 8, 1, PixelColor32.Black, true); // Pupil

            // Hair / Headgear
            buffer.FillRect(cx - 8, cy - 16, 16, 8, hair);
            if (npc == CampNPCType.PriestessLeah)
            {
                // Priestess Circlet & Mantle
                buffer.DrawLine(cx - 8, cy - 12, cx + 8, cy - 12, accent);
                buffer.SetPixel(cx, cy - 13, PixelColor32.SpiritCyan); // Circlet gem
            }
            else if (npc == CampNPCType.MerchantLeo)
            {
                // Merchant Feather Cap
                buffer.FillRect(cx - 9, cy - 18, 18, 4, accent);
                buffer.DrawLine(cx - 4, cy - 24, cx + 2, cy - 18, PixelColor32.Crimson);
            }
            else if (npc == CampNPCType.BlacksmithVulcan)
            {
                // Blacksmith Leather Apron & Bandana
                buffer.FillRect(cx - 8, cy - 16, 16, 4, accent);
                buffer.FillRect(cx - 6, cy, 12, 12, PixelColor32.LeatherBrown);
            }
        }

        private static void DrawNPCActionOverlay(PixelBitmapBuffer buffer, int ox, int oy, int size, CampNPCType npc, int frame)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;

            if (npc == CampNPCType.PriestessLeah)
            {
                // Holy Spirit particles rising
                buffer.DrawCircle(cx + (frame - 2) * 6, cy - 18 - frame * 3, 2 + (frame % 2), PixelColor32.SpiritCyan, true);
            }
            else if (npc == CampNPCType.BlacksmithVulcan)
            {
                // Hammer swing & sparks
                if (frame == 2)
                {
                    buffer.SetPixel(cx + 12, cy + 6, PixelColor32.FeverYellow);
                    buffer.SetPixel(cx + 14, cy + 4, PixelColor32.White);
                    buffer.SetPixel(cx + 10, cy + 8, PixelColor32.AmberGold);
                }
            }
            else if (npc == CampNPCType.MerchantLeo)
            {
                // Gold coin toss
                int coinY = cy - 12 - (int)(Math.Sin(frame * Math.PI / 3) * 8);
                buffer.DrawCircle(cx + 10, coinY, 2, PixelColor32.StarGold, true);
            }
        }
    }
}
