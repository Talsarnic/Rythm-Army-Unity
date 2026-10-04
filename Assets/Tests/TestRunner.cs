using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Gameplay.Camp;
using RhythmArmy.Gameplay.Narrative;
using RhythmArmy.UI;
using RhythmArmy.Visuals;

namespace RhythmArmy.Tests
{
    public static class TestRunner
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static int Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(" RUNNING RHYTHM ARMY UNITY C# TEST SUITE ");
            Console.WriteLine("========================================");

            RunTest("Rhythm Engine: Tap & Command Evaluation", TestRhythmEngineCommandEvaluation);
            RunTest("Rhythm Engine: Combo & Fever Mode", TestRhythmEngineComboAndFever);
            RunTest("Rhythm Engine: Advance Beats & Response Phase", TestRhythmEngineAdvanceBeats);
            RunTest("Rhythm Engine: Compute Input Offset", TestComputeOffset);
            RunTest("Combat Formulas: Base HP & Level Scaling", TestCombatFormulasBaseHp);
            RunTest("Combat Formulas: Damage & Mitigation", TestCombatFormulasDamage);
            RunTest("Formation System: Squad Ranks & Offsets", TestFormationSystem);
            RunTest("Ballistics: Parabolic Trajectory", TestBallisticsCalculator);
            RunTest("Save & Inventory: Serialization Roundtrip", TestSaveAndInventory);
            RunTest("Camp: Equipment Auto-Optimizer", TestEquipmentOptimizer);
            RunTest("Battle: March, Collision & Attack Rules", TestCombatRules);
            RunTest("Battle: Enemy AI & Boss Telegraphs", TestEnemyAIAndBossTelegraphs);
            RunTest("Battle: Charge & Jump Combat Mechanics", TestChargeAndJumpCombatMechanics);
            RunTest("Battle: Loot Drops & Reward Payout", TestLootDropsAndBattlePayout);
            RunTest("Camp: Blacksmith Crafting & Recipes", TestBlacksmithCrafting);
            RunTest("Camp: Altar Evolution & Subspecies Scaling", TestAltarEvolution);
            RunTest("Camp: Barracks Recruitment & Equipment Loadout", TestBarracksRosterAndLoadout);
            RunTest("Visuals: Pixel Perfect & Lighting Profiles", TestVisualsAndLighting);
            RunTest("Visuals: Pixel Sprite Animation State Machine", TestPixelAnimationStateMachine);
            RunTest("Visuals: Pixel Camera & Screen Shake Trauma", TestPixelCameraAndScreenShake);
            RunTest("Audio: DSP Clock Timing", TestDSPAudioClock);
            RunTest("Audio: Rhythm Sequencer & Vocal Chants", TestRhythmAudioSequencer);
            RunTest("Battle: Mission Simulation & Battle Manager Loop", TestBattleManagerSimulation);
            RunTest("Visuals: Moonlighter Biome Parallax & Weather", TestEnvironmentAndParallaxSystem);
            RunTest("UI: Battle HUD & Camp HUD ViewModels", TestBattleHUDAndCampHUDState);
            RunTest("Narrative: Dialogue Player & Story Sequences", TestDialogueSystem);
            RunTest("Flow: Game Orchestrator & Campaign Progression", TestGameOrchestratorFlow);
            RunTest("Visuals: Unit, Enemy & Projectile Presentation Views", TestPresentationViews);
            RunTest("Controllers: Battle Scene & UI Controller Flow", TestBattleControllerAndUI);
            RunTest("Controllers: Camp Hub & UI Controller Operations", TestCampControllerAndUI);
            RunTest("Audio: Master Audio Controller & UI SFX", TestAudioControllerAndSound);
            RunTest("Registry: Moonlighter Sprite Anchors & Tilemaps", TestMoonlighterSpriteAndTilemapRegistry);
            RunTest("Enemies: 10 Rival Tribe Enemy Classes & Metadata", TestRivalTribeEnemyClasses);
            RunTest("Currency: Patapon-Style Kill Drops & Mission Rewards", TestCurrencyDropSystem);
            RunTest("Runtime: Input Manager & Standalone Game Loop", TestRuntimeInputAndGameLoop);
            RunTest("Miracles: Divine Rain, Tailwind, Earthquake & Storm", TestDivineMiraclesSystem);
            RunTest("Hero: Champion Designation & Hero Fever Mode", TestHeroChampionSystem);
            RunTest("Minigames: Tree of Life & Blacksmith Anvil", TestCampRhythmMinigames);
            RunTest("Camp: Chef Stew Minigame & Feast Battle Buffs", TestCampCookingAndFeastBuffs);
            RunTest("Boss Trials: Scalable Boss Hunting & Hero Mask Relics", TestBossHuntingAndHeroRelics);
            RunTest("Miracles: Juju Drum Rhythm Ritual & Invocation Dance", TestMiracleRitualDanceSystem);
            RunTest("Weather: Stage Weather Simulation & Wind Ballistics Drift", TestWeatherAndWindDrift);
            RunTest("Camp: Mater Tree Awakening, Masteries & Subspecies Fusion", TestMaterTreeAwakeningAndFusion);
            RunTest("Assets: Master Sprite & Animation Catalog Metadata", TestAssetCatalogAndAnimationMeta);
            RunTest("Audio: Game Settings & Latency Calibration", TestGameSettingsAndLatencyCalibration);
            RunTest("Prefabs: Scene Setup Templates & Component Hierarchies", TestScenePrefabRegistryAndTemplates);
            RunTest("Camp: Elemental Gem Enchanting & Weapon Sockets", TestEnchantingAndElementalGems);
            RunTest("Dungeon: Moonlighter Endless Ruins & Escape Medallion", TestDungeonRuinsAndEscapeMedallion);
            RunTest("Feats: Creator Milestones & Achievement Bounties", TestFeatsOfAlmightyCreator);
            RunTest("Camp: Moonlighter Merchant Shop & Barter Market", TestMerchantShopAndBarterMarket);
            RunTest("Battle: Tactical Field Charms & Buff Items", TestBattleCharmsAndTacticalItems);
            RunTest("Puzzles: Secret Ancient Rhythm Monoliths & Totems", TestRhythmMonolithsAndTotemPuzzles);
            RunTest("Visuals: Pixel Sprite Generator & Atlas Packaging", TestPixelSpriteGeneratorAndAtlas);
            RunTest("Visuals: Camp NPCs & Hub Structures Art Generation", TestCampHubArtGenerators);
            RunTest("Visuals: VFX, Ballistics, Relic Masks & UI Frames Art Generation", TestVFXAndUIArtGenerators);
            RunTest("Visuals: 8-Biome Parallax Backdrops & Disk Asset Pipeline Exporter", TestEnvironmentBackdropsAndDiskExporter);
            RunTest("Standalone: Unity Scene Descriptors & Standalone Window Bootstrapping", TestUnitySceneExporterAndStandaloneBoot);
            RunTest("Audio: Procedural 16-Bit PCM WAV Audio Synthesis & RIFF Headers", TestProceduralAudioSynthesizer);
            RunTest("Visuals: 2D URP Normal Map Generation & Lighting Normals", TestNormalMapGenerator);
            RunTest("Visuals: Title Screen Splash Banner & Campaign World Map Art", TestTitleAndWorldMapScreens);
            RunTest("Combat: Floating Damage Text & Boss Slow-Motion Dilation", TestFloatingCombatTextAndSlowMotion);
            RunTest("Rhythm: Beat Metronome Progress & Pulsating Scale", TestBeatMetronomeAndProgress);
            RunTest("Camp: Gear Optimization Comparison & Summary", TestCampOptimizationSummary);

            // Export all art and audio assets to Assets/Sprites and Assets/Audio for Unity Editor
            try
            {
                var summary = AssetDiskExporter.ExportAllAssetsToDisk("Assets/Sprites");
                Console.WriteLine(string.Format("[PIPELINE] Exported {0} total files ({1:N0} bytes) to Assets/Sprites & Assets/Audio", summary.TotalFilesExported, summary.TotalBytesWritten));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PIPELINE] Asset export warning: " + ex.Message);
            }

            Console.WriteLine("========================================");
            Console.WriteLine(string.Format(" TESTS COMPLETE: {0} PASSED, {1} FAILED", _passed, _failed));
            Console.WriteLine("========================================");

            return _failed > 0 ? 1 : 0;
        }

        private static void RunTest(string testName, Action testAction)
        {
            try
            {
                testAction();
                Console.WriteLine(string.Format("[PASS] {0}", testName));
                _passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[FAIL] {0}: {1}", testName, ex.Message));
                _failed++;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion Failed: " + message);
            }
        }

        // --- TEST IMPLEMENTATIONS ---

        private static void TestRhythmEngineCommandEvaluation()
        {
            var engine = new RhythmEngine(120f, 0f);
            float beatLen = 0.5f; // 60 / 120 = 0.5s

            // Tap March: Boom (0), Boom (0), Boom (0), Tak (1)
            var j0 = engine.Tap(DrumId.Boom, 0.0f);
            var j1 = engine.Tap(DrumId.Boom, beatLen * 1f);
            var j2 = engine.Tap(DrumId.Boom, beatLen * 2f);
            var j3 = engine.Tap(DrumId.Tak, beatLen * 3f);

            Assert(j0.Grade == Grade.Perfect, "Tap 0 should be perfect");
            Assert(j1.Grade == Grade.Perfect, "Tap 1 should be perfect");
            Assert(j2.Grade == Grade.Perfect, "Tap 2 should be perfect");
            Assert(j3.Grade == Grade.Perfect, "Tap 3 should be perfect");

            var result = engine.Evaluate(0);
            Assert(result != null, "Evaluate should return an event");
            Assert(result.Type == RhythmEventType.Command, "Result should be Command");
            Assert(result.Command.Id == CommandId.March, "Command should be March");
            Assert(result.Perfects == 4, "Should have 4 perfects");
            Assert(engine.Combo == 1, "Combo should be 1");
        }

        private static void TestRhythmEngineComboAndFever()
        {
            var engine = new RhythmEngine(120f, 0f);
            float beatLen = 0.5f;

            for (int m = 0; m < 10; m++)
            {
                float baseTime = m * RhythmConstants.MeasureBeats * beatLen;
                engine.Tap(DrumId.Tak, baseTime + (0 * beatLen));
                engine.Tap(DrumId.Tak, baseTime + (1 * beatLen));
                engine.Tap(DrumId.Boom, baseTime + (2 * beatLen));
                engine.Tap(DrumId.Tak, baseTime + (3 * beatLen));

                var ev = engine.Evaluate(m);
                Assert(ev.Type == RhythmEventType.Command, "Should evaluate attack command");
            }

            Assert(engine.Combo == 10, "Combo should reach 10");
            Assert(engine.Fever, "Fever mode should activate at 10 combo");
        }

        private static void TestRhythmEngineAdvanceBeats()
        {
            var engine = new RhythmEngine(120f, 0f);
            var events = engine.Advance(1.6f); // 3.2 beats

            Assert(events.Count >= 3, "Should have recorded at least 3 beat events");
            Assert(events[0].Phase == "input", "First beats should be in input phase");
        }

        private static void TestComputeOffset()
        {
            var deltas = new List<float> { 20f, 25f, 18f, 22f, 21f };
            int? offset = RhythmEngine.ComputeOffset(deltas);
            Assert(offset.HasValue, "Offset should be calculated");
            Assert(offset.Value >= 18 && offset.Value <= 25, "Offset should be median value");
        }

        private static void TestCombatFormulasBaseHp()
        {
            float swordsmanHpLvl1 = CombatFormulas.CalculateBaseHealth(UnitClass.Swordsman, 1);
            float swordsmanHpLvl3 = CombatFormulas.CalculateBaseHealth(UnitClass.Swordsman, 3);

            Assert(swordsmanHpLvl1 == 120f, "Swordsman level 1 HP should be 120");
            Assert(swordsmanHpLvl3 == 150f, "Swordsman level 3 HP should be 150 (120 + 2*15)");
        }

        private static void TestCombatFormulasDamage()
        {
            var unit = new UnitMember("u1", UnitClass.Brawler, 1);
            var weapon = ItemDef.Get("arm-crusher");
            var rng = new Random(42);

            var hit = CombatFormulas.CalculateDamage(unit, weapon, targetDefense: 10f, isTargetStructure: true, isDefending: false, isFever: true, rng: rng);

            Assert(hit.FinalDamage > 0f, "Damage must be positive");
            Assert(hit.Knockback > 0f, "Knockback should be scaled");
        }

        private static void TestFormationSystem()
        {
            var slot0 = FormationSystem.GetUnitSlot(UnitClass.Swordsman, 0, 3);
            var slot1 = FormationSystem.GetUnitSlot(UnitClass.Spearman, 0, 3);
            var slot2 = FormationSystem.GetUnitSlot(UnitClass.Archer, 0, 3);

            Assert(slot0.Rank == 0, "Swordsman should be Rank 0 (Frontline)");
            Assert(slot1.Rank == 1, "Spearman should be Rank 1 (Midline)");
            Assert(slot2.Rank == 2, "Archer should be Rank 2 (Backline)");
            Assert(slot0.OffsetX > slot1.OffsetX, "Swordsman should be ahead of Spearman");
            Assert(slot1.OffsetX > slot2.OffsetX, "Spearman should be ahead of Archer");
        }

        private static void TestBallisticsCalculator()
        {
            var start = new TrajectoryPoint(0f, 0f, 0f);
            var mid = BallisticsCalculator.SampleParabolicArc(0f, 0f, 400f, 0f, 0.5f, 100f);
            var end = BallisticsCalculator.SampleParabolicArc(0f, 0f, 400f, 0f, 1.0f, 100f);

            Assert(Math.Abs(mid.X - 200f) < 0.1f, "Midpoint X should be 200");
            Assert(Math.Abs(mid.Y - 100f) < 0.1f, "Midpoint Y peak should be 100");
            Assert(Math.Abs(end.X - 400f) < 0.1f, "Endpoint X should be 400");
            Assert(Math.Abs(end.Y - 0f) < 0.1f, "Endpoint Y should land at 0");
        }

        private static void TestSaveAndInventory()
        {
            var save = SaveData.CreateDefault();
            save.AddItem("mat-timber", 5);
            Assert(save.GetItemCount("mat-timber") == 15, "Timber count should be 15");

            bool removed = save.RemoveItem("mat-timber", 3);
            Assert(removed, "Removal should succeed");
            Assert(save.GetItemCount("mat-timber") == 12, "Timber count should be 12");

            string json = UnityEngineJsonHelper.ToJson(save);
            Assert(!string.IsNullOrEmpty(json), "JSON should not be empty");

            var loaded = UnityEngineJsonHelper.FromJson<SaveData>(json);
            Assert(loaded != null, "Deserialized save should not be null");
            Assert(loaded.GetItemCount("mat-timber") == 12, "Loaded timber should be 12");
            Assert(loaded.Roster.Count == save.Roster.Count, "Roster count should match");
        }

        private static void TestEquipmentOptimizer()
        {
            var save = SaveData.CreateDefault();
            var spearUnit = save.Roster.First(u => u.Class == UnitClass.Spearman);

            // Add Epic weapon to inventory
            save.AddItem("spear-storm", 1);
            EquipmentOptimizer.OptimizeUnit(spearUnit, save);

            Assert(spearUnit.WeaponId == "spear-storm", "Spearman unit should equip Thunder Lance");
            Assert(save.GetItemCount("spear-storm") == 0, "Thunder Lance should be removed from inventory");
            Assert(save.GetItemCount("spear-wood") >= 1, "Previous wood spear should be returned to inventory");
        }

        private static void TestCombatRules()
        {
            var mission = MissionDef.Get("coast-hunt");
            var roster = SaveData.CreateDefault().Roster;
            var battle = new BattleState();

            CombatRules.InitializeBattle(battle, mission, roster);

            Assert(battle.Units.Count == roster.Count, "All roster units should be spawned");
            Assert(battle.Enemies.Count > 0, "First wave enemies should be spawned");
            Assert(battle.Phase == BattlePhase.Marching, "Battle should start in Marching phase");

            // Execute March command
            float initialX = battle.BannerX;
            CombatRules.ExecuteCommand(battle, CommandId.March, false);
            Assert(battle.BannerX > initialX, "Banner should move forward on March");
        }

        private static void TestVisualsAndLighting()
        {
            var profile = BiomeLightingProfile.GetProfile("caldera");
            Assert(profile != null, "Caldera profile should exist");
            Assert(profile.LightIntensity > 1.0f, "Caldera intensity should be > 1.0");

            float orthoSize = PixelPerfectSetup.CalculateCameraOrthographicSize(270, 16);
            Assert(orthoSize > 0f, "Ortho size should be positive");
        }

        private static void TestDSPAudioClock()
        {
            var clock = new DSPAudioClock(120f);
            clock.Start(10.0);

            double pos = clock.GetSongPosition(12.0);
            double beat = clock.GetCurrentBeat(12.0);

            Assert(Math.Abs(pos - 2.0) < 0.001, "Song position at dsp 12 should be 2.0s");
            Assert(Math.Abs(beat - 4.0) < 0.001, "Beat at 2.0s (120 BPM) should be 4.0");
        }

        private static void TestEnemyAIAndBossTelegraphs()
        {
            var battle = new BattleState();
            battle.BannerX = 500f;
            var golem = new LiveEnemy
            {
                Id = "boss-golem",
                Kind = EnemyKind.ColossusGolem,
                X = 750f,
                CurrentHp = 1500f,
                MaxHp = 1500f,
                Defense = 10f
            };
            battle.Enemies.Add(golem);

            var enemyStates = new Dictionary<string, EnemyCombatState>();
            var rng = new Random(100);

            // Update enemy turn
            EnemyAISystem.UpdateEnemyTurn(battle, enemyStates, rng);

            Assert(enemyStates.ContainsKey("boss-golem"), "Golem should have AI state registered");
            var ai = enemyStates["boss-golem"];
            Assert(ai.State == EnemyState.Telegraphing, "Boss in range should start telegraphing");
            Assert(ai.ActiveTelegraph != null, "Active telegraph should be assigned");
            Assert(!string.IsNullOrEmpty(ai.ActiveTelegraph.WarningHint), "Telegraph hint should be populated");

            // Test Wildlife Fleeing
            var runner = new LiveEnemy
            {
                Id = "wild-runner",
                Kind = EnemyKind.PlainsRunner,
                X = 650f,
                CurrentHp = 25f,
                MaxHp = 25f
            };
            battle.Enemies.Add(runner);
            float startRunnerX = runner.X;

            EnemyAISystem.UpdateEnemyTurn(battle, enemyStates, rng);
            Assert(runner.X > startRunnerX, "Wildlife should flee forward when army is close");
        }

        private static void TestChargeAndJumpCombatMechanics()
        {
            var mission = MissionDef.Get("coast-hunt");
            var roster = SaveData.CreateDefault().Roster;
            var battle = new BattleState();
            CombatRules.InitializeBattle(battle, mission, roster);

            // Test Jump
            CombatRules.ExecuteCommand(battle, CommandId.Jump, false);
            Assert(battle.IsAirborne, "Jump command should make army airborne");

            // Test Charge
            CombatRules.ExecuteCommand(battle, CommandId.Charge, false);
            Assert(battle.IsCharged, "Charge command should store energy");
            Assert(!battle.IsAirborne, "Airborne should reset on next command");

            // Enemy in range
            var target = battle.Enemies.First(e => e.IsAlive);
            target.X = battle.BannerX + 50f;
            float hpBefore = target.CurrentHp;

            // Attack with charge
            CombatRules.ExecuteCommand(battle, CommandId.Attack, false);
            Assert(!battle.IsCharged, "Charge should be consumed after attack");
            Assert(target.CurrentHp < hpBefore, "Target enemy should take heavy charged damage");
        }

        private static void TestLootDropsAndBattlePayout()
        {
            var rng = new Random(42);
            var drops = LootSystem.RollDrops(EnemyKind.DrakeTitan, rng);
            Assert(drops.Count > 0, "Drake Titan should roll loot drops");
            Assert(drops.Any(d => d.ItemId == "mat-dragon-scale"), "Drake drops should include dragon scales");

            // Battle collection
            var battle = new BattleState();
            battle.LootCollected.AddRange(drops);
            Assert(battle.LootCollected.Count > 0, "Battle state should collect loot drops");
        }

        private static void TestBlacksmithCrafting()
        {
            var save = SaveData.CreateDefault();
            save.Gold = 100;
            save.AddItem("mat-timber", 5);
            save.AddItem("mat-iron-slag", 5);
            save.CompletedMissions.Add("shadowmask-clash"); // Unlock iron weapons

            var recipe = BlacksmithCrafting.Recipes.First(r => r.OutputItemId == "spear-iron");
            Assert(BlacksmithCrafting.CanCraft(recipe, save), "Player should have requirements to craft Iron Spear");

            int initialWood = save.GetItemCount("mat-timber");
            int initialSlag = save.GetItemCount("mat-iron-slag");
            int initialGold = save.Gold;

            bool success = BlacksmithCrafting.CraftItem(recipe, save);
            Assert(success, "Crafting Iron Spear should succeed");
            Assert(save.GetItemCount("spear-iron") == 1, "Iron Spear should be in inventory");
            Assert(save.Gold == initialGold - recipe.GoldCost, "Gold should be deducted");
            Assert(save.GetItemCount("mat-timber") == initialWood - recipe.RequiredMaterials["mat-timber"], "Timber should be deducted");
            Assert(save.GetItemCount("mat-iron-slag") == initialSlag - recipe.RequiredMaterials["mat-iron-slag"], "Slag should be deducted");
        }

        private static void TestAltarEvolution()
        {
            var save = SaveData.CreateDefault();
            save.Gold = 200;
            save.AddItem("mat-sun-cabbage", 5);
            save.AddItem("mat-jerky", 5);

            var spearUnit = save.Roster.First(u => u.Class == UnitClass.Spearman);
            Assert(spearUnit.Subspecies == Subspecies.Normal, "Initial unit should be Normal subspecies");

            Assert(AltarEvolution.CanEvolve(save, spearUnit.Id, Subspecies.Swiftpaw), "Should meet Swiftpaw evolution requirements");

            bool evolved = AltarEvolution.EvolveUnit(save, spearUnit.Id, Subspecies.Swiftpaw);
            Assert(evolved, "Evolution to Swiftpaw should succeed");
            Assert(spearUnit.Subspecies == Subspecies.Swiftpaw, "Unit should now be Swiftpaw");

            // Verify stat scaling
            float baseHp = CombatFormulas.CalculateBaseHealth(UnitClass.Spearman, 1, Subspecies.Normal);
            float frogtideHp = CombatFormulas.CalculateBaseHealth(UnitClass.Spearman, 1, Subspecies.Frogtide);
            Assert(frogtideHp > baseHp, "Frogtide subspecies should have increased base HP");
        }

        private static void TestBarracksRosterAndLoadout()
        {
            var save = SaveData.CreateDefault();
            save.Gold = 300;
            save.AddItem("mat-jerky", 5);

            int initialSpearCount = save.Roster.Count(u => u.Class == UnitClass.Spearman);
            var recruited = BarracksManager.RecruitUnit(save, UnitClass.Spearman);
            Assert(recruited != null, "Recruiting Spearman should succeed");
            Assert(save.Roster.Count(u => u.Class == UnitClass.Spearman) == initialSpearCount + 1, "Roster should have +1 spearman");

            // Equip weapon
            save.AddItem("spear-iron", 1);
            bool equipped = BarracksManager.EquipItem(save, recruited.Id, "spear-iron");
            Assert(equipped, "Equipping iron spear should succeed");
            Assert(recruited.WeaponId == "spear-iron", "Recruit should have iron spear equipped");

            // Level up
            bool leveled = BarracksManager.LevelUpUnit(save, recruited.Id);
            Assert(leveled, "Leveling up unit should succeed");
            Assert(recruited.Level == 2, "Unit level should now be 2");
        }

        private static void TestPixelAnimationStateMachine()
        {
            var anim = PixelAnimator.CreateStandardUnitAnimator("unit-1", 32);
            Assert(anim.CurrentStateName == "Idle", "Initial animation should be Idle");

            string firedEvent = null;
            anim.OnAnimationEvent += (evt) => { firedEvent = evt; };

            anim.Play("Attack", true);
            Assert(anim.CurrentStateName == "Attack", "Should switch to Attack state");
            Assert(firedEvent == "Windup", "First attack frame should fire Windup event");

            // Advance time to release frame
            anim.Update(0.11f);
            Assert(firedEvent == "Release", "Should advance to Release frame and fire event");

            // Advance time to impact frame
            anim.Update(0.09f);
            Assert(firedEvent == "Impact", "Should advance to Impact frame and fire event");
        }

        private static void TestPixelCameraAndScreenShake()
        {
            var cam = new PixelCamera();
            cam.PixelsPerUnit = 16f;

            cam.Update(100f, 0.5f);
            Assert(cam.CurrentX > 0f, "Camera should track banner target");

            // Test Trauma
            cam.AddTrauma(0.8f);
            Assert(cam.Trauma == 0.8f, "Trauma should be 0.8");

            cam.Update(100f, 0.016f);
            Assert(Math.Abs(cam.ShakeOffsetX) > 0f || Math.Abs(cam.ShakeOffsetY) > 0f, "Shake offset should be non-zero under trauma");

            float snapped = cam.SnappedX;
            float remainder = (snapped * 16f) % 1.0f;
            Assert(Math.Abs(remainder) < 0.001f, "Snapped coordinate must be aligned to pixel grid");
        }

        private static void TestRhythmAudioSequencer()
        {
            var audio = new RhythmAudioSequencer();

            audio.TriggerDrumHit(DrumId.Boom, 1.0f, Grade.Perfect);
            audio.TriggerDrumHit(DrumId.Tak, 1.5f, Grade.Good);

            Assert(audio.PlaybackLog.Count == 2, "Audio log should contain 2 drum triggers");
            Assert(audio.PlaybackLog[0].ClipName == "drum_boom", "Boom drum clip should be drum_boom");
            Assert(audio.PlaybackLog[1].ClipName == "drum_tak", "Tak drum clip should be drum_tak");
            Assert(audio.PlaybackLog[1].Pitch < 1.0f, "Good grade should have slight pitch modulation");

            var marchCmd = CommandDef.StandardCommands.First(c => c.Id == CommandId.March);
            audio.QueueChantSequence(marchCmd, 0.0f, 0.5f, fever: true);

            Assert(audio.PlaybackLog.Count == 6, "Chant should add 4 response vocal clips");
            Assert(audio.PlaybackLog[2].Type == AudioPlaybackType.VocalChant, "Queued clips should be VocalChant");
            Assert(audio.PlaybackLog[2].Pitch > 1.0f, "Fever chant should have excited pitch");

            audio.UpdateFeverMusicCrossfade(true, 1.0f);
            Assert(audio.FeverStemGain > 0f, "Fever stem gain should increase");
        }

        private static void TestBattleManagerSimulation()
        {
            var manager = new BattleManager(new Random(42));
            var mission = MissionDatabase.Get("mission-1");
            var save = SaveData.CreateDefault();

            manager.StartMission(mission, save.Roster);
            Assert(manager.State != null, "Battle state should be initialized");
            Assert(manager.State.Phase == BattlePhase.Marching, "Should start in marching phase");

            // Feed a March command: Boom, Boom, Boom, Tak
            float beatLen = manager.Rhythm.BeatInterval;
            manager.HandleDrumInput(DrumId.Boom);
            manager.Update(beatLen);
            manager.HandleDrumInput(DrumId.Boom);
            manager.Update(beatLen);
            manager.HandleDrumInput(DrumId.Boom);
            manager.Update(beatLen);
            manager.HandleDrumInput(DrumId.Tak);

            // Advance through response phase to evaluate command and execute march
            manager.Update(beatLen);
            manager.Update(beatLen * 4f);

            Assert(manager.CurrentMeasure >= 1, "Measure index should advance");
            Assert(manager.LastExecutedMeasure >= 0, "March command should have been evaluated and executed");
            Assert(manager.State.BannerX > 0f, "Banner should have moved forward");
            Assert(manager.Camera.TargetX > 0f, "Camera should track battle units");
        }

        private static void TestEnvironmentAndParallaxSystem()
        {
            // Test parsing biomes
            Assert(EnvironmentSystem.ParseBiome("coral-coast") == BiomeType.CoralCoast, "Coral coast parse");
            Assert(EnvironmentSystem.ParseBiome("volcanic-caldera") == BiomeType.VolcanicCaldera, "Volcano parse");
            Assert(EnvironmentSystem.ParseBiome("jungle") == BiomeType.JungleFort, "Jungle parse");
            Assert(EnvironmentSystem.ParseBiome("ruins") == BiomeType.RuinAltar, "Ruins parse");

            // Test all 8 biomes theme creation
            var biomes = (BiomeType[])Enum.GetValues(typeof(BiomeType));
            Assert(biomes.Length == 8, "There should be 8 biomes");

            foreach (var b in biomes)
            {
                var theme = EnvironmentSystem.CreateBiomeTheme(b);
                Assert(!string.IsNullOrEmpty(theme.DisplayName), "Theme should have display name");
                Assert(!string.IsNullOrEmpty(theme.GroundTileId), "Theme should have ground tile");
                Assert(theme.Weather != null, "Theme should have weather profile");
                Assert(theme.Layers.Count >= 4, "Theme should have at least 4 parallax layers");

                // Verify layer parallax calculation
                var sky = theme.Layers[0];
                float skyOffset = sky.ComputePositionX(100f);
                Assert(Math.Abs(skyOffset - (-100f * sky.ScrollMultiplier)) < 0.001f, "Parallax math correct");
            }
        }

        private static void TestBattleHUDAndCampHUDState()
        {
            var save = SaveData.CreateDefault();
            var campHUD = new CampHUDState();
            campHUD.RefreshFromSave(save);

            Assert(campHUD.RosterUnits.Count == save.Roster.Count, "Camp HUD should list all roster units");
            Assert(campHUD.AvailableRecipes.Count > 0, "Camp HUD should list available recipes");
            Assert(campHUD.MissionCards.Count == 8, "Camp HUD should list all 8 campaign missions");
            Assert(campHUD.MissionCards[0].IsUnlocked, "First mission should be unlocked");

            // Tab switching
            campHUD.SelectTab(CampMenuTab.Barracks);
            Assert(campHUD.CurrentTab == CampMenuTab.Barracks, "Tab should be Barracks");

            campHUD.SelectTab(CampMenuTab.SpiritAltar);
            Assert(campHUD.EvolutionBranches.Count == 5, "Altar should show 5 evolution options");

            // Battle HUD
            var battleMgr = new BattleManager(new Random(42));
            var mission = MissionDatabase.Get("mission-1");
            battleMgr.StartMission(mission, save.Roster);

            var battleHUD = new BattleHUDState();
            battleHUD.RecordDrumTap(DrumId.Boom, Grade.Perfect, 0f);
            Assert(battleHUD.RecentBeats.Count == 1, "Recent beat recorded");

            battleHUD.UpdateFromBattleManager(battleMgr, 0.016f);
            Assert(battleHUD.UnitCards.Count == save.Roster.Count, "Battle HUD should sync unit cards");

            battleHUD.AddLootToast("mat-jerky", 2);
            Assert(battleHUD.ActiveLootToasts.Count == 1, "Loot toast added");
            Assert(battleHUD.ActiveLootToasts[0].ItemName != null, "Toast has item name");
        }

        private static void TestDialogueSystem()
        {
            var intro = DialogueDatabase.Get("intro-awakening");
            Assert(intro != null, "Intro sequence should exist");
            Assert(intro.Lines.Count == 3, "Intro sequence should have 3 lines");

            var player = new DialoguePlayer();
            player.Play(intro);

            Assert(player.CurrentLine != null, "Current line should be active");
            Assert(player.CurrentLine.Speaker == "High Priestess Leah", "Speaker is High Priestess Leah");
            Assert(!player.IsLineFullyRevealed, "Line should start unrevealed");

            // Update typewriter
            player.Update(0.1f);
            Assert(player.VisibleText.Length > 0, "Typewriter should advance visible text");

            // Skip to full line
            player.AdvanceOrSkip();
            Assert(player.IsLineFullyRevealed, "Line should now be fully revealed");

            // Advance to line 2
            bool advanced = player.AdvanceOrSkip();
            Assert(advanced, "Should advance to line 2");
            Assert(player.CurrentLineIndex == 1, "Should be on line 2");

            // Skip line 2 & advance to line 3
            player.AdvanceOrSkip();
            player.AdvanceOrSkip();
            Assert(player.CurrentLineIndex == 2, "Should be on line 3");

            // Skip line 3 & finish
            player.AdvanceOrSkip();
            bool notFinished = player.AdvanceOrSkip();
            Assert(!notFinished, "Sequence should be completed");
            Assert(player.IsFinished, "Player marked as finished");
        }

        private static void TestGameOrchestratorFlow()
        {
            var orch = new GameOrchestrator(new Random(42));
            orch.Initialize();

            Assert(orch.CurrentState == GameFlowState.CampHub, "Initial state should be CampHub");

            // Launch Mission 1
            bool launched = orch.LaunchMission("mission-1");
            Assert(launched, "Mission 1 launch should succeed");
            Assert(orch.CurrentState == GameFlowState.DialogueCutscene, "Briefing cutscene should start");

            // Skip cutscene
            while (orch.CurrentState == GameFlowState.DialogueCutscene)
            {
                orch.AdvanceDialogue();
            }

            Assert(orch.CurrentState == GameFlowState.BattleActive, "Should enter BattleActive state");
            Assert(orch.ActiveBiome != null, "Active biome should be configured");
            Assert(orch.ActiveBiome.Biome == BiomeType.CoralCoast, "Biome is Coral Coast");

            // Simulate drum taps and progress until enemies cleared
            float beatLen = orch.BattleMgr.Rhythm.BeatInterval;
            for (int measure = 0; measure < 50; measure++)
            {
                var nearestEnemy = orch.BattleMgr.State.Enemies.FirstOrDefault(e => e.IsAlive);
                bool shouldAttack = nearestEnemy != null && (nearestEnemy.X - orch.BattleMgr.State.BannerX) <= 280f;

                if (shouldAttack)
                {
                    orch.HandleDrumInput(DrumId.Tak); // Attack: Tak, Tak, Boom, Tak
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Tak);
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Boom);
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Tak);
                    orch.Update(beatLen);
                }
                else
                {
                    orch.HandleDrumInput(DrumId.Boom); // March: Boom, Boom, Boom, Tak
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Boom);
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Boom);
                    orch.Update(beatLen);
                    orch.HandleDrumInput(DrumId.Tak);
                    orch.Update(beatLen);
                }

                // Advance through response phase
                for (int step = 0; step < 8; step++)
                {
                    orch.Update(beatLen * 0.5f);
                }

                if (orch.CurrentState == GameFlowState.BattleResultsScreen)
                {
                    break;
                }
            }

            Assert(orch.CurrentState == GameFlowState.BattleResultsScreen, "Battle should complete into results screen");
            Assert(orch.LastBattleResult.IsVictory, "Mission 1 should result in victory");
            Assert(orch.Save.MissionsCleared.Contains("coast-hunt") || orch.Save.MissionsCleared.Contains("mission-1"), "Mission 1 recorded as cleared");
            Assert(orch.Save.MissionsUnlocked.Contains("shadowmask-clash") || orch.Save.MissionsUnlocked.Contains("mission-2"), "Mission 2 should be unlocked");
            Assert(orch.Save.Gold > 250, "Gold reward should be credited");

            // Return to camp
            orch.ReturnToCamp();
            Assert(orch.CurrentState == GameFlowState.CampHub, "Should return to CampHub");
            Assert(orch.CampHUD.CurrentGold == orch.Save.Gold, "Camp HUD synced with save gold");
        }

        private static void TestPresentationViews()
        {
            var unit = new UnitMember("u1", UnitClass.Swordsman, 1, "sword-wood", "shield-buckler", "helm-leather");
            var liveUnit = new LiveUnit
            {
                Member = unit,
                X = 100f,
                Y = 0f,
                CurrentHp = 100f,
                MaxHp = 100f
            };

            var unitView = new UnitView(liveUnit);
            Assert(unitView.WeaponSpriteId == "item_sword-wood", "Weapon sprite mapped");
            Assert(unitView.ShieldSpriteId == "item_shield-buckler", "Shield sprite mapped");
            Assert(unitView.HelmetSpriteId == "item_helm-leather", "Helmet sprite mapped");

            // Damage flash
            unitView.TakeDamage(20f);
            Assert(unitView.IsFlashingWhite, "Should flash white on damage");
            unitView.Update(0.2f);
            Assert(!unitView.IsFlashingWhite, "Flash should decay");

            // Enemy View
            var liveEnemy = new LiveEnemy
            {
                Id = "boss1",
                Kind = EnemyKind.DrakeTitan,
                X = 400f,
                Y = 0f,
                CurrentHp = 800f,
                MaxHp = 800f
            };
            var enemyView = new EnemyView(liveEnemy);
            Assert(enemyView.SpriteSize == 96, "Boss sprite size is 96");
            Assert(enemyView.SpriteScale > 1.0f, "Boss sprite scale is magnified");

            // Telegraph
            enemyView.StartTelegraph(BossAttackType.FireBreath, 1.5f);
            Assert(enemyView.IsTelegraphing, "Enemy is telegraphing");
            enemyView.Update(0.75f);
            Assert(enemyView.TelegraphProgress >= 0.5f, "Telegraph progress tracks elapsed time");

            // Projectile View
            var proj = new ProjectileView("p1", ProjectileVisualType.Spear, 0f, 0f, 200f, 0f, 400f, 50f);
            Assert(!proj.IsFinished, "Projectile starts active");
            proj.Update(0.2f);
            Assert(proj.CurrentX > 0f, "Projectile moves forward");
            Assert(proj.RotationDegrees != 0f, "Projectile rotation aligns with trajectory");
        }

        private static void TestBattleControllerAndUI()
        {
            var mission = MissionDatabase.Get("coast-hunt");
            var roster = new List<UnitMember>
            {
                new UnitMember("b1", UnitClass.Banner, 1),
                new UnitMember("s1", UnitClass.Spearman, 1, "spear-wood"),
                new UnitMember("w1", UnitClass.Swordsman, 1, "sword-wood")
            };

            var battleCtrl = new BattleController();
            var battleUI = new BattleUIController(battleCtrl);

            bool beatPulseReceived = false;
            battleUI.OnBeatPulse += (beatIdx, scale) => { beatPulseReceived = true; };

            battleCtrl.StartBattle(mission, roster);
            Assert(battleCtrl.IsActive, "Battle controller should be active");
            Assert(battleCtrl.UnitViews.Count == 3, "3 UnitViews instantiated");

            // Input
            var tap = battleCtrl.HandleInput(DrumId.Boom);
            Assert(tap.Grade == Grade.Perfect, "First tap is perfect");

            // UI update
            battleCtrl.Update(0.1f);
            battleUI.Update(0.1f);
            Assert(beatPulseReceived, "Beat pulse event triggered in UI");
        }

        private static void TestCampControllerAndUI()
        {
            var save = SaveSystem.CreateInitialSave();
            save.Gold = 500;
            save.AddItem("mat-jerky", 10);
            save.AddItem("mat-timber", 10);
            save.AddItem("mat-iron-slag", 10);

            var campCtrl = new CampController(save);
            var campUI = new CampUIController(campCtrl);

            // Tab switch
            bool tabChanged = false;
            campUI.OnTabChanged += (tab) => { tabChanged = true; };
            campUI.SwitchTab(CampMenuTab.Blacksmith);
            Assert(tabChanged, "Tab change event fired");

            // Recruit unit
            bool recruited = campUI.RequestRecruit(UnitClass.Archer);
            Assert(recruited, "Archer recruitment succeeded");
            Assert(save.Roster.Any(u => u.Class == UnitClass.Archer), "Archer added to roster");

            // Craft item
            bool crafted = campUI.RequestCraft("spear-wood");
            Assert(crafted, "Crafting spear-wood succeeded");
            Assert(save.GetItemCount("spear-wood") >= 1, "Crafted item in inventory");

            // Auto Optimize
            bool optimized = campUI.RequestAutoOptimize();
            Assert(optimized, "Auto optimize succeeded");
        }

        private static void TestAudioControllerAndSound()
        {
            var audioCtrl = new AudioController();
            string lastClip = null;
            audioCtrl.OnPlayAudioClip += (clip, vol, pitch) => { lastClip = clip; };

            // Drum sounds
            audioCtrl.PlayDrum(DrumId.Boom, Grade.Perfect);
            Assert(lastClip == "drum_boom", "Boom drum clip triggered");

            audioCtrl.PlayDrum(DrumId.Tak, Grade.Good);
            Assert(lastClip == "drum_tak", "Tak drum clip triggered");

            // UI sounds
            audioCtrl.PlayUISound(UISoundType.ItemCraft);
            Assert(lastClip == "sfx_anvil_strike", "Anvil strike SFX triggered");

            audioCtrl.PlayUISound(UISoundType.UnitEvolve);
            Assert(lastClip == "sfx_altar_mystic", "Altar mystic SFX triggered");
        }

        private static void TestMoonlighterSpriteAndTilemapRegistry()
        {
            // Unit sprite specs
            var spearSpec = PixelSpriteRegistry.GetSpec(UnitClass.Spearman);
            Assert(spearSpec.SpriteWidth == 32, "Spearman width 32");
            Assert(spearSpec.WeaponAnchor.X > 0, "Weapon anchor defined");

            var cavSpec = PixelSpriteRegistry.GetSpec(UnitClass.Cavalry);
            Assert(cavSpec.SpriteWidth == 48, "Cavalry width 48");

            // Tile definitions
            var sandTile = TilemapRegistry.GetTile("tile_coast_sand_center");
            Assert(sandTile.Biome == BiomeType.CoralCoast, "Sand tile belongs to Coral Coast");

            var gateTile = TilemapRegistry.GetTile("tile_bastion_iron_gate");
            Assert(gateTile.Collider == TileColliderType.DestructibleStructure, "Iron gate is destructible structure");
        }

        private static void TestRivalTribeEnemyClasses()
        {
            var tribeClasses = new[]
            {
                EnemyKind.TribeBanner,
                EnemyKind.TribeSpearman,
                EnemyKind.TribeSwordsman,
                EnemyKind.TribeArcher,
                EnemyKind.TribeCavalry,
                EnemyKind.TribeHammerer,
                EnemyKind.TribeHornist,
                EnemyKind.TribeSkyrider,
                EnemyKind.TribeMage,
                EnemyKind.TribeBrawler
            };

            Assert(tribeClasses.Length == 10, "Should have 10 tribe counterpart classes");

            foreach (var kind in tribeClasses)
            {
                Assert(EnemyMetadata.GetCategory(kind) == EnemyCategory.RivalClan, string.Format("{0} must be RivalClan", kind));
                Assert(EnemyMetadata.GetCollisionRadius(kind) >= 20f, string.Format("{0} collision radius valid", kind));
                Assert(LootSystem.EnemyLootTables.ContainsKey(kind), string.Format("{0} has loot table", kind));
            }
        }

        private static void TestCurrencyDropSystem()
        {
            var rng = new Random(42);

            // 1. Verify currency drops by category
            int wildlifeCoin = LootSystem.RollCurrencyDrop(EnemyKind.PlainsRunner, rng);
            Assert(wildlifeCoin >= 2 && wildlifeCoin <= 10, "Wildlife drops small coin pouch");

            int tribeCoin = LootSystem.RollCurrencyDrop(EnemyKind.TribeHammerer, rng);
            Assert(tribeCoin >= 10 && tribeCoin <= 30, "Tribe warrior drops standard coin");

            int bossCoin = LootSystem.RollCurrencyDrop(EnemyKind.DrakeTitan, rng);
            Assert(bossCoin >= 100 && bossCoin <= 300, "Boss drops massive coin bounty");

            // 2. Battle State currency tracking
            var mission = MissionDatabase.Get("coast-hunt");
            var roster = new List<UnitMember>
            {
                new UnitMember("b1", UnitClass.Banner, 1),
                new UnitMember("s1", UnitClass.Spearman, 1, "spear-wood")
            };

            var battle = new BattleManager(rng);
            battle.StartMission(mission, roster);

            int killsReceived = 0;
            int totalCoinsFromKills = 0;
            battle.OnEnemySlain += (enemy, drops, currency) =>
            {
                killsReceived++;
                totalCoinsFromKills += currency;
            };

            // March forward into range and attack
            for (int i = 0; i < 4; i++)
            {
                CombatRules.ExecuteCommand(battle.State, CommandId.March, false, rng);
            }
            for (int i = 0; i < 15; i++)
            {
                CombatRules.ExecuteCommand(battle.State, CommandId.Attack, false, rng);
            }

            Assert(battle.State.CurrencyCollected > 0, "Currency was collected from defeated enemies");
            var result = battle.FinishBattle();
            Assert(result.GoldEarned >= battle.State.CurrencyCollected, "Result gold includes drop currency and mission bonus");
        }

        private static void TestRuntimeInputAndGameLoop()
        {
            var runtime = new GameRuntime(new Random(42));
            runtime.StartNewGame();
            Assert(runtime.IsRunning, "Runtime is running");
            Assert(runtime.Orchestrator.CurrentState == GameFlowState.CampHub, "Started in Camp Hub");

            // Test input bindings
            DrumId? drumA = runtime.InputManager.GetDrumForKey("A");
            Assert(drumA == DrumId.Boom, "Key A binds to Boom");

            DrumId? drumS = runtime.InputManager.GetDrumForKey("S");
            Assert(drumS == DrumId.Tak, "Key S binds to Tak");

            // Launch Mission and test battle input triggering
            bool launched = runtime.StartMission("coast-hunt");
            Assert(launched, "Coast hunt launched");

            // Advance through briefing dialogue if any
            while (runtime.Orchestrator.CurrentState == GameFlowState.DialogueCutscene)
            {
                runtime.InputManager.ProcessKeyInput("Space");
            }

            Assert(runtime.Orchestrator.CurrentState == GameFlowState.BattleActive, "Battle active now");

            // Trigger drum inputs via key presses
            bool processedA = runtime.InputManager.ProcessKeyInput("A");
            bool processedS = runtime.InputManager.ProcessKeyInput("S");
            Assert(processedA && processedS, "Key inputs processed");

            // Update tick simulation
            for (int i = 0; i < 10; i++)
            {
                runtime.Tick(0.05f);
            }
            Assert(runtime.TotalElapsedSeconds > 0.4f, "Runtime simulation clock advanced");
        }

        private static void TestDivineMiraclesSystem()
        {
            var miracleSys = new MiracleSystem();
            Assert(!miracleSys.State.IsActive, "Miracle state begins inactive");

            // 1. Invoke Rain Miracle
            bool invoked = false;
            miracleSys.OnMiracleInvoked += type => invoked = (type == MiracleType.Rain);
            miracleSys.InvokeMiracle(MiracleType.Rain, 10.0f);

            Assert(miracleSys.State.IsActive, "Rain miracle is now active");
            Assert(miracleSys.State.ActiveMiracle == MiracleType.Rain, "Active miracle is Rain");
            Assert(invoked, "Miracle invoked event fired");

            // 2. Test Rain periodic heal and douse
            var battleState = new BattleState();
            var unit = new LiveUnit
            {
                CurrentHp = 20f,
                MaxHp = 100f,
                IsBurning = true
            };
            battleState.Units.Add(unit);

            miracleSys.Update(1.1f, battleState, null, new Random(42));
            Assert(unit.CurrentHp > 20f, "Rain heals friendly units over time");
            Assert(!unit.IsBurning, "Rain extinguishes burning condition");

            // 3. Test Earthquake on structures
            miracleSys.InvokeMiracle(MiracleType.Earthquake, 10.0f);
            var barrier = new LiveEnemy
            {
                Id = "barrier_1",
                Kind = EnemyKind.Barricade,
                CurrentHp = 100f,
                MaxHp = 100f,
                IsStructure = true
            };
            battleState.Enemies.Add(barrier);

            miracleSys.Update(1.1f, battleState, null, new Random(42));
            Assert(barrier.CurrentHp < 100f, "Earthquake damages structures with multiplier");
            Assert(miracleSys.State.TotalEarthquakePulses > 0, "Quake pulses counted");

            // 4. Test Relic item lookup
            Assert(MiracleSystem.GetMiracleFromRelicId("relic-storm-charm") == MiracleType.Storm, "Storm charm yields Storm miracle");
            Assert(MiracleSystem.GetMiracleFromRelicId("relic-wind-charm") == MiracleType.Tailwind, "Wind charm yields Tailwind miracle");
        }

        private static void TestHeroChampionSystem()
        {
            var save = SaveSystem.CreateInitialSave();
            var spearman = save.Roster.First(u => u.Class == UnitClass.Spearman);

            // 1. Designate Hero
            bool setHero = BarracksManager.SetHeroUnit(save, spearman.Id);
            Assert(setHero, "Set hero unit succeeded");
            Assert(spearman.IsHero, "Spearman is designated as Hero");
            Assert(spearman.Name.StartsWith("Hero"), "Hero name prefix updated");

            // 2. Equip Hero Mask
            save.AddItem("hero-mask-courage", 1);
            bool equippedMask = BarracksManager.EquipItem(save, spearman.Id, "hero-mask-courage");
            Assert(equippedMask, "Equipped hero mask");
            Assert(spearman.MaskId == "hero-mask-courage", "Mask attached to hero");

            // 3. Hero trigger check (Fever + 3+ Perfects)
            Assert(HeroSystem.CanTriggerHeroMode(CommandId.Attack, true, 3), "Hero mode triggers on Fever + 3 Perfects");
            Assert(!HeroSystem.CanTriggerHeroMode(CommandId.Attack, false, 4), "Hero mode cannot trigger outside Fever");
            Assert(!HeroSystem.CanTriggerHeroMode(CommandId.Attack, true, 2), "Hero mode requires at least 3 Perfects");

            // 4. Execute Hero ability
            var battleState = new BattleState();
            var heroLive = new LiveUnit
            {
                Member = spearman,
                CurrentHp = 100f,
                MaxHp = 100f,
                X = 100f
            };
            battleState.Units.Add(heroLive);

            var enemy = new LiveEnemy
            {
                Id = "enemy_1",
                Kind = EnemyKind.TribeSwordsman,
                CurrentHp = 200f,
                MaxHp = 200f,
                X = 250f
            };
            battleState.Enemies.Add(enemy);

            var heroAction = HeroSystem.ExecuteHeroAction(battleState, heroLive, CommandId.Attack, new Random(42));
            Assert(heroAction.TriggeredHeroMode, "Hero mode action triggered");
            Assert(heroAction.Ability == HeroAbilityType.SpearTempest, "Spearman executes Spear Tempest");
            Assert(heroAction.DamageDealt > 0f, "Hero dealt massive ability damage");
            Assert(enemy.CurrentHp < 200f, "Enemy took Hero ability damage");
        }

        private static void TestCampRhythmMinigames()
        {
            var minigames = new CampMinigameManager();
            Assert(!minigames.IsActive, "Minigame starts inactive");

            // 1. Start Tree of Life Minigame
            minigames.StartMinigame(MinigameType.TreeOfLife);
            Assert(minigames.IsActive, "Minigame is active");
            Assert(minigames.CurrentMinigame == MinigameType.TreeOfLife, "Current minigame is TreeOfLife");
            Assert(minigames.CurrentPrompt != null, "Round 1 prompt generated");

            // 2. Submit matching rhythm sequence for Round 1
            var prompt = minigames.CurrentPrompt;
            foreach (var drum in prompt.Sequence)
            {
                minigames.SubmitDrumInput(drum);
            }

            Assert(minigames.CurrentRoundIndex == 1, "Advanced to round 2 after prompt submission");

            // 3. Play remaining rounds to finish
            while (minigames.IsActive)
            {
                var curPrompt = minigames.CurrentPrompt;
                foreach (var drum in curPrompt.Sequence)
                {
                    minigames.SubmitDrumInput(drum);
                }
            }

            Assert(!minigames.IsActive, "Minigame successfully completed all rounds");

            // 4. Test reward integration to SaveData
            var save = SaveSystem.CreateInitialSave();
            int startingTimber = save.GetItemCount("mat-timber");

            var result = new MinigameResult
            {
                Type = MinigameType.TreeOfLife,
                RoundsCompleted = 4,
                TotalRounds = 4
            };
            result.RewardsEarned.Add(new InventoryEntry("mat-timber", 5));
            minigames.ApplyRewardsToSave(save, result);

            Assert(save.GetItemCount("mat-timber") == startingTimber + 5, "Minigame rewards correctly added to SaveData inventory");
        }

        private static void TestCampCookingAndFeastBuffs()
        {
            var camp = new CampController();
            var save = camp.SaveData;

            // 1. Play Camp Chef Minigame
            camp.StartMinigame(MinigameType.CampChefStew);
            Assert(camp.Minigames.IsActive, "Chef minigame active");
            Assert(camp.Minigames.CurrentMinigame == MinigameType.CampChefStew, "Minigame is Chef Stew");

            while (camp.Minigames.IsActive)
            {
                var curPrompt = camp.Minigames.CurrentPrompt;
                foreach (var drum in curPrompt.Sequence)
                {
                    camp.SubmitMinigameInput(drum);
                }
            }
            Assert(!camp.Minigames.IsActive, "Chef minigame completed");
            Assert(save.GetItemCount("food-simple-stew") > 0 || save.GetItemCount("food-divine-feast") > 0, "Earned cooked dishes");

            // 2. Consume Feast
            save.AddItem("food-divine-feast", 1);
            bool consumed = camp.ConsumeFeast("food-divine-feast");
            Assert(consumed, "Feast was consumed");
            Assert(save.ActiveMealBuffId == "food-divine-feast", "Active meal buff saved");

            // 3. Verify Battle HP & Damage with active meal buff
            var battle = new BattleManager(new Random(42));
            var mission = MissionDatabase.Get("coast-hunt");
            battle.StartMission(mission, save.Roster, 120f, save.ActiveMealBuffId);

            var firstUnit = battle.State.Units[0];
            Assert(firstUnit.MaxHp > CombatFormulas.CalculateBaseHealth(firstUnit.Member.Class, firstUnit.Member.Level, firstUnit.Member.Subspecies), "Unit Max HP boosted by Feast");
            Assert(battle.State.ActiveMealBuff != null && battle.State.ActiveMealBuff.Id == "food-divine-feast", "Battle has active meal buff");

            // 4. Test Damage formula with meal buff
            var hit = CombatFormulas.CalculateDamage(firstUnit.Member, ItemDef.Get(firstUnit.Member.WeaponId), 0f, false, false, false, new Random(42), battle.State.ActiveMealBuff);
            Assert(hit.FinalDamage > 10f, "Damage includes meal attack bonus");
        }

        private static void TestBossHuntingAndHeroRelics()
        {
            var save = SaveSystem.CreateInitialSave();
            save.CompletedMissions.Add("mission-3"); // Unlocks Drake Titan
            save.CompletedMissions.Add("mission-7"); // Unlocks Iron Behemoth
            save.CompletedMissions.Add("mission-8"); // Unlocks Colossus Golem

            // 1. Verify Unlocked Bosses & Initial Levels
            Assert(BossHuntingSystem.IsBossUnlocked(save, EnemyKind.DrakeTitan), "Drake Titan unlocked");
            Assert(BossHuntingSystem.IsBossUnlocked(save, EnemyKind.IronBehemoth), "Iron Behemoth unlocked");
            Assert(BossHuntingSystem.IsBossUnlocked(save, EnemyKind.ColossusGolem), "Colossus Golem unlocked");
            Assert(BossHuntingSystem.GetBossLevel(save, EnemyKind.DrakeTitan) == 1, "Initial Drake level is 1");

            var availableHunts = BossHuntingSystem.GetAvailableBossHunts(save);
            Assert(availableHunts.Count == 3, "3 boss hunts available");

            // 2. Generate Mission & Verify Level Scaling
            var lvl1Mission = BossHuntingSystem.GenerateBossHuntMission(EnemyKind.DrakeTitan, 1);
            var lvl5Mission = BossHuntingSystem.GenerateBossHuntMission(EnemyKind.DrakeTitan, 5);

            Assert(lvl5Mission.GoldReward > lvl1Mission.GoldReward, "Gold rewards scale with boss level");
            Assert(lvl5Mission.RecommendedGearScore > lvl1Mission.RecommendedGearScore, "Gear score scales with boss level");
            Assert(lvl5Mission.Waves.Count > lvl1Mission.Waves.Count, "Higher level boss hunt features escort waves");
            Assert(lvl5Mission.LootTable.Contains("hero-mask-apex"), "Level 5+ boss trial offers rare Apex Crown and Ancient Ore");

            // 3. Test Victory Recording and Level Advancement
            BossHuntingSystem.RecordBossVictory(save, EnemyKind.DrakeTitan, 1);
            Assert(BossHuntingSystem.GetBossLevel(save, EnemyKind.DrakeTitan) == 2, "Drake level advanced to 2");

            BossHuntingSystem.RecordBossVictory(save, EnemyKind.DrakeTitan, 2);
            Assert(BossHuntingSystem.GetBossLevel(save, EnemyKind.DrakeTitan) == 3, "Drake level advanced to 3");

            // 4. Test Hero Mask Relic Equipment & Hero Mode Buff
            var heroMember = save.Roster.Find(u => u.Class == UnitClass.Swordsman);
            save.AddItem("hero-mask-wrath", 1);
            bool maskEquipped = BarracksManager.EquipItem(save, heroMember.Id, "hero-mask-wrath");
            Assert(maskEquipped, "Equipped Hero Mask of Wrath");
            Assert(heroMember.MaskId == "hero-mask-wrath", "Unit member has mask id");

            var battleState = new BattleState();
            var heroUnit = new LiveUnit { Member = heroMember, CurrentHp = 150f, MaxHp = 150f, X = 100f };
            battleState.Units.Add(heroUnit);

            var enemyTarget = new LiveEnemy { Id = "test_boss", Kind = EnemyKind.DrakeTitan, CurrentHp = 1000f, MaxHp = 1000f, X = 200f };
            battleState.Enemies.Add(enemyTarget);

            var heroAction = HeroSystem.ExecuteHeroAction(battleState, heroUnit, CommandId.Attack, new Random(42));
            Assert(heroAction.TriggeredHeroMode, "Hero mode action triggered with Mask of Wrath");
            Assert(heroAction.DamageDealt > 0f, "Hero action dealt amplified damage");

            // 5. Test MissionDatabase resolving dynamic boss hunt ID
            var resolvedMission = MissionDatabase.Get("bosshunt-draketitan-lvl3");
            Assert(resolvedMission != null, "MissionDatabase resolved dynamic boss hunt ID");
            Assert(resolvedMission.GoldReward > 300, "Dynamic mission has scaled gold reward");
        }

        private static void TestMiracleRitualDanceSystem()
        {
            var miracleSystem = new MiracleSystem();

            // 1. Start Miracle Ritual for Rain
            miracleSystem.StartMiracleRitual(MiracleType.Rain, 2);
            Assert(miracleSystem.Ritual.IsRitualActive, "Miracle ritual is active");
            Assert(miracleSystem.Ritual.CurrentRound == 1, "Round 1 initialized");
            Assert(miracleSystem.Ritual.ChantPrompt == "TING TING TING TAK", "Round 1 chant prompt set");
            Assert(!miracleSystem.State.IsActive, "Miracle not active until ritual finished");

            // 2. Play Round 1 (Ting Ting Ting Tak)
            miracleSystem.RecordRitualDrumTap(DrumId.Ting);
            miracleSystem.RecordRitualDrumTap(DrumId.Ting);
            miracleSystem.RecordRitualDrumTap(DrumId.Ting);
            miracleSystem.RecordRitualDrumTap(DrumId.Tak);

            Assert(miracleSystem.Ritual.CurrentRound == 2, "Advanced to round 2");
            Assert(miracleSystem.Ritual.ChantPrompt == "BOOM TING BOOM TING", "Round 2 chant prompt set");

            // 3. Play Round 2 (Boom Ting Boom Ting)
            miracleSystem.RecordRitualDrumTap(DrumId.Boom);
            miracleSystem.RecordRitualDrumTap(DrumId.Ting);
            miracleSystem.RecordRitualDrumTap(DrumId.Boom);
            miracleSystem.RecordRitualDrumTap(DrumId.Ting);

            Assert(!miracleSystem.Ritual.IsRitualActive, "Ritual finished");
            Assert(miracleSystem.Ritual.Stage == MiracleRitualStage.Success, "Ritual stage is Success");
            Assert(miracleSystem.State.IsActive, "Divine Rain Miracle is now active");
            Assert(miracleSystem.State.ActiveMiracle == MiracleType.Rain, "Active miracle is Rain");
            Assert(miracleSystem.State.RemainingDurationSeconds >= 15.0f, "Extended duration applied for completed ritual");

            // 4. Test Miracle ritual failure on wrong drum
            miracleSystem.StartMiracleRitual(MiracleType.Tailwind, 2);
            Assert(miracleSystem.Ritual.IsRitualActive, "Tailwind ritual active");
            miracleSystem.RecordRitualDrumTap(DrumId.Boom); // Wrong beat (expected Ting)
            Assert(!miracleSystem.Ritual.IsRitualActive, "Ritual failed on wrong input");
            Assert(miracleSystem.Ritual.Stage == MiracleRitualStage.Failed, "Ritual marked failed");
        }

        private static void TestWeatherAndWindDrift()
        {
            var weatherSystem = new WeatherSimulationSystem();

            // 1. Initial State in Jungle Biome (Rain & mild tailwind)
            weatherSystem.InitializeForBiome(BiomeType.JungleFort, WeatherType.Rain, 20f);
            Assert(weatherSystem.State.CurrentWeather == WeatherType.Rain, "Weather initialized to Rain");
            Assert(weatherSystem.State.Direction == WindDirection.Tailwind, "Wind direction is Tailwind (> 10)");
            Assert(weatherSystem.State.IsRainDousingFire, "Rain douses fire");

            // 2. Wind drift calculation on ballistic projectiles
            float flightDuration = 1.0f;
            float playerDrift = weatherSystem.CalculateWindDriftOffset(flightDuration, isPlayerProjectile: true);
            float enemyDrift = weatherSystem.CalculateWindDriftOffset(flightDuration, isPlayerProjectile: false);

            Assert(playerDrift > 0f, "Tailwind pushes player projectile forward (+X)");
            Assert(enemyDrift < 0f, "Tailwind pushes enemy projectile backward (-X)");

            // 3. Parabolic arc wind drift integration
            var calmPoint = BallisticsCalculator.SampleParabolicArc(0f, 0f, 400f, 0f, 0.5f, 100f, windDriftX: 0f);
            var driftedPoint = BallisticsCalculator.SampleParabolicArc(0f, 0f, 400f, 0f, 0.5f, 100f, windDriftX: playerDrift);

            Assert(driftedPoint.X > calmPoint.X, "Drifted midpoint X is further than calm midpoint X");

            // 4. ProjectileView with wind drift
            var proj = new ProjectileView("p1", ProjectileVisualType.Arrow, 0f, 0f, 400f, 0f, 400f, 60f, null, windDriftX: 30f);
            proj.Update(0.5f); // Halfway progress
            Assert(proj.CurrentX > 200f, "Projectile position reflects positive wind drift progression");

            // 5. Headwind shift & wind gust
            weatherSystem.SetWeather(WeatherType.Blizzard, 0.9f, -30f);
            Assert(weatherSystem.State.Direction == WindDirection.Headwind, "Negative wind speed yields Headwind");
            Assert(weatherSystem.State.MovementModifier < 0.9f, "Blizzard reduces movement speed");

            weatherSystem.ApplyWindGust(50f, 2.0f); // -30 + 50 = +20 (Tailwind)
            Assert(weatherSystem.State.Direction == WindDirection.Tailwind, "Gust temporarily shifts wind direction");
            weatherSystem.Update(2.5f); // Gust expires
            Assert(weatherSystem.State.Direction == WindDirection.Headwind, "Wind reverts after gust expiration");
        }

        private static void TestMaterTreeAwakeningAndFusion()
        {
            var save = SaveData.CreateDefault();
            var unit = save.Roster.Find(u => u.Class == UnitClass.Swordsman);
            unit.Level = 4; // High enough for Awakening Tiers 1-3
            save.Gold = 1000;
            save.AddItem("mat-sun-cabbage", 10);
            save.AddItem("mat-iron-slag", 10);
            save.AddItem("mat-beast-hide", 10);
            save.AddItem("mat-hardwood", 10);
            save.AddItem("mat-mithril", 5);

            // 1. Initial State
            Assert(unit.AwakeningTier == 0, "Initial unit is Tier 0 Awakening");
            Assert(MaterTreeAwakening.GetAvailablePoints(unit) == 0, "No mastery points available at Tier 0");

            // 2. Awaken to Tier 1 (+5 Points)
            bool awaken1 = MaterTreeAwakening.AwakenTier(save, unit.Id);
            Assert(awaken1, "Unit awakened to Tier 1");
            Assert(unit.AwakeningTier == 1, "Unit tier is 1");
            Assert(MaterTreeAwakening.GetAvailablePoints(unit) == 5, "5 Mastery points unlocked");

            // 3. Allocate Points into Attack and Defense
            MaterTreeAwakening.AllocateMasteryPoint(unit, MasteryStatType.Attack);
            MaterTreeAwakening.AllocateMasteryPoint(unit, MasteryStatType.Attack);
            MaterTreeAwakening.AllocateMasteryPoint(unit, MasteryStatType.Defense);
            MaterTreeAwakening.AllocateMasteryPoint(unit, MasteryStatType.Health);
            Assert(unit.AttackMastery == 2, "2 points in Attack Mastery (+4% atk)");
            Assert(unit.DefenseMastery == 1, "1 point in Defense Mastery (+2% def)");
            Assert(unit.HealthMastery == 1, "1 point in Health Mastery (+3% hp)");
            Assert(MaterTreeAwakening.GetAvailablePoints(unit) == 1, "1 point remaining");

            // 4. Combat Stats verification with Masteries
            var baseStats = CombatFormulas.CalculateMemberStats(unit);
            Assert(baseStats.Attack > 5f, "Attack stats reflect weapon and attack mastery");

            // 5. Awaken to Tier 2 and Tier 3
            MaterTreeAwakening.AwakenTier(save, unit.Id); // Tier 2 (+5 points, total 10)
            MaterTreeAwakening.AwakenTier(save, unit.Id); // Tier 3 (+5 points, total 15)
            Assert(unit.AwakeningTier == 3, "Unit is Tier 3");
            Assert(MaterTreeAwakening.GetMaxAllowedPoints(unit) == 15, "15 Total mastery points available");

            // 6. Secondary Subspecies Fusion (Tier 3 Requirement)
            unit.Subspecies = Subspecies.Swiftpaw; // Primary
            bool canFuseSame = MaterTreeAwakening.CanFuseSecondarySubspecies(save, unit.Id, Subspecies.Swiftpaw);
            Assert(!canFuseSame, "Cannot fuse same primary subspecies");

            bool fuseApex = MaterTreeAwakening.FuseSecondarySubspecies(save, unit.Id, Subspecies.Apex);
            Assert(fuseApex, "Fused Apex secondary memory into unit");
            Assert(unit.SecondaryFusion == Subspecies.Apex, "Unit has Apex secondary fusion");

            // 7. Respec Mastery Points
            MaterTreeAwakening.RespecMastery(save, unit.Id);
            Assert(unit.AttackMastery == 0, "Attack mastery reset");
            Assert(MaterTreeAwakening.GetAvailablePoints(unit) == 15, "All 15 points refunded on respec");

            // 8. Camp Controller Integration
            var camp = new CampController(save);
            camp.SetBuilding(CampBuilding.SpiritAltar);
            bool allocated = camp.AllocateMastery(unit.Id, MasteryStatType.Health);
            Assert(allocated, "CampController allocated health mastery");
            Assert(unit.HealthMastery == 1, "Unit health mastery is 1");
            Assert(camp.HUDState.RosterUnits.Find(u => u.Id == unit.Id).HealthMastery == 1, "CampHUDState synced");
        }

        private static void TestAssetCatalogAndAnimationMeta()
        {
            // 1. Verify Unit Class animation metadata
            var spearmanMeta = AssetCatalog.GetUnitMeta(UnitClass.Spearman);
            Assert(spearmanMeta != null, "Spearman metadata exists");
            Assert(spearmanMeta.SpriteWidth == 32 && spearmanMeta.SpriteHeight == 32, "Spearman is 32x32");
            Assert(spearmanMeta.ClipStates.ContainsKey(AnimationClipType.March), "Spearman has March animation");
            Assert(spearmanMeta.ClipStates.ContainsKey(AnimationClipType.Attack), "Spearman has Attack animation");
            Assert(spearmanMeta.ClipStates.ContainsKey(AnimationClipType.Fever), "Spearman has Fever animation");
            Assert(spearmanMeta.Hitbox != null && spearmanMeta.Hurtbox != null, "Hitboxes and Hurtboxes configured");

            var cavalryMeta = AssetCatalog.GetUnitMeta(UnitClass.Cavalry);
            Assert(cavalryMeta.SpriteWidth == 48 && cavalryMeta.SpriteHeight == 48, "Cavalry is 48x48");

            // 2. Verify Enemy animation metadata
            var behemothMeta = AssetCatalog.GetEnemyMeta(EnemyKind.IronBehemoth);
            Assert(behemothMeta != null, "Iron Behemoth metadata exists");
            Assert(behemothMeta.SpriteWidth == 96, "Iron Behemoth boss is 96x96");
            Assert(behemothMeta.ClipStates.ContainsKey(AnimationClipType.Telegraph), "Boss has Telegraph warning animation");
            Assert(behemothMeta.ClipStates.ContainsKey(AnimationClipType.Roar), "Boss has Roar animation");

            // 3. Verify Item Icon Atlas slicing
            PixelRect iconSlice;
            bool foundSpear = AssetCatalog.TryGetItemIconSlice("wpn-wooden-spear", out iconSlice);
            Assert(foundSpear, "Item icon slice found for wooden spear");
            Assert(iconSlice.Width == 24 && iconSlice.Height == 24, "Icon slice is 24x24 px");
        }

        private static void TestGameSettingsAndLatencyCalibration()
        {
            // 1. Latency Calibration Session (8 taps at 120 BPM)
            var calibration = new LatencyCalibrationSession(120f, 4);
            calibration.RegisterTap(0.535f, 0.500f); // +35ms
            calibration.RegisterTap(1.040f, 1.000f); // +40ms
            calibration.RegisterTap(1.530f, 1.500f); // +30ms
            calibration.RegisterTap(2.035f, 2.000f); // +35ms

            Assert(calibration.IsComplete, "Calibration completed after 4 taps");
            float latency = calibration.CalculateCalibratedLatency();
            Assert(Math.Abs(latency - 0.035f) < 0.001f, "Calculated average latency is 35ms (+0.035s)");

            // 2. Game Settings setup and JSON Roundtrip
            var settings = new GameSettings();
            settings.MasterVolume = 0.85f;
            settings.MusicVolume = 0.70f;
            settings.AudioLatencySeconds = latency;
            settings.PixelSnap = true;
            settings.VibrationEnabled = true;

            string json = settings.ToJson();
            var restored = GameSettings.FromJson(json);

            Assert(Math.Abs(restored.MasterVolume - 0.85f) < 0.01f, "Master volume preserved");
            Assert(Math.Abs(restored.MusicVolume - 0.70f) < 0.01f, "Music volume preserved");
            Assert(Math.Abs(restored.AudioLatencySeconds - 0.035f) < 0.001f, "Latency offset preserved");
            Assert(restored.PixelSnap, "Pixel snap flag preserved");

            // 3. Latency adjustment math
            float rawTap = 1.035f;
            float calibratedTap = settings.AdjustInputTimeWithLatency(rawTap);
            Assert(Math.Abs(calibratedTap - 1.000f) < 0.001f, "Raw tap adjusted to exact beat grid time");

            // 4. Audio Controller integration
            var audioCtrl = new AudioController();
            settings.ApplyToAudioController(audioCtrl);
            Assert(audioCtrl.MasterVolume == 0.85f, "AudioController Master volume synced with GameSettings");
        }

        private static void TestScenePrefabRegistryAndTemplates()
        {
            // 1. Battle Scene Setup Template
            var battleTemplate = ScenePrefabRegistry.GetTemplate(SceneType.BattleScene);
            Assert(battleTemplate != null, "Battle scene template exists");
            Assert(battleTemplate.SceneName == "BattleScene", "Scene name matches");
            Assert(battleTemplate.RootGameObjects.Exists(go => go.Name == "PixelPerfectMainCamera"), "Camera rig configured");
            Assert(battleTemplate.RootGameObjects.Exists(go => go.Name == "Moonlighter2DLighting"), "2D Lighting rig configured");
            Assert(battleTemplate.RootGameObjects.Exists(go => go.Name == "BattleController"), "Battle controller configured");
            Assert(battleTemplate.RootGameObjects.Exists(go => go.Name == "BattleHUDCanvas"), "Battle HUD Canvas configured");

            // 2. Camp Scene Setup Template
            var campTemplate = ScenePrefabRegistry.GetTemplate(SceneType.CampHub);
            Assert(campTemplate != null, "Camp scene template exists");
            Assert(campTemplate.SceneName == "CampScene", "Camp scene name matches");
            Assert(campTemplate.RootGameObjects.Exists(go => go.Name == "CampController"), "Camp controller configured");
            Assert(campTemplate.RootGameObjects.Exists(go => go.Name == "CampCanvas"), "Camp canvas configured");
            Assert(campTemplate.RootGameObjects.Exists(go => go.Name == "DialogueCanvas"), "Dialogue cutscene canvas configured");
        }

        private static void TestEnchantingAndElementalGems()
        {
            // 1. Gem Catalog Lookups
            var flameGem = EnchantingSystem.GetGem("gem-flame-2");
            Assert(flameGem != null, "Flame Gem exists");
            Assert(flameGem.Element == GemElement.Flame, "Element is Flame");
            Assert(flameGem.Tier == 2, "Tier is 2");

            // 2. Socketing costs
            var cost = EnchantingSystem.GetSocketCost(2);
            Assert(cost.Gold == 350, "Tier 2 socketing gold cost is 350");
            Assert(cost.MaterialId == "mat-mithril", "Tier 2 requires Mithril");

            // 3. Socketing Gem into Unit Weapon
            var unit = new UnitMember("spear-test", UnitClass.Spearman, 1);
            unit.WeaponId = "wpn-iron-spear";

            var inventory = new Dictionary<string, int>();
            inventory["mat-mithril"] = 5;
            int gold = 1000;

            bool socketed = EnchantingSystem.SocketGem(unit, "gem-flame-2", inventory, ref gold);
            Assert(socketed, "Socket gem succeeded");
            Assert(gold == 650, "Gold deducted (1000 - 350 = 650)");
            Assert(inventory["mat-mithril"] == 2, "Mithril deducted (5 - 3 = 2)");
            Assert(unit.WeaponId == "wpn-iron-spear:gem-flame-2", "Weapon has socketed gem suffix");
            Assert(EnchantingSystem.GetItemElement(unit.WeaponId) == GemElement.Flame, "Weapon element resolved as Flame");
        }

        private static void TestDungeonRuinsAndEscapeMedallion()
        {
            // 1. Floor Generation
            var floor1 = DungeonRuinsSystem.GenerateFloor(1);
            Assert(floor1.FloorNumber == 1, "Floor 1 generated");
            Assert(floor1.Waves.Count == 2, "Floor 1 has 2 normal waves");

            var floor5 = DungeonRuinsSystem.GenerateFloor(5);
            Assert(floor5.FloorNumber == 5, "Floor 5 generated");
            Assert(floor5.Waves.Count == 1, "Floor 5 is a boss room wave");
            Assert(floor5.Affix != DungeonHazardAffix.None, "Floor 5 has a hazard affix");

            // 2. Floor to MissionDef conversion
            var mission = DungeonRuinsSystem.CreateFloorMission(floor5);
            Assert(mission.Id == "dungeon-floor-5", "Mission ID matches floor");
            Assert(mission.GoldReward == floor5.CompletionGoldBonus, "Gold reward matches floor");
            Assert(mission.Waves.Count == 1, "Mission contains generated waves");

            // 3. Merchant Escape Medallion Loot Banking
            var expedition = new DungeonExpeditionState();
            expedition.CurrentFloor = 4;
            expedition.TotalGoldBanked = 1200;
            expedition.PendingLootBag.Add("mat-iron-slag");
            expedition.PendingLootBag.Add("mat-mithril");

            var inventory = new Dictionary<string, int>();
            int playerGold = 500;

            bool escaped = DungeonRuinsSystem.UseEscapeMedallion(expedition, inventory, ref playerGold);
            Assert(escaped, "Escape Medallion successfully used");
            Assert(playerGold == 1700, "Banked gold awarded in full (500 + 1200 = 1700)");
            Assert(inventory["mat-mithril"] == 1, "Mithril added to inventory");
            Assert(expedition.IsEscapedWithMedallion, "Expedition marked as escaped");

            // 4. Wipeout salvage penalty
            var failedExpedition = new DungeonExpeditionState();
            failedExpedition.TotalGoldBanked = 800;
            failedExpedition.PendingLootBag.Add("mat-dragon-scale");
            failedExpedition.PendingLootBag.Add("mat-hardwood");

            int salvageGold = 0;
            var salvageInv = new Dictionary<string, int>();
            DungeonRuinsSystem.HandleExpeditionDefeat(failedExpedition, salvageInv, ref salvageGold);
            Assert(salvageGold == 400, "50% consolation salvage gold retained on wipeout");
            Assert(failedExpedition.IsWipedOut, "Expedition marked as wiped out");
        }

        private static void TestFeatsOfAlmightyCreator()
        {
            var featProgress = new Dictionary<string, int>();
            var completedFeats = new HashSet<string>();
            var inventory = new Dictionary<string, int>();
            int gold = 100;

            // 1. Partial Progress
            bool completed1 = FeatsSystem.ProgressFeat("feat-combat-enemies50", 25, featProgress, completedFeats, inventory, ref gold);
            Assert(!completed1, "Feat not yet completed at 25/50");
            Assert(featProgress["feat-combat-enemies50"] == 25, "Feat progress is 25");

            // 2. Completion & Reward Payout
            bool completed2 = FeatsSystem.ProgressFeat("feat-combat-enemies50", 25, featProgress, completedFeats, inventory, ref gold);
            Assert(completed2, "Feat completed at 50/50");
            Assert(completedFeats.Contains("feat-combat-enemies50"), "Feat registered in completed set");
            Assert(gold == 400, "Gold rewarded (100 + 300 = 400)");
            Assert(inventory["mat-iron-slag"] == 1, "Reward item granted");

            // 3. Repeated progression does not double-award
            bool completedAgain = FeatsSystem.ProgressFeat("feat-combat-enemies50", 10, featProgress, completedFeats, inventory, ref gold);
            Assert(!completedAgain, "Completed feat ignores further progress triggers");
            Assert(gold == 400, "Gold unchanged");
        }

        private static void TestMerchantShopAndBarterMarket()
        {
            var save = SaveData.CreateDefault();
            save.Gold = 1000;

            // 1. Catalog Lookup & Categories
            var catalog = MerchantShopSystem.GetCatalog();
            Assert(catalog.Count >= 10, "Shop catalog has full assortment of items");
            var timberShop = MerchantShopSystem.GetShopItem("shop-mat-timber");
            Assert(timberShop != null, "Timber is sold in shop");
            Assert(timberShop.BasePrice == 25, "Timber base price is 25");

            // 2. Buying standard material
            bool boughtTimber = MerchantShopSystem.BuyItem(save, "shop-mat-timber", 4);
            Assert(boughtTimber, "Bought 4x timber");
            Assert(save.Gold == 900, "Gold deducted (1000 - 100 = 900)");
            Assert(save.GetItemCount("mat-timber") == 14, "Timber added to inventory (10 + 4 = 14)");

            // 3. Daily Discounts
            MerchantShopSystem.SetDailyDiscount("shop-gem-flame-1", 0.20f); // 20% off 150 = 120
            var gemShop = MerchantShopSystem.GetShopItem("shop-gem-flame-1");
            Assert(gemShop.CurrentPrice == 120, "20% discount price is 120");
            bool boughtGem = MerchantShopSystem.BuyItem(save, "shop-gem-flame-1", 1);
            Assert(boughtGem, "Bought discounted flame gem");
            Assert(save.Gold == 780, "Gold deducted (900 - 120 = 780)");
            Assert(save.GetItemCount("gem-flame-1") == 1, "Gem in inventory");

            // 4. Selling Surplus Items
            int startingStone = save.GetItemCount("mat-stone");
            int stoneSellPrice = MerchantShopSystem.GetSellPrice("mat-stone");
            bool soldStone = MerchantShopSystem.SellItem(save, "mat-stone", 5);
            Assert(soldStone, "Sold 5x stone");
            Assert(save.GetItemCount("mat-stone") == startingStone - 5, "Stone deducted from inventory");
            Assert(save.Gold == 780 + (stoneSellPrice * 5), "Gold added from resale");

            // 5. Mystery Satchel Opening
            int startingGold = save.Gold;
            bool boughtSatchel = MerchantShopSystem.BuyItem(save, "shop-satchel-dragon", 1);
            Assert(boughtSatchel, "Bought dragon hoard satchel");
            Assert(save.GetItemCount("mat-dragon-scale") >= 2, "Dragon scale rolled into inventory");
            Assert(save.GetItemCount("mat-ancient-ore") >= 1, "Ancient star ore rolled into inventory");
        }

        private static void TestBattleCharmsAndTacticalItems()
        {
            var battle = new BattleManager();
            var mission = MissionDatabase.Get("mission-1");
            var roster = SaveData.CreateDefault().Roster;
            battle.StartMission(mission, roster);

            // 1. Healing Tincture Charm
            foreach (var unit in battle.State.Units)
            {
                unit.CurrentHp = 10f; // Lower HP to test healing
            }

            var healResult = BattleItemSystem.UseBattleCharm("charm-healing-tincture", battle);
            Assert(healResult.Success, "Healing charm succeeded");
            Assert(healResult.HealingDone > 0, "Healing performed on wounded squad");
            Assert(battle.State.Units[0].CurrentHp > 10f, "Unit HP restored");

            // 2. Harmonic Fever Bell Charm
            Assert(!battle.Rhythm.Fever, "Fever is not yet active");
            var feverResult = BattleItemSystem.UseBattleCharm("charm-fever-bell", battle);
            Assert(feverResult.Success, "Fever bell succeeded");
            Assert(battle.Rhythm.Fever, "Fever mode forced active");
            Assert(feverResult.FeverTriggered, "Fever flag returned");

            // 3. Smoke Bomb Charm (Damage Reduction)
            var smokeResult = BattleItemSystem.UseBattleCharm("charm-smoke-bomb", battle);
            Assert(smokeResult.Success, "Smoke bomb deployed");
            Assert(smokeResult.DamageReductionMultiplier == 0.5f, "Damage reduction is 50%");

            // 4. Purification Incense Charm (Cleansing Debuffs)
            battle.State.Units[0].IsBurning = true;
            battle.State.Units[1].IsRushing = true;
            var purifyResult = BattleItemSystem.UseBattleCharm("charm-purification-incense", battle);
            Assert(purifyResult.Success, "Purification incense used");
            Assert(!battle.State.Units[0].IsBurning, "Burning debuff purged");
            Assert(purifyResult.DebuffsCleansed, "Debuffs marked cleansed");
        }

        private static void TestRhythmMonolithsAndTotemPuzzles()
        {
            var save = SaveData.CreateDefault();
            int startingGold = save.Gold;

            // 1. Monolith Creation
            var monolith = RhythmMonolithSystem.CreateInstance("monolith-rain-shrine", 500f);
            Assert(monolith != null, "Monolith instance created");
            Assert(monolith.WorldX == 500f, "WorldX position initialized");
            Assert(monolith.TotalStages == 3, "Rain shrine requires 3 stages");
            Assert(monolith.CurrentRequiredCommand == CommandId.Defend, "Stage 1 requires Defend");

            // 2. Proximity Check (Too far)
            var farResult = RhythmMonolithSystem.InteractWithMonolith(monolith, 100f, CommandId.Defend, save);
            Assert(!farResult.StageAdvanced, "Interaction rejected when army is too far (dist > 220)");

            // 3. Stage 1 Correct Chant: Defend
            var stage1Result = RhythmMonolithSystem.InteractWithMonolith(monolith, 480f, CommandId.Defend, save);
            Assert(stage1Result.StageAdvanced, "Stage 1 advanced");
            Assert(monolith.CurrentStage == 1, "Current stage is 1");
            Assert(monolith.CurrentRequiredCommand == CommandId.Jump, "Stage 2 requires Jump");

            // 4. Stage 2 Incorrect Chant: Attack instead of Jump
            var wrongResult = RhythmMonolithSystem.InteractWithMonolith(monolith, 480f, CommandId.Attack, save);
            Assert(!wrongResult.StageAdvanced, "Wrong command did not advance stage");
            Assert(monolith.CurrentStage == 1, "Stage remains at 1");

            // 5. Stage 2 Correct Chant: Jump
            var stage2Result = RhythmMonolithSystem.InteractWithMonolith(monolith, 480f, CommandId.Jump, save);
            Assert(stage2Result.StageAdvanced, "Stage 2 advanced");
            Assert(monolith.CurrentStage == 2, "Current stage is 2");
            Assert(monolith.CurrentRequiredCommand == CommandId.Retreat, "Stage 3 requires Retreat");

            // 6. Stage 3 Final Chant: Retreat (Solves puzzle)
            var finalResult = RhythmMonolithSystem.InteractWithMonolith(monolith, 480f, CommandId.Retreat, save);
            Assert(finalResult.MonolithSolved, "Monolith puzzle solved!");
            Assert(monolith.IsSolved, "Monolith marked solved");
            Assert(monolith.RiseHeightPercent == 1.0f, "Totem fully risen to 100%");
            Assert(save.Gold == startingGold + 300, "300 Gold reward granted");
            Assert(save.GetItemCount("relic-rain-charm") >= 1, "Rain relic charm granted into inventory");
            Assert(save.MissionsUnlocked.Contains("mission-rain-shrine"), "Secret rain shrine mission unlocked");
        }

        private static void TestPixelSpriteGeneratorAndAtlas()
        {
            // 1. PixelBitmapBuffer creation & drawing
            var buffer = new PixelBitmapBuffer(32, 32);
            buffer.FillRect(0, 0, 32, 32, PixelColor32.Black);
            buffer.DrawCircle(16, 16, 8, PixelColor32.AmberGold, true);
            Assert(buffer.CountNonEmptyPixels() > 0, "Bitmap buffer contains solid drawn pixels");

            // 2. Unit Sprite Sheet Generation (All 10 Classes)
            foreach (UnitClass unitClass in Enum.GetValues(typeof(UnitClass)))
            {
                var unitSheet = PixelAssetGenerator.GenerateUnitSpriteSheet(unitClass, Subspecies.Normal);
                Assert(unitSheet != null, "Generated sprite sheet for " + unitClass);
                Assert(unitSheet.Width >= 128, "Sprite sheet has at least 4 horizontal frame columns");
                Assert(unitSheet.Height >= 288, "Sprite sheet has 9 animation state rows");
                Assert(unitSheet.CountNonEmptyPixels() > 100, "Sprite sheet contains rasterized pixel art data");
            }

            // 3. Subspecies Skin Color Rendering (Apex Celestial Gold)
            var apexSheet = PixelAssetGenerator.GenerateUnitSpriteSheet(UnitClass.Swordsman, Subspecies.Apex);
            Assert(apexSheet != null, "Generated Apex subspecies sprite sheet");

            // 4. Enemy & Colossal Boss Sprite Sheet Generation (Drake Titan & Plains Runner)
            var bossSheet = PixelAssetGenerator.GenerateEnemySpriteSheet(EnemyKind.DrakeTitan);
            Assert(bossSheet != null, "Generated Drake Titan boss sprite sheet");
            Assert(bossSheet.Width == 96 * 4, "Boss sprite width matches 96px x 4 frames");
            Assert(bossSheet.Height == 96 * 6, "Boss sprite height matches 96px x 6 rows");

            // 5. Item Icon Generation
            var iconSpear = PixelAssetGenerator.GenerateItemIcon("spear-iron");
            Assert(iconSpear.Width == 24 && iconSpear.Height == 24, "Item icon size is 24x24 px");
            Assert(iconSpear.CountNonEmptyPixels() > 20, "Item icon has rasterized pixel art");

            // 6. Biome Parallax Tile Generation
            var coastTile = PixelAssetGenerator.GenerateBiomeTile(BiomeType.CoralCoast, TileLayerType.Ground);
            Assert(coastTile.Width == 16 && coastTile.Height == 16, "Tile size is 16x16 px");

            // 7. Master Texture Atlas Building & Shelf Packing
            var itemAtlas = SpriteAtlasBuilder.BuildItemIconAtlas();
            Assert(itemAtlas.Entries.Count >= 20, "Item icon atlas packed 20+ icons");
            Assert(itemAtlas.GetEntry("spear-iron") != null, "Item entry found in atlas");
            Assert(itemAtlas.AtlasBuffer.Width == 256, "Atlas buffer dimension is 256x256");

            // 8. Portable PPM format export
            byte[] ppmBytes = iconSpear.ExportToPPMBytes();
            Assert(ppmBytes != null && ppmBytes.Length > 50, "Exported pixel bitmap to PPM binary data");
        }

        private static void TestCampHubArtGenerators()
        {
            // 1. NPC Spritesheets (Priestess Leah, Merchant Leo, Blacksmith Vulcan, Chef)
            foreach (CampNPCType npc in Enum.GetValues(typeof(CampNPCType)))
            {
                var sheet = CampArtGenerator.GenerateNPCSpriteSheet(npc);
                Assert(sheet != null, "Generated NPC spritesheet for " + npc);
                Assert(sheet.Width == 48 * 4, "NPC width matches 48px x 4 frames");
                Assert(sheet.Height == 48 * 2, "NPC height matches 48px x 2 rows (Idle + Action)");
                Assert(sheet.CountNonEmptyPixels() > 100, "NPC spritesheet has rasterized pixel art data");
            }

            // 2. Camp Buildings & Structures
            foreach (CampStructureType structType in Enum.GetValues(typeof(CampStructureType)))
            {
                var structure = CampArtGenerator.GenerateCampStructure(structType);
                Assert(structure != null, "Generated camp structure for " + structType);
                Assert(structure.Width == 64 && structure.Height == 64, "Structure size is 64x64 px");
                Assert(structure.CountNonEmptyPixels() > 200, "Camp structure contains rasterized pixel art");
            }
        }

        private static void TestVFXAndUIArtGenerators()
        {
            // 1. Multi-Frame VFX Sprite Sheets
            foreach (VFXSpriteType vfx in Enum.GetValues(typeof(VFXSpriteType)))
            {
                var vfxSheet = VFXArtGenerator.GenerateVFXSpriteSheet(vfx);
                Assert(vfxSheet != null, "Generated VFX sheet for " + vfx);
                Assert(vfxSheet.Height == 32, "VFX height is 32px");
                Assert(vfxSheet.Width >= 128, "VFX width has at least 4 frames");
                Assert(vfxSheet.CountNonEmptyPixels() > 20, "VFX sheet has solid pixels");
            }

            // 2. Ballistic Projectiles (Arrows, Spears, Boulders, Magic Orbs)
            foreach (ProjectileArtType proj in Enum.GetValues(typeof(ProjectileArtType)))
            {
                var projSprite = VFXArtGenerator.GenerateProjectileSprite(proj);
                Assert(projSprite != null, "Generated projectile sprite for " + proj);
                Assert(projSprite.Width == 24 && projSprite.Height == 24, "Projectile is 24x24 px");
                Assert(projSprite.CountNonEmptyPixels() > 10, "Projectile has drawn pixels");
            }

            // 3. Hero Relic Masks (Courage, Valor, Wrath, Apex)
            string[] maskIds = { "hero-mask-courage", "hero-mask-valor", "hero-mask-wrath", "hero-mask-apex" };
            foreach (var maskId in maskIds)
            {
                var maskSprite = VFXArtGenerator.GenerateHeroRelicMask(maskId);
                Assert(maskSprite != null, "Generated hero relic mask for " + maskId);
                Assert(maskSprite.Width == 32 && maskSprite.Height == 32, "Relic mask is 32x32 px");
                Assert(maskSprite.CountNonEmptyPixels() > 50, "Relic mask contains drawn pixels");
            }

            // 4. Drum Chant Buttons across Normal, Pressed, and Fever States
            foreach (DrumId drum in Enum.GetValues(typeof(DrumId)))
            {
                foreach (DrumButtonState state in Enum.GetValues(typeof(DrumButtonState)))
                {
                    var btn = UIArtGenerator.GenerateDrumButton(drum, state);
                    Assert(btn != null, "Generated drum button for " + drum + " (" + state + ")");
                    Assert(btn.Width == 32 && btn.Height == 32, "Drum button is 32x32 px");
                    Assert(btn.CountNonEmptyPixels() > 50, "Drum button contains drawn runes");
                }
            }

            // 5. Dialogue Frame & Beat Track
            var dialogueFrame = UIArtGenerator.GenerateDialogueFrame();
            Assert(dialogueFrame.Width == 96 && dialogueFrame.Height == 48, "Dialogue frame is 96x48 px");

            var beatTrack = UIArtGenerator.GenerateRhythmBeatTrack();
            Assert(beatTrack.Width == 128 && beatTrack.Height == 16, "Beat track is 128x16 px");

            // 6. Currency Coins & Loot Chests
            foreach (CurrencyCoinType coin in Enum.GetValues(typeof(CurrencyCoinType)))
            {
                var coinSprite = UIArtGenerator.GenerateCurrencyCoin(coin);
                Assert(coinSprite.Width == 16 && coinSprite.Height == 16, "Coin is 16x16 px");
            }

            foreach (ChestTierType chest in Enum.GetValues(typeof(ChestTierType)))
            {
                var closed = UIArtGenerator.GenerateLootChest(chest, false);
                var opened = UIArtGenerator.GenerateLootChest(chest, true);
                Assert(closed.Width == 24 && opened.Width == 24, "Chest is 24x24 px");
            }
        }

        private static void TestEnvironmentBackdropsAndDiskExporter()
        {
            // 1. 8 Biome Parallax Backdrops across all 4 layers
            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
            {
                foreach (ParallaxLayerRole layer in Enum.GetValues(typeof(ParallaxLayerRole)))
                {
                    var backdrop = EnvironmentArtGenerator.GenerateBiomeBackdrop(biome, layer);
                    Assert(backdrop != null, "Generated backdrop for " + biome + " (" + layer + ")");
                    Assert(backdrop.Width == EnvironmentArtGenerator.NativeWidth, "Backdrop width is 384 px");
                    Assert(backdrop.Height == EnvironmentArtGenerator.NativeHeight, "Backdrop height is 216 px");
                    Assert(backdrop.CountNonEmptyPixels() > 0, "Backdrop contains solid pixel art");
                }
            }

            // 2. BMP & PNG Binary Encoding Formats
            var testBuffer = new PixelBitmapBuffer(16, 16);
            testBuffer.FillRect(0, 0, 16, 16, PixelColor32.AmberGold);
            testBuffer.DrawCircle(8, 8, 4, PixelColor32.Crimson, true);

            byte[] bmpBytes = testBuffer.ExportToBmp32Bytes();
            Assert(bmpBytes != null && bmpBytes.Length > 54, "BMP bytes generated");
            Assert(bmpBytes[0] == 0x42 && bmpBytes[1] == 0x4D, "BMP magic header bytes match 'BM'");

            byte[] pngBytes = testBuffer.ExportToPngBytes();
            Assert(pngBytes != null && pngBytes.Length > 30, "PNG bytes generated");
            Assert(pngBytes[0] == 137 && pngBytes[1] == 80 && pngBytes[2] == 78 && pngBytes[3] == 71, "PNG magic header matches 0x89 PNG");

            // 3. Batch Asset Disk Exporter Pipeline
            string tempExportDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RhythmArmy_ArtExportTest");
            var summary = AssetDiskExporter.ExportAllAssetsToDisk(tempExportDir);
            Assert(summary.TotalFilesExported >= 60, "Exported 60+ art asset files to disk (actual: " + summary.TotalFilesExported + ")");
            Assert(summary.TotalBytesWritten > 10000, "Written total pixel art bytes to disk: " + summary.TotalBytesWritten);
            Assert(summary.ExportedFilePaths.Count == summary.TotalFilesExported, "Tracked all exported file paths");

            // Clean up temporary test files
            if (System.IO.Directory.Exists(tempExportDir))
            {
                try
                {
                    System.IO.Directory.Delete(tempExportDir, true);
                }
                catch { }
            }
        }

        private static void TestUnitySceneExporterAndStandaloneBoot()
        {
            // 1. Generate Unity Scenes in temp folder
            string tempSceneDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RhythmArmy_SceneTest");
            UnitySceneExporter.GenerateAllScenes(tempSceneDir);

            Assert(System.IO.File.Exists(System.IO.Path.Combine(tempSceneDir, "CampScene.unity")), "CampScene.unity created");
            Assert(System.IO.File.Exists(System.IO.Path.Combine(tempSceneDir, "BattleScene.unity")), "BattleScene.unity created");
            Assert(System.IO.File.Exists(System.IO.Path.Combine(tempSceneDir, "TitleScene.unity")), "TitleScene.unity created");
            Assert(System.IO.File.Exists(System.IO.Path.Combine(tempSceneDir, "CampScene.unity.meta")), "CampScene.unity.meta created");

            // Clean up temporary test scenes
            if (System.IO.Directory.Exists(tempSceneDir))
            {
                try { System.IO.Directory.Delete(tempSceneDir, true); } catch { }
            }

            // Export real scenes to Assets/Scenes
            UnitySceneExporter.GenerateAllScenes("Assets/Scenes");
            Assert(System.IO.File.Exists("Assets/Scenes/CampScene.unity"), "Assets/Scenes/CampScene.unity generated for Unity Editor");
            Assert(System.IO.File.Exists("Assets/Scenes/BattleScene.unity"), "Assets/Scenes/BattleScene.unity generated for Unity Editor");

            // 2. Standalone Runtime Driver Bootstrapping
            var runtime = new GameRuntime(new Random(100));
            runtime.StartNewGame();
            Assert(runtime.IsRunning, "Runtime is initialized and running");
            Assert(runtime.Orchestrator.CurrentState == GameFlowState.CampHub, "Runtime starts in Camp Hub");

            // 3. Test Mission Launch & Input Simulation in Standalone Runtime
            runtime.StartMission("m01-patata-plains");
            if (runtime.Orchestrator.CurrentState == GameFlowState.DialogueCutscene)
            {
                // Advance through pre-mission story cutscene to active battle
                while (runtime.Orchestrator.CurrentState == GameFlowState.DialogueCutscene)
                {
                    runtime.InputManager.ProcessKeyInput("Space");
                }
            }
            Assert(runtime.Orchestrator.CurrentState == GameFlowState.BattleActive, "Runtime transitioned to BattleActive");

            // Simulate 4 drum beats (March: BOOM BOOM BOOM TAK)
            runtime.InputManager.ProcessKeyInput("A");
            runtime.Tick(0.5f);
            runtime.InputManager.ProcessKeyInput("A");
            runtime.Tick(0.5f);
            runtime.InputManager.ProcessKeyInput("A");
            runtime.Tick(0.5f);
            runtime.InputManager.ProcessKeyInput("S");
            runtime.Tick(0.5f);

            Assert(runtime.Orchestrator.BattleMgr != null, "Battle is actively executing");
        }

        private static void TestProceduralAudioSynthesizer()
        {
            // 1. Drum Hits WAV Generation & RIFF Header Validation
            byte[] boom = ProceduralAudioGenerator.GenerateDrumBoom();
            byte[] tak = ProceduralAudioGenerator.GenerateDrumTak();
            byte[] rat = ProceduralAudioGenerator.GenerateDrumRat();
            byte[] ting = ProceduralAudioGenerator.GenerateDrumTing();

            Assert(boom != null && boom.Length > 1000, "Drum Boom WAV generated");
            Assert(tak != null && tak.Length > 1000, "Drum Tak WAV generated");
            Assert(rat != null && rat.Length > 1000, "Drum Rat WAV generated");
            Assert(ting != null && ting.Length > 1000, "Drum Ting WAV generated");

            // Check RIFF header "RIFF....WAVEfmt "
            Assert(boom[0] == 'R' && boom[1] == 'I' && boom[2] == 'F' && boom[3] == 'F', "Valid RIFF header");
            Assert(boom[8] == 'W' && boom[9] == 'A' && boom[10] == 'V' && boom[11] == 'E', "Valid WAVE format tag");

            // 2. Vocal Chants (Boom, Tak, Rat, Ting, Fever)
            string[] chants = { "boom", "tak", "rat", "ting", "fever" };
            foreach (var ch in chants)
            {
                byte[] chantWav = ProceduralAudioGenerator.GenerateVocalChant(ch);
                Assert(chantWav != null && chantWav.Length > 2000, "Generated vocal chant for " + ch);
            }

            // 3. Combat & UI SFX
            byte[] slash = ProceduralAudioGenerator.GenerateSwordSlash();
            byte[] arrow = ProceduralAudioGenerator.GenerateArrowWhistle();
            byte[] shield = ProceduralAudioGenerator.GenerateShieldBlock();
            byte[] roar = ProceduralAudioGenerator.GenerateBossRoar();
            byte[] coin = ProceduralAudioGenerator.GenerateCoinClink();
            byte[] click = ProceduralAudioGenerator.GenerateUIClick();
            byte[] anvil = ProceduralAudioGenerator.GenerateAnvilStrike();

            Assert(slash != null && slash.Length > 500, "Slash SFX generated");
            Assert(arrow != null && arrow.Length > 500, "Arrow SFX generated");
            Assert(shield != null && shield.Length > 500, "Shield SFX generated");
            Assert(roar != null && roar.Length > 2000, "Boss Roar SFX generated");
            Assert(coin != null && coin.Length > 500, "Coin Clink SFX generated");
            Assert(click != null && click.Length > 200, "UI Click SFX generated");
            Assert(anvil != null && anvil.Length > 500, "Anvil Strike SFX generated");

            // 4. Music Fanfares & Multi-track Fever BGM Stems
            byte[] victory = ProceduralAudioGenerator.GenerateVictoryFanfare();
            byte[] defeat = ProceduralAudioGenerator.GenerateDefeatJingle();
            byte[] baseBgm = ProceduralAudioGenerator.GenerateBgmRhythmLoop(false);
            byte[] feverBgm = ProceduralAudioGenerator.GenerateBgmRhythmLoop(true);

            Assert(victory != null && victory.Length > 5000, "Victory Fanfare generated");
            Assert(defeat != null && defeat.Length > 5000, "Defeat Jingle generated");
            Assert(baseBgm != null && baseBgm.Length > 10000, "Base BGM Loop generated");
            Assert(feverBgm != null && feverBgm.Length > 10000, "Fever BGM Loop generated");
        }

        private static void TestNormalMapGenerator()
        {
            // 1. Create a simple test diffuse sprite
            var diffuse = new PixelBitmapBuffer(32, 32);
            diffuse.FillCircle(16, 16, 10, PixelColor32.Crimson);
            diffuse.DrawCircle(16, 16, 10, PixelColor32.AmberGold);

            // 2. Generate Tangent-Space Normal Map
            var normalMap = NormalMapGenerator.GenerateNormalMap(diffuse, strength: 2.5f);
            Assert(normalMap != null, "Normal map generated");
            Assert(normalMap.Width == 32 && normalMap.Height == 32, "Normal map dimensions match diffuse (32x32)");

            // Transparent pixel in corner should have default flat normal (128, 128, 255, 0)
            var corner = normalMap.GetPixel(0, 0);
            Assert(corner.A == 0, "Corner alpha is transparent");
            Assert(corner.R == 128 && corner.G == 128 && corner.B == 255, "Corner has flat tangent-space normal (128, 128, 255)");

            // Solid pixel inside sphere should have non-flat relief gradient
            var center = normalMap.GetPixel(16, 16);
            Assert(center.A > 0, "Center pixel is opaque");
            Assert(center.B >= 200, "Normal Z component is pointing outward");

            // 3. Generate Normal Maps for Unit and Enemy Spritesheets
            var unitSheet = PixelAssetGenerator.GenerateUnitSpriteSheet(UnitClass.Swordsman, Subspecies.Normal);
            var unitNormal = NormalMapGenerator.GenerateNormalMap(unitSheet, 2.0f);
            Assert(unitNormal.Width == unitSheet.Width && unitNormal.Height == unitSheet.Height, "Unit normal map matches sheet dimensions");
            Assert(unitNormal.CountNonEmptyPixels() > 0, "Unit normal map contains relief normal data");
        }

        private static void TestTitleAndWorldMapScreens()
        {
            // 1. Title Screen Splash Banner
            var titleSplash = TitleAndMapArtGenerator.GenerateTitleScreenSplash();
            Assert(titleSplash != null, "Title screen splash generated");
            Assert(titleSplash.Width == 384 && titleSplash.Height == 216, "Title splash resolution is 384x216 px");
            Assert(titleSplash.CountNonEmptyPixels() == 384 * 216, "Title splash has full screen pixel coverage");

            // 2. Campaign World Map Stage Selector
            var worldMap = TitleAndMapArtGenerator.GenerateCampaignWorldMap();
            Assert(worldMap != null, "Campaign world map generated");
            Assert(worldMap.Width == 384 && worldMap.Height == 216, "World map resolution is 384x216 px");
            Assert(worldMap.CountNonEmptyPixels() == 384 * 216, "World map has full parchment coverage");
        }

        private static void TestFloatingCombatTextAndSlowMotion()
        {
            var text = new FloatingCombatText("150 CRIT!", 100f, 50f, FloatingTextType.CriticalDamage, 1.0f);
            Assert(text.Text == "150 CRIT!", "Floating text holds string");
            Assert(text.Alpha > 0.9f, "Initial alpha is high");
            text.Update(0.5f);
            Assert(text.Y < 50f, "Text floats upward");
            Assert(text.Lifetime == 0.5f, "Lifetime reduced");

            var mgr = new BattleManager(new Random(42));
            var mission = MissionDatabase.Get("m01-patata-plains");
            var roster = new List<UnitMember> { new UnitMember("u1", UnitClass.Swordsman, 1) };
            mgr.StartMission(mission, roster);

            mgr.TriggerSlowMotion(0.6f);
            Assert(mgr.IsSlowMotionActive, "Slow motion is active");
            Assert(mgr.CurrentTimeDilation == 0.25f, "Time dilation scale is 0.25x");

            mgr.AddFloatingText("TEST", 10f, 20f, FloatingTextType.StandardDamage);
            Assert(mgr.FloatingTexts.Count == 1, "Floating text registered in BattleManager");

            mgr.Update(0.8f);
            Assert(!mgr.IsSlowMotionActive, "Slow motion expired after duration");
        }

        private static void TestBeatMetronomeAndProgress()
        {
            var engine = new RhythmEngine(120f, 0f);
            engine.Tap(DrumId.Boom, 0f); // Starts engine at t=0

            float progress0 = engine.BeatProgress(0f);
            Assert(Math.Abs(progress0) < 0.05f, "Progress at beat start is near 0");

            float progressHalf = engine.BeatProgress(0.25f); // 0.5s beat length at 120 bpm, so 0.25s is half
            Assert(Math.Abs(progressHalf - 0.5f) < 0.05f, "Progress at half beat is near 0.5");

            var hud = new BattleHUDState();
            var mgr = new BattleManager(new Random(42));
            var mission = MissionDatabase.Get("m01-patata-plains");
            mgr.StartMission(mission, new List<UnitMember> { new UnitMember("u1", UnitClass.Swordsman, 1) });
            mgr.HandleDrumInput(DrumId.Boom);
            mgr.Update(0.1f);
            hud.UpdateFromBattleManager(mgr, 0.1f);

            Assert(hud.BeatProgress >= 0f && hud.BeatProgress <= 1f, "HUD beat progress is normalized 0..1");
            Assert(hud.MetronomeOffset >= -1f && hud.MetronomeOffset <= 1f, "HUD metronome pendulum offset is between -1 and 1");
            Assert(hud.ShowCheatSheet == true, "HUD cheat sheet is enabled by default");
        }

        private static void TestCampOptimizationSummary()
        {
            var save = new SaveData();
            save.Gold = 1000;
            var unit = new UnitMember("u1", UnitClass.Swordsman, 1);
            unit.WeaponId = "sword-wood"; // GS = ~20
            save.Roster.Add(unit);

            // Add better weapon to inventory
            save.AddItem("sword-iron", 1); // GS = ~48

            var summary = EquipmentOptimizer.OptimizeArmyWithSummary(save);
            Assert(summary.UnitsUpgraded == 1, "1 unit was upgraded");
            Assert(summary.ScoreDelta > 0, "Gear score delta is positive");
            Assert(unit.WeaponId == "sword-iron", "Unit equipped with iron sword");
            Assert(summary.SummaryMessage.Contains("Optimized!"), "Summary message formatted correctly");
        }
    }
}
