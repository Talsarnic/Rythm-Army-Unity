using System;
using System.Collections.Generic;

namespace RhythmArmy.Core.Rhythm
{
    [Serializable]
    public class CommandDef
    {
        public CommandId Id { get; set; }
        public string Name { get; set; }
        public DrumId[] Pattern { get; set; }
        public DrumId[] DrumSequence
        {
            get { return Pattern; }
            set { Pattern = value; }
        }
        public string Hint { get; set; }
        public string Chant { get; set; }

        public CommandDef(CommandId id, string name, DrumId[] pattern, string hint, string chant)
        {
            Id = id;
            Name = name;
            Pattern = pattern;
            Hint = hint;
            Chant = chant;
        }

        public static readonly List<CommandDef> StandardCommands = new List<CommandDef>
        {
            new CommandDef(
                CommandId.March,
                "March",
                new[] { DrumId.Boom, DrumId.Boom, DrumId.Boom, DrumId.Tak },
                "BOOM BOOM BOOM TAK",
                "BOOM BOOM BOOM TAK"
            ),
            new CommandDef(
                CommandId.Attack,
                "Attack",
                new[] { DrumId.Tak, DrumId.Tak, DrumId.Boom, DrumId.Tak },
                "TAK TAK BOOM TAK",
                "TAK TAK BOOM TAK"
            ),
            new CommandDef(
                CommandId.Defend,
                "Defend",
                new[] { DrumId.Rat, DrumId.Rat, DrumId.Boom, DrumId.Tak },
                "RAT RAT BOOM TAK",
                "RAT RAT BOOM TAK"
            ),
            new CommandDef(
                CommandId.Retreat,
                "Retreat",
                new[] { DrumId.Tak, DrumId.Boom, DrumId.Tak, DrumId.Boom },
                "TAK BOOM TAK BOOM",
                "TAK BOOM TAK BOOM"
            ),
            new CommandDef(
                CommandId.Charge,
                "Charge",
                new[] { DrumId.Tak, DrumId.Tak, DrumId.Rat, DrumId.Rat },
                "TAK TAK RAT RAT",
                "TAK TAK RAT RAT"
            ),
            new CommandDef(
                CommandId.Jump,
                "Jump",
                new[] { DrumId.Ting, DrumId.Ting, DrumId.Boom, DrumId.Tak },
                "TING TING BOOM TAK",
                "TING TING BOOM TAK"
            ),
            new CommandDef(
                CommandId.Miracle,
                "Miracle",
                new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak },
                "TING TING TING TAK",
                "TING TING TING TAK"
            ),
        };

        public static CommandDef Match(DrumId[] sequence)
        {
            if (sequence == null || sequence.Length != 4) return null;
            foreach (var cmd in StandardCommands)
            {
                if (cmd.Pattern.Length == 4 &&
                    cmd.Pattern[0] == sequence[0] &&
                    cmd.Pattern[1] == sequence[1] &&
                    cmd.Pattern[2] == sequence[2] &&
                    cmd.Pattern[3] == sequence[3])
                {
                    return cmd;
                }
            }
            return null;
        }
    }

    public struct Judgement
    {
        public DrumId Drum;
        public int Beat;
        public float DeltaMs;
        public Grade Grade;
        public int Measure;
        public int Slot;
        public bool Ignored;
    }

    public enum RhythmEventType
    {
        Beat,
        Command,
        Fail
    }

    public class RhythmEvent
    {
        public RhythmEventType Type { get; set; }
        public int Beat { get; set; }
        public int Measure { get; set; }
        public int Slot { get; set; }
        public string Phase { get; set; } // "input" or "response"
        
        // Command specific
        public CommandDef Command { get; set; }
        public int Perfects { get; set; }
        public bool Fever { get; set; }

        // Fail specific
        public string FailReason { get; set; } // "incomplete" or "unknown"
    }
}
