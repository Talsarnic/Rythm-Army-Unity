using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmArmy.World
{
    public enum CampaignBiome
    {
        CoralCoast,
        JungleFort,
        MistySwamp,
        VolcanicCaldera,
        IronBastion,
        DesertDunes,
        FrozenPeaks,
        RuinAltar
    }

    public enum LevelObjective
    {
        Advance,
        BreakFortification,
        DefeatBoss,
        Survive,
        Rescue
    }

    [Serializable]
    public class CampaignLevelDef
    {
        public string Id;
        public string DisplayName;
        public CampaignBiome Biome;
        public LevelObjective Objective;
        [TextArea] public string Description;
        public string SceneName;
        public int RecommendedPower;
        public int GoldReward;
        public string UnlocksAfter;
        public bool IsBossLevel;
        public bool IsFinalLevel;
        public List<string> RewardItemIds = new List<string>();

        public CampaignLevelDef(string id, string name, CampaignBiome biome, LevelObjective objective, string sceneName, string unlocksAfter, int power, int gold)
        {
            Id = id;
            DisplayName = name;
            Biome = biome;
            Objective = objective;
            SceneName = sceneName;
            UnlocksAfter = unlocksAfter;
            RecommendedPower = power;
            GoldReward = gold;
        }
    }

    public static class CampaignLevelDatabase
    {
        private static readonly List<CampaignLevelDef> Levels = new List<CampaignLevelDef>
        {
            new CampaignLevelDef("coast-01", "Tidebreak Landing", CampaignBiome.CoralCoast, LevelObjective.Advance, "Battle_CoralCoast_01", null, 10, 80),
            new CampaignLevelDef("coast-02", "Reefside Ambush", CampaignBiome.CoralCoast, LevelObjective.BreakFortification, "Battle_CoralCoast_02", "coast-01", 14, 110),
            new CampaignLevelDef("jungle-01", "The Green Rampart", CampaignBiome.JungleFort, LevelObjective.BreakFortification, "Battle_JungleFort_01", "coast-02", 18, 140),
            new CampaignLevelDef("swamp-01", "Whispering Mire", CampaignBiome.MistySwamp, LevelObjective.Survive, "Battle_MistySwamp_01", "jungle-01", 23, 175),
            new CampaignLevelDef("volcano-01", "Caldera March", CampaignBiome.VolcanicCaldera, LevelObjective.Advance, "Battle_VolcanicCaldera_01", "swamp-01", 28, 210),
            new CampaignLevelDef("bastion-01", "Iron Gate", CampaignBiome.IronBastion, LevelObjective.BreakFortification, "Battle_IronBastion_01", "volcano-01", 34, 260),
            new CampaignLevelDef("desert-01", "Dunes of the Fallen", CampaignBiome.DesertDunes, LevelObjective.Advance, "Battle_DesertDunes_01", "bastion-01", 40, 320),
            new CampaignLevelDef("frozen-01", "Whitefang Pass", CampaignBiome.FrozenPeaks, LevelObjective.DefeatBoss, "Battle_FrozenPeaks_01", "desert-01", 48, 400),
            new CampaignLevelDef("ruins-01", "Altar of the Last Beat", CampaignBiome.RuinAltar, LevelObjective.DefeatBoss, "Battle_RuinAltar_01", "frozen-01", 58, 650)
        };

        public static IReadOnlyList<CampaignLevelDef> All => Levels;

        public static CampaignLevelDef Get(string id)
        {
            return Levels.Find(level => level.Id == id);
        }

        public static bool IsUnlocked(string id, Func<string, bool> completionCheck)
        {
            var level = Get(id);
            if (level == null) return false;
            if (string.IsNullOrEmpty(level.UnlocksAfter)) return true;
            return completionCheck != null && completionCheck(level.UnlocksAfter);
        }
    }
}
