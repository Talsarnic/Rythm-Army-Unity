using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.UI;
using RhythmArmy.Visuals;

namespace RhythmArmy.Gameplay.Battle
{
    public struct DamageNumberEvent
    {
        public float WorldX;
        public float WorldY;
        public float Damage;
        public bool IsCritical;
        public bool IsHeal;
    }

    /// <summary>
    /// Master Battle Scene Controller orchestrating simulation, views, inputs, HUD state, and visual presentation.
    /// Acts as the bridge between Unity scene GameObjects/monobehaviours and core game simulation systems.
    /// </summary>
    public class BattleController
    {
        public BattleManager BattleManager { get; private set; }
        public EnvironmentView Environment { get; private set; }
        public PixelCamera Camera
        {
            get { return BattleManager != null ? BattleManager.Camera : null; }
        }
        public BattleHUDState HUDState { get; private set; }

        public List<UnitView> UnitViews { get; private set; }
        public List<EnemyView> EnemyViews { get; private set; }
        public List<ProjectileView> ProjectileViews { get; private set; }

        public List<DamageNumberEvent> PendingDamageNumbers { get; private set; }

        public event Action<DamageNumberEvent> OnDamageNumberSpawned;
        public event Action<BattleResult> OnBattleEnded;
        public event Action<TerrainProfile> OnTerrainChanged;

        public bool IsActive
        {
            get
            {
                return BattleManager != null && BattleManager.State != null &&
                    (BattleManager.State.Phase == BattlePhase.Marching ||
                     BattleManager.State.Phase == BattlePhase.Engaged ||
                     BattleManager.State.Phase == BattlePhase.Starting);
            }
        }

        public BattleController()
        {
            BattleManager = new BattleManager();
            Environment = new EnvironmentView();
            HUDState = new BattleHUDState();
            UnitViews = new List<UnitView>();
            EnemyViews = new List<EnemyView>();
            ProjectileViews = new List<ProjectileView>();
            PendingDamageNumbers = new List<DamageNumberEvent>();

            BattleManager.OnCommandEvaluated += HandleCommandEvaluated;
            BattleManager.OnEnemySlain += HandleEnemySlain;
            BattleManager.OnCombatFeedback += HandleCombatFeedback;
        }

        public void StartBattle(MissionDef mission, List<UnitMember> roster)
        {
            if (mission == null) throw new ArgumentNullException("mission");
            if (roster == null || roster.Count == 0) throw new ArgumentException("Roster cannot be empty", "roster");

            // Set biome environment
            var biome = EnvironmentSystem.ParseBiome(mission.Biome);
            Environment.SetBiome(biome);

            // Start battle simulation
            BattleManager.StartMission(mission, roster);

            // Instantiate Unit Views
            UnitViews.Clear();
            foreach (var unit in BattleManager.State.Units)
            {
                UnitViews.Add(new UnitView(unit));
            }

            // Instantiate initial Enemy Views
            EnemyViews.Clear();
            foreach (var enemy in BattleManager.State.Enemies)
            {
                EnemyViews.Add(new EnemyView(enemy));
            }

            ProjectileViews.Clear();
            PendingDamageNumbers.Clear();

            // Synchronize HUD
            HUDState.UpdateFromBattleManager(BattleManager, 0f);
        }

        public TapJudgment HandleInput(DrumId drum)
        {
            if (!IsActive) return new TapJudgment(Grade.Miss, 0f, 0, drum);

            var judgment = BattleManager.HandleDrumInput(drum);
            HUDState.RecordDrumTap(drum, judgment.Grade, judgment.DeltaMs);

            return judgment;
        }

        public void Update(float deltaTime)
        {
            if (BattleManager == null || BattleManager.State == null) return;

            // 1. Update Simulation
            BattleManager.Update(deltaTime);

            // 2. Synchronize Enemy Views (handle wave spawns)
            SyncEnemyViews();

            // 3. Update Presentation Views
            foreach (var uv in UnitViews)
            {
                uv.IsFeverActive = BattleManager.Rhythm.IsFever;
                uv.Update(deltaTime);
            }

            foreach (var ev in EnemyViews)
            {
                ev.Update(deltaTime);
            }

            // 4. Update Projectiles
            for (int i = ProjectileViews.Count - 1; i >= 0; i--)
            {
                var proj = ProjectileViews[i];
                proj.Update(deltaTime);
                if (proj.IsFinished)
                {
                    ProjectileViews.RemoveAt(i);
                }
            }

            // 5. Update Camera and Environment
            if (Camera != null)
            {
                Environment.Update(Camera.CurrentX, Camera.CurrentY, deltaTime);
            }

            // 6. Update HUD State
            HUDState.UpdateFromBattleManager(BattleManager, deltaTime);

            // 7. Check Battle End
            if (BattleManager.State.Phase == BattlePhase.Victory || BattleManager.State.Phase == BattlePhase.Defeat)
            {
                var result = BattleManager.FinishBattle();
                if (result != null && OnBattleEnded != null)
                {
                    OnBattleEnded(result);
                }
            }
        }

        private void SyncEnemyViews()
        {
            var liveEnemies = BattleManager.State.Enemies;

            // Check for newly spawned enemies in wave
            foreach (var le in liveEnemies)
            {
                if (!EnemyViews.Any(ev => ev.LiveData.Id == le.Id))
                {
                    EnemyViews.Add(new EnemyView(le));
                }
            }

            // Check for boss telegraphs
            foreach (var ev in EnemyViews)
            {
                EnemyCombatState aiState;
                if (BattleManager.EnemyAIStates.TryGetValue(ev.LiveData.Id, out aiState))
                {
                    if (aiState.AttackWindupBeats > 0 && !ev.IsTelegraphing)
                    {
                        ev.StartNormalTelegraph(aiState.PendingAttackName, 1.0f);
                    }
                    else if (aiState.ActiveTelegraph != null && !ev.IsTelegraphing)
                    {
                        ev.StartTelegraph(aiState.ActiveTelegraph.AttackType, aiState.ActiveTelegraph.WindupBeatsRemaining * 0.5f);
                    }
                    else if (aiState.ActiveTelegraph == null && ev.IsTelegraphing)
                    {
                        ev.ClearTelegraph();
                    }
                }
            }
        }

        private void HandleCombatFeedback(CombatFeedbackEvent feedback)
        {
            if (feedback.Type == CombatFeedbackType.EnemyAttackTelegraph)
            {
                var enemyView = feedback.Enemy != null
                    ? EnemyViews.FirstOrDefault(v => v.LiveData.Id == feedback.Enemy.Id)
                    : null;
                if (enemyView != null)
                {
                    enemyView.StartNormalTelegraph(feedback.AttackName, 1.0f);
                }
                return;
            }

            if (feedback.Type == CombatFeedbackType.EnemyHit ||
                feedback.Type == CombatFeedbackType.EnemyKnockback)
            {
                var enemyView = feedback.Enemy != null
                    ? EnemyViews.FirstOrDefault(v => v.LiveData.Id == feedback.Enemy.Id)
                    : null;
                if (enemyView != null && feedback.Type == CombatFeedbackType.EnemyHit)
                {
                    enemyView.TakeDamage(feedback.Damage);
                }

                if (feedback.Type == CombatFeedbackType.EnemyHit && feedback.Damage > 0f)
                {
                    SpawnDamageNumber(
                        feedback.WorldX,
                        feedback.WorldY - 18f,
                        feedback.Damage,
                        feedback.IsCritical);
                    if (Camera != null)
                    {
                        Camera.AddTrauma(feedback.IsCritical ? 0.10f : 0.04f);
                    }
                }
                return;
            }

            if (feedback.Type == CombatFeedbackType.UnitHurt)
            {
                var unitView = feedback.Unit != null
                    ? UnitViews.FirstOrDefault(v => v.LiveData.Member.Id == feedback.Unit.Member.Id)
                    : null;
                if (unitView != null)
                {
                    unitView.TakeDamage(feedback.Damage);
                    SpawnDamageNumber(
                        feedback.WorldX,
                        feedback.WorldY - 16f,
                        feedback.Damage,
                        false);
                }
                return;
            }

            if (feedback.Type == CombatFeedbackType.TerrainChanged)
            {
                TerrainProfile profile = BattleManager.State.CurrentTerrain;
                if (OnTerrainChanged != null)
                {
                    OnTerrainChanged.Invoke(profile);
                }
                return;
            }

            if (feedback.Type == CombatFeedbackType.EnemyAttackImpact)
            {
                var enemyView = feedback.Enemy != null
                    ? EnemyViews.FirstOrDefault(v => v.LiveData.Id == feedback.Enemy.Id)
                    : null;
                var unitView = feedback.Unit != null
                    ? UnitViews.FirstOrDefault(v => v.LiveData.Id == feedback.Unit.Member.Id)
                    : null;

                if (enemyView != null && enemyView.LiveData.IsAlive)
                {
                    enemyView.SetState(EnemyVisualState.Attacking);
                }

                if (unitView != null)
                {
                    unitView.TakeDamage(feedback.Damage);
                    SpawnDamageNumber(
                        feedback.WorldX,
                        feedback.WorldY - 16f,
                        feedback.Damage,
                        false);
                }

                if (Camera != null)
                {
                    Camera.AddTrauma(0.08f);
                }
                return;
            }

            if (feedback.Type == CombatFeedbackType.FormationPressure)
            {
                if (feedback.Damage >= CombatRules.FormationPressureCriticalThreshold)
                {
                    AddFormationWarning();
                }
            }
        }

        private void AddFormationWarning()
        {
            SpawnDamageNumber(
                BattleManager.State.BannerX,
                -70f,
                BattleManager.State.FormationPressure,
                false);
            if (Camera != null)
            {
                Camera.AddTrauma(0.05f);
            }
        }

        private void HandleCommandEvaluated(RhythmEvent rhythmEvent)
        {
            if (rhythmEvent.Type == RhythmEventType.Command)
            {
                HUDState.DisplayCommandChant(rhythmEvent.Command);

                // Set unit visual animation states based on command
                switch (rhythmEvent.Command.Id)
                {
                    case CommandId.March:
                        SetAllUnitsVisualState(UnitVisualState.Marching);
                        break;
                    case CommandId.Attack:
                        SetAllUnitsVisualState(UnitVisualState.Attacking);
                        SpawnRangedProjectiles();
                        break;
                    case CommandId.Defend:
                        SetAllUnitsVisualState(UnitVisualState.Defending);
                        break;
                    case CommandId.Charge:
                        SetAllUnitsVisualState(UnitVisualState.Charging);
                        break;
                    case CommandId.Jump:
                        SetAllUnitsVisualState(UnitVisualState.Jumping);
                        break;
                }
            }
        }

        private void SpawnRangedProjectiles()
        {
            // Find alive ranged units
            foreach (var u in UnitViews.Where(uv => uv.LiveData.IsAlive))
            {
                if (u.UnitMember.Class == UnitClass.Spearman)
                {
                    var target = EnemyViews.FirstOrDefault(ev => ev.LiveData.IsAlive);
                    float targetX = target != null ? target.LiveData.X : u.LiveData.X + 220f;
                    ProjectileViews.Add(new ProjectileView(Guid.NewGuid().ToString(), ProjectileVisualType.Spear, u.LiveData.X + 10f, u.LiveData.Y + 15f, targetX, 0f, 380f, 50f));
                }
                else if (u.UnitMember.Class == UnitClass.Archer)
                {
                    var target = EnemyViews.FirstOrDefault(ev => ev.LiveData.IsAlive);
                    float targetX = target != null ? target.LiveData.X : u.LiveData.X + 320f;
                    ProjectileViews.Add(new ProjectileView(Guid.NewGuid().ToString(), ProjectileVisualType.Arrow, u.LiveData.X + 10f, u.LiveData.Y + 12f, targetX, 0f, 420f, 75f));
                }
                else if (u.UnitMember.Class == UnitClass.Mage)
                {
                    var target = EnemyViews.FirstOrDefault(ev => ev.LiveData.IsAlive);
                    float targetX = target != null ? target.LiveData.X : u.LiveData.X + 250f;
                    ProjectileViews.Add(new ProjectileView(Guid.NewGuid().ToString(), ProjectileVisualType.ArcaneOrb, u.LiveData.X + 10f, u.LiveData.Y + 20f, targetX, 0f, 300f, 30f));
                }
            }
        }

        private void SetAllUnitsVisualState(UnitVisualState state)
        {
            foreach (var uv in UnitViews)
            {
                if (uv.LiveData.IsAlive)
                {
                    uv.SetState(state);
                }
            }
        }

        private void HandleEnemySlain(LiveEnemy enemy, List<LootRewardSummary> loot, int currency)
        {
            var ev = EnemyViews.FirstOrDefault(v => v.LiveData.Id == enemy.Id);
            if (ev != null)
            {
                ev.SetState(EnemyVisualState.Dead);
            }
            if (Camera != null)
            {
                Camera.AddTrauma(0.12f);
            }

            if (currency > 0)
            {
                HUDState.AddLootToast("currency_coin", currency);
            }

            if (loot != null)
            {
                foreach (var drop in loot)
                {
                    HUDState.AddLootToast(drop.ItemId, drop.Quantity);
                }
            }
        }

        public void SpawnDamageNumber(float worldX, float worldY, float damage, bool isCrit = false, bool isHeal = false)
        {
            var dmg = new DamageNumberEvent
            {
                WorldX = worldX,
                WorldY = worldY,
                Damage = damage,
                IsCritical = isCrit,
                IsHeal = isHeal
            };
            PendingDamageNumbers.Add(dmg);
            if (OnDamageNumberSpawned != null)
            {
                OnDamageNumberSpawned(dmg);
            }
        }
    }
}
