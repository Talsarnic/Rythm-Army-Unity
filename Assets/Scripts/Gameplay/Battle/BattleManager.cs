using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay.Battle
{
    public class BattleResult
    {
        public bool IsVictory;
        public string MissionId;
        public int TotalCommandsIssued;
        public int MaxCombo;
        public bool AchievedFever;
        public int EnemiesDefeated;
        public int GoldEarned;
        public List<LootRewardSummary> LootRewards = new List<LootRewardSummary>();
        public float BattleDurationSeconds;
    }

    public class BattleManager
    {
        public BattleState State { get; private set; }
        public RhythmEngine Rhythm { get; private set; }
        public RhythmAudioSequencer AudioSequencer { get; private set; }
        public PixelCamera Camera { get; private set; }
        public Dictionary<string, EnemyCombatState> EnemyAIStates { get; private set; }
        public MiracleSystem Miracles { get; private set; }
        public WeatherSimulationSystem Weather { get; private set; }
        public MiracleType EquippedMiracle { get; set; }
        public HeroActionResult LastHeroAction { get; private set; }
        public List<FloatingCombatText> FloatingTexts { get; private set; }

        public float ElapsedTime { get; private set; }
        public int CurrentMeasure { get; private set; }
        public int LastExecutedMeasure { get; private set; }
        public int MaxComboAchieved { get; private set; }
        public bool FeverAchieved { get; private set; }
        public int TotalEnemiesKilled { get; private set; }
        public int CurrencyCollectedOnKills { get; private set; }

        // Time dilation / Slow-motion state (e.g. on Boss defeat)
        public bool IsSlowMotionActive { get { return SlowMotionTimer > 0f; } }
        public float CurrentTimeDilation { get { return IsSlowMotionActive ? 0.25f : 1.0f; } }
        public float SlowMotionTimer { get; private set; }

        public event Action<RhythmEvent> OnCommandEvaluated;
        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<LiveEnemy, List<LootRewardSummary>, int> OnEnemySlain;
        public event Action<HeroActionResult> OnHeroModeTriggered;

        private Random _rng;

        public BattleManager(Random rng = null)
        {
            _rng = rng ?? new Random();
            EnemyAIStates = new Dictionary<string, EnemyCombatState>();
            Camera = new PixelCamera();
            AudioSequencer = new RhythmAudioSequencer();
            Miracles = new MiracleSystem();
            Weather = new WeatherSimulationSystem();
            FloatingTexts = new List<FloatingCombatText>();
            EquippedMiracle = MiracleType.Rain;
        }

        public void StartMission(MissionDef mission, List<UnitMember> roster, float bpm = 120f, string activeMealBuffId = null)
        {
            State = new BattleState();
            var mealBuff = !string.IsNullOrEmpty(activeMealBuffId) ? ItemDef.Get(activeMealBuffId) : null;
            CombatRules.InitializeBattle(State, mission, roster, mealBuff);

            Rhythm = new RhythmEngine(bpm, null);
            Rhythm.Start(0f);
            AudioSequencer.SetBPM(bpm);
            AudioSequencer.SetFeverActive(false);

            EnemyAIStates.Clear();
            FloatingTexts.Clear();
            ElapsedTime = 0f;
            CurrentMeasure = 0;
            LastExecutedMeasure = -1;
            MaxComboAchieved = 0;
            FeverAchieved = false;
            TotalEnemiesKilled = 0;
            LastHeroAction = null;
            SlowMotionTimer = 0f;

            // Determine equipped miracle from Banner relic or default to Rain
            var banner = roster != null ? roster.FirstOrDefault(u => u.Class == UnitClass.Banner) : null;
            if (banner != null && !string.IsNullOrEmpty(banner.RelicId))
            {
                EquippedMiracle = MiracleSystem.GetMiracleFromRelicId(banner.RelicId);
            }
            if (EquippedMiracle == MiracleType.None)
            {
                EquippedMiracle = MiracleType.Rain;
            }
            Miracles.State.Clear();

            // Initialize Weather from mission biome
            var biomeType = EnvironmentSystem.ParseBiome(mission.Biome);
            var biomeTheme = EnvironmentSystem.CreateBiomeTheme(biomeType);
            var initialWeather = biomeTheme != null && biomeTheme.Weather != null ? biomeTheme.Weather.Type : WeatherType.Clear;
            var initialWind = biomeTheme != null && biomeTheme.Weather != null ? biomeTheme.Weather.WindSpeedX : 0f;
            Weather.InitializeForBiome(biomeType, initialWeather, initialWind);

            Camera.SnapTo(State.BannerX + 100f, 0f);
        }

        public TapJudgment HandleDrumInput(DrumId drum)
        {
            if (State == null || State.Phase != BattlePhase.Marching)
            {
                return new TapJudgment(Grade.Miss, 0f, 0, drum);
            }

            var judgment = Rhythm.Tap(drum, ElapsedTime);
            AudioSequencer.PlayDrum(drum, judgment.Grade == Grade.Perfect);

            if (judgment.Grade == Grade.Perfect)
            {
                Camera.AddTrauma(0.04f); // Subtle responsive punch
            }

            if (Miracles != null && Miracles.Ritual.IsRitualActive)
            {
                Miracles.RecordRitualDrumTap(drum);
            }

            return judgment;
        }

        public void TriggerSlowMotion(float duration = 0.6f)
        {
            SlowMotionTimer = duration;
        }

        public void AddFloatingText(string text, float x, float y, FloatingTextType type, float duration = 1.0f)
        {
            if (FloatingTexts == null) FloatingTexts = new List<FloatingCombatText>();
            FloatingTexts.Add(new FloatingCombatText(text, x, y, type, duration));
        }

        public void Update(float deltaTime)
        {
            if (State == null || State.Phase == BattlePhase.Victory || State.Phase == BattlePhase.Defeat)
            {
                return;
            }

            if (SlowMotionTimer > 0f)
            {
                SlowMotionTimer -= deltaTime;
                deltaTime *= 0.25f; // Apply slow-motion time dilation to combat simulation
            }

            // Update floating texts
            for (int i = FloatingTexts.Count - 1; i >= 0; i--)
            {
                FloatingTexts[i].Update(deltaTime);
                if (FloatingTexts[i].Lifetime <= 0f)
                {
                    FloatingTexts.RemoveAt(i);
                }
            }

            ElapsedTime += deltaTime;
            AudioSequencer.Update(deltaTime);

            // Update Rhythm Engine
            var rhythmEvents = Rhythm.AdvanceBeats(ElapsedTime);
            CurrentMeasure = (int)(ElapsedTime / (Rhythm.BeatInterval * RhythmConstants.MeasureBeats));

            if (Rhythm.Combo > MaxComboAchieved)
            {
                MaxComboAchieved = Rhythm.Combo;
            }

            if (Rhythm.IsFever)
            {
                FeverAchieved = true;
                AudioSequencer.SetFeverActive(true);
            }
            else
            {
                AudioSequencer.SetFeverActive(false);
            }

            foreach (var ev in rhythmEvents)
            {
                if (ev.Type == RhythmEventType.Command)
                {
                    CurrentMeasure = ev.Measure;
                    LastExecutedMeasure = ev.Measure;
                    ExecutePlayerCommand(ev);

                    // Advance Enemy AI on measure
                    EnemyAISystem.UpdateEnemyTurn(State, EnemyAIStates, _rng);

                    // Check for defeated enemies to roll loot
                    CheckDefeatedEnemies();
                }
            }

            // Check wave transitions and victory/defeat
            var prevPhase = State.Phase;
            CombatRules.CheckBattleConditions(State);
            if (State.Phase != prevPhase && OnPhaseChanged != null)
            {
                OnPhaseChanged.Invoke(State.Phase);
            }

            // Update Miracles
            Miracles.Update(deltaTime, State, Camera, _rng);

            // Synchronize Miracle weather effects if active
            if (Miracles.State.IsActive)
            {
                if (Miracles.State.ActiveMiracle == MiracleType.Rain)
                {
                    Weather.SetWeather(WeatherType.Rain, 0.9f, 15f);
                }
                else if (Miracles.State.ActiveMiracle == MiracleType.Tailwind)
                {
                    Weather.SetWeather(WeatherType.Clear, 0.4f, 60f); // Strong tailwind
                }
                else if (Miracles.State.ActiveMiracle == MiracleType.Storm)
                {
                    Weather.SetWeather(WeatherType.Rain, 1.0f, 40f);
                }
            }

            // Update Weather Simulation
            Weather.Update(deltaTime);

            // Update camera tracking
            UpdateCameraTracking(deltaTime);
        }

        private void ExecutePlayerCommand(RhythmEvent rhythmEvent)
        {
            if (rhythmEvent.Command == null) return;

            // Trigger vocal chants in audio sequencer
            AudioSequencer.QueueResponseChants(rhythmEvent.Command.DrumSequence, ElapsedTime);

            // Execute combat action
            CombatRules.ExecuteCommand(State, rhythmEvent.Command.Id, Rhythm.IsFever, _rng);

            // Handle Miracle Invocation
            if (rhythmEvent.Command.Id == CommandId.Miracle)
            {
                if (Rhythm.IsFever)
                {
                    var miracleToInvoke = EquippedMiracle != MiracleType.None ? EquippedMiracle : MiracleType.Rain;
                    Miracles.StartMiracleRitual(miracleToInvoke, 2);
                    Camera.AddTrauma(0.50f);
                }
            }

            // Handle Hero Mode Trigger
            if (HeroSystem.CanTriggerHeroMode(rhythmEvent.Command.Id, Rhythm.IsFever, rhythmEvent.Perfects))
            {
                var hero = HeroSystem.GetHeroUnit(State.Units);
                if (hero != null)
                {
                    LastHeroAction = HeroSystem.ExecuteHeroAction(State, hero, rhythmEvent.Command.Id, _rng);
                    Camera.AddTrauma(0.30f);
                    if (OnHeroModeTriggered != null)
                    {
                        OnHeroModeTriggered.Invoke(LastHeroAction);
                    }
                }
            }

            if (rhythmEvent.Command.Id == CommandId.Attack)
            {
                Camera.AddTrauma(Rhythm.IsFever ? 0.35f : 0.20f);
            }
            else if (rhythmEvent.Command.Id == CommandId.Charge)
            {
                Camera.AddTrauma(0.15f);
            }

            if (OnCommandEvaluated != null)
            {
                OnCommandEvaluated.Invoke(rhythmEvent);
            }
        }

        private void CheckDefeatedEnemies()
        {
            foreach (var enemy in State.Enemies)
            {
                if (!enemy.IsAlive)
                {
                    EnemyCombatState ai;
                    if (EnemyAIStates.TryGetValue(enemy.Id, out ai) && !ai.HasFled)
                    {
                        // Check if we already processed this kill
                        if (ai.State != EnemyState.Dead)
                        {
                            ai.State = EnemyState.Dead;
                            TotalEnemiesKilled++;

                            var drops = LootSystem.RollDrops(enemy.Kind, _rng);
                            int currencyDrop = LootSystem.RollCurrencyDrop(enemy.Kind, _rng);
                            State.LootCollected.AddRange(drops);
                            State.CurrencyCollected += currencyDrop;
                            CurrencyCollectedOnKills += currencyDrop;

                            if (EnemyMetadata.GetCategory(enemy.Kind) == EnemyCategory.Boss)
                            {
                                TriggerSlowMotion(0.65f);
                                Camera.AddTrauma(0.70f);
                                AddFloatingText("BOSS DEFEATED!", enemy.X, -50f, FloatingTextType.CriticalDamage, 2.0f);
                            }
                            else
                            {
                                AddFloatingText(string.Format("+{0} Gold", currencyDrop), enemy.X, -25f, FloatingTextType.CurrencyGain, 1.2f);
                            }

                            if (OnEnemySlain != null)
                            {
                                OnEnemySlain.Invoke(enemy, drops, currencyDrop);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateCameraTracking(float deltaTime)
        {
            // Center camera between Banner and nearest alive enemy / vanguard unit
            float targetX = State.BannerX + 120f;
            var frontEnemy = State.Enemies.FirstOrDefault(e => e.IsAlive);
            if (frontEnemy != null)
            {
                targetX = (State.BannerX + frontEnemy.X) * 0.5f;
            }

            Camera.FollowTarget(targetX, 0f, deltaTime);
            Camera.UpdateShake(deltaTime);
        }

        public BattleResult FinishBattle()
        {
            var result = new BattleResult
            {
                IsVictory = (State != null && State.Phase == BattlePhase.Victory),
                MissionId = (State != null && State.Mission != null) ? State.Mission.Id : string.Empty,
                TotalCommandsIssued = LastExecutedMeasure + 1,
                MaxCombo = MaxComboAchieved,
                AchievedFever = FeverAchieved,
                EnemiesDefeated = TotalEnemiesKilled,
                BattleDurationSeconds = ElapsedTime
            };

            if (State != null)
            {
                int missionGold = (result.IsVictory && State.Mission != null) ? State.Mission.GoldReward : 0;
                result.GoldEarned = missionGold + State.CurrencyCollected;
                result.LootRewards.AddRange(State.LootCollected);

                // Add mission guaranteed rewards
                if (result.IsVictory && State.Mission != null)
                {
                    foreach (var fixedDrop in State.Mission.LootTable)
                    {
                        result.LootRewards.Add(new LootRewardSummary(fixedDrop, 1));
                    }
                }
            }

            return result;
        }
    }
}
