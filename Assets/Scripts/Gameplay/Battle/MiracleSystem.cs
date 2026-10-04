using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay.Battle
{
    public enum MiracleType
    {
        None,
        Rain,
        Tailwind,
        Earthquake,
        Storm
    }

    public class MiracleState
    {
        public MiracleType ActiveMiracle { get; set; }
        public float RemainingDurationSeconds { get; set; }
        public float TotalDurationSeconds { get; set; }
        public float TickTimer { get; set; }
        public int TotalLightningStrikes { get; set; }
        public int TotalEarthquakePulses { get; set; }
        public float WindForceBoost { get; set; }
        public bool IsActive { get { return ActiveMiracle != MiracleType.None && RemainingDurationSeconds > 0f; } }

        public MiracleState()
        {
            ActiveMiracle = MiracleType.None;
            RemainingDurationSeconds = 0f;
            TotalDurationSeconds = 0f;
            TickTimer = 0f;
        }

        public void Clear()
        {
            ActiveMiracle = MiracleType.None;
            RemainingDurationSeconds = 0f;
            TotalDurationSeconds = 0f;
            TickTimer = 0f;
            WindForceBoost = 0f;
        }
    }

    public enum MiracleRitualStage
    {
        Inactive,
        WaitingCall,      // Showing prompt pattern (e.g., TING-TING-TING-TAK)
        PlayerResponse,   // Player drumming response during measure
        Success,          // Ritual complete, Miracle unleashed
        Failed            // Mistimed or broken beat
    }

    public class MiracleRitualState
    {
        public bool IsRitualActive { get; set; }
        public MiracleType TargetMiracle { get; set; }
        public MiracleRitualStage Stage { get; set; }
        public int RequiredRounds { get; set; }
        public int CurrentRound { get; set; }
        public List<DrumId> ExpectedPattern { get; set; }
        public List<DrumId> PlayerInputs { get; set; }
        public float RoundTimer { get; set; }
        public string ChantPrompt { get; set; }

        public MiracleRitualState()
        {
            ExpectedPattern = new List<DrumId>();
            PlayerInputs = new List<DrumId>();
            Stage = MiracleRitualStage.Inactive;
            RequiredRounds = 2;
            CurrentRound = 0;
            ChantPrompt = string.Empty;
        }

        public void Reset()
        {
            IsRitualActive = false;
            TargetMiracle = MiracleType.None;
            Stage = MiracleRitualStage.Inactive;
            CurrentRound = 0;
            ExpectedPattern.Clear();
            PlayerInputs.Clear();
            RoundTimer = 0f;
            ChantPrompt = string.Empty;
        }
    }

    /// <summary>
    /// Handles Divine Miracles (Rain, Tailwind, Earthquake, Storm) invoked during Fever mode.
    /// Includes the interactive Juju Drum Rhythm Ritual sequence (Miracle Dance) with call-and-response beats.
    /// Alters environmental conditions, applies continuous buffs to friendly units, and triggers elemental damage onto enemies.
    /// </summary>
    public class MiracleSystem
    {
        public MiracleState State { get; private set; }
        public MiracleRitualState Ritual { get; private set; }

        public event Action<MiracleType> OnMiracleInvoked;
        public event Action<MiracleType> OnMiracleExpired;
        public event Action<string, float> OnLightningStrike; // enemyId, damage
        public event Action<float> OnEarthquakePulse; // damage to structures
        public event Action<MiracleRitualStage, string> OnRitualStageChanged;

        public MiracleSystem()
        {
            State = new MiracleState();
            Ritual = new MiracleRitualState();
        }

        public void StartMiracleRitual(MiracleType type, int rounds = 2)
        {
            if (type == MiracleType.None) return;

            Ritual.Reset();
            Ritual.IsRitualActive = true;
            Ritual.TargetMiracle = type;
            Ritual.RequiredRounds = Math.Max(1, rounds);
            Ritual.CurrentRound = 1;
            Ritual.Stage = MiracleRitualStage.WaitingCall;

            SetupRitualPatternForRound(Ritual.CurrentRound);

            if (OnRitualStageChanged != null)
            {
                OnRitualStageChanged.Invoke(Ritual.Stage, Ritual.ChantPrompt);
            }
        }

        private void SetupRitualPatternForRound(int round)
        {
            Ritual.ExpectedPattern.Clear();
            Ritual.PlayerInputs.Clear();

            switch (Ritual.TargetMiracle)
            {
                case MiracleType.Rain:
                    // Round 1: Ting Ting Ting Tak -> Round 2: Boom Ting Boom Ting
                    if (round == 1)
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak });
                        Ritual.ChantPrompt = "TING TING TING TAK";
                    }
                    else
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Boom, DrumId.Ting, DrumId.Boom, DrumId.Ting });
                        Ritual.ChantPrompt = "BOOM TING BOOM TING";
                    }
                    break;

                case MiracleType.Tailwind:
                    // Round 1: Ting Ting Ting Tak -> Round 2: Rat Rat Boom Tak
                    if (round == 1)
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak });
                        Ritual.ChantPrompt = "TING TING TING TAK";
                    }
                    else
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Rat, DrumId.Rat, DrumId.Boom, DrumId.Tak });
                        Ritual.ChantPrompt = "RAT RAT BOOM TAK";
                    }
                    break;

                case MiracleType.Earthquake:
                    // Round 1: Ting Ting Ting Tak -> Round 2: Tak Tak Rat Rat
                    if (round == 1)
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak });
                        Ritual.ChantPrompt = "TING TING TING TAK";
                    }
                    else
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Tak, DrumId.Tak, DrumId.Rat, DrumId.Rat });
                        Ritual.ChantPrompt = "TAK TAK RAT RAT";
                    }
                    break;

                case MiracleType.Storm:
                default:
                    // Round 1: Ting Ting Ting Tak -> Round 2: Ting Boom Ting Tak
                    if (round == 1)
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak });
                        Ritual.ChantPrompt = "TING TING TING TAK";
                    }
                    else
                    {
                        Ritual.ExpectedPattern.AddRange(new[] { DrumId.Ting, DrumId.Boom, DrumId.Ting, DrumId.Tak });
                        Ritual.ChantPrompt = "TING BOOM TING TAK";
                    }
                    break;
            }
        }

        public bool RecordRitualDrumTap(DrumId drum)
        {
            if (!Ritual.IsRitualActive) return false;

            Ritual.PlayerInputs.Add(drum);
            int idx = Ritual.PlayerInputs.Count - 1;

            if (idx >= Ritual.ExpectedPattern.Count)
            {
                // Too many inputs
                FailRitual("Excess beat inputs");
                return false;
            }

            if (Ritual.ExpectedPattern[idx] != drum)
            {
                // Wrong drum tapped
                FailRitual("Missed ritual beat");
                return false;
            }

            // If measure complete
            if (Ritual.PlayerInputs.Count == Ritual.ExpectedPattern.Count)
            {
                if (Ritual.CurrentRound >= Ritual.RequiredRounds)
                {
                    // Full ritual success!
                    CompleteRitualSuccess();
                }
                else
                {
                    // Move to next round
                    Ritual.CurrentRound++;
                    SetupRitualPatternForRound(Ritual.CurrentRound);
                    if (OnRitualStageChanged != null)
                    {
                        OnRitualStageChanged.Invoke(MiracleRitualStage.WaitingCall, Ritual.ChantPrompt);
                    }
                }
            }

            return true;
        }

        private void CompleteRitualSuccess()
        {
            var target = Ritual.TargetMiracle;
            Ritual.Stage = MiracleRitualStage.Success;
            Ritual.IsRitualActive = false;

            InvokeMiracle(target, 16.0f); // Longer duration when invoked via full ritual

            if (OnRitualStageChanged != null)
            {
                OnRitualStageChanged.Invoke(MiracleRitualStage.Success, "DIVINE MIRACLE UNLEASHED!");
            }
        }

        public void FailRitual(string reason)
        {
            Ritual.Stage = MiracleRitualStage.Failed;
            Ritual.IsRitualActive = false;

            if (OnRitualStageChanged != null)
            {
                OnRitualStageChanged.Invoke(MiracleRitualStage.Failed, reason);
            }
        }

        public void InvokeMiracle(MiracleType type, float duration = 12.0f)
        {
            if (type == MiracleType.None) return;

            State.ActiveMiracle = type;
            State.TotalDurationSeconds = duration;
            State.RemainingDurationSeconds = duration;
            State.TickTimer = 0f;
            State.TotalLightningStrikes = 0;
            State.TotalEarthquakePulses = 0;

            if (type == MiracleType.Tailwind)
            {
                State.WindForceBoost = 120f;
            }
            else
            {
                State.WindForceBoost = 0f;
            }

            if (OnMiracleInvoked != null)
            {
                OnMiracleInvoked.Invoke(type);
            }
        }

        public void Update(float deltaTime, BattleState battleState, PixelCamera camera = null, Random rng = null)
        {
            if (!State.IsActive) return;

            var r = rng ?? new Random();
            State.RemainingDurationSeconds -= deltaTime;
            State.TickTimer += deltaTime;

            // Tick effects every 1.0 second (approximately 1 measure at 120 BPM)
            if (State.TickTimer >= 1.0f)
            {
                State.TickTimer -= 1.0f;
                ApplyPeriodicMiracleEffects(battleState, camera, r);
            }

            if (State.RemainingDurationSeconds <= 0f)
            {
                var expired = State.ActiveMiracle;
                State.Clear();
                if (OnMiracleExpired != null)
                {
                    OnMiracleExpired.Invoke(expired);
                }
            }
        }

        private void ApplyPeriodicMiracleEffects(BattleState battleState, PixelCamera camera, Random rng)
        {
            if (battleState == null) return;

            switch (State.ActiveMiracle)
            {
                case MiracleType.Rain:
                    // Douse burning status and heal friendly squad
                    foreach (var unit in battleState.Units)
                    {
                        if (unit.IsAlive)
                        {
                            unit.CurrentHp = Math.Min(unit.MaxHp, unit.CurrentHp + 8f);
                            unit.IsBurning = false;
                        }
                    }
                    break;

                case MiracleType.Tailwind:
                    // Friendly projectile speed & push force bonus already active
                    break;

                case MiracleType.Earthquake:
                    // Quake damage to structures and ground forces
                    State.TotalEarthquakePulses++;
                    if (camera != null)
                    {
                        camera.AddTrauma(0.35f);
                    }

                    float quakeDamage = 25f;
                    foreach (var enemy in battleState.Enemies)
                    {
                        if (enemy.IsAlive)
                        {
                            var cat = EnemyMetadata.GetCategory(enemy.Kind);
                            float dmg = cat == EnemyCategory.Fortification ? quakeDamage * 3f : quakeDamage;
                            enemy.CurrentHp = Math.Max(0f, enemy.CurrentHp - dmg);
                        }
                    }

                    if (OnEarthquakePulse != null)
                    {
                        OnEarthquakePulse.Invoke(quakeDamage);
                    }
                    break;

                case MiracleType.Storm:
                    // Lightning bolt strikes a random alive enemy
                    var aliveEnemies = battleState.Enemies.Where(e => e.IsAlive).ToList();
                    if (aliveEnemies.Count > 0)
                    {
                        var target = aliveEnemies[rng.Next(aliveEnemies.Count)];
                        float lightningDmg = rng.Next(50, 95);
                        target.CurrentHp = Math.Max(0f, target.CurrentHp - lightningDmg);

                        State.TotalLightningStrikes++;
                        if (camera != null)
                        {
                            camera.AddTrauma(0.40f);
                        }

                        if (OnLightningStrike != null)
                        {
                            OnLightningStrike.Invoke(target.Id, lightningDmg);
                        }
                    }
                    break;
            }
        }

        public static MiracleType GetMiracleFromRelicId(string relicId)
        {
            if (string.IsNullOrEmpty(relicId)) return MiracleType.None;

            switch (relicId)
            {
                case "relic-rain-charm": return MiracleType.Rain;
                case "relic-wind-charm": return MiracleType.Tailwind;
                case "relic-earthquake-charm": return MiracleType.Earthquake;
                case "relic-storm-charm": return MiracleType.Storm;
                default: return MiracleType.None;
            }
        }
    }
}
