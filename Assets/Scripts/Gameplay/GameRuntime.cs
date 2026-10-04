using System;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay;
using RhythmArmy.Gameplay.Camp;

namespace RhythmArmy.Gameplay
{
    /// <summary>
    /// Master runtime loop and driver for real-time playability, testing, and Unity standalone execution.
    /// </summary>
    public class GameRuntime
    {
        public GameOrchestrator Orchestrator { get; private set; }
        public RuntimeInputManager InputManager { get; private set; }
        public AudioController Audio { get; private set; }

        public bool IsRunning { get; private set; }
        public float TimeScale { get; set; }
        public float TotalElapsedSeconds { get; private set; }

        public event Action<string> OnLogMessage;

        public GameRuntime(Random rng = null)
        {
            TimeScale = 1.0f;
            Orchestrator = new GameOrchestrator(rng ?? new Random());
            InputManager = new RuntimeInputManager();
            Audio = new AudioController();

            // Wire input manager to orchestrator
            InputManager.OnDrumTriggered += drum =>
            {
                if (Orchestrator.CurrentState == GameFlowState.BattleActive)
                {
                    var judgment = Orchestrator.HandleDrumInput(drum);
                    Audio.PlayDrum(drum, judgment.Grade);
                    if (OnLogMessage != null)
                    {
                        OnLogMessage(string.Format("[DRUM] {0} -> {1} ({2:+0.0;-0.0;0.0}ms)", drum, judgment.Grade, judgment.DeltaMs));
                    }
                }
                else if (Orchestrator.CurrentState == GameFlowState.MinigameActive)
                {
                    Audio.PlayDrum(drum, Grade.Perfect);
                    Orchestrator.HandleDrumInput(drum);
                    if (OnLogMessage != null)
                    {
                        OnLogMessage(string.Format("[MINIGAME] Drum {0} tapped.", drum));
                    }
                }
            };

            InputManager.OnAdvanceCutsceneTriggered += () =>
            {
                if (Orchestrator.CurrentState == GameFlowState.DialogueCutscene)
                {
                    Orchestrator.AdvanceDialogue();
                    if (OnLogMessage != null)
                    {
                        OnLogMessage("[DIALOGUE] Advanced cutscene");
                    }
                }
            };
        }

        public void StartNewGame()
        {
            var initialSave = SaveSystem.CreateInitialSave();
            Orchestrator.Initialize(initialSave);
            IsRunning = true;
            TotalElapsedSeconds = 0f;
            if (OnLogMessage != null)
            {
                OnLogMessage("[RUNTIME] New game started in Camp Hub.");
            }
        }

        public void LoadGame(string json)
        {
            var save = UnityEngineJsonHelper.FromJson<SaveData>(json);
            Orchestrator.Initialize(save);
            IsRunning = true;
            TotalElapsedSeconds = 0f;
            if (OnLogMessage != null)
            {
                OnLogMessage("[RUNTIME] Game loaded successfully.");
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            float scaledDelta = deltaTime * TimeScale;
            TotalElapsedSeconds += scaledDelta;

            Orchestrator.Update(scaledDelta);
            Audio.Update(scaledDelta);
        }

        public bool StartMission(string missionId)
        {
            bool success = Orchestrator.LaunchMission(missionId);
            if (success && OnLogMessage != null)
            {
                OnLogMessage(string.Format("[MISSION] Launched: {0}", missionId));
            }
            return success;
        }

        public void StartMinigame(MinigameType type)
        {
            Orchestrator.StartMinigame(type);
            if (OnLogMessage != null)
            {
                OnLogMessage(string.Format("[MINIGAME] Started: {0}", type));
            }
        }

        public void ReturnToCamp()
        {
            Orchestrator.ReturnToCamp();
            if (OnLogMessage != null)
            {
                OnLogMessage("[RUNTIME] Returned to Camp Hub.");
            }
        }
    }
}
