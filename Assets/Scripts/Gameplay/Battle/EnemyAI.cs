using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Gameplay.Battle
{
    public enum EnemyState
    {
        Idle,
        Approaching,
        Attacking,
        Telegraphing,
        Fleeing,
        Stunned,
        Dead
    }

    public enum NormalEnemyAttackType
    {
        Melee,
        RangedPhysical,
        RangedFire,
        Sonic
    }

    public enum BossAttackType
    {
        EarthquakeStomp, // Ground shockwave - Avoided by JUMP command
        FireBreath,      // Line AoE - Mitigated by DEFEND or RETREAT
        ChargeRush,      // Forward tackle - Blocked by DEFEND or evaded by RETREAT
        TailSwipe,       // Frontline sweep - Mitigated by DEFEND
        BoulderThrow     // Ballistic rock hitting rear ranks
    }

    public class BossTelegraph
    {
        public BossAttackType AttackType;
        public string Name;
        public int WindupBeatsTotal;
        public int WindupBeatsRemaining;
        public float Range;
        public float BaseDamage;
        public DamageElement Element;
        public string WarningHint;

        public BossTelegraph(BossAttackType type, string name, int windupBeats, float range, float baseDamage, DamageElement element, string hint)
        {
            AttackType = type;
            Name = name;
            WindupBeatsTotal = windupBeats;
            WindupBeatsRemaining = windupBeats;
            Range = range;
            BaseDamage = baseDamage;
            Element = element;
            WarningHint = hint;
        }
    }

    public enum StatusEffectType
    {
        Burn,
        Freeze,
        Stun,
        Charged,
        Defending,
        Airborne
    }

    public class ActiveStatusEffect
    {
        public StatusEffectType Type;
        public int BeatsRemaining;
        public float Power; // Damage tick or multiplier

        public ActiveStatusEffect(StatusEffectType type, int beatsRemaining, float power = 0f)
        {
            Type = type;
            BeatsRemaining = beatsRemaining;
            Power = power;
        }
    }

    public class EnemyCombatState
    {
        public LiveEnemy Enemy;
        public EnemyState State = EnemyState.Idle;
        public BossTelegraph ActiveTelegraph = null;
        public List<ActiveStatusEffect> StatusEffects = new List<ActiveStatusEffect>();
        public int AttackCooldown = 0;
        public int AttackWindupBeats = 0;
        public NormalEnemyAttackType PendingAttackType = NormalEnemyAttackType.Melee;
        public float PendingAttackDamage = 0f;
        public string PendingAttackName = null;
        public float AlertDistance = 350f;
        public bool HasFled = false;

        public EnemyCombatState(LiveEnemy enemy)
        {
            Enemy = enemy;
            var category = EnemyMetadata.GetCategory(enemy.Kind);
            if (category == EnemyCategory.Wildlife)
            {
                AlertDistance = 280f;
            }
            else if (category == EnemyCategory.Boss)
            {
                AlertDistance = 600f;
            }
        }

        public bool HasStatus(StatusEffectType type)
        {
            return StatusEffects.Any(s => s.Type == type && s.BeatsRemaining > 0);
        }

        public void AddStatus(StatusEffectType type, int durationBeats, float power = 0f)
        {
            var existing = StatusEffects.FirstOrDefault(s => s.Type == type);
            if (existing != null)
            {
                existing.BeatsRemaining = Math.Max(existing.BeatsRemaining, durationBeats);
                existing.Power = Math.Max(existing.Power, power);
            }
            else
            {
                StatusEffects.Add(new ActiveStatusEffect(type, durationBeats, power));
            }
        }

        public void TickStatusEffects()
        {
            for (int i = StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = StatusEffects[i];
                if (effect.Type == StatusEffectType.Burn)
                {
                    Enemy.CurrentHp = Math.Max(0f, Enemy.CurrentHp - effect.Power);
                }

                effect.BeatsRemaining--;
                if (effect.BeatsRemaining <= 0)
                {
                    StatusEffects.RemoveAt(i);
                }
            }

            if (Enemy.CurrentHp <= 0)
            {
                State = EnemyState.Dead;
            }
        }
    }

    public static class EnemyAISystem
    {
        internal static bool ResolvePendingNormalAttack(BattleState state, EnemyCombatState ai, Random rng)
        {
            if (ai.AttackWindupBeats <= 0) return false;
            ai.AttackWindupBeats--;
            ai.State = ai.AttackWindupBeats > 0 ? EnemyState.Telegraphing : EnemyState.Attacking;
            if (ai.AttackWindupBeats > 0) return true;

            switch (ai.PendingAttackType)
            {
                case NormalEnemyAttackType.RangedPhysical:
                    ExecuteEnemyRangedAttack(state, ai.Enemy, ai.PendingAttackDamage, DamageElement.Physical, rng);
                    break;
                case NormalEnemyAttackType.RangedFire:
                    ExecuteEnemyRangedAttack(state, ai.Enemy, ai.PendingAttackDamage, DamageElement.Fire, rng);
                    break;
                case NormalEnemyAttackType.Sonic:
                    ExecuteEnemySonicAttack(state, ai.Enemy, ai.PendingAttackDamage, rng);
                    break;
                default:
                    float damage = ai.Enemy.Kind == EnemyKind.TribeHammerer ? 18f :
                                   ai.Enemy.Kind == EnemyKind.TribeBrawler ? 15f :
                                   ai.Enemy.Kind == EnemyKind.TribeSkyrider ? 14f : 12f;
                    ExecuteEnemyMeleeAttack(state, ai.Enemy,
                        ai.PendingAttackDamage > 0f ? ai.PendingAttackDamage : damage, rng);
                    break;
            }

            ai.PendingAttackName = null;
            return true;
        }

        internal static void BeginNormalAttackTelegraph(BattleState state, EnemyCombatState ai, NormalEnemyAttackType type, string name)
        {
            ai.PendingAttackType = type;
            ai.PendingAttackName = name;
            ai.PendingAttackDamage = 0f;
            ai.AttackWindupBeats = 1;
            ai.State = EnemyState.Telegraphing;
            state.PendingCombatFeedback.Add(CombatFeedbackEvent.EnemyAttackTelegraph(ai.Enemy, name));
        }

        public static void ExecuteEnemyMeleeAttack(BattleState state, LiveEnemy enemy, float baseDamage, Random rng)
        {
            var target = state.Units.Where(u => u.IsAlive && u.Member.Class != UnitClass.Banner)
                .OrderBy(u => Math.Abs(u.X - enemy.X)).FirstOrDefault();
            if (target == null || Math.Abs(target.X - enemy.X) > 110f) return;
            ApplyEnemyDamage(state, enemy, target, baseDamage, DamageElement.Physical, rng);
        }

        public static void ExecuteEnemyRangedAttack(BattleState state, LiveEnemy enemy, float baseDamage, DamageElement element, Random rng)
        {
            var target = state.Units.Where(u => u.IsAlive && u.Member.Class != UnitClass.Banner)
                .OrderBy(u => Math.Abs(u.X - enemy.X)).FirstOrDefault();
            if (target == null || Math.Abs(target.X - enemy.X) > 520f) return;
            ApplyEnemyDamage(state, enemy, target, baseDamage, element, rng);
        }

        public static void ExecuteEnemySonicAttack(BattleState state, LiveEnemy enemy, float baseDamage, Random rng)
        {
            var targets = state.Units.Where(u => u.IsAlive && u.Member.Class != UnitClass.Banner &&
                                                  Math.Abs(u.X - enemy.X) <= 180f).ToList();
            foreach (var target in targets)
                ApplyEnemyDamage(state, enemy, target, baseDamage, DamageElement.Sonic, rng);
        }

        private static void ApplyEnemyDamage(BattleState state, LiveEnemy enemy, LiveUnit target, float baseDamage, DamageElement element, Random rng)
        {
            var stats = CombatFormulas.CalculateMemberStats(target.Member);
            float damage = baseDamage * (100f / (100f + Math.Max(0f, stats.Defense)));
            damage *= 0.90f + (float)rng.NextDouble() * 0.20f;
            if (state.IsDefending) damage *= 0.45f;
            damage = Math.Max(1f, (float)Math.Round(damage, 1));
            target.CurrentHp = Math.Max(0f, target.CurrentHp - damage);
            state.PendingCombatFeedback.Add(CombatFeedbackEvent.EnemyAttackImpact(enemy, target, damage));
        }

        public static void UpdateEnemyTurn(BattleState state, Dictionary<string, EnemyCombatState> enemyStates, Random rng = null)
        {
            rng = rng ?? new Random();

            foreach (var enemy in state.Enemies.Where(e => e.IsAlive))
            {
                EnemyCombatState ai;
                if (!enemyStates.TryGetValue(enemy.Id, out ai))
                {
                    ai = new EnemyCombatState(enemy);
                    enemyStates[enemy.Id] = ai;
                }

                ai.TickStatusEffects();
                if (!enemy.IsAlive) continue;

                if (ai.HasStatus(StatusEffectType.Stun) || ai.HasStatus(StatusEffectType.Freeze))
                {
                    ai.State = EnemyState.Stunned;
                    continue;
                }

                if (ResolvePendingNormalAttack(state, ai, rng))
                {
                    continue;
                }

                var category = EnemyMetadata.GetCategory(enemy.Kind);
                switch (category)
                {
                    case EnemyCategory.Wildlife:
                        UpdateWildlifeAI(state, ai);
                        break;
                    case EnemyCategory.RivalClan:
                        UpdateRivalClanAI(state, ai, rng);
                        break;
                    case EnemyCategory.Boss:
                        UpdateBossAI(state, ai, rng);
                        break;
                    case EnemyCategory.Fortification:
                        UpdateFortificationAI(state, ai, rng);
                        break;
                }
            }
        }

        private static void UpdateWildlifeAI(BattleState state, EnemyCombatState ai)
        {
            float distToArmy = ai.Enemy.X - state.BannerX;
            if (distToArmy <= ai.AlertDistance && distToArmy > 0)
            {
                ai.State = EnemyState.Fleeing;
                ai.Enemy.X += 50f; // Flees forward away from army

                if (state.Mission != null && ai.Enemy.X >= state.Mission.WorldLength + 100f)
                {
                    ai.HasFled = true;
                    ai.Enemy.CurrentHp = 0f; // Escaped battle area
                }
            }
            else
            {
                ai.State = EnemyState.Idle;
            }
        }

        private static void UpdateRivalClanAI(BattleState state, EnemyCombatState ai, Random rng)
        {
            float distToBanner = ai.Enemy.X - state.BannerX;

            if (ai.Enemy.Kind == EnemyKind.TribeArcher)
            {
                // Ranged attack
                if (distToBanner <= 450f && distToBanner >= 100f)
                {
                    BeginNormalAttackTelegraph(state, ai, NormalEnemyAttackType.RangedPhysical, "Arrow Volley");
                    ai.PendingAttackDamage = 8f;
                }
                else if (distToBanner > 450f)
                {
                    ai.State = EnemyState.Approaching;
                    ai.Enemy.X -= 30f;
                }
            }
            else if (ai.Enemy.Kind == EnemyKind.TribeMage)
            {
                // Arcane projectile attack
                if (distToBanner <= 400f && distToBanner >= 80f)
                {
                    BeginNormalAttackTelegraph(state, ai, NormalEnemyAttackType.RangedFire, "Arcane Bolt");
                    ai.PendingAttackDamage = 12f;
                }
                else if (distToBanner > 400f)
                {
                    ai.State = EnemyState.Approaching;
                    ai.Enemy.X -= 25f;
                }
            }
            else if (ai.Enemy.Kind == EnemyKind.TribeHornist)
            {
                // Sonic blast support / frontline AoE
                if (distToBanner <= 180f)
                {
                    BeginNormalAttackTelegraph(state, ai, NormalEnemyAttackType.Sonic, "Sonic Burst");
                    ai.PendingAttackDamage = 7f;
                }
                else if (distToBanner > 180f && distToBanner <= ai.AlertDistance)
                {
                    ai.State = EnemyState.Approaching;
                    ai.Enemy.X = Math.Max(state.BannerX + 40f, ai.Enemy.X - 30f);
                }
            }
            else if (ai.Enemy.Kind == EnemyKind.TribeBanner)
            {
                // Banner stands ground and inspires rival troops
                ai.State = EnemyState.Idle;
            }
            else
            {
                // Melee warriors (TribeSpearman, TribeSwordsman, TribeCavalry, TribeHammerer, TribeSkyrider, TribeBrawler)
                float attackReach = (ai.Enemy.Kind == EnemyKind.TribeSpearman) ? 120f : 80f;
                float attackDamage = 12f;

                if (ai.Enemy.Kind == EnemyKind.TribeHammerer) attackDamage = 18f;
                else if (ai.Enemy.Kind == EnemyKind.TribeBrawler) attackDamage = 15f;
                else if (ai.Enemy.Kind == EnemyKind.TribeSkyrider) attackDamage = 14f;

                if (distToBanner <= attackReach)
                {
                    BeginNormalAttackTelegraph(state, ai, NormalEnemyAttackType.Melee,
                        ai.Enemy.Kind == EnemyKind.TribeHammerer ? "Hammer Smash" : "Melee Strike");
                    ai.PendingAttackDamage = attackDamage;
                }
                else if (distToBanner > attackReach && distToBanner <= ai.AlertDistance)
                {
                    ai.State = EnemyState.Approaching;
                    float moveSpeed = (ai.Enemy.Kind == EnemyKind.TribeCavalry || ai.Enemy.Kind == EnemyKind.TribeSkyrider) ? 60f : 35f;
                    ai.Enemy.X = Math.Max(state.BannerX + 40f, ai.Enemy.X - moveSpeed);
                }
            }
        }

        private static void UpdateBossAI(BattleState state, EnemyCombatState ai, Random rng)
        {
            float distToBanner = ai.Enemy.X - state.BannerX;

            // If telegraph is active, count down beats
            if (ai.ActiveTelegraph != null)
            {
                ai.ActiveTelegraph.WindupBeatsRemaining--;
                ai.State = EnemyState.Telegraphing;

                if (ai.ActiveTelegraph.WindupBeatsRemaining <= 0)
                {
                    // Unleash attack!
                    UnleashBossAttack(state, ai, ai.ActiveTelegraph, rng);
                    ai.ActiveTelegraph = null;
                    ai.AttackCooldown = 2; // 2 measures/beats cooldown
                    ai.State = EnemyState.Idle;
                }
                return;
            }

            if (ai.AttackCooldown > 0)
            {
                ai.AttackCooldown--;
                ai.State = EnemyState.Idle;
                return;
            }

            // Decide new telegraph if in range
            if (distToBanner <= ai.AlertDistance)
            {
                BossTelegraph telegraph = ChooseBossTelegraph(ai.Enemy.Kind, rng);
                if (telegraph != null)
                {
                    ai.ActiveTelegraph = telegraph;
                    ai.State = EnemyState.Telegraphing;
                }
            }
        }

        private static void UpdateFortificationAI(BattleState state, EnemyCombatState ai, Random rng)
        {
            if (ai.Enemy.Kind == EnemyKind.Watchtower || ai.Enemy.Kind == EnemyKind.CatapultTower)
            {
                float dist = ai.Enemy.X - state.BannerX;
                if (dist <= 500f && dist >= 0f)
                {
                    float dmg = (ai.Enemy.Kind == EnemyKind.CatapultTower) ? 22f : 10f;
                    BeginNormalAttackTelegraph(state, ai,
                        NormalEnemyAttackType.RangedPhysical,
                        ai.Enemy.Kind == EnemyKind.CatapultTower ? "Catapult Barrage" : "Tower Shot");
                    ai.PendingAttackDamage = dmg;
                }
            }
        }

        public static BossTelegraph ChooseBossTelegraph(EnemyKind kind, Random rng)
        {
            switch (kind)
            {
                case EnemyKind.ColossusGolem:
                    return rng.Next(2) == 0
                        ? new BossTelegraph(BossAttackType.EarthquakeStomp, "Titan Quake", 2, 400f, 35f, DamageElement.Physical, "JUMP to dodge ground tremor!")
                        : new BossTelegraph(BossAttackType.BoulderThrow, "Boulder Barrage", 1, 600f, 25f, DamageElement.Physical, "DEFEND to block boulders!");

                case EnemyKind.DrakeTitan:
                    return rng.Next(2) == 0
                        ? new BossTelegraph(BossAttackType.FireBreath, "Inferno Breath", 2, 450f, 40f, DamageElement.Fire, "RETREAT or DEFEND to avoid inferno!")
                        : new BossTelegraph(BossAttackType.TailSwipe, "Draconic Sweep", 1, 150f, 30f, DamageElement.Physical, "DEFEND frontline!");

                case EnemyKind.IronBehemoth:
                    return rng.Next(2) == 0
                        ? new BossTelegraph(BossAttackType.ChargeRush, "Rampage Charge", 2, 350f, 38f, DamageElement.Physical, "DEFEND to brace or RETREAT!")
                        : new BossTelegraph(BossAttackType.TailSwipe, "Iron Bash", 1, 120f, 25f, DamageElement.Physical, "DEFEND against bash!");

                default:
                    return null;
            }
        }

        public static void UnleashBossAttack(BattleState state, EnemyCombatState ai, BossTelegraph telegraph, Random rng)
        {
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                float dist = Math.Abs(ai.Enemy.X - unit.X);
                if (dist <= telegraph.Range)
                {
                    float dmg = telegraph.BaseDamage;

                    // Check defensive counters
                    if (telegraph.AttackType == BossAttackType.EarthquakeStomp)
                    {
                        // Avoided completely if jumping (Airborne)
                        bool isAirborne = state.IsAirborne;
                        if (isAirborne)
                        {
                            continue; // Dodged!
                        }
                    }

                    // Apply damage reduction if player defended
                    var shield = ItemDef.Get(unit.Member.ShieldId);
                    float defBonus = shield != null ? shield.Defense : 0f;
                    float finalDmg = Math.Max(1f, dmg - defBonus);
                    if (state.IsDefending)
                    {
                        finalDmg *= 0.5f;
                    }

                    unit.CurrentHp = Math.Max(0f, unit.CurrentHp - finalDmg);
                    state.PendingCombatFeedback.Add(
                        CombatFeedbackEvent.EnemyAttackImpact(ai.Enemy, unit, finalDmg));
                }
            }
        }

        private static void ExecuteEnemyMeleeAttack(BattleState state, LiveEnemy enemy, float baseDmg, Random rng)
        {
            // Attacks frontline units (Swordsman, Brawler, Hammerer)
            var target = state.Units.Where(u => u.IsAlive).OrderBy(u => Math.Abs(u.X - enemy.X)).FirstOrDefault();
            if (target != null)
            {
                var shield = ItemDef.Get(target.Member.ShieldId);
                float def = shield != null ? shield.Defense : 0f;
                float finalDmg = Math.Max(1f, baseDmg - def);
                target.CurrentHp = Math.Max(0f, target.CurrentHp - finalDmg);
                state.PendingCombatFeedback.Add(
                    CombatFeedbackEvent.EnemyAttackImpact(enemy, target, finalDmg));
            }
        }

        private static void ExecuteEnemySonicAttack(BattleState state, LiveEnemy enemy, float baseDmg, Random rng)
        {
            // Sonic attack waves hit frontline units in range
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                if (Math.Abs(unit.X - enemy.X) <= 150f)
                {
                    var shield = ItemDef.Get(unit.Member.ShieldId);
                    float def = shield != null ? shield.Defense : 0f;
                    float finalDmg = Math.Max(1f, baseDmg - def * 0.5f);
                    unit.CurrentHp = Math.Max(0f, unit.CurrentHp - finalDmg);
                    state.PendingCombatFeedback.Add(
                        CombatFeedbackEvent.EnemyAttackImpact(enemy, unit, finalDmg));
                }
            }
        }

        private static void ExecuteEnemyRangedAttack(BattleState state, LiveEnemy enemy, float baseDmg, DamageElement element, Random rng)
        {
            var aliveUnits = state.Units.Where(u => u.IsAlive).ToList();
            if (aliveUnits.Count == 0) return;

            var target = aliveUnits[rng.Next(aliveUnits.Count)];
            var helm = ItemDef.Get(target.Member.HelmetId);
            float def = helm != null ? helm.Defense : 0f;
            float finalDmg = Math.Max(1f, baseDmg - def);
            target.CurrentHp = Math.Max(0f, target.CurrentHp - finalDmg);
            state.PendingCombatFeedback.Add(
                CombatFeedbackEvent.EnemyAttackImpact(enemy, target, finalDmg));
        }
    }
}
