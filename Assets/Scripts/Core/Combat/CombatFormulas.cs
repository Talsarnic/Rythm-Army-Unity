using System;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Core.Combat
{
    public struct HitResult
    {
        public float RawDamage;
        public float FinalDamage;
        public bool IsCritical;
        public float Knockback;
        public DamageElement Element;
    }

    public struct UnitStatsSummary
    {
        public float Hp;
        public float Attack;
        public float Defense;
    }

    public static class CombatFormulas
    {
        public static UnitStatsSummary CalculateMemberStats(UnitMember member)
        {
            if (member == null) return new UnitStatsSummary { Hp = 50f, Attack = 5f, Defense = 0f };

            float hp = CalculateBaseHealth(member.Class, member.Level, member.Subspecies);
            
            // Secondary fusion health bonus
            if (member.SecondaryFusion == Subspecies.Frogtide) hp *= 1.15f;
            else if (member.SecondaryFusion == Subspecies.Colossus) hp *= 1.35f;
            else if (member.SecondaryFusion == Subspecies.Apex) hp *= 1.25f;

            // Health Mastery (+3% per point)
            if (member.HealthMastery > 0)
            {
                hp *= (1f + member.HealthMastery * 0.03f);
            }

            var weapon = ItemDef.Get(member.WeaponId);
            var shield = ItemDef.Get(member.ShieldId);
            var helm = ItemDef.Get(member.HelmetId);

            float def = 0f;
            float atk = 5f;

            if (weapon != null)
            {
                atk = (weapon.MinDamage + weapon.MaxDamage) / 2f;
                def += weapon.Defense;
                hp += weapon.HealthBonus;
            }

            if (shield != null)
            {
                def += shield.Defense;
                hp += shield.HealthBonus;
            }

            if (helm != null)
            {
                def += helm.Defense;
                hp += helm.HealthBonus;
            }

            // Attack Mastery (+2% per point)
            if (member.AttackMastery > 0)
            {
                atk *= (1f + member.AttackMastery * 0.02f);
            }

            // Defense Mastery (+2% per point)
            if (member.DefenseMastery > 0)
            {
                def *= (1f + member.DefenseMastery * 0.02f);
            }

            return new UnitStatsSummary { Hp = hp, Attack = atk, Defense = def };
        }

        public static int CalculateGearScore(UnitMember member)
        {
            if (member == null) return 0;
            int score = 0;
            var weapon = ItemDef.Get(member.WeaponId);
            var shield = ItemDef.Get(member.ShieldId);
            var helm = ItemDef.Get(member.HelmetId);

            if (weapon != null) score += weapon.GearScore;
            if (shield != null) score += shield.GearScore;
            if (helm != null) score += helm.GearScore;

            score += member.Level * 5;
            return score;
        }

        public static float CalculateBaseHealth(UnitClass unitClass, int level, Subspecies subspecies = Subspecies.Normal)
        {
            float baseHp;
            switch (unitClass)
            {
                case UnitClass.Banner: baseHp = 150f; break;
                case UnitClass.Swordsman: baseHp = 120f; break;
                case UnitClass.Brawler: baseHp = 140f; break;
                case UnitClass.Hammerer: baseHp = 160f; break;
                case UnitClass.Cavalry: baseHp = 100f; break;
                case UnitClass.Spearman: baseHp = 70f; break;
                case UnitClass.Skyrider: baseHp = 65f; break;
                case UnitClass.Archer: baseHp = 50f; break;
                case UnitClass.Mage: baseHp = 45f; break;
                case UnitClass.Hornist: baseHp = 60f; break;
                default: baseHp = 60f; break;
            }

            float total = baseHp + ((level - 1) * 15f);

            switch (subspecies)
            {
                case Subspecies.Frogtide: total *= 1.30f; break;
                case Subspecies.Ironwool: total *= 1.20f; break;
                case Subspecies.Colossus: total *= 1.80f; break;
                case Subspecies.Apex: total *= 1.50f; break;
            }

            return total;
        }

        public static HitResult CalculateDamage(
            UnitMember attacker,
            ItemDef weapon,
            float targetDefense,
            bool isTargetStructure,
            bool isDefending,
            bool isFever,
            Random rng = null,
            ItemDef mealBuff = null)
        {
            rng = rng ?? new Random();

            float minDmg = (weapon != null ? weapon.MinDamage : 2f) + (mealBuff != null ? mealBuff.MinDamage : 0f);
            float maxDmg = (weapon != null ? weapon.MaxDamage : 5f) + (mealBuff != null ? mealBuff.MaxDamage : 0f);
            float critChance = (weapon != null ? weapon.CritChance : 0.05f) + (mealBuff != null ? mealBuff.CritChance : 0f);
            float structureMult = weapon != null ? weapon.StructureBonus : 1.0f;
            float knockback = weapon != null ? weapon.KnockbackPower : 10f;
            DamageElement element = weapon != null ? weapon.Element : DamageElement.Physical;

            // Subspecies bonus
            if (attacker != null)
            {
                switch (attacker.Subspecies)
                {
                    case Subspecies.Swiftpaw:
                        critChance += 0.10f;
                        minDmg *= 1.10f;
                        maxDmg *= 1.10f;
                        break;
                    case Subspecies.Colossus:
                        minDmg *= 1.30f;
                        maxDmg *= 1.30f;
                        knockback *= 1.50f;
                        break;
                    case Subspecies.Apex:
                        critChance += 0.15f;
                        minDmg *= 1.50f;
                        maxDmg *= 1.50f;
                        knockback *= 1.30f;
                        break;
                }

                // Secondary Fusion bonuses
                switch (attacker.SecondaryFusion)
                {
                    case Subspecies.Swiftpaw:
                        critChance += 0.05f;
                        minDmg *= 1.05f;
                        maxDmg *= 1.05f;
                        break;
                    case Subspecies.Colossus:
                        minDmg *= 1.15f;
                        maxDmg *= 1.15f;
                        knockback *= 1.25f;
                        break;
                    case Subspecies.Apex:
                        critChance += 0.07f;
                        minDmg *= 1.25f;
                        maxDmg *= 1.25f;
                        knockback *= 1.15f;
                        break;
                }

                // Attack Mastery bonus
                if (attacker.AttackMastery > 0)
                {
                    float masteryMult = 1f + (attacker.AttackMastery * 0.02f);
                    minDmg *= masteryMult;
                    maxDmg *= masteryMult;
                }
            }

            // Roll base damage
            float roll = (float)(minDmg + (rng.NextDouble() * (maxDmg - minDmg)));

            // Level bonus
            int lvl = attacker != null ? attacker.Level : 1;
            roll *= (1f + ((lvl - 1) * 0.1f));

            // Fever bonus
            if (isFever)
            {
                roll *= 1.35f;
                knockback *= 1.5f;
            }

            // Critical hit check
            bool isCrit = rng.NextDouble() < (isFever ? critChance * 1.5f : critChance);
            if (isCrit)
            {
                roll *= 1.5f;
            }

            // Structure multiplier
            if (isTargetStructure)
            {
                roll *= structureMult;
            }

            // Defense reduction
            // Formula: Damage = Roll * (100 / (100 + Defense))
            float mitigation = 100f / (100f + Math.Max(0f, targetDefense));
            float finalDmg = roll * mitigation;

            // Shield Wall defense bonus during Defend command
            if (isDefending)
            {
                finalDmg *= 0.45f; // 55% reduction while defending
            }

            return new HitResult
            {
                RawDamage = (float)Math.Round(roll, 1),
                FinalDamage = Math.Max(1f, (float)Math.Round(finalDmg, 1)),
                IsCritical = isCrit,
                Knockback = knockback,
                Element = element
            };
        }
    }
}
