using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public class OptimizationResultSummary
    {
        public int UnitsUpgraded;
        public float GearScoreBefore;
        public float GearScoreAfter;
        public float ScoreDelta { get { return GearScoreAfter - GearScoreBefore; } }
        public string SummaryMessage;
    }

    public static class EquipmentOptimizer
    {
        public static float CalculateRosterGearScore(SaveData save)
        {
            if (save == null || save.Roster == null) return 0f;
            float total = 0f;
            foreach (var unit in save.Roster)
            {
                var weapon = ItemDef.Get(unit.WeaponId);
                var shield = ItemDef.Get(unit.ShieldId);
                var helmet = ItemDef.Get(unit.HelmetId);
                if (weapon != null) total += weapon.CalculateGearScore();
                if (shield != null) total += shield.CalculateGearScore();
                if (helmet != null) total += helmet.CalculateGearScore();
            }
            return total;
        }

        public static bool OptimizeUnit(UnitMember unit, SaveData save)
        {
            if (unit == null || save == null) return false;
            bool changed = false;

            // 1. Optimize Weapon
            var availableWeapons = save.Inventory
                .Where(e => e.Quantity > 0)
                .Select(e => ItemDef.Get(e.ItemId))
                .Where(item => item != null && item.Category == ItemCategory.Weapon && (item.UsableBy == null || item.UsableBy == unit.Class))
                .OrderByDescending(item => item.CalculateGearScore())
                .ToList();

            if (availableWeapons.Count > 0)
            {
                var bestWeapon = availableWeapons[0];
                var currentWeapon = ItemDef.Get(unit.WeaponId);

                float currentScore = currentWeapon != null ? currentWeapon.CalculateGearScore() : -1f;
                float bestScore = bestWeapon.CalculateGearScore();

                if (bestScore > currentScore)
                {
                    // Unequip current
                    if (!string.IsNullOrEmpty(unit.WeaponId))
                    {
                        save.AddItem(unit.WeaponId, 1);
                    }
                    // Equip new
                    save.RemoveItem(bestWeapon.Id, 1);
                    unit.WeaponId = bestWeapon.Id;
                    changed = true;
                }
            }

            // 2. Optimize Shield (only if unit is Swordsman / uses shield)
            if (unit.Class == UnitClass.Swordsman)
            {
                var availableShields = save.Inventory
                    .Where(e => e.Quantity > 0)
                    .Select(e => ItemDef.Get(e.ItemId))
                    .Where(item => item != null && item.Category == ItemCategory.Shield && (item.UsableBy == null || item.UsableBy == unit.Class))
                    .OrderByDescending(item => item.CalculateGearScore())
                    .ToList();

                if (availableShields.Count > 0)
                {
                    var bestShield = availableShields[0];
                    var currentShield = ItemDef.Get(unit.ShieldId);

                    float currentScore = currentShield != null ? currentShield.CalculateGearScore() : -1f;
                    float bestScore = bestShield.CalculateGearScore();

                    if (bestScore > currentScore)
                    {
                        if (!string.IsNullOrEmpty(unit.ShieldId))
                        {
                            save.AddItem(unit.ShieldId, 1);
                        }
                        save.RemoveItem(bestShield.Id, 1);
                        unit.ShieldId = bestShield.Id;
                        changed = true;
                    }
                }
            }

            // 3. Optimize Helmet
            var availableHelmets = save.Inventory
                .Where(e => e.Quantity > 0)
                .Select(e => ItemDef.Get(e.ItemId))
                .Where(item => item != null && item.Category == ItemCategory.Helmet)
                .OrderByDescending(item => item.CalculateGearScore())
                .ToList();

            if (availableHelmets.Count > 0)
            {
                var bestHelm = availableHelmets[0];
                var currentHelm = ItemDef.Get(unit.HelmetId);

                float currentScore = currentHelm != null ? currentHelm.CalculateGearScore() : -1f;
                float bestScore = bestHelm.CalculateGearScore();

                if (bestScore > currentScore)
                {
                    if (!string.IsNullOrEmpty(unit.HelmetId))
                    {
                        save.AddItem(unit.HelmetId, 1);
                    }
                    save.RemoveItem(bestHelm.Id, 1);
                    unit.HelmetId = bestHelm.Id;
                    changed = true;
                }
            }

            return changed;
        }

        public static OptimizationResultSummary OptimizeArmyWithSummary(SaveData save)
        {
            var summary = new OptimizationResultSummary();
            if (save == null || save.Roster == null)
            {
                summary.SummaryMessage = "No roster found to optimize.";
                return summary;
            }

            summary.GearScoreBefore = CalculateRosterGearScore(save);
            int upgradedCount = 0;

            foreach (var unit in save.Roster)
            {
                if (OptimizeUnit(unit, save))
                {
                    upgradedCount++;
                }
            }

            summary.UnitsUpgraded = upgradedCount;
            summary.GearScoreAfter = CalculateRosterGearScore(save);

            if (summary.ScoreDelta > 0)
            {
                summary.SummaryMessage = string.Format("Optimized! +{0:0} Gear Score gained across {1} units.", summary.ScoreDelta, upgradedCount);
            }
            else
            {
                summary.SummaryMessage = "Roster is already equipped with optimal gear.";
            }

            return summary;
        }

        public static void OptimizeArmy(SaveData save)
        {
            OptimizeArmyWithSummary(save);
        }

        public static void OptimizeAll(SaveData save)
        {
            OptimizeArmyWithSummary(save);
        }
    }
}
