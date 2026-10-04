using System;
using System.IO;
using System.Collections.Generic;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Visuals
{
    public class AssetExportSummary
    {
        public int TotalFilesExported;
        public long TotalBytesWritten;
        public string OutputDirectory;
        public List<string> ExportedFilePaths = new List<string>();
    }

    /// <summary>
    /// Master Asset Pipeline Exporter: Rasterizes all procedural pixel art assets
    /// (Units, Enemies, Camp NPCs/Buildings, VFX, Projectiles, UI Frames, Drum Buttons,
    /// Biome Backdrops, Item Icons, and Packed Texture Atlases) and exports them to disk
    /// as standard PNG files with accompanying Unity sprite slice metadata manifests.
    /// </summary>
    public static class AssetDiskExporter
    {
        public static AssetExportSummary ExportAllAssetsToDisk(string baseDir = "Assets/Sprites")
        {
            var summary = new AssetExportSummary
            {
                OutputDirectory = baseDir
            };

            string unitsDir = Path.Combine(baseDir, "Units");
            string enemiesDir = Path.Combine(baseDir, "Enemies");
            string campDir = Path.Combine(baseDir, "Camp");
            string vfxDir = Path.Combine(baseDir, "VFX");
            string uiDir = Path.Combine(baseDir, "UI");
            string envDir = Path.Combine(baseDir, "Environment");
            string iconsDir = Path.Combine(baseDir, "Icons");
            string atlasesDir = Path.Combine(baseDir, "Atlases");
            string normalsDir = Path.Combine(baseDir, "NormalMaps");
            string screensDir = Path.Combine(baseDir, "Screens");

            EnsureDirectory(unitsDir);
            EnsureDirectory(enemiesDir);
            EnsureDirectory(campDir);
            EnsureDirectory(vfxDir);
            EnsureDirectory(uiDir);
            EnsureDirectory(envDir);
            EnsureDirectory(iconsDir);
            EnsureDirectory(atlasesDir);
            EnsureDirectory(normalsDir);
            EnsureDirectory(screensDir);

            // 1. Unit Spritesheets (10 Classes + Subspecies)
            foreach (UnitClass uClass in Enum.GetValues(typeof(UnitClass)))
            {
                var sheet = PixelAssetGenerator.GenerateUnitSpriteSheet(uClass, Subspecies.Normal);
                string path = Path.Combine(unitsDir, string.Format("unit_{0}.png", uClass.ToString().ToLower()));
                SavePng(sheet, path, summary);

                // Subspecies variants for core classes
                if (uClass == UnitClass.Swordsman || uClass == UnitClass.Spearman || uClass == UnitClass.Archer)
                {
                    foreach (Subspecies sub in Enum.GetValues(typeof(Subspecies)))
                    {
                        if (sub == Subspecies.Normal) continue;
                        var subSheet = PixelAssetGenerator.GenerateUnitSpriteSheet(uClass, sub);
                        string subPath = Path.Combine(unitsDir, string.Format("unit_{0}_{1}.png", uClass.ToString().ToLower(), sub.ToString().ToLower()));
                        SavePng(subSheet, subPath, summary);
                    }
                }
            }

            // 2. Enemy & Colossal Boss Spritesheets (17 Types)
            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                var sheet = PixelAssetGenerator.GenerateEnemySpriteSheet(kind);
                string path = Path.Combine(enemiesDir, string.Format("enemy_{0}.png", kind.ToString().ToLower()));
                SavePng(sheet, path, summary);
            }

            // 3. Camp NPCs & Structures
            foreach (CampNPCType npc in Enum.GetValues(typeof(CampNPCType)))
            {
                var sheet = CampArtGenerator.GenerateNPCSpriteSheet(npc);
                string path = Path.Combine(campDir, string.Format("npc_{0}.png", npc.ToString().ToLower()));
                SavePng(sheet, path, summary);
            }

            foreach (CampStructureType structType in Enum.GetValues(typeof(CampStructureType)))
            {
                var structure = CampArtGenerator.GenerateCampStructure(structType);
                string path = Path.Combine(campDir, string.Format("structure_{0}.png", structType.ToString().ToLower()));
                SavePng(structure, path, summary);
            }

            // 4. VFX Sheets & Ballistics & Relic Masks
            foreach (VFXSpriteType vfx in Enum.GetValues(typeof(VFXSpriteType)))
            {
                var sheet = VFXArtGenerator.GenerateVFXSpriteSheet(vfx);
                string path = Path.Combine(vfxDir, string.Format("vfx_{0}.png", vfx.ToString().ToLower()));
                SavePng(sheet, path, summary);
            }

            foreach (ProjectileArtType proj in Enum.GetValues(typeof(ProjectileArtType)))
            {
                var sprite = VFXArtGenerator.GenerateProjectileSprite(proj);
                string path = Path.Combine(vfxDir, string.Format("proj_{0}.png", proj.ToString().ToLower()));
                SavePng(sprite, path, summary);
            }

            string[] maskIds = { "hero-mask-courage", "hero-mask-valor", "hero-mask-wrath", "hero-mask-apex" };
            foreach (var maskId in maskIds)
            {
                var mask = VFXArtGenerator.GenerateHeroRelicMask(maskId);
                string path = Path.Combine(vfxDir, string.Format("mask_{0}.png", maskId.Replace("hero-mask-", "")));
                SavePng(mask, path, summary);
            }

            // 5. UI Elements & Drum Chant Buttons
            foreach (DrumId drum in Enum.GetValues(typeof(DrumId)))
            {
                foreach (DrumButtonState state in Enum.GetValues(typeof(DrumButtonState)))
                {
                    var btn = UIArtGenerator.GenerateDrumButton(drum, state);
                    string path = Path.Combine(uiDir, string.Format("drum_{0}_{1}.png", drum.ToString().ToLower(), state.ToString().ToLower()));
                    SavePng(btn, path, summary);
                }
            }

            SavePng(UIArtGenerator.GenerateDialogueFrame(), Path.Combine(uiDir, "ui_dialogue_frame.png"), summary);
            SavePng(UIArtGenerator.GenerateRhythmBeatTrack(), Path.Combine(uiDir, "ui_beat_track.png"), summary);

            foreach (CurrencyCoinType coin in Enum.GetValues(typeof(CurrencyCoinType)))
            {
                var coinSprite = UIArtGenerator.GenerateCurrencyCoin(coin);
                SavePng(coinSprite, Path.Combine(uiDir, string.Format("coin_{0}.png", coin.ToString().ToLower())), summary);
            }

            foreach (ChestTierType chest in Enum.GetValues(typeof(ChestTierType)))
            {
                SavePng(UIArtGenerator.GenerateLootChest(chest, false), Path.Combine(uiDir, string.Format("chest_{0}_closed.png", chest.ToString().ToLower())), summary);
                SavePng(UIArtGenerator.GenerateLootChest(chest, true), Path.Combine(uiDir, string.Format("chest_{0}_opened.png", chest.ToString().ToLower())), summary);
            }

            // 6. 8 Biome Parallax Backdrops
            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
            {
                foreach (ParallaxLayerRole layer in Enum.GetValues(typeof(ParallaxLayerRole)))
                {
                    var backdrop = EnvironmentArtGenerator.GenerateBiomeBackdrop(biome, layer);
                    string path = Path.Combine(envDir, string.Format("bg_{0}_{1}.png", biome.ToString().ToLower(), layer.ToString().ToLower()));
                    SavePng(backdrop, path, summary);
                }
            }

            // 7. Master Texture Atlases
            var itemsAtlas = SpriteAtlasBuilder.BuildItemsAtlas();
            SavePng(itemsAtlas.AtlasBuffer, Path.Combine(atlasesDir, "atlas_items.png"), summary);
            SaveManifest(itemsAtlas.ToManifestJson(), Path.Combine(atlasesDir, "atlas_items.json"), summary);

            var unitsAtlas = SpriteAtlasBuilder.BuildUnitsAtlas();
            SavePng(unitsAtlas.AtlasBuffer, Path.Combine(atlasesDir, "atlas_units.png"), summary);
            SaveManifest(unitsAtlas.ToManifestJson(), Path.Combine(atlasesDir, "atlas_units.json"), summary);

            var enemiesAtlas = SpriteAtlasBuilder.BuildEnemiesAtlas();
            SavePng(enemiesAtlas.AtlasBuffer, Path.Combine(atlasesDir, "atlas_enemies.png"), summary);
            SaveManifest(enemiesAtlas.ToManifestJson(), Path.Combine(atlasesDir, "atlas_enemies.json"), summary);

            // 8. 2D URP Normal Maps
            foreach (UnitClass uClass in Enum.GetValues(typeof(UnitClass)))
            {
                var diffuseSheet = PixelAssetGenerator.GenerateUnitSpriteSheet(uClass, Subspecies.Normal);
                var normalMap = NormalMapGenerator.GenerateNormalMap(diffuseSheet, 2.0f);
                string path = Path.Combine(normalsDir, string.Format("normal_unit_{0}.png", uClass.ToString().ToLower()));
                SavePng(normalMap, path, summary);
            }

            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                var diffuseSheet = PixelAssetGenerator.GenerateEnemySpriteSheet(kind);
                var normalMap = NormalMapGenerator.GenerateNormalMap(diffuseSheet, 2.5f);
                string path = Path.Combine(normalsDir, string.Format("normal_enemy_{0}.png", kind.ToString().ToLower()));
                SavePng(normalMap, path, summary);
            }

            foreach (CampStructureType structType in Enum.GetValues(typeof(CampStructureType)))
            {
                var diffuse = CampArtGenerator.GenerateCampStructure(structType);
                var normalMap = NormalMapGenerator.GenerateNormalMap(diffuse, 2.2f);
                string path = Path.Combine(normalsDir, string.Format("normal_structure_{0}.png", structType.ToString().ToLower()));
                SavePng(normalMap, path, summary);
            }

            // 9. Screen Plates: Title Splash & Campaign World Map
            var titleSplash = TitleAndMapArtGenerator.GenerateTitleScreenSplash();
            SavePng(titleSplash, Path.Combine(screensDir, "title_screen_splash.png"), summary);

            var worldMap = TitleAndMapArtGenerator.GenerateCampaignWorldMap();
            SavePng(worldMap, Path.Combine(screensDir, "world_map_campaign.png"), summary);

            // 10. Synthesized 16-Bit PCM WAV Audio Files
            ExportAllAudioToDisk("Assets/Audio", summary);

            return summary;
        }

        public static void ExportAllAudioToDisk(string audioBaseDir, AssetExportSummary summary = null)
        {
            string drumsDir = Path.Combine(audioBaseDir, "Drums");
            string chantsDir = Path.Combine(audioBaseDir, "Chants");
            string sfxDir = Path.Combine(audioBaseDir, "SFX");
            string musicDir = Path.Combine(audioBaseDir, "Music");

            EnsureDirectory(drumsDir);
            EnsureDirectory(chantsDir);
            EnsureDirectory(sfxDir);
            EnsureDirectory(musicDir);

            // Drums
            SaveWav(ProceduralAudioGenerator.GenerateDrumBoom(), Path.Combine(drumsDir, "drum_boom.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateDrumTak(), Path.Combine(drumsDir, "drum_tak.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateDrumRat(), Path.Combine(drumsDir, "drum_rat.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateDrumTing(), Path.Combine(drumsDir, "drum_ting.wav"), summary);

            // Vocal Chants
            string[] chants = { "boom", "tak", "rat", "ting", "fever" };
            foreach (var ch in chants)
            {
                SaveWav(ProceduralAudioGenerator.GenerateVocalChant(ch), Path.Combine(chantsDir, string.Format("chant_{0}.wav", ch)), summary);
            }

            // SFX
            SaveWav(ProceduralAudioGenerator.GenerateSwordSlash(), Path.Combine(sfxDir, "sfx_sword_slash.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateArrowWhistle(), Path.Combine(sfxDir, "sfx_arrow_whistle.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateShieldBlock(), Path.Combine(sfxDir, "sfx_shield_block.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateBossRoar(), Path.Combine(sfxDir, "sfx_boss_roar.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateCoinClink(), Path.Combine(sfxDir, "sfx_coin_clink.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateUIClick(), Path.Combine(sfxDir, "sfx_ui_click.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateAnvilStrike(), Path.Combine(sfxDir, "sfx_anvil_strike.wav"), summary);

            // Music / Fanfares
            SaveWav(ProceduralAudioGenerator.GenerateVictoryFanfare(), Path.Combine(musicDir, "mus_victory_fanfare.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateDefeatJingle(), Path.Combine(musicDir, "mus_defeat_jingle.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateBgmRhythmLoop(false), Path.Combine(musicDir, "bgm_battle_base.wav"), summary);
            SaveWav(ProceduralAudioGenerator.GenerateBgmRhythmLoop(true), Path.Combine(musicDir, "bgm_battle_fever.wav"), summary);
        }

        private static void SaveWav(byte[] wavBytes, string path, AssetExportSummary summary)
        {
            File.WriteAllBytes(path, wavBytes);
            if (summary != null)
            {
                summary.TotalFilesExported++;
                summary.TotalBytesWritten += wavBytes.Length;
                summary.ExportedFilePaths.Add(path);
            }
        }

        private static void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static void SavePng(PixelBitmapBuffer buffer, string path, AssetExportSummary summary)
        {
            byte[] pngBytes = buffer.ExportToPngBytes();
            File.WriteAllBytes(path, pngBytes);
            summary.TotalFilesExported++;
            summary.TotalBytesWritten += pngBytes.Length;
            summary.ExportedFilePaths.Add(path);
        }

        private static void SaveManifest(string json, string path, AssetExportSummary summary)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(path, bytes);
            summary.TotalFilesExported++;
            summary.TotalBytesWritten += bytes.Length;
            summary.ExportedFilePaths.Add(path);
        }
    }
}
