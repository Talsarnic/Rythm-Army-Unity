using System;
using RhythmArmy.Core.Data;

namespace RhythmArmy.Gameplay.Battle
{
    public enum TerrainType
    {
        Ground,
        DeepWater,
        Mud,
        Ice,
        Heat,
        Sand,
        Elevated,
        SacredStone
    }

    public struct TerrainProfile
    {
        public TerrainType Type;
        public float MovementMultiplier;
        public float HeavyMovementMultiplier;
        public float RangedDamageMultiplier;
        public float ChargeMomentumMultiplier;
        public float HeatDamagePercent;
        public string DisplayName;

        public TerrainProfile(
            TerrainType type,
            float movementMultiplier,
            float heavyMovementMultiplier,
            float rangedDamageMultiplier,
            float chargeMomentumMultiplier,
            float heatDamagePercent,
            string displayName)
        {
            Type = type;
            MovementMultiplier = movementMultiplier;
            HeavyMovementMultiplier = heavyMovementMultiplier;
            RangedDamageMultiplier = rangedDamageMultiplier;
            ChargeMomentumMultiplier = chargeMomentumMultiplier;
            HeatDamagePercent = heatDamagePercent;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Central terrain rules. The campaign can later replace these biome bands
    /// with authored map segments without changing combat code.
    /// </summary>
    public static class TerrainRules
    {
        public static TerrainProfile Ground = new TerrainProfile(
            TerrainType.Ground, 1f, 1f, 1f, 1f, 0f, "Firm Ground");

        public static TerrainProfile GetProfile(MissionDef mission, float worldX)
        {
            if (mission == null) return Ground;

            string biome = (mission.Biome ?? string.Empty).ToLowerInvariant();
            float progress = mission.WorldLength > 0f
                ? Math.Max(0f, Math.Min(1f, worldX / mission.WorldLength))
                : 0f;

            switch (biome)
            {
                case "coral-coast":
                    if (progress >= 0.16f && progress < 0.30f)
                        return new TerrainProfile(TerrainType.DeepWater, 0.72f, 0.50f, 1f, 0.85f, 0f, "Tide Flats");
                    if (progress >= 0.56f && progress < 0.68f)
                        return new TerrainProfile(TerrainType.DeepWater, 0.78f, 0.55f, 1f, 0.90f, 0f, "Tide Cave");
                    return Ground;

                case "jungle-fort":
                    if (progress >= 0.22f && progress < 0.42f)
                        return new TerrainProfile(TerrainType.Mud, 0.70f, 0.55f, 1f, 0.80f, 0f, "Jungle Mud");
                    if (progress >= 0.52f && progress < 0.68f)
                        return new TerrainProfile(TerrainType.Elevated, 1.0f, 0.95f, 1.15f, 1.05f, 0f, "Rampart Ridge");
                    return Ground;

                case "misty-swamp":
                    if (progress >= 0.08f && progress < 0.28f)
                        return new TerrainProfile(TerrainType.Mud, 0.62f, 0.45f, 1f, 0.75f, 0f, "Mire Mud");
                    if (progress >= 0.30f && progress < 0.48f)
                        return new TerrainProfile(TerrainType.DeepWater, 0.58f, 0.38f, 1f, 0.70f, 0f, "Mire Shallows");
                    if (progress >= 0.60f && progress < 0.72f)
                        return new TerrainProfile(TerrainType.Elevated, 1.0f, 0.95f, 1.10f, 1f, 0f, "Root Bridge");
                    return Ground;

                case "volcanic-caldera":
                    if (progress >= 0.18f && progress < 0.35f)
                        return new TerrainProfile(TerrainType.Heat, 0.88f, 0.72f, 1f, 0.95f, 0.015f, "Scorched Ground");
                    if (progress >= 0.45f && progress < 0.60f)
                        return new TerrainProfile(TerrainType.Heat, 0.78f, 0.60f, 1f, 1.05f, 0.025f, "Lava Shelf");
                    return Ground;

                case "iron-bastion":
                    if (progress >= 0.18f && progress < 0.36f)
                        return new TerrainProfile(TerrainType.Elevated, 1f, 0.92f, 1.15f, 1.05f, 0f, "Outer Wall");
                    if (progress >= 0.48f && progress < 0.62f)
                        return new TerrainProfile(TerrainType.Elevated, 1f, 0.92f, 1.20f, 1.05f, 0f, "Tower Walk");
                    return Ground;

                case "desert-dunes":
                    if (progress >= 0.10f && progress < 0.38f)
                        return new TerrainProfile(TerrainType.Sand, 0.78f, 0.62f, 0.82f, 0.90f, 0f, "Soft Dunes");
                    if (progress >= 0.50f && progress < 0.66f)
                        return new TerrainProfile(TerrainType.Sand, 0.68f, 0.52f, 0.76f, 0.82f, 0f, "Deep Sand");
                    return Ground;

                case "frozen-peaks":
                    if (progress >= 0.14f && progress < 0.42f)
                        return new TerrainProfile(TerrainType.Ice, 0.94f, 0.88f, 1f, 1.20f, 0f, "Frozen Pass");
                    if (progress >= 0.50f && progress < 0.66f)
                        return new TerrainProfile(TerrainType.Ice, 0.90f, 0.82f, 1f, 1.30f, 0f, "Glacier Shelf");
                    return Ground;

                case "ruin-altar":
                    if (progress >= 0.28f && progress < 0.44f)
                        return new TerrainProfile(TerrainType.Elevated, 1f, 0.95f, 1.12f, 1.05f, 0f, "Ritual Bridge");
                    if (progress >= 0.70f && progress < 0.86f)
                        return new TerrainProfile(TerrainType.SacredStone, 1f, 1f, 1.15f, 1.05f, 0f, "Altar Stone");
                    return Ground;

                default:
                    return Ground;
            }
        }

        public static bool IsHeavy(UnitClass unitClass)
        {
            return unitClass == UnitClass.Hammerer ||
                   unitClass == UnitClass.Brawler ||
                   unitClass == UnitClass.Swordsman;
        }

        public static bool IsRanged(UnitClass unitClass)
        {
            return unitClass == UnitClass.Archer ||
                   unitClass == UnitClass.Mage ||
                   unitClass == UnitClass.Hornist ||
                   unitClass == UnitClass.Spearman ||
                   unitClass == UnitClass.Skyrider;
        }

        public static float GetMovementMultiplier(TerrainProfile profile, UnitClass unitClass)
        {
            if (IsHeavy(unitClass))
                return profile.MovementMultiplier * profile.HeavyMovementMultiplier;
            return profile.MovementMultiplier;
        }

        public static float GetChargeMultiplier(TerrainProfile profile)
        {
            return profile.ChargeMomentumMultiplier;
        }

        public static float GetRangedDamageMultiplier(TerrainProfile profile)
        {
            return profile.RangedDamageMultiplier;
        }
    }
}
