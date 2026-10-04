using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Gameplay.Camp
{
    public enum GemElement
    {
        None,
        Flame,      // Burn DoT over time
        Frost,      // Slows enemy cooldowns and movement
        Lightning,  // Chain shock damage to adjacent targets
        Earth       // Shield break, structure bonus damage, and armor stagger
    }

    public class GemSocketDef
    {
        public string GemId;
        public string Name;
        public GemElement Element;
        public int Tier;
        public float PrimaryValue;     // e.g. Burn damage per sec, Frost slow %, Shock chain %, Earth armor break
        public string Description;

        public GemSocketDef(string gemId, string name, GemElement element, int tier, float value, string description)
        {
            GemId = gemId;
            Name = name;
            Element = element;
            Tier = tier;
            PrimaryValue = value;
            Description = description;
        }
    }

    public class SocketCost
    {
        public int Gold;
        public string MaterialId;
        public int MaterialCount;

        public SocketCost(int gold, string materialId, int count)
        {
            Gold = gold;
            MaterialId = materialId;
            MaterialCount = count;
        }
    }

    /// <summary>
    /// Moonlighter-inspired Blacksmith Enchanting and Elemental Gem Socketing system.
    /// Allows socketing weapons and shields with elemental gems to unleash elemental procs in battle.
    /// </summary>
    public static class EnchantingSystem
    {
        private static readonly Dictionary<string, GemSocketDef> _gems = new Dictionary<string, GemSocketDef>();

        static EnchantingSystem()
        {
            // Flame Gems (Burn DoT)
            _gems["gem-flame-1"] = new GemSocketDef("gem-flame-1", "Lesser Ember Gem", GemElement.Flame, 1, 6f, "Ignites enemies for 6 burn damage per second for 3s.");
            _gems["gem-flame-2"] = new GemSocketDef("gem-flame-2", "Blazing Fire Gem", GemElement.Flame, 2, 15f, "Ignites enemies for 15 burn damage per second for 4s.");
            _gems["gem-flame-3"] = new GemSocketDef("gem-flame-3", "Infernal Core Gem", GemElement.Flame, 3, 30f, "Ignites enemies for 30 burn damage per second for 5s.");

            // Frost Gems (Slowdown & Freeze)
            _gems["gem-frost-1"] = new GemSocketDef("gem-frost-1", "Chilled Rime Gem", GemElement.Frost, 1, 0.20f, "Chills targets slowing movement and attack rate by 20%.");
            _gems["gem-frost-2"] = new GemSocketDef("gem-frost-2", "Glacial Shard Gem", GemElement.Frost, 2, 0.35f, "Chills targets by 35% with chance to freeze bosses during telegraphs.");
            _gems["gem-frost-3"] = new GemSocketDef("gem-frost-3", "Absolute Zero Gem", GemElement.Frost, 3, 0.50f, "Chills targets by 50% with massive boss action delays.");

            // Lightning Gems (Chain Shock)
            _gems["gem-storm-1"] = new GemSocketDef("gem-storm-1", "Spark Topaz Gem", GemElement.Lightning, 1, 0.25f, "Chain shocks up to 2 nearby enemies for 25% weapon damage.");
            _gems["gem-storm-2"] = new GemSocketDef("gem-storm-2", "Thunderbolt Gem", GemElement.Lightning, 2, 0.40f, "Chain shocks up to 3 nearby enemies for 40% weapon damage.");
            _gems["gem-storm-3"] = new GemSocketDef("gem-storm-3", "Tempest Core Gem", GemElement.Lightning, 3, 0.60f, "Chain shocks up to 4 nearby enemies for 60% weapon damage.");

            // Earth Gems (Armor Shatter & Knockback)
            _gems["gem-earth-1"] = new GemSocketDef("gem-earth-1", "Granite Pebble Gem", GemElement.Earth, 1, 10f, "Ignores 10 enemy defense and deals +30% vs barricades/structures.");
            _gems["gem-earth-2"] = new GemSocketDef("gem-earth-2", "Seismic Quartz Gem", GemElement.Earth, 2, 25f, "Ignores 25 enemy defense and deals +60% vs barricades/structures.");
            _gems["gem-earth-3"] = new GemSocketDef("gem-earth-3", "Titan Bedrock Gem", GemElement.Earth, 3, 50f, "Ignores 50 enemy defense, shatters heavy shields, +100% vs structures.");
        }

        public static GemSocketDef GetGem(string gemId)
        {
            GemSocketDef gem;
            if (_gems.TryGetValue(gemId, out gem)) return gem;
            return null;
        }

        public static List<GemSocketDef> GetAllGems()
        {
            return new List<GemSocketDef>(_gems.Values);
        }

        public static SocketCost GetSocketCost(int tier)
        {
            switch (tier)
            {
                case 1: return new SocketCost(100, "mat-iron-slag", 2);
                case 2: return new SocketCost(350, "mat-mithril", 3);
                case 3: return new SocketCost(1000, "mat-dragon-scale", 2);
                default: return new SocketCost(100, "mat-iron-slag", 1);
            }
        }

        /// <summary>
        /// Sockets an elemental gem into a unit's active weapon or helmet gear.
        /// </summary>
        public static bool SocketGem(UnitMember unit, string gemId, Dictionary<string, int> inventory, ref int playerGold)
        {
            if (unit == null || string.IsNullOrEmpty(gemId)) return false;
            var gem = GetGem(gemId);
            if (gem == null) return false;

            var cost = GetSocketCost(gem.Tier);
            if (playerGold < cost.Gold) return false;

            int matCount = 0;
            inventory.TryGetValue(cost.MaterialId, out matCount);
            if (matCount < cost.MaterialCount) return false;

            // Deduct
            playerGold -= cost.Gold;
            inventory[cost.MaterialId] = matCount - cost.MaterialCount;

            // Socket onto weapon ID metadata (e.g. "wpn-iron-spear:gem-flame-2")
            unit.WeaponId = ApplyGemToItem(unit.WeaponId, gemId);
            return true;
        }

        public static string ApplyGemToItem(string baseItem, string gemId)
        {
            if (string.IsNullOrEmpty(baseItem)) return baseItem;
            int colon = baseItem.IndexOf(':');
            string cleanItem = colon >= 0 ? baseItem.Substring(0, colon) : baseItem;
            return cleanItem + ":" + gemId;
        }

        public static string ExtractGemId(string itemString)
        {
            if (string.IsNullOrEmpty(itemString)) return null;
            int colon = itemString.IndexOf(':');
            if (colon >= 0 && colon < itemString.Length - 1)
            {
                return itemString.Substring(colon + 1);
            }
            return null;
        }

        public static GemElement GetItemElement(string itemString)
        {
            string gemId = ExtractGemId(itemString);
            if (string.IsNullOrEmpty(gemId)) return GemElement.None;
            var gem = GetGem(gemId);
            return gem != null ? gem.Element : GemElement.None;
        }
    }
}
