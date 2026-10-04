using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Defines the art-facing contract for equipment. Item stats never determine visuals;
    /// each item receives an explicit visual family and silhouette treatment.
    /// </summary>
    public sealed class EquipmentVisualSpec
    {
        public string ItemId;
        public string Family;
        public string Material;
        public string Silhouette;
        public string Accent;
        public Vector2Int AnchorOffset;
        public bool ChangesSilhouette;

        public EquipmentVisualSpec(string itemId, string family, string material, string silhouette,
            string accent, Vector2Int anchorOffset, bool changesSilhouette)
        {
            ItemId = itemId;
            Family = family;
            Material = material;
            Silhouette = silhouette;
            Accent = accent;
            AnchorOffset = anchorOffset;
            ChangesSilhouette = changesSilhouette;
        }
    }

    public static class EquipmentVisualRegistry
    {
        private static readonly Dictionary<string, EquipmentVisualSpec> Specs =
            new Dictionary<string, EquipmentVisualSpec>
        {
            { "spear-wood", Spec("spear-wood", "spear", "wood", "straight_ash", "leather", 1, 0, true) },
            { "spear-iron", Spec("spear-iron", "spear", "iron", "leaf_blade", "steel", 1, -1, true) },
            { "spear-storm", Spec("spear-storm", "spear", "mithril", "forked_thunder", "spirit_cyan", 2, -1, true) },

            { "shield-buckler", Spec("shield-buckler", "shield", "wood", "round_buckler", "leather", -1, 0, true) },
            { "shield-vanguard-core", Spec("shield-vanguard-core", "shield", "steel", "tower_bastion", "gold", -2, -1, true) },

            { "helm-leather", Spec("helm-leather", "helmet", "leather", "skirmisher_cap", "amber", 0, 0, true) },
            { "helm-iron", Spec("helm-iron", "helmet", "iron", "closed_vanguard", "steel", 0, -1, true) },

            { "hero-mask-courage", Spec("hero-mask-courage", "mask", "painted_wood", "lion_face", "gold", 0, 0, true) },
            { "hero-mask-valor", Spec("hero-mask-valor", "mask", "steel", "war_face", "crimson", 0, -1, true) },
            { "hero-mask-wrath", Spec("hero-mask-wrath", "mask", "obsidian", "horned_face", "crimson", 0, -1, true) },
            { "hero-mask-apex", Spec("hero-mask-apex", "mask", "celestial", "crown_face", "star_gold", 0, -2, true) },

            { "relic-rain-charm", Spec("relic-rain-charm", "relic", "crystal", "water_charm", "ocean_teal", 0, 0, true) },
            { "relic-wind-charm", Spec("relic-wind-charm", "relic", "feather", "wind_talisman", "spirit_cyan", 0, 0, true) },
            { "relic-earthquake-charm", Spec("relic-earthquake-charm", "relic", "stone", "earth_totem", "amber", 0, 0, true) },
            { "relic-storm-charm", Spec("relic-storm-charm", "relic", "crystal", "thunder_orb", "spirit_cyan", 0, -1, true) }
        };

        private static EquipmentVisualSpec Spec(string id, string family, string material,
            string silhouette, string accent, int x, int y, bool changesSilhouette)
        {
            return new EquipmentVisualSpec(id, family, material, silhouette, accent,
                new Vector2Int(x, y), changesSilhouette);
        }

        public static EquipmentVisualSpec Get(string itemId)
        {
            EquipmentVisualSpec spec;
            return !string.IsNullOrEmpty(itemId) && Specs.TryGetValue(itemId, out spec) ? spec : null;
        }

        public static bool ChangesSilhouette(string itemId)
        {
            var spec = Get(itemId);
            return spec != null && spec.ChangesSilhouette;
        }
    }
}
