using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public enum SceneType
    {
        BootSplash,
        CampHub,
        BattleScene,
        CutsceneDialogue
    }

    public class GameObjectDescriptor
    {
        public string Name;
        public string ComponentType;
        public List<string> RequiredChildren;
        public Dictionary<string, string> Properties;

        public GameObjectDescriptor(string name, string componentType)
        {
            Name = name;
            ComponentType = componentType;
            RequiredChildren = new List<string>();
            Properties = new Dictionary<string, string>();
        }
    }

    public class SceneSetupTemplate
    {
        public SceneType Scene;
        public string SceneName;
        public List<GameObjectDescriptor> RootGameObjects;

        public SceneSetupTemplate(SceneType scene, string sceneName)
        {
            Scene = scene;
            SceneName = sceneName;
            RootGameObjects = new List<GameObjectDescriptor>();
        }
    }

    /// <summary>
    /// Master registry of Unity Scene Prefab templates, component hierarchies,
    /// and scene bootstrapping definitions for instant Unity Editor assembly.
    /// </summary>
    public static class ScenePrefabRegistry
    {
        private static readonly Dictionary<SceneType, SceneSetupTemplate> _templates = new Dictionary<SceneType, SceneSetupTemplate>();

        static ScenePrefabRegistry()
        {
            InitializeBattleSceneTemplate();
            InitializeCampSceneTemplate();
        }

        public static SceneSetupTemplate GetTemplate(SceneType sceneType)
        {
            SceneSetupTemplate template;
            if (_templates.TryGetValue(sceneType, out template)) return template;
            return _templates[SceneType.BattleScene];
        }

        private static void InitializeBattleSceneTemplate()
        {
            var battle = new SceneSetupTemplate(SceneType.BattleScene, "BattleScene");

            // 1. Camera & Render Pipeline
            var cam = new GameObjectDescriptor("PixelPerfectMainCamera", "PixelCamera");
            cam.Properties["OrthoSize"] = "5.0";
            cam.Properties["PixelSnap"] = "True";
            battle.RootGameObjects.Add(cam);

            // 2. 2D Lighting Rig
            var lightRig = new GameObjectDescriptor("Moonlighter2DLighting", "MoonlighterLighting");
            lightRig.RequiredChildren.Add("GlobalSunLight");
            lightRig.RequiredChildren.Add("TorchPointLight_Squad");
            lightRig.RequiredChildren.Add("FeverAuraLight");
            battle.RootGameObjects.Add(lightRig);

            // 3. Battle Simulation Controller
            var battleCtrl = new GameObjectDescriptor("BattleController", "BattleController");
            battleCtrl.RequiredChildren.Add("SquadRoot");
            battleCtrl.RequiredChildren.Add("EnemySpawnRoot");
            battleCtrl.RequiredChildren.Add("ProjectileRoot");
            battleCtrl.RequiredChildren.Add("VFXRoot");
            battle.RootGameObjects.Add(battleCtrl);

            // 4. Parallax Environment View
            var env = new GameObjectDescriptor("ParallaxEnvironment", "EnvironmentView");
            env.RequiredChildren.Add("BackgroundFarLayer");
            env.RequiredChildren.Add("BackgroundMidLayer");
            env.RequiredChildren.Add("ForegroundGroundTilemap");
            env.RequiredChildren.Add("WeatherParticleEmitter");
            battle.RootGameObjects.Add(env);

            // 5. Canvas HUD
            var hud = new GameObjectDescriptor("BattleHUDCanvas", "BattleUIController");
            hud.RequiredChildren.Add("RhythmChantBar");
            hud.RequiredChildren.Add("BeatPulseBorder");
            hud.RequiredChildren.Add("FeverGauge");
            hud.RequiredChildren.Add("MiracleDanceOverlay");
            hud.RequiredChildren.Add("SquadStatusPanel");
            hud.RequiredChildren.Add("WeatherWindIndicator");
            battle.RootGameObjects.Add(hud);

            _templates[SceneType.BattleScene] = battle;
        }

        private static void InitializeCampSceneTemplate()
        {
            var camp = new SceneSetupTemplate(SceneType.CampHub, "CampScene");

            var cam = new GameObjectDescriptor("CampMainCamera", "Camera");
            camp.RootGameObjects.Add(cam);

            var campCtrl = new GameObjectDescriptor("CampController", "CampController");
            camp.RootGameObjects.Add(campCtrl);

            var campCanvas = new GameObjectDescriptor("CampCanvas", "CampUIController");
            campCanvas.RequiredChildren.Add("BarracksModal");
            campCanvas.RequiredChildren.Add("BlacksmithForgeModal");
            campCanvas.RequiredChildren.Add("MaterTreeAwakeningModal");
            campCanvas.RequiredChildren.Add("CampChefStewModal");
            campCanvas.RequiredChildren.Add("BossHuntingMapModal");
            campCanvas.RequiredChildren.Add("SettingsMenuModal");
            camp.RootGameObjects.Add(campCanvas);

            var dialogueCanvas = new GameObjectDescriptor("DialogueCanvas", "DialogueUIController");
            dialogueCanvas.RequiredChildren.Add("HighPriestessPortrait");
            dialogueCanvas.RequiredChildren.Add("TypewriterTextBox");
            camp.RootGameObjects.Add(dialogueCanvas);

            _templates[SceneType.CampHub] = camp;
        }
    }
}
