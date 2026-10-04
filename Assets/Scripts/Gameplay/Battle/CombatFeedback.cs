using RhythmArmy.Core.Data;

namespace RhythmArmy.Gameplay.Battle
{
    public enum CombatFeedbackType
    {
        EnemyHit,
        EnemyKnockback,
        UnitHurt,
        EnemyAttackTelegraph,
        EnemyAttackImpact,
        FormationPressure
    }

    /// <summary>
    /// Simulation-to-presentation combat event. The simulation records these
    /// at the exact damage/pressure moment; BattleController turns them into
    /// hit reactions, damage numbers, camera response, and telegraphs.
    /// </summary>
    public struct CombatFeedbackEvent
    {
        public CombatFeedbackType Type;
        public LiveUnit Unit;
        public LiveEnemy Enemy;
        public float Damage;
        public float Knockback;
        public bool IsCritical;
        public float WorldX;
        public float WorldY;
        public string AttackName;

        public static CombatFeedbackEvent EnemyHit(LiveEnemy enemy, float damage, float knockback, bool critical = false)
        {
            return new CombatFeedbackEvent
            {
                Type = CombatFeedbackType.EnemyHit,
                Enemy = enemy,
                Damage = damage,
                Knockback = knockback,
                IsCritical = critical,
                WorldX = enemy != null ? enemy.X : 0f,
                WorldY = enemy != null ? enemy.Y : 0f
            };
        }

        public static CombatFeedbackEvent UnitHurt(LiveUnit unit, float damage)
        {
            return new CombatFeedbackEvent
            {
                Type = CombatFeedbackType.UnitHurt,
                Unit = unit,
                Damage = damage,
                WorldX = unit != null ? unit.X : 0f,
                WorldY = unit != null ? unit.Y : 0f
            };
        }

        public static CombatFeedbackEvent EnemyAttackTelegraph(LiveEnemy enemy, string name)
        {
            return new CombatFeedbackEvent
            {
                Type = CombatFeedbackType.EnemyAttackTelegraph,
                Enemy = enemy,
                AttackName = name,
                WorldX = enemy != null ? enemy.X : 0f,
                WorldY = enemy != null ? enemy.Y : 0f
            };
        }

        public static CombatFeedbackEvent EnemyAttackImpact(LiveEnemy enemy, LiveUnit unit, float damage)
        {
            return new CombatFeedbackEvent
            {
                Type = CombatFeedbackType.EnemyAttackImpact,
                Enemy = enemy,
                Unit = unit,
                Damage = damage,
                WorldX = unit != null ? unit.X : (enemy != null ? enemy.X : 0f),
                WorldY = unit != null ? unit.Y : (enemy != null ? enemy.Y : 0f)
            };
        }

        public static CombatFeedbackEvent FormationPressure(float pressure, float integrity)
        {
            return new CombatFeedbackEvent
            {
                Type = CombatFeedbackType.FormationPressure,
                Damage = pressure,
                Knockback = integrity
            };
        }
    }
}
