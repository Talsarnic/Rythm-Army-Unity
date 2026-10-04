using System;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum UnitVisualState
    {
        Idle,
        Marching,
        Attacking,
        Defending,
        Charging,
        Jumping,
        Hurt,
        Dead,
        Victory
    }

    /// <summary>
    /// Presentation model for a friendly squad unit in the Moonlighter 2D pixel art style.
    /// Handles sprite animation state, equipment overlay anchors, damage flash, and pixel-grid rendering.
    /// </summary>
    public class UnitView
    {
        public LiveUnit LiveData { get; private set; }
        public UnitMember UnitMember
        {
            get { return LiveData != null ? LiveData.Member : null; }
        }
        public UnitVisualState CurrentState { get; private set; }
        public float VisualX { get; set; }
        public float VisualY { get; set; }
        public float ElevationY { get; set; }
        public bool FacingRight { get; set; }

        public PixelAnimator Animator { get; private set; }

        // Equipment visual sprite identifiers
        public string WeaponSpriteId { get; private set; }
        public string ArmorSpriteId { get; private set; }
        public string ShieldSpriteId { get; private set; }
        public string HelmetSpriteId { get; private set; }

        // Visual FX / Feedback
        public float HurtFlashTimer { get; private set; }
        public bool IsFlashingWhite
        {
            get { return HurtFlashTimer > 0f; }
        }
        public float FeverSparkleTimer { get; private set; }
        public bool IsFeverActive { get; set; }

        public UnitView(LiveUnit liveData)
        {
            if (liveData == null) throw new ArgumentNullException("liveData");
            LiveData = liveData;
            VisualX = liveData.X;
            VisualY = liveData.Y;
            ElevationY = 0f;
            FacingRight = true;
            CurrentState = UnitVisualState.Idle;

            Animator = PixelAnimator.CreateStandardUnitAnimator(liveData.Member.Class.ToString().ToLower(), 32);
            RefreshEquipmentSprites();
        }

        public void RefreshEquipmentSprites()
        {
            var m = UnitMember;
            if (m == null) return;

            WeaponSpriteId = !string.IsNullOrEmpty(m.WeaponId) ? "item_" + m.WeaponId : null;
            ShieldSpriteId = !string.IsNullOrEmpty(m.ShieldId) ? "item_" + m.ShieldId : null;
            HelmetSpriteId = !string.IsNullOrEmpty(m.HelmetId) ? "item_" + m.HelmetId : null;
        }

        public void SetState(UnitVisualState newState)
        {
            if (CurrentState == UnitVisualState.Dead && newState != UnitVisualState.Idle)
                return; // Dead units remain dead until revived

            CurrentState = newState;

            switch (newState)
            {
                case UnitVisualState.Idle:
                    Animator.Play("Idle");
                    break;
                case UnitVisualState.Marching:
                    Animator.Play("March");
                    break;
                case UnitVisualState.Attacking:
                    Animator.Play("Attack");
                    break;
                case UnitVisualState.Defending:
                    Animator.Play("Defend");
                    break;
                case UnitVisualState.Charging:
                    Animator.Play("March"); // Charging uses intense march cycle
                    break;
                case UnitVisualState.Jumping:
                    Animator.Play("March");
                    break;
                case UnitVisualState.Hurt:
                    Animator.Play("Hurt");
                    HurtFlashTimer = 0.18f;
                    break;
                case UnitVisualState.Dead:
                    Animator.Play("Death");
                    break;
                case UnitVisualState.Victory:
                    Animator.Play("Fever");
                    break;
            }
        }

        public void TakeDamage(float damage)
        {
            HurtFlashTimer = 0.18f;
            if (LiveData == null || !LiveData.IsAlive)
            {
                SetState(UnitVisualState.Dead);
            }
            else
            {
                SetState(UnitVisualState.Hurt);
            }
        }

        public void Update(float deltaTime)
        {
            if (LiveData != null)
            {
                // Smoothly interpolate visual position to live physics position
                VisualX += (LiveData.X - VisualX) * Math.Min(1f, 15f * deltaTime);
                VisualY += (LiveData.Y - VisualY) * Math.Min(1f, 15f * deltaTime);
            }

            Animator.Update(deltaTime);

            if (HurtFlashTimer > 0f)
            {
                HurtFlashTimer -= deltaTime;
                if (HurtFlashTimer <= 0f && LiveData != null && LiveData.IsAlive && CurrentState == UnitVisualState.Hurt)
                {
                    SetState(UnitVisualState.Idle);
                }
            }

            if (IsFeverActive)
            {
                FeverSparkleTimer += deltaTime;
            }
        }
    }
}
