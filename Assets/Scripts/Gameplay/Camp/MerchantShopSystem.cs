using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public enum ShopItemCategory
    {
        Material,
        ElementalGem,
        BattleCharm,
        Blueprint,
        MysterySatchel
    }

    public class ShopItemDef
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ShopItemCategory Category { get; set; }
        public int BasePrice { get; set; }
        public int StockQuantity { get; set; } // -1 for unlimited
        public float DiscountPercentage { get; set; } // e.g. 0.25f for 25% off

        public int CurrentPrice
        {
            get
            {
                if (DiscountPercentage <= 0f) return BasePrice;
                int discounted = (int)Math.Round(BasePrice * (1.0f - DiscountPercentage));
                return Math.Max(1, discounted);
            }
        }

        public ShopItemDef(string id, string itemId, string name, string description, ShopItemCategory category, int basePrice, int stock = -1, float discount = 0f)
        {
            Id = id;
            ItemId = itemId;
            Name = name;
            Description = description;
            Category = category;
            BasePrice = basePrice;
            StockQuantity = stock;
            DiscountPercentage = discount;
        }
    }

    public class SatchelOpenResult
    {
        public string SatchelId;
        public List<LootRewardSummary> ItemsGranted = new List<LootRewardSummary>();
        public int GoldBonus;
    }

    /// <summary>
    /// Moonlighter-style Camp Merchant Shop & Barter Market.
    /// Supports buying bulk materials, elemental gems, battle charms, blueprints,
    /// selling surplus loot, daily discount rotations, and mystery satchels.
    /// </summary>
    public static class MerchantShopSystem
    {
        private static readonly Dictionary<string, ShopItemDef> _catalog = new Dictionary<string, ShopItemDef>();
        private static readonly Dictionary<string, int> _sellPrices = new Dictionary<string, int>();

        static MerchantShopSystem()
        {
            // Raw Materials
            RegisterShopItem(new ShopItemDef("shop-mat-timber", "mat-timber", "Birch Timber Bundle", "Common crafting timber.", ShopItemCategory.Material, 25));
            RegisterShopItem(new ShopItemDef("shop-mat-stone", "mat-stone", "Quarry Stone Block", "Common masonry stone.", ShopItemCategory.Material, 25));
            RegisterShopItem(new ShopItemDef("shop-mat-iron-slag", "mat-iron-slag", "Slag Alloy Ingot", "Uncommon metal alloy for weapons.", ShopItemCategory.Material, 60));
            RegisterShopItem(new ShopItemDef("shop-mat-hardwood", "mat-hardwood", "Aged Hardwood Plank", "Reinforced lumber for great shields and bows.", ShopItemCategory.Material, 75));
            RegisterShopItem(new ShopItemDef("shop-mat-beast-hide", "mat-beast-hide", "Tanned Beast Hide", "Sturdy leather for vanguards.", ShopItemCategory.Material, 40));
            RegisterShopItem(new ShopItemDef("shop-mat-mithril", "mat-mithril", "Mithril Ore Vein", "Rare gleaming ore for masterwork gear.", ShopItemCategory.Material, 180));
            RegisterShopItem(new ShopItemDef("shop-mat-dragon-scale", "mat-dragon-scale", "Titan Dragon Scale", "Epic scale imbued with draconic essence.", ShopItemCategory.Material, 400));
            RegisterShopItem(new ShopItemDef("shop-mat-ancient-ore", "mat-ancient-ore", "Ancient Star Ore", "Legendary extraterrestrial ore.", ShopItemCategory.Material, 850));

            // Elemental Gems (Tier 1 & 2)
            RegisterShopItem(new ShopItemDef("shop-gem-flame-1", "gem-flame-1", "Ruby Flame Shard", "Tier 1 Flame Gem (+10% Burn damage).", ShopItemCategory.ElementalGem, 150));
            RegisterShopItem(new ShopItemDef("shop-gem-frost-1", "gem-frost-1", "Sapphire Frost Shard", "Tier 1 Frost Gem (+15% Freeze slow).", ShopItemCategory.ElementalGem, 150));
            RegisterShopItem(new ShopItemDef("shop-gem-lightning-1", "gem-lightning-1", "Topaz Lightning Shard", "Tier 1 Lightning Gem (+10% Shock chain).", ShopItemCategory.ElementalGem, 150));
            RegisterShopItem(new ShopItemDef("shop-gem-earth-1", "gem-earth-1", "Emerald Earth Shard", "Tier 1 Earth Gem (+15% Structure shatter).", ShopItemCategory.ElementalGem, 150));
            RegisterShopItem(new ShopItemDef("shop-gem-flame-2", "gem-flame-2", "Brilliant Flame Core", "Tier 2 Flame Gem (+25% Burn damage).", ShopItemCategory.ElementalGem, 350));

            // Battle Charms
            RegisterShopItem(new ShopItemDef("shop-charm-healing", "charm-healing-tincture", "Herbal Healing Tincture", "Restores 35% HP across the squad.", ShopItemCategory.BattleCharm, 120));
            RegisterShopItem(new ShopItemDef("shop-charm-fever", "charm-fever-bell", "Harmonic Fever Bell", "Adds +3 combo and triggers Fever mode.", ShopItemCategory.BattleCharm, 200));
            RegisterShopItem(new ShopItemDef("shop-charm-smoke", "charm-smoke-bomb", "Alchemical Smoke Bomb", "Halves incoming damage for 1 measure.", ShopItemCategory.BattleCharm, 150));
            RegisterShopItem(new ShopItemDef("shop-charm-purify", "charm-purification-incense", "Purification Incense", "Cleanses all negative status ailments.", ShopItemCategory.BattleCharm, 100));
            RegisterShopItem(new ShopItemDef("shop-charm-horn", "charm-war-horn", "Courageous War Horn", "Boosts army damage by 25% for 2 measures.", ShopItemCategory.BattleCharm, 180));

            // Mystery Satchels
            RegisterShopItem(new ShopItemDef("shop-satchel-miner", "satchel-miner-pouch", "Prospector's Mineral Pouch", "Contains 3-5 assorted ores and alloys.", ShopItemCategory.MysterySatchel, 175));
            RegisterShopItem(new ShopItemDef("shop-satchel-elemental", "satchel-elemental-coffer", "Alchemist's Elemental Coffer", "Contains random elemental gems and dust.", ShopItemCategory.MysterySatchel, 320));
            RegisterShopItem(new ShopItemDef("shop-satchel-dragon", "satchel-dragon-hoard", "Dragon Hoard Satchel", "Contains rare dragon scales, star ore, and ancient relics.", ShopItemCategory.MysterySatchel, 650));

            // Blueprint Unlocks
            RegisterShopItem(new ShopItemDef("shop-bp-mithril-spear", "bp-mithril-spear", "Blueprint: Mithril Lance", "Unlocks masterwork lance crafting at Blacksmith.", ShopItemCategory.Blueprint, 300));
            RegisterShopItem(new ShopItemDef("shop-bp-dragon-blade", "bp-dragon-blade", "Blueprint: Dragonfire Blade", "Unlocks dragonblade crafting at Blacksmith.", ShopItemCategory.Blueprint, 500));

            // Initialize default sell prices (50% of buy value or fixed based on rarity)
            _sellPrices["mat-timber"] = 12;
            _sellPrices["mat-stone"] = 12;
            _sellPrices["mat-iron-slag"] = 30;
            _sellPrices["mat-hardwood"] = 35;
            _sellPrices["mat-beast-hide"] = 20;
            _sellPrices["mat-mithril"] = 90;
            _sellPrices["mat-dragon-scale"] = 200;
            _sellPrices["mat-ancient-ore"] = 425;
            _sellPrices["mat-jerky"] = 15;
            _sellPrices["mat-sun-cabbage"] = 15;
            _sellPrices["mat-healing-herb"] = 18;
            _sellPrices["spear-wood"] = 20;
            _sellPrices["sword-wood"] = 20;
            _sellPrices["shield-buckler"] = 25;
            _sellPrices["helm-leather"] = 25;
            _sellPrices["spear-iron"] = 60;
            _sellPrices["sword-iron"] = 60;
            _sellPrices["shield-iron"] = 70;
            _sellPrices["helm-iron"] = 70;
        }

        private static void RegisterShopItem(ShopItemDef item)
        {
            _catalog[item.Id] = item;
        }

        public static ShopItemDef GetShopItem(string shopItemId)
        {
            ShopItemDef item;
            if (_catalog.TryGetValue(shopItemId, out item)) return item;
            return null;
        }

        public static List<ShopItemDef> GetCatalog()
        {
            return new List<ShopItemDef>(_catalog.Values);
        }

        public static List<ShopItemDef> GetCatalogByCategory(ShopItemCategory category)
        {
            var result = new List<ShopItemDef>();
            foreach (var item in _catalog.Values)
            {
                if (item.Category == category) result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Applies a daily discount percentage to a specific shop item or rotates discounts.
        /// </summary>
        public static void SetDailyDiscount(string shopItemId, float discountPercentage)
        {
            var item = GetShopItem(shopItemId);
            if (item != null)
            {
                item.DiscountPercentage = Math.Max(0f, Math.Min(0.75f, discountPercentage));
            }
        }

        /// <summary>
        /// Purchases an item from the merchant, deducting gold and adding it to SaveData.
        /// </summary>
        public static bool BuyItem(SaveData save, string shopItemId, int quantity = 1, Random rng = null)
        {
            if (save == null || quantity <= 0) return false;
            var item = GetShopItem(shopItemId);
            if (item == null) return false;

            int totalCost = item.CurrentPrice * quantity;
            if (save.Gold < totalCost) return false;

            save.Gold -= totalCost;

            if (item.Category == ShopItemCategory.MysterySatchel)
            {
                for (int i = 0; i < quantity; i++)
                {
                    OpenSatchel(item.ItemId, save, rng);
                }
            }
            else
            {
                save.AddItem(item.ItemId, quantity);
            }

            return true;
        }

        /// <summary>
        /// Calculates the resale value for any item or material.
        /// </summary>
        public static int GetSellPrice(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            int price;
            if (_sellPrices.TryGetValue(itemId, out price)) return price;

            // Fallback: Check ItemDef database
            var itemDef = ItemDef.Get(itemId);
            if (itemDef != null)
            {
                switch (itemDef.Rarity)
                {
                    case ItemRarity.Common: return 20;
                    case ItemRarity.Uncommon: return 50;
                    case ItemRarity.Rare: return 120;
                    case ItemRarity.Epic: return 250;
                    case ItemRarity.Legendary: return 500;
                }
            }
            return 10;
        }

        /// <summary>
        /// Sells surplus materials or equipment from SaveData inventory for gold.
        /// </summary>
        public static bool SellItem(SaveData save, string itemId, int quantity = 1)
        {
            if (save == null || quantity <= 0) return false;
            int count = save.GetItemCount(itemId);
            if (count < quantity) return false;

            int unitPrice = GetSellPrice(itemId);
            int totalEarnings = unitPrice * quantity;

            if (save.RemoveItem(itemId, quantity))
            {
                save.Gold += totalEarnings;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Opens a mystery satchel and rolls rewards directly into save inventory.
        /// </summary>
        public static SatchelOpenResult OpenSatchel(string satchelId, SaveData save, Random rng = null)
        {
            var rand = rng ?? new Random();
            var result = new SatchelOpenResult { SatchelId = satchelId };

            if (satchelId == "satchel-miner-pouch")
            {
                int slagCount = rand.Next(2, 5);
                int timberCount = rand.Next(3, 6);
                int stoneCount = rand.Next(3, 6);
                save.AddItem("mat-iron-slag", slagCount);
                save.AddItem("mat-timber", timberCount);
                save.AddItem("mat-stone", stoneCount);
                result.ItemsGranted.Add(new LootRewardSummary("mat-iron-slag", slagCount));
                result.ItemsGranted.Add(new LootRewardSummary("mat-timber", timberCount));
                result.ItemsGranted.Add(new LootRewardSummary("mat-stone", stoneCount));
                if (rand.NextDouble() < 0.35)
                {
                    save.AddItem("mat-mithril", 1);
                    result.ItemsGranted.Add(new LootRewardSummary("mat-mithril", 1));
                }
            }
            else if (satchelId == "satchel-elemental-coffer")
            {
                string[] gems = new string[] { "gem-flame-1", "gem-frost-1", "gem-lightning-1", "gem-earth-1" };
                string pickedGem = gems[rand.Next(gems.Length)];
                save.AddItem(pickedGem, 1);
                save.AddItem("mat-mithril", 2);
                result.ItemsGranted.Add(new LootRewardSummary(pickedGem, 1));
                result.ItemsGranted.Add(new LootRewardSummary("mat-mithril", 2));
            }
            else if (satchelId == "satchel-dragon-hoard")
            {
                save.AddItem("mat-dragon-scale", 2);
                save.AddItem("mat-ancient-ore", 1);
                save.Gold += 200;
                result.ItemsGranted.Add(new LootRewardSummary("mat-dragon-scale", 2));
                result.ItemsGranted.Add(new LootRewardSummary("mat-ancient-ore", 1));
                result.GoldBonus = 200;
            }

            return result;
        }
    }
}
