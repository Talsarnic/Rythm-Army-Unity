using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public static class BarracksManager
    {
        public static int GetMaxSquadSize(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Banner: return 1;
                case UnitClass.Spearman: return 6;
                case UnitClass.Swordsman: return 6;
                case UnitClass.Archer: return 6;
                case UnitClass.Cavalry: return 3;
                case UnitClass.Hammerer: return 3;
                case UnitClass.Hornist: return 3;
                case UnitClass.Skyrider: return 3;
                case UnitClass.Mage: return 3;
                case UnitClass.Brawler: return 3;
                default: return 3;
            }
        }

        public static int GetRecruitmentCost(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Spearman: return 30;
                case UnitClass.Swordsman: return 30;
                case UnitClass.Archer: return 40;
                case UnitClass.Cavalry: return 75;
                case UnitClass.Hammerer: return 80;
                case UnitClass.Brawler: return 90;
                case UnitClass.Skyrider: return 85;
                case UnitClass.Hornist: return 100;
                case UnitClass.Mage: return 110;
                default: return 50;
            }
        }

        public static bool CanRecruit(SaveData save, UnitClass unitClass)
        {
            if (save == null || unitClass == UnitClass.Banner) return false;

            int currentCount = save.Roster.Count(u => u.Class == unitClass);
            int maxLimit = GetMaxSquadSize(unitClass);
            if (currentCount >= maxLimit) return false;

            int cost = GetRecruitmentCost(unitClass);
            if (save.Gold < cost) return false;

            // Also requires 1 meat material for recruitment
            if (save.GetItemCount("mat-jerky") < 1) return false;

            return true;
        }

        public static UnitMember RecruitUnit(SaveData save, UnitClass unitClass, string defaultWeaponId = null)
        {
            if (!CanRecruit(save, unitClass)) return null;

            int cost = GetRecruitmentCost(unitClass);
            save.Gold -= cost;
            save.RemoveItem("mat-jerky", 1);

            // Starter weapon if not provided
            if (string.IsNullOrEmpty(defaultWeaponId))
            {
                switch (unitClass)
                {
                    case UnitClass.Spearman: defaultWeaponId = "spear-wood"; break;
                    case UnitClass.Swordsman: defaultWeaponId = "sword-wood"; break;
                    case UnitClass.Archer: defaultWeaponId = "bow-wood"; break;
                    case UnitClass.Brawler: defaultWeaponId = "arm-wood"; break;
                }
            }

            var newUnit = new UnitMember(Guid.NewGuid().ToString(), unitClass, 1, defaultWeaponId);
            save.Roster.Add(newUnit);
            return newUnit;
        }

        public static bool EquipItem(SaveData save, string unitId, string itemId)
        {
            if (save == null || string.IsNullOrEmpty(unitId) || string.IsNullOrEmpty(itemId)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            var item = ItemDef.Get(itemId);
            if (item == null || !item.Slot.HasValue) return false;

            // Check class restriction
            if (item.UsableBy.HasValue && item.UsableBy.Value != unit.Class) return false;

            // Check inventory
            if (save.GetItemCount(itemId) < 1) return false;

            // Unequip existing item in that slot
            string oldItemId = null;
            switch (item.Slot.Value)
            {
                case EquipSlot.Weapon:
                    oldItemId = unit.WeaponId;
                    unit.WeaponId = itemId;
                    break;
                case EquipSlot.Shield:
                    oldItemId = unit.ShieldId;
                    unit.ShieldId = itemId;
                    break;
                case EquipSlot.Helmet:
                    oldItemId = unit.HelmetId;
                    unit.HelmetId = itemId;
                    break;
                case EquipSlot.Mask:
                    oldItemId = unit.MaskId;
                    unit.MaskId = itemId;
                    break;
                case EquipSlot.Relic:
                    oldItemId = unit.RelicId;
                    unit.RelicId = itemId;
                    break;
            }

            save.RemoveItem(itemId, 1);
            if (!string.IsNullOrEmpty(oldItemId))
            {
                save.AddItem(oldItemId, 1);
            }

            return true;
        }

        public static bool UnequipSlot(SaveData save, string unitId, EquipSlot slot)
        {
            if (save == null || string.IsNullOrEmpty(unitId)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            string oldItemId = null;
            switch (slot)
            {
                case EquipSlot.Weapon:
                    oldItemId = unit.WeaponId;
                    unit.WeaponId = null;
                    break;
                case EquipSlot.Shield:
                    oldItemId = unit.ShieldId;
                    unit.ShieldId = null;
                    break;
                case EquipSlot.Helmet:
                    oldItemId = unit.HelmetId;
                    unit.HelmetId = null;
                    break;
                case EquipSlot.Mask:
                    oldItemId = unit.MaskId;
                    unit.MaskId = null;
                    break;
                case EquipSlot.Relic:
                    oldItemId = unit.RelicId;
                    unit.RelicId = null;
                    break;
            }

            if (!string.IsNullOrEmpty(oldItemId))
            {
                save.AddItem(oldItemId, 1);
            }

            return true;
        }

        public static bool LevelUpUnit(SaveData save, string unitId)
        {
            if (save == null || string.IsNullOrEmpty(unitId)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null || unit.Level >= 10) return false; // Max level 10

            int goldRequired = unit.Level * 40;
            if (save.Gold < goldRequired) return false;

            if (save.GetItemCount("mat-jerky") < unit.Level) return false;

            save.Gold -= goldRequired;
            save.RemoveItem("mat-jerky", unit.Level);
            unit.Level++;

            return true;
        }

        public static bool SetHeroUnit(SaveData save, string unitId)
        {
            if (save == null || string.IsNullOrEmpty(unitId)) return false;
            var target = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (target == null || target.Class == UnitClass.Banner) return false;

            foreach (var u in save.Roster)
            {
                u.IsHero = (u.Id == unitId);
            }
            return true;
        }
    }
}
