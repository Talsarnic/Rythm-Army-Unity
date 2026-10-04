using System;
using RhythmArmy.Gameplay.Narrative;

namespace RhythmArmy.UI
{
    /// <summary>
    /// UI Presentation Controller for Typewriter Dialogue and Cutscenes.
    /// Bridges DialoguePlayer state with dialogue bubble visual views and audio blip triggers.
    /// </summary>
    public class DialogueUIController
    {
        public DialoguePlayer DialoguePlayer { get; private set; }

        public event Action<string, string, DialogueEmotion> OnDialogueLineStarted; // speaker, portraitId, emotion
        public event Action<string> OnTextUpdated; // visible typewriter string
        public event Action OnSequenceCompleted;
        public event Action<float> OnTypewriterCharacterTyped; // character pitch

        private int _lastCharCount;

        public DialogueUIController(DialoguePlayer player = null)
        {
            DialoguePlayer = player != null ? player : new DialoguePlayer();
            _lastCharCount = 0;
            DialoguePlayer.OnLineChanged += HandleLineChanged;
            DialoguePlayer.OnSequenceCompleted += HandleSequenceCompleted;
        }

        public void PlaySequence(DialogueSequence sequence)
        {
            _lastCharCount = 0;
            DialoguePlayer.Play(sequence);
        }

        public void AdvanceDialogue()
        {
            DialoguePlayer.AdvanceOrSkip();
        }

        public void Update(float deltaTime)
        {
            if (DialoguePlayer == null || DialoguePlayer.IsFinished) return;

            DialoguePlayer.Update(deltaTime);

            string visible = DialoguePlayer.VisibleText;
            if (visible.Length != _lastCharCount)
            {
                _lastCharCount = visible.Length;
                if (OnTextUpdated != null)
                {
                    OnTextUpdated(visible);
                }

                if (DialoguePlayer.CurrentLine != null && OnTypewriterCharacterTyped != null)
                {
                    OnTypewriterCharacterTyped(DialoguePlayer.CurrentLine.VoicePitch);
                }
            }
        }

        private void HandleLineChanged(DialogueLine line)
        {
            _lastCharCount = 0;
            if (line != null)
            {
                if (OnDialogueLineStarted != null)
                {
                    OnDialogueLineStarted(line.Speaker, line.PortraitSpriteId, line.Emotion);
                }
                if (OnTextUpdated != null)
                {
                    OnTextUpdated(string.Empty);
                }
            }
        }

        private void HandleSequenceCompleted()
        {
            if (OnSequenceCompleted != null)
            {
                OnSequenceCompleted();
            }
        }
    }
}
