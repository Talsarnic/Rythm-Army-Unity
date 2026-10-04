using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public enum BiomeType
    {
        CoralCoast,
        JungleFort,
        MistySwamp,
        VolcanicCaldera,
        IronBastion,
        RuinAltar,
        DesertDunes,
        FrozenPeaks
    }

    public enum WeatherType
    {
        Clear,
        Rain,
        Embers,
        Spores,
        Blizzard,
        Sandstorm,
        Sunbeams
    }

    public class ParallaxLayer
    {
        public string Name;
        public float ScrollMultiplier;
        public float BaseY;
        public float TileWidth;
        public float Alpha;
        public string SpriteSheetId;

        public ParallaxLayer(string name, float scrollMultiplier, float baseY, float tileWidth = 480f, float alpha = 1f, string spriteSheetId = "")
        {
            Name = name;
            ScrollMultiplier = scrollMultiplier;
            BaseY = baseY;
            TileWidth = tileWidth;
            Alpha = alpha;
            SpriteSheetId = spriteSheetId;
        }

        public float ComputePositionX(float cameraX)
        {
            // Parallax offset with seamless wrapping
            float rawX = -cameraX * ScrollMultiplier;
            return rawX;
        }
    }

    public class WeatherEffect
    {
        public WeatherType Type;
        public float Intensity; // 0.0 to 1.0
        public float WindSpeedX;
        public float FallSpeedY;
        public float SpawnRate; // particles/sec
        public string TintHex;

        public WeatherEffect(WeatherType type, float intensity, float windX, float fallY, float spawnRate, string tintHex)
        {
            Type = type;
            Intensity = intensity;
            WindSpeedX = windX;
            FallSpeedY = fallY;
            SpawnRate = spawnRate;
            TintHex = tintHex;
        }
    }

    public class BiomeTheme
    {
        public BiomeType Biome;
        public string DisplayName;
        public string GroundTileId;
        public string PropSetId;
        public WeatherEffect Weather;
        public List<ParallaxLayer> Layers = new List<ParallaxLayer>();

        public BiomeTheme(BiomeType biome, string displayName, string groundTileId, string propSetId, WeatherEffect weather)
        {
            Biome = biome;
            DisplayName = displayName;
            GroundTileId = groundTileId;
            PropSetId = propSetId;
            Weather = weather;
        }
    }

    public static class EnvironmentSystem
    {
        public static BiomeType ParseBiome(string biomeStr)
        {
            if (string.IsNullOrEmpty(biomeStr)) return BiomeType.CoralCoast;

            switch (biomeStr.ToLowerInvariant())
            {
                case "coral-coast":
                case "coralcoast":
                case "coast":
                    return BiomeType.CoralCoast;
                case "jungle":
                case "jungle-fort":
                case "junglefort":
                    return BiomeType.JungleFort;
                case "swamp":
                case "misty-swamp":
                case "mistyswamp":
                    return BiomeType.MistySwamp;
                case "volcano":
                case "volcanic-caldera":
                case "caldera":
                    return BiomeType.VolcanicCaldera;
                case "bastion":
                case "iron-bastion":
                case "ironbastion":
                    return BiomeType.IronBastion;
                case "ruins":
                case "ruin-altar":
                case "ruinaltar":
                    return BiomeType.RuinAltar;
                case "desert":
                case "desert-dunes":
                    return BiomeType.DesertDunes;
                case "frozen":
                case "frozen-peaks":
                    return BiomeType.FrozenPeaks;
                default:
                    return BiomeType.CoralCoast;
            }
        }

        public static BiomeTheme CreateBiomeTheme(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.CoralCoast:
                {
                    var theme = new BiomeTheme(
                        BiomeType.CoralCoast,
                        "Coral Coast",
                        "tile_sand_shore_01",
                        "props_coastal_shells",
                        new WeatherEffect(WeatherType.Sunbeams, 0.4f, 15f, 0f, 10f, "#FFF8DC")
                    );
                    theme.Layers.Add(new ParallaxLayer("Sky Horizon", 0.05f, 0f, 480f, 1.0f, "bg_coast_sky"));
                    theme.Layers.Add(new ParallaxLayer("Distant Ocean & Cliffs", 0.15f, -20f, 480f, 0.9f, "bg_coast_ocean"));
                    theme.Layers.Add(new ParallaxLayer("Palm Canopy & Dunes", 0.40f, -10f, 480f, 1.0f, "bg_coast_palms"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Sand", 1.00f, 0f, 480f, 1.0f, "tile_coast_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Reef Props", 1.30f, 10f, 480f, 0.95f, "fg_coast_corals"));
                    return theme;
                }

                case BiomeType.JungleFort:
                {
                    var theme = new BiomeTheme(
                        BiomeType.JungleFort,
                        "Jungle Fort",
                        "tile_jungle_moss_01",
                        "props_jungle_vines",
                        new WeatherEffect(WeatherType.Spores, 0.5f, 10f, -5f, 25f, "#7CFC00")
                    );
                    theme.Layers.Add(new ParallaxLayer("Misty Sky", 0.05f, 0f, 480f, 1.0f, "bg_jungle_sky"));
                    theme.Layers.Add(new ParallaxLayer("Deep Canopy Forest", 0.20f, -15f, 480f, 0.85f, "bg_jungle_canopy_deep"));
                    theme.Layers.Add(new ParallaxLayer("Ancient Stone Pillars", 0.45f, -10f, 480f, 1.0f, "bg_jungle_ruins"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Grass", 1.00f, 0f, 480f, 1.0f, "tile_jungle_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Foliage", 1.25f, 15f, 480f, 0.95f, "fg_jungle_ferns"));
                    return theme;
                }

                case BiomeType.MistySwamp:
                {
                    var theme = new BiomeTheme(
                        BiomeType.MistySwamp,
                        "Misty Swamp",
                        "tile_swamp_muck_01",
                        "props_swamp_fungi",
                        new WeatherEffect(WeatherType.Rain, 0.6f, -30f, -150f, 50f, "#87CEEB")
                    );
                    theme.Layers.Add(new ParallaxLayer("Overcast Sky", 0.05f, 0f, 480f, 1.0f, "bg_swamp_sky"));
                    theme.Layers.Add(new ParallaxLayer("Ghostly Willow Trees", 0.18f, -20f, 480f, 0.75f, "bg_swamp_willows"));
                    theme.Layers.Add(new ParallaxLayer("Midground Mist Fog", 0.35f, 0f, 480f, 0.60f, "bg_swamp_fog"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Mud", 1.00f, 0f, 480f, 1.0f, "tile_swamp_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Reeds", 1.25f, 12f, 480f, 0.90f, "fg_swamp_reeds"));
                    return theme;
                }

                case BiomeType.VolcanicCaldera:
                {
                    var theme = new BiomeTheme(
                        BiomeType.VolcanicCaldera,
                        "Volcanic Caldera",
                        "tile_obsidian_magma_01",
                        "props_volcano_vents",
                        new WeatherEffect(WeatherType.Embers, 0.8f, 20f, 40f, 40f, "#FF4500")
                    );
                    theme.Layers.Add(new ParallaxLayer("Smoky Red Sky", 0.05f, 0f, 480f, 1.0f, "bg_caldera_sky"));
                    theme.Layers.Add(new ParallaxLayer("Volcano Ridge Peak", 0.15f, -25f, 480f, 0.9f, "bg_caldera_mountains"));
                    theme.Layers.Add(new ParallaxLayer("Lava Rivers & Basalt", 0.40f, -10f, 480f, 1.0f, "bg_caldera_lava_flows"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Obsidian", 1.00f, 0f, 480f, 1.0f, "tile_caldera_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Magma Rocks", 1.30f, 10f, 480f, 0.95f, "fg_caldera_rocks"));
                    return theme;
                }

                case BiomeType.IronBastion:
                {
                    var theme = new BiomeTheme(
                        BiomeType.IronBastion,
                        "Iron Bastion",
                        "tile_iron_fortress_01",
                        "props_bastion_gears",
                        new WeatherEffect(WeatherType.Clear, 0.2f, 0f, 0f, 0f, "#B0C4DE")
                    );
                    theme.Layers.Add(new ParallaxLayer("Industrial Sky", 0.05f, 0f, 480f, 1.0f, "bg_bastion_sky"));
                    theme.Layers.Add(new ParallaxLayer("Citadel Spire Towers", 0.18f, -20f, 480f, 0.85f, "bg_bastion_spires"));
                    theme.Layers.Add(new ParallaxLayer("Fortress Walls & Banners", 0.45f, -10f, 480f, 1.0f, "bg_bastion_ramparts"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Cobblestone", 1.00f, 0f, 480f, 1.0f, "tile_bastion_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Barricade Spikes", 1.25f, 10f, 480f, 0.95f, "fg_bastion_spikes"));
                    return theme;
                }

                case BiomeType.RuinAltar:
                {
                    var theme = new BiomeTheme(
                        BiomeType.RuinAltar,
                        "Ruin Altar",
                        "tile_ruin_marble_01",
                        "props_ruins_monoliths",
                        new WeatherEffect(WeatherType.Sunbeams, 0.7f, 5f, 0f, 20f, "#E6E6FA")
                    );
                    theme.Layers.Add(new ParallaxLayer("Cosmic Nebula Sky", 0.05f, 0f, 480f, 1.0f, "bg_altar_sky"));
                    theme.Layers.Add(new ParallaxLayer("Floating Shrine Islands", 0.16f, -20f, 480f, 0.85f, "bg_altar_shrine_islands"));
                    theme.Layers.Add(new ParallaxLayer("Ancient Rune Pillars", 0.42f, -10f, 480f, 1.0f, "bg_altar_pillars"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Crystal Tiles", 1.00f, 0f, 480f, 1.0f, "tile_altar_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Glowing Crystals", 1.30f, 12f, 480f, 0.95f, "fg_altar_crystals"));
                    return theme;
                }

                case BiomeType.DesertDunes:
                {
                    var theme = new BiomeTheme(
                        BiomeType.DesertDunes,
                        "Desert Dunes",
                        "tile_desert_sand_01",
                        "props_desert_cacti",
                        new WeatherEffect(WeatherType.Sandstorm, 0.75f, 60f, -10f, 45f, "#DEB887")
                    );
                    theme.Layers.Add(new ParallaxLayer("Blazing Sun & Heat Mirage", 0.05f, 0f, 480f, 1.0f, "bg_desert_sky"));
                    theme.Layers.Add(new ParallaxLayer("Distant Giant Dunes", 0.15f, -20f, 480f, 0.85f, "bg_desert_dunes_far"));
                    theme.Layers.Add(new ParallaxLayer("Ruined Oasis Palms", 0.40f, -10f, 480f, 1.0f, "bg_desert_oasis"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Shifting Sand", 1.00f, 0f, 480f, 1.0f, "tile_desert_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Sand Tufts", 1.25f, 10f, 480f, 0.95f, "fg_desert_tufts"));
                    return theme;
                }

                case BiomeType.FrozenPeaks:
                default:
                {
                    var theme = new BiomeTheme(
                        BiomeType.FrozenPeaks,
                        "Frozen Peaks",
                        "tile_snow_ice_01",
                        "props_peaks_icicles",
                        new WeatherEffect(WeatherType.Blizzard, 0.85f, -50f, -100f, 60f, "#F0FFFF")
                    );
                    theme.Layers.Add(new ParallaxLayer("Aurora Night Sky", 0.05f, 0f, 480f, 1.0f, "bg_peaks_sky"));
                    theme.Layers.Add(new ParallaxLayer("Glacier Mountain Range", 0.15f, -25f, 480f, 0.9f, "bg_peaks_glaciers"));
                    theme.Layers.Add(new ParallaxLayer("Frozen Pine Forest", 0.38f, -10f, 480f, 1.0f, "bg_peaks_pines"));
                    theme.Layers.Add(new ParallaxLayer("Battle Lane Packed Snow", 1.00f, 0f, 480f, 1.0f, "tile_peaks_ground"));
                    theme.Layers.Add(new ParallaxLayer("Foreground Frost Drifts", 1.30f, 10f, 480f, 0.95f, "fg_peaks_frost"));
                    return theme;
                }
            }
        }
    }
}
