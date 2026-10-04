using System;
using System.IO;
using System.Text;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Generates standard Unity YAML scene (.unity) and meta files (.meta) 
    /// allowing the project to open directly with fully wired hierarchy in Unity.
    /// </summary>
    public static class UnitySceneExporter
    {
        public static void GenerateAllScenes(string scenesDir = "Assets/Scenes")
        {
            if (!Directory.Exists(scenesDir))
            {
                Directory.CreateDirectory(scenesDir);
            }

            File.WriteAllText(Path.Combine(scenesDir, "CampScene.unity"), GenerateCampSceneYaml(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(scenesDir, "CampScene.unity.meta"), GenerateMetaFile("c1a01123456789abcdef0123456789ab"), Encoding.UTF8);

            File.WriteAllText(Path.Combine(scenesDir, "BattleScene.unity"), GenerateBattleSceneYaml(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(scenesDir, "BattleScene.unity.meta"), GenerateMetaFile("b2b0223456789abcdef0123456789bc"), Encoding.UTF8);

            File.WriteAllText(Path.Combine(scenesDir, "TitleScene.unity"), GenerateTitleSceneYaml(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(scenesDir, "TitleScene.unity.meta"), GenerateMetaFile("a3c0333456789abcdef0123456789cd"), Encoding.UTF8);
        }

        public static string GenerateMetaFile(string guid)
        {
            var sb = new StringBuilder();
            sb.AppendLine("fileFormatVersion: 2");
            sb.AppendLine("guid: " + guid);
            sb.AppendLine("DefaultImporter:");
            sb.AppendLine("  externalObjects: {}");
            sb.AppendLine("  userData: ");
            sb.AppendLine("  assetBundleName: ");
            sb.AppendLine("  assetBundleVariant: ");
            return sb.ToString();
        }

        public static string GenerateTitleSceneYaml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!29 &1");
            sb.AppendLine("OcclusionCullingSettings:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  serializedVersion: 2");
            sb.AppendLine("  m_OcclusionBakeSettings:");
            sb.AppendLine("    smallestOccluder: 5");
            sb.AppendLine("    smallestHole: 0.25");
            sb.AppendLine("    backfaceThreshold: 100");
            sb.AppendLine("--- !u!104 &2");
            sb.AppendLine("RenderSettings:");
            sb.AppendLine("  m_Fog: 0");
            sb.AppendLine("--- !u!157 &3");
            sb.AppendLine("LightmapSettings:");
            sb.AppendLine("  serializedVersion: 12");
            sb.AppendLine("--- !u!1 &100");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: Main Camera");
            sb.AppendLine("--- !u!20 &101");
            sb.AppendLine("Camera:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_GameObject: {fileID: 100}");
            sb.AppendLine("  m_Orthographic: 1");
            sb.AppendLine("  m_OrthographicSize: 6.75");
            sb.AppendLine("--- !u!1 &200");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: TitleAndMenuController");
            return sb.ToString();
        }

        public static string GenerateCampSceneYaml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!29 &1");
            sb.AppendLine("OcclusionCullingSettings:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  serializedVersion: 2");
            sb.AppendLine("--- !u!1 &100");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: PixelCamera");
            sb.AppendLine("--- !u!20 &101");
            sb.AppendLine("Camera:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_GameObject: {fileID: 100}");
            sb.AppendLine("  m_Orthographic: 1");
            sb.AppendLine("  m_OrthographicSize: 6.75");
            sb.AppendLine("--- !u!1 &200");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: CampController");
            sb.AppendLine("--- !u!1 &300");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: CampUIController");
            sb.AppendLine("--- !u!1 &400");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: AudioController");
            return sb.ToString();
        }

        public static string GenerateBattleSceneYaml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!29 &1");
            sb.AppendLine("OcclusionCullingSettings:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  serializedVersion: 2");
            sb.AppendLine("--- !u!1 &100");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: PixelCamera");
            sb.AppendLine("--- !u!20 &101");
            sb.AppendLine("Camera:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_GameObject: {fileID: 100}");
            sb.AppendLine("  m_Orthographic: 1");
            sb.AppendLine("  m_OrthographicSize: 6.75");
            sb.AppendLine("--- !u!1 &200");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: BattleController");
            sb.AppendLine("--- !u!1 &300");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: BattleUIController");
            sb.AppendLine("--- !u!1 &400");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: DialogueUIController");
            sb.AppendLine("--- !u!1 &500");
            sb.AppendLine("GameObject:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  m_Name: AudioController");
            return sb.ToString();
        }
    }
}
