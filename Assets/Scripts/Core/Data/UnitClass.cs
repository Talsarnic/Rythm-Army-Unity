using System;

namespace RhythmArmy.Core.Data
{
    public enum UnitClass
    {
        Banner,     // Standard Bearer
        Spearman,   // Spear Thrower
        Swordsman,  // Sword & Shield Vanguard
        Archer,     // Long-range Sniper
        Cavalry,    // High-speed Mounted Charger
        Hammerer,   // Heavy Hammer / Mace Breaker
        Hornist,    // Sonic Warhornist
        Skyrider,   // Winged Aerial Lancer
        Mage,       // Arcane Staff Channeler
        Brawler     // Heavy Gauntlet Demolisher
    }

    public enum Subspecies
    {
        Normal,
        Swiftpaw,   // Agile rabbit warrior (+10% Crit, +10% Dmg)
        Frogtide,   // Amphibian rain warrior (+30% HP, Freeze immune)
        Ironwool,   // Wool-clad guardian (+40% Def, Stun immune)
        Colossus,   // Horned behemoth (+80% HP, +50% Knockback)
        Apex        // Legendary winged avatar (+50% HP, +50% Dmg, +15% Crit)
    }

    public enum EquipSlot
    {
        Weapon,
        Shield,
        Helmet,
        Mask,
        Relic
    }

    [Serializable]
    public class UnitMember
    {
        public string Id;
        public UnitClass Class;
        public Subspecies Subspecies = Subspecies.Normal;
        public int Level;
        public string WeaponId;
        public string ShieldId;
        public string HelmetId;
        public string MaskId;
        public string RelicId;
        public bool IsHero;

        // Mater Tree Awakening & Fusion
        public int AwakeningTier = 0; // 0 to 5
        public int AttackMastery = 0; // +2% per point
        public int DefenseMastery = 0; // +2% per point
        public int HealthMastery = 0; // +3% per point
        public int SpeedMastery = 0; // +1% per point
        public Subspecies SecondaryFusion = Subspecies.Normal; // Subspecies gene fusion memory

        public Subspecies Species
        {
            get { return Subspecies; }
            set { Subspecies = value; }
        }

        public string Name
        {
            get { return (IsHero ? "Hero " : "") + Class.ToString(); }
        }

        public UnitMember()
        {
            Id = Guid.NewGuid().ToString();
            Level = 1;
            Subspecies = Subspecies.Normal;
        }

        public UnitMember(string id, UnitClass unitClass, int level = 1, string weapon = null, string shield = null, string helmet = null, Subspecies subspecies = Subspecies.Normal, bool isHero = false, string mask = null, string relic = null)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
            Class = unitClass;
            Level = level;
            WeaponId = weapon;
            ShieldId = shield;
            HelmetId = helmet;
            Subspecies = subspecies;
            IsHero = isHero;
            MaskId = mask;
            RelicId = relic;
        }

        public UnitMember Clone()
        {
            var clone = new UnitMember(Id, Class, Level, WeaponId, ShieldId, HelmetId, Subspecies, IsHero, MaskId, RelicId);
            clone.AwakeningTier = AwakeningTier;
            clone.AttackMastery = AttackMastery;
            clone.DefenseMastery = DefenseMastery;
            clone.HealthMastery = HealthMastery;
            clone.SpeedMastery = SpeedMastery;
            clone.SecondaryFusion = SecondaryFusion;
            return clone;
        }
    }
}
