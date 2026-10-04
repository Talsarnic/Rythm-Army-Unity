using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmArmy.Core.Rhythm
{
    public enum MeasurePhase
    {
        Input,
        Response
    }

    public struct TapJudgment
    {
        public Grade Grade;
        public float DeltaMs;
        public int Beat;
        public DrumId Drum;

        public TapJudgment(Grade grade, float deltaMs, int beat, DrumId drum)
        {
            Grade = grade;
            DeltaMs = deltaMs;
            Beat = beat;
            Drum = drum;
        }

        public static implicit operator TapJudgment(Judgement j)
        {
            return new TapJudgment(j.Grade, j.DeltaMs, j.Beat, j.Drum);
        }

        public static implicit operator Judgement(TapJudgment t)
        {
            Judgement j;
            j.Grade = t.Grade;
            j.DeltaMs = t.DeltaMs;
            j.Beat = t.Beat;
            j.Drum = t.Drum;
            j.Measure = 0;
            j.Slot = 0;
            j.Ignored = false;
            return j;
        }
    }

    public class RhythmEngine
    {
        public const int FEVER_COMBO_THRESHOLD = RhythmConstants.FeverComboThreshold;
        public float Bpm { get; private set; }
        public float? StartTime { get; private set; }
        public float InputOffsetMs { get; set; }
        public float PerfectMs { get; private set; }
        public float GoodMs { get; private set; }
        public List<CommandDef> Commands { get; private set; }

        public float BeatLength
        {
            get { return 60f / Bpm; }
        }

        public float BeatInterval
        {
            get { return BeatLength; }
        }

        public bool Started
        {
            get { return StartTime.HasValue; }
        }

        public bool Fever
        {
            get { return Combo >= RhythmConstants.FeverComboThreshold; }
        }

        public bool IsFever
        {
            get { return Fever; }
        }

        public MeasurePhase Phase
        {
            get
            {
                int slot = CurrentBeatInMeasure;
                return slot < RhythmConstants.InputBeats ? MeasurePhase.Input : MeasurePhase.Response;
            }
        }

        public int CurrentBeatInMeasure
        {
            get
            {
                if (!Started || !StartTime.HasValue) return 0;
                return Math.Max(0, (_nextBeat - 1) % RhythmConstants.MeasureBeats);
            }
        }

        public float TimeSinceBeat(float time)
        {
            if (!Started || !StartTime.HasValue) return 0f;
            float pos = (time - StartTime.Value) / BeatLength;
            return (pos - (float)Math.Floor(pos)) * BeatLength;
        }

        public float BeatProgress(float time)
        {
            if (!Started || !StartTime.HasValue || BeatLength <= 0f) return 0f;
            float pos = (time - StartTime.Value) / BeatLength;
            return pos - (float)Math.Floor(pos);
        }

        public List<RhythmEvent> AdvanceBeats(float time)
        {
            return Advance(time);
        }

        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int CommandCount { get; private set; }
        public int FailCount { get; private set; }

        private readonly Dictionary<int, Judgement?[]> _slots = new Dictionary<int, Judgement?[]>();
        private int _nextBeat;
        private int _nextMeasure;

        public RhythmEngine(
            float bpm = RhythmConstants.DefaultBpm,
            float? startTime = null,
            float inputOffsetMs = 0f,
            float perfectMs = RhythmConstants.DefaultPerfectMs,
            float goodMs = RhythmConstants.DefaultGoodMs,
            List<CommandDef> commands = null)
        {
            Bpm = bpm > 0 ? bpm : RhythmConstants.DefaultBpm;
            StartTime = startTime;
            InputOffsetMs = inputOffsetMs;
            Commands = commands != null ? commands : CommandDef.StandardCommands;

            float cap = (60f / Bpm) * 1000f * RhythmConstants.MaxGoodBeatShare;
            GoodMs = Math.Min(goodMs, cap);
            PerfectMs = Math.Min(perfectMs, GoodMs);
        }

        public void Start(float time)
        {
            float adjusted = time - (InputOffsetMs / 1000f);
            StartTime = adjusted;
            _slots.Clear();
            _nextBeat = 0;
            _nextMeasure = 0;
        }

        public void Reset()
        {
            StartTime = null;
            _slots.Clear();
            Combo = 0;
            _nextBeat = 0;
            _nextMeasure = 0;
        }

        public float BeatTime(int n)
        {
            if (!StartTime.HasValue) return float.PositiveInfinity;
            return StartTime.Value + (n * BeatLength);
        }

        public float BeatPosition(float time)
        {
            if (!StartTime.HasValue) return -1f;
            return (time - StartTime.Value) / BeatLength;
        }

        public Judgement JudgeTap(DrumId drum, float time)
        {
            if (!Started || !StartTime.HasValue)
            {
                Judgement initial;
                initial.Drum = drum;
                initial.Beat = 0;
                initial.DeltaMs = 0;
                initial.Grade = Grade.Perfect;
                initial.Measure = 0;
                initial.Slot = 0;
                initial.Ignored = false;
                return initial;
            }

            float adjusted = time - (InputOffsetMs / 1000f);
            int beat = (int)Math.Round((adjusted - StartTime.Value) / BeatLength);
            float deltaMs = (adjusted - BeatTime(beat)) * 1000f;
            float abs = Math.Abs(deltaMs);

            Grade grade = abs <= PerfectMs ? Grade.Perfect : (abs <= GoodMs ? Grade.Good : Grade.Miss);
            Judgement result;
            result.Drum = drum;
            result.Beat = beat;
            result.DeltaMs = deltaMs;
            result.Grade = grade;
            result.Measure = 0;
            result.Slot = 0;
            result.Ignored = false;
            return result;
        }

        public Judgement Tap(DrumId drum, float time)
        {
            if (!Started || !StartTime.HasValue)
            {
                Start(time);
                var initialRow = new Judgement?[RhythmConstants.InputBeats];
                Judgement initialJudgement;
                initialJudgement.Drum = drum;
                initialJudgement.Beat = 0;
                initialJudgement.DeltaMs = 0;
                initialJudgement.Grade = Grade.Perfect;
                initialJudgement.Measure = 0;
                initialJudgement.Slot = 0;
                initialJudgement.Ignored = false;

                initialRow[0] = initialJudgement;
                _slots[0] = initialRow;
                return initialJudgement;
            }

            var j = JudgeTap(drum, time);
            if (j.Beat < 0)
            {
                j.Measure = -1;
                j.Slot = -1;
                j.Ignored = true;
                return j;
            }

            int measure = j.Beat / RhythmConstants.MeasureBeats;
            int slot = j.Beat % RhythmConstants.MeasureBeats;

            if (slot >= RhythmConstants.InputBeats)
            {
                // Army response phase: taps ignored
                j.Measure = measure;
                j.Slot = slot;
                j.Ignored = true;
                return j;
            }

            if (j.Grade == Grade.Miss)
            {
                // The song clock never stops on a bad tap. A miss breaks the
                // current phrase, but the next phrase still begins on the
                // next musical measure exactly as it would in a live performance.
                j.Measure = measure;
                j.Slot = slot;
                j.Ignored = false;
                return j;
            }

            Judgement?[] row;
            if (!_slots.TryGetValue(measure, out row))
            {
                row = new Judgement?[RhythmConstants.InputBeats];
                _slots[measure] = row;
            }

            var existing = row[slot];
            if (!existing.HasValue || Math.Abs(j.DeltaMs) < Math.Abs(existing.Value.DeltaMs))
            {
                j.Measure = measure;
                j.Slot = slot;
                j.Ignored = false;
                row[slot] = j;
            }
            else
            {
                j.Measure = measure;
                j.Slot = slot;
                j.Ignored = false;
            }

            return j;
        }

        public List<RhythmEvent> Advance(float time)
        {
            if (!Started || !StartTime.HasValue) return new List<RhythmEvent>();

            var events = new List<RhythmEvent>();

            while (Started && BeatTime(_nextBeat) <= time)
            {
                int beat = _nextBeat++;
                int slot = beat % RhythmConstants.MeasureBeats;
                events.Add(new RhythmEvent
                {
                    Type = RhythmEventType.Beat,
                    Beat = beat,
                    Measure = beat / RhythmConstants.MeasureBeats,
                    Slot = slot,
                    Phase = slot < RhythmConstants.InputBeats ? "input" : "response"
                });
            }

            float grace = (GoodMs / 1000f) + Math.Max(0f, InputOffsetMs / 1000f);

            while (Started)
            {
                int m = _nextMeasure;
                float decideAt = BeatTime(m * RhythmConstants.MeasureBeats + RhythmConstants.InputBeats);
                if (time < decideAt) break;

                Judgement?[] row;
                bool rowExists = _slots.TryGetValue(m, out row);
                bool full = rowExists && row != null && row.All(s => s.HasValue);

                if (!full && time < decideAt + grace) break;

                _nextMeasure++;
                var ev = Evaluate(m);
                if (ev != null)
                {
                    events.Add(ev);
                }
            }

            return events;
        }

        public void ForceFever()
        {
            Combo = Math.Max(Combo, RhythmConstants.FeverComboThreshold);
            BestCombo = Math.Max(BestCombo, Combo);
        }

        public void AddCombo(int amount)
        {
            if (amount <= 0) return;
            Combo += amount;
            BestCombo = Math.Max(BestCombo, Combo);
        }

        public RhythmEvent Evaluate(int measure)
        {
            Judgement?[] row;
            _slots.TryGetValue(measure, out row);
            _slots.Remove(measure);

            int beat = measure * RhythmConstants.MeasureBeats + RhythmConstants.InputBeats;

            if (row == null || row.Any(s => !s.HasValue))
            {
                Combo = 0;
                FailCount++;
                _slots.Remove(measure);
                return new RhythmEvent
                {
                    Type = RhythmEventType.Fail,
                    FailReason = "incomplete",
                    Measure = measure,
                    Beat = beat
                };
            }

            var drums = row.Select(s => s.Value.Drum).ToArray();
            var command = Commands.FirstOrDefault(c =>
                c.Pattern.Length == drums.Length &&
                c.Pattern.SequenceEqual(drums));

            if (command == null)
            {
                Combo = 0;
                FailCount++;
                _slots.Remove(measure);
                return new RhythmEvent
                {
                    Type = RhythmEventType.Fail,
                    FailReason = "unknown",
                    Measure = measure,
                    Beat = beat
                };
            }

            int perfects = row.Count(s => s.Value.Grade == Grade.Perfect);
            Combo++;
            BestCombo = Math.Max(BestCombo, Combo);
            CommandCount++;

            return new RhythmEvent
            {
                Type = RhythmEventType.Command,
                Command = command,
                Measure = measure,
                Beat = beat,
                Perfects = perfects,
                Fever = Fever
            };
        }

        public static int? ComputeOffset(List<float> deltasMs)
        {
            var usable = deltasMs.Where(d => Math.Abs(d) <= 250f).OrderBy(d => d).ToList();
            if (usable.Count < 4) return null;

            int mid = usable.Count / 2;
            float median = (usable.Count % 2 != 0) ? usable[mid] : (usable[mid - 1] + usable[mid]) / 2f;
            return (int)Math.Round(median);
        }
    }
}
