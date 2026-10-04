using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public struct Vector2Int
    {
        public int X;
        public int Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class UnitSpriteSpec
    {
        public UnitClass Class;
        public int SpriteWidth;
        public int SpriteHeight;
        public Vector2Int WeaponAnchor;
        public Vector2Int ShieldAnchor;
        public Vector2Int HelmetAnchor;
        public string TextureAtlasId;

        public UnitSpriteSpec(UnitClass unitClass, int width, int height, Vector2Int weaponAnchor, Vector2Int shieldAnchor, Vector2Int helmetAnchor, string textureAtlasId)
        {
            Class = unitClass;
            SpriteWidth = width;
            SpriteHeight = height;
            WeaponAnchor = weaponAnchor;
            ShieldAnchor = shieldAnchor;
            HelmetAnchor = helmetAnchor;
            TextureAtlasId = textureAtlasId;
        }
    }

    public static class MoonlighterColorPalettes
    {
        // 16-bit Painterly Warm Palettes
        public const string AmberGold = "#FFA726";
        public const string DeepCrimson = "#D32F2F";
        public const string VerdantMoss = "#4CAF50";
        public const string OceanicTeal = "#00897B";
        public const string ObsidianSlate = "#263238";
        public const string SpiritCyan = "#00E5FF";
        public const string FeverSparkle = "#FFEB3B";
    }

    public static class PixelSpriteRegistry
    {
        private static readonly Dictionary<UnitClass, UnitSpriteSpec> _unitSpecs = new Dictionary<UnitClass, UnitSpriteSpec>
        {
            {
                UnitClass.Banner,
                new UnitSpriteSpec(UnitClass.Banner, 32, 32, new Vector2Int(18, 14), new Vector2Int(6, 12), new Vector2Int(16, 26), "atlas_units_banner")
            },
            {
                UnitClass.Spearman,
                new UnitSpriteSpec(UnitClass.Spearman, 32, 32, new Vector2Int(20, 16), new Vector2Int(8, 12), new Vector2Int(16, 26), "atlas_units_spearman")
            },
            {
                UnitClass.Swordsman,
                new UnitSpriteSpec(UnitClass.Swordsman, 32, 32, new Vector2Int(18, 14), new Vector2Int(6, 14), new Vector2Int(16, 26), "atlas_units_swordsman")
            },
            {
                UnitClass.Archer,
                new UnitSpriteSpec(UnitClass.Archer, 32, 32, new Vector2Int(19, 15), new Vector2Int(7, 12), new Vector2Int(16, 26), "atlas_units_archer")
            },
            {
                UnitClass.Cavalry,
                new UnitSpriteSpec(UnitClass.Cavalry, 48, 48, new Vector2Int(30, 24), new Vector2Int(12, 20), new Vector2Int(24, 40), "atlas_units_cavalry")
            },
            {
                UnitClass.Hammerer,
                new UnitSpriteSpec(UnitClass.Hammerer, 48, 48, new Vector2Int(28, 22), new Vector2Int(10, 18), new Vector2Int(24, 40), "atlas_units_hammerer")
            },
            {
                UnitClass.Hornist,
                new UnitSpriteSpec(UnitClass.Hornist, 32, 32, new Vector2Int(18, 15), new Vector2Int(8, 12), new Vector2Int(16, 26), "atlas_units_hornist")
            },
            {
                UnitClass.Skyrider,
                new UnitSpriteSpec(UnitClass.Skyrider, 48, 48, new Vector2Int(28, 26), new Vector2Int(10, 20), new Vector2Int(24, 42), "atlas_units_skyrider")
            },
            {
                UnitClass.Mage,
                new UnitSpriteSpec(UnitClass.Mage, 32, 32, new Vector2Int(20, 18), new Vector2Int(6, 12), new Vector2Int(16, 26), "atlas_units_mage")
            },
            {
                UnitClass.Brawler,
                new UnitSpriteSpec(UnitClass.Brawler, 32, 32, new Vector2Int(20, 14), new Vector2Int(8, 14), new Vector2Int(16, 26), "atlas_units_brawler")
            }
        };

        public static UnitSpriteSpec GetSpec(UnitClass unitClass)
        {
            UnitSpriteSpec spec;
            if (_unitSpecs.TryGetValue(unitClass, out spec))
            {
                return spec;
            }
            return _unitSpecs[UnitClass.Spearman];
        }
    }
}
