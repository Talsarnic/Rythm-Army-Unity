using UnityEditor;
using UnityEngine;

namespace RhythmArmy.Editor
{
    public class SpearmanAtlasImporter : AssetPostprocessor
    {
        private const string AtlasPath = "Assets/Art/Units/Spearman/atlas_units_spearman.png";
        private const int FrameSize = 32;
        private const int Columns = 4;
        private const int Rows = 10;

        void OnPreprocessTexture()
        {
            if (assetPath != AtlasPath) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = FrameSize;

            var sprites = new SpriteMetaData[Columns * Rows];
            int index = 0;
            string[] states =
            {
                "Idle", "March", "Attack", "Defend", "Fever",
                "Charge", "Jump", "Hurt", "Death", "HeroAbility"
            };

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    sprites[index] = new SpriteMetaData
                    {
                        name = "spearman_" + states[row].ToLowerInvariant() + "_" + col,
                        rect = new Rect(col * FrameSize, (Rows - 1 - row) * FrameSize, FrameSize, FrameSize),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f)
                    };
                    index++;
                }
            }

            importer.spritesheet = sprites;
        }

        [MenuItem("Rhythm Army/Art/Reimport Spearman Atlas")]
        private static void Reimport()
        {
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Rhythm Army: reimported Spearman production atlas.");
        }
    }
}
