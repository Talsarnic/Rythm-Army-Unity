using System;
using System.Collections.Generic;

namespace RhythmArmy.Core.Data
{
    public enum ItemType
    {
        Weapon,
        Shield,
        Helmet,
        Material,
        Food,
        Mask,
        Relic
    }

    public enum ItemCategory
    {
        Weapon,
        Shield,
        Helmet,
        Material,
        Food,
        Mask,
        Relic
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum DamageElement
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Sonic
    }

    [Serializable]
    public class ItemDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemCategory Category { get; set; }
        public ItemType Type
        {
            get { return (ItemType)Category; }
            set { Category = (ItemCategory)value; }
        }
        public ItemRarity Rarity { get; set; }
        public UnitClass? UsableBy { get; set; } // Null if usable by all or not equippable
        public EquipSlot? Slot { get; set; }

        public int GearScore
        {
            get { return (int)CalculateGearScore(); }
        }

        // Combat Stats
        public float MinDamage { get; set; }
        public float MaxDamage { get; set; }
        public float Defense { get; set; }
        public float HealthBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float AttackRange { get; set; }
        public float CritChance { get; set; }
        public float KnockbackPower { get; set; }
        public float StructureBonus { get; set; } // Multiplier against walls/towers
        public DamageElement Element { get; set; }

        // Crafting / Economics
        public int GoldCost { get; set; }
        public Dictionary<string, int> Recipe { get; set; }

        public ItemDef()
        {
            StructureBonus = 1.0f;
            Element = DamageElement.Physical;
            Recipe = new Dictionary<string, int>();
        }

        public float CalculateGearScore()
        {
            float avgDmg = (MinDamage + MaxDamage) / 2f;
            return (avgDmg * 2.5f) + (Defense * 2.0f) + (HealthBonus * 0.1f) + (AttackRange * 0.05f) + (CritChance * 100f);
        }

        public static readonly Dictionary<string, ItemDef> Database = new Dictionary<string, ItemDef>
        {
            // Starter Weapons
            {
                "spear-wood", new ItemDef
                {
                    Id = "spear-wood", Name = "Wooden Spear", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Common, UsableBy = UnitClass.Spearman, Slot = EquipSlot.Weapon,
                    MinDamage = 4f, MaxDamage = 8f, AttackRange = 380f, CritChance = 0.05f
                }
            },
            {
                "spear-iron", new ItemDef
                {
                    Id = "spear-iron", Name = "Iron Spear", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Uncommon, UsableBy = UnitClass.Spearman, Slot = EquipSlot.Weapon,
                    MinDamage = 9f, MaxDamage = 16f, AttackRange = 400f, CritChance = 0.08f
                }
            },
            {
                "spear-storm", new ItemDef
                {
                    Id = "spear-storm", Name = "Thunder Lance", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Epic, UsableBy = UnitClass.Spearman, Slot = EquipSlot.Weapon,
                    MinDamage = 22f, MaxDamage = 35f, AttackRange = 430f, CritChance = 0.15f,
                    Element = DamageElement.Lightning
                }
            },
            {
                "sword-wood", new ItemDef
                {
                    Id = "sword-wood", Name = "Wooden Blade", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Common, UsableBy = UnitClass.Swordsman, Slot = EquipSlot.Weapon,
                    MinDamage = 5f, MaxDamage = 9f, AttackRange = 60f, Defense = 2f
                }
            },
            {
                "sword-iron", new ItemDef
                {
                    Id = "sword-iron", Name = "Broad Iron Sword", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Uncommon, UsableBy = UnitClass.Swordsman, Slot = EquipSlot.Weapon,
                    MinDamage = 10f, MaxDamage = 18f, AttackRange = 65f, Defense = 5f
                }
            },
            {
                "bow-wood", new ItemDef
                {
                    Id = "bow-wood", Name = "Short Hunting Bow", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Common, UsableBy = UnitClass.Archer, Slot = EquipSlot.Weapon,
                    MinDamage = 3f, MaxDamage = 7f, AttackRange = 450f, CritChance = 0.05f
                }
            },
            {
                "bow-cyclone", new ItemDef
                {
                    Id = "bow-cyclone", Name = "Cyclone Warbow", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Epic, UsableBy = UnitClass.Archer, Slot = EquipSlot.Weapon,
                    MinDamage = 18f, MaxDamage = 28f, AttackRange = 520f, CritChance = 0.20f
                }
            },
            {
                "arm-wood", new ItemDef
                {
                    Id = "arm-wood", Name = "Wood Brawler Fist", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Common, UsableBy = UnitClass.Brawler, Slot = EquipSlot.Weapon,
                    MinDamage = 8f, MaxDamage = 14f, AttackRange = 45f, StructureBonus = 2.5f
                }
            },
            {
                "arm-crusher", new ItemDef
                {
                    Id = "arm-crusher", Name = "Demolition Crusher", Category = ItemCategory.Weapon,
                    Rarity = ItemRarity.Epic, UsableBy = UnitClass.Brawler, Slot = EquipSlot.Weapon,
                    MinDamage = 25f, MaxDamage = 45f, AttackRange = 50f, StructureBonus = 3.5f, KnockbackPower = 50f
                }
            },

            // Shields
            {
                "shield-buckler", new ItemDef
                {
                    Id = "shield-buckler", Name = "Wooden Buckler", Category = ItemCategory.Shield,
                    Rarity = ItemRarity.Common, UsableBy = UnitClass.Swordsman, Slot = EquipSlot.Shield,
                    Defense = 6f, HealthBonus = 20f
                }
            },
            {
                "shield-vanguard-core", new ItemDef
                {
                    Id = "shield-vanguard-core", Name = "Vanguard Bastion", Category = ItemCategory.Shield,
                    Rarity = ItemRarity.Epic, UsableBy = UnitClass.Swordsman, Slot = EquipSlot.Shield,
                    Defense = 24f, HealthBonus = 120f
                }
            },

            // Helmets
            {
                "helm-leather", new ItemDef
                {
                    Id = "helm-leather", Name = "Leather Cap", Category = ItemCategory.Helmet,
                    Rarity = ItemRarity.Common, Slot = EquipSlot.Helmet,
                    Defense = 2f, HealthBonus = 15f
                }
            },
            {
                "helm-iron", new ItemDef
                {
                    Id = "helm-iron", Name = "Iron Vanguard Helm", Category = ItemCategory.Helmet,
                    Rarity = ItemRarity.Uncommon, Slot = EquipSlot.Helmet,
                    Defense = 8f, HealthBonus = 50f
                }
            },

            // Hero Relic Masks
            {
                "hero-mask-courage", new ItemDef
                {
                    Id = "hero-mask-courage", Name = "Mask of Courage", Category = ItemCategory.Mask,
                    Rarity = ItemRarity.Rare, Slot = EquipSlot.Mask,
                    HealthBonus = 60f, Defense = 5f, MinDamage = 4f, MaxDamage = 8f
                }
            },
            {
                "hero-mask-valor", new ItemDef
                {
                    Id = "hero-mask-valor", Name = "Mask of Valor", Category = ItemCategory.Mask,
                    Rarity = ItemRarity.Epic, Slot = EquipSlot.Mask,
                    HealthBonus = 120f, Defense = 12f, MinDamage = 8f, MaxDamage = 16f, CritChance = 0.10f
                }
            },
            {
                "hero-mask-wrath", new ItemDef
                {
                    Id = "hero-mask-wrath", Name = "Mask of Wrath", Category = ItemCategory.Mask,
                    Rarity = ItemRarity.Epic, Slot = EquipSlot.Mask,
                    MinDamage = 15f, MaxDamage = 30f, CritChance = 0.20f, KnockbackPower = 35f
                }
            },
            {
                "hero-mask-apex", new ItemDef
                {
                    Id = "hero-mask-apex", Name = "Crown of the Apex", Category = ItemCategory.Mask,
                    Rarity = ItemRarity.Legendary, Slot = EquipSlot.Mask,
                    HealthBonus = 200f, Defense = 20f, MinDamage = 25f, MaxDamage = 45f, CritChance = 0.25f, KnockbackPower = 50f
                }
            },

            // Divine Miracle Charms
            {
                "relic-rain-charm", new ItemDef
                {
                    Id = "relic-rain-charm", Name = "Rainmaker Relic", Category = ItemCategory.Relic,
                    Rarity = ItemRarity.Rare, Slot = EquipSlot.Relic,
                    Description = "Calls torrential rains during Fever to extinguish fires and restore health."
                }
            },
            {
                "relic-wind-charm", new ItemDef
                {
                    Id = "relic-wind-charm", Name = "Tailwind Relic", Category = ItemCategory.Relic,
                    Rarity = ItemRarity.Rare, Slot = EquipSlot.Relic,
                    Description = "Summons powerful favorable tailwinds during Fever, boosting projectile range and speed."
                }
            },
            {
                "relic-earthquake-charm", new ItemDef
                {
                    Id = "relic-earthquake-charm", Name = "Earthshaker Relic", Category = ItemCategory.Relic,
                    Rarity = ItemRarity.Epic, Slot = EquipSlot.Relic,
                    Description = "Shakes the earth during Fever, shattering enemy fortifications and barriers."
                }
            },
            {
                "relic-storm-charm", new ItemDef
                {
                    Id = "relic-storm-charm", Name = "Thunderstorm Relic", Category = ItemCategory.Relic,
                    Rarity = ItemRarity.Legendary, Slot = EquipSlot.Relic,
                    Description = "Calls divine thunderbolts during Fever to strike and stun enemy forces."
                }
            },

            // Crafting Materials & Foods
            { "mat-timber", new ItemDef { Id = "mat-timber", Name = "Birch Timber", Category = ItemCategory.Material, Rarity = ItemRarity.Common } },
            { "mat-stone", new ItemDef { Id = "mat-stone", Name = "Quarry Stone", Category = ItemCategory.Material, Rarity = ItemRarity.Common } },
            { "mat-iron-slag", new ItemDef { Id = "mat-iron-slag", Name = "Slag Alloy", Category = ItemCategory.Material, Rarity = ItemRarity.Uncommon } },
            { "mat-hardwood", new ItemDef { Id = "mat-hardwood", Name = "Aged Hardwood", Category = ItemCategory.Material, Rarity = ItemRarity.Uncommon } },
            { "mat-beast-hide", new ItemDef { Id = "mat-beast-hide", Name = "Beast Hide", Category = ItemCategory.Material, Rarity = ItemRarity.Common } },
            { "mat-wind-feather", new ItemDef { Id = "mat-wind-feather", Name = "Wind Feather", Category = ItemCategory.Material, Rarity = ItemRarity.Uncommon } },
            { "mat-mithril", new ItemDef { Id = "mat-mithril", Name = "Mithril Ore", Category = ItemCategory.Material, Rarity = ItemRarity.Rare } },
            { "mat-dragon-scale", new ItemDef { Id = "mat-dragon-scale", Name = "Dragon Scale", Category = ItemCategory.Material, Rarity = ItemRarity.Epic } },
            { "mat-sacred-sap", new ItemDef { Id = "mat-sacred-sap", Name = "Sacred Sap", Category = ItemCategory.Material, Rarity = ItemRarity.Rare } },
            { "mat-healing-herb", new ItemDef { Id = "mat-healing-herb", Name = "Sunbloom Herb", Category = ItemCategory.Material, Rarity = ItemRarity.Common } },
            { "mat-iron-ingot", new ItemDef { Id = "mat-iron-ingot", Name = "Refined Iron Ingot", Category = ItemCategory.Material, Rarity = ItemRarity.Uncommon } },
            { "mat-steel-plate", new ItemDef { Id = "mat-steel-plate", Name = "Tempered Steel Plate", Category = ItemCategory.Material, Rarity = ItemRarity.Rare } },
            { "mat-spirit-alloy", new ItemDef { Id = "mat-spirit-alloy", Name = "Spirit Alloy", Category = ItemCategory.Material, Rarity = ItemRarity.Epic } },
            { "mat-ancient-ore", new ItemDef { Id = "mat-ancient-ore", Name = "Ancient Star Ore", Category = ItemCategory.Material, Rarity = ItemRarity.Legendary } },
            { "mat-jerky", new ItemDef { Id = "mat-jerky", Name = "Tough Jerky", Category = ItemCategory.Food, Rarity = ItemRarity.Common } },
            { "mat-sun-cabbage", new ItemDef { Id = "mat-sun-cabbage", Name = "Sun Cabbage", Category = ItemCategory.Food, Rarity = ItemRarity.Common } },
            { "food-simple-stew", new ItemDef { Id = "food-simple-stew", Name = "Campfire Stew", Category = ItemCategory.Food, Rarity = ItemRarity.Common, HealthBonus = 30f, Description = "Hearty stew granting +30 max HP to all units." } },
            { "food-savory-roast", new ItemDef { Id = "food-savory-roast", Name = "Savory Beast Roast", Category = ItemCategory.Food, Rarity = ItemRarity.Uncommon, HealthBonus = 50f, MinDamage = 5f, MaxDamage = 5f, Description = "Nourishing roast granting +50 HP and +5 damage." } },
            { "food-divine-feast", new ItemDef { Id = "food-divine-feast", Name = "Divine Feast", Category = ItemCategory.Food, Rarity = ItemRarity.Rare, HealthBonus = 100f, MinDamage = 10f, MaxDamage = 10f, CritChance = 0.10f, Description = "Exquisite feast granting +100 HP, +10 damage, and +10% Crit." } },
        };

        public static ItemDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            ItemDef item;
            Database.TryGetValue(id, out item);
            return item;
        }
    }

    public static class ItemDatabase
    {
        public static ItemDef Get(string id)
        {
            return ItemDef.Get(id);
        }

        public static List<ItemDef> GetAll()
        {
            return new List<ItemDef>(ItemDef.Database.Values);
        }
    }
}
