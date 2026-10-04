using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmArmy.UnityPresentation
{
    /// <summary>
    /// Runtime bridge for the production battle vertical slice.
    /// The core battle simulation is intentionally engine-agnostic; this presenter
    /// turns its UnitView/EnemyView models and procedural pixel art into Unity sprites.
    /// </summary>
    public sealed class BattleSceneRuntimePresenter : MonoBehaviour
    {
        private const float WorldScale = 0.02f;
        private const float GroundY = -3.15f;
        private const float PixelsPerUnit = 16f;

        private BattleController _battle;
        private Camera _camera;
        private readonly Dictionary<string, RuntimeActor> _actors = new Dictionary<string, RuntimeActor>();
        private readonly Dictionary<UnitClass, Texture2D> _unitTextures = new Dictionary<UnitClass, Texture2D>();
        private readonly Dictionary<EnemyKind, Texture2D> _enemyTextures = new Dictionary<EnemyKind, Texture2D>();
        private readonly List<Texture2D> _ownedTextures = new List<Texture2D>();
        private readonly List<Sprite> _ownedSprites = new List<Sprite>();
        private readonly List<GameObject> _backgroundObjects = new List<GameObject>();
        private readonly List<SpriteRenderer> _backgroundRenderers = new List<SpriteRenderer>();
        private readonly List<RuntimeActor> _runtimeActors = new List<RuntimeActor>();

        private Texture2D _environmentSky;
        private Texture2D _environmentFar;
        private Texture2D _environmentMid;
        private Texture2D _environmentGround;

        private float _cameraX;
        private bool _started;
        private string _lastTerrain = "Firm Ground";

        private sealed class RuntimeActor
        {
            public GameObject Object;
            public SpriteRenderer Renderer;
            public bool IsEnemy;
            public string Id;
            public UnitView Unit;
            public EnemyView Enemy;
            public Texture2D Sheet;
            public int SpriteSize;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "Battle_CoralCoast_01" && scene.name != "BattleScene")
                return;

            if (FindObjectOfType<BattleSceneRuntimePresenter>() != null)
                return;

            var go = new GameObject("BattleSceneRuntimePresenter");
            DontDestroyOnLoad(go);
            go.AddComponent<BattleSceneRuntimePresenter>();
        }

        private void Start()
        {
            if (_started) return;
            _started = true;

            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("BattleCamera");
                _camera = cameraObject.AddComponent<Camera>();
                _camera.tag = "MainCamera";
            }

            ConfigureCamera();
            DisableGreybox();
            BuildProductionEnvironment();

            var save = SaveSystem.Load();
            var mission = MissionDatabase.Get("coast-hunt");
            if (mission == null)
            {
                Debug.LogError("[Rhythm Army] Could not resolve coast-hunt mission.");
                return;
            }

            _battle = new BattleController();
            _battle.StartBattle(mission, save.Roster);
            _battle.OnCombatFeedback += HandleCombatFeedback;
            _lastTerrain = _battle.BattleManager.State.CurrentTerrain.DisplayName;

            BuildInitialActors();
            CenterCameraImmediate();
        }

        private void ConfigureCamera()
        {
            _camera.orthographic = true;
            _camera.orthographicSize = 6.75f;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.backgroundColor = new Color(0.08f, 0.11f, 0.15f, 1f);
        }

        private void DisableGreybox()
        {
            var greybox = GameObject.Find("JOURNEY_GREYBOX");
            if (greybox != null)
                greybox.SetActive(false);

            var layout = GameObject.Find("JourneyGreyboxLayout");
            if (layout != null)
                layout.SetActive(false);
        }

        private void BuildProductionEnvironment()
        {
            var sky = EnvironmentArtGenerator.GenerateBiomeBackdrop(BiomeType.CoralCoast, ParallaxLayerRole.SkyAtmosphere);
            var far = EnvironmentArtGenerator.GenerateBiomeBackdrop(BiomeType.CoralCoast, ParallaxLayerRole.FarScenery);
            var mid = EnvironmentArtGenerator.GenerateBiomeBackdrop(BiomeType.CoralCoast, ParallaxLayerRole.MidTerrain);
            var ground = EnvironmentArtGenerator.GenerateBiomeBackdrop(BiomeType.CoralCoast, ParallaxLayerRole.ForegroundGround);

            _environmentSky = CreateTexture(sky, "CoralCoast_Sky");
            _environmentFar = CreateTexture(far, "CoralCoast_Far");
            _environmentMid = CreateTexture(mid, "CoralCoast_Mid");
            _environmentGround = CreateTexture(ground, "CoralCoast_Ground");

            CreateBackgroundLayer(_environmentSky, 0.00f, 20);
            CreateBackgroundLayer(_environmentFar, 0.18f, 19);
            CreateBackgroundLayer(_environmentMid, 0.42f, 18);
            CreateBackgroundLayer(_environmentGround, 0.78f, 17);

            // A restrained foreground path gives the units a clear marching lane.
            var pathObject = new GameObject("ProductionPath");
            pathObject.transform.position = new Vector3(26f, -4.25f, 4f);
            var pathRenderer = pathObject.AddComponent<SpriteRenderer>();
            pathRenderer.sprite = CreatePixelSprite(CreatePathTexture(), "CoralPath", 256, 32, 16);
            pathRenderer.sortingOrder = 25;
            _backgroundObjects.Add(pathObject);
        }

        private PixelBitmapBuffer CreatePathTexture()
        {
            var buffer = new PixelBitmapBuffer(256, 32);
            var sandDark = PixelColor32.FromHex("#5B4633");
            var sandMid = PixelColor32.FromHex("#9A7650");
            var sandLight = PixelColor32.FromHex("#D1A36C");
            var foam = PixelColor32.FromHex("#D9E7D6");

            buffer.FillRect(0, 0, 256, 32, sandDark);
            buffer.FillRect(0, 0, 256, 25, sandMid);
            for (int x = 0; x < 256; x += 11)
            {
                buffer.FillRect(x, 20 + (x % 5), 5, 2, sandLight);
                if (x % 22 == 0) buffer.SetPixel(x + 2, 16, foam);
            }
            buffer.DrawLine(0, 24, 255, 24, sandLight);
            return buffer;
        }

        private void CreateBackgroundLayer(Texture2D texture, float parallax, int sortingOrder)
        {
            if (texture == null) return;

            float width = texture.width / PixelsPerUnit;
            int tiles = 5;
            for (int i = -2; i <= 2; i++)
            {
                var go = new GameObject("BackgroundLayer_" + sortingOrder + "_" + i);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    PixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
                renderer.sortingOrder = sortingOrder;
                renderer.transform.position = new Vector3(i * width, 0f, 10f - sortingOrder * 0.01f);
                _backgroundObjects.Add(go);
                _backgroundRenderers.Add(renderer);
            }
        }

        private void BuildInitialActors()
        {
            foreach (var unit in _battle.UnitViews)
                CreateUnitActor(unit);

            foreach (var enemy in _battle.EnemyViews)
                CreateEnemyActor(enemy);
        }

        private void CreateUnitActor(UnitView unit)
        {
            if (unit == null || unit.UnitMember == null) return;
            var id = "unit_" + unit.LiveData.Member.Id;
            var go = new GameObject(id);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 100 + unit.UnitMember.Class.GetHashCode() % 10;

            var texture = GetUnitTexture(unit.UnitMember.Class, unit.UnitMember.Subspecies);
            var size = PixelSpriteRegistry.GetSpec(unit.UnitMember.Class).SpriteWidth;
            var actor = new RuntimeActor
            {
                Object = go,
                Renderer = renderer,
                IsEnemy = false,
                Id = id,
                Unit = unit,
                Sheet = texture,
                SpriteSize = size
            };
            _actors[id] = actor;
            _runtimeActors.Add(actor);
            ApplyUnitSprite(actor);
        }

        private void CreateEnemyActor(EnemyView enemy)
        {
            if (enemy == null || enemy.LiveData == null) return;
            var id = "enemy_" + enemy.LiveData.Id;
            var go = new GameObject(id);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 90;

            var texture = GetEnemyTexture(enemy.Kind);
            var actor = new RuntimeActor
            {
                Object = go,
                Renderer = renderer,
                IsEnemy = true,
                Id = id,
                Enemy = enemy,
                Sheet = texture,
                SpriteSize = enemy.SpriteSize
            };
            _actors[id] = actor;
            _runtimeActors.Add(actor);
            ApplyEnemySprite(actor);
        }

        private Texture2D GetUnitTexture(UnitClass unitClass, Subspecies species)
        {
            if (_unitTextures.TryGetValue(unitClass, out var cached))
                return cached;

            var buffer = PixelAssetGenerator.GenerateUnitSpriteSheet(unitClass, species);
            var texture = CreateTexture(buffer, "Unit_" + unitClass);
            _unitTextures[unitClass] = texture;
            return texture;
        }

        private Texture2D GetEnemyTexture(EnemyKind kind)
        {
            if (_enemyTextures.TryGetValue(kind, out var cached))
                return cached;

            var buffer = PixelAssetGenerator.GenerateEnemySpriteSheet(kind);
            var texture = CreateTexture(buffer, "Enemy_" + kind);
            _enemyTextures[kind] = texture;
            return texture;
        }

        private void ApplyUnitSprite(RuntimeActor actor)
        {
            if (actor == null || actor.Unit == null || actor.Renderer == null || actor.Sheet == null)
                return;

            var frame = actor.Unit.Animator.CurrentFrame;
            var size = actor.SpriteSize;
            int row = frame != null ? frame.Rect.Y / 32 : 0;
            int index = frame != null ? frame.Rect.X / 32 : 0;

            // PixelAnimator now uses each class's actual production frame size.
            row = frame != null ? frame.Rect.Y / size : 0;
            index = frame != null ? frame.Rect.X / size : 0;

            actor.Renderer.sprite = CreateSheetSprite(actor.Sheet, index * size, row * size, size, size, "UnitFrame");
            actor.Renderer.flipX = !actor.Unit.FacingRight;
            actor.Object.transform.position = ToWorldPosition(actor.Unit.VisualX, actor.Unit.VisualY, 0f);
        }

        private void ApplyEnemySprite(RuntimeActor actor)
        {
            if (actor == null || actor.Enemy == null || actor.Renderer == null || actor.Sheet == null)
                return;

            int size = actor.SpriteSize;
            var frame = actor.Enemy.Animator.CurrentFrame;
            int row = frame != null ? frame.Rect.Y / 32 : 0;
            int index = frame != null ? frame.Rect.X / 32 : 0;
            actor.Renderer.sprite = CreateSheetSprite(actor.Sheet, index * size, row * size, size, size, "EnemyFrame");
            actor.Object.transform.position = ToWorldPosition(actor.Enemy.VisualX, actor.Enemy.VisualY, 0.4f);
            actor.Object.transform.localScale = Vector3.one * actor.Enemy.SpriteScale;
        }

        private Sprite CreateSheetSprite(Texture2D texture, int x, int y, int width, int height, string name)
        {
            // PixelBitmapBuffer is top-left based; Unity texture coordinates are bottom-left.
            int flippedY = texture.height - y - height;
            var sprite = Sprite.Create(
                texture,
                new Rect(x, flippedY, width, height),
                new Vector2(0.5f, 0.0f),
                PixelsPerUnit);
            sprite.name = name;
            _ownedSprites.Add(sprite);
            return sprite;
        }

        private Sprite CreatePixelSprite(PixelBitmapBuffer buffer, string name, int width, int height, int ppu)
        {
            var texture = CreateTexture(buffer, name);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), ppu);
            _ownedSprites.Add(sprite);
            return sprite;
        }

        private Texture2D CreateTexture(PixelBitmapBuffer buffer, string name)
        {
            if (buffer == null) return null;

            var texture = new Texture2D(buffer.Width, buffer.Height, TextureFormat.RGBA32, false);
            texture.name = name;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var colors = new Color32[buffer.Width * buffer.Height];

            for (int y = 0; y < buffer.Height; y++)
            {
                for (int x = 0; x < buffer.Width; x++)
                {
                    var pixel = buffer.GetPixel(x, y);
                    int unityY = buffer.Height - 1 - y;
                    colors[unityY * buffer.Width + x] =
                        new Color32(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            texture.SetPixels32(colors);
            texture.Apply(false, false);
            _ownedTextures.Add(texture);
            return texture;
        }

        private Vector3 ToWorldPosition(float simX, float simY, float z)
        {
            float x = (simX - 100f) * WorldScale;
            float y = GroundY + simY * WorldScale;
            return new Vector3(x, y, z);
        }

        private void CenterCameraImmediate()
        {
            if (_camera == null || _battle == null || _battle.BattleManager.State == null) return;
            _cameraX = (_battle.BattleManager.State.BannerX - 100f) * WorldScale;
            _camera.transform.position = new Vector3(_cameraX, 0f, -10f);
        }

        private void Update()
        {
            if (!_started || _battle == null || _battle.BattleManager.State == null)
                return;

            HandleDrumInput();
            _battle.Update(Time.deltaTime);
            SyncActors();
            SyncCamera();
            SyncBackground();
        }

        private void HandleDrumInput()
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.J))
                _battle.HandleInput(DrumId.Boom);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.K))
                _battle.HandleInput(DrumId.Tak);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.I))
                _battle.HandleInput(DrumId.Rat);
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.L))
                _battle.HandleInput(DrumId.Ting);
        }

        private void SyncActors()
        {
            var activeIds = new HashSet<string>();

            foreach (var unit in _battle.UnitViews)
            {
                if (unit == null || unit.LiveData == null) continue;
                string id = "unit_" + unit.LiveData.Member.Id;
                activeIds.Add(id);

                if (!_actors.TryGetValue(id, out var actor))
                    CreateUnitActor(unit);
                else
                    ApplyUnitSprite(actor);
            }

            foreach (var enemy in _battle.EnemyViews)
            {
                if (enemy == null || enemy.LiveData == null) continue;
                string id = "enemy_" + enemy.LiveData.Id;
                activeIds.Add(id);

                if (!_actors.TryGetValue(id, out var actor))
                    CreateEnemyActor(enemy);
                else
                    ApplyEnemySprite(actor);

                if (enemy.CurrentState == EnemyVisualState.Dead)
                    actor.Renderer.enabled = false;
            }

            foreach (var actor in _runtimeActors)
            {
                if (actor == null || actor.Object == null) continue;
                if (actor.IsEnemy && actor.Enemy != null && !actor.Enemy.LiveData.IsAlive)
                    actor.Renderer.enabled = false;
                else
                    actor.Renderer.enabled = true;
            }
        }

        private void SyncCamera()
        {
            var state = _battle.BattleManager.State;
            float targetX = (state.BannerX - 100f) * WorldScale;
            _cameraX = Mathf.Lerp(_cameraX, targetX, 1f - Mathf.Exp(-7f * Time.deltaTime));
            _camera.transform.position = new Vector3(_cameraX, 0f, -10f);
        }

        private void SyncBackground()
        {
            float tileWidth = 384f / PixelsPerUnit;
            int index = 0;
            foreach (var renderer in _backgroundRenderers)
            {
                if (renderer == null) continue;
                int tileIndex = (index % 5) - 2;
                float parallax = renderer.sortingOrder == 20 ? 0f :
                                 renderer.sortingOrder == 19 ? 0.18f :
                                 renderer.sortingOrder == 18 ? 0.42f : 0.78f;
                renderer.transform.position = new Vector3(
                    _cameraX * parallax + tileIndex * tileWidth,
                    0f,
                    renderer.transform.position.z);
                index++;
            }
        }

        private void HandleCombatFeedback(CombatFeedbackEvent feedback)
        {
            if (feedback == null) return;
            if (feedback.Type == CombatFeedbackType.TerrainChanged)
                _lastTerrain = feedback.AttackName;
        }

        private void OnGUI()
        {
            if (!_started || _battle == null || _battle.BattleManager.State == null)
                return;

            var state = _battle.BattleManager.State;
            GUI.color = new Color(0.96f, 0.91f, 0.76f, 1f);

            GUI.Box(new Rect(16f, 14f, 360f, 92f), string.Empty);
            GUI.Label(new Rect(30f, 24f, 330f, 22f), "TIDEBREAK LANDING  •  CORAL COAST");
            GUI.Label(new Rect(30f, 47f, 330f, 20f),
                string.Format("Formation  {0:0}%    Pressure {1:0}%", state.FormationIntegrity, state.FormationPressure));
            GUI.Label(new Rect(30f, 68f, 330f, 20f),
                string.Format("Terrain  {0}    Combo {1}", _lastTerrain, _battle.BattleManager.Rhythm.Combo));

            GUI.Box(new Rect(Screen.width - 350f, Screen.height - 92f, 334f, 76f), string.Empty);
            GUI.Label(new Rect(Screen.width - 334f, Screen.height - 82f, 310f, 20f), "DRUMS");
            GUI.Label(new Rect(Screen.width - 334f, Screen.height - 60f, 310f, 20f),
                "A / S / D / F  •  Boom  Tak  Rat  Ting");
            GUI.Label(new Rect(Screen.width - 334f, Screen.height - 38f, 310f, 20f),
                "Match the beat to March / Attack / Defend / Charge");
        }

        private void OnDestroy()
        {
            foreach (var sprite in _ownedSprites)
                if (sprite != null) Destroy(sprite);
            foreach (var texture in _ownedTextures)
                if (texture != null) Destroy(texture);
            foreach (var go in _backgroundObjects)
                if (go != null) Destroy(go);
        }
    }
}
