using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay.Narrative;
using RhythmArmy.UI;

namespace RhythmArmy.Gameplay.Camp
{
    public enum CampBuilding
    {
        TownSquare,
        Barracks,
        Blacksmith,
        SpiritAltar,
        SacredTree,
        CampfireFeast,
        HeroShrine,
        MerchantMarket,
        MissionWarTable
    }

    /// <summary>
    /// Master Camp Hub Controller managing player save state, barracks recruitment,
    /// blacksmith crafting, spirit altar subspecies evolution, camp rhythm minigames, and mission deployment.
    /// </summary>
    public class CampController
    {
        public SaveData SaveData { get; private set; }
        public CampHUDState HUDState { get; private set; }
        public DialoguePlayer Dialogue { get; private set; }
        public CampMinigameManager Minigames { get; private set; }
        public CampBuilding CurrentBuilding { get; private set; }

        public event Action<string> OnNotificationMessage;
        public event Action<UnitMember> OnUnitRecruited;
        public event Action<UnitMember, Subspecies> OnUnitEvolved;
        public event Action<string, int> OnItemCrafted;
        public event Action<MinigameResult> OnMinigameCompleted;

        public CampController(SaveData saveData = null)
        {
            SaveData = saveData != null ? saveData : SaveSystem.CreateInitialSave();
            HUDState = new CampHUDState();
            Dialogue = new DialoguePlayer();
            Minigames = new CampMinigameManager();
            CurrentBuilding = CampBuilding.TownSquare;

            Minigames.OnMinigameFinished += result =>
            {
                Minigames.ApplyRewardsToSave(SaveData, result);
                SyncHUD();
                if (OnMinigameCompleted != null)
                {
                    OnMinigameCompleted.Invoke(result);
                }
                if (OnNotificationMessage != null)
                {
                    OnNotificationMessage.Invoke(string.Format("Minigame finished! Collected {0} reward items.", result.RewardsEarned.Count));
                }
            };

            SyncHUD();
        }

        public void SetBuilding(CampBuilding building)
        {
            CurrentBuilding = building;
            SyncHUD();
        }

        public bool RecruitUnit(UnitClass unitClass)
        {
            var unit = BarracksManager.RecruitUnit(SaveData, unitClass);
            if (unit != null)
            {
                if (OnUnitRecruited != null) OnUnitRecruited(unit);
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("Recruited {0} into the army!", unit.Name));
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Failed to recruit: insufficient gold, supplies, or squad limit reached.");
            return false;
        }

        public bool DismissUnit(string unitId)
        {
            var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null || unit.Class == UnitClass.Banner) return false;

            // Unequip all items before dismissing
            if (!string.IsNullOrEmpty(unit.WeaponId)) SaveData.AddItem(unit.WeaponId, 1);
            if (!string.IsNullOrEmpty(unit.ShieldId)) SaveData.AddItem(unit.ShieldId, 1);
            if (!string.IsNullOrEmpty(unit.HelmetId)) SaveData.AddItem(unit.HelmetId, 1);

            SaveData.Roster.Remove(unit);
            if (OnNotificationMessage != null) OnNotificationMessage("Unit dismissed.");
            SyncHUD();
            return true;
        }

        public bool EquipItem(string unitId, EquipSlot slot, string itemId)
        {
            return BarracksManager.EquipItem(SaveData, unitId, itemId);
        }

        public bool UnequipSlot(string unitId, EquipSlot slot)
        {
            return BarracksManager.UnequipSlot(SaveData, unitId, slot);
        }

        public bool AutoOptimizeAllUnits()
        {
            EquipmentOptimizer.OptimizeAll(SaveData);
            if (OnNotificationMessage != null) OnNotificationMessage("Army equipment optimized for maximum combat score!");
            SyncHUD();
            return true;
        }

        public bool CraftItem(string recipeId)
        {
            var recipe = BlacksmithCrafting.Recipes.FirstOrDefault(r => r.ResultItemId == recipeId);
            if (recipe == null) return false;

            bool success = BlacksmithCrafting.CraftItem(recipe, SaveData);
            if (success)
            {
                if (OnItemCrafted != null) OnItemCrafted(recipe.ResultItemId, 1);
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("Crafted {0}!", recipe.ResultItemId));
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Crafting failed: missing materials or gold.");
            return false;
        }

        public bool EvolveUnit(string unitId, Subspecies targetSpecies)
        {
            bool success = AltarEvolution.EvolveUnit(SaveData, unitId, targetSpecies);
            if (success)
            {
                var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
                if (unit != null)
                {
                    if (OnUnitEvolved != null) OnUnitEvolved(unit, targetSpecies);
                    if (OnNotificationMessage != null) OnNotificationMessage(string.Format("{0} evolved into {1}!", unit.Name, targetSpecies));
                }
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Evolution failed: requirements not met.");
            return false;
        }

        public bool AwakenUnitTier(string unitId)
        {
            var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            if (MaterTreeAwakening.AwakenTier(SaveData, unitId))
            {
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("{0} awakened to Tier {1}! +5 Mastery Points unlocked.", unit.Name, unit.AwakeningTier));
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Cannot awaken: missing level requirement, materials, or gold.");
            return false;
        }

        public bool AllocateMastery(string unitId, MasteryStatType stat)
        {
            var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            if (MaterTreeAwakening.AllocateMasteryPoint(unit, stat))
            {
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("Allocated point to {0} Mastery for {1}.", stat, unit.Name));
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("No available mastery points to allocate.");
            return false;
        }

        public bool RespecUnitMastery(string unitId)
        {
            var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            if (MaterTreeAwakening.RespecMastery(SaveData, unitId))
            {
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("Mastery points reset for {0}.", unit.Name));
                SyncHUD();
                return true;
            }
            return false;
        }

        public bool FuseSecondarySubspecies(string unitId, Subspecies secondary)
        {
            var unit = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            if (MaterTreeAwakening.FuseSecondarySubspecies(SaveData, unitId, secondary))
            {
                if (OnNotificationMessage != null) OnNotificationMessage(string.Format("Fused {0} essence into {1}!", secondary, unit.Name));
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Cannot fuse: Requires Tier 3 Awakening, Mithril, and Gold.");
            return false;
        }

        public bool SetHeroUnit(string unitId)
        {
            bool success = BarracksManager.SetHeroUnit(SaveData, unitId);
            if (success)
            {
                var hero = SaveData.Roster.FirstOrDefault(u => u.Id == unitId);
                if (OnNotificationMessage != null && hero != null)
                {
                    OnNotificationMessage(string.Format("{0} designated as Army Hero Champion!", hero.Name));
                }
                SyncHUD();
            }
            return success;
        }

        public bool ConsumeFeast(string foodItemId)
        {
            var food = ItemDef.Get(foodItemId);
            if (food == null || food.Category != ItemCategory.Food) return false;

            if (SaveData.RemoveItem(foodItemId, 1))
            {
                SaveData.ActiveMealBuffId = foodItemId;
                if (OnNotificationMessage != null)
                {
                    OnNotificationMessage(string.Format("The army enjoyed {0}! Squad health and combat stats boosted for the next battle.", food.Name));
                }
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("No dishes remaining to feast upon.");
            return false;
        }

        public bool BuyFromMerchant(string shopItemId, int quantity = 1, Random rng = null)
        {
            if (MerchantShopSystem.BuyItem(SaveData, shopItemId, quantity, rng))
            {
                var item = MerchantShopSystem.GetShopItem(shopItemId);
                if (OnNotificationMessage != null && item != null)
                {
                    OnNotificationMessage(string.Format("Purchased {0}x {1} from High Merchant Leo.", quantity, item.Name));
                }
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Cannot purchase: insufficient gold or invalid item.");
            return false;
        }

        public bool SellToMerchant(string itemId, int quantity = 1)
        {
            if (MerchantShopSystem.SellItem(SaveData, itemId, quantity))
            {
                int price = MerchantShopSystem.GetSellPrice(itemId) * quantity;
                if (OnNotificationMessage != null)
                {
                    OnNotificationMessage(string.Format("Sold {0}x item for {1} gold.", quantity, price));
                }
                SyncHUD();
                return true;
            }

            if (OnNotificationMessage != null) OnNotificationMessage("Cannot sell: item not found in inventory.");
            return false;
        }

        public void StartMinigame(MinigameType type)
        {
            Minigames.StartMinigame(type);
        }

        public bool SubmitMinigameInput(DrumId drum)
        {
            return Minigames.SubmitDrumInput(drum);
        }

        public void PlayDialogue(string sequenceId)
        {
            var seq = DialogueDatabase.Get(sequenceId);
            if (seq != null)
            {
                Dialogue.Play(seq);
            }
        }

        public void SyncHUD()
        {
            HUDState.RefreshFromSave(SaveData);
        }
    }
}
