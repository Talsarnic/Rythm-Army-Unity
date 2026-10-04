using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public class SpriteAtlasEntry
    {
        public string SpriteId { get; set; }
        public PixelRect AtlasRect { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        public SpriteAtlasEntry(string spriteId, PixelRect rect, int width, int height)
        {
            SpriteId = spriteId;
            AtlasRect = rect;
            OriginalWidth = width;
            OriginalHeight = height;
        }
    }

    /// <summary>
    /// Master Sprite Atlas Builder.
    /// Packs individual character animations, item icons, VFX sprites, and biome tiles
    /// into unified texture atlases with UV rectangle coordinates for Unity rendering.
    /// </summary>
    public class SpriteAtlasBuilder
    {
        private readonly int _atlasWidth;
        private readonly int _atlasHeight;
        private readonly PixelBitmapBuffer _atlasBuffer;
        private readonly Dictionary<string, SpriteAtlasEntry> _entries = new Dictionary<string, SpriteAtlasEntry>();
        private int _currentX = 0;
        private int _currentY = 0;
        private int _currentRowHeight = 0;

        public PixelBitmapBuffer AtlasBuffer { get { return _atlasBuffer; } }
        public IReadOnlyDictionary<string, SpriteAtlasEntry> Entries { get { return _entries; } }

        public SpriteAtlasBuilder(int width = 512, int height = 512)
        {
            _atlasWidth = width;
            _atlasHeight = height;
            _atlasBuffer = new PixelBitmapBuffer(width, height);
        }

        /// <summary>
        /// Packs a sub-texture into the atlas using a 2D shelf-packing algorithm.
        /// </summary>
        public bool AddSprite(string spriteId, PixelBitmapBuffer sprite)
        {
            if (string.IsNullOrEmpty(spriteId) || sprite == null) return false;

            // Check if sprite fits in current row
            if (_currentX + sprite.Width > _atlasWidth)
            {
                // Advance to next row
                _currentX = 0;
                _currentY += _currentRowHeight;
                _currentRowHeight = 0;
            }

            // Check if sprite fits vertically
            if (_currentY + sprite.Height > _atlasHeight)
            {
                return false; // Atlas full
            }

            // Blit into atlas
            _atlasBuffer.Blit(sprite, _currentX, _currentY);

            var rect = new PixelRect(_currentX, _currentY, sprite.Width, sprite.Height);
            _entries[spriteId] = new SpriteAtlasEntry(spriteId, rect, sprite.Width, sprite.Height);

            _currentX += sprite.Width;
            _currentRowHeight = Math.Max(_currentRowHeight, sprite.Height);

            return true;
        }

        public SpriteAtlasEntry GetEntry(string spriteId)
        {
            SpriteAtlasEntry entry;
            _entries.TryGetValue(spriteId, out entry);
            return entry;
        }

        /// <summary>
        /// Builds the master UI Item Icon atlas containing all 50+ weapons, armors, and materials.
        /// </summary>
        public static SpriteAtlasBuilder BuildItemIconAtlas()
        {
            var atlas = new SpriteAtlasBuilder(256, 256);
            var catalog = ItemDatabase.GetAll();

            foreach (var item in catalog)
            {
                var icon = PixelAssetGenerator.GenerateItemIcon(item.Id);
                atlas.AddSprite(item.Id, icon);
            }

            return atlas;
        }

        public static SpriteAtlasBuilder BuildItemsAtlas()
        {
            return BuildItemIconAtlas();
        }

        /// <summary>
        /// Builds the master Units atlas containing all 10 friendly unit classes.
        /// </summary>
        public static SpriteAtlasBuilder BuildUnitRosterAtlas()
        {
            var atlas = new SpriteAtlasBuilder(512, 1024);
            foreach (UnitClass unitClass in Enum.GetValues(typeof(UnitClass)))
            {
                var sheet = PixelAssetGenerator.GenerateUnitSpriteSheet(unitClass);
                atlas.AddSprite("unit_" + unitClass.ToString().ToLower(), sheet);
            }
            return atlas;
        }

        public static SpriteAtlasBuilder BuildUnitsAtlas()
        {
            return BuildUnitRosterAtlas();
        }

        /// <summary>
        /// Builds the master Enemy & Colossal Boss atlas.
        /// </summary>
        public static SpriteAtlasBuilder BuildEnemyRosterAtlas()
        {
            var atlas = new SpriteAtlasBuilder(1024, 1024);
            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                var sheet = PixelAssetGenerator.GenerateEnemySpriteSheet(kind);
                atlas.AddSprite("enemy_" + kind.ToString().ToLower(), sheet);
            }
            return atlas;
        }

        public static SpriteAtlasBuilder BuildEnemiesAtlas()
        {
            return BuildEnemyRosterAtlas();
        }

        /// <summary>
        /// Exports the atlas sprite entry bounding boxes and UV coordinates to JSON format.
        /// </summary>
        public string ToManifestJson()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\n");
            sb.AppendFormat("  \"atlasWidth\": {0},\n", _atlasWidth);
            sb.AppendFormat("  \"atlasHeight\": {0},\n", _atlasHeight);
            sb.Append("  \"sprites\": [\n");

            int idx = 0;
            foreach (var kvp in _entries)
            {
                var e = kvp.Value;
                sb.Append("    {\n");
                sb.AppendFormat("      \"name\": \"{0}\",\n", e.SpriteId);
                sb.AppendFormat("      \"x\": {0},\n", e.AtlasRect.X);
                sb.AppendFormat("      \"y\": {0},\n", e.AtlasRect.Y);
                sb.AppendFormat("      \"w\": {0},\n", e.AtlasRect.Width);
                sb.AppendFormat("      \"h\": {0},\n", e.AtlasRect.Height);
                sb.AppendFormat("      \"u0\": {0:F4},\n", (float)e.AtlasRect.X / _atlasWidth);
                sb.AppendFormat("      \"v0\": {0:F4},\n", (float)e.AtlasRect.Y / _atlasHeight);
                sb.AppendFormat("      \"u1\": {0:F4},\n", (float)(e.AtlasRect.X + e.AtlasRect.Width) / _atlasWidth);
                sb.AppendFormat("      \"v1\": {0:F4}\n", (float)(e.AtlasRect.Y + e.AtlasRect.Height) / _atlasHeight);
                sb.Append("    }");
                if (idx < _entries.Count - 1) sb.Append(",");
                sb.Append("\n");
                idx++;
            }

            sb.Append("  ]\n");
            sb.Append("}");
            return sb.ToString();
        }
    }
}
