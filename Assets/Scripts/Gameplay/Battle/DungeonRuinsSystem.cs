using System;
using System.Collections.Generic;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Gameplay.Battle
{
    public enum DungeonHazardAffix
    {
        None,
        ScorchingHeat,     // Periodic fire damage ticks unless Rain miracle is active
        DenseFog,          // Reduces ranged accuracy/distance
        GaleWinds,         // Strong headwinds impeding march speed
        ArmoredPlating,    // All enemies gain +15 base defense
        BloodMoonFury      // Enemies deal +25% attack damage
    }

    public class DungeonFloorDef
    {
        public int FloorNumber;
        public string Biome;
        public DungeonHazardAffix Affix;
        public List<WaveDef> Waves;
        public int CompletionGoldBonus;
        public string GuaranteedMaterialDrop;

        public DungeonFloorDef(int floor, string biome, DungeonHazardAffix affix, int goldBonus, string material)
        {
            FloorNumber = floor;
            Biome = biome;
            Affix = affix;
            Waves = new List<WaveDef>();
            CompletionGoldBonus = goldBonus;
            GuaranteedMaterialDrop = material;
        }
    }

    public class DungeonExpeditionState
    {
        public int CurrentFloor = 1;
        public int MaxFloorReached = 1;
        public int TotalGoldBanked = 0;
        public List<string> PendingLootBag = new List<string>();
        public bool IsEscapedWithMedallion = false;
        public bool IsWipedOut = false;
    }

    /// <summary>
    /// Moonlighter-inspired Dungeon Ruins endless crawl expedition.
    /// Features floor depth scaling, hazard affixes, boss rooms every 5th floor,
    /// and the Merchant Escape Medallion to extract all collected loot safely.
    /// </summary>
    public static class DungeonRuinsSystem
    {
        public const int BossFloorInterval = 5;

        public static DungeonFloorDef GenerateFloor(int floorNumber)
        {
            string[] biomes = new string[]
            {
                "coral-coast",
                "jungle-fort",
                "misty-swamp",
                "volcanic-caldera",
                "iron-bastion",
                "ruin-altar",
                "desert-dunes",
                "frozen-peaks"
            };

            string biome = biomes[(floorNumber - 1) % biomes.Length];
            var affix = DungeonHazardAffix.None;
            if (floorNumber >= 2)
            {
                var affixes = (DungeonHazardAffix[])Enum.GetValues(typeof(DungeonHazardAffix));
                affix = affixes[((floorNumber - 1) % (affixes.Length - 1)) + 1];
            }

            int goldBonus = 150 + (floorNumber * 75);
            string material = "mat-stone";
            if (floorNumber >= 10) material = "mat-dragon-scale";
            else if (floorNumber >= 5) material = "mat-mithril";
            else if (floorNumber >= 2) material = "mat-iron-slag";

            var floorDef = new DungeonFloorDef(floorNumber, biome, affix, goldBonus, material);

            bool isBossFloor = (floorNumber % BossFloorInterval == 0);
            if (isBossFloor)
            {
                // Boss Floor
                EnemyKind bossKind = EnemyKind.IronBehemoth;
                if (floorNumber % 15 == 0) bossKind = EnemyKind.DrakeTitan;
                else if (floorNumber % 10 == 0) bossKind = EnemyKind.ColossusGolem;

                var bossWave = new WaveDef(1000f,
                    new EnemySpawnDef(bossKind, 1),
                    new EnemySpawnDef(EnemyKind.TribeSpearman, 2),
                    new EnemySpawnDef(EnemyKind.TribeArcher, 2)
                );
                floorDef.Waves.Add(bossWave);
            }
            else
            {
                // Normal Wave 1
                var wave1 = new WaveDef(600f,
                    new EnemySpawnDef(EnemyKind.PlainsRunner, 2),
                    new EnemySpawnDef(EnemyKind.WildBoar, 1)
                );
                floorDef.Waves.Add(wave1);

                // Normal Wave 2
                var wave2 = new WaveDef(1200f,
                    new EnemySpawnDef(EnemyKind.TribeSwordsman, 2),
                    new EnemySpawnDef(EnemyKind.TribeArcher, 2)
                );
                if (floorNumber >= 3)
                {
                    wave2.Enemies.Add(new EnemySpawnDef(EnemyKind.TribeCavalry, 1));
                }
                floorDef.Waves.Add(wave2);
            }

            return floorDef;
        }

        /// <summary>
        /// Converts a Dungeon Floor into an executable MissionDef for BattleManager.
        /// </summary>
        public static MissionDef CreateFloorMission(DungeonFloorDef floor)
        {
            var mission = new MissionDef
            {
                Id = "dungeon-floor-" + floor.FloorNumber,
                Name = "Dungeon Ruins - Floor " + floor.FloorNumber,
                Biome = floor.Biome,
                Blurb = "Explore ancient ruins. Affix: " + floor.Affix.ToString(),
                RecommendedGearScore = 10 + (floor.FloorNumber * 5),
                GoldReward = floor.CompletionGoldBonus,
                WorldLength = 2600f,
                GoalX = 2400f,
                Waves = new List<WaveDef>()
            };

            mission.Waves.AddRange(floor.Waves);
            mission.BaseRewards.Add(new LootRewardSummary(floor.GuaranteedMaterialDrop, 2));
            mission.LootTable.Add(floor.GuaranteedMaterialDrop);
            return mission;
        }

        /// <summary>
        /// Uses the Merchant Escape Medallion to bank all loot and gold collected on the expedition.
        /// </summary>
        public static bool UseEscapeMedallion(DungeonExpeditionState expedition, Dictionary<string, int> playerInventory, ref int playerGold)
        {
            if (expedition == null || expedition.IsWipedOut) return false;

            playerGold += expedition.TotalGoldBanked;
            foreach (var mat in expedition.PendingLootBag)
            {
                int count = 0;
                playerInventory.TryGetValue(mat, out count);
                playerInventory[mat] = count + 1;
            }

            expedition.IsEscapedWithMedallion = true;
            return true;
        }

        /// <summary>
        /// If the squad is defeated in the dungeon without escaping, half of the pending gold and materials are lost.
        /// </summary>
        public static void HandleExpeditionDefeat(DungeonExpeditionState expedition, Dictionary<string, int> playerInventory, ref int playerGold)
        {
            if (expedition == null) return;
            expedition.IsWipedOut = true;

            // Retain 50% consolation salvage
            int recoveredGold = expedition.TotalGoldBanked / 2;
            playerGold += recoveredGold;

            for (int i = 0; i < expedition.PendingLootBag.Count / 2; i++)
            {
                string mat = expedition.PendingLootBag[i];
                int count = 0;
                playerInventory.TryGetValue(mat, out count);
                playerInventory[mat] = count + 1;
            }
        }
    }
}
