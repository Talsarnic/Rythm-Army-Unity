using System;
using System.Collections.Generic;
using RhythmArmy.Core.Audio;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Core.Audio
{
    public class LatencyCalibrationSession
    {
        private readonly List<float> _tapOffsets = new List<float>();
        public float TargetInterval { get; private set; } // e.g. 0.5s for 120 BPM
        public int TotalTapsRequired { get; private set; }

        public LatencyCalibrationSession(float bpm = 120f, int tapsRequired = 8)
        {
            TargetInterval = 60f / bpm;
            TotalTapsRequired = tapsRequired;
        }

        public void RegisterTap(float dspTime, float beatGridTime)
        {
            // Difference between actual user tap and target beat grid
            float offset = dspTime - beatGridTime;
            _tapOffsets.Add(offset);
        }

        public bool IsComplete
        {
            get { return _tapOffsets.Count >= TotalTapsRequired; }
        }

        public float CalculateCalibratedLatency()
        {
            if (_tapOffsets.Count == 0) return 0f;
            float sum = 0f;
            for (int i = 0; i < _tapOffsets.Count; i++)
            {
                sum += _tapOffsets[i];
            }
            // Average offset in seconds (e.g. +0.035s = 35ms)
            return sum / _tapOffsets.Count;
        }
    }

    public class GameSettings
    {
        // Audio Settings
        public float MasterVolume = 1.0f; // 0.0 to 1.0
        public float MusicVolume = 0.8f;
        public float SfxVolume = 0.9f;
        public float ChantVolume = 1.0f;
        public float AudioLatencySeconds = 0.0f; // Input/Audio DSP offset in seconds (-0.20s to +0.20s)

        // Visual / Video Settings
        public bool Fullscreen = true;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public bool PixelSnap = true;
        public bool ScreenShakeEnabled = true;

        // Controls & Haptics
        public bool VibrationEnabled = true;
        public float VibrationStrength = 0.8f;
        public Dictionary<DrumId, KeyBinding> DrumBindings = new Dictionary<DrumId, KeyBinding>();

        public GameSettings()
        {
            DrumBindings[DrumId.Boom] = new KeyBinding("A", DrumId.Boom);
            DrumBindings[DrumId.Tak] = new KeyBinding("D", DrumId.Tak);
            DrumBindings[DrumId.Rat] = new KeyBinding("W", DrumId.Rat);
            DrumBindings[DrumId.Ting] = new KeyBinding("S", DrumId.Ting);
        }

        public string ToJson()
        {
            var dict = new Dictionary<string, object>
            {
                { "MasterVolume", (double)MasterVolume },
                { "MusicVolume", (double)MusicVolume },
                { "SfxVolume", (double)SfxVolume },
                { "ChantVolume", (double)ChantVolume },
                { "AudioLatencySeconds", (double)AudioLatencySeconds },
                { "Fullscreen", Fullscreen },
                { "ResolutionWidth", ResolutionWidth },
                { "ResolutionHeight", ResolutionHeight },
                { "PixelSnap", PixelSnap },
                { "ScreenShakeEnabled", ScreenShakeEnabled },
                { "VibrationEnabled", VibrationEnabled },
                { "VibrationStrength", (double)VibrationStrength }
            };

            return MiniJson.Serialize(dict);
        }

        public static GameSettings FromJson(string json)
        {
            var settings = new GameSettings();
            if (string.IsNullOrEmpty(json)) return settings;

            var dict = MiniJson.Deserialize<Dictionary<string, object>>(json);
            if (dict == null) return settings;

            if (dict.ContainsKey("MasterVolume")) settings.MasterVolume = Convert.ToSingle(dict["MasterVolume"]);
            if (dict.ContainsKey("MusicVolume")) settings.MusicVolume = Convert.ToSingle(dict["MusicVolume"]);
            if (dict.ContainsKey("SfxVolume")) settings.SfxVolume = Convert.ToSingle(dict["SfxVolume"]);
            if (dict.ContainsKey("ChantVolume")) settings.ChantVolume = Convert.ToSingle(dict["ChantVolume"]);
            if (dict.ContainsKey("AudioLatencySeconds")) settings.AudioLatencySeconds = Convert.ToSingle(dict["AudioLatencySeconds"]);
            if (dict.ContainsKey("Fullscreen")) settings.Fullscreen = Convert.ToBoolean(dict["Fullscreen"]);
            if (dict.ContainsKey("ResolutionWidth")) settings.ResolutionWidth = Convert.ToInt32(dict["ResolutionWidth"]);
            if (dict.ContainsKey("ResolutionHeight")) settings.ResolutionHeight = Convert.ToInt32(dict["ResolutionHeight"]);
            if (dict.ContainsKey("PixelSnap")) settings.PixelSnap = Convert.ToBoolean(dict["PixelSnap"]);
            if (dict.ContainsKey("ScreenShakeEnabled")) settings.ScreenShakeEnabled = Convert.ToBoolean(dict["ScreenShakeEnabled"]);
            if (dict.ContainsKey("VibrationEnabled")) settings.VibrationEnabled = Convert.ToBoolean(dict["VibrationEnabled"]);
            if (dict.ContainsKey("VibrationStrength")) settings.VibrationStrength = Convert.ToSingle(dict["VibrationStrength"]);

            return settings;
        }

        public void ApplyToAudioController(AudioController controller)
        {
            if (controller == null) return;
            controller.MasterVolume = MasterVolume;
            controller.BGMVolume = MusicVolume;
            controller.SFXVolume = SfxVolume;
            controller.VoiceVolume = ChantVolume;
        }

        public float AdjustInputTimeWithLatency(float rawInputTime)
        {
            // Subtract latency: if user taps late due to display/audio lag (+0.05s), adjust earlier
            return rawInputTime - AudioLatencySeconds;
        }
    }
}
