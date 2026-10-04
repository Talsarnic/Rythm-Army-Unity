using System;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum UnitVisualState
    {
        Idle, Marching, Attacking, Defending, Charging, Jumping,
        Hurt, Dead, Victory, Fever, HeroAbility
    }

    public class UnitView
    {
        public LiveUnit LiveData { get; private set; }
        public UnitMember UnitMember { get { return LiveData != null ? LiveData.Member : null; } }
        public UnitVisualState CurrentState { get; private set; }
        public float VisualX { get; set; }
        public float VisualY { get; set; }
        public float ElevationY { get; set; }
        public bool FacingRight { get; set; }

        public PixelAnimator Animator { get; private set; }
        public UnitVisualProfile VisualProfile { get; private set; }

        public string WeaponSpriteId { get; private set; }
        public string ArmorSpriteId { get; private set; }
        public string ShieldSpriteId { get; private set; }
        public string HelmetSpriteId { get; private set; }
        public string MaskSpriteId { get; private set; }
        public string RelicSpriteId { get; private set; }

        public bool HasVisibleEquipment
        {
            get
            {
                return VisualProfile != null &&
                    (VisualProfile.HasWeapon || VisualProfile.HasShield ||
                     VisualProfile.HasHelmet || VisualProfile.HasMask || VisualProfile.HasRelic);
            }
        }

        public float HurtFlashTimer { get; private set; }
        public bool IsFlashingWhite { get { return HurtFlashTimer > 0f; } }
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

            Animator = PixelAnimator.CreateStandardUnitAnimator(
                liveData.Member.Class.ToString().ToLowerInvariant(),
                PixelSpriteRegistry.GetSpec(liveData.Member.Class).SpriteWidth);
            RefreshEquipmentSprites();
        }

        public void RefreshEquipmentSprites()
        {
            var m = UnitMember;
            if (m == null) return;

            VisualProfile = UnitVisualProfile.FromUnit(m);
            WeaponSpriteId = VisualProfile.WeaponSpriteId;
            ArmorSpriteId = VisualProfile.ArmorSpriteId;
            ShieldSpriteId = VisualProfile.ShieldSpriteId;
            HelmetSpriteId = VisualProfile.HelmetSpriteId;
            MaskSpriteId = VisualProfile.MaskSpriteId;
            RelicSpriteId = VisualProfile.RelicSpriteId;
        }

        public void SetState(UnitVisualState newState)
        {
            if (CurrentState == UnitVisualState.Dead &&
                newState != UnitVisualState.Idle &&
                newState != UnitVisualState.Victory)
                return;

            CurrentState = newState;
            switch (newState)
            {
                case UnitVisualState.Idle: Animator.Play("Idle"); break;
                case UnitVisualState.Marching: Animator.Play("March"); break;
                case UnitVisualState.Attacking: Animator.Play("Attack"); break;
                case UnitVisualState.Defending: Animator.Play("Defend"); break;
                case UnitVisualState.Charging: Animator.Play("Charge", true); break;
                case UnitVisualState.Jumping: Animator.Play("Jump"); break;
                case UnitVisualState.Hurt:
                    Animator.Play("Hurt");
                    HurtFlashTimer = 0.18f;
                    break;
                case UnitVisualState.Dead: Animator.Play("Death"); break;
                case UnitVisualState.Victory:
                case UnitVisualState.Fever:
                    Animator.Play("Fever");
                    break;
                case UnitVisualState.HeroAbility: Animator.Play("HeroAbility"); break;
            }
        }

        public void ApplyCombatAnimationCue(CombatAnimationCue cue)
        {
            switch (cue)
            {
                case CombatAnimationCue.AttackAnticipation:
                case CombatAnimationCue.AttackRelease:
                case CombatAnimationCue.AttackImpact:
                    SetState(UnitVisualState.Attacking);
                    break;
                case CombatAnimationCue.AttackRecovery:
                    SetState(UnitVisualState.Idle);
                    break;
                case CombatAnimationCue.ChargeAnticipation:
                case CombatAnimationCue.ChargeSurge:
                    SetState(UnitVisualState.Charging);
                    break;
                case CombatAnimationCue.ChargeRecovery:
                    SetState(UnitVisualState.Idle);
                    break;
                case CombatAnimationCue.DefendStart:
                case CombatAnimationCue.DefendHold:
                    SetState(UnitVisualState.Defending);
                    break;
                case CombatAnimationCue.DefendRecovery:
                    SetState(UnitVisualState.Idle);
                    break;
                case CombatAnimationCue.JumpTakeoff:
                case CombatAnimationCue.JumpAirborne:
                case CombatAnimationCue.JumpLanding:
                    SetState(UnitVisualState.Jumping);
                    break;
                case CombatAnimationCue.JumpRecovery:
                    SetState(UnitVisualState.Idle);
                    break;
                case CombatAnimationCue.MarchStart:
                    SetState(UnitVisualState.Marching);
                    break;
                case CombatAnimationCue.FeverPulse:
                    IsFeverActive = true;
                    SetState(UnitVisualState.Fever);
                    break;
                case CombatAnimationCue.HeroAbilityStart:
                case CombatAnimationCue.HeroAbilityImpact:
                    SetState(UnitVisualState.HeroAbility);
                    break;
                case CombatAnimationCue.HeroAbilityRecovery:
                    SetState(UnitVisualState.Idle);
                    break;
                case CombatAnimationCue.VictoryMarch:
                    SetState(UnitVisualState.Victory);
                    break;
            }
        }

        public void TakeDamage(float damage)
        {
            HurtFlashTimer = 0.18f;
            if (LiveData == null || !LiveData.IsAlive) SetState(UnitVisualState.Dead);
            else SetState(UnitVisualState.Hurt);
        }

        public void Update(float deltaTime)
        {
            if (LiveData != null)
            {
                VisualX += (LiveData.X - VisualX) * Math.Min(1f, 15f * deltaTime);
                VisualY += (LiveData.Y - VisualY) * Math.Min(1f, 15f * deltaTime);
            }

            Animator.Update(deltaTime);

            if (HurtFlashTimer > 0f)
            {
                HurtFlashTimer -= deltaTime;
                if (HurtFlashTimer <= 0f && LiveData != null &&
                    LiveData.IsAlive && CurrentState == UnitVisualState.Hurt)
                    SetState(UnitVisualState.Idle);
            }

            if (IsFeverActive) FeverSparkleTimer += deltaTime;
        }
    }
}
