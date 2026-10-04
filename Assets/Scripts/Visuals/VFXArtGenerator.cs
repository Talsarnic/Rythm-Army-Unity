using System;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum VFXSpriteType
    {
        HitSparks,
        DustPuff,
        FeverSparkleAura,
        ShieldBlockFlash,
        BladeSlashArc,
        BludgeonShockwave,
        RainDropRipples,
        TailwindWindStreaks,
        EarthquakeFissure,
        StormLightningBolt
    }

    public enum ProjectileArtType
    {
        ArrowIron,
        ArrowFlame,
        ArrowFrost,
        ArrowLightning,
        SpearJavelin,
        CatapultBoulder,
        MageFireball,
        MageFrostbolt,
        SonicBlastWave
    }

    /// <summary>
    /// Generates Moonlighter-style pixel VFX sprite sheets, ballistic projectiles, and relic masks.
    /// </summary>
    public static class VFXArtGenerator
    {
        /// <summary>
        /// Generates multi-frame VFX sprite sheets (e.g. 32x32 per frame, 4–6 frames).
        /// </summary>
        public static PixelBitmapBuffer GenerateVFXSpriteSheet(VFXSpriteType vfx)
        {
            int frameSize = 32;
            int frameCount = (vfx == VFXSpriteType.StormLightningBolt) ? 6 : 4;
            var buffer = new PixelBitmapBuffer(frameSize * frameCount, frameSize);

            for (int f = 0; f < frameCount; f++)
            {
                int ox = f * frameSize;
                int cx = ox + frameSize / 2;
                int cy = frameSize / 2;

                switch (vfx)
                {
                    case VFXSpriteType.HitSparks:
                        // High impact orange/yellow spark starburst
                        int r = 2 + f * 3;
                        buffer.DrawLine(cx - r, cy, cx + r, cy, PixelColor32.AmberGold);
                        buffer.DrawLine(cx, cy - r, cx, cy + r, PixelColor32.AmberGold);
                        buffer.DrawLine(cx - r / 2, cy - r / 2, cx + r / 2, cy + r / 2, PixelColor32.FeverYellow);
                        buffer.DrawLine(cx - r / 2, cy + r / 2, cx + r / 2, cy - r / 2, PixelColor32.FeverYellow);
                        buffer.SetPixel(cx, cy, PixelColor32.White);
                        break;

                    case VFXSpriteType.DustPuff:
                        // Soft cloudy expanding dust on jump/march
                        int dustR = 3 + f * 2;
                        buffer.DrawCircle(cx - 2, cy + 2, dustR, PixelColor32.FromHex("#CFD8DC"), true);
                        buffer.DrawCircle(cx + 3, cy + 3, dustR - 1, PixelColor32.FromHex("#ECEFF1"), true);
                        break;

                    case VFXSpriteType.FeverSparkleAura:
                        // Radiant golden/cyan musical energy stars
                        buffer.DrawCircle(cx, cy - (f * 2), 2 + (f % 2), PixelColor32.FeverYellow, true);
                        buffer.SetPixel(cx - 4 + f * 2, cy + 4 - f, PixelColor32.SpiritCyan);
                        buffer.SetPixel(cx + 4 - f * 2, cy - 4 + f, PixelColor32.White);
                        break;

                    case VFXSpriteType.ShieldBlockFlash:
                        // Expanding hexagonal forcefield ring
                        int hexR = 4 + f * 3;
                        buffer.DrawCircle(cx, cy, hexR, PixelColor32.SpiritCyan, false);
                        buffer.DrawCircle(cx, cy, Math.Max(1, hexR - 2), PixelColor32.White, false);
                        break;

                    case VFXSpriteType.BladeSlashArc:
                        // Dynamic glowing weapon sweep
                        int arcW = 4 + f * 4;
                        buffer.DrawLine(cx - arcW, cy - 8 + f * 3, cx + arcW, cy + 8 - f * 3, PixelColor32.Crimson);
                        buffer.DrawLine(cx - arcW + 1, cy - 7 + f * 3, cx + arcW - 1, cy + 7 - f * 3, PixelColor32.AmberGold);
                        buffer.DrawLine(cx - arcW + 2, cy - 6 + f * 3, cx + arcW - 2, cy + 6 - f * 3, PixelColor32.White);
                        break;

                    case VFXSpriteType.BludgeonShockwave:
                        // Expanding ground shockwave oval
                        buffer.DrawCircle(cx, cy + 6, 4 + f * 3, PixelColor32.ObsidianSlate, false);
                        buffer.DrawCircle(cx, cy + 6, 3 + f * 3, PixelColor32.IronGrey, false);
                        buffer.DrawCircle(cx, cy + 6, 2 + f * 3, PixelColor32.White, false);
                        break;

                    case VFXSpriteType.RainDropRipples:
                        // Falling rain streak & ground splash ripple
                        buffer.DrawLine(cx + 4, cy - 12 + f * 4, cx - 2, cy - 4 + f * 4, PixelColor32.SpiritCyan);
                        buffer.DrawCircle(cx, cy + 8, 2 + f * 2, PixelColor32.MithrilBlue, false);
                        break;

                    case VFXSpriteType.TailwindWindStreaks:
                        // Whirling green breeze lines and floating leaf
                        buffer.DrawLine(cx - 10, cy - 4 + f * 2, cx + 10, cy - 4 + f * 2, PixelColor32.VerdantMoss);
                        buffer.DrawCircle(cx + f * 3 - 6, cy + 2, 2, PixelColor32.VerdantMoss, true); // Leaf
                        break;

                    case VFXSpriteType.EarthquakeFissure:
                        // Jagged cracked earth splitting open
                        buffer.DrawLine(cx - 12, cy + 8, cx - 4, cy + 4, PixelColor32.ObsidianSlate);
                        buffer.DrawLine(cx - 4, cy + 4, cx + 4, cy + 10, PixelColor32.Black);
                        buffer.DrawLine(cx + 4, cy + 10, cx + 12, cy + 6, PixelColor32.ObsidianSlate);
                        break;

                    case VFXSpriteType.StormLightningBolt:
                        // Piercing electric jagged bolt striking from sky
                        int topX = cx + (f % 2 == 0 ? -2 : 2);
                        int midX = cx + (f % 2 == 0 ? 4 : -4);
                        buffer.DrawLine(topX, 0, midX, cy, PixelColor32.SpiritCyan);
                        buffer.DrawLine(midX, cy, cx, frameSize - 1, PixelColor32.FeverYellow);
                        buffer.DrawLine(midX - 1, cy, cx - 1, frameSize - 1, PixelColor32.White);
                        break;
                }
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates 24x24 px ballistic projectile sprite.
        /// </summary>
        public static PixelBitmapBuffer GenerateProjectileSprite(ProjectileArtType type)
        {
            var buffer = new PixelBitmapBuffer(24, 24);

            switch (type)
            {
                case ProjectileArtType.ArrowIron:
                    buffer.DrawLine(3, 20, 19, 4, PixelColor32.WoodBrown); // Shaft
                    buffer.DrawLine(18, 5, 21, 2, PixelColor32.IronGrey); // Iron tip
                    buffer.DrawLine(2, 21, 4, 21, PixelColor32.White); // Fletching
                    break;

                case ProjectileArtType.ArrowFlame:
                    buffer.DrawLine(3, 20, 18, 5, PixelColor32.WoodBrown);
                    buffer.DrawCircle(19, 4, 3, PixelColor32.Crimson, true);
                    buffer.DrawCircle(19, 4, 1, PixelColor32.AmberGold, true);
                    break;

                case ProjectileArtType.ArrowFrost:
                    buffer.DrawLine(3, 20, 18, 5, PixelColor32.WoodBrown);
                    buffer.DrawCircle(19, 4, 3, PixelColor32.SpiritCyan, true);
                    buffer.SetPixel(19, 4, PixelColor32.White);
                    break;

                case ProjectileArtType.ArrowLightning:
                    buffer.DrawLine(3, 20, 18, 5, PixelColor32.AmberGold);
                    buffer.DrawLine(16, 7, 21, 2, PixelColor32.FeverYellow);
                    buffer.DrawLine(15, 6, 20, 1, PixelColor32.White);
                    break;

                case ProjectileArtType.SpearJavelin:
                    buffer.DrawLine(2, 21, 20, 3, PixelColor32.LeatherBrown); // Heavy shaft
                    buffer.DrawLine(19, 4, 22, 1, PixelColor32.IronGrey); // Broad spearhead
                    buffer.DrawLine(18, 5, 21, 2, PixelColor32.White);
                    break;

                case ProjectileArtType.CatapultBoulder:
                    buffer.DrawCircle(12, 12, 6, PixelColor32.ObsidianSlate, true);
                    buffer.DrawCircle(10, 10, 4, PixelColor32.IronGrey, true);
                    buffer.SetPixel(9, 9, PixelColor32.White);
                    break;

                case ProjectileArtType.MageFireball:
                    buffer.DrawCircle(12, 12, 5, PixelColor32.Crimson, true);
                    buffer.DrawCircle(12, 12, 3, PixelColor32.AmberGold, true);
                    buffer.DrawCircle(12, 12, 1, PixelColor32.White, true);
                    buffer.DrawLine(6, 16, 12, 12, PixelColor32.AmberGold); // Fire tail
                    break;

                case ProjectileArtType.MageFrostbolt:
                    buffer.DrawCircle(12, 12, 5, PixelColor32.OceanicTeal, true);
                    buffer.DrawCircle(12, 12, 3, PixelColor32.SpiritCyan, true);
                    buffer.SetPixel(12, 12, PixelColor32.White);
                    break;

                case ProjectileArtType.SonicBlastWave:
                    buffer.DrawCircle(12, 12, 7, PixelColor32.SpiritCyan, false);
                    buffer.DrawCircle(12, 12, 5, PixelColor32.MithrilBlue, false);
                    buffer.DrawCircle(12, 12, 3, PixelColor32.White, false);
                    break;
            }

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }

        /// <summary>
        /// Generates 32x32 px Relic Mask pixel sprites for Squad Hero Champions.
        /// </summary>
        public static PixelBitmapBuffer GenerateHeroRelicMask(string maskId)
        {
            var buffer = new PixelBitmapBuffer(32, 32);

            PixelColor32 maskBase;
            PixelColor32 maskGem;

            if (maskId.Contains("courage"))
            {
                maskBase = PixelColor32.IronGrey;
                maskGem = PixelColor32.Crimson;
            }
            else if (maskId.Contains("valor"))
            {
                maskBase = PixelColor32.AmberGold;
                maskGem = PixelColor32.OceanicTeal;
            }
            else if (maskId.Contains("wrath"))
            {
                maskBase = PixelColor32.ObsidianSlate;
                maskGem = PixelColor32.FeverYellow;
            }
            else // Apex
            {
                maskBase = PixelColor32.StarGold;
                maskGem = PixelColor32.SpiritCyan;
            }

            // Visor Crest / Mask Face
            buffer.FillRect(8, 8, 16, 16, maskBase);
            buffer.DrawCircle(16, 12, 6, maskBase, true);

            // Horns / Crown Spikes
            buffer.DrawLine(8, 8, 4, 2, maskBase);
            buffer.DrawLine(23, 8, 27, 2, maskBase);
            buffer.DrawLine(16, 6, 16, 1, maskBase); // Center crown spire

            // Piercing Glowing Eye Slits
            buffer.FillRect(10, 14, 4, 2, maskGem);
            buffer.FillRect(18, 14, 4, 2, maskGem);
            buffer.SetPixel(11, 14, PixelColor32.White);
            buffer.SetPixel(19, 14, PixelColor32.White);

            // Center Relic Gem Jewel
            buffer.DrawCircle(16, 8, 2, maskGem, true);
            buffer.SetPixel(16, 8, PixelColor32.White);

            buffer.ApplyDarkOutline(PixelColor32.Black);
            return buffer;
        }
    }
}
