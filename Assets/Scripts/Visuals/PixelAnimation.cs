using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public struct PixelRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public PixelRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public class AnimationFrame
    {
        public PixelRect Rect;
        public float DurationSeconds;
        public string EventTag; // "Step", "Swing", "Release", "Impact", "Chant"

        public AnimationFrame(PixelRect rect, float durationSeconds, string eventTag = null)
        {
            Rect = rect;
            DurationSeconds = durationSeconds;
            EventTag = eventTag;
        }
    }

    public class AnimationStateDef
    {
        public string Name;
        public List<AnimationFrame> Frames;
        public bool Loop;

        public AnimationStateDef(string name, bool loop = true)
        {
            Name = name;
            Frames = new List<AnimationFrame>();
            Loop = loop;
        }

        public void AddFrame(int x, int y, int width, int height, float duration, string eventTag = null)
        {
            Frames.Add(new AnimationFrame(new PixelRect(x, y, width, height), duration, eventTag));
        }
    }

    public class PixelAnimator
    {
        public string UnitId;
        public Dictionary<string, AnimationStateDef> States = new Dictionary<string, AnimationStateDef>();
        public string CurrentStateName { get; private set; }
        public int CurrentFrameIndex { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished { get; private set; }

        public event Action<string> OnAnimationEvent;

        public AnimationFrame CurrentFrame
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentStateName)) return null;
                AnimationStateDef state;
                if (!States.TryGetValue(CurrentStateName, out state) || state.Frames.Count == 0) return null;
                return state.Frames[Math.Min(CurrentFrameIndex, state.Frames.Count - 1)];
            }
        }

        public void Play(string stateName, bool forceRestart = false)
        {
            if (CurrentStateName == stateName && !forceRestart && !IsFinished) return;

            if (States.ContainsKey(stateName))
            {
                CurrentStateName = stateName;
                CurrentFrameIndex = 0;
                ElapsedTime = 0f;
                IsFinished = false;

                // Fire event on first frame if tag exists
                var frame = CurrentFrame;
                if (frame != null && !string.IsNullOrEmpty(frame.EventTag))
                {
                    if (OnAnimationEvent != null) OnAnimationEvent(frame.EventTag);
                }
            }
        }

        public void Update(float deltaTime)
        {
            if (string.IsNullOrEmpty(CurrentStateName) || IsFinished) return;

            AnimationStateDef state;
            if (!States.TryGetValue(CurrentStateName, out state) || state.Frames.Count == 0) return;

            ElapsedTime += deltaTime;
            var frame = state.Frames[CurrentFrameIndex];

            if (ElapsedTime >= frame.DurationSeconds)
            {
                ElapsedTime -= frame.DurationSeconds;
                CurrentFrameIndex++;

                if (CurrentFrameIndex >= state.Frames.Count)
                {
                    if (state.Loop)
                    {
                        CurrentFrameIndex = 0;
                    }
                    else
                    {
                        CurrentFrameIndex = state.Frames.Count - 1;
                        IsFinished = true;
                    }
                }

                var newFrame = state.Frames[CurrentFrameIndex];
                if (!string.IsNullOrEmpty(newFrame.EventTag))
                {
                    if (OnAnimationEvent != null) OnAnimationEvent(newFrame.EventTag);
                }
            }
        }

        public static PixelAnimator CreateStandardUnitAnimator(string unitId, int spriteSize = 32)
        {
            var anim = new PixelAnimator { UnitId = unitId };

            // 1. Idle (4 frames, 0.15s per frame)
            var idle = new AnimationStateDef("Idle", true);
            for (int i = 0; i < 4; i++)
            {
                idle.AddFrame(i * spriteSize, 0, spriteSize, spriteSize, 0.15f);
            }
            anim.States["Idle"] = idle;

            // 2. March (4 frames with Step on frames 0 and 2)
            var march = new AnimationStateDef("March", true);
            march.AddFrame(0, spriteSize, spriteSize, spriteSize, 0.125f, "Step");
            march.AddFrame(spriteSize, spriteSize, spriteSize, spriteSize, 0.125f);
            march.AddFrame(spriteSize * 2, spriteSize, spriteSize, spriteSize, 0.125f, "Step");
            march.AddFrame(spriteSize * 3, spriteSize, spriteSize, spriteSize, 0.125f);
            anim.States["March"] = march;

            // 3. Attack (Windup -> Swing/Release -> Followthrough)
            var attack = new AnimationStateDef("Attack", false);
            attack.AddFrame(0, spriteSize * 2, spriteSize, spriteSize, 0.10f, "Windup");
            attack.AddFrame(spriteSize, spriteSize * 2, spriteSize, spriteSize, 0.08f, "Release");
            attack.AddFrame(spriteSize * 2, spriteSize * 2, spriteSize, spriteSize, 0.12f, "Impact");
            attack.AddFrame(spriteSize * 3, spriteSize * 2, spriteSize, spriteSize, 0.15f);
            anim.States["Attack"] = attack;

            // 4. Defend (Brace pose)
            var defend = new AnimationStateDef("Defend", true);
            defend.AddFrame(0, spriteSize * 3, spriteSize, spriteSize, 0.25f, "ShieldWall");
            defend.AddFrame(spriteSize, spriteSize * 3, spriteSize, spriteSize, 0.25f);
            anim.States["Defend"] = defend;

            // 5. Fever (Energetic dance)
            var fever = new AnimationStateDef("Fever", true);
            for (int i = 0; i < 4; i++)
            {
                fever.AddFrame(i * spriteSize, spriteSize * 4, spriteSize, spriteSize, 0.10f, i % 2 == 0 ? "Chant" : null);
            }
            anim.States["Fever"] = fever;

            // 6. Hurt
            var hurt = new AnimationStateDef("Hurt", false);
            hurt.AddFrame(0, spriteSize * 5, spriteSize, spriteSize, 0.10f);
            hurt.AddFrame(spriteSize, spriteSize * 5, spriteSize, spriteSize, 0.15f);
            anim.States["Hurt"] = hurt;

            // 7. Death
            var death = new AnimationStateDef("Death", false);
            death.AddFrame(0, spriteSize * 6, spriteSize, spriteSize, 0.12f);
            death.AddFrame(spriteSize, spriteSize * 6, spriteSize, spriteSize, 0.15f);
            death.AddFrame(spriteSize * 2, spriteSize * 6, spriteSize, spriteSize, 0.30f);
            anim.States["Death"] = death;

            anim.Play("Idle");
            return anim;
        }
    }
}
