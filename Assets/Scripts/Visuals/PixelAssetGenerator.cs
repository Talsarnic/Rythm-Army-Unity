using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Procedural Moonlighter-style Pixel Art Generator and Sprite Sheet Rasterizer.
    /// Generates high-fidelity 16-bit spritesheets, equipment overlays, enemy archetypes,
    /// colossal boss telegraph frames, item icons, and biome tiles.
    /// </summary>
    public static class PixelAssetGenerator
    {
        /// <summary>
        /// Generates the complete 8-row animation sprite sheet for any squad unit class.
        /// (Idle: 4 frames, March: 4 frames, Attack: 4 frames, Defend: 2 frames,
        /// Fever: 4 frames, Charge: 3 frames, Jump: 3 frames, Hurt: 2 frames, Death: 3 frames).
        /// </summary>
        public static PixelBitmapBuffer GenerateUnitSpriteSheet(UnitClass unitClass, Subspecies species = Subspecies.Normal)
        {
            var spec = PixelSpriteRegistry.GetSpec(unitClass);
            int size = spec.SpriteWidth;
            int sheetWidth = size * 4;
            int sheetHeight = size * 9;
            var buffer = new PixelBitmapBuffer(sheetWidth, sheetHeight);

            PixelColor32 skinColor = GetSpeciesSkinColor(species);
            PixelColor32 classColor = GetClassAccentColor(unitClass);
            PixelColor32 outlineColor = PixelColor32.Black;

            // 1. Idle (Row 0, 4 frames: gentle breathing bob)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = 0;
                int bobY = (f == 1 || f == 2) ? 1 : 0;
                DrawUnitBody(buffer, ox, oy + bobY, size, skinColor, classColor, unitClass);
            }

            // 2. March (Row 1, 4 frames: step cycle)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size;
                int stride = (f == 0 || f == 2) ? 2 : 0;
                DrawUnitBody(buffer, ox, oy, size, skinColor, classColor, unitClass, stride);
            }

            // 3. Attack (Row 2, 4 frames: windup, strike, followthrough, recover)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size * 2;
                int forwardX = (f == 1) ? 4 : (f == 2 ? 2 : 0);
                DrawUnitBody(buffer, ox + forwardX, oy, size, skinColor, classColor, unitClass);
                DrawWeaponSlashFX(buffer, ox + forwardX, oy, size, classColor, f);
            }

            // 4. Defend (Row 3, 2 frames: shield wall crouch)
            for (int f = 0; f < 2; f++)
            {
                int ox = f * size;
                int oy = size * 3;
                DrawUnitBody(buffer, ox, oy - 1, size, skinColor, classColor, unitClass);
                // Draw shield glow
                buffer.DrawCircle(ox + size / 2, oy + size / 2, size / 3, PixelColor32.SpiritCyan, false);
            }

            // 5. Fever (Row 4, 4 frames: jump dance & sparkle)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size * 4;
                int hopY = (f % 2 == 0) ? 3 : 1;
                DrawUnitBody(buffer, ox, oy + hopY, size, skinColor, PixelColor32.FeverYellow, unitClass);
                buffer.SetPixel(ox + 4 + f * 5, oy + 4, PixelColor32.FeverYellow);
                buffer.SetPixel(ox + size - 6, oy + 6 + f * 3, PixelColor32.White);
            }

            // 6. Charge (Row 5, 3 frames: low forward rush)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 5;
                DrawUnitBody(buffer, ox + f * 2, oy - 2, size, skinColor, PixelColor32.AmberGold, unitClass);
            }

            // 7. Jump (Row 6, 3 frames: launch, apex, land)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 6;
                int jumpOffset = (f == 1) ? 6 : (f == 0 ? 3 : 0);
                DrawUnitBody(buffer, ox, oy + jumpOffset, size, skinColor, classColor, unitClass);
            }

            // 8. Hurt (Row 7, 2 frames: flash white / recoil)
            for (int f = 0; f < 2; f++)
            {
                int ox = f * size;
                int oy = size * 7;
                var hurtSkin = (f == 0) ? PixelColor32.White : PixelColor32.Crimson;
                DrawUnitBody(buffer, ox - 2, oy, size, hurtSkin, classColor, unitClass);
            }

            // 9. Death (Row 8, 3 frames: collapse & spirit fade)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 8;
                if (f < 2)
                {
                    DrawUnitBody(buffer, ox, oy - f * 2, size, PixelColor32.ObsidianSlate, classColor, unitClass);
                }
                else
                {
                    // Spirit orb floating up
                    buffer.DrawCircle(ox + size / 2, oy + size / 2 + 4, 3, PixelColor32.SpiritCyan, true);
                }
            }

            buffer.ApplyDarkOutline(outlineColor);
            return buffer;
        }

        /// <summary>
        /// Generates enemy and colossal boss spritesheets with custom scale and telegraph glow.
        /// </summary>
        public static PixelBitmapBuffer GenerateEnemySpriteSheet(EnemyKind kind)
        {
            int size = 32;
            if (kind == EnemyKind.IronBehemoth || kind == EnemyKind.DrakeTitan || kind == EnemyKind.ColossusGolem) size = 96;
            else if (kind == EnemyKind.WildBoar || kind == EnemyKind.GiantBoar || kind == EnemyKind.StoneWall || kind == EnemyKind.Watchtower || kind == EnemyKind.CatapultTower) size = 64;
            else if (kind == EnemyKind.TribeCavalry || kind == EnemyKind.TribeHammerer || kind == EnemyKind.TribeSkyrider) size = 48;

            int sheetWidth = size * 4;
            int sheetHeight = size * 6;
            var buffer = new PixelBitmapBuffer(sheetWidth, sheetHeight);

            PixelColor32 baseColor = GetEnemyBaseColor(kind);
            PixelColor32 accentColor = GetEnemyAccentColor(kind);

            for (int row = 0; row < 6; row++)
            {
                int frameCount = (row == 2 || row == 4) ? 3 : (row == 5 ? 2 : 4);
                for (int f = 0; f < frameCount; f++)
                {
                    int ox = f * size;
                    int oy = row * size;

                    if (row == 0) // Idle
                    {
                        DrawEnemyBody(buffer, ox, oy, size, baseColor, accentColor, kind);
                    }
                    else if (row == 1) // March / Patrol
                    {
                        DrawEnemyBody(buffer, ox, oy, size, baseColor, accentColor, kind, f * 2);
                    }
                    else if (row == 2) // Attack
                    {
                        DrawEnemyBody(buffer, ox - f * 3, oy, size, baseColor, accentColor, kind);
                    }
                    else if (row == 3) // Telegraph Warning Glow
                    {
                        DrawEnemyBody(buffer, ox, oy, size, PixelColor32.Crimson, PixelColor32.AmberGold, kind);
                        buffer.DrawCircle(ox + size / 2, oy + size / 2, size / 2 - 2, PixelColor32.Crimson, false);
                    }
                    else if (row == 4) // Hurt / Stagger
                    {
                        DrawEnemyBody(buffer, ox + 2, oy, size, PixelColor32.White, accentColor, kind);
                    }
                    else if (row == 5) // Death
                    {
                        DrawEnemyBody(buffer, ox, oy - 2, size, PixelColor32.ObsidianSlate, accentColor, kind);
                    }
                }
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates a 24x24 px pixel icon for weapons, armors, materials, or charms.
        /// </summary>
        public static PixelBitmapBuffer GenerateItemIcon(string itemId)
        {
            var buffer = new PixelBitmapBuffer(24, 24);
            buffer.FillRect(1, 1, 22, 22, PixelColor32.FromHex("#1A202C")); // Slate dark tile background

            if (itemId.StartsWith("wpn-") || itemId.StartsWith("spear-") || itemId.StartsWith("sword-") || itemId.StartsWith("bow-"))
            {
                // Sword / Spear blade icon
                buffer.DrawLine(4, 20, 19, 5, PixelColor32.IronGrey);
                buffer.DrawLine(5, 20, 20, 5, PixelColor32.White);
                buffer.DrawLine(3, 19, 7, 23, PixelColor32.WoodBrown); // Hilt
                buffer.SetPixel(19, 4, PixelColor32.AmberGold); // Gem tip
            }
            else if (itemId.StartsWith("shd-") || itemId.StartsWith("shield-"))
            {
                // Shield crest
                buffer.FillRect(5, 4, 14, 15, PixelColor32.WoodBrown);
                buffer.DrawCircle(12, 11, 4, PixelColor32.IronGrey, true);
                buffer.SetPixel(12, 11, PixelColor32.SpiritCyan);
            }
            else if (itemId.StartsWith("hlm-") || itemId.StartsWith("helm-"))
            {
                // Helmet visor
                buffer.FillRect(5, 5, 14, 12, PixelColor32.IronGrey);
                buffer.DrawLine(6, 11, 17, 11, PixelColor32.Black); // Visor slit
                buffer.DrawLine(11, 2, 13, 5, PixelColor32.Crimson); // Plume
            }
            else if (itemId.StartsWith("gem-"))
            {
                // Elemental Gem Shard
                PixelColor32 gemCol = itemId.Contains("flame") ? PixelColor32.Crimson :
                                      itemId.Contains("frost") ? PixelColor32.SpiritCyan :
                                      itemId.Contains("lightning") ? PixelColor32.AmberGold : PixelColor32.VerdantMoss;
                buffer.DrawCircle(12, 12, 6, gemCol, true);
                buffer.SetPixel(10, 9, PixelColor32.White); // Glint
            }
            else if (itemId.StartsWith("charm-"))
            {
                // Tactical Field Charm
                buffer.DrawCircle(12, 12, 7, PixelColor32.StarGold, false);
                buffer.FillRect(9, 9, 6, 6, PixelColor32.OceanicTeal);
            }
            else
            {
                // Raw Material (Ore, timber, hide)
                buffer.FillRect(6, 7, 12, 10, PixelColor32.WoodBrown);
                buffer.DrawLine(7, 8, 17, 15, PixelColor32.AmberGold);
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates a 16x16 px environment biome tile.
        /// </summary>
        public static PixelBitmapBuffer GenerateBiomeTile(BiomeType biome, TileLayerType layer)
        {
            var buffer = new PixelBitmapBuffer(16, 16);
            PixelColor32 baseColor;
            PixelColor32 detailColor;

            switch (biome)
            {
                case BiomeType.CoralCoast:
                    baseColor = PixelColor32.FromHex("#FFE082"); // Warm sand
                    detailColor = PixelColor32.OceanicTeal;
                    break;
                case BiomeType.JungleFort:
                    baseColor = PixelColor32.FromHex("#388E3C"); // Jungle foliage
                    detailColor = PixelColor32.FromHex("#1B5E20");
                    break;
                case BiomeType.VolcanicCaldera:
                    baseColor = PixelColor32.FromHex("#263238"); // Obsidian basalt
                    detailColor = PixelColor32.Crimson;
                    break;
                case BiomeType.IronBastion:
                    baseColor = PixelColor32.FromHex("#546E7A"); // Cobblestone
                    detailColor = PixelColor32.FromHex("#37474F");
                    break;
                default:
                    baseColor = PixelColor32.FromHex("#78909C");
                    detailColor = PixelColor32.FromHex("#455A64");
                    break;
            }

            buffer.FillRect(0, 0, 16, 16, baseColor);

            if (layer == TileLayerType.Ground)
            {
                // Textured dither
                buffer.SetPixel(3, 4, detailColor);
                buffer.SetPixel(7, 12, detailColor);
                buffer.SetPixel(13, 8, detailColor);
                buffer.SetPixel(11, 2, detailColor);
            }
            else if (layer == TileLayerType.Obstacles)
            {
                // Stone/structure border
                buffer.FillRect(2, 2, 12, 12, detailColor);
                buffer.DrawLine(4, 4, 12, 4, PixelColor32.White);
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        private static void DrawUnitBody(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 skin, PixelColor32 accent, UnitClass unitClass, int stride = 0)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;
            int radius = size / 4;

            // 1. Head / Core sphere
            buffer.DrawCircle(cx, cy + 2, radius, skin, true);

            // 2. Large Iconic Eyeball (Moonlighter/Patapon style)
            buffer.DrawCircle(cx + 2, cy + 2, radius / 2 + 1, PixelColor32.White, true);
            buffer.DrawCircle(cx + 3, cy + 2, radius / 4 + 1, PixelColor32.Black, true);
            buffer.SetPixel(cx + 2, cy + 3, PixelColor32.White); // Eye catchlight

            // 3. Class Hat / Crest / Plume
            buffer.DrawLine(cx - 2, cy + radius + 1, cx + 4, cy + radius + 3, accent);
            if (unitClass == UnitClass.Banner)
            {
                buffer.DrawLine(cx - 4, cy - radius, cx - 4, cy + radius + 8, PixelColor32.WoodBrown); // Banner pole
                buffer.FillRect(cx - 4, cy + radius + 2, 7, 5, accent); // Flag cloth
            }

            // 4. Feet / Stride legs
            buffer.FillRect(cx - 3 - stride, cy - radius - 1, 3, 2, PixelColor32.LeatherBrown);
            buffer.FillRect(cx + 1 + stride, cy - radius - 1, 3, 2, PixelColor32.LeatherBrown);
        }

        private static void DrawWeaponSlashFX(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 color, int frame)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;
            if (frame == 1) // Strike slash arc
            {
                buffer.DrawLine(cx + 4, cy + 6, cx + 12, cy - 2, color);
                buffer.DrawLine(cx + 5, cy + 6, cx + 13, cy - 2, PixelColor32.White);
            }
            else if (frame == 2) // Impact star
            {
                buffer.SetPixel(cx + 12, cy + 2, PixelColor32.FeverYellow);
                buffer.SetPixel(cx + 13, cy + 3, PixelColor32.White);
            }
        }

        private static void DrawEnemyBody(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 baseColor, PixelColor32 accent, EnemyKind kind, int stride = 0)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;
            int r = size / 3;

            // Core monster silhouette
            buffer.DrawCircle(cx, cy, r, baseColor, true);

            // Menacing Glowing Eye
            buffer.DrawCircle(cx - 2, cy + 2, Math.Max(2, r / 3), accent, true);
            buffer.SetPixel(cx - 2, cy + 2, PixelColor32.White);

            // Horns / Spikes for heavy monsters
            if (size >= 48)
            {
                buffer.DrawLine(cx + r / 2, cy + r - 2, cx + r, cy + r + 4, PixelColor32.IronGrey);
                buffer.DrawLine(cx - r / 2, cy + r - 2, cx - r, cy + r + 4, PixelColor32.IronGrey);
            }

            // Feet
            buffer.FillRect(cx - 4 - stride, cy - r, 4, 3, PixelColor32.Black);
            buffer.FillRect(cx + 2 + stride, cy - r, 4, 3, PixelColor32.Black);
        }

        private static PixelColor32 GetSpeciesSkinColor(Subspecies species)
        {
            switch (species)
            {
                case Subspecies.Swiftpaw: return PixelColor32.FromHex("#FFCC80"); // Sandy rabbit
                case Subspecies.Frogtide: return PixelColor32.FromHex("#80CBC4"); // Amphibian cyan
                case Subspecies.Ironwool: return PixelColor32.FromHex("#ECEFF1"); // Pure wool white
                case Subspecies.Colossus: return PixelColor32.FromHex("#B0BEC5"); // Slate horn
                case Subspecies.Apex: return PixelColor32.FromHex("#FFE082"); // Celestial gold
                default: return PixelColor32.FromHex("#FFFFFF"); // Classic white
            }
        }

        private static PixelColor32 GetClassAccentColor(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Banner: return PixelColor32.AmberGold;
                case UnitClass.Spearman: return PixelColor32.Crimson;
                case UnitClass.Swordsman: return PixelColor32.OceanicTeal;
                case UnitClass.Archer: return PixelColor32.VerdantMoss;
                case UnitClass.Cavalry: return PixelColor32.AmberGold;
                case UnitClass.Hammerer: return PixelColor32.ObsidianSlate;
                case UnitClass.Hornist: return PixelColor32.SpiritCyan;
                case UnitClass.Skyrider: return PixelColor32.MithrilBlue;
                case UnitClass.Mage: return PixelColor32.FromHex("#AB47BC");
                case UnitClass.Brawler: return PixelColor32.FromHex("#E65100");
                default: return PixelColor32.AmberGold;
            }
        }

        private static PixelColor32 GetEnemyBaseColor(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.PlainsRunner: return PixelColor32.AmberGold;
                case EnemyKind.WildBoar: return PixelColor32.LeatherBrown;
                case EnemyKind.GiantBoar: return PixelColor32.Crimson;
                case EnemyKind.Stag: return PixelColor32.WoodBrown;
                case EnemyKind.IronBehemoth: return PixelColor32.ObsidianSlate;
                case EnemyKind.DrakeTitan: return PixelColor32.DragonRuby;
                case EnemyKind.ColossusGolem: return PixelColor32.IronGrey;
                default: return PixelColor32.FromHex("#455A64");
            }
        }

        private static PixelColor32 GetEnemyAccentColor(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.DrakeTitan: return PixelColor32.AmberGold;
                case EnemyKind.IronBehemoth: return PixelColor32.Crimson;
                case EnemyKind.ColossusGolem: return PixelColor32.SpiritCyan;
                default: return PixelColor32.Crimson;
            }
        }
    }
}
