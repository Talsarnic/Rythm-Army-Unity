using System;
using System.IO;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    /// <summary>
    /// Procedural Audio Synthesizer: Generates 16-bit PCM WAV audio files (44.1 kHz)
    /// for drum beats (Boom, Tak, Rat, Ting), vocal chants, combat & UI SFX, and Fever BGM stems.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        public const int SampleRate = 44100;

        /// <summary>
        /// Generates a complete 16-bit PCM WAV byte array with standard RIFF header from float audio samples (-1.0 to 1.0).
        /// </summary>
        public static byte[] EncodeWav(float[] samples, int sampleRate = SampleRate, int channels = 1)
        {
            if (samples == null) samples = new float[0];

            int byteRate = sampleRate * channels * 2; // 16-bit = 2 bytes per sample
            int blockAlign = channels * 2;
            int dataChunkSize = samples.Length * 2;
            int fileSize = 36 + dataChunkSize;

            using (var memoryStream = new MemoryStream(fileSize + 8))
            using (var writer = new BinaryWriter(memoryStream))
            {
                // RIFF Header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(fileSize);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // subchunk1 size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 = PCM)
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)16); // BitsPerSample

                // data chunk
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(dataChunkSize);

                for (int i = 0; i < samples.Length; i++)
                {
                    float clamped = Math.Max(-1.0f, Math.Min(1.0f, samples[i]));
                    short pcmSample = (short)(clamped * 32767.0f);
                    writer.Write(pcmSample);
                }

                return memoryStream.ToArray();
            }
        }

        #region Drum Synthesizers

        public static byte[] GenerateDrumBoom(float duration = 0.45f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                // Frequency drop from 140 Hz down to 48 Hz
                float freq = 48.0f + 92.0f * (float)Math.Exp(-progress * 9.0f);
                float phase = 2.0f * (float)Math.PI * freq * t;

                // Punchy sine + subtle sub-bass 2nd harmonic
                float signal = (float)Math.Sin(phase) * 0.85f + (float)Math.Sin(phase * 0.5f) * 0.25f;

                // Punch transient at start (first 15ms)
                if (t < 0.015f)
                {
                    signal += (float)(Math.Sin(2.0 * Math.PI * 300.0 * t) * (1.0 - t / 0.015f) * 0.35f);
                }

                // Envelope: instant attack, exponential decay
                float env = (float)Math.Exp(-progress * 6.5f);
                samples[i] = signal * env * 0.95f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateDrumTak(float duration = 0.28f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(1337);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                // Snare/Woodblock: tonal body at 320 Hz + white noise burst
                float tonal = (float)Math.Sin(2.0 * Math.PI * (320.0 - progress * 120.0) * t);
                float noise = ((float)random.NextDouble() * 2.0f - 1.0f);

                // Blend: noise dominates initially, tonal resonates briefly
                float signal = tonal * 0.45f + noise * 0.55f;

                float env = (float)Math.Exp(-progress * 14.0f);
                samples[i] = signal * env * 0.90f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateDrumRat(float duration = 0.22f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(4242);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                // High frequency rimshot click (~1200 Hz) + sharp high-passed rattle
                float click = (float)Math.Sin(2.0 * Math.PI * 1250.0 * t);
                float rattle = ((float)random.NextDouble() * 2.0f - 1.0f);

                // Two fast micro-impulses (flam / roll feel)
                float flamMultiplier = (t > 0.025f && t < 0.06f) ? 1.4f : 1.0f;

                float signal = (click * 0.4f + rattle * 0.6f) * flamMultiplier;
                float env = (float)Math.Exp(-progress * 18.0f);
                samples[i] = signal * env * 0.88f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateDrumTing(float duration = 0.65f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                // Metallic chime: 3 high harmonics (1760 Hz, 3520 Hz, 5280 Hz)
                float h1 = (float)Math.Sin(2.0 * Math.PI * 1760.0 * t);
                float h2 = (float)Math.Sin(2.0 * Math.PI * 3520.0 * t) * 0.45f;
                float h3 = (float)Math.Sin(2.0 * Math.PI * 5280.0 * t) * 0.20f;

                // Shimmer modulation
                float tremolo = 1.0f + 0.15f * (float)Math.Sin(2.0 * Math.PI * 18.0 * t);

                float signal = (h1 + h2 + h3) * tremolo;
                float env = (float)Math.Exp(-progress * 4.5f);
                samples[i] = signal * env * 0.85f;
            }

            return EncodeWav(samples);
        }

        #endregion

        #region Vocal Chant Synthesizers

        public static byte[] GenerateVocalChant(string syllable, float duration = 0.35f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float basePitch = 180.0f; // Fundamental male/tribal choir pitch (F3)
            float formant1 = 600.0f;  // Vowel formant frequencies
            float formant2 = 1200.0f;

            string s = syllable.ToLowerInvariant();
            if (s.Contains("boom")) { basePitch = 160f; formant1 = 450f; formant2 = 900f; }
            else if (s.Contains("tak")) { basePitch = 195f; formant1 = 750f; formant2 = 1400f; }
            else if (s.Contains("rat")) { basePitch = 210f; formant1 = 650f; formant2 = 1700f; }
            else if (s.Contains("ting")) { basePitch = 240f; formant1 = 500f; formant2 = 2200f; }
            else if (s.Contains("fever")) { basePitch = 260f; formant1 = 680f; formant2 = 1900f; }

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                // Sawtooth-like glottal pulse
                float glottal = (t * basePitch) - (float)Math.Floor(t * basePitch + 0.5f);

                // Formant resonance simulation
                float res1 = (float)Math.Sin(2.0 * Math.PI * formant1 * t) * 0.5f;
                float res2 = (float)Math.Sin(2.0 * Math.PI * formant2 * t) * 0.35f;

                float signal = glottal * 0.4f + res1 + res2;

                // Attack and release vocal envelope
                float attack = Math.Min(1.0f, t / 0.03f);
                float decay = (float)Math.Exp(-progress * 5.0f);

                samples[i] = signal * attack * decay * 0.80f;
            }

            return EncodeWav(samples);
        }

        #endregion

        #region SFX & Fanfare Synthesizers

        public static byte[] GenerateSwordSlash(float duration = 0.25f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(55);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float noise = ((float)random.NextDouble() * 2.0f - 1.0f);
                float sweepFreq = 2400.0f - progress * 1600.0f;
                float whistle = (float)Math.Sin(2.0 * Math.PI * sweepFreq * t);

                float env = (float)Math.Sin(progress * Math.PI) * (float)Math.Exp(-progress * 4.0f);
                samples[i] = (noise * 0.6f + whistle * 0.4f) * env * 0.85f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateArrowWhistle(float duration = 0.30f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float freq = 900.0f + 1400.0f * (float)Math.Sin(progress * Math.PI * 0.8f);
                float signal = (float)Math.Sin(2.0 * Math.PI * freq * t);

                float env = (float)Math.Sin(progress * Math.PI);
                samples[i] = signal * env * 0.75f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateShieldBlock(float duration = 0.28f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(777);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float metallic = (float)Math.Sin(2.0 * Math.PI * 880.0 * t) + (float)Math.Sin(2.0 * Math.PI * 1760.0 * t) * 0.5f;
                float thud = (float)Math.Sin(2.0 * Math.PI * 120.0 * t);
                float noise = ((float)random.NextDouble() * 2.0f - 1.0f);

                float env = (float)Math.Exp(-progress * 12.0f);
                samples[i] = (metallic * 0.4f + thud * 0.4f + noise * 0.2f) * env * 0.90f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateBossRoar(float duration = 0.85f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(999);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float sub = (float)Math.Sin(2.0 * Math.PI * (65.0 + 20.0 * Math.Sin(14.0 * t)) * t);
                float growl = (float)Math.Sin(2.0 * Math.PI * 130.0 * t) * ((float)random.NextDouble() * 0.8f + 0.6f);
                float rumble = ((float)random.NextDouble() * 2.0f - 1.0f) * 0.35f;

                float env = (float)Math.Sin(progress * Math.PI * 0.9f) * (float)Math.Exp(-progress * 1.5f);
                samples[i] = (sub * 0.45f + growl * 0.35f + rumble * 0.20f) * env * 0.95f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateCoinClink(float duration = 0.25f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float c1 = (float)Math.Sin(2.0 * Math.PI * 2400.0 * t);
                float c2 = (float)Math.Sin(2.0 * Math.PI * 4800.0 * t) * 0.4f;

                float env = (float)Math.Exp(-progress * 16.0f);
                samples[i] = (c1 + c2) * env * 0.80f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateUIClick(float duration = 0.08f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float signal = (float)Math.Sin(2.0 * Math.PI * 800.0 * t);
                float env = (float)Math.Exp(-progress * 35.0f);
                samples[i] = signal * env * 0.80f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateAnvilStrike(float duration = 0.40f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            var random = new Random(88);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / numSamples;

                float ironRing = (float)Math.Sin(2.0 * Math.PI * 1450.0 * t) + (float)Math.Sin(2.0 * Math.PI * 2900.0 * t) * 0.3f;
                float hammerThump = (float)Math.Sin(2.0 * Math.PI * 180.0 * t);
                float sparkNoise = ((float)random.NextDouble() * 2.0f - 1.0f) * 0.25f;

                float env = (float)Math.Exp(-progress * 7.0f);
                samples[i] = (ironRing * 0.5f + hammerThump * 0.35f + sparkNoise) * env * 0.90f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateVictoryFanfare(float duration = 2.0f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // 4 Arpeggiated trumpet chords: C4 (261.63), E4 (329.63), G4 (392.0), C5 (523.25)
            float[] notes = { 261.63f, 329.63f, 392.00f, 523.25f };
            float noteDuration = duration / notes.Length;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Math.Min(notes.Length - 1, (int)(t / noteDuration));
                float noteFreq = notes[noteIndex];
                float noteT = t - (noteIndex * noteDuration);
                float noteProgress = noteT / noteDuration;

                // Trumpet wave (fundamental + 2nd, 3rd harmonics)
                float signal = (float)Math.Sin(2.0 * Math.PI * noteFreq * noteT)
                             + (float)Math.Sin(2.0 * Math.PI * (noteFreq * 2.0f) * noteT) * 0.4f
                             + (float)Math.Sin(2.0 * Math.PI * (noteFreq * 3.0f) * noteT) * 0.2f;

                float env = (float)Math.Sin(Math.Min(1.0f, noteProgress * 4.0f) * Math.PI * 0.5f)
                            * (1.0f - noteProgress * 0.4f);

                samples[i] = signal * env * 0.70f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateDefeatJingle(float duration = 1.6f)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // Minor descending progression: Eb4 (311.13), D4 (293.66), C4 (261.63), Bb3 (233.08)
            float[] notes = { 311.13f, 293.66f, 261.63f, 233.08f };
            float noteDuration = duration / notes.Length;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Math.Min(notes.Length - 1, (int)(t / noteDuration));
                float noteFreq = notes[noteIndex];
                float noteT = t - (noteIndex * noteDuration);
                float noteProgress = noteT / noteDuration;

                float signal = (float)Math.Sin(2.0 * Math.PI * noteFreq * noteT)
                             + (float)Math.Sin(2.0 * Math.PI * (noteFreq * 2.0f) * noteT) * 0.25f;

                float env = (float)Math.Exp(-noteProgress * 3.5f);
                samples[i] = signal * env * 0.75f;
            }

            return EncodeWav(samples);
        }

        public static byte[] GenerateBgmRhythmLoop(bool isFever, float duration = 4.0f) // 2 measures at 120 BPM
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float beatLen = 0.5f; // 120 BPM
            int totalBeats = (int)(duration / beatLen);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                int currentBeat = (int)(t / beatLen);
                float beatT = t - (currentBeat * beatLen);

                // Rhythm percussion groove
                float kickSignal = 0f;
                float snareSignal = 0f;
                float hihatSignal = 0f;

                // Kick on beats 0, 2, 4, 6
                if (currentBeat % 2 == 0)
                {
                    float kEnv = (float)Math.Exp(-beatT * 18.0f);
                    kickSignal = (float)Math.Sin(2.0 * Math.PI * (60.0 - beatT * 40.0) * beatT) * kEnv * 0.5f;
                }
                // Snare on beats 1, 3, 5, 7
                else
                {
                    float sEnv = (float)Math.Exp(-beatT * 22.0f);
                    snareSignal = ((float)Math.Sin(2.0 * Math.PI * 280.0 * beatT) * 0.4f + 0.3f) * sEnv * 0.4f;
                }

                // Hi-hat 8th note pulse
                float eighthT = beatT % 0.25f;
                float hEnv = (float)Math.Exp(-eighthT * 45.0f);
                hihatSignal = ((float)Math.Sin(2.0 * Math.PI * 6500.0 * eighthT) * 0.3f) * hEnv * 0.25f;

                float feverMelody = 0f;
                if (isFever)
                {
                    // Upbeat brass/synth lead during Fever mode (Pentatonic arpeggios)
                    float[] melodyNotes = { 440f, 523.25f, 659.25f, 783.99f, 880f, 783.99f, 659.25f, 523.25f };
                    float leadFreq = melodyNotes[currentBeat % melodyNotes.Length];
                    float mEnv = (float)Math.Sin(beatT / beatLen * Math.PI) * 0.5f;
                    feverMelody = (float)Math.Sin(2.0 * Math.PI * leadFreq * beatT) * mEnv * 0.35f;
                }

                samples[i] = kickSignal + snareSignal + hihatSignal + feverMelody;
            }

            return EncodeWav(samples);
        }

        #endregion
    }
}
