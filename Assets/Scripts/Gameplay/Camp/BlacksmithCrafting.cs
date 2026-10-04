using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public class CraftingRecipe
    {
        public string OutputItemId;
        public string ResultItemId { get { return OutputItemId; } set { OutputItemId = value; } }
        public int GoldCost;
        public Dictionary<string, int> RequiredMaterials;
        public string RequiredMissionUnlock;

        public CraftingRecipe(string outputItemId, int goldCost, Dictionary<string, int> requiredMaterials, string requiredMissionUnlock = null)
        {
            OutputItemId = outputItemId;
            GoldCost = goldCost;
            RequiredMaterials = requiredMaterials;
            RequiredMissionUnlock = requiredMissionUnlock;
        }
    }

    public static class BlacksmithCrafting
    {
        public static List<CraftingRecipe> AllRecipes { get { return Recipes; } }

        public static readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>
        {
            new CraftingRecipe("spear-wood", 15, new Dictionary<string, int> { { "mat-timber", 2 } }),
            new CraftingRecipe("spear-iron", 50, new Dictionary<string, int> { { "mat-timber", 3 }, { "mat-iron-slag", 2 } }, "shadowmask-clash"),
            new CraftingRecipe("spear-storm", 250, new Dictionary<string, int> { { "mat-mithril", 4 }, { "mat-dragon-scale", 2 } }, "drake-sanctum"),

            new CraftingRecipe("sword-wood", 15, new Dictionary<string, int> { { "mat-timber", 2 } }),
            new CraftingRecipe("sword-iron", 50, new Dictionary<string, int> { { "mat-timber", 2 }, { "mat-iron-slag", 2 } }, "shadowmask-clash"),

            new CraftingRecipe("bow-wood", 15, new Dictionary<string, int> { { "mat-timber", 2 } }),
            new CraftingRecipe("bow-cyclone", 180, new Dictionary<string, int> { { "mat-timber", 4 }, { "mat-wind-feather", 2 } }, "shadowmask-clash"),

            new CraftingRecipe("arm-wood", 20, new Dictionary<string, int> { { "mat-timber", 3 } }),
            new CraftingRecipe("arm-crusher", 160, new Dictionary<string, int> { { "mat-iron-slag", 4 }, { "mat-stone", 3 } }, "golem-bastion"),

            new CraftingRecipe("shield-buckler", 20, new Dictionary<string, int> { { "mat-timber", 2 }, { "mat-stone", 1 } }),
            new CraftingRecipe("shield-vanguard-core", 220, new Dictionary<string, int> { { "mat-iron-slag", 4 }, { "mat-dragon-scale", 2 } }, "drake-sanctum"),

            new CraftingRecipe("helm-leather", 20, new Dictionary<string, int> { { "mat-beast-hide", 2 } }),
            new CraftingRecipe("helm-iron", 60, new Dictionary<string, int> { { "mat-iron-slag", 3 } }, "shadowmask-clash")
        };

        public static bool IsRecipeUnlocked(CraftingRecipe recipe, SaveData save)
        {
            if (string.IsNullOrEmpty(recipe.RequiredMissionUnlock)) return true;
            return save.CompletedMissions.Contains(recipe.RequiredMissionUnlock);
        }

        public static bool CanCraft(CraftingRecipe recipe, SaveData save)
        {
            if (save == null || recipe == null) return false;
            if (!IsRecipeUnlocked(recipe, save)) return false;
            if (save.Gold < recipe.GoldCost) return false;

            foreach (var kvp in recipe.RequiredMaterials)
            {
                if (save.GetItemCount(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CraftItem(CraftingRecipe recipe, SaveData save)
        {
            if (!CanCraft(recipe, save)) return false;

            // Deduct Gold
            save.Gold -= recipe.GoldCost;

            // Deduct Materials
            foreach (var kvp in recipe.RequiredMaterials)
            {
                save.RemoveItem(kvp.Key, kvp.Value);
            }

            // Add Crafted Item
            save.AddItem(recipe.OutputItemId, 1);
            return true;
        }

        public static bool Craft(CraftingRecipe recipe, SaveData save)
        {
            return CraftItem(recipe, save);
        }
    }
}
