using System;

namespace RhythmArmy.Core.Combat
{
    public enum FloatingTextType
    {
        StandardDamage,
        CriticalDamage,
        ShieldBlock,
        Heal,
        FeverBonus,
        HeroSkill,
        CurrencyGain
    }

    /// <summary>
    /// Represents floating numbers and combat notifications appearing above units and enemies.
    /// </summary>
    public class FloatingCombatText
    {
        public string Text;
        public float X;
        public float Y;
        public float VelocityY;
        public float Lifetime;
        public float MaxLifetime;
        public FloatingTextType Type;
        public float Alpha { get { return MaxLifetime > 0 ? Math.Max(0f, Math.Min(1f, Lifetime / (MaxLifetime * 0.4f))) : 0f; } }

        public FloatingCombatText(string text, float x, float y, FloatingTextType type, float duration = 1.0f)
        {
            Text = text;
            X = x;
            Y = y;
            Type = type;
            Lifetime = duration;
            MaxLifetime = duration;
            VelocityY = type == FloatingTextType.CriticalDamage ? -45f : -25f;
        }

        public void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
            Y += VelocityY * deltaTime;
            VelocityY *= 0.92f;
        }
    }
}
