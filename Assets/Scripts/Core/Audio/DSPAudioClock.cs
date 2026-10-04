using System;

namespace RhythmArmy.Core.Audio
{
    public class DSPAudioClock
    {
        public double DspStartTime { get; private set; }
        public float Bpm { get; private set; }
        public double SecondsPerBeat
        {
            get { return 60.0 / Bpm; }
        }

        public bool IsRunning { get; private set; }

        public DSPAudioClock(float bpm = 120f)
        {
            Bpm = bpm > 0 ? bpm : 120f;
        }

        public void Start(double currentDspTime)
        {
            DspStartTime = currentDspTime;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public double GetSongPosition(double currentDspTime)
        {
            if (!IsRunning) return 0.0;
            return currentDspTime - DspStartTime;
        }

        public double GetCurrentBeat(double currentDspTime)
        {
            if (!IsRunning) return 0.0;
            return (currentDspTime - DspStartTime) / SecondsPerBeat;
        }

        public double GetNextBeatDspTime(double currentDspTime)
        {
            double currentBeat = GetCurrentBeat(currentDspTime);
            double nextBeat = Math.Ceiling(currentBeat);
            return DspStartTime + (nextBeat * SecondsPerBeat);
        }
    }
}
