using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public enum TileLayerType
    {
        Ground,
        Cliffs,
        Obstacles,
        Decorations,
        Foreground
    }

    public enum TileColliderType
    {
        None,
        Solid,
        DestructibleStructure,
        PitHazard
    }

    public class TileDefinition
    {
        public string TileId;
        public string Name;
        public BiomeType Biome;
        public TileLayerType Layer;
        public TileColliderType Collider;
        public int TileSize;
        public int AtlasIndex;

        public TileDefinition(string tileId, string name, BiomeType biome, TileLayerType layer, TileColliderType collider = TileColliderType.None, int tileSize = 16, int atlasIndex = 0)
        {
            TileId = tileId;
            Name = name;
            Biome = biome;
            Layer = layer;
            Collider = collider;
            TileSize = tileSize;
            AtlasIndex = atlasIndex;
        }
    }

    public static class TilemapRegistry
    {
        private static readonly Dictionary<string, TileDefinition> _tiles = new Dictionary<string, TileDefinition>
        {
            // Coral Coast
            { "tile_coast_sand_center", new TileDefinition("tile_coast_sand_center", "Coast Wet Sand", BiomeType.CoralCoast, TileLayerType.Ground) },
            { "tile_coast_water_edge", new TileDefinition("tile_coast_water_edge", "Coast Shallow Tide", BiomeType.CoralCoast, TileLayerType.Ground) },
            { "tile_coast_coral_barrier", new TileDefinition("tile_coast_coral_barrier", "Coral Reef Barrier", BiomeType.CoralCoast, TileLayerType.Obstacles, TileColliderType.Solid) },

            // Jungle Fort
            { "tile_jungle_grass_center", new TileDefinition("tile_jungle_grass_center", "Jungle Moss Floor", BiomeType.JungleFort, TileLayerType.Ground) },
            { "tile_jungle_ruin_pillar", new TileDefinition("tile_jungle_ruin_pillar", "Mossy Stone Pillar", BiomeType.JungleFort, TileLayerType.Obstacles, TileColliderType.Solid) },

            // Volcanic Caldera
            { "tile_caldera_obsidian", new TileDefinition("tile_caldera_obsidian", "Basalt Obsidian Road", BiomeType.VolcanicCaldera, TileLayerType.Ground) },
            { "tile_caldera_magma_vent", new TileDefinition("tile_caldera_magma_vent", "Active Magma Vent", BiomeType.VolcanicCaldera, TileLayerType.Obstacles, TileColliderType.PitHazard) },

            // Iron Bastion
            { "tile_bastion_cobblestone", new TileDefinition("tile_bastion_cobblestone", "Bastion Fortress Stone", BiomeType.IronBastion, TileLayerType.Ground) },
            { "tile_bastion_iron_gate", new TileDefinition("tile_bastion_iron_gate", "Fortified Iron Gate", BiomeType.IronBastion, TileLayerType.Obstacles, TileColliderType.DestructibleStructure) }
        };

        public static TileDefinition GetTile(string tileId)
        {
            TileDefinition tile;
            if (_tiles.TryGetValue(tileId, out tile))
            {
                return tile;
            }
            return _tiles["tile_coast_sand_center"];
        }
    }
}
