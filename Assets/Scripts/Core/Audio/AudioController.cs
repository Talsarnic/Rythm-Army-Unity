using System;
using System.Collections.Generic;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Core.Audio
{
    public enum UISoundType
    {
        ButtonClick,
        MenuOpen,
        ItemEquip,
        ItemCraft,
        UnitEvolve,
        UnitRecruit,
        MissionStart,
        VictoryFanfare,
        DefeatJingle
    }

    /// <summary>
    /// Master Audio Controller bridging Unity AudioSource playback channels with rhythm sequencer and UI sound effects.
    /// Supports dynamic Fever BGM stem layering and volume mixing.
    /// </summary>
    public class AudioController
    {
        public RhythmAudioSequencer Sequencer { get; private set; }

        public float MasterVolume { get; set; }
        public float BGMVolume { get; set; }
        public float SFXVolume { get; set; }
        public float VoiceVolume { get; set; }

        public bool IsMuted { get; set; }

        public List<string> PlayedSoundEffectsLog { get; private set; }

        public event Action<string, float, float> OnPlayAudioClip; // clipName, volume, pitch

        public AudioController(RhythmAudioSequencer sequencer = null)
        {
            Sequencer = sequencer != null ? sequencer : new RhythmAudioSequencer();
            MasterVolume = 1.0f;
            BGMVolume = 0.8f;
            SFXVolume = 0.9f;
            VoiceVolume = 1.0f;
            IsMuted = false;
            PlayedSoundEffectsLog = new List<string>();
        }

        public void PlayDrum(DrumId drum, Grade grade)
        {
            if (IsMuted) return;

            string clipName;
            switch (drum)
            {
                case DrumId.Boom: clipName = "drum_boom"; break;
                case DrumId.Tak: clipName = "drum_tak"; break;
                case DrumId.Rat: clipName = "drum_rat"; break;
                case DrumId.Ting: clipName = "drum_ting"; break;
                default: clipName = "drum_boom"; break;
            }

            float volume = MasterVolume * SFXVolume * (grade == Grade.Perfect ? 1.0f : 0.85f);
            float pitch = grade == Grade.Perfect ? 1.0f : 0.96f;

            PlayedSoundEffectsLog.Add(clipName);
            if (OnPlayAudioClip != null)
            {
                OnPlayAudioClip(clipName, volume, pitch);
            }
        }

        public void PlayUISound(UISoundType soundType)
        {
            if (IsMuted) return;

            string clipName;
            float pitch = 1.0f;

            switch (soundType)
            {
                case UISoundType.ButtonClick:
                    clipName = "sfx_ui_click";
                    break;
                case UISoundType.MenuOpen:
                    clipName = "sfx_ui_open";
                    break;
                case UISoundType.ItemEquip:
                    clipName = "sfx_equip_armor";
                    break;
                case UISoundType.ItemCraft:
                    clipName = "sfx_anvil_strike";
                    break;
                case UISoundType.UnitEvolve:
                    clipName = "sfx_altar_mystic";
                    pitch = 1.1f;
                    break;
                case UISoundType.UnitRecruit:
                    clipName = "sfx_cheer_squad";
                    break;
                case UISoundType.MissionStart:
                    clipName = "sfx_horn_advance";
                    break;
                case UISoundType.VictoryFanfare:
                    clipName = "mus_victory_fanfare";
                    break;
                case UISoundType.DefeatJingle:
                    clipName = "mus_defeat_jingle";
                    break;
                default:
                    clipName = "sfx_ui_click";
                    break;
            }

            float volume = MasterVolume * SFXVolume;
            PlayedSoundEffectsLog.Add(clipName);
            if (OnPlayAudioClip != null)
            {
                OnPlayAudioClip(clipName, volume, pitch);
            }
        }

        public void Update(float deltaTime)
        {
            Sequencer.Update(deltaTime);
        }
    }
}
