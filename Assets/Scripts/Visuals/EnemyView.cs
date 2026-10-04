using System;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum EnemyVisualState
    {
        Idle,
        Moving,
        Telegraphing,
        Attacking,
        Hurt,
        Dead
    }

    /// <summary>
    /// Presentation model for enemies and colossal bosses in the Moonlighter 2D pixel art style.
    /// Handles sprite animation state, telegraph warning circles, damage flashes, and boss size scaling.
    /// </summary>
    public class EnemyView
    {
        public LiveEnemy LiveData { get; private set; }
        public EnemyKind Kind
        {
            get { return LiveData != null ? LiveData.Kind : EnemyKind.PlainsRunner; }
        }
        public EnemyCategory Category
        {
            get { return EnemyMetadata.GetCategory(Kind); }
        }

        public EnemyVisualState CurrentState { get; private set; }
        public float VisualX { get; set; }
        public float VisualY { get; set; }
        public float SpriteScale { get; private set; }
        public int SpriteSize { get; private set; }

        public PixelAnimator Animator { get; private set; }

        // Telegraph & Boss Warning Visuals
        public bool IsTelegraphing { get; private set; }
        public BossAttackType CurrentTelegraph { get; private set; }
        public float TelegraphProgress { get; private set; } // 0.0 to 1.0
        public float TelegraphDuration { get; private set; }
        public float TelegraphTimer { get; private set; }

        // Damage FX
        public float HurtFlashTimer { get; private set; }
        public bool IsFlashingWhite
        {
            get { return HurtFlashTimer > 0f; }
        }

        public EnemyView(LiveEnemy liveData)
        {
            if (liveData == null) throw new ArgumentNullException("liveData");
            LiveData = liveData;
            VisualX = liveData.X;
            VisualY = liveData.Y;
            SpriteScale = 1.0f;
            SpriteSize = 32;
            CurrentState = EnemyVisualState.Idle;

            DetermineSpriteDimensions();
            Animator = PixelAnimator.CreateStandardUnitAnimator(Kind.ToString().ToLower(), SpriteSize);
        }

        private void DetermineSpriteDimensions()
        {
            switch (Kind)
            {
                case EnemyKind.ColossusGolem:
                case EnemyKind.DrakeTitan:
                case EnemyKind.IronBehemoth:
                    SpriteSize = 96;
                    SpriteScale = 1.5f;
                    break;
                case EnemyKind.CatapultTower:
                case EnemyKind.Watchtower:
                case EnemyKind.StoneWall:
                case EnemyKind.Barricade:
                case EnemyKind.GiantBoar:
                    SpriteSize = 64;
                    SpriteScale = 1.25f;
                    break;
                case EnemyKind.TribeCavalry:
                case EnemyKind.TribeHammerer:
                case EnemyKind.TribeSkyrider:
                    SpriteSize = 48;
                    SpriteScale = 1.15f;
                    break;
                default:
                    SpriteSize = 32;
                    SpriteScale = 1.0f;
                    break;
            }
        }

        public void StartTelegraph(BossAttackType telegraphType, float duration)
        {
            IsTelegraphing = true;
            CurrentTelegraph = telegraphType;
            TelegraphDuration = duration > 0f ? duration : 1.0f;
            TelegraphTimer = 0f;
            TelegraphProgress = 0f;
            CurrentState = EnemyVisualState.Telegraphing;
            Animator.Play("Idle");
        }

        public void ClearTelegraph()
        {
            IsTelegraphing = false;
            TelegraphProgress = 0f;
            TelegraphTimer = 0f;
            if (CurrentState == EnemyVisualState.Telegraphing)
            {
                CurrentState = EnemyVisualState.Idle;
            }
        }

        public void SetState(EnemyVisualState newState)
        {
            if (CurrentState == EnemyVisualState.Dead && newState != EnemyVisualState.Idle)
                return;

            CurrentState = newState;

            switch (newState)
            {
                case EnemyVisualState.Idle:
                    Animator.Play("Idle");
                    break;
                case EnemyVisualState.Moving:
                    Animator.Play("March");
                    break;
                case EnemyVisualState.Attacking:
                    Animator.Play("Attack");
                    break;
                case EnemyVisualState.Hurt:
                    Animator.Play("Hurt");
                    HurtFlashTimer = 0.15f;
                    break;
                case EnemyVisualState.Dead:
                    Animator.Play("Death");
                    IsTelegraphing = false;
                    break;
            }
        }

        public void TakeDamage(float damage)
        {
            HurtFlashTimer = 0.15f;
            if (LiveData == null || !LiveData.IsAlive)
            {
                SetState(EnemyVisualState.Dead);
            }
            else
            {
                SetState(EnemyVisualState.Hurt);
            }
        }

        public void Update(float deltaTime)
        {
            if (LiveData != null)
            {
                VisualX += (LiveData.X - VisualX) * Math.Min(1f, 15f * deltaTime);
                VisualY += (LiveData.Y - VisualY) * Math.Min(1f, 15f * deltaTime);
            }

            Animator.Update(deltaTime);

            if (IsTelegraphing)
            {
                TelegraphTimer += deltaTime;
                TelegraphProgress = Math.Min(1.0f, TelegraphTimer / TelegraphDuration);
            }

            if (HurtFlashTimer > 0f)
            {
                HurtFlashTimer -= deltaTime;
                if (HurtFlashTimer <= 0f && LiveData != null && LiveData.IsAlive && CurrentState == EnemyVisualState.Hurt)
                {
                    SetState(EnemyVisualState.Idle);
                }
            }
        }
    }
}
