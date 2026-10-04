using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Gameplay.Camp;
using RhythmArmy.Gameplay.Narrative;
using RhythmArmy.UI;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay
{
    public enum GameFlowState
    {
        CampHub,
        MinigameActive,
        DialogueCutscene,
        BattleActive,
        BattleResultsScreen
    }

    public class GameOrchestrator
    {
        public GameFlowState CurrentState { get; private set; }
        public SaveData Save { get; private set; }

        public CampHUDState CampHUD { get; private set; }
        public CampController CampCtrl { get; private set; }
        public BattleManager BattleMgr { get; private set; }
        public BattleHUDState BattleHUD { get; private set; }
        public DialoguePlayer Dialogue { get; private set; }
        public BiomeTheme ActiveBiome { get; private set; }
        public BattleResult LastBattleResult { get; private set; }

        public event Action<GameFlowState> OnGameStateChanged;

        private Random _rng;

        public GameOrchestrator(Random rng = null)
        {
            _rng = rng ?? new Random();
            CampHUD = new CampHUDState();
            BattleMgr = new BattleManager(_rng);
            BattleHUD = new BattleHUDState();
            Dialogue = new DialoguePlayer();

            // Hook up battle manager events to battle HUD
            BattleMgr.OnCommandEvaluated += ev =>
            {
                if (ev.Command != null)
                {
                    BattleHUD.DisplayCommandChant(ev.Command);
                }
            };

            BattleMgr.OnEnemySlain += (enemy, drops, currency) =>
            {
                if (currency > 0)
                {
                    BattleHUD.AddLootToast("currency_coin", currency);
                }
                foreach (var d in drops)
                {
                    BattleHUD.AddLootToast(d.ItemId, d.Quantity);
                }
            };
        }

        public void Initialize(SaveData save = null)
        {
            Save = save ?? SaveSystem.CreateInitialSave();
            CampCtrl = new CampController(Save);
            CampHUD.RefreshFromSave(Save);

            CampCtrl.OnMinigameCompleted += result =>
            {
                ReturnToCamp();
            };

            SetState(GameFlowState.CampHub);
        }

        public void SetState(GameFlowState newState)
        {
            CurrentState = newState;
            if (OnGameStateChanged != null)
            {
                OnGameStateChanged.Invoke(newState);
            }
        }

        public bool LaunchMission(string missionId)
        {
            var mission = MissionDatabase.Get(missionId);
            if (mission == null) return false;

            var biomeType = EnvironmentSystem.ParseBiome(mission.Biome);
            ActiveBiome = EnvironmentSystem.CreateBiomeTheme(biomeType);

            // Check if briefing dialogue exists
            string briefingId = "briefing-" + missionId;
            var briefing = DialogueDatabase.Get(briefingId);

            // Start battle manager with active meal buff if consumed
            BattleMgr.StartMission(mission, Save.Roster, 120f, Save.ActiveMealBuffId);
            // Meal buff is consumed for this mission run
            Save.ActiveMealBuffId = null;
            BattleHUD.UnitCards.Clear();
            BattleHUD.ActiveLootToasts.Clear();

            if (briefing != null)
            {
                Dialogue.Play(briefing);
                Dialogue.OnSequenceCompleted += OnBriefingCompleted;
                SetState(GameFlowState.DialogueCutscene);
            }
            else
            {
                SetState(GameFlowState.BattleActive);
            }

            return true;
        }

        private void OnBriefingCompleted()
        {
            Dialogue.OnSequenceCompleted -= OnBriefingCompleted;
            SetState(GameFlowState.BattleActive);
        }

        public TapJudgment HandleDrumInput(DrumId drum)
        {
            if (CurrentState == GameFlowState.MinigameActive && CampCtrl != null)
            {
                CampCtrl.SubmitMinigameInput(drum);
                return new TapJudgment(Grade.Perfect, 0f, 0, drum);
            }

            if (CurrentState != GameFlowState.BattleActive)
            {
                return new TapJudgment(Grade.Miss, 0f, 0, drum);
            }

            var judgment = BattleMgr.HandleDrumInput(drum);
            BattleHUD.RecordDrumTap(drum, judgment.Grade, judgment.DeltaMs);
            return judgment;
        }

        public void StartMinigame(MinigameType type)
        {
            if (CampCtrl == null) return;
            SetState(GameFlowState.MinigameActive);
            CampCtrl.StartMinigame(type);
        }

        public void AdvanceDialogue()
        {
            if (CurrentState == GameFlowState.DialogueCutscene)
            {
                Dialogue.AdvanceOrSkip();
            }
        }

        public void Update(float deltaTime)
        {
            switch (CurrentState)
            {
                case GameFlowState.DialogueCutscene:
                    Dialogue.Update(deltaTime);
                    break;

                case GameFlowState.BattleActive:
                    BattleMgr.Update(deltaTime);
                    BattleHUD.UpdateFromBattleManager(BattleMgr, deltaTime);

                    if (BattleMgr.State.Phase == BattlePhase.Victory || BattleMgr.State.Phase == BattlePhase.Defeat)
                    {
                        ProcessBattleCompletion();
                    }
                    break;

                case GameFlowState.CampHub:
                case GameFlowState.BattleResultsScreen:
                    break;
            }
        }

        private void ProcessBattleCompletion()
        {
            LastBattleResult = BattleMgr.FinishBattle();

            if (LastBattleResult.IsVictory)
            {
                Save.Gold += LastBattleResult.GoldEarned;

                // Add loot to inventory
                foreach (var reward in LastBattleResult.LootRewards)
                {
                    Save.AddMaterial(reward.ItemId, reward.Quantity);
                }

                if (!Save.MissionsCleared.Contains(LastBattleResult.MissionId))
                {
                    Save.MissionsCleared.Add(LastBattleResult.MissionId);
                }

                // Unlock next stage if any
                var allMissions = MissionDatabase.GetAll();
                int currentIndex = allMissions.FindIndex(m => m.Id == LastBattleResult.MissionId);
                if (currentIndex >= 0 && currentIndex + 1 < allMissions.Count)
                {
                    string nextId = allMissions[currentIndex + 1].Id;
                    if (!Save.MissionsUnlocked.Contains(nextId))
                    {
                        Save.MissionsUnlocked.Add(nextId);
                    }
                }

                // If boss hunt, record victory
                if (LastBattleResult.MissionId.StartsWith("bosshunt-"))
                {
                    var parts = LastBattleResult.MissionId.Split('-');
                    if (parts.Length >= 3)
                    {
                        string bossStr = parts[1];
                        string lvlStr = parts[2].Replace("lvl", "");
                        int lvl = 1;
                        int.TryParse(lvlStr, out lvl);

                        EnemyKind kind = EnemyKind.DrakeTitan;
                        if (string.Equals(bossStr, "ironbehemoth", StringComparison.OrdinalIgnoreCase))
                        {
                            kind = EnemyKind.IronBehemoth;
                        }
                        else if (string.Equals(bossStr, "colossusgolem", StringComparison.OrdinalIgnoreCase))
                        {
                            kind = EnemyKind.ColossusGolem;
                        }

                        BossHuntingSystem.RecordBossVictory(Save, kind, lvl);
                    }
                }
            }

            SetState(GameFlowState.BattleResultsScreen);
        }

        public void ReturnToCamp()
        {
            CampHUD.RefreshFromSave(Save);
            SetState(GameFlowState.CampHub);
        }

        public bool CraftItem(string recipeResultId)
        {
            var recipe = BlacksmithCrafting.AllRecipes.FirstOrDefault(r => r.ResultItemId == recipeResultId);
            if (recipe == null) return false;

            bool success = BlacksmithCrafting.Craft(recipe, Save);
            if (success)
            {
                CampHUD.RefreshFromSave(Save);
            }
            return success;
        }

        public bool EvolveUnit(string unitId, Subspecies targetSpecies)
        {
            var unit = Save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null) return false;

            bool success = AltarEvolution.EvolveUnit(unit, targetSpecies, Save);
            if (success)
            {
                CampHUD.RefreshFromSave(Save);
            }
            return success;
        }

        public void AutoOptimizeEquipment()
        {
            EquipmentOptimizer.OptimizeAll(Save);
            CampHUD.RefreshFromSave(Save);
        }
    }
}
