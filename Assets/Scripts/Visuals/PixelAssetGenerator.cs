using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Procedural Moonlighter-style Pixel Art Generator and Sprite Sheet Rasterizer.
    /// Generates high-fidelity 16-bit spritesheets, equipment overlays, enemy archetypes,
    /// colossal boss telegraph frames, item icons, and biome tiles with rich shading ramps,
    /// anatomical proportions, gear details, and dynamic animation keyframes.
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

            // 1. Idle (Row 0, 4 frames: gentle breathing bob)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = 0;
                int bobY = (f == 1 || f == 2) ? 1 : 0;
                DrawUnitFrame(buffer, ox, oy + bobY, size, skinColor, classColor, unitClass, species, 0, f, 0);
            }

            // 2. March (Row 1, 4 frames: step cycle)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size;
                int stride = (f == 0) ? -2 : (f == 2 ? 2 : 0);
                int bob = (f == 1 || f == 3) ? -1 : 0;
                DrawUnitFrame(buffer, ox, oy + bob, size, skinColor, classColor, unitClass, species, 1, f, stride);
            }

            // 3. Attack (Row 2, 4 frames: windup, strike, followthrough, recover)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size * 2;
                int forwardX = (f == 1) ? 4 : (f == 2 ? 2 : (f == 0 ? -1 : 0));
                DrawUnitFrame(buffer, ox + forwardX, oy, size, skinColor, classColor, unitClass, species, 2, f, 0);
                DrawWeaponSlashFX(buffer, ox + forwardX, oy, size, classColor, f);
            }

            // 4. Defend (Row 3, 2 frames: shield wall crouch)
            for (int f = 0; f < 2; f++)
            {
                int ox = f * size;
                int oy = size * 3;
                DrawUnitFrame(buffer, ox, oy - 1, size, skinColor, classColor, unitClass, species, 3, f, 0);
                // Draw divine shield barrier glow
                int cx = ox + size / 2;
                int cy = oy + size / 2;
                buffer.DrawCircle(cx + 4, cy, size / 3, PixelColor32.SpiritCyan, false);
                buffer.DrawCircle(cx + 4, cy, size / 3 - 2, new PixelColor32(0, 229, 255, 120), false);
            }

            // 5. Fever (Row 4, 4 frames: jump dance & sparkle)
            for (int f = 0; f < 4; f++)
            {
                int ox = f * size;
                int oy = size * 4;
                int hopY = (f % 2 == 0) ? 3 : 1;
                DrawUnitFrame(buffer, ox, oy + hopY, size, skinColor, PixelColor32.FeverYellow, unitClass, species, 4, f, 0);
                // Golden sparkles
                buffer.SetPixel(ox + 4 + f * 5, oy + 4, PixelColor32.FeverYellow);
                buffer.SetPixel(ox + 5 + f * 5, oy + 4, PixelColor32.White);
                buffer.SetPixel(ox + size - 6, oy + 6 + f * 3, PixelColor32.AmberGold);
            }

            // 6. Charge (Row 5, 3 frames: low forward rush)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 5;
                DrawUnitFrame(buffer, ox + f * 2, oy - 2, size, skinColor, PixelColor32.AmberGold, unitClass, species, 5, f, 1);
                // Dust streak
                buffer.FillRect(ox + 2, oy + size - 4, 4 + f * 2, 2, new PixelColor32(200, 180, 140, 180));
            }

            // 7. Jump (Row 6, 3 frames: launch, apex, land)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 6;
                int jumpOffset = (f == 1) ? 7 : (f == 0 ? 3 : 1);
                DrawUnitFrame(buffer, ox, oy + jumpOffset, size, skinColor, classColor, unitClass, species, 6, f, 0);
            }

            // 8. Hurt (Row 7, 2 frames: flash white / recoil)
            for (int f = 0; f < 2; f++)
            {
                int ox = f * size;
                int oy = size * 7;
                var hurtSkin = (f == 0) ? PixelColor32.White : PixelColor32.Crimson;
                DrawUnitFrame(buffer, ox - 3, oy, size, hurtSkin, classColor, unitClass, species, 7, f, 0);
            }

            // 9. Death (Row 8, 3 frames: collapse & spirit fade)
            for (int f = 0; f < 3; f++)
            {
                int ox = f * size;
                int oy = size * 8;
                if (f < 2)
                {
                    DrawUnitFrame(buffer, ox, oy - f * 3, size, PixelColor32.ObsidianSlate, classColor, unitClass, species, 8, f, 0);
                }
                else
                {
                    // Ascending spirit wisp
                    int cx = ox + size / 2;
                    int cy = oy + size / 2 + 4;
                    buffer.FillCircle(cx, cy, 3, PixelColor32.SpiritCyan);
                    buffer.FillCircle(cx, cy, 1, PixelColor32.White);
                    buffer.DrawCircle(cx, cy, 4, new PixelColor32(0, 229, 255, 120), false);
                }
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        private static void DrawUnitFrame(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 skin, PixelColor32 accent, UnitClass unitClass, Subspecies species, int row, int frame, int stride)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;

            if (size >= 48)
            {
                DrawLargeUnitFrame(buffer, ox, oy, size, skin, accent, unitClass, species, row, frame, stride);
                return;
            }

            // --- 32x32 Chibi Heroic Unit Rendering ---
            int groundY = oy + size - 3;

            // 1. Drop Shadow
            buffer.FillRect(cx - 6, groundY - 1, 12, 3, new PixelColor32(15, 15, 20, 100));

            // 2. Armored / Leather Boots & Legs
            int bootY = groundY - 5;
            PixelColor32 bootDark = PixelColor32.LeatherDark;
            PixelColor32 bootLight = PixelColor32.LeatherLight;
            // Left Boot
            int lbX = cx - 4 + stride;
            buffer.FillRect(lbX, bootY, 3, 4, bootDark);
            buffer.SetPixel(lbX + 2, bootY + 2, bootLight);
            // Right Boot
            int rbX = cx + 1 - stride;
            buffer.FillRect(rbX, bootY, 3, 4, bootDark);
            buffer.SetPixel(rbX + 2, bootY + 2, bootLight);

            // 3. Torso, Tunic & Armor Layer
            int torsoY = cy + 1;
            int torsoW = 8;
            int torsoH = 7;
            // Tunic base (Class accent color with shading ramp)
            PixelColor32 tunicDark = accent.Darken(0.35f);
            PixelColor32 tunicMid = accent;
            PixelColor32 tunicLight = accent.Lighten(0.35f);

            buffer.FillRect(cx - 4, torsoY, torsoW, torsoH, tunicMid);
            buffer.FillRect(cx - 4, torsoY + torsoH - 2, torsoW, 2, tunicDark); // Tunic hem shadow
            buffer.DrawLine(cx - 4, torsoY, cx + 3, torsoY, tunicLight); // Shoulder highlight

            // Belt & Buckle
            buffer.DrawLine(cx - 4, torsoY + 4, cx + 3, torsoY + 4, PixelColor32.LeatherDark);
            buffer.FillRect(cx - 1, torsoY + 3, 2, 2, PixelColor32.GoldMid); // Brass buckle

            // Chestplate / Spaulders for melee & heavy units
            if (unitClass == UnitClass.Swordsman || unitClass == UnitClass.Banner || unitClass == UnitClass.Brawler)
            {
                buffer.FillRect(cx - 3, torsoY + 1, 6, 3, PixelColor32.SteelMid);
                buffer.SetPixel(cx - 2, torsoY + 1, PixelColor32.SteelGlint);
                buffer.DrawLine(cx - 3, torsoY + 3, cx + 2, torsoY + 3, PixelColor32.SteelDark);
            }

            // 4. Head, Face & Subspecies Features
            int headY = cy - 8;
            int headRadius = 4;
            PixelColor32 skinDark = skin.Darken(0.25f);
            PixelColor32 skinLight = skin.Lighten(0.25f);

            // Head sphere
            buffer.FillCircle(cx, headY, headRadius, skin);
            buffer.DrawLine(cx - 3, headY + 3, cx + 3, headY + 3, skinDark); // Jaw shadow

            // Subspecies Anatomy Overlays
            if (species == Subspecies.Swiftpaw)
            {
                // Long rabbit/hare ears
                buffer.FillRect(cx - 3, headY - 8, 2, 6, skin);
                buffer.SetPixel(cx - 2, headY - 6, PixelColor32.Crimson.Lighten(0.5f));
                buffer.FillRect(cx + 2, headY - 8, 2, 6, skin);
                buffer.SetPixel(cx + 3, headY - 6, PixelColor32.Crimson.Lighten(0.5f));
            }
            else if (species == Subspecies.Frogtide)
            {
                // Coral/Fin aquatic horns
                buffer.DrawLine(cx - 4, headY - 2, cx - 7, headY - 6, PixelColor32.SpiritCyan);
                buffer.DrawLine(cx + 4, headY - 2, cx + 7, headY - 6, PixelColor32.SpiritCyan);
            }
            else if (species == Subspecies.Ironwool)
            {
                // Fluffy wool collar & curly ram horns
                buffer.FillRect(cx - 5, torsoY - 2, 10, 3, PixelColor32.White);
                buffer.SetPixel(cx - 5, headY - 2, PixelColor32.WoodBrown);
                buffer.SetPixel(cx + 4, headY - 2, PixelColor32.WoodBrown);
            }
            else if (species == Subspecies.Colossus)
            {
                // Heavy obsidian brow and stone horns
                buffer.FillRect(cx - 4, headY - 3, 8, 2, PixelColor32.ObsidianSlate);
                buffer.DrawLine(cx - 4, headY - 4, cx - 6, headY - 7, PixelColor32.IronGrey);
                buffer.DrawLine(cx + 3, headY - 4, cx + 5, headY - 7, PixelColor32.IronGrey);
            }
            else if (species == Subspecies.Apex)
            {
                // Glowing celestial starlight halo
                buffer.DrawCircle(cx, headY - 6, 4, PixelColor32.StarGold, false);
                buffer.SetPixel(cx, headY - 7, PixelColor32.White);
            }

            // Moonlighter Iconic Eyes
            int eyeX = cx + 1;
            int eyeY = headY - 1;
            // Eye sclera
            buffer.FillRect(eyeX, eyeY, 3, 4, PixelColor32.White);
            // Pupil & Iris
            buffer.FillRect(eyeX + 1, eyeY + 1, 2, 3, PixelColor32.Black);
            // Catchlight glint
            buffer.SetPixel(eyeX + 1, eyeY + 1, PixelColor32.White);

            // 5. Class Headwear & Helmets
            DrawClassHeadwear(buffer, cx, headY, unitClass, accent);

            // 6. Class Weapons & Equipment Overlays
            DrawClassWeapons(buffer, cx, cy, unitClass, accent, row, frame);
        }

        private static void DrawLargeUnitFrame(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 skin, PixelColor32 accent, UnitClass unitClass, Subspecies species, int row, int frame, int stride)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;
            int groundY = oy + size - 4;

            // Large shadow
            buffer.FillRect(cx - 12, groundY - 2, 24, 4, new PixelColor32(15, 15, 20, 100));

            if (unitClass == UnitClass.Cavalry)
            {
                // Armored War Steed
                int horseY = cy + 2;
                PixelColor32 horseCoat = PixelColor32.LeatherMid;
                PixelColor32 horseDark = PixelColor32.LeatherDark;
                PixelColor32 horseArmor = PixelColor32.SteelMid;

                // Steed torso
                buffer.FillRect(cx - 12, horseY - 2, 22, 10, horseCoat);
                buffer.FillRect(cx - 12, horseY + 4, 22, 4, horseDark);
                // Saddle & Barding
                buffer.FillRect(cx - 4, horseY - 4, 10, 8, accent);
                buffer.FillRect(cx + 4, horseY - 2, 6, 6, horseArmor); // Chest barding
                // Steed Neck & Head
                buffer.FillRect(cx + 8, horseY - 8, 6, 8, horseCoat);
                buffer.FillRect(cx + 10, horseY - 12, 6, 6, horseCoat);
                buffer.FillRect(cx + 12, horseY - 12, 4, 5, horseArmor); // Chanfron
                buffer.SetPixel(cx + 11, horseY - 10, PixelColor32.White); // Eye
                // Horse legs with stride
                int legY = horseY + 8;
                buffer.FillRect(cx - 10 + stride * 2, legY, 3, 8, horseDark); // Back leg 1
                buffer.FillRect(cx - 6 - stride * 2, legY, 3, 8, horseCoat); // Back leg 2
                buffer.FillRect(cx + 4 - stride * 2, legY, 3, 8, horseDark); // Front leg 1
                buffer.FillRect(cx + 8 + stride * 2, legY, 3, 8, horseCoat); // Front leg 2

                // Mounted Rider
                int riderY = horseY - 14;
                buffer.FillRect(cx - 3, riderY, 7, 10, accent);
                buffer.FillRect(cx - 2, riderY + 1, 5, 5, PixelColor32.SteelMid); // Cuirass
                // Rider Head & Helm
                buffer.FillCircle(cx, riderY - 4, 4, skin);
                buffer.FillRect(cx - 3, riderY - 8, 7, 5, PixelColor32.SteelMid);
                buffer.DrawLine(cx - 1, riderY - 5, cx + 2, riderY - 5, PixelColor32.Black); // Visor
                buffer.DrawLine(cx - 1, riderY - 9, cx + 3, riderY - 12, PixelColor32.Crimson); // Plume

                // Heavy Charging Lance
                int lanceY = riderY + 4;
                int lanceLen = (row == 2) ? 24 : 18;
                buffer.DrawLine(cx - 4, lanceY, cx + lanceLen, lanceY - 2, PixelColor32.SteelLight);
                buffer.DrawLine(cx - 4, lanceY + 1, cx + lanceLen, lanceY - 1, PixelColor32.WoodBrown);
                buffer.SetPixel(cx + lanceLen, lanceY - 2, PixelColor32.SteelGlint);
                // Pennant
                buffer.FillRect(cx + 10, lanceY, 5, 3, accent);
            }
            else if (unitClass == UnitClass.Hammerer)
            {
                // Heavy Horned Brute
                int bodyY = cy - 2;
                // Heavy legs
                buffer.FillRect(cx - 7 + stride, groundY - 8, 5, 7, PixelColor32.SteelDark);
                buffer.FillRect(cx + 2 - stride, groundY - 8, 5, 7, PixelColor32.SteelDark);
                // Thick plate torso
                buffer.FillRect(cx - 8, bodyY, 16, 12, PixelColor32.SteelMid);
                buffer.FillRect(cx - 6, bodyY + 2, 12, 6, accent);
                buffer.DrawRectOutline(cx - 8, bodyY, 16, 12, PixelColor32.SteelDark);
                // Shoulder Pauldrons
                buffer.FillRect(cx - 11, bodyY - 2, 5, 6, PixelColor32.SteelLight);
                buffer.FillRect(cx + 6, bodyY - 2, 5, 6, PixelColor32.SteelLight);

                // Heavy Horned Helm
                int headY = bodyY - 10;
                buffer.FillCircle(cx, headY, 6, skin);
                buffer.FillRect(cx - 5, headY - 4, 10, 8, PixelColor32.SteelMid);
                buffer.DrawLine(cx - 3, headY, cx + 3, headY, PixelColor32.Black); // Visor
                // Giant iron horns
                buffer.DrawLine(cx - 5, headY - 2, cx - 10, headY - 8, PixelColor32.GoldMid);
                buffer.DrawLine(cx + 5, headY - 2, cx + 10, headY - 8, PixelColor32.GoldMid);

                // Massive Stone / Iron Warhammer
                int hamX = (row == 2) ? cx + 14 : cx - 12;
                int hamY = (row == 2) ? groundY - 10 : headY - 4;
                buffer.DrawLine(cx, bodyY + 4, hamX, hamY, PixelColor32.WoodBrown);
                buffer.FillRect(hamX - 4, hamY - 6, 8, 12, PixelColor32.ObsidianSlate);
                buffer.FillRect(hamX - 2, hamY - 4, 4, 8, PixelColor32.SteelMid);
                buffer.DrawRectOutline(hamX - 4, hamY - 6, 8, 12, PixelColor32.Black);
            }
            else if (unitClass == UnitClass.Skyrider)
            {
                // Aerial Glider Pilot
                int bodyY = cy;
                // Mechanical / Avian Glider Wings
                int wingSpread = (frame % 2 == 0) ? 14 : 10;
                buffer.DrawLine(cx - 4, bodyY - 4, cx - wingSpread - 6, bodyY - 10, PixelColor32.MithrilBlue);
                buffer.FillRect(cx - wingSpread - 6, bodyY - 8, wingSpread, 6, new PixelColor32(129, 212, 250, 200));
                buffer.DrawLine(cx + 4, bodyY - 4, cx + wingSpread + 6, bodyY - 10, PixelColor32.MithrilBlue);
                buffer.FillRect(cx + 6, bodyY - 8, wingSpread, 6, new PixelColor32(129, 212, 250, 200));

                // Pilot Body
                buffer.FillRect(cx - 4, bodyY, 8, 10, accent);
                buffer.FillRect(cx - 3, groundY - 6, 3, 5, PixelColor32.LeatherDark);
                buffer.FillRect(cx + 1, groundY - 6, 3, 5, PixelColor32.LeatherDark);

                // Aviator Helmet & Brass Goggles
                int headY = bodyY - 8;
                buffer.FillCircle(cx, headY, 5, skin);
                buffer.FillRect(cx - 4, headY - 4, 8, 4, PixelColor32.LeatherMid);
                // Goggles
                buffer.FillRect(cx - 2, headY - 2, 2, 2, PixelColor32.GoldMid);
                buffer.FillRect(cx + 1, headY - 2, 2, 2, PixelColor32.GoldMid);
                buffer.SetPixel(cx - 1, headY - 2, PixelColor32.SpiritCyan);
                buffer.SetPixel(cx + 2, headY - 2, PixelColor32.SpiritCyan);

                // Winged Sky Lance
                buffer.DrawLine(cx + 2, bodyY + 4, cx + 18, bodyY - 4, PixelColor32.MithrilBlue);
                buffer.SetPixel(cx + 18, bodyY - 4, PixelColor32.White);
            }
        }

        private static void DrawClassHeadwear(PixelBitmapBuffer buffer, int cx, int headY, UnitClass unitClass, PixelColor32 accent)
        {
            switch (unitClass)
            {
                case UnitClass.Banner:
                    // Golden Winged Commander Helm
                    buffer.FillRect(cx - 3, headY - 6, 6, 3, PixelColor32.GoldMid);
                    buffer.DrawLine(cx - 4, headY - 7, cx - 6, headY - 10, PixelColor32.GoldLight);
                    buffer.DrawLine(cx + 3, headY - 7, cx + 5, headY - 10, PixelColor32.GoldLight);
                    buffer.DrawLine(cx - 1, headY - 6, cx - 1, headY - 10, PixelColor32.Crimson); // Plume
                    break;

                case UnitClass.Spearman:
                    // Conical Skirmisher Helm with Red Feather
                    buffer.FillRect(cx - 3, headY - 5, 6, 2, PixelColor32.SteelMid);
                    buffer.SetPixel(cx, headY - 6, PixelColor32.SteelLight);
                    buffer.DrawLine(cx - 1, headY - 6, cx - 3, headY - 10, PixelColor32.Crimson); // Feather quill
                    break;

                case UnitClass.Swordsman:
                    // Heavy Knight Visor Helm
                    buffer.FillRect(cx - 4, headY - 5, 8, 5, PixelColor32.SteelMid);
                    buffer.DrawLine(cx - 3, headY - 5, cx + 2, headY - 5, PixelColor32.SteelGlint);
                    buffer.DrawLine(cx - 2, headY - 2, cx + 2, headY - 2, PixelColor32.Black); // Visor slit
                    break;

                case UnitClass.Archer:
                    // Ranger Cowl Hood with Emerald Feather
                    buffer.FillRect(cx - 4, headY - 6, 8, 4, PixelColor32.VerdantMoss);
                    buffer.DrawLine(cx - 4, headY - 6, cx - 4, headY - 1, PixelColor32.VerdantMoss.Darken(0.3f));
                    buffer.DrawLine(cx + 2, headY - 6, cx + 4, headY - 10, PixelColor32.VerdantMoss.Lighten(0.4f)); // Feather
                    break;

                case UnitClass.Hornist:
                    // Minstrel Velvet Beret
                    buffer.FillRect(cx - 4, headY - 6, 9, 3, accent);
                    buffer.DrawLine(cx + 2, headY - 6, cx + 6, headY - 9, PixelColor32.White); // Plume
                    break;

                case UnitClass.Mage:
                    // Pointed Wizard Hat with Moon Brooch
                    buffer.FillRect(cx - 5, headY - 4, 10, 2, PixelColor32.FromHex("#7B1FA2")); // Brim
                    buffer.FillRect(cx - 3, headY - 7, 6, 3, PixelColor32.FromHex("#6A1B9A"));
                    buffer.FillRect(cx - 1, headY - 10, 3, 3, PixelColor32.FromHex("#4A148C"));
                    buffer.SetPixel(cx - 1, headY - 4, PixelColor32.GoldLight); // Star/Moon buckle
                    break;

                case UnitClass.Brawler:
                    // Spiked Leather Headband
                    buffer.FillRect(cx - 4, headY - 4, 8, 2, PixelColor32.LeatherDark);
                    buffer.SetPixel(cx - 2, headY - 5, PixelColor32.SteelLight);
                    buffer.SetPixel(cx + 1, headY - 5, PixelColor32.SteelLight);
                    break;
            }
        }

        private static void DrawClassWeapons(PixelBitmapBuffer buffer, int cx, int cy, UnitClass unitClass, PixelColor32 accent, int row, int frame)
        {
            switch (unitClass)
            {
                case UnitClass.Banner:
                    // Tall Ceremonial Royal Banner
                    int poleX = cx - 5;
                    buffer.DrawLine(poleX, cy + 8, poleX, cy - 14, PixelColor32.WoodBrown);
                    buffer.SetPixel(poleX, cy - 14, PixelColor32.GoldMid); // Spearhead finial
                    // Billowing Flag Cloth (wave animation)
                    int wave = (frame % 2 == 0) ? 0 : 1;
                    buffer.FillRect(poleX + 1, cy - 13 + wave, 7, 6, accent);
                    buffer.SetPixel(poleX + 3, cy - 10 + wave, PixelColor32.GoldLight); // Royal Emblem
                    break;

                case UnitClass.Spearman:
                    // Ashwood Javelin / Spear
                    int spearX = cx + 3;
                    int spearY = cy + 2;
                    int spearLen = (row == 2) ? 14 : 10;
                    buffer.DrawLine(spearX - 2, spearY + 4, spearX + spearLen, spearY - spearLen / 2, PixelColor32.WoodBrown);
                    buffer.FillRect(spearX + spearLen, spearY - spearLen / 2 - 1, 3, 2, PixelColor32.SteelLight);
                    buffer.SetPixel(spearX + spearLen + 2, spearY - spearLen / 2, PixelColor32.SteelGlint);
                    break;

                case UnitClass.Swordsman:
                    // Kite Shield on Left
                    int shdX = cx - 7;
                    int shdY = cy + 1;
                    buffer.FillRect(shdX, shdY, 4, 7, PixelColor32.SteelMid);
                    buffer.DrawRectOutline(shdX, shdY, 4, 7, PixelColor32.GoldMid);
                    buffer.SetPixel(shdX + 1, shdY + 3, PixelColor32.SteelGlint); // Boss

                    // Broadsword on Right
                    int swdX = (row == 2) ? cx + 6 : cx + 4;
                    int swdY = (row == 2) ? cy - 2 : cy + 3;
                    buffer.DrawLine(swdX, swdY + 2, swdX + 5, swdY - 5, PixelColor32.SteelLight);
                    buffer.DrawLine(swdX - 1, swdY + 2, swdX + 2, swdY + 2, PixelColor32.GoldMid); // Crossguard
                    break;

                case UnitClass.Archer:
                    // Recurve Longbow & Quiver
                    int bowX = cx + 4;
                    int bowY = cy - 2;
                    buffer.DrawLine(bowX, bowY - 5, bowX + 3, bowY, PixelColor32.WoodBrown);
                    buffer.DrawLine(bowX + 3, bowY, bowX, bowY + 5, PixelColor32.WoodBrown);
                    buffer.DrawLine(bowX, bowY - 5, bowX, bowY + 5, PixelColor32.White); // Bowstring
                    // Quiver on back
                    buffer.FillRect(cx - 6, cy - 3, 2, 6, PixelColor32.LeatherDark);
                    buffer.SetPixel(cx - 6, cy - 4, PixelColor32.VerdantMoss); // Feather fletchings
                    break;

                case UnitClass.Hornist:
                    // Polished Brass War Horn
                    int hornX = cx + 2;
                    int hornY = cy + 1;
                    buffer.DrawLine(hornX, hornY + 2, hornX + 4, hornY - 1, PixelColor32.GoldDark);
                    buffer.FillRect(hornX + 4, hornY - 3, 3, 5, PixelColor32.GoldLight); // Flared bell
                    buffer.SetPixel(hornX + 6, hornY - 1, PixelColor32.White);
                    break;

                case UnitClass.Mage:
                    // Arcane Twisted Staff with Glowing Crystal Orb
                    int stfX = cx + 4;
                    int stfY = cy - 6;
                    buffer.DrawLine(stfX, cy + 6, stfX, stfY, PixelColor32.WoodBrown);
                    // Glowing Arcane Orb
                    buffer.FillCircle(stfX, stfY - 2, 2, PixelColor32.SpiritCyan);
                    buffer.SetPixel(stfX, stfY - 2, PixelColor32.White);
                    break;

                case UnitClass.Brawler:
                    // Dual Spiked Demolition Knuckle Gauntlets
                    int gntX = (row == 2) ? cx + 6 : cx + 4;
                    int gntY = cy + 3;
                    buffer.FillRect(gntX, gntY, 4, 4, PixelColor32.SteelDark);
                    buffer.SetPixel(gntX + 1, gntY - 1, PixelColor32.SteelLight); // Spike 1
                    buffer.SetPixel(gntX + 3, gntY - 1, PixelColor32.SteelLight); // Spike 2
                    break;
            }
        }

        private static void DrawWeaponSlashFX(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 color, int frame)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;
            if (frame == 1) // Strike slash arc
            {
                buffer.DrawLine(cx + 4, cy + 6, cx + 13, cy - 3, color);
                buffer.DrawLine(cx + 5, cy + 6, cx + 14, cy - 3, PixelColor32.White);
                buffer.DrawLine(cx + 6, cy + 5, cx + 13, cy - 2, color.Lighten(0.4f));
            }
            else if (frame == 2) // Impact star burst
            {
                buffer.SetPixel(cx + 12, cy + 2, PixelColor32.FeverYellow);
                buffer.SetPixel(cx + 13, cy + 2, PixelColor32.White);
                buffer.SetPixel(cx + 12, cy + 1, PixelColor32.White);
                buffer.SetPixel(cx + 14, cy + 3, PixelColor32.AmberGold);
            }
        }

        /// <summary>
        /// Generates enemy and colossal boss spritesheets with custom scale and telegraph glow.
        /// </summary>
        public static PixelBitmapBuffer GenerateEnemySpriteSheet(EnemyKind kind)
        {
            int size = 32;
            if (kind == EnemyKind.IronBehemoth || kind == EnemyKind.DrakeTitan || kind == EnemyKind.ColossusGolem) size = 96;
            else if (kind == EnemyKind.WildBoar || kind == EnemyKind.GiantBoar || kind == EnemyKind.Stag || kind == EnemyKind.StoneWall || kind == EnemyKind.Watchtower || kind == EnemyKind.CatapultTower) size = 64;
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
                        DrawEnemyBody(buffer, ox, oy, size, baseColor, accentColor, kind, 0, 0);
                    }
                    else if (row == 1) // March / Patrol
                    {
                        int stride = (f == 0 || f == 2) ? 2 : 0;
                        DrawEnemyBody(buffer, ox, oy, size, baseColor, accentColor, kind, stride, 1);
                    }
                    else if (row == 2) // Attack
                    {
                        int lunge = (f == 1) ? -4 : (f == 2 ? -2 : 0);
                        DrawEnemyBody(buffer, ox + lunge, oy, size, baseColor, accentColor, kind, 0, 2);
                    }
                    else if (row == 3) // Telegraph Warning Glow
                    {
                        DrawEnemyBody(buffer, ox, oy, size, PixelColor32.Crimson, PixelColor32.AmberGold, kind, 0, 3);
                        int cx = ox + size / 2;
                        int cy = oy + size / 2;
                        buffer.DrawCircle(cx, cy, size / 2 - 2, PixelColor32.Crimson, false);
                        buffer.DrawCircle(cx, cy, size / 2 - 4, new PixelColor32(255, 23, 68, 140), false);
                    }
                    else if (row == 4) // Hurt / Stagger
                    {
                        DrawEnemyBody(buffer, ox + 3, oy, size, PixelColor32.White, accentColor, kind, 0, 4);
                    }
                    else if (row == 5) // Death
                    {
                        DrawEnemyBody(buffer, ox, oy - 2, size, PixelColor32.ObsidianSlate, accentColor, kind, 0, 5);
                    }
                }
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        private static void DrawEnemyBody(PixelBitmapBuffer buffer, int ox, int oy, int size, PixelColor32 baseColor, PixelColor32 accent, EnemyKind kind, int stride, int animState)
        {
            int cx = ox + size / 2;
            int cy = oy + size / 2;

            if (size == 96)
            {
                DrawColossalBoss(buffer, ox, oy, cx, cy, baseColor, accent, kind, animState);
                return;
            }

            if (size == 64)
            {
                DrawMediumMonster(buffer, ox, oy, cx, cy, baseColor, accent, kind, stride, animState);
                return;
            }

            // --- 32x32 / 48x48 Enemy Character Rendering ---
            int groundY = oy + size - 3;
            buffer.FillRect(cx - 6, groundY - 1, 12, 3, new PixelColor32(15, 15, 20, 100)); // Shadow

            if (kind == EnemyKind.PlainsRunner)
            {
                // Feathered Ostro-Runner Bird
                int birdY = cy;
                buffer.FillCircle(cx, birdY, 6, baseColor);
                buffer.FillRect(cx - 8, birdY - 3, 6, 4, baseColor.Darken(0.2f)); // Tail feathers
                // Running bird legs
                buffer.DrawLine(cx - 3 + stride, groundY, cx - 2, birdY + 4, PixelColor32.LeatherDark);
                buffer.DrawLine(cx + 2 - stride, groundY, cx + 1, birdY + 4, PixelColor32.LeatherDark);
                // Head & Beak
                buffer.FillCircle(cx + 4, birdY - 5, 4, baseColor);
                buffer.FillRect(cx + 7, birdY - 5, 4, 3, PixelColor32.AmberGold); // Beak
                buffer.SetPixel(cx + 5, birdY - 6, PixelColor32.Black); // Eye
                buffer.SetPixel(cx + 5, birdY - 6, PixelColor32.White);
                return;
            }

            // Shadowmask Clan Warriors (Tribe Units)
            int bodyY = cy + 1;
            // Boots & legs
            buffer.FillRect(cx - 4 + stride, groundY - 5, 3, 4, PixelColor32.LeatherDark);
            buffer.FillRect(cx + 1 - stride, groundY - 5, 3, 4, PixelColor32.LeatherDark);

            // Dark Tribal Tunic
            buffer.FillRect(cx - 4, bodyY, 8, 7, baseColor);
            buffer.FillRect(cx - 4, bodyY + 5, 8, 2, baseColor.Darken(0.35f));

            // Shadowmask Bone Mask & Glowing Crimson Eyes
            int headY = cy - 7;
            buffer.FillCircle(cx, headY, 5, PixelColor32.ObsidianSlate);
            // Tribal Mask Plate
            buffer.FillRect(cx - 3, headY - 4, 6, 6, new PixelColor32(230, 220, 200, 255)); // Bone mask
            buffer.DrawLine(cx - 2, headY - 2, cx - 1, headY - 2, PixelColor32.Crimson); // Left eye slit
            buffer.DrawLine(cx + 1, headY - 2, cx + 2, headY - 2, PixelColor32.Crimson); // Right eye slit
            buffer.SetPixel(cx - 1, headY - 2, PixelColor32.White); // Eye glint
            buffer.SetPixel(cx + 2, headY - 2, PixelColor32.White);
            // Dark War Plume
            buffer.DrawLine(cx - 1, headY - 5, cx - 2, headY - 9, accent);

            // Tribal Jagged Weapon
            int wpnX = cx + 4;
            int wpnY = cy;
            buffer.DrawLine(wpnX, wpnY + 4, wpnX + 6, wpnY - 6, PixelColor32.IronGrey);
            buffer.SetPixel(wpnX + 6, wpnY - 6, PixelColor32.Crimson);
        }

        private static void DrawMediumMonster(PixelBitmapBuffer buffer, int ox, int oy, int cx, int cy, PixelColor32 baseColor, PixelColor32 accent, EnemyKind kind, int stride, int animState)
        {
            int groundY = oy + 60;
            buffer.FillRect(cx - 16, groundY - 2, 32, 4, new PixelColor32(15, 15, 20, 110)); // Shadow

            if (kind == EnemyKind.WildBoar || kind == EnemyKind.GiantBoar)
            {
                // Heavy Bristly Boar
                int boarY = cy + 4;
                PixelColor32 furDark = baseColor.Darken(0.35f);
                PixelColor32 furLight = baseColor.Lighten(0.3f);

                // Body barrel
                buffer.FillRect(cx - 16, boarY - 10, 28, 18, baseColor);
                buffer.FillRect(cx - 16, boarY + 2, 28, 6, furDark);
                // Bristly dorsal ridge
                buffer.DrawLine(cx - 14, boarY - 11, cx + 8, boarY - 11, furLight);

                // Boar Head & Snout
                buffer.FillRect(cx + 8, boarY - 8, 12, 14, baseColor);
                buffer.FillRect(cx + 16, boarY - 4, 6, 8, furDark); // Snout
                buffer.SetPixel(cx + 13, boarY - 6, (kind == EnemyKind.GiantBoar) ? PixelColor32.Crimson : PixelColor32.White); // Eye

                // Massive White Curved Tusks
                buffer.DrawLine(cx + 14, boarY + 4, cx + 18, boarY - 2, PixelColor32.White);
                buffer.DrawLine(cx + 15, boarY + 4, cx + 19, boarY - 2, PixelColor32.White);
                if (kind == EnemyKind.GiantBoar)
                {
                    // Armored plates & Spikes
                    buffer.FillRect(cx - 10, boarY - 12, 16, 4, PixelColor32.SteelMid);
                    buffer.SetPixel(cx - 4, boarY - 14, PixelColor32.GoldMid);
                }

                // 4 Hoofed Legs
                buffer.FillRect(cx - 14 + stride * 2, groundY - 8, 5, 8, furDark);
                buffer.FillRect(cx - 6 - stride * 2, groundY - 8, 5, 8, baseColor);
                buffer.FillRect(cx + 4 - stride * 2, groundY - 8, 5, 8, furDark);
                buffer.FillRect(cx + 12 + stride * 2, groundY - 8, 5, 8, baseColor);
            }
            else if (kind == EnemyKind.Stag)
            {
                // Majestic Forest Stag
                int stagY = cy + 2;
                buffer.FillRect(cx - 14, stagY - 8, 24, 14, baseColor);
                // Slender hooves
                buffer.FillRect(cx - 12 + stride, groundY - 12, 3, 12, PixelColor32.LeatherDark);
                buffer.FillRect(cx + 6 - stride, groundY - 12, 3, 12, PixelColor32.LeatherDark);
                // Long Arched Neck & Head
                buffer.DrawLine(cx + 8, stagY - 4, cx + 14, stagY - 16, baseColor);
                buffer.FillCircle(cx + 14, stagY - 16, 4, baseColor);
                buffer.SetPixel(cx + 15, stagY - 17, PixelColor32.White);
                // Multi-Branched Antlers
                buffer.DrawLine(cx + 14, stagY - 18, cx + 12, stagY - 26, PixelColor32.WoodBrown);
                buffer.DrawLine(cx + 12, stagY - 22, cx + 8, stagY - 24, PixelColor32.WoodBrown);
                buffer.DrawLine(cx + 14, stagY - 18, cx + 18, stagY - 26, PixelColor32.WoodBrown);
                buffer.DrawLine(cx + 18, stagY - 22, cx + 22, stagY - 24, PixelColor32.WoodBrown);
            }
            else
            {
                // Fortifications: StoneWall, Watchtower, CatapultTower
                buffer.FillRect(cx - 18, groundY - 45, 36, 45, PixelColor32.ObsidianSlate);
                buffer.FillRect(cx - 16, groundY - 43, 32, 41, PixelColor32.IronGrey);
                // Masonry Bricks & Battlements
                buffer.DrawLine(cx - 18, groundY - 45, cx + 18, groundY - 45, PixelColor32.SteelLight);
                buffer.FillRect(cx - 18, groundY - 50, 8, 6, PixelColor32.ObsidianSlate);
                buffer.FillRect(cx - 4, groundY - 50, 8, 6, PixelColor32.ObsidianSlate);
                buffer.FillRect(cx + 10, groundY - 50, 8, 6, PixelColor32.ObsidianSlate);
                // Arrow Slit
                buffer.FillRect(cx - 2, groundY - 25, 4, 10, PixelColor32.Black);
            }
        }

        private static void DrawColossalBoss(PixelBitmapBuffer buffer, int ox, int oy, int cx, int cy, PixelColor32 baseColor, PixelColor32 accent, EnemyKind kind, int animState)
        {
            int groundY = oy + 90;
            buffer.FillRect(cx - 28, groundY - 3, 56, 6, new PixelColor32(15, 15, 20, 130)); // Giant Shadow

            if (kind == EnemyKind.IronBehemoth)
            {
                // Massive Quadruped Iron Juggernaut Tank Beast
                int tankY = cy + 6;
                // Heavy riveted iron armor body
                buffer.FillRect(cx - 26, tankY - 18, 52, 28, PixelColor32.ObsidianSlate);
                buffer.FillRect(cx - 24, tankY - 16, 48, 24, PixelColor32.SteelDark);
                buffer.DrawRectOutline(cx - 26, tankY - 18, 52, 28, PixelColor32.Black);

                // Glowing Molten Firebox Furnace
                buffer.FillRect(cx - 8, tankY - 2, 16, 10, PixelColor32.Crimson);
                buffer.FillRect(cx - 6, tankY, 12, 6, PixelColor32.AmberGold);
                buffer.SetPixel(cx, tankY + 2, PixelColor32.White);

                // Steam Exhaust Smokestacks on Back
                buffer.FillRect(cx - 18, tankY - 28, 6, 12, PixelColor32.SteelMid);
                buffer.FillRect(cx - 8, tankY - 28, 6, 12, PixelColor32.SteelMid);
                // Steam clouds
                buffer.FillCircle(cx - 15, tankY - 32, 4, new PixelColor32(230, 235, 240, 180));
                buffer.FillCircle(cx - 5, tankY - 32, 4, new PixelColor32(230, 235, 240, 180));

                // Battering Ram Head & Glowing Ruby Visor
                buffer.FillRect(cx + 18, tankY - 14, 16, 20, PixelColor32.SteelMid);
                buffer.FillRect(cx + 24, tankY - 8, 6, 4, PixelColor32.DragonRuby); // Eye slit
                buffer.SetPixel(cx + 26, tankY - 7, PixelColor32.White);
                // Massive Ram Horns
                buffer.DrawLine(cx + 24, tankY - 14, cx + 34, tankY - 24, PixelColor32.GoldMid);
                buffer.DrawLine(cx + 26, tankY - 14, cx + 36, tankY - 24, PixelColor32.GoldLight);

                // 4 Hydraulic Piston Legs
                buffer.FillRect(cx - 24, groundY - 12, 10, 12, PixelColor32.SteelDark);
                buffer.FillRect(cx - 10, groundY - 12, 10, 12, PixelColor32.ObsidianSlate);
                buffer.FillRect(cx + 6, groundY - 12, 10, 12, PixelColor32.SteelDark);
                buffer.FillRect(cx + 18, groundY - 12, 10, 12, PixelColor32.ObsidianSlate);
            }
            else if (kind == EnemyKind.DrakeTitan)
            {
                // Colossal Ancient Red Dragon
                int dragY = cy + 4;
                PixelColor32 dragRed = PixelColor32.Crimson;
                PixelColor32 dragDark = dragRed.Darken(0.4f);
                PixelColor32 dragGold = PixelColor32.AmberGold;

                // Scaled Muscular Body
                buffer.FillRect(cx - 24, dragY - 14, 44, 24, dragRed);
                buffer.FillRect(cx - 24, dragY + 4, 44, 6, dragDark);
                // Molten belly scales
                buffer.FillRect(cx - 10, dragY, 20, 8, dragGold);
                buffer.SetPixel(cx, dragY + 3, PixelColor32.White);

                // Giant Ribbed Dragon Wings
                int wingY = dragY - 32;
                buffer.DrawLine(cx - 14, dragY - 10, cx - 28, wingY, dragDark);
                buffer.DrawLine(cx - 28, wingY, cx + 8, wingY + 4, dragDark);
                buffer.FillRect(cx - 26, wingY + 4, 30, 14, new PixelColor32(239, 83, 80, 200));

                // Dragon Skull, Horns & Glowing Eye
                int skullX = cx + 16;
                int skullY = dragY - 18;
                buffer.FillRect(skullX, skullY, 18, 16, dragRed);
                buffer.FillRect(skullX + 10, skullY + 8, 8, 6, dragDark); // Jaw
                buffer.FillRect(skullX + 6, skullY + 4, 4, 3, PixelColor32.StarGold); // Eye
                buffer.SetPixel(skullX + 8, skullY + 5, PixelColor32.White);
                // Curved Dragon Horns
                buffer.DrawLine(skullX + 4, skullY, skullX - 4, skullY - 12, dragGold);
                buffer.DrawLine(skullX + 8, skullY, skullX, skullY - 12, dragGold);

                // Spined Tail & Talons
                buffer.DrawLine(cx - 24, dragY + 4, cx - 40, dragY - 4, dragRed);
                buffer.FillRect(cx - 16, groundY - 10, 8, 10, dragDark);
                buffer.FillRect(cx + 8, groundY - 10, 8, 10, dragDark);
            }
            else if (kind == EnemyKind.ColossusGolem)
            {
                // Ancient Monumental Stone Titan
                int golemY = cy + 2;
                PixelColor32 stone = PixelColor32.IronGrey;
                PixelColor32 stoneDark = PixelColor32.ObsidianSlate;
                PixelColor32 runeCyan = PixelColor32.SpiritCyan;

                // Weathered Stone Torso
                buffer.FillRect(cx - 22, golemY - 18, 44, 30, stone);
                buffer.FillRect(cx - 22, golemY + 6, 44, 6, stoneDark);
                buffer.DrawRectOutline(cx - 22, golemY - 18, 44, 30, PixelColor32.Black);

                // Ancient Inscribed Glowing Glyphs
                buffer.DrawLine(cx - 10, golemY - 6, cx + 10, golemY - 6, runeCyan);
                buffer.DrawLine(cx, golemY - 12, cx, golemY + 2, runeCyan);
                buffer.FillCircle(cx, golemY - 6, 3, runeCyan);
                buffer.SetPixel(cx, golemY - 6, PixelColor32.White);

                // Boulder Shoulders with Moss / Vines
                buffer.FillCircle(cx - 24, golemY - 14, 10, stone);
                buffer.FillRect(cx - 30, golemY - 20, 12, 4, PixelColor32.VerdantMoss); // Moss
                buffer.FillCircle(cx + 24, golemY - 14, 10, stone);
                buffer.FillRect(cx + 18, golemY - 20, 12, 4, PixelColor32.VerdantMoss); // Moss

                // Monolith Head & Single Ancient Eye Core
                buffer.FillRect(cx - 8, golemY - 32, 16, 14, stone);
                buffer.FillCircle(cx, golemY - 25, 4, runeCyan);
                buffer.SetPixel(cx, golemY - 25, PixelColor32.White);

                // Massive Stone Pillar Legs
                buffer.FillRect(cx - 18, groundY - 14, 12, 14, stoneDark);
                buffer.FillRect(cx + 6, groundY - 14, 12, 14, stoneDark);
            }
        }

        /// <summary>
        /// Generates a 24x24 px pixel icon for weapons, armors, materials, or charms.
        /// </summary>
        /// <summary>
        /// Generates a transparent equipment overlay at the same frame size as the unit.
        /// Equipment is intentionally authored as a separate silhouette layer so changing
        /// gear changes the visible character rather than only changing combat stats.
        /// </summary>
        public static PixelBitmapBuffer GenerateEquipmentOverlay(string itemId, int size)
        {
            var buffer = new PixelBitmapBuffer(size, size);
            var spec = EquipmentVisualRegistry.Get(itemId);
            if (spec == null) return buffer;

            float scale = size / 32f;
            int cx = size / 2;
            int anchorX = (int)(spec.AnchorOffset.X * scale);
            int anchorY = (int)(spec.AnchorOffset.Y * scale);

            PixelColor32 dark = PixelColor32.Black;
            PixelColor32 steel = PixelColor32.SteelMid;
            PixelColor32 steelLight = PixelColor32.SteelLight;
            PixelColor32 wood = PixelColor32.WoodBrown;
            PixelColor32 leather = PixelColor32.LeatherMid;
            PixelColor32 gold = PixelColor32.GoldMid;
            PixelColor32 crimson = PixelColor32.Crimson;
            PixelColor32 cyan = PixelColor32.SpiritCyan;

            int S(float value) { return Math.Max(1, (int)Math.Round(value * scale)); }
            void Rect(int x, int y, int w, int h, PixelColor32 color)
            {
                buffer.FillRect((int)(x * scale) + anchorX, (int)(y * scale) + anchorY,
                    S(w), S(h), color);
            }

            switch (spec.Family)
            {
                case "spear":
                    int shaftX = (int)(21 * scale) + anchorX;
                    int shaftY = (int)(21 * scale) + anchorY;
                    int tipX = (int)(29 * scale) + anchorX;
                    int tipY = (int)(8 * scale) + anchorY;
                    buffer.DrawLine(shaftX, shaftY, tipX, tipY, spec.Material == "wood" ? wood : steel);
                    if (spec.Silhouette == "forked_thunder")
                    {
                        buffer.DrawLine(tipX, tipY, tipX - S(2), tipY - S(4), cyan);
                        buffer.DrawLine(tipX, tipY, tipX + S(2), tipY - S(4), cyan);
                        buffer.SetPixel(tipX, tipY, PixelColor32.White);
                    }
                    else if (spec.Silhouette == "leaf_blade")
                    {
                        buffer.FillRect(tipX - S(1), tipY - S(3), S(3), S(5), steelLight);
                        buffer.SetPixel(tipX, tipY - S(4), PixelColor32.White);
                    }
                    else
                    {
                        buffer.FillRect(tipX - S(1), tipY - S(2), S(3), S(3), steelLight);
                    }
                    break;

                case "shield":
                    int sx = (int)(4 * scale) + anchorX;
                    int sy = (int)(13 * scale) + anchorY;
                    int sw = spec.Silhouette == "tower_bastion" ? 8 : 7;
                    int sh = spec.Silhouette == "tower_bastion" ? 13 : 9;
                    buffer.FillRect(sx, sy, S(sw), S(sh), spec.Material == "wood" ? wood : steel);
                    buffer.DrawRectOutline(sx, sy, S(sw), S(sh), dark);
                    buffer.DrawRectOutline(sx + S(1), sy + S(1), Math.Max(1, S(sw - 2)), Math.Max(1, S(sh - 2)), spec.Material == "wood" ? leather : gold);
                    if (spec.Silhouette == "tower_bastion")
                        buffer.FillRect(sx + S(2), sy + S(4), S(3), S(4), gold);
                    else
                        buffer.SetPixel(sx + S(3), sy + S(4), steelLight);
                    break;

                case "helmet":
                    int hy = (int)(4 * scale) + anchorY;
                    if (spec.Silhouette == "closed_vanguard")
                    {
                        Rect(11, 4, 10, 7, steel);
                        Rect(13, 8, 6, 2, dark);
                        Rect(12, 3, 8, 2, steelLight);
                    }
                    else
                    {
                        Rect(10, 5, 12, 4, leather);
                        Rect(12, 3, 8, 2, leather);
                        Rect(19, 2, 2, 5, gold);
                    }
                    break;

                case "mask":
                    int my = (int)(12 * scale) + anchorY;
                    Rect(10, 12, 12, 8, spec.Material == "steel" ? steel : dark);
                    if (spec.Silhouette == "lion_face")
                    {
                        Rect(8, 13, 3, 4, gold);
                        Rect(21, 13, 3, 4, gold);
                        Rect(14, 15, 4, 2, gold);
                    }
                    else if (spec.Silhouette == "war_face")
                    {
                        Rect(12, 14, 2, 2, crimson);
                        Rect(18, 14, 2, 2, crimson);
                        Rect(15, 17, 3, 2, steelLight);
                    }
                    else if (spec.Silhouette == "horned_face")
                    {
                        buffer.DrawLine((int)(11 * scale) + anchorX, my,
                            (int)(7 * scale) + anchorX, (int)(8 * scale) + anchorY, crimson);
                        buffer.DrawLine((int)(21 * scale) + anchorX, my,
                            (int)(25 * scale) + anchorX, (int)(8 * scale) + anchorY, crimson);
                    }
                    else
                    {
                        Rect(12, 10, 2, 4, gold);
                        Rect(18, 10, 2, 4, gold);
                        Rect(14, 15, 4, 2, gold);
                        buffer.SetPixel((int)(16 * scale) + anchorX, (int)(12 * scale) + anchorY, PixelColor32.White);
                    }
                    break;

                case "relic":
                    int rx = (int)(23 * scale) + anchorX;
                    int ry = (int)(17 * scale) + anchorY;
                    if (spec.Silhouette == "water_charm")
                    {
                        buffer.FillCircle(rx, ry, S(3), cyan, true);
                        buffer.SetPixel(rx, ry - S(1), PixelColor32.White);
                    }
                    else if (spec.Silhouette == "wind_talisman")
                    {
                        buffer.DrawLine(rx, ry + S(4), rx + S(2), ry - S(3), cyan);
                        buffer.DrawLine(rx + S(2), ry - S(3), rx + S(5), ry - S(1), PixelColor32.White);
                    }
                    else if (spec.Silhouette == "earth_totem")
                    {
                        buffer.FillRect(rx - S(2), ry - S(4), S(4), S(8), leather);
                        buffer.FillRect(rx - S(3), ry + S(2), S(6), S(2), gold);
                    }
                    else
                    {
                        buffer.FillCircle(rx, ry, S(3), cyan, true);
                        buffer.DrawCircle(rx, ry, S(4), cyan, false);
                        buffer.SetPixel(rx, ry, PixelColor32.White);
                    }
                    break;
            }

            buffer.ApplyDarkOutline(dark);
            return buffer;
        }

        public static PixelBitmapBuffer GenerateItemIcon(string itemId)
        {
            var buffer = new PixelBitmapBuffer(24, 24);
            // Engraved parchment / slate item frame
            buffer.FillRect(0, 0, 24, 24, PixelColor32.FromHex("#1A202C"));
            buffer.DrawRectOutline(0, 0, 24, 24, PixelColor32.FromHex("#37474F"));
            buffer.FillRect(2, 2, 20, 20, PixelColor32.FromHex("#263238"));

            if (itemId.StartsWith("wpn-") || itemId.StartsWith("spear-") || itemId.StartsWith("sword-") || itemId.StartsWith("bow-") || itemId.StartsWith("hammer-") || itemId.StartsWith("horn-") || itemId.StartsWith("staff-") || itemId.StartsWith("fist-"))
            {
                // Gleaming Weapon Blade & Hilt
                buffer.DrawLine(4, 20, 19, 5, PixelColor32.SteelLight);
                buffer.DrawLine(5, 20, 20, 5, PixelColor32.White);
                buffer.DrawLine(3, 19, 7, 23, PixelColor32.WoodBrown); // Crossguard
                buffer.SetPixel(19, 4, PixelColor32.GoldMid); // Gem tip
                buffer.SetPixel(20, 4, PixelColor32.White);
            }
            else if (itemId.StartsWith("shd-") || itemId.StartsWith("shield-"))
            {
                // Heraldic Kite Shield Crest
                buffer.FillRect(5, 4, 14, 15, PixelColor32.SteelMid);
                buffer.DrawRectOutline(5, 4, 14, 15, PixelColor32.GoldMid);
                buffer.FillCircle(12, 11, 3, PixelColor32.SpiritCyan);
                buffer.SetPixel(12, 11, PixelColor32.White);
            }
            else if (itemId.StartsWith("hlm-") || itemId.StartsWith("helm-") || itemId.StartsWith("hero-mask-"))
            {
                // Royal Knight Helmet / Relic Mask
                buffer.FillRect(5, 6, 14, 12, PixelColor32.SteelMid);
                buffer.DrawLine(6, 12, 17, 12, PixelColor32.Black); // Visor slit
                buffer.DrawLine(11, 2, 13, 6, PixelColor32.Crimson); // Plume
                buffer.SetPixel(12, 2, PixelColor32.GoldMid);
            }
            else if (itemId.StartsWith("gem-"))
            {
                // Faceted Elemental Gem Jewel
                PixelColor32 gemCol = itemId.Contains("flame") ? PixelColor32.Crimson :
                                      itemId.Contains("frost") ? PixelColor32.SpiritCyan :
                                      itemId.Contains("lightning") ? PixelColor32.AmberGold : PixelColor32.VerdantMoss;
                buffer.FillCircle(12, 12, 6, gemCol);
                buffer.DrawCircle(12, 12, 6, PixelColor32.White, false);
                buffer.SetPixel(10, 9, PixelColor32.White); // Specular glint
            }
            else if (itemId.StartsWith("charm-") || itemId.StartsWith("food-"))
            {
                // Glass Alchemy Flask / Feast Bowl
                buffer.FillCircle(12, 14, 6, PixelColor32.Crimson);
                buffer.FillRect(10, 5, 4, 5, PixelColor32.WoodBrown); // Cork neck
                buffer.SetPixel(10, 12, PixelColor32.White); // Glass highlight
            }
            else
            {
                // Raw Crafting Material (Ore, timber, hide, alloy)
                buffer.FillRect(6, 7, 12, 10, PixelColor32.WoodBrown);
                buffer.DrawLine(7, 8, 17, 15, PixelColor32.AmberGold);
                buffer.SetPixel(15, 10, PixelColor32.SteelGlint);
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
                    baseColor = PixelColor32.FromHex("#FFE082"); // Warm golden sand
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
                buffer.SetPixel(4, 14, baseColor.Lighten(0.2f));
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
                default: return PixelColor32.ObsidianSlate;
            }
        }

        private static PixelColor32 GetEnemyAccentColor(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.IronBehemoth: return PixelColor32.DragonRuby;
                case EnemyKind.DrakeTitan: return PixelColor32.AmberGold;
                case EnemyKind.ColossusGolem: return PixelColor32.SpiritCyan;
                default: return PixelColor32.Crimson;
            }
        }
    }
}
