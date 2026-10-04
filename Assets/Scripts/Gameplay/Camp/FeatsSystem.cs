using System;
using System.Collections.Generic;

namespace RhythmArmy.Gameplay.Camp
{
    public enum FeatCategory
    {
        Combat,
        Rhythm,
        CampProgression,
        BossHunting,
        DungeonRuins
    }

    public class FeatDef
    {
        public string Id;
        public string Title;
        public string Description;
        public FeatCategory Category;
        public int TargetCount;
        public int GoldReward;
        public string RewardItemId;

        public FeatDef(string id, string title, string description, FeatCategory category, int targetCount, int goldReward, string rewardItemId = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            TargetCount = targetCount;
            GoldReward = goldReward;
            RewardItemId = rewardItemId;
        }
    }

    /// <summary>
    /// Feats of the Almighty Creator: Achievement and milestone progression system
    /// rewarding gold, rare crafting materials, and exclusive relic titles.
    /// </summary>
    public static class FeatsSystem
    {
        private static readonly Dictionary<string, FeatDef> _feats = new Dictionary<string, FeatDef>();

        static FeatsSystem()
        {
            // Rhythm Feats
            _feats["feat-rhythm-fever"] = new FeatDef("feat-rhythm-fever", "Fever Frenzy", "Reach Fever Mode in 10 battles.", FeatCategory.Rhythm, 10, 200, "mat-timber");
            _feats["feat-rhythm-combo30"] = new FeatDef("feat-rhythm-combo30", "Harmonic Perfection", "Achieve a combo chain of 30 or higher.", FeatCategory.Rhythm, 30, 500, "mat-mithril");

            // Combat Feats
            _feats["feat-combat-enemies50"] = new FeatDef("feat-combat-enemies50", "Vanguard Conqueror", "Defeat 50 enemy units across all missions.", FeatCategory.Combat, 50, 300, "mat-iron-slag");
            _feats["feat-combat-miracles5"] = new FeatDef("feat-combat-miracles5", "Voice of the Heavens", "Invoke 5 Divine Miracles during battle.", FeatCategory.Combat, 5, 400, "mat-stone");

            // Camp Progression
            _feats["feat-camp-craft10"] = new FeatDef("feat-camp-craft10", "Master Blacksmith", "Forge or socket 10 equipment items.", FeatCategory.CampProgression, 10, 450, "mat-mithril");
            _feats["feat-camp-awaken3"] = new FeatDef("feat-camp-awaken3", "Spiritual Evolution", "Awaken 3 squad units to Tier 2 or higher.", FeatCategory.CampProgression, 3, 600, "mat-dragon-scale");

            // Boss Hunting
            _feats["feat-boss-trials5"] = new FeatDef("feat-boss-trials5", "Titan Slayer", "Complete 5 Colossal Boss Hunting Trials.", FeatCategory.BossHunting, 5, 800, "hero-mask-wrath");

            // Dungeon Ruins
            _feats["feat-dungeon-floor5"] = new FeatDef("feat-dungeon-floor5", "Ruins Delver", "Reach Floor 5 in the Dungeon Ruins expedition.", FeatCategory.DungeonRuins, 5, 1000, "mat-dragon-scale");
        }

        public static FeatDef GetFeat(string featId)
        {
            FeatDef feat;
            if (_feats.TryGetValue(featId, out feat)) return feat;
            return null;
        }

        public static List<FeatDef> GetAllFeats()
        {
            return new List<FeatDef>(_feats.Values);
        }

        /// <summary>
        /// Updates progress for a feat and claims reward if newly completed.
        /// </summary>
        public static bool ProgressFeat(string featId, int increment, Dictionary<string, int> featProgress, HashSet<string> completedFeats, Dictionary<string, int> inventory, ref int playerGold)
        {
            var feat = GetFeat(featId);
            if (feat == null || completedFeats.Contains(featId)) return false;

            int current = 0;
            featProgress.TryGetValue(featId, out current);
            current += increment;
            featProgress[featId] = current;

            if (current >= feat.TargetCount)
            {
                completedFeats.Add(featId);
                playerGold += feat.GoldReward;

                if (!string.IsNullOrEmpty(feat.RewardItemId))
                {
                    int qty = 0;
                    inventory.TryGetValue(feat.RewardItemId, out qty);
                    inventory[feat.RewardItemId] = qty + 1;
                }
                return true;
            }
            return false;
        }
    }
}
