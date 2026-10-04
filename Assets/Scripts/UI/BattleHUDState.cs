using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Visuals;

namespace RhythmArmy.UI
{
    public class DrumBeatVisual
    {
        public DrumId Drum;
        public Grade Grade;
        public float TimeOffsetMs;
        public string IconName;

        public DrumBeatVisual(DrumId drum, Grade grade, float offsetMs)
        {
            Drum = drum;
            Grade = grade;
            TimeOffsetMs = offsetMs;
            switch (drum)
            {
                case DrumId.Boom: IconName = "icon_drum_boom_square"; break;
                case DrumId.Tak: IconName = "icon_drum_tak_circle"; break;
                case DrumId.Rat: IconName = "icon_drum_rat_triangle"; break;
                case DrumId.Ting: IconName = "icon_drum_ting_cross"; break;
                default: IconName = "icon_drum_generic"; break;
            }
        }
    }

    public class UnitHUDCard
    {
        public string UnitId;
        public UnitClass Class;
        public Subspecies Species;
        public float CurrentHp;
        public float MaxHp;
        public float HpPercentage { get { return MaxHp > 0 ? CurrentHp / MaxHp : 0f; } }
        public bool IsAlive { get { return CurrentHp > 0; } }
        public bool IsCharged;
        public bool IsAirborne;
        public bool IsDefending;
        public bool IsBurned;
    }

    public class BossHUDTarget
    {
        public string Name;
        public EnemyKind Kind;
        public float CurrentHp;
        public float MaxHp;
        public float HpPercentage { get { return MaxHp > 0 ? CurrentHp / MaxHp : 0f; } }
        public bool IsAlive { get { return CurrentHp > 0; } }
        public bool HasActiveTelegraph;
        public string TelegraphWarning;
        public int WindupBeatsRemaining;
        public BossAttackType AttackType;
    }

    public class LootToast
    {
        public string ItemId;
        public string ItemName;
        public int Quantity;
        public ItemRarity Rarity;
        public float TimeRemainingSeconds;

        public LootToast(string itemId, int quantity, float durationSeconds = 3.0f)
        {
            ItemId = itemId;
            Quantity = quantity;
            TimeRemainingSeconds = durationSeconds;

            var item = ItemDatabase.Get(itemId);
            if (item != null)
            {
                ItemName = item.Name;
                Rarity = item.Rarity;
            }
            else
            {
                ItemName = itemId;
                Rarity = ItemRarity.Common;
            }
        }
    }

    public class BattleHUDState
    {
        public int CurrentBeatIndex; // 0..3
        public MeasurePhase Phase;
        public float BeatPulseScale = 1.0f; // Bounces to 1.3 on beat tick
        public float BeatProgress = 0f; // 0.0 to 1.0 within beat
        public float MetronomeOffset = 0f; // -1.0 to 1.0 pendulum swing
        public List<DrumBeatVisual> RecentBeats = new List<DrumBeatVisual>();

        public int ComboCount;
        public bool IsFeverActive;
        public float FeverBarPercentage; // 0.0 to 1.0

        public string ActiveCommandChant;
        public string ActiveCommandName;
        public bool ShowCheatSheet = true;

        public bool IsSlowMotionActive;
        public float SlowMotionFactor = 1.0f;

        public List<UnitHUDCard> UnitCards = new List<UnitHUDCard>();
        public BossHUDTarget BossTarget = null;
        public List<LootToast> ActiveLootToasts = new List<LootToast>();
        public List<FloatingCombatText> FloatingTexts = new List<FloatingCombatText>();

        // Miracle & Juju Ritual HUD state
        public bool IsMiracleActive;
        public MiracleType ActiveMiracleType;
        public float MiracleRemainingSeconds;
        public bool IsRitualActive;
        public MiracleRitualStage RitualStage;
        public string RitualPrompt;
        public int RitualRound;
        public int RitualTotalRounds;

        // Stage Weather HUD state
        public WeatherType StageWeather;
        public float WindSpeed;
        public WindDirection WindDir;

        public void UpdateFromBattleManager(BattleManager manager, float deltaTime)
        {
            if (manager == null || manager.State == null) return;

            var battle = manager.State;
            var rhythm = manager.Rhythm;

            if (rhythm != null)
            {
                CurrentBeatIndex = rhythm.CurrentBeatInMeasure;
                Phase = rhythm.Phase;
                ComboCount = rhythm.Combo;
                IsFeverActive = rhythm.IsFever;
                FeverBarPercentage = Math.Min(1.0f, (float)rhythm.Combo / RhythmEngine.FEVER_COMBO_THRESHOLD);

                // Pulse beat animation
                float beatTime = rhythm.TimeSinceBeat(manager.ElapsedTime);
                BeatPulseScale = 1.0f + Math.Max(0f, (0.15f - beatTime) * 2.0f);
                BeatProgress = rhythm.BeatProgress(manager.ElapsedTime);
                // Sine wave pendulum metronome (-1.0 to 1.0)
                MetronomeOffset = (float)Math.Sin(BeatProgress * Math.PI * 2.0);
            }

            IsSlowMotionActive = manager.IsSlowMotionActive;
            SlowMotionFactor = manager.CurrentTimeDilation;

            // Sync floating combat texts
            FloatingTexts.Clear();
            if (manager.FloatingTexts != null)
            {
                FloatingTexts.AddRange(manager.FloatingTexts);
            }

            // Sync Miracles and Rituals
            if (manager.Miracles != null)
            {
                IsMiracleActive = manager.Miracles.State.IsActive;
                ActiveMiracleType = manager.Miracles.State.ActiveMiracle;
                MiracleRemainingSeconds = manager.Miracles.State.RemainingDurationSeconds;

                if (manager.Miracles.Ritual != null)
                {
                    IsRitualActive = manager.Miracles.Ritual.IsRitualActive;
                    RitualStage = manager.Miracles.Ritual.Stage;
                    RitualPrompt = manager.Miracles.Ritual.ChantPrompt;
                    RitualRound = manager.Miracles.Ritual.CurrentRound;
                    RitualTotalRounds = manager.Miracles.Ritual.RequiredRounds;
                }
            }

            // Sync Weather
            if (manager.Weather != null)
            {
                StageWeather = manager.Weather.State.CurrentWeather;
                WindSpeed = manager.Weather.State.WindSpeed;
                WindDir = manager.Weather.State.Direction;
            }

            // Sync Unit Cards
            UnitCards.Clear();
            foreach (var unit in battle.Units)
            {
                UnitCards.Add(new UnitHUDCard
                {
                    UnitId = unit.Member.Id,
                    Class = unit.Member.Class,
                    Species = unit.Member.Species,
                    CurrentHp = unit.CurrentHp,
                    MaxHp = unit.MaxHp,
                    IsCharged = battle.IsCharged,
                    IsAirborne = battle.IsAirborne,
                    IsDefending = battle.IsDefending,
                    IsBurned = false
                });
            }

            // Sync Boss / Target HUD
            var bossEnemy = battle.Enemies.FirstOrDefault(e => e.IsAlive && EnemyMetadata.GetCategory(e.Kind) == EnemyCategory.Boss);
            if (bossEnemy != null)
            {
                if (BossTarget == null) BossTarget = new BossHUDTarget();
                BossTarget.Name = bossEnemy.Kind.ToString();
                BossTarget.Kind = bossEnemy.Kind;
                BossTarget.CurrentHp = bossEnemy.CurrentHp;
                BossTarget.MaxHp = bossEnemy.MaxHp;

                EnemyCombatState ai;
                if (manager.EnemyAIStates.TryGetValue(bossEnemy.Id, out ai) && ai.ActiveTelegraph != null)
                {
                    BossTarget.HasActiveTelegraph = true;
                    BossTarget.TelegraphWarning = ai.ActiveTelegraph.WarningHint;
                    BossTarget.WindupBeatsRemaining = ai.ActiveTelegraph.WindupBeatsRemaining;
                    BossTarget.AttackType = ai.ActiveTelegraph.AttackType;
                }
                else
                {
                    BossTarget.HasActiveTelegraph = false;
                    BossTarget.TelegraphWarning = string.Empty;
                    BossTarget.WindupBeatsRemaining = 0;
                }
            }
            else
            {
                BossTarget = null;
            }

            // Update Loot Toasts
            for (int i = ActiveLootToasts.Count - 1; i >= 0; i--)
            {
                ActiveLootToasts[i].TimeRemainingSeconds -= deltaTime;
                if (ActiveLootToasts[i].TimeRemainingSeconds <= 0f)
                {
                    ActiveLootToasts.RemoveAt(i);
                }
            }
        }

        public void RecordDrumTap(DrumId drum, Grade grade, float offsetMs)
        {
            if (RecentBeats.Count >= 4)
            {
                RecentBeats.RemoveAt(0);
            }
            RecentBeats.Add(new DrumBeatVisual(drum, grade, offsetMs));
        }

        public void DisplayCommandChant(CommandDef command)
        {
            if (command != null)
            {
                ActiveCommandChant = command.Chant;
                ActiveCommandName = command.Name.ToUpperInvariant();
            }
        }

        public void AddLootToast(string itemId, int quantity)
        {
            ActiveLootToasts.Add(new LootToast(itemId, quantity));
        }
    }
}
