using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Runtime visual contract for a unit. Gameplay data remains authoritative for stats;
    /// this profile translates that data into visible art layers and attachment points.
    /// </summary>
    [Serializable]
    public sealed class UnitVisualProfile
    {
        public UnitClass Class;
        public Subspecies Species;
        public string BodySpriteId;
        public string WeaponSpriteId;
        public string ShieldSpriteId;
        public string HelmetSpriteId;
        public string MaskSpriteId;
        public string RelicSpriteId;
        public string ArmorSpriteId;
        public string EvolutionSpriteId;

        public bool HasWeapon { get { return !string.IsNullOrEmpty(WeaponSpriteId); } }
        public bool HasShield { get { return !string.IsNullOrEmpty(ShieldSpriteId); } }
        public bool HasHelmet { get { return !string.IsNullOrEmpty(HelmetSpriteId); } }
        public bool HasMask { get { return !string.IsNullOrEmpty(MaskSpriteId); } }
        public bool HasRelic { get { return !string.IsNullOrEmpty(RelicSpriteId); } }

        public static UnitVisualProfile FromUnit(UnitMember member)
        {
            if (member == null) throw new ArgumentNullException("member");

            var className = member.Class.ToString().ToLowerInvariant();
            var speciesName = member.Subspecies.ToString().ToLowerInvariant();

            return new UnitVisualProfile
            {
                Class = member.Class,
                Species = member.Subspecies,
                BodySpriteId = "unit_" + className + "_body_" + speciesName,
                EvolutionSpriteId = "unit_" + className + "_evolution_" + speciesName,
                WeaponSpriteId = VisualId("weapon", member.WeaponId),
                ShieldSpriteId = VisualId("shield", member.ShieldId),
                HelmetSpriteId = VisualId("helmet", member.HelmetId),
                MaskSpriteId = VisualId("mask", member.MaskId),
                RelicSpriteId = VisualId("relic", member.RelicId),
                ArmorSpriteId = "unit_" + className + "_armor_" + speciesName
            };
        }

        private static string VisualId(string prefix, string itemId)
        {
            return string.IsNullOrEmpty(itemId) ? null : prefix + "_" + itemId;
        }

        public IEnumerable<string> GetLayerSpriteIds()
        {
            if (!string.IsNullOrEmpty(BodySpriteId)) yield return BodySpriteId;
            if (!string.IsNullOrEmpty(ArmorSpriteId)) yield return ArmorSpriteId;
            if (!string.IsNullOrEmpty(ShieldSpriteId)) yield return ShieldSpriteId;
            if (!string.IsNullOrEmpty(WeaponSpriteId)) yield return WeaponSpriteId;
            if (!string.IsNullOrEmpty(HelmetSpriteId)) yield return HelmetSpriteId;
            if (!string.IsNullOrEmpty(MaskSpriteId)) yield return MaskSpriteId;
            if (!string.IsNullOrEmpty(RelicSpriteId)) yield return RelicSpriteId;
        }
    }
}
