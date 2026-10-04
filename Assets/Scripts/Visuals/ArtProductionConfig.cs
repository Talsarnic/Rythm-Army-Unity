using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Single source of truth for the production art contract.
    /// Keeps the approved visual direction explicit inside the Unity project.
    /// </summary>
    public static class ArtProductionConfig
    {
        public const string StyleName = "Rhythm Army Painterly Pixel";
        public const string StyleReference = "Original fantasy pixel art with chunky silhouettes, painterly clusters, selective outlines, warm highlights, cool shadows, and rich material definition.";
        public const int GameplayWidth = 384;
        public const int GameplayHeight = 216;
        public const int InfantrySpriteSize = 32;
        public const int HeavySpriteSize = 48;
        public const int BossSpriteSize = 96;

        public static readonly string[] RequiredUnitStates =
        {
            "Idle", "March", "Attack", "Defend", "Charge",
            "Jump", "Hurt", "Death", "Fever", "HeroAbility"
        };

        public static readonly string[] RequiredBiomeLayers =
        {
            "SkyAtmosphere", "FarScenery", "MidTerrain", "ForegroundGround"
        };

        public static readonly string[] RequiredCampCategories =
        {
            "TownSquare", "Barracks", "Blacksmith", "SpiritAltar", "SacredTree",
            "CampfireFeast", "HeroShrine", "MerchantMarket", "MissionWarTable"
        };

        public static bool IsProductionClass(UnitClass unitClass)
        {
            return unitClass == UnitClass.Spearman;
        }

        public static bool HasApprovedEquipmentVisual(string itemId)
        {
            return EquipmentVisualRegistry.Get(itemId) != null;
        }
    }
}
