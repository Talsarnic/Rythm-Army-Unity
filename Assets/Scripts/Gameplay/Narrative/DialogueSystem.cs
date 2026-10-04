using System;
using System.Collections.Generic;

namespace RhythmArmy.Gameplay.Narrative
{
    public enum DialogueEmotion
    {
        Neutral,
        Determined,
        Worried,
        Excited,
        Mystical,
        Triumphant
    }

    public class DialogueLine
    {
        public string Speaker;
        public string PortraitSpriteId;
        public DialogueEmotion Emotion;
        public string Text;
        public float VoicePitch;

        public DialogueLine(string speaker, string portraitSpriteId, DialogueEmotion emotion, string text, float voicePitch = 1.0f)
        {
            Speaker = speaker;
            PortraitSpriteId = portraitSpriteId;
            Emotion = emotion;
            Text = text;
            VoicePitch = voicePitch;
        }
    }

    public class DialogueSequence
    {
        public string Id;
        public string Title;
        public List<DialogueLine> Lines = new List<DialogueLine>();

        public DialogueSequence(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public DialogueSequence AddLine(string speaker, string portraitSpriteId, DialogueEmotion emotion, string text, float pitch = 1.0f)
        {
            Lines.Add(new DialogueLine(speaker, portraitSpriteId, emotion, text, pitch));
            return this;
        }
    }

    public class DialoguePlayer
    {
        public DialogueSequence CurrentSequence { get; private set; }
        public int CurrentLineIndex { get; private set; }
        public float TypewriterProgress { get; private set; }
        public float TypewriterCharsPerSec = 45f;
        public bool IsFinished { get; private set; }

        public event Action<DialogueLine> OnLineChanged;
        public event Action OnSequenceCompleted;

        public DialogueLine CurrentLine
        {
            get
            {
                if (CurrentSequence == null || CurrentLineIndex >= CurrentSequence.Lines.Count)
                    return null;
                return CurrentSequence.Lines[CurrentLineIndex];
            }
        }

        public string VisibleText
        {
            get
            {
                var line = CurrentLine;
                if (line == null) return string.Empty;
                int chars = Math.Min(line.Text.Length, (int)TypewriterProgress);
                return line.Text.Substring(0, chars);
            }
        }

        public bool IsLineFullyRevealed
        {
            get
            {
                var line = CurrentLine;
                if (line == null) return true;
                return TypewriterProgress >= line.Text.Length;
            }
        }

        public void Play(DialogueSequence sequence)
        {
            CurrentSequence = sequence;
            CurrentLineIndex = 0;
            TypewriterProgress = 0f;
            IsFinished = false;

            if (CurrentLine != null && OnLineChanged != null)
            {
                OnLineChanged.Invoke(CurrentLine);
            }
        }

        public void Update(float deltaTime)
        {
            if (IsFinished || CurrentLine == null) return;

            if (!IsLineFullyRevealed)
            {
                TypewriterProgress += TypewriterCharsPerSec * deltaTime;
            }
        }

        public bool AdvanceOrSkip()
        {
            if (IsFinished || CurrentLine == null) return false;

            if (!IsLineFullyRevealed)
            {
                // Instantly complete line typewriter
                TypewriterProgress = CurrentLine.Text.Length;
                return true;
            }

            CurrentLineIndex++;
            TypewriterProgress = 0f;

            if (CurrentLineIndex < CurrentSequence.Lines.Count)
            {
                if (OnLineChanged != null)
                {
                    OnLineChanged.Invoke(CurrentLine);
                }
                return true;
            }
            else
            {
                IsFinished = true;
                if (OnSequenceCompleted != null)
                {
                    OnSequenceCompleted.Invoke();
                }
                return false;
            }
        }
    }

    public static class DialogueDatabase
    {
        private static readonly Dictionary<string, DialogueSequence> _sequences = new Dictionary<string, DialogueSequence>();

        static DialogueDatabase()
        {
            RegisterIntro();
            RegisterMissionBriefings();
            RegisterCampDialogues();
        }

        private static void RegisterIntro()
        {
            var seq = new DialogueSequence("intro-awakening", "Awakening of the Sacred Drums");
            seq.AddLine("High Priestess Leah", "portrait_leah_mystical", DialogueEmotion.Mystical, "Almighty Creator! The sacred heartbeat has returned to our tribe at last...");
            seq.AddLine("High Priestess Leah", "portrait_leah_determined", DialogueEmotion.Determined, "Our ancestors' lands have been overrun by the Shadowmask Clan and savage beasts.");
            seq.AddLine("High Priestess Leah", "portrait_leah_excited", DialogueEmotion.Excited, "Take the sacred drums: BOOM, TAK, RAT, and TING! Lead our army to the World's Edge!");
            _sequences[seq.Id] = seq;
        }

        private static void RegisterMissionBriefings()
        {
            var m1 = new DialogueSequence("briefing-coast-hunt", "Hunting on the Coral Coast");
            m1.AddLine("High Priestess Leah", "portrait_leah_determined", DialogueEmotion.Determined, "Our tribe is starving. Hunt the plains runners along the Coral Coast for meat and hides.");
            m1.AddLine("High Priestess Leah", "portrait_leah_neutral", DialogueEmotion.Neutral, "Strike the March rhythm (BOOM BOOM BOOM TAK) to advance, then Attack (TAK TAK BOOM TAK)!");
            _sequences[m1.Id] = m1;
            _sequences["briefing-m01-patata-plains"] = m1;
            _sequences["briefing-m01-coral-coast"] = m1;
            _sequences["briefing-mission-1"] = m1;

            var m3 = new DialogueSequence("briefing-drake-caldera", "The Volcanic Caldera & Drake Titan");
            m3.AddLine("High Priestess Leah", "portrait_leah_worried", DialogueEmotion.Worried, "Beware, Almighty! The Drake Titan slumbers in the caldera. When it roars and charges, chant JUMP (TING TING BOOM TAK)!");
            _sequences[m3.Id] = m3;
            _sequences["briefing-m04-volcanic-caldera"] = m3;
            _sequences["briefing-mission-3"] = m3;

            var m6 = new DialogueSequence("briefing-golem-altar", "The Colossus Golem of Ruin Altar");
            m6.AddLine("High Priestess Leah", "portrait_leah_mystical", DialogueEmotion.Mystical, "The ancient Colossus Golem guards the Sacred Tree of Life relics. Charge power (TAK TAK RAT RAT) before unleashing your strikes!");
            _sequences[m6.Id] = m6;
            _sequences["briefing-mission-6"] = m6;
        }

        private static void RegisterCampDialogues()
        {
            var bs = new DialogueSequence("camp-blacksmith-greeting", "Blacksmith Vulkan's Forge");
            bs.AddLine("Blacksmith Vulkan", "portrait_vulkan_neutral", DialogueEmotion.Determined, "Need sharper spears and sturdier shields? Bring me iron slag and hardwood from your hunts!");
            _sequences[bs.Id] = bs;

            var ma = new DialogueSequence("camp-altar-greeting", "The Spirit Altar");
            ma.AddLine("Altar Guardian", "portrait_altar_mystical", DialogueEmotion.Mystical, "Offer rare materials to the sacred tree to awaken ancient subspecies: Swiftpaw, Frogtide, or the mighty Apex!");
            _sequences[ma.Id] = ma;
        }

        public static DialogueSequence Get(string id)
        {
            DialogueSequence seq;
            _sequences.TryGetValue(id, out seq);
            return seq;
        }
    }
}
