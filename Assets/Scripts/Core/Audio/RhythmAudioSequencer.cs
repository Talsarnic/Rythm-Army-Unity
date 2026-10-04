using System;
using System.Collections.Generic;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Core.Audio
{
    public enum AudioPlaybackType
    {
        DrumHit,
        VocalChant,
        BgmStem
    }

    public class QueuedAudioClip
    {
        public AudioPlaybackType Type;
        public string ClipName;
        public float Volume;
        public float Pitch;
        public float ScheduledDspTime;

        public QueuedAudioClip(AudioPlaybackType type, string clipName, float volume, float pitch, float scheduledDspTime)
        {
            Type = type;
            ClipName = clipName;
            Volume = volume;
            Pitch = pitch;
            ScheduledDspTime = scheduledDspTime;
        }
    }

    public class RhythmAudioSequencer
    {
        public float MasterVolume = 1.0f;
        public float DrumVolume = 1.0f;
        public float ChantVolume = 0.9f;
        public float BgmVolume = 0.8f;
        public float BeatDuration = 0.5f;
        private bool _isFever = false;

        // Fever dynamic stem crossfade (0.0 = base theme, 1.0 = fever layer max)
        public float FeverStemGain { get; private set; }
        public float BaseStemGain { get; private set; }

        public List<QueuedAudioClip> PlaybackLog = new List<QueuedAudioClip>();

        public void SetBPM(float bpm)
        {
            BeatDuration = 60f / bpm;
        }

        public void SetFeverActive(bool fever)
        {
            _isFever = fever;
        }

        public void PlayDrum(DrumId drum, bool perfect = true)
        {
            TriggerDrumHit(drum, 0f, perfect ? Grade.Perfect : Grade.Good);
        }

        public void Update(float deltaTime)
        {
            UpdateFeverMusicCrossfade(_isFever, deltaTime);
        }

        public void QueueResponseChants(DrumId[] sequence, float currentTime)
        {
            if (sequence == null) return;
            var cmd = CommandDef.Match(sequence);
            if (cmd != null)
            {
                QueueChantSequence(cmd, currentTime, BeatDuration, _isFever);
            }
        }

        public void TriggerDrumHit(DrumId drum, float dspTime, Grade grade = Grade.Perfect)
        {
            string clipName;
            float pitch = 1.0f;

            switch (drum)
            {
                case DrumId.Boom: clipName = "drum_boom"; break;
                case DrumId.Tak: clipName = "drum_tak"; break;
                case DrumId.Rat: clipName = "drum_rat"; break;
                case DrumId.Ting: clipName = "drum_ting"; break;
                default: clipName = "drum_boom"; break;
            }

            if (grade == Grade.Good)
            {
                pitch = 0.95f; // Slight pitch variance for non-perfect
            }

            var clip = new QueuedAudioClip(AudioPlaybackType.DrumHit, clipName, DrumVolume * MasterVolume, pitch, dspTime);
            PlaybackLog.Add(clip);
        }

        public void QueueChantSequence(CommandDef command, float measureStartTime, float beatDuration, bool fever = false)
        {
            if (command == null || command.Pattern == null) return;

            string[] syllables = command.Chant.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Chant plays on the 4 response beats (indices 4, 5, 6, 7 in the 8-beat measure)
            for (int i = 0; i < 4 && i < syllables.Length; i++)
            {
                float chantTime = measureStartTime + ((4 + i) * beatDuration);
                string clipName = "chant_" + syllables[i].ToLowerInvariant();
                float pitch = fever ? 1.08f : 1.0f; // Higher excitement in Fever mode

                var clip = new QueuedAudioClip(AudioPlaybackType.VocalChant, clipName, ChantVolume * MasterVolume, pitch, chantTime);
                PlaybackLog.Add(clip);
            }
        }

        public void UpdateFeverMusicCrossfade(bool isFever, float deltaTime, float fadeSpeed = 3.0f)
        {
            float targetFever = isFever ? 1.0f : 0.0f;
            FeverStemGain += (targetFever - FeverStemGain) * Math.Min(1f, fadeSpeed * deltaTime);
            BaseStemGain = 1.0f - (FeverStemGain * 0.3f); // Base stays prominent with slight dip for fever leads
        }
    }
}
