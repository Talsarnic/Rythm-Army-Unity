using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Gameplay.Battle
{
    public enum BattlePhase
    {
        Starting,
        Marching,
        Engaged,
        Victory,
        Defeat
    }

    public class LiveUnit
    {
        public UnitMember Member;
        public float X;
        public float Y;
        public float CurrentHp;
        public float MaxHp;
        public bool IsAlive
        {
            get { return CurrentHp > 0; }
        }
        public bool IsRushing;
        public bool IsBurning;
        public bool IsHeroMode;
        public float FormationOffsetX;
        public float FormationOffsetY;
    }

    public class LiveEnemy
    {
        public string Id;
        public EnemyKind Kind;
        public float X;
        public float Y;
        public float CurrentHp;
        public float MaxHp;
        public float Defense;
        public float CollisionRadius;
        public bool IsStructure;
        public bool IsAlive
        {
            get { return CurrentHp > 0; }
        }
    }

    public class BattleState
    {
        public MissionDef Mission;
        public BattlePhase Phase = BattlePhase.Marching;
        public float ArmyFrontX = 0f;
        public float BannerX = 0f;
        public List<LiveUnit> Units = new List<LiveUnit>();
        public List<LiveEnemy> Enemies = new List<LiveEnemy>();
        public Dictionary<string, EnemyCombatState> EnemyStates = new Dictionary<string, EnemyCombatState>();
        public List<LootRewardSummary> LootCollected = new List<LootRewardSummary>();
        public int CurrencyCollected = 0;
        public int CurrentWaveIndex = 0;
        public int TotalCommandsIssued = 0;
        public int BestComboReached = 0;
        public bool FeverActive = false;
        public bool IsCharged = false;
        public bool IsAirborne = false;
        public bool IsDefending = false;
        public ItemDef ActiveMealBuff = null;
        public string DefeatReason = null;
    }

    public static class CombatRules
    {
        public const float MarchStepDistance = 140f;
        public const float RetreatStepDistance = 180f;
        public const float MeleeStrikeDistance = 35f;

        public static void InitializeBattle(BattleState state, MissionDef mission, List<UnitMember> roster, ItemDef mealBuff = null)
        {
            state.Mission = mission;
            state.Phase = BattlePhase.Marching;
            state.ArmyFrontX = 100f;
            state.BannerX = 100f;
            state.Units.Clear();
            state.Enemies.Clear();
            state.CurrentWaveIndex = 0;
            state.ActiveMealBuff = mealBuff;

            // Group by class to calculate squad formation slots
            var classGroups = roster.GroupBy(u => u.Class);
            foreach (var group in classGroups)
            {
                var list = group.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var unit = list[i];
                    var slot = FormationSystem.GetUnitSlot(unit.Class, i, list.Count);
                    float hp = CombatFormulas.CalculateBaseHealth(unit.Class, unit.Level, unit.Subspecies);
                    if (mealBuff != null)
                    {
                        hp += mealBuff.HealthBonus;
                    }

                    state.Units.Add(new LiveUnit
                    {
                        Member = unit,
                        X = state.BannerX + slot.OffsetX,
                        Y = slot.OffsetY,
                        CurrentHp = hp,
                        MaxHp = hp,
                        FormationOffsetX = slot.OffsetX,
                        FormationOffsetY = slot.OffsetY
                    });
                }
            }

            // Spawn first wave
            SpawnNextWave(state);
        }

        public static void SpawnNextWave(BattleState state)
        {
            if (state.Mission == null || state.CurrentWaveIndex >= state.Mission.Waves.Count) return;

            var wave = state.Mission.Waves[state.CurrentWaveIndex++];
            float spawnX = wave.AtX;

            foreach (var enemyGroup in wave.Enemies)
            {
                for (int i = 0; i < enemyGroup.Count; i++)
                {
                    var category = EnemyMetadata.GetCategory(enemyGroup.Kind);
                    bool isStructure = category == EnemyCategory.Fortification;
                    float baseHp = isStructure ? 350f : (category == EnemyCategory.Boss ? 1500f : 80f);
                    float def = isStructure ? 12f : 5f;

                    state.Enemies.Add(new LiveEnemy
                    {
                        Id = Guid.NewGuid().ToString(),
                        Kind = enemyGroup.Kind,
                        X = spawnX + (i * enemyGroup.SpawnOffset),
                        Y = 0f,
                        CurrentHp = baseHp,
                        MaxHp = baseHp,
                        Defense = def,
                        CollisionRadius = EnemyMetadata.GetCollisionRadius(enemyGroup.Kind),
                        IsStructure = isStructure
                    });
                }
            }
        }

        public static void ExecuteCommand(BattleState state, CommandId command, bool fever, Random rng = null)
        {
            rng = rng ?? new Random();
            state.TotalCommandsIssued++;
            state.FeverActive = fever;
            state.IsAirborne = (command == CommandId.Jump);
            state.IsDefending = (command == CommandId.Defend);

            switch (command)
            {
                case CommandId.March:
                    ExecuteMarch(state);
                    break;
                case CommandId.Attack:
                    ExecuteAttack(state, rng);
                    break;
                case CommandId.Defend:
                    ExecuteDefend(state);
                    break;
                case CommandId.Retreat:
                    ExecuteRetreat(state);
                    break;
                case CommandId.Charge:
                    ExecuteCharge(state);
                    break;
                case CommandId.Jump:
                    ExecuteJump(state);
                    break;
                case CommandId.Miracle:
                    // Miracle invocation does not move army, but readies elemental divine invocation
                    break;
            }

            // Enemy AI Turn
            EnemyAISystem.UpdateEnemyTurn(state, state.EnemyStates, rng);

            CheckBattleConditions(state);
        }

        private static void ExecuteMarch(BattleState state)
        {
            // Find nearest alive enemy blocking path
            var nearestEnemy = state.Enemies.Where(e => e.IsAlive && e.X > state.BannerX).OrderBy(e => e.X).FirstOrDefault();
            float targetX = state.BannerX + MarchStepDistance;

            if (nearestEnemy != null)
            {
                float vanguardOffset = state.Units.Count > 0 ? state.Units.Max(u => u.FormationOffsetX) : 0f;
                float stoppingDistance = nearestEnemy.CollisionRadius + vanguardOffset + 15f;
                if (targetX > nearestEnemy.X - stoppingDistance)
                {
                    targetX = nearestEnemy.X - stoppingDistance;
                }
            }

            float delta = targetX - state.BannerX;
            if (delta > 0)
            {
                state.BannerX = targetX;
                foreach (var unit in state.Units)
                {
                    unit.X = state.BannerX + unit.FormationOffsetX;
                }
            }

            // Check wave trigger
            if (state.Mission != null && state.CurrentWaveIndex < state.Mission.Waves.Count)
            {
                var nextWave = state.Mission.Waves[state.CurrentWaveIndex];
                if (state.BannerX >= nextWave.AtX - 500f)
                {
                    SpawnNextWave(state);
                }
            }
        }

        private static void ExecuteAttack(BattleState state, Random rng)
        {
            var aliveEnemies = state.Enemies.Where(e => e.IsAlive).ToList();
            if (aliveEnemies.Count == 0) return;

            bool wasCharged = state.IsCharged;
            state.IsCharged = false; // Charge consumed on attack

            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                float range = FormationSystem.GetEngagementRange(unit.Member.Class);
                var weapon = ItemDef.Get(unit.Member.WeaponId);

                // Find enemy in range
                var target = aliveEnemies
                    .Where(e => e.IsAlive && Math.Abs(e.X - unit.X) <= range + 30f)
                    .OrderBy(e => Math.Abs(e.X - unit.X))
                    .FirstOrDefault();

                if (target != null)
                {
                    var hit = CombatFormulas.CalculateDamage(
                        unit.Member,
                        weapon,
                        target.Defense,
                        target.IsStructure,
                        false,
                        state.FeverActive,
                        rng,
                        state.ActiveMealBuff
                    );

                    float damage = hit.FinalDamage;
                    if (wasCharged)
                    {
                        damage *= 2.5f; // Charge attack boost
                    }

                    target.CurrentHp = Math.Max(0f, target.CurrentHp - damage);

                    // Check if killed by this hit to roll loot drops & currency
                    if (!target.IsAlive)
                    {
                        var drops = LootSystem.RollDrops(target.Kind, rng);
                        state.LootCollected.AddRange(drops);
                        state.CurrencyCollected += LootSystem.RollCurrencyDrop(target.Kind, rng);
                    }
                }
            }
        }

        private static void ExecuteDefend(BattleState state)
        {
            state.IsDefending = true;
        }

        private static void ExecuteRetreat(BattleState state)
        {
            state.BannerX = Math.Max(50f, state.BannerX - RetreatStepDistance);
            foreach (var unit in state.Units)
            {
                unit.X = state.BannerX + unit.FormationOffsetX;
            }
        }

        private static void ExecuteCharge(BattleState state)
        {
            state.IsCharged = true;
        }

        private static void ExecuteJump(BattleState state)
        {
            state.IsAirborne = true;
        }

        public static void CheckBattleConditions(BattleState state)
        {
            // Check Defeat (Banner fallen or all units dead)
            var banner = state.Units.FirstOrDefault(u => u.Member.Class == UnitClass.Banner);
            if (banner == null || !banner.IsAlive || state.Units.All(u => !u.IsAlive))
            {
                state.Phase = BattlePhase.Defeat;
                state.DefeatReason = "The banner has fallen!";
                return;
            }

            // Check Victory (Reached goal line or cleared all waves)
            if (state.Mission != null)
            {
                bool allEnemiesDead = state.Enemies.All(e => !e.IsAlive);
                bool reachedGoal = state.BannerX >= state.Mission.GoalX;

                if (reachedGoal || (state.CurrentWaveIndex >= state.Mission.Waves.Count && allEnemiesDead))
                {
                    state.Phase = BattlePhase.Victory;
                    // Add mission base rewards
                    if (state.Mission.BaseRewards != null)
                    {
                        state.LootCollected.AddRange(state.Mission.BaseRewards);
                    }
                }
            }
        }
    }
}
