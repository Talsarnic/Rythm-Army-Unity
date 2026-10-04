using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Battle
{
    public class BossHuntOption
    {
        public EnemyKind BossKind { get; set; }
        public string BossName { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevelUnlocked { get; set; }
        public string Biome { get; set; }
        public int RecommendedGearScore { get; set; }
        public int GoldReward { get; set; }
        public List<string> GuaranteedLoot { get; set; }

        public BossHuntOption()
        {
            GuaranteedLoot = new List<string>();
        }
    }

    /// <summary>
    /// Manages repeatable Boss Hunting Trials with dynamic level scaling (Levels 1 to 10+),
    /// escalating HP, attack power, gold bounties, and tiered material & relic mask drop tables.
    /// </summary>
    public static class BossHuntingSystem
    {
        public static readonly EnemyKind[] ColossalBosses = new[]
        {
            EnemyKind.DrakeTitan,
            EnemyKind.IronBehemoth,
            EnemyKind.ColossusGolem
        };

        public static string GetBossName(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.DrakeTitan: return "Pyro Drake Titan";
                case EnemyKind.IronBehemoth: return "Armored Iron Behemoth";
                case EnemyKind.ColossusGolem: return "Ancient Ruin Colossus";
                default: return kind.ToString();
            }
        }

        public static string GetBossBiome(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.DrakeTitan: return "volcanic-caldera";
                case EnemyKind.IronBehemoth: return "frozen-peaks";
                case EnemyKind.ColossusGolem: return "ruin-altar";
                default: return "ruin-altar";
            }
        }

        public static bool IsBossUnlocked(SaveData save, EnemyKind kind)
        {
            if (save == null) return false;
            switch (kind)
            {
                case EnemyKind.DrakeTitan:
                    return save.CompletedMissions.Contains("drake-caldera") || save.CompletedMissions.Contains("mission-3");
                case EnemyKind.IronBehemoth:
                    return save.CompletedMissions.Contains("iron-ridge") || save.CompletedMissions.Contains("mission-7");
                case EnemyKind.ColossusGolem:
                    return save.CompletedMissions.Contains("golem-altar") || save.CompletedMissions.Contains("mission-8");
                default:
                    return false;
            }
        }

        public static int GetBossLevel(SaveData save, EnemyKind kind)
        {
            if (save == null) return 1;
            string key = kind.ToString();
            int lvl;
            if (save.BossHuntLevels != null && save.BossHuntLevels.TryGetValue(key, out lvl))
            {
                return Math.Max(1, lvl);
            }
            return 1;
        }

        public static void RecordBossVictory(SaveData save, EnemyKind kind, int levelDefeated)
        {
            if (save == null) return;
            if (save.BossHuntLevels == null)
            {
                save.BossHuntLevels = new Dictionary<string, int>();
            }

            string key = kind.ToString();
            int currentLevel = GetBossLevel(save, kind);
            if (levelDefeated >= currentLevel)
            {
                save.BossHuntLevels[key] = levelDefeated + 1;
            }
        }

        public static List<BossHuntOption> GetAvailableBossHunts(SaveData save)
        {
            var list = new List<BossHuntOption>();
            foreach (var boss in ColossalBosses)
            {
                if (IsBossUnlocked(save, boss))
                {
                    int lvl = GetBossLevel(save, boss);
                    int baseScore = boss == EnemyKind.DrakeTitan ? 25 : (boss == EnemyKind.IronBehemoth ? 50 : 75);
                    int baseGold = boss == EnemyKind.DrakeTitan ? 200 : (boss == EnemyKind.IronBehemoth ? 350 : 500);

                    var opt = new BossHuntOption
                    {
                        BossKind = boss,
                        BossName = GetBossName(boss),
                        CurrentLevel = lvl,
                        MaxLevelUnlocked = lvl,
                        Biome = GetBossBiome(boss),
                        RecommendedGearScore = baseScore + (lvl - 1) * 12,
                        GoldReward = (int)(baseGold * (1f + (lvl - 1) * 0.35f))
                    };

                    // Add preview loot
                    if (lvl >= 5)
                    {
                        opt.GuaranteedLoot.Add("mat-ancient-ore");
                        opt.GuaranteedLoot.Add("hero-mask-apex");
                    }
                    else if (lvl >= 3)
                    {
                        opt.GuaranteedLoot.Add("mat-mithril");
                        opt.GuaranteedLoot.Add(boss == EnemyKind.DrakeTitan ? "mat-dragon-scale" : "hero-mask-valor");
                    }
                    else
                    {
                        opt.GuaranteedLoot.Add("mat-iron-slag");
                        opt.GuaranteedLoot.Add("hero-mask-courage");
                    }

                    list.Add(opt);
                }
            }
            return list;
        }

        public static MissionDef GenerateBossHuntMission(EnemyKind bossKind, int level)
        {
            level = Math.Max(1, level);
            string bossName = GetBossName(bossKind);
            string biome = GetBossBiome(bossKind);

            int baseGold = bossKind == EnemyKind.DrakeTitan ? 220 : (bossKind == EnemyKind.IronBehemoth ? 380 : 550);
            int baseScore = bossKind == EnemyKind.DrakeTitan ? 25 : (bossKind == EnemyKind.IronBehemoth ? 50 : 75);

            var mission = new MissionDef
            {
                Id = string.Format("bosshunt-{0}-lvl{1}", bossKind.ToString().ToLowerInvariant(), level),
                Name = string.Format("Trial: {0} (Lv. {1})", bossName, level),
                Blurb = string.Format("Face the mighty {0} at power level {1}. Defeat the beast to claim supreme materials and sacred relic masks.", bossName, level),
                Biome = biome,
                GoldReward = (int)(baseGold * (1f + (level - 1) * 0.40f)),
                RecommendedGearScore = baseScore + (level - 1) * 15,
                Bpm = 120f,
                WorldLength = 3400f,
                GoalX = 3200f
            };

            // Setup scaled waves
            var waveList = new List<WaveDef>();
            if (level >= 3)
            {
                // Flanking escort wave
                if (bossKind == EnemyKind.DrakeTitan)
                {
                    waveList.Add(new WaveDef(800f, new EnemySpawnDef(EnemyKind.WildBoar, 2)));
                }
                else if (bossKind == EnemyKind.IronBehemoth)
                {
                    waveList.Add(new WaveDef(800f, new EnemySpawnDef(EnemyKind.TribeCavalry, 2)));
                }
                else
                {
                    waveList.Add(new WaveDef(800f, new EnemySpawnDef(EnemyKind.TribeHammerer, 2), new EnemySpawnDef(EnemyKind.TribeMage, 1)));
                }
            }

            // Boss wave
            waveList.Add(new WaveDef(1800f, new EnemySpawnDef(bossKind, 1)));
            mission.Waves = waveList;

            // Tiered rewards
            var rewards = new List<LootRewardSummary>();
            var lootTable = new List<string>();

            if (level >= 5)
            {
                rewards.Add(new LootRewardSummary("mat-ancient-ore", 1 + (level - 5)));
                rewards.Add(new LootRewardSummary("mat-mithril", 3));
                lootTable.Add("mat-ancient-ore");
                lootTable.Add("mat-mithril");
                lootTable.Add("hero-mask-apex");
                lootTable.Add("hero-mask-wrath");
            }
            else if (level >= 3)
            {
                rewards.Add(new LootRewardSummary("mat-mithril", 2));
                rewards.Add(new LootRewardSummary(bossKind == EnemyKind.DrakeTitan ? "mat-dragon-scale" : "mat-beast-hide", 2));
                lootTable.Add("mat-mithril");
                lootTable.Add("mat-dragon-scale");
                lootTable.Add("hero-mask-valor");
            }
            else
            {
                rewards.Add(new LootRewardSummary("mat-iron-slag", 3));
                rewards.Add(new LootRewardSummary("mat-beast-hide", 1));
                lootTable.Add("mat-iron-slag");
                lootTable.Add("mat-beast-hide");
                lootTable.Add("hero-mask-courage");
            }

            mission.BaseRewards = rewards;
            mission.LootTable = lootTable;

            return mission;
        }
    }
}
