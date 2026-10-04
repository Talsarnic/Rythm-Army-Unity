using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;

namespace RhythmArmy.Gameplay.Battle
{
    public enum HeroAbilityType
    {
        SpearTempest,       // Spearman: 3x piercing spear spread
        CycloneBlade,       // Swordsman: Whirlwind slash + full block
        ArrowRainVolley,    // Archer: 5-arrow ballistic barrage
        SonicLancePierce,   // Cavalry: Rushing lance through front ranks
        TitanQuakeBreaker,  // Hammerer: 4x crushing structural smash
        ResonanceRoar,      // Hornist: Sonic AOE wave + squad shield
        AerialDivebomb,     // Skyrider: Swoop strike onto enemy backline
        GrandCelestialFlare,// Mage: Screen-wide elemental blast
        MeteorImpact,       // Brawler: Heavy single-target crusher
        DivineInspiration   // Banner: Rallying squad heal & buff
    }

    public class HeroActionResult
    {
        public bool TriggeredHeroMode;
        public HeroAbilityType Ability;
        public string HeroUnitId;
        public UnitClass HeroClass;
        public float DamageDealt;
        public int TargetsHit;
        public string Description;
    }

    /// <summary>
    /// Manages the Squad Hero Champion, Hero Mode trigger conditions (Fever + Perfect beats),
    /// and class-specific signature relic abilities.
    /// </summary>
    public static class HeroSystem
    {
        public static LiveUnit GetHeroUnit(List<LiveUnit> squad)
        {
            if (squad == null || squad.Count == 0) return null;
            return squad.FirstOrDefault(u => u.Member != null && u.Member.IsHero && u.IsAlive)
                   ?? squad.FirstOrDefault(u => u.IsAlive && u.Member != null && u.Member.Class != UnitClass.Banner);
        }

        public static HeroAbilityType GetAbilityForClass(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Spearman: return HeroAbilityType.SpearTempest;
                case UnitClass.Swordsman: return HeroAbilityType.CycloneBlade;
                case UnitClass.Archer: return HeroAbilityType.ArrowRainVolley;
                case UnitClass.Cavalry: return HeroAbilityType.SonicLancePierce;
                case UnitClass.Hammerer: return HeroAbilityType.TitanQuakeBreaker;
                case UnitClass.Hornist: return HeroAbilityType.ResonanceRoar;
                case UnitClass.Skyrider: return HeroAbilityType.AerialDivebomb;
                case UnitClass.Mage: return HeroAbilityType.GrandCelestialFlare;
                case UnitClass.Brawler: return HeroAbilityType.MeteorImpact;
                case UnitClass.Banner: return HeroAbilityType.DivineInspiration;
                default: return HeroAbilityType.SpearTempest;
            }
        }

        public static bool CanTriggerHeroMode(CommandId command, bool isFever, int perfectCount)
        {
            // Hero mode triggers on Attack, Charge, or Jump during Fever with high rhythm precision (3+ Perfects)
            if (!isFever) return false;
            if (perfectCount < 3) return false;

            return command == CommandId.Attack || command == CommandId.Charge || command == CommandId.Jump;
        }

        public static HeroActionResult ExecuteHeroAction(BattleState state, LiveUnit hero, CommandId command, Random rng = null)
        {
            var result = new HeroActionResult
            {
                TriggeredHeroMode = true,
                HeroUnitId = hero.Member != null ? hero.Member.Id : "hero",
                HeroClass = hero.Member != null ? hero.Member.Class : UnitClass.Spearman,
                Ability = GetAbilityForClass(hero.Member != null ? hero.Member.Class : UnitClass.Spearman)
            };

            var r = rng ?? new Random();
            hero.IsHeroMode = true;

            // Calculate Hero base damage including weapon and mask relic
            string weaponId = hero.Member != null ? hero.Member.WeaponId : null;
            var weapon = ItemDef.Get(weaponId);
            var hit = CombatFormulas.CalculateDamage(hero.Member, weapon, 0f, false, false, true, r);
            float baseDmg = hit.FinalDamage * 1.5f;

            string maskId = hero.Member != null ? hero.Member.MaskId : null;
            if (!string.IsNullOrEmpty(maskId))
            {
                var mask = ItemDef.Get(maskId);
                if (mask != null)
                {
                    baseDmg += (mask.MinDamage + mask.MaxDamage) * 0.5f;
                }
            }

            var aliveEnemies = state.Enemies.Where(e => e.IsAlive).OrderBy(e => e.X).ToList();

            switch (result.Ability)
            {
                case HeroAbilityType.SpearTempest:
                    // 3 piercing spears striking front targets
                    float spearDmg = baseDmg * 2.8f;
                    int spearHits = Math.Min(3, aliveEnemies.Count);
                    for (int i = 0; i < spearHits; i++)
                    {
                        aliveEnemies[i].CurrentHp = Math.Max(0f, aliveEnemies[i].CurrentHp - spearDmg);
                        result.DamageDealt += spearDmg;
                        result.TargetsHit++;
                    }
                    result.Description = "Hero hurled a tempest of 3 piercing radiant spears!";
                    break;

                case HeroAbilityType.CycloneBlade:
                    // Swordsman whirlwind slash + damage negation
                    float bladeDmg = baseDmg * 3.2f;
                    if (aliveEnemies.Count > 0)
                    {
                        var target = aliveEnemies[0];
                        target.CurrentHp = Math.Max(0f, target.CurrentHp - bladeDmg);
                        result.DamageDealt += bladeDmg;
                        result.TargetsHit++;
                    }
                    result.Description = "Hero performed an invincible Cyclone Blade slash!";
                    break;

                case HeroAbilityType.ArrowRainVolley:
                    // Archer 5-arrow ballistic spread
                    float arrowDmg = baseDmg * 1.5f;
                    int hits = 0;
                    for (int i = 0; i < 5 && aliveEnemies.Count > 0; i++)
                    {
                        var target = aliveEnemies[i % aliveEnemies.Count];
                        target.CurrentHp = Math.Max(0f, target.CurrentHp - arrowDmg);
                        result.DamageDealt += arrowDmg;
                        hits++;
                    }
                    result.TargetsHit = hits;
                    result.Description = "Hero unleashed a rapid 5-arrow celestial rain!";
                    break;

                case HeroAbilityType.SonicLancePierce:
                    // Cavalry rush through first 2 enemies
                    float lanceDmg = baseDmg * 2.5f;
                    int cavalryHits = Math.Min(2, aliveEnemies.Count);
                    for (int i = 0; i < cavalryHits; i++)
                    {
                        aliveEnemies[i].CurrentHp = Math.Max(0f, aliveEnemies[i].CurrentHp - lanceDmg);
                        aliveEnemies[i].X += 60f; // Knockback
                        result.DamageDealt += lanceDmg;
                        result.TargetsHit++;
                    }
                    result.Description = "Hero charged with a sonic shockwave lance!";
                    break;

                case HeroAbilityType.TitanQuakeBreaker:
                    // Hammerer 4x structure damage & ground smash
                    if (aliveEnemies.Count > 0)
                    {
                        var front = aliveEnemies[0];
                        float multi = front.IsStructure ? 4.5f : 3.0f;
                        float hammerDmg = baseDmg * multi;
                        front.CurrentHp = Math.Max(0f, front.CurrentHp - hammerDmg);
                        result.DamageDealt += hammerDmg;
                        result.TargetsHit = 1;
                    }
                    result.Description = "Hero shattered the ground with Titan Quake Breaker!";
                    break;

                case HeroAbilityType.GrandCelestialFlare:
                    // Mage AOE burst hitting all enemies
                    float flareDmg = baseDmg * 2.2f;
                    foreach (var enemy in aliveEnemies)
                    {
                        enemy.CurrentHp = Math.Max(0f, enemy.CurrentHp - flareDmg);
                        result.DamageDealt += flareDmg;
                        result.TargetsHit++;
                    }
                    result.Description = "Hero invoked Grand Celestial Flare across the battlefield!";
                    break;

                case HeroAbilityType.MeteorImpact:
                    // Brawler 5x crushing slam
                    if (aliveEnemies.Count > 0)
                    {
                        var strongest = aliveEnemies.OrderByDescending(e => e.MaxHp).First();
                        float meteorDmg = baseDmg * 4.5f;
                        strongest.CurrentHp = Math.Max(0f, strongest.CurrentHp - meteorDmg);
                        result.DamageDealt += meteorDmg;
                        result.TargetsHit = 1;
                    }
                    result.Description = "Hero leaped and crashed down with Meteor Impact!";
                    break;

                default:
                    // General rally heal for squad
                    foreach (var unit in state.Units)
                    {
                        if (unit.IsAlive)
                        {
                            unit.CurrentHp = Math.Min(unit.MaxHp, unit.CurrentHp + 25f);
                        }
                    }
                    result.Description = "Hero rallied the squad with Divine Inspiration!";
                    break;
            }

            return result;
        }
    }
}
