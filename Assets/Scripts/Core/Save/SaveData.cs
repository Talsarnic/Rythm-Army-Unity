using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Core.Save
{
    [Serializable]
    public class AudioSettingsData
    {
        public float Master = 1.0f;
        public float Music = 0.8f;
        public float Sfx = 1.0f;
        public float ScreenShake = 1.0f;
    }

    [Serializable]
    public class InventoryEntry
    {
        public string ItemId;
        public int Quantity;

        public InventoryEntry() { }
        public InventoryEntry(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public int Gold = 250;
        public List<string> CompletedMissions = new List<string>();
        public List<string> MissionsUnlocked = new List<string> { "mission-1" };
        public List<string> MissionsCleared = new List<string>();
        public int BestCombo = 0;
        public float InputOffsetMs = 0f;
        public List<UnitMember> Roster = new List<UnitMember>();
        public List<InventoryEntry> Inventory = new List<InventoryEntry>();
        public Dictionary<string, int> BossHuntLevels = new Dictionary<string, int>();
        public string ActiveMealBuffId = null;
        public AudioSettingsData Settings = new AudioSettingsData();

        public static SaveData CreateDefault()
        {
            var save = new SaveData
            {
                Version = 1,
                Gold = 250,
                CompletedMissions = new List<string>(),
                MissionsUnlocked = new List<string> { "mission-1" },
                MissionsCleared = new List<string>(),
                BestCombo = 0,
                InputOffsetMs = 0f,
                Roster = new List<UnitMember>
                {
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Banner, 1),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Spearman, 1, "spear-wood", null, "helm-leather", Subspecies.Normal, true),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Spearman, 1, "spear-wood", null, null),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Spearman, 1, "spear-wood", null, null),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Swordsman, 1, "sword-wood", "shield-buckler", "helm-leather"),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Swordsman, 1, "sword-wood", "shield-buckler", null),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Archer, 1, "bow-wood", null, null),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Archer, 1, "bow-wood", null, null),
                    new UnitMember(Guid.NewGuid().ToString(), UnitClass.Brawler, 1, "arm-wood", null, null)
                },
                Inventory = new List<InventoryEntry>
                {
                    new InventoryEntry("mat-timber", 10),
                    new InventoryEntry("mat-stone", 10),
                    new InventoryEntry("mat-jerky", 8),
                    new InventoryEntry("spear-wood", 2),
                    new InventoryEntry("sword-wood", 1),
                    new InventoryEntry("shield-buckler", 1)
                }
            };

            return save;
        }

        public int GetItemCount(string itemId)
        {
            var entry = Inventory.Find(e => e.ItemId == itemId);
            return entry != null ? entry.Quantity : 0;
        }

        public void AddItem(string itemId, int count = 1)
        {
            if (count <= 0) return;
            var entry = Inventory.Find(e => e.ItemId == itemId);
            if (entry != null)
            {
                entry.Quantity += count;
            }
            else
            {
                Inventory.Add(new InventoryEntry(itemId, count));
            }
        }

        public void AddMaterial(string itemId, int count = 1)
        {
            AddItem(itemId, count);
        }

        public bool RemoveItem(string itemId, int count = 1)
        {
            if (count <= 0) return true;
            var entry = Inventory.Find(e => e.ItemId == itemId);
            if (entry == null || entry.Quantity < count) return false;

            entry.Quantity -= count;
            if (entry.Quantity <= 0)
            {
                Inventory.Remove(entry);
            }
            return true;
        }
    }
}
