using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public class EvolutionRequirement
    {
        public Subspecies TargetSubspecies;
        public int GoldCost;
        public Dictionary<string, int> RequiredMaterials;
        public int MinLevel;

        public Dictionary<string, int> Materials { get { return RequiredMaterials; } }

        public string PerksDescription
        {
            get
            {
                switch (TargetSubspecies)
                {
                    case Subspecies.Swiftpaw: return "+10% Critical Strike, +10% Damage";
                    case Subspecies.Frogtide: return "+30% Max HP, Freeze Immune";
                    case Subspecies.Ironwool: return "+40% Defense, Stun Immune";
                    case Subspecies.Colossus: return "+80% Max HP, +50% Knockback";
                    case Subspecies.Apex: return "+50% Max HP, +50% Damage, +15% Crit";
                    default: return "Base stats";
                }
            }
        }

        public EvolutionRequirement(Subspecies targetSubspecies, int goldCost, Dictionary<string, int> requiredMaterials, int minLevel = 1)
        {
            TargetSubspecies = targetSubspecies;
            GoldCost = goldCost;
            RequiredMaterials = requiredMaterials;
            MinLevel = minLevel;
        }
    }

    public static class AltarEvolution
    {
        public static Dictionary<Subspecies, EvolutionRequirement> EvolutionRecipes { get { return Requirements; } }

        public static readonly Dictionary<Subspecies, EvolutionRequirement> Requirements = new Dictionary<Subspecies, EvolutionRequirement>
        {
            {
                Subspecies.Swiftpaw,
                new EvolutionRequirement(
                    Subspecies.Swiftpaw,
                    50,
                    new Dictionary<string, int> { { "mat-sun-cabbage", 2 }, { "mat-jerky", 1 } },
                    1
                )
            },
            {
                Subspecies.Frogtide,
                new EvolutionRequirement(
                    Subspecies.Frogtide,
                    80,
                    new Dictionary<string, int> { { "mat-stone", 2 }, { "mat-sun-cabbage", 2 } },
                    2
                )
            },
            {
                Subspecies.Ironwool,
                new EvolutionRequirement(
                    Subspecies.Ironwool,
                    100,
                    new Dictionary<string, int> { { "mat-beast-hide", 3 }, { "mat-timber", 2 } },
                    2
                )
            },
            {
                Subspecies.Colossus,
                new EvolutionRequirement(
                    Subspecies.Colossus,
                    150,
                    new Dictionary<string, int> { { "mat-iron-slag", 3 }, { "mat-stone", 2 } },
                    3
                )
            },
            {
                Subspecies.Apex,
                new EvolutionRequirement(
                    Subspecies.Apex,
                    300,
                    new Dictionary<string, int> { { "mat-dragon-scale", 2 }, { "mat-mithril", 2 } },
                    4
                )
            }
        };

        public static bool CanEvolve(SaveData save, string unitId, Subspecies targetSubspecies)
        {
            if (save == null || string.IsNullOrEmpty(unitId) || targetSubspecies == Subspecies.Normal) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            if (unit == null || unit.Class == UnitClass.Banner) return false; // Banner cannot evolve
            if (unit.Subspecies == targetSubspecies) return false; // Already this subspecies

            EvolutionRequirement req;
            if (!Requirements.TryGetValue(targetSubspecies, out req)) return false;

            if (unit.Level < req.MinLevel) return false;
            if (save.Gold < req.GoldCost) return false;

            foreach (var kvp in req.RequiredMaterials)
            {
                if (save.GetItemCount(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EvolveUnit(SaveData save, string unitId, Subspecies targetSubspecies)
        {
            if (!CanEvolve(save, unitId, targetSubspecies)) return false;

            var unit = save.Roster.FirstOrDefault(u => u.Id == unitId);
            var req = Requirements[targetSubspecies];

            // Deduct Gold & Materials
            save.Gold -= req.GoldCost;
            foreach (var kvp in req.RequiredMaterials)
            {
                save.RemoveItem(kvp.Key, kvp.Value);
            }

            // Apply Subspecies
            unit.Subspecies = targetSubspecies;
            return true;
        }

        public static bool EvolveUnit(UnitMember unit, Subspecies targetSubspecies, SaveData save)
        {
            if (unit == null) return false;
            return EvolveUnit(save, unit.Id, targetSubspecies);
        }
    }
}
