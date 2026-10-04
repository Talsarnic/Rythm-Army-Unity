using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Gameplay.Battle
{
    public class LootDropEntry
    {
        public string ItemId;
        public float DropChance; // 0.0 to 1.0
        public int MinQty;
        public int MaxQty;

        public LootDropEntry(string itemId, float dropChance, int minQty = 1, int maxQty = 1)
        {
            ItemId = itemId;
            DropChance = dropChance;
            MinQty = minQty;
            MaxQty = maxQty;
        }
    }

    public static class LootSystem
    {
        public static readonly Dictionary<EnemyKind, List<LootDropEntry>> EnemyLootTables = new Dictionary<EnemyKind, List<LootDropEntry>>
        {
            {
                EnemyKind.PlainsRunner,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-jerky", 0.90f, 1, 2),
                    new LootDropEntry("mat-sun-cabbage", 0.40f, 1, 1),
                    new LootDropEntry("mat-beast-hide", 0.30f, 1, 1)
                }
            },
            {
                EnemyKind.SandCrab,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-stone", 0.70f, 1, 2),
                    new LootDropEntry("mat-jerky", 0.50f, 1, 1)
                }
            },
            {
                EnemyKind.Stag,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-jerky", 1.0f, 2, 4),
                    new LootDropEntry("mat-beast-hide", 0.80f, 2, 3),
                    new LootDropEntry("mat-hardwood", 0.60f, 1, 2)
                }
            },
            {
                EnemyKind.WildBoar,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-jerky", 1.0f, 2, 3),
                    new LootDropEntry("mat-beast-hide", 0.70f, 1, 2)
                }
            },
            {
                EnemyKind.GiantBoar,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-jerky", 1.0f, 3, 5),
                    new LootDropEntry("mat-beast-hide", 0.90f, 2, 4),
                    new LootDropEntry("mat-iron-slag", 0.50f, 1, 2)
                }
            },
            {
                EnemyKind.Barricade,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-timber", 1.0f, 2, 4),
                    new LootDropEntry("mat-hardwood", 0.40f, 1, 2)
                }
            },
            {
                EnemyKind.StoneWall,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-stone", 1.0f, 3, 6),
                    new LootDropEntry("mat-iron-slag", 0.60f, 1, 3)
                }
            },
            {
                EnemyKind.Watchtower,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-timber", 1.0f, 3, 5),
                    new LootDropEntry("mat-hardwood", 0.80f, 2, 3),
                    new LootDropEntry("bow-wood", 0.25f, 1, 1)
                }
            },
            {
                EnemyKind.CatapultTower,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-stone", 1.0f, 4, 8),
                    new LootDropEntry("mat-iron-slag", 0.80f, 2, 4),
                    new LootDropEntry("arm-crusher", 0.15f, 1, 1)
                }
            },
            {
                EnemyKind.TribeBanner,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-timber", 0.80f, 1, 2),
                    new LootDropEntry("mat-wind-feather", 0.50f, 1, 2)
                }
            },
            {
                EnemyKind.TribeSpearman,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-timber", 0.60f, 1, 2),
                    new LootDropEntry("spear-wood", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeSwordsman,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-iron-slag", 0.60f, 1, 2),
                    new LootDropEntry("shield-buckler", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeArcher,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-timber", 0.60f, 1, 2),
                    new LootDropEntry("mat-wind-feather", 0.35f, 1, 1),
                    new LootDropEntry("bow-wood", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeCavalry,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-beast-hide", 0.70f, 1, 3),
                    new LootDropEntry("spear-iron", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeHammerer,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-iron-slag", 0.75f, 2, 3),
                    new LootDropEntry("hammer-iron", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeHornist,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-hardwood", 0.65f, 1, 2),
                    new LootDropEntry("horn-wood", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeSkyrider,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-wind-feather", 0.80f, 2, 4),
                    new LootDropEntry("spear-iron", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.TribeMage,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-hardwood", 0.60f, 1, 2),
                    new LootDropEntry("staff-wood", 0.25f, 1, 1)
                }
            },
            {
                EnemyKind.TribeBrawler,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-iron-slag", 0.70f, 1, 3),
                    new LootDropEntry("fist-iron", 0.20f, 1, 1)
                }
            },
            {
                EnemyKind.DrakeTitan,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-dragon-scale", 1.0f, 2, 4),
                    new LootDropEntry("mat-mithril", 0.80f, 2, 4),
                    new LootDropEntry("spear-storm", 0.30f, 1, 1)
                }
            },
            {
                EnemyKind.ColossusGolem,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-stone", 1.0f, 8, 15),
                    new LootDropEntry("mat-mithril", 1.0f, 3, 5),
                    new LootDropEntry("arm-crusher", 0.40f, 1, 1)
                }
            },
            {
                EnemyKind.IronBehemoth,
                new List<LootDropEntry>
                {
                    new LootDropEntry("mat-iron-slag", 1.0f, 4, 8),
                    new LootDropEntry("mat-mithril", 0.70f, 2, 3),
                    new LootDropEntry("shield-vanguard-core", 0.25f, 1, 1)
                }
            }
        };

        public static int RollCurrencyDrop(EnemyKind kind, Random rng = null)
        {
            rng = rng ?? new Random();
            var category = EnemyMetadata.GetCategory(kind);
            
            switch (category)
            {
                case EnemyCategory.Wildlife:
                    // Wildlife drops smaller currency pouches (3 - 12)
                    switch (kind)
                    {
                        case EnemyKind.PlainsRunner: return rng.Next(2, 6);
                        case EnemyKind.SandCrab: return rng.Next(3, 7);
                        case EnemyKind.WildBoar: return rng.Next(5, 10);
                        case EnemyKind.Stag: return rng.Next(8, 15);
                        case EnemyKind.GiantBoar: return rng.Next(10, 20);
                        default: return rng.Next(3, 8);
                    }

                case EnemyCategory.RivalClan:
                    // Rival Clan warriors drop soldier coins (8 - 25)
                    switch (kind)
                    {
                        case EnemyKind.TribeBanner: return rng.Next(15, 30);
                        case EnemyKind.TribeCavalry:
                        case EnemyKind.TribeHammerer:
                        case EnemyKind.TribeSkyrider: return rng.Next(12, 25);
                        case EnemyKind.TribeMage:
                        case EnemyKind.TribeHornist:
                        case EnemyKind.TribeBrawler: return rng.Next(10, 22);
                        default: return rng.Next(8, 18);
                    }

                case EnemyCategory.Fortification:
                    // Fortifications yield hoard caches (20 - 60)
                    switch (kind)
                    {
                        case EnemyKind.Barricade: return rng.Next(15, 30);
                        case EnemyKind.StoneWall: return rng.Next(25, 45);
                        case EnemyKind.Watchtower: return rng.Next(30, 50);
                        case EnemyKind.CatapultTower: return rng.Next(40, 75);
                        default: return rng.Next(20, 40);
                    }

                case EnemyCategory.Boss:
                    // Colossal Bosses yield massive currency bounties (100 - 300)
                    switch (kind)
                    {
                        case EnemyKind.DrakeTitan: return rng.Next(150, 250);
                        case EnemyKind.ColossusGolem: return rng.Next(180, 300);
                        case EnemyKind.IronBehemoth: return rng.Next(120, 220);
                        default: return rng.Next(100, 200);
                    }

                default:
                    return rng.Next(5, 15);
            }
        }

        public static List<LootRewardSummary> RollDrops(EnemyKind kind, Random rng = null)
        {
            rng = rng ?? new Random();
            var drops = new List<LootRewardSummary>();

            List<LootDropEntry> table;
            if (EnemyLootTables.TryGetValue(kind, out table))
            {
                foreach (var entry in table)
                {
                    double roll = rng.NextDouble();
                    if (roll <= entry.DropChance)
                    {
                        int qty = entry.MinQty == entry.MaxQty 
                            ? entry.MinQty 
                            : rng.Next(entry.MinQty, entry.MaxQty + 1);

                        drops.Add(new LootRewardSummary(entry.ItemId, qty));
                    }
                }
            }

            return drops;
        }
    }
}
