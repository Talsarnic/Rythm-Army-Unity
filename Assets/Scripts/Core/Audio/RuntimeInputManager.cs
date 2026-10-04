using System;
using System.Collections.Generic;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Core.Audio
{
    public enum InputDeviceType
    {
        Keyboard,
        Gamepad
    }

    public class KeyBinding
    {
        public string KeyName { get; set; }
        public DrumId Drum { get; set; }

        public KeyBinding(string keyName, DrumId drum)
        {
            KeyName = keyName;
            Drum = drum;
        }
    }

    /// <summary>
    /// Manages configurable input mapping for Drums (Boom, Tak, Rat, Ting) and system commands across Keyboard and Gamepads.
    /// </summary>
    public class RuntimeInputManager
    {
        private readonly Dictionary<string, DrumId> _keyMap = new Dictionary<string, DrumId>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, DrumId> _gamepadButtonMap = new Dictionary<int, DrumId>();

        public event Action<DrumId> OnDrumTriggered;
        public event Action OnAdvanceCutsceneTriggered;
        public event Action OnPauseTriggered;

        public RuntimeInputManager()
        {
            SetDefaultBindings();
        }

        public void SetDefaultBindings()
        {
            _keyMap.Clear();
            _gamepadButtonMap.Clear();

            // Default Keyboard layout:
            // A / Left Arrow -> Boom (Square)
            // S / Down Arrow -> Tak (Circle)
            // D / Up Arrow   -> Rat (Triangle)
            // F / Right Arrow -> Ting (Cross)
            BindKey("A", DrumId.Boom);
            BindKey("LeftArrow", DrumId.Boom);
            BindKey("J", DrumId.Boom);

            BindKey("S", DrumId.Tak);
            BindKey("DownArrow", DrumId.Tak);
            BindKey("K", DrumId.Tak);

            BindKey("D", DrumId.Rat);
            BindKey("UpArrow", DrumId.Rat);
            BindKey("I", DrumId.Rat);

            BindKey("F", DrumId.Ting);
            BindKey("RightArrow", DrumId.Ting);
            BindKey("L", DrumId.Ting);

            // Gamepad default button mapping (PlayStation / Xbox equivalents):
            // Button 0 (X / A) -> Tak
            // Button 1 (Circle / B) -> Boom
            // Button 2 (Square / X) -> Rat
            // Button 3 (Triangle / Y) -> Ting
            BindGamepadButton(0, DrumId.Tak);
            BindGamepadButton(1, DrumId.Boom);
            BindGamepadButton(2, DrumId.Rat);
            BindGamepadButton(3, DrumId.Ting);
        }

        public void BindKey(string keyName, DrumId drum)
        {
            _keyMap[keyName] = drum;
        }

        public void BindGamepadButton(int buttonIndex, DrumId drum)
        {
            _gamepadButtonMap[buttonIndex] = drum;
        }

        public bool ProcessKeyInput(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return false;

            if (keyName.Equals("Space", StringComparison.OrdinalIgnoreCase) ||
                keyName.Equals("Return", StringComparison.OrdinalIgnoreCase) ||
                keyName.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                if (OnAdvanceCutsceneTriggered != null)
                {
                    OnAdvanceCutsceneTriggered.Invoke();
                }
                return true;
            }

            if (keyName.Equals("Escape", StringComparison.OrdinalIgnoreCase) ||
                keyName.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                if (OnPauseTriggered != null)
                {
                    OnPauseTriggered.Invoke();
                }
                return true;
            }

            DrumId drum;
            if (_keyMap.TryGetValue(keyName, out drum))
            {
                if (OnDrumTriggered != null)
                {
                    OnDrumTriggered.Invoke(drum);
                }
                return true;
            }

            return false;
        }

        public bool ProcessGamepadButton(int buttonIndex)
        {
            DrumId drum;
            if (_gamepadButtonMap.TryGetValue(buttonIndex, out drum))
            {
                if (OnDrumTriggered != null)
                {
                    OnDrumTriggered.Invoke(drum);
                }
                return true;
            }

            return false;
        }

        public DrumId? GetDrumForKey(string keyName)
        {
            DrumId drum;
            if (_keyMap.TryGetValue(keyName, out drum))
            {
                return drum;
            }
            return null;
        }
    }
}
