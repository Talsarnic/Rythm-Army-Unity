using System;
using System.Collections.Generic;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Gameplay.Battle;

namespace RhythmArmy.UI
{
    /// <summary>
    /// UI Presentation Controller for the in-battle HUD.
    /// Bridges BattleHUDState and BattleController with Unity UI Canvas / UI Toolkit elements.
    /// </summary>
    public class BattleUIController
    {
        public BattleController BattleController { get; private set; }
        public BattleHUDState HUDState
        {
            get { return BattleController != null ? BattleController.HUDState : null; }
        }

        // UI Event Callbacks
        public event Action<int, float> OnBeatPulse; // beatIndex, pulseScale
        public event Action<string, string> OnCommandChantDisplayed; // chantText, commandName
        public event Action<int, bool, float> OnFeverStateChanged; // combo, isFever, feverBarPercent
        public event Action<string, int> OnLootToastSpawned; // itemId, quantity
        public event Action<BossHUDTarget> OnBossTargetUpdated;

        private int _lastBeatIndex;
        private int _lastCombo;
        private bool _lastFever;

        public BattleUIController(BattleController battleController)
        {
            if (battleController == null) throw new ArgumentNullException("battleController");
            BattleController = battleController;
            _lastBeatIndex = -1;
            _lastCombo = -1;
            _lastFever = false;
        }

        public void Update(float deltaTime)
        {
            if (HUDState == null) return;

            // Check beat pulse tick
            if (HUDState.CurrentBeatIndex != _lastBeatIndex)
            {
                _lastBeatIndex = HUDState.CurrentBeatIndex;
                if (OnBeatPulse != null)
                {
                    OnBeatPulse(_lastBeatIndex, HUDState.BeatPulseScale);
                }
            }

            // Check combo / fever changes
            if (HUDState.ComboCount != _lastCombo || HUDState.IsFeverActive != _lastFever)
            {
                _lastCombo = HUDState.ComboCount;
                _lastFever = HUDState.IsFeverActive;
                if (OnFeverStateChanged != null)
                {
                    OnFeverStateChanged(_lastCombo, _lastFever, HUDState.FeverBarPercentage);
                }
            }

            // Boss target updates
            if (HUDState.BossTarget != null)
            {
                if (OnBossTargetUpdated != null)
                {
                    OnBossTargetUpdated(HUDState.BossTarget);
                }
            }
        }

        public void NotifyCommandChant(string chant, string commandName)
        {
            if (OnCommandChantDisplayed != null)
            {
                OnCommandChantDisplayed(chant, commandName);
            }
        }

        public void NotifyLootToast(string itemId, int quantity)
        {
            if (OnLootToastSpawned != null)
            {
                OnLootToastSpawned(itemId, quantity);
            }
        }
    }
}
