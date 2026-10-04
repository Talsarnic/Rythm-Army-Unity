using System;
using System.Collections.Generic;

namespace RhythmArmy.Core.Data
{
    [Serializable]
    public class EnemySpawnDef
    {
        public EnemyKind Kind;
        public int Count;
        public float SpawnOffset = 25f;

        public EnemySpawnDef(EnemyKind kind, int count, float spawnOffset = 25f)
        {
            Kind = kind;
            Count = count;
            SpawnOffset = spawnOffset;
        }
    }

    [Serializable]
    public class WaveDef
    {
        public float AtX;
        public List<EnemySpawnDef> Enemies = new List<EnemySpawnDef>();
        public List<EnemySpawnDef> Spawns { get { return Enemies; } }

        public WaveDef(float atX, params EnemySpawnDef[] enemies)
        {
            AtX = atX;
            Enemies.AddRange(enemies);
        }
    }

    [Serializable]
    public class LootRewardSummary
    {
        public string ItemId;
        public int Qty;
        public int Quantity
        {
            get { return Qty; }
            set { Qty = value; }
        }

        public LootRewardSummary(string itemId, int qty)
        {
            ItemId = itemId;
            Qty = qty;
        }
    }

    [Serializable]
    public class MissionDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Blurb { get; set; }
        public string Biome { get; set; }
        public int GoldReward { get; set; }
        public int RecommendedGearScore { get; set; }
        public float Bpm { get; set; }
        public float WorldLength { get; set; }
        public float GoalX { get; set; }
        public bool IsTutorial { get; set; }
        public string UnlockAfter { get; set; }
        public List<WaveDef> Waves { get; set; }
        public List<LootRewardSummary> BaseRewards { get; set; }
        public List<string> LootTable { get; set; }

        public MissionDef()
        {
            Bpm = 120f;
            Biome = "coral-coast";
            GoldReward = 100;
            RecommendedGearScore = 10;
            Waves = new List<WaveDef>();
            BaseRewards = new List<LootRewardSummary>();
            LootTable = new List<string>();
        }

        public static readonly List<MissionDef> Campaign = new List<MissionDef>
        {
            new MissionDef
            {
                Id = "coast-hunt",
                Name = "1. Hunting on the Coral Coast",
                Blurb = "Hunt evasive plains runners and sand crabs along the shimmering coastline to gather meats and wood.",
                Biome = "coral-coast",
                GoldReward = 80,
                RecommendedGearScore = 5,
                Bpm = 120f,
                WorldLength = 2600f,
                GoalX = 2400f,
                Waves = new List<WaveDef>
                {
                    new WaveDef(600f, new EnemySpawnDef(EnemyKind.PlainsRunner, 3)),
                    new WaveDef(1200f, new EnemySpawnDef(EnemyKind.SandCrab, 2), new EnemySpawnDef(EnemyKind.PlainsRunner, 2)),
                    new WaveDef(1800f, new EnemySpawnDef(EnemyKind.Stag, 1), new EnemySpawnDef(EnemyKind.PlainsRunner, 2))
                },
                BaseRewards = new List<LootRewardSummary>
                {
                    new LootRewardSummary("mat-jerky", 4),
                    new LootRewardSummary("mat-timber", 2)
                },
                LootTable = new List<string> { "mat-jerky", "mat-timber" }
            },
            new MissionDef
            {
                Id = "shadowmask-clash",
                Name = "2. The Masked Clan in the Jungle",
                Blurb = "Clash with rival tribe spearmen and shield guards defending their jungle outposts.",
                Biome = "jungle-fort",
                GoldReward = 120,
                RecommendedGearScore = 15,
                Bpm = 120f,
                WorldLength = 3200f,
                GoalX = 3000f,
                UnlockAfter = "coast-hunt",
                Waves = new List<WaveDef>
                {
                    new WaveDef(700f, new EnemySpawnDef(EnemyKind.Barricade, 1), new EnemySpawnDef(EnemyKind.TribeBanner, 1), new EnemySpawnDef(EnemyKind.TribeSpearman, 3)),
                    new WaveDef(1500f, new EnemySpawnDef(EnemyKind.TribeSwordsman, 2), new EnemySpawnDef(EnemyKind.TribeArcher, 2), new EnemySpawnDef(EnemyKind.TribeBrawler, 2)),
                    new WaveDef(2300f, new EnemySpawnDef(EnemyKind.Barricade, 2), new EnemySpawnDef(EnemyKind.TribeSpearman, 3), new EnemySpawnDef(EnemyKind.TribeHornist, 1))
                },
                BaseRewards = new List<LootRewardSummary>
                {
                    new LootRewardSummary("mat-stone", 3),
                    new LootRewardSummary("mat-iron-slag", 2)
                },
                LootTable = new List<string> { "mat-stone", "mat-iron-slag" }
            },
            new MissionDef
            {
                Id = "drake-caldera",
                Name = "3. Volcanic Drake of the Caldera",
                Blurb = "Venture into the molten caldera to confront the colossal Pyro Drake Titan.",
                Biome = "volcanic-caldera",
                GoldReward = 200,
                RecommendedGearScore = 25,
                Bpm = 120f,
                WorldLength = 3800f,
                GoalX = 3600f,
                UnlockAfter = "shadowmask-clash",
                Waves = new List<WaveDef>
                {
                    new WaveDef(1000f, new EnemySpawnDef(EnemyKind.WildBoar, 2)),
                    new WaveDef(2200f, new EnemySpawnDef(EnemyKind.DrakeTitan, 1))
                },
                BaseRewards = new List<LootRewardSummary>
                {
                    new LootRewardSummary("spear-iron", 1),
                    new LootRewardSummary("mat-iron-slag", 5)
                },
                LootTable = new List<string> { "mat-dragon-scale", "mat-mithril" }
            },
            new MissionDef
            {
                Id = "swamp-hunt",
                Name = "4. Wild Game in the Misty Swamps",
                Blurb = "Track giant boars and horned stags deep within the toxic fog.",
                Biome = "misty-swamp",
                GoldReward = 160,
                RecommendedGearScore = 30,
                Bpm = 120f,
                WorldLength = 3600f,
                GoalX = 3400f,
                UnlockAfter = "drake-caldera",
                Waves = new List<WaveDef>
                {
                    new WaveDef(800f, new EnemySpawnDef(EnemyKind.GiantBoar, 2)),
                    new WaveDef(1600f, new EnemySpawnDef(EnemyKind.Stag, 2), new EnemySpawnDef(EnemyKind.WildBoar, 2)),
                    new WaveDef(2500f, new EnemySpawnDef(EnemyKind.GiantBoar, 3))
                },
                LootTable = new List<string> { "mat-jerky", "mat-beast-hide" }
            },
            new MissionDef
            {
                Id = "jungle-gate",
                Name = "5. Breaking the Jungle Fort Gate",
                Blurb = "Assault the heavily fortified stone gate rampart guarded by archer watchtowers.",
                Biome = "jungle-fort",
                GoldReward = 220,
                RecommendedGearScore = 40,
                Bpm = 120f,
                WorldLength = 4200f,
                GoalX = 4000f,
                UnlockAfter = "swamp-hunt",
                Waves = new List<WaveDef>
                {
                    new WaveDef(900f, new EnemySpawnDef(EnemyKind.Watchtower, 1), new EnemySpawnDef(EnemyKind.TribeSwordsman, 3), new EnemySpawnDef(EnemyKind.TribeMage, 2)),
                    new WaveDef(2000f, new EnemySpawnDef(EnemyKind.StoneWall, 1), new EnemySpawnDef(EnemyKind.TribeArcher, 3), new EnemySpawnDef(EnemyKind.TribeSpearman, 3), new EnemySpawnDef(EnemyKind.TribeSkyrider, 2))
                },
                LootTable = new List<string> { "mat-stone", "mat-hardwood" }
            },
            new MissionDef
            {
                Id = "bastion-siege",
                Name = "6. Siege of the Iron Bastion",
                Blurb = "Overcome enemy catapult towers and cavalry lancers guarding the iron bastion.",
                Biome = "iron-bastion",
                GoldReward = 300,
                RecommendedGearScore = 55,
                Bpm = 120f,
                WorldLength = 5000f,
                GoalX = 4800f,
                UnlockAfter = "jungle-gate",
                Waves = new List<WaveDef>
                {
                    new WaveDef(1000f, new EnemySpawnDef(EnemyKind.TribeCavalry, 3)),
                    new WaveDef(2200f, new EnemySpawnDef(EnemyKind.CatapultTower, 1), new EnemySpawnDef(EnemyKind.TribeHammerer, 2)),
                    new WaveDef(3500f, new EnemySpawnDef(EnemyKind.StoneWall, 1), new EnemySpawnDef(EnemyKind.CatapultTower, 1), new EnemySpawnDef(EnemyKind.TribeSkyrider, 2))
                },
                LootTable = new List<string> { "mat-iron-slag", "mat-mithril" }
            },
            new MissionDef
            {
                Id = "iron-ridge",
                Name = "7. The Armored Mountain Behemoth",
                Blurb = "Face the armored iron behemoth in a battle of attrition on the mountain ridge.",
                Biome = "frozen-peaks",
                GoldReward = 380,
                RecommendedGearScore = 70,
                Bpm = 120f,
                WorldLength = 5600f,
                GoalX = 5400f,
                UnlockAfter = "bastion-siege",
                Waves = new List<WaveDef>
                {
                    new WaveDef(1200f, new EnemySpawnDef(EnemyKind.TribeCavalry, 2), new EnemySpawnDef(EnemyKind.TribeArcher, 3)),
                    new WaveDef(3000f, new EnemySpawnDef(EnemyKind.IronBehemoth, 1))
                },
                LootTable = new List<string> { "mat-iron-slag", "mat-mithril" }
            },
            new MissionDef
            {
                Id = "golem-altar",
                Name = "8. Awakening of the Ruin Colossus",
                Blurb = "The ancient Colossus Golem awakens at the sacred ruin altar. Unleash the power of the drum!",
                Biome = "ruin-altar",
                GoldReward = 500,
                RecommendedGearScore = 90,
                Bpm = 120f,
                WorldLength = 6800f,
                GoalX = 6600f,
                UnlockAfter = "iron-ridge",
                Waves = new List<WaveDef>
                {
                    new WaveDef(1500f, new EnemySpawnDef(EnemyKind.StoneWall, 1), new EnemySpawnDef(EnemyKind.TribeHammerer, 3)),
                    new WaveDef(3800f, new EnemySpawnDef(EnemyKind.ColossusGolem, 1))
                },
                LootTable = new List<string> { "mat-stone", "mat-mithril", "mat-dragon-scale" }
            }
        };

        public static MissionDef Get(string id)
        {
            return MissionDatabase.Get(id);
        }
    }

    public static class MissionDatabase
    {
        public static List<MissionDef> GetAll()
        {
            return MissionDef.Campaign;
        }

        public static MissionDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var match = MissionDef.Campaign.Find(m => m.Id == id);
            if (match != null) return match;

            if (id == "mission-1" || id == "stage-1" || id == "coast-hunt" || id == "m01-patata-plains" || id == "m01-coral-coast") return MissionDef.Campaign[0];
            if (id == "mission-2" || id == "stage-2" || id == "shadowmask-clash" || id == "m02-jungle-fort") return MissionDef.Campaign[1];
            if (id == "mission-3" || id == "stage-3" || id == "drake-caldera" || id == "m04-volcanic-caldera") return MissionDef.Campaign[2];
            if (id == "mission-4" || id == "stage-4" || id == "swamp-hunt" || id == "m03-misty-swamp") return MissionDef.Campaign[3];
            if (id == "mission-5" || id == "stage-5" || id == "jungle-gate") return MissionDef.Campaign[4];
            if (id == "mission-6" || id == "stage-6" || id == "bastion-siege") return MissionDef.Campaign[5];
            if (id == "mission-7" || id == "stage-7" || id == "iron-ridge") return MissionDef.Campaign[6];
            if (id == "mission-8" || id == "stage-8" || id == "golem-altar") return MissionDef.Campaign[7];

            if (id.StartsWith("bosshunt-"))
            {
                // Format: bosshunt-{bossKind}-lvl{level}
                var parts = id.Split('-');
                if (parts.Length >= 3)
                {
                    string bossStr = parts[1];
                    string lvlStr = parts[2].Replace("lvl", "");
                    int lvl = 1;
                    int.TryParse(lvlStr, out lvl);

                    EnemyKind kind = EnemyKind.DrakeTitan;
                    if (string.Equals(bossStr, "ironbehemoth", StringComparison.OrdinalIgnoreCase))
                    {
                        kind = EnemyKind.IronBehemoth;
                    }
                    else if (string.Equals(bossStr, "colossusgolem", StringComparison.OrdinalIgnoreCase))
                    {
                        kind = EnemyKind.ColossusGolem;
                    }

                    // Dynamically generate mission definition
                    var def = new MissionDef
                    {
                        Id = id,
                        Name = string.Format("Trial: {0} (Lv. {1})", kind.ToString(), lvl),
                        Biome = kind == EnemyKind.DrakeTitan ? "volcanic-caldera" : (kind == EnemyKind.IronBehemoth ? "frozen-peaks" : "ruin-altar"),
                        GoldReward = 200 + lvl * 80,
                        RecommendedGearScore = 20 + lvl * 15,
                        Bpm = 120f,
                        WorldLength = 3400f,
                        GoalX = 3200f,
                        Waves = new List<WaveDef>
                        {
                            new WaveDef(1800f, new EnemySpawnDef(kind, 1))
                        },
                        LootTable = new List<string> { "mat-mithril", "mat-dragon-scale" }
                    };
                    return def;
                }
            }

            return null;
        }
    }
}
