using System;

namespace RhythmArmy.Core.Data
{
    public enum EnemyCategory
    {
        Wildlife,
        Fortification,
        RivalClan,
        Boss
    }

    public enum EnemyKind
    {
        // Wildlife / Hunting Game
        PlainsRunner,  // Swift Plains Runner (flees/runs, drops meats/veggies)
        WildBoar,      // Wild Tusked Boar
        GiantBoar,     // Great Armored Boar
        Stag,          // Giant Horned Stag (elusive big game)
        SandCrab,      // Armored Desert Crab

        // Barriers & Fortifications
        Barricade,     // Wooden Barrier / Palisade
        StoneWall,     // Fortified Stone Gate Rampart
        Watchtower,    // Arrow-shooting wooden watchtower
        CatapultTower, // Heavy stone ballista/catapult tower

        // Rival Tribe Squads
        TribeBanner,    // Enemy Standard Bearer
        TribeSpearman,  // Enemy Spear Hurler
        TribeSwordsman, // Enemy Sword & Shield Vanguard
        TribeArcher,    // Enemy Archer
        TribeCavalry,   // Enemy Mounted Charger
        TribeHammerer,  // Enemy Heavy Breaker
        TribeHornist,   // Enemy Sonic Warhornist
        TribeSkyrider,  // Enemy Flying Sky Lancer
        TribeMage,      // Enemy Arcane Channeler
        TribeBrawler,   // Enemy Gauntlet Demolisher

        // Colossal Bosses
        IronBehemoth,  // Armored Mountain Behemoth
        DrakeTitan,    // Fire-Breathing Drake Titan
        ColossusGolem  // Ancient Stone Golem
    }

    public static class EnemyMetadata
    {
        public static EnemyCategory GetCategory(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.PlainsRunner:
                case EnemyKind.WildBoar:
                case EnemyKind.GiantBoar:
                case EnemyKind.Stag:
                case EnemyKind.SandCrab:
                    return EnemyCategory.Wildlife;

                case EnemyKind.Barricade:
                case EnemyKind.StoneWall:
                case EnemyKind.Watchtower:
                case EnemyKind.CatapultTower:
                    return EnemyCategory.Fortification;

                case EnemyKind.TribeBanner:
                case EnemyKind.TribeSpearman:
                case EnemyKind.TribeSwordsman:
                case EnemyKind.TribeArcher:
                case EnemyKind.TribeCavalry:
                case EnemyKind.TribeHammerer:
                case EnemyKind.TribeHornist:
                case EnemyKind.TribeSkyrider:
                case EnemyKind.TribeMage:
                case EnemyKind.TribeBrawler:
                    return EnemyCategory.RivalClan;

                case EnemyKind.IronBehemoth:
                case EnemyKind.DrakeTitan:
                case EnemyKind.ColossusGolem:
                    return EnemyCategory.Boss;

                default:
                    return EnemyCategory.Wildlife;
            }
        }

        public static float GetCollisionRadius(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.ColossusGolem: return 110f;
                case EnemyKind.DrakeTitan: return 100f;
                case EnemyKind.IronBehemoth: return 85f;
                case EnemyKind.CatapultTower: return 78f;
                case EnemyKind.Watchtower: return 70f;
                case EnemyKind.StoneWall: return 60f;
                case EnemyKind.Barricade: return 50f;
                case EnemyKind.GiantBoar: return 45f;
                case EnemyKind.WildBoar: return 35f;
                case EnemyKind.SandCrab: return 30f;
                case EnemyKind.Stag: return 28f;
                case EnemyKind.PlainsRunner: return 20f;
                case EnemyKind.TribeCavalry:
                case EnemyKind.TribeHammerer:
                case EnemyKind.TribeSkyrider: return 32f;
                case EnemyKind.TribeBanner:
                case EnemyKind.TribeSpearman:
                case EnemyKind.TribeSwordsman:
                case EnemyKind.TribeArcher:
                case EnemyKind.TribeHornist:
                case EnemyKind.TribeMage:
                case EnemyKind.TribeBrawler: return 24f;
                default: return 25f;
            }
        }
    }
}
