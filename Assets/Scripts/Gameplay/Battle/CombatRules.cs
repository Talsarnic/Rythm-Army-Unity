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

        // Formation pressure is a persistent battlefield state. It rises when
        // enemies compress the frontline and falls when the army regains space.
        public float FormationPressure = 0f;
        public float FormationIntegrity = 100f;
        public int FormationPressureTier = 0;
        public TerrainProfile CurrentTerrain;
        public TerrainType CurrentTerrainType = TerrainType.Ground;
        public List<CombatFeedbackEvent> PendingCombatFeedback = new List<CombatFeedbackEvent>();
    }

    public static class CombatRules
    {
        public const float MarchStepDistance = 140f;
        public const float RetreatStepDistance = 180f;
        public const float MeleeStrikeDistance = 35f;
        public const float FormationPressureStartDistance = 150f;
        public const float FormationPressureCriticalThreshold = 80f;
        public const float FormationCollisionDistance = 30f;

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
            state.FormationPressure = 0f;
            state.FormationIntegrity = 100f;
            state.FormationPressureTier = 0;
            state.CurrentTerrain = TerrainRules.GetProfile(mission, state.BannerX);
            state.CurrentTerrainType = state.CurrentTerrain.Type;
            state.PendingCombatFeedback.Clear();

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

            UpdateTerrainState(state);
            state.IsAirborne = (command == CommandId.Jump);
            state.IsDefending = (command == CommandId.Defend);

            // A jump lasts for the response/action window only. Every new
            // command re-establishes normal ground formation unless it jumps again.
            if (command != CommandId.Jump)
            {
                foreach (var unit in state.Units.Where(u => u.IsAlive))
                {
                    unit.Y = unit.FormationOffsetY;
                }
            }

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

            UpdateTerrainState(state);
            ResolveFormationCollisions(state);
            UpdateFormationPressure(state, command);
            ApplyTerrainHazards(state);
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
                    float multiplier = TerrainRules.GetMovementMultiplier(
                        TerrainRules.GetProfile(state.Mission, unit.X),
                        unit.Member.Class);
                    unit.X += delta * multiplier;
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
            state.IsCharged = false; // Charge is consumed by the attack phrase.

            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                float range = FormationSystem.GetEngagementRange(unit.Member.Class);
                var weapon = ItemDef.Get(unit.Member.WeaponId);

                var target = aliveEnemies
                    .Where(e => e.IsAlive && e.X >= unit.X - 25f && Math.Abs(e.X - unit.X) <= range + 30f)
                    .OrderBy(e => Math.Abs(e.X - unit.X))
                    .FirstOrDefault();

                if (target == null) continue;

                bool melee = range <= 120f;
                if (melee)
                {
                    // Patapon-style attack motion: frontline units surge into
                    // engagement distance instead of standing on formation slots.
                    float strikeX = target.X - Math.Max(18f, target.CollisionRadius + 12f);
                    float lunge = wasCharged ? 95f : 48f;
                    TerrainProfile terrain = TerrainRules.GetProfile(state.Mission, unit.X);
                    lunge *= TerrainRules.GetMovementMultiplier(terrain, unit.Member.Class);
                    unit.X = Math.Max(unit.X, Math.Min(strikeX, unit.X + lunge));
                    unit.IsRushing = wasCharged;
                }

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
                TerrainProfile attackTerrain = TerrainRules.GetProfile(state.Mission, unit.X);
                if (TerrainRules.IsRanged(unit.Member.Class))
                {
                    damage *= TerrainRules.GetRangedDamageMultiplier(attackTerrain);
                }
                if (wasCharged)
                {
                    damage *= 2.5f * TerrainRules.GetChargeMultiplier(attackTerrain);
                }

                target.CurrentHp = Math.Max(0f, target.CurrentHp - damage);

                // Knockback is a presentation-visible part of the hit, but it
                // also affects spacing so the frontline does not visually stack.
                if (!target.IsStructure)
                {
                    float direction = target.X >= unit.X ? 1f : -1f;
                    float knockback = Math.Min(wasCharged ? 42f : 26f, hit.Knockback * (wasCharged ? 0.65f : 0.45f));
                    target.X += direction * knockback;
                    state.PendingCombatFeedback.Add(CombatFeedbackEvent.EnemyHit(
                        target, damage, knockback, hit.IsCritical));
                    if (knockback > 0.5f)
                    {
                        state.PendingCombatFeedback.Add(new CombatFeedbackEvent
                        {
                            Type = CombatFeedbackType.EnemyKnockback,
                            Enemy = target,
                            Knockback = knockback,
                            WorldX = target.X,
                            WorldY = target.Y
                        });
                    }
                }
                else
                {
                    state.PendingCombatFeedback.Add(CombatFeedbackEvent.EnemyHit(
                        target, damage, 0f, hit.IsCritical));
                }

                if (!target.IsAlive)
                {
                    var drops = LootSystem.RollDrops(target.Kind, rng);
                    state.LootCollected.AddRange(drops);
                    state.CurrencyCollected += LootSystem.RollCurrencyDrop(target.Kind, rng);
                }
            }
        }

        private static void ExecuteDefend(BattleState state)
        {
            state.IsDefending = true;
            state.IsCharged = false;

            // Defend pulls the army back into its formation instead of freezing
            // every unit at whatever attack position it previously occupied.
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                unit.IsRushing = false;
                unit.X = state.BannerX + unit.FormationOffsetX;
                unit.Y = unit.FormationOffsetY;
            }
        }

        private static void ExecuteRetreat(BattleState state)
        {
            TerrainProfile terrain = TerrainRules.GetProfile(state.Mission, state.BannerX);
            float retreatDistance = RetreatStepDistance * terrain.MovementMultiplier;
            state.BannerX = Math.Max(50f, state.BannerX - retreatDistance);
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                unit.IsRushing = false;
                unit.X = state.BannerX + unit.FormationOffsetX;
                unit.Y = unit.FormationOffsetY;
            }
            state.IsCharged = false;
        }

        private static void ExecuteCharge(BattleState state)
        {
            state.IsCharged = true;

            // Charge is a movement command first. The following attack converts
            // this forward momentum into bonus damage.
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                TerrainProfile terrain = TerrainRules.GetProfile(state.Mission, unit.X);
                float distance = unit.Member.Class == UnitClass.Cavalry ||
                                 unit.Member.Class == UnitClass.Skyrider ? 145f : 85f;
                distance *= TerrainRules.GetMovementMultiplier(terrain, unit.Member.Class);
                distance *= TerrainRules.GetChargeMultiplier(terrain);
                unit.X += distance;
                unit.IsRushing = true;
            }

            TerrainProfile bannerTerrain = TerrainRules.GetProfile(state.Mission, state.BannerX);
            state.BannerX += 45f * TerrainRules.GetChargeMultiplier(bannerTerrain);
        }

        private static void ExecuteJump(BattleState state)
        {
            state.IsAirborne = true;
            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                unit.IsRushing = false;
                unit.X += 18f;
                unit.Y = unit.FormationOffsetY + 42f;
            }
        }

        private static void UpdateTerrainState(BattleState state)
        {
            TerrainProfile profile = TerrainRules.GetProfile(state.Mission, state.BannerX);
            if (profile.Type != state.CurrentTerrainType)
            {
                state.CurrentTerrain = profile;
                state.CurrentTerrainType = profile.Type;
                state.PendingCombatFeedback.Add(
                    CombatFeedbackEvent.TerrainChanged(profile));
            }
            else
            {
                state.CurrentTerrain = profile;
            }
        }

        private static void ApplyTerrainHazards(BattleState state)
        {
            TerrainProfile profile = TerrainRules.GetProfile(state.Mission, state.BannerX);
            state.CurrentTerrain = profile;

            if (profile.HeatDamagePercent <= 0f) return;

            foreach (var unit in state.Units.Where(u => u.IsAlive))
            {
                float damage = Math.Max(1f, unit.MaxHp * profile.HeatDamagePercent);
                unit.CurrentHp = Math.Max(0f, unit.CurrentHp - damage);
                state.PendingCombatFeedback.Add(
                    CombatFeedbackEvent.UnitHurt(unit, damage));
            }
        }

        private static void ResolveFormationCollisions(BattleState state)
        {
            var aliveUnits = state.Units.Where(u => u.IsAlive).ToList();
            var aliveEnemies = state.Enemies.Where(e => e.IsAlive && !e.IsStructure).ToList();

            foreach (var enemy in aliveEnemies)
            {
                foreach (var unit in aliveUnits)
                {
                    float distance = Math.Abs(enemy.X - unit.X);
                    float minDistance = Math.Max(FormationCollisionDistance, enemy.CollisionRadius + 12f);
                    if (distance >= minDistance) continue;

                    float overlap = minDistance - distance;
                    float direction = enemy.X >= unit.X ? 1f : -1f;

                    // Most of the correction is applied to the army. This makes
                    // a packed enemy line visibly push the vanguard backward.
                    unit.X -= direction * overlap * 0.72f;
                    enemy.X += direction * overlap * 0.28f;
                }
            }

            // Keep the banner behind the frontline and prevent a collision
            // correction from moving a unit into impossible negative space.
            foreach (var unit in aliveUnits)
            {
                if (unit.Member.Class == UnitClass.Banner)
                {
                    unit.X = state.BannerX;
                }
                else
                {
                    unit.X = Math.Max(state.BannerX - 180f, unit.X);
                }
            }
        }

        private static void UpdateFormationPressure(BattleState state, CommandId command)
        {
            var aliveEnemies = state.Enemies.Where(e => e.IsAlive && !e.IsStructure).ToList();
            var aliveUnits = state.Units.Where(u => u.IsAlive && u.Member.Class != UnitClass.Banner).ToList();

            float pressureInput = 0f;
            foreach (var enemy in aliveEnemies)
            {
                float frontDistance = enemy.X - state.BannerX;
                if (frontDistance <= FormationPressureStartDistance && frontDistance >= -90f)
                {
                    float weight = 1f;
                    var category = EnemyMetadata.GetCategory(enemy.Kind);
                    if (category == EnemyCategory.Boss) weight = 3f;
                    else if (enemy.Kind == EnemyKind.TribeHammerer ||
                             enemy.Kind == EnemyKind.TribeCavalry ||
                             enemy.Kind == EnemyKind.TribeSkyrider)
                        weight = 1.5f;

                    if (frontDistance < 35f) weight *= 1.35f;
                    pressureInput += weight;
                }

                if (enemy.X < state.BannerX + 15f)
                {
                    pressureInput += 2f; // Enemy has breached the banner line.
                }
            }

            foreach (var unit in aliveUnits)
            {
                int nearbyEnemies = aliveEnemies.Count(e => Math.Abs(e.X - unit.X) <= 75f);
                if (nearbyEnemies > 1)
                {
                    pressureInput += (nearbyEnemies - 1) * 1.25f;
                }
            }

            float response = pressureInput * 4.5f;
            if (command == CommandId.Defend) response *= 0.55f;
            else if (command == CommandId.Charge) response *= 0.80f;
            else if (command == CommandId.Retreat) response *= 0.35f;
            else if (command == CommandId.Attack) response *= 0.85f;

            // Space regained by the player's command is a meaningful relief.
            float recovery = command == CommandId.March ? 8f :
                             command == CommandId.Defend ? 12f :
                             command == CommandId.Retreat ? 18f : 5f;

            state.FormationPressure = Math.Max(0f, Math.Min(100f,
                state.FormationPressure + response - recovery));
            state.FormationIntegrity = 100f - state.FormationPressure;

            int tier = state.FormationPressure >= FormationPressureCriticalThreshold ? 3 :
                       state.FormationPressure >= 55f ? 2 :
                       state.FormationPressure >= 25f ? 1 : 0;

            if (tier != state.FormationPressureTier)
            {
                state.FormationPressureTier = tier;
                state.PendingCombatFeedback.Add(
                    CombatFeedbackEvent.FormationPressure(
                        state.FormationPressure, state.FormationIntegrity));
            }

            if (state.FormationPressure >= FormationPressureCriticalThreshold)
            {
                foreach (var unit in aliveUnits.Where(u => u.X > state.BannerX - 120f))
                {
                    float push = unit.Member.Class == UnitClass.Cavalry ||
                                 unit.Member.Class == UnitClass.Skyrider ? 2.5f : 4f;
                    unit.X = Math.Max(state.BannerX - 120f, unit.X - push);
                    unit.IsRushing = false;
                }
            }
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
