using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Core.Combat
{
    public struct FormationSlot
    {
        public float OffsetX;
        public float OffsetY;
        public int Rank; // 0 = frontline, 1 = midline, 2 = backline

        public FormationSlot(float offsetX, float offsetY, int rank)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Rank = rank;
        }
    }

    public static class FormationSystem
    {
        // Base formation offsets relative to the Banner standard
        public static FormationSlot GetUnitSlot(UnitClass unitClass, int unitIndexInSquad, int totalInSquad)
        {
            float squadStagger = (unitIndexInSquad - ((totalInSquad - 1) / 2f)) * 14f;

            switch (unitClass)
            {
                case UnitClass.Banner:
                    return new FormationSlot(0f, 0f, 1);

                case UnitClass.Swordsman: // Frontline Tank (Sword & Shield)
                    return new FormationSlot(80f + (unitIndexInSquad * 12f), squadStagger, 0);

                case UnitClass.Brawler:   // Heavy Frontline Demolisher
                    return new FormationSlot(70f + (unitIndexInSquad * 15f), squadStagger, 0);

                case UnitClass.Hammerer:  // Heavy Breaker Vanguard (Hammer/Club)
                    return new FormationSlot(60f + (unitIndexInSquad * 16f), squadStagger, 0);

                case UnitClass.Cavalry:   // Mounted Cavalry (Spear & Mount)
                    return new FormationSlot(45f + (unitIndexInSquad * 18f), squadStagger, 1);

                case UnitClass.Spearman:  // Midline Javelin Throwers
                    return new FormationSlot(-30f - (unitIndexInSquad * 14f), squadStagger, 1);

                case UnitClass.Skyrider:  // Flying Sky Lancers
                    return new FormationSlot(-10f - (unitIndexInSquad * 16f), 45f + squadStagger, 1);

                case UnitClass.Archer:    // Rear Backline Archers
                    return new FormationSlot(-110f - (unitIndexInSquad * 15f), squadStagger, 2);

                case UnitClass.Mage:      // Rear Mystic Spellcasters
                    return new FormationSlot(-140f - (unitIndexInSquad * 16f), squadStagger, 2);

                case UnitClass.Hornist:   // Rear Sonic Warhorns
                    return new FormationSlot(-160f - (unitIndexInSquad * 16f), squadStagger, 2);

                default:
                    return new FormationSlot(-20f, 0f, 1);
            }
        }

        public static float GetEngagementRange(UnitClass unitClass)
        {
            switch (unitClass)
            {
                case UnitClass.Swordsman: return 60f;
                case UnitClass.Brawler: return 50f;
                case UnitClass.Hammerer: return 70f;
                case UnitClass.Cavalry: return 120f;
                case UnitClass.Spearman: return 380f;
                case UnitClass.Skyrider: return 320f;
                case UnitClass.Archer: return 480f;
                case UnitClass.Mage: return 420f;
                case UnitClass.Hornist: return 350f;
                default: return 50f;
            }
        }
    }
}
