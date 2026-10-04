using System;

namespace RhythmArmy.Core.Rhythm
{
    public enum DrumId
    {
        Boom = 0, // Key A / Key J
        Tak = 1,  // Key S / Key K
        Rat = 2,  // Key D / Key L
        Ting = 3  // Key F / Key ;
    }

    public enum Grade
    {
        Perfect,
        Good,
        Miss
    }

    public enum CommandId
    {
        March,
        Attack,
        Defend,
        Retreat,
        Charge,
        Jump,
        Miracle
    }

    public static class RhythmConstants
    {
        public const int MeasureBeats = 8;
        public const int InputBeats = 4;
        public const int ResponseBeats = 4;
        public const int FeverComboThreshold = 10;
        public const float DefaultBpm = 120f;
        public const float DefaultPerfectMs = 120f;
        public const float DefaultGoodMs = 220f;
        public const float MaxGoodBeatShare = 0.45f;
    }
}
