using System;
using System.Collections.Generic;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Battle
{
    public enum BattleCharmType
    {
        HealingTincture,
        FeverBell,
        SmokeBomb,
        PurificationIncense,
        WarHorn
    }

    public class BattleCharmDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public BattleCharmType CharmType { get; set; }
        public string Description { get; set; }
        public int GoldPrice { get; set; }
        public float CooldownMeasures { get; set; }

        public BattleCharmDef(string id, string name, BattleCharmType charmType, string description, int goldPrice, float cooldownMeasures = 4f)
        {
            Id = id;
            Name = name;
            CharmType = charmType;
            Description = description;
            GoldPrice = goldPrice;
            CooldownMeasures = cooldownMeasures;
        }
    }

    public class BattleItemEffectResult
    {
        public bool Success;
        public string Message;
        public int HealingDone;
        public int ComboAdded;
        public bool FeverTriggered;
        public bool DebuffsCleansed;
        public float DamageBuffMultiplier;
        public float DamageReductionMultiplier;
    }

    /// <summary>
    /// Tactical Battle Charms and Consumable Field Items System.
    /// Provides active in-combat tactical buffs, healing, and crisis intervention.
    /// </summary>
    public static class BattleItemSystem
    {
        private static readonly Dictionary<string, BattleCharmDef> _charms = new Dictionary<string, BattleCharmDef>();

        static BattleItemSystem()
        {
            _charms["charm-healing-tincture"] = new BattleCharmDef(
                "charm-healing-tincture",
                "Herbal Healing Tincture",
                BattleCharmType.HealingTincture,
                "Restores 35% max HP to all surviving squad members.",
                120,
                3f
            );

            _charms["charm-fever-bell"] = new BattleCharmDef(
                "charm-fever-bell",
                "Harmonic Fever Bell",
                BattleCharmType.FeverBell,
                "Rings with sacred resonance, adding +3 combo and instantly activating Fever Mode.",
                200,
                6f
            );

            _charms["charm-smoke-bomb"] = new BattleCharmDef(
                "charm-smoke-bomb",
                "Alchemical Smoke Bomb",
                BattleCharmType.SmokeBomb,
                "Deploys dense concealing smoke, reducing incoming enemy damage by 50% for 1 measure.",
                150,
                4f
            );

            _charms["charm-purification-incense"] = new BattleCharmDef(
                "charm-purification-incense",
                "Purification Incense",
                BattleCharmType.PurificationIncense,
                "Purges burning, freezing, and staggering debuffs from all friendly units.",
                100,
                2f
            );

            _charms["charm-war-horn"] = new BattleCharmDef(
                "charm-war-horn",
                "Courageous War Horn",
                BattleCharmType.WarHorn,
                "Sounds an inspiring battle charge, boosting army attack damage by 25% for 2 measures.",
                180,
                5f
            );
        }

        public static BattleCharmDef GetCharm(string charmId)
        {
            if (string.IsNullOrEmpty(charmId)) return null;
            BattleCharmDef charm;
            _charms.TryGetValue(charmId, out charm);
            return charm;
        }

        public static List<BattleCharmDef> GetAllCharms()
        {
            return new List<BattleCharmDef>(_charms.Values);
        }

        /// <summary>
        /// Activates a tactical battle charm within an active battle.
        /// </summary>
        public static BattleItemEffectResult UseBattleCharm(string charmId, BattleManager battle, SaveData save = null)
        {
            var result = new BattleItemEffectResult { Success = false };
            var charm = GetCharm(charmId);
            if (charm == null || battle == null || battle.State == null)
            {
                result.Message = "Invalid charm or inactive battle.";
                return result;
            }

            // Consume from save inventory if provided
            if (save != null)
            {
                if (save.GetItemCount(charmId) <= 0)
                {
                    result.Message = "Charm not available in inventory.";
                    return result;
                }
                save.RemoveItem(charmId, 1);
            }

            result.Success = true;
            switch (charm.CharmType)
            {
                case BattleCharmType.HealingTincture:
                    int totalHealed = 0;
                    foreach (var unit in battle.State.Units)
                    {
                        if (unit.IsAlive)
                        {
                            float healAmount = unit.MaxHp * 0.35f;
                            float oldHp = unit.CurrentHp;
                            unit.CurrentHp = Math.Min((float)unit.MaxHp, unit.CurrentHp + healAmount);
                            totalHealed += (int)(unit.CurrentHp - oldHp);
                        }
                    }
                    result.HealingDone = totalHealed;
                    result.Message = string.Format("Healing Tincture restored {0} HP across the squad!", totalHealed);
                    break;

                case BattleCharmType.FeverBell:
                    if (battle.Rhythm != null)
                    {
                        battle.Rhythm.ForceFever();
                        result.FeverTriggered = true;
                        result.ComboAdded = 3;
                        result.Message = "Harmonic Fever Bell rang out! FEVER MODE ACTIVATED!";
                    }
                    break;

                case BattleCharmType.SmokeBomb:
                    result.DamageReductionMultiplier = 0.5f;
                    result.Message = "Smoke bomb deployed! Incoming damage reduced by 50%.";
                    break;

                case BattleCharmType.PurificationIncense:
                    foreach (var unit in battle.State.Units)
                    {
                        unit.IsBurning = false;
                        unit.IsRushing = false;
                    }
                    result.DebuffsCleansed = true;
                    result.Message = "Purification incense cleansed all status ailments!";
                    break;

                case BattleCharmType.WarHorn:
                    result.DamageBuffMultiplier = 1.25f;
                    result.Message = "War horn sounded! Army attack damage boosted by 25%.";
                    break;
            }

            return result;
        }
    }
}
