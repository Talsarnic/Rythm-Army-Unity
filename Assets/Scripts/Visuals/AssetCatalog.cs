using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Visuals
{
    public enum AnimationClipType
    {
        Idle,
        March,
        Attack,
        Defend,
        Fever,
        Charge,
        Jump,
        Hurt,
        Death,
        Telegraph,
        Roar,
        Cast
    }

    public class HitboxBounds
    {
        public float OffsetX;
        public float OffsetY;
        public float Width;
        public float Height;

        public HitboxBounds(float offsetX, float offsetY, float width, float height)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Width = width;
            Height = height;
        }
    }

    public class UnitAnimationMeta
    {
        public UnitClass Class;
        public int SpriteWidth;
        public int SpriteHeight;
        public HitboxBounds Hurtbox;
        public HitboxBounds Hitbox;
        public Dictionary<AnimationClipType, AnimationStateDef> ClipStates;

        public UnitAnimationMeta(UnitClass unitClass, int width, int height, HitboxBounds hurtbox, HitboxBounds hitbox)
        {
            Class = unitClass;
            SpriteWidth = width;
            SpriteHeight = height;
            Hurtbox = hurtbox;
            Hitbox = hitbox;
            ClipStates = new Dictionary<AnimationClipType, AnimationStateDef>();
        }
    }

    public class EnemyAnimationMeta
    {
        public EnemyKind Kind;
        public int SpriteWidth;
        public int SpriteHeight;
        public HitboxBounds Hurtbox;
        public HitboxBounds Hitbox;
        public Dictionary<AnimationClipType, AnimationStateDef> ClipStates;

        public EnemyAnimationMeta(EnemyKind kind, int width, int height, HitboxBounds hurtbox, HitboxBounds hitbox)
        {
            Kind = kind;
            SpriteWidth = width;
            SpriteHeight = height;
            Hurtbox = hurtbox;
            Hitbox = hitbox;
            ClipStates = new Dictionary<AnimationClipType, AnimationStateDef>();
        }
    }

    /// <summary>
    /// Master Sprite and Animation Asset Catalog specifying Moonlighter-styled
    /// frame dimensions, texture atlas slicing, animation event triggers, and collision boxes.
    /// </summary>
    public static class AssetCatalog
    {
        private static readonly Dictionary<UnitClass, UnitAnimationMeta> _unitMetaRegistry = new Dictionary<UnitClass, UnitAnimationMeta>();
        private static readonly Dictionary<EnemyKind, EnemyAnimationMeta> _enemyMetaRegistry = new Dictionary<EnemyKind, EnemyAnimationMeta>();
        private static readonly Dictionary<string, PixelRect> _itemIconSlices = new Dictionary<string, PixelRect>();

        static AssetCatalog()
        {
            InitializeUnitMeta();
            InitializeEnemyMeta();
            InitializeItemIcons();
        }

        public static UnitAnimationMeta GetUnitMeta(UnitClass unitClass)
        {
            UnitAnimationMeta meta;
            if (_unitMetaRegistry.TryGetValue(unitClass, out meta)) return meta;
            return _unitMetaRegistry[UnitClass.Spearman];
        }

        public static EnemyAnimationMeta GetEnemyMeta(EnemyKind kind)
        {
            EnemyAnimationMeta meta;
            if (_enemyMetaRegistry.TryGetValue(kind, out meta)) return meta;
            return _enemyMetaRegistry[EnemyKind.PlainsRunner];
        }

        public static bool TryGetItemIconSlice(string itemId, out PixelRect slice)
        {
            return _itemIconSlices.TryGetValue(itemId, out slice);
        }

        private static void InitializeUnitMeta()
        {
            foreach (UnitClass unitClass in Enum.GetValues(typeof(UnitClass)))
            {
                var spec = PixelSpriteRegistry.GetSpec(unitClass);
                int size = spec.SpriteWidth;

                var hurtbox = new HitboxBounds(0f, 0f, size * 0.75f, size * 0.90f);
                var hitbox = new HitboxBounds(size * 0.5f, 0f, size * 0.60f, size * 0.80f);

                var meta = new UnitAnimationMeta(unitClass, size, spec.SpriteHeight, hurtbox, hitbox);

                // Idle
                var idle = new AnimationStateDef("Idle", true);
                for (int i = 0; i < 4; i++) idle.AddFrame(i * size, 0, size, size, 0.15f);
                meta.ClipStates[AnimationClipType.Idle] = idle;

                // March
                var march = new AnimationStateDef("March", true);
                march.AddFrame(0, size, size, size, 0.125f, "Step");
                march.AddFrame(size, size, size, size, 0.125f);
                march.AddFrame(size * 2, size, size, size, 0.125f, "Step");
                march.AddFrame(size * 3, size, size, size, 0.125f);
                meta.ClipStates[AnimationClipType.March] = march;

                // Attack
                var attack = new AnimationStateDef("Attack", false);
                attack.AddFrame(0, size * 2, size, size, 0.10f, "Windup");
                attack.AddFrame(size, size * 2, size, size, 0.08f, "Release");
                attack.AddFrame(size * 2, size * 2, size, size, 0.12f, "Impact");
                attack.AddFrame(size * 3, size * 2, size, size, 0.15f);
                meta.ClipStates[AnimationClipType.Attack] = attack;

                // Defend
                var defend = new AnimationStateDef("Defend", true);
                defend.AddFrame(0, size * 3, size, size, 0.25f, "ShieldWall");
                defend.AddFrame(size, size * 3, size, size, 0.25f);
                meta.ClipStates[AnimationClipType.Defend] = defend;

                // Fever
                var fever = new AnimationStateDef("Fever", true);
                for (int i = 0; i < 4; i++) fever.AddFrame(i * size, size * 4, size, size, 0.10f, i % 2 == 0 ? "Chant" : null);
                meta.ClipStates[AnimationClipType.Fever] = fever;

                // Charge
                var charge = new AnimationStateDef("Charge", true);
                for (int i = 0; i < 3; i++) charge.AddFrame(i * size, size * 5, size, size, 0.08f, "ChargeDust");
                meta.ClipStates[AnimationClipType.Charge] = charge;

                // Jump
                var jump = new AnimationStateDef("Jump", false);
                jump.AddFrame(0, size * 6, size, size, 0.10f, "JumpLaunch");
                jump.AddFrame(size, size * 6, size, size, 0.20f, "ApexAir");
                jump.AddFrame(size * 2, size * 6, size, size, 0.10f, "LandImpact");
                meta.ClipStates[AnimationClipType.Jump] = jump;

                // Hurt
                var hurt = new AnimationStateDef("Hurt", false);
                hurt.AddFrame(0, size * 7, size, size, 0.10f);
                hurt.AddFrame(size, size * 7, size, size, 0.15f);
                meta.ClipStates[AnimationClipType.Hurt] = hurt;

                // Death
                var death = new AnimationStateDef("Death", false);
                death.AddFrame(0, size * 8, size, size, 0.12f);
                death.AddFrame(size, size * 8, size, size, 0.15f);
                death.AddFrame(size * 2, size * 8, size, size, 0.30f, "VanishCorpse");
                meta.ClipStates[AnimationClipType.Death] = death;

                _unitMetaRegistry[unitClass] = meta;
            }
        }

        private static void InitializeEnemyMeta()
        {
            foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
            {
                int size = 32;
                if (kind == EnemyKind.IronBehemoth || kind == EnemyKind.DrakeTitan || kind == EnemyKind.ColossusGolem) size = 96;
                else if (kind == EnemyKind.WildBoar || kind == EnemyKind.GiantBoar || kind == EnemyKind.StoneWall || kind == EnemyKind.Watchtower || kind == EnemyKind.CatapultTower) size = 64;
                else if (kind == EnemyKind.TribeCavalry || kind == EnemyKind.TribeHammerer || kind == EnemyKind.TribeSkyrider) size = 48;

                var hurtbox = new HitboxBounds(0f, 0f, size * 0.85f, size * 0.90f);
                var hitbox = new HitboxBounds(-size * 0.3f, 0f, size * 0.70f, size * 0.80f);

                var meta = new EnemyAnimationMeta(kind, size, size, hurtbox, hitbox);

                var idle = new AnimationStateDef("Idle", true);
                for (int i = 0; i < 4; i++) idle.AddFrame(i * size, 0, size, size, 0.20f);
                meta.ClipStates[AnimationClipType.Idle] = idle;

                var march = new AnimationStateDef("March", true);
                for (int i = 0; i < 4; i++) march.AddFrame(i * size, size, size, size, 0.15f, "Step");
                meta.ClipStates[AnimationClipType.March] = march;

                var attack = new AnimationStateDef("Attack", false);
                attack.AddFrame(0, size * 2, size, size, 0.12f, "Windup");
                attack.AddFrame(size, size * 2, size, size, 0.08f, "Impact");
                attack.AddFrame(size * 2, size * 2, size, size, 0.20f);
                meta.ClipStates[AnimationClipType.Attack] = attack;

                if (size >= 64)
                {
                    var telegraph = new AnimationStateDef("Telegraph", true);
                    telegraph.AddFrame(0, size * 3, size, size, 0.15f, "WarningFlash");
                    telegraph.AddFrame(size, size * 3, size, size, 0.15f);
                    meta.ClipStates[AnimationClipType.Telegraph] = telegraph;

                    var roar = new AnimationStateDef("Roar", false);
                    roar.AddFrame(0, size * 4, size, size, 0.20f, "ScreenShake");
                    roar.AddFrame(size, size * 4, size, size, 0.30f);
                    meta.ClipStates[AnimationClipType.Roar] = roar;
                }

                var hurt = new AnimationStateDef("Hurt", false);
                hurt.AddFrame(0, size * 5, size, size, 0.10f);
                hurt.AddFrame(size, size * 5, size, size, 0.15f);
                meta.ClipStates[AnimationClipType.Hurt] = hurt;

                var death = new AnimationStateDef("Death", false);
                death.AddFrame(0, size * 6, size, size, 0.15f);
                death.AddFrame(size, size * 6, size, size, 0.20f);
                death.AddFrame(size * 2, size * 6, size, size, 0.35f, "LootBurst");
                meta.ClipStates[AnimationClipType.Death] = death;

                _enemyMetaRegistry[kind] = meta;
            }
        }

        private static void InitializeItemIcons()
        {
            // 24x24 px Moonlighter UI icon atlas offsets
            _itemIconSlices["wpn-wooden-spear"] = new PixelRect(0, 0, 24, 24);
            _itemIconSlices["wpn-iron-spear"] = new PixelRect(24, 0, 24, 24);
            _itemIconSlices["wpn-fire-spear"] = new PixelRect(48, 0, 24, 24);
            _itemIconSlices["wpn-short-sword"] = new PixelRect(72, 0, 24, 24);
            _itemIconSlices["wpn-steel-sword"] = new PixelRect(96, 0, 24, 24);
            _itemIconSlices["wpn-wooden-bow"] = new PixelRect(120, 0, 24, 24);
            _itemIconSlices["wpn-iron-bow"] = new PixelRect(144, 0, 24, 24);
            _itemIconSlices["shd-wooden-shield"] = new PixelRect(0, 24, 24, 24);
            _itemIconSlices["shd-iron-shield"] = new PixelRect(24, 24, 24, 24);
            _itemIconSlices["hlm-leather-cap"] = new PixelRect(48, 24, 24, 24);
            _itemIconSlices["hlm-iron-helm"] = new PixelRect(72, 24, 24, 24);
            _itemIconSlices["mat-timber"] = new PixelRect(0, 48, 24, 24);
            _itemIconSlices["mat-stone"] = new PixelRect(24, 48, 24, 24);
            _itemIconSlices["mat-iron-slag"] = new PixelRect(48, 48, 24, 24);
            _itemIconSlices["mat-mithril"] = new PixelRect(72, 48, 24, 24);
            _itemIconSlices["mat-dragon-scale"] = new PixelRect(96, 48, 24, 24);
        }
    }
}
