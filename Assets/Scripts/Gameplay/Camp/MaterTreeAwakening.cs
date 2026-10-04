using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public enum MasteryStatType
    {
        Attack,     // +2% Base Damage per level
        Defense,    // +2% Armor/Mitigation per level
        Health,     // +3% Max HP per level
        Speed       // +1% March & Recovery Speed per level
    }

    public class AwakeningTierDef
    {
        public int Tier;
        public int MaxMasteryPoints;
        public int GoldCost;
        public Dictionary<string, int> RequiredMaterials;
        public int MinUnitLevel;

        public AwakeningTierDef(int tier, int maxPoints, int goldCost, Dictionary<string, int> materials, int minLevel)
        {
            Tier = tier;
            MaxMasteryPoints = maxPoints;
            GoldCost = goldCost;
            RequiredMaterials = materials;
            MinUnitLevel = minLevel;
        }
    }

    /// <summary>
    /// Manages the Tree of Life / Spirit Altar Awakening rituals, allowing units to:
    /// 1. Unlock Awakening Tiers (1 to 5) to expand Mastery Point capacity.
    /// 2. Allocate Mastery Points into Attack, Defense, Health, and Speed.
    /// 3. Respec Mastery points freely or for a nominal fee.
    /// 4. Fuse secondary Subspecies memories for hybrid perk combinations.
    /// </summary>
    public static class MaterTreeAwakening
    {
        public const int MaxTier = 5;
        public const int PointsPerTier = 5;

        public static readonly Dictionary<int, AwakeningTierDef> TierDefinitions = new Dictionary<int, AwakeningTierDef>
        {
            {
                1,
                new AwakeningTierDef(1, 5, 100, new Dictionary<string, int> { { "mat-timber", 5 }, { "mat-stone", 5 } }, 2)
            },
            {
                2,
                new AwakeningTierDef(2, 10, 200, new Dictionary<string, int> { { "mat-sun-cabbage", 4 }, { "mat-iron-slag", 4 } }, 3)
            },
            {
                3,
                new AwakeningTierDef(3, 15, 350, new Dictionary<string, int> { { "mat-beast-hide", 4 }, { "mat-hardwood", 3 } }, 4)
            },
            {
                4,
                new AwakeningTierDef(4, 20, 500, new Dictionary<string, int> { { "mat-mithril", 3 }, { "mat-iron-slag", 5 } }, 5)
            },
            {
                5,
                new AwakeningTierDef(5, 25, 800, new Dictionary<string, int> { { "mat-dragon-scale", 3 }, { "mat-mithril", 4 } }, 6)
            }
        };

        public static int GetAllocatedPoints(UnitMember unit)
        {
            if (unit == null) return 0;
            return unit.AttackMastery + unit.DefenseMastery + unit.HealthMastery + unit.SpeedMastery;
        }

        public static int GetMaxAllowedPoints(UnitMember unit)
        {
            if (unit == null || unit.AwakeningTier <= 0) return 0;
            return unit.AwakeningTier * PointsPerTier;
        }

        public static int GetAvailablePoints(UnitMember unit)
        {
            return Math.Max(0, GetMaxAllowedPoints(unit) - GetAllocatedPoints(unit));
        }

        public static bool CanAwakenTier(SaveData save, string unitId)
        {
            if (save == null || string.IsNullOrEmpty(unitId)) return false;
            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null || unit.Class == UnitClass.Banner) return false;

            int nextTier = unit.AwakeningTier + 1;
            if (nextTier > MaxTier) return false;

            AwakeningTierDef def;
            if (!TierDefinitions.TryGetValue(nextTier, out def)) return false;

            if (unit.Level < def.MinUnitLevel) return false;
            if (save.Gold < def.GoldCost) return false;

            foreach (var kvp in def.RequiredMaterials)
            {
                if (save.GetItemCount(kvp.Key) < kvp.Value) return false;
            }

            return true;
        }

        public static bool AwakenTier(SaveData save, string unitId)
        {
            if (!CanAwakenTier(save, unitId)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            int nextTier = unit.AwakeningTier + 1;
            var def = TierDefinitions[nextTier];

            save.Gold -= def.GoldCost;
            foreach (var kvp in def.RequiredMaterials)
            {
                save.RemoveItem(kvp.Key, kvp.Value);
            }

            unit.AwakeningTier = nextTier;
            return true;
        }

        public static bool AllocateMasteryPoint(UnitMember unit, MasteryStatType stat)
        {
            if (unit == null) return false;
            if (GetAvailablePoints(unit) <= 0) return false;

            switch (stat)
            {
                case MasteryStatType.Attack:
                    unit.AttackMastery++;
                    break;
                case MasteryStatType.Defense:
                    unit.DefenseMastery++;
                    break;
                case MasteryStatType.Health:
                    unit.HealthMastery++;
                    break;
                case MasteryStatType.Speed:
                    unit.SpeedMastery++;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static bool RespecMastery(SaveData save, string unitId)
        {
            if (save == null || string.IsNullOrEmpty(unitId)) return false;
            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            unit.AttackMastery = 0;
            unit.DefenseMastery = 0;
            unit.HealthMastery = 0;
            unit.SpeedMastery = 0;
            return true;
        }

        public static bool CanFuseSecondarySubspecies(SaveData save, string unitId, Subspecies secondarySubspecies)
        {
            if (save == null || string.IsNullOrEmpty(unitId) || secondarySubspecies == Subspecies.Normal) return false;
            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null || unit.Class == UnitClass.Banner) return false;
            if (unit.AwakeningTier < 3) return false; // Requires Tier 3 Awakening
            if (unit.Subspecies == secondarySubspecies) return false; // Cannot fuse same primary

            int fusionGoldCost = 250;
            if (save.Gold < fusionGoldCost) return false;
            if (save.GetItemCount("mat-mithril") < 1) return false;

            return true;
        }

        public static bool FuseSecondarySubspecies(SaveData save, string unitId, Subspecies secondarySubspecies)
        {
            if (!CanFuseSecondarySubspecies(save, unitId, secondarySubspecies)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            save.Gold -= 250;
            save.RemoveItem("mat-mithril", 1);
            unit.SecondaryFusion = secondarySubspecies;
            return true;
        }
    }
}
