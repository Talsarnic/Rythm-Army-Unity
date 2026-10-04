using System;
using System.Collections.Generic;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Gameplay.Battle
{
    public enum CombatAnimationCue
    {
        AttackAnticipation,
        AttackRelease,
        AttackImpact,
        AttackRecovery,
        ChargeAnticipation,
        ChargeSurge,
        ChargeRecovery,
        DefendStart,
        DefendHold,
        DefendRecovery,
        JumpTakeoff,
        JumpAirborne,
        JumpLanding,
        JumpRecovery,
        MarchStart,
        FeverPulse,
        HeroAbilityStart,
        HeroAbilityImpact,
        HeroAbilityRecovery,
        VictoryMarch
    }

    public struct UnitAnimationCue
    {
        public LiveUnit Unit;
        public CommandId Command;
        public CombatAnimationCue Cue;
        public float ScheduledTime;
        public int FormationIndex;

        public UnitAnimationCue(LiveUnit unit, CommandId command, CombatAnimationCue cue, float scheduledTime, int formationIndex)
        {
            Unit = unit;
            Command = command;
            Cue = cue;
            ScheduledTime = scheduledTime;
            FormationIndex = formationIndex;
        }
    }

    /// <summary>
    /// Converts rhythm command resolution into beat-synchronized animation cues.
    /// The director is intentionally independent of sprites: the same timing works
    /// with placeholder art, production atlases, or future class-specific sheets.
    /// </summary>
    public class RhythmCombatAnimationDirector
    {
        private class ScheduledCue
        {
            public UnitAnimationCue Cue;
        }

        private readonly List<ScheduledCue> _pending = new List<ScheduledCue>();
        private readonly float _beatLength;

        public event Action<UnitAnimationCue> OnCue;

        public RhythmCombatAnimationDirector(float bpm)
        {
            _beatLength = bpm > 0f ? 60f / bpm : 0.5f;
        }

        public void Clear()
        {
            _pending.Clear();
        }

        public void ScheduleCommand(BattleState state, CommandId command, float startTime, bool fever)
        {
            if (state == null) return;

            _pending.Clear();

            var alive = new List<LiveUnit>();
            for (int i = 0; i < state.Units.Count; i++)
            {
                if (state.Units[i].IsAlive) alive.Add(state.Units[i]);
            }

            for (int i = 0; i < alive.Count; i++)
            {
                var unit = alive[i];
                float stagger = GetStagger(unit, i);
                Schedule(unit, command, CombatAnimationCueForStart(command), startTime + stagger, i);

                switch (command)
                {
                    case CommandId.Attack:
                        Schedule(unit, command, CombatAnimationCue.AttackRelease, startTime + (_beatLength * 0.30f) + stagger, i);
                        Schedule(unit, command, CombatAnimationCue.AttackImpact, startTime + (_beatLength * 0.55f) + stagger, i);
                        Schedule(unit, command, CombatAnimationCue.AttackRecovery, startTime + (_beatLength * 1.20f) + stagger, i);
                        break;

                    case CommandId.Charge:
                        Schedule(unit, command, CombatAnimationCue.ChargeSurge, startTime + (_beatLength * 0.55f) + stagger, i);
                        Schedule(unit, command, CombatAnimationCue.ChargeRecovery, startTime + (_beatLength * 1.80f) + stagger, i);
                        break;

                    case CommandId.Defend:
                        Schedule(unit, command, CombatAnimationCue.DefendHold, startTime + (_beatLength * 0.45f), i);
                        Schedule(unit, command, CombatAnimationCue.DefendRecovery, startTime + (_beatLength * 3.25f), i);
                        break;

                    case CommandId.Jump:
                        Schedule(unit, command, CombatAnimationCue.JumpAirborne, startTime + (_beatLength * 0.40f) + stagger, i);
                        Schedule(unit, command, CombatAnimationCue.JumpLanding, startTime + (_beatLength * 1.20f) + stagger, i);
                        Schedule(unit, command, CombatAnimationCue.JumpRecovery, startTime + (_beatLength * 1.90f) + stagger, i);
                        break;

                    case CommandId.March:
                        Schedule(unit, command, CombatAnimationCue.MarchStart, startTime + (_beatLength * 1.50f) + stagger, i);
                        break;
                }

                if (fever)
                {
                    Schedule(unit, command, CombatAnimationCue.FeverPulse, startTime + (_beatLength * 0.05f) + stagger, i);
                }

                if (unit.Member != null && unit.Member.IsHero && command != CommandId.March)
                {
                    Schedule(unit, command, CombatAnimationCue.HeroAbilityStart, startTime + (_beatLength * 0.10f) + stagger, i);
                    Schedule(unit, command, CombatAnimationCue.HeroAbilityImpact, startTime + (_beatLength * 0.65f) + stagger, i);
                    Schedule(unit, command, CombatAnimationCue.HeroAbilityRecovery, startTime + (_beatLength * 1.80f) + stagger, i);
                }
            }

            _pending.Sort((a, b) => a.Cue.ScheduledTime.CompareTo(b.Cue.ScheduledTime));
        }

        public void Update(float time)
        {
            for (int i = 0; i < _pending.Count;)
            {
                var scheduled = _pending[i];
                if (scheduled.Cue.ScheduledTime > time) break;

                var cue = scheduled.Cue;
                _pending.RemoveAt(i);
                if (cue.Unit != null && cue.Unit.IsAlive && OnCue != null)
                {
                    OnCue.Invoke(cue);
                }
            }
        }

        private void Schedule(LiveUnit unit, CommandId command, CombatAnimationCue cue, float time, int formationIndex)
        {
            _pending.Add(new ScheduledCue
            {
                Cue = new UnitAnimationCue(unit, command, cue, time, formationIndex)
            });
        }

        private static CombatAnimationCue CombatAnimationCueForStart(CommandId command)
        {
            switch (command)
            {
                case CommandId.Attack: return CombatAnimationCue.AttackAnticipation;
                case CommandId.Charge: return CombatAnimationCue.ChargeAnticipation;
                case CommandId.Defend: return CombatAnimationCue.DefendStart;
                case CommandId.Jump: return CombatAnimationCue.JumpTakeoff;
                default: return CombatAnimationCue.MarchStart;
            }
        }

        private static float GetStagger(LiveUnit unit, int index)
        {
            // Keep the squad rhythmically together while preventing every sprite
            // from changing pose on the exact same frame.
            int hash = index * 37;
            if (unit != null && unit.Member != null && !string.IsNullOrEmpty(unit.Member.Id))
            {
                for (int i = 0; i < unit.Member.Id.Length; i++)
                {
                    hash = (hash * 31) + unit.Member.Id[i];
                }
            }

            int bucket = Math.Abs(hash % 4);
            return bucket * 0.025f;
        }
    }
}
