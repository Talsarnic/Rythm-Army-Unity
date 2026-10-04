using System;
using System.Collections.Generic;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Battle
{
    public class MonolithDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LoreDescription { get; set; }
        public List<CommandId> RequiredChants { get; set; }
        public int RewardGold { get; set; }
        public string RewardItemId { get; set; }
        public string UnlockMissionId { get; set; }

        public MonolithDef(string id, string name, string description, List<CommandId> requiredChants, int gold, string itemId = null, string unlockMission = null)
        {
            Id = id;
            Name = name;
            LoreDescription = description;
            RequiredChants = requiredChants ?? new List<CommandId>();
            RewardGold = gold;
            RewardItemId = itemId;
            UnlockMissionId = unlockMission;
        }
    }

    public class MonolithInstance
    {
        public MonolithDef Definition { get; set; }
        public float WorldX { get; set; }
        public int CurrentStage { get; set; }
        public bool IsActive { get; set; }
        public bool IsSolved { get; set; }
        public float RiseHeightPercent { get; set; } // 0.0 to 1.0 (fully risen)

        public int TotalStages
        {
            get { return Definition != null && Definition.RequiredChants != null ? Definition.RequiredChants.Count : 3; }
        }

        public CommandId CurrentRequiredCommand
        {
            get
            {
                if (Definition == null || Definition.RequiredChants == null || CurrentStage >= Definition.RequiredChants.Count)
                    return CommandId.March;
                return Definition.RequiredChants[CurrentStage];
            }
        }

        public MonolithInstance(MonolithDef def, float worldX)
        {
            Definition = def;
            WorldX = worldX;
            CurrentStage = 0;
            IsActive = false;
            IsSolved = false;
            RiseHeightPercent = 0f;
        }
    }

    public class MonolithInteractionResult
    {
        public bool StageAdvanced;
        public bool MonolithSolved;
        public int RewardGold;
        public string RewardItemId;
        public string UnlockedMissionId;
        public string Message;
    }

    /// <summary>
    /// Interactive Ancient Rhythm Monoliths & Totem Puzzles.
    /// Encountered across campaign missions and dungeon ruins. Matching the engraved
    /// rhythmic chants causes the monolith to rise from the earth, granting ancient
    /// treasures and unlocking secret missions.
    /// </summary>
    public static class RhythmMonolithSystem
    {
        private static readonly Dictionary<string, MonolithDef> _registry = new Dictionary<string, MonolithDef>();

        static RhythmMonolithSystem()
        {
            _registry["monolith-rain-shrine"] = new MonolithDef(
                "monolith-rain-shrine",
                "Ancient Rain Shrine Totem",
                "An ancient weathered stone carved with rain droplets and drum glyphs.",
                new List<CommandId> { CommandId.Defend, CommandId.Jump, CommandId.Retreat },
                300,
                "relic-rain-charm",
                "mission-rain-shrine"
            );

            _registry["monolith-ruin-totem"] = new MonolithDef(
                "monolith-ruin-totem",
                "Totem of the Earth Titan",
                "A towering obsidian pillar pulsating with seismic energy.",
                new List<CommandId> { CommandId.Charge, CommandId.Attack, CommandId.Jump },
                500,
                "mat-ancient-ore",
                "mission-titan-ruins"
            );

            _registry["monolith-celestial-pillar"] = new MonolithDef(
                "monolith-celestial-pillar",
                "Celestial Pillar of the Creator",
                "A radiant marble pillar inscribed with sacred heavenly harmonics.",
                new List<CommandId> { CommandId.Defend, CommandId.Charge, CommandId.Attack, CommandId.Jump },
                1000,
                "hero-mask-apex",
                "mission-celestial-summit"
            );
        }

        public static MonolithDef GetDef(string monolithId)
        {
            if (string.IsNullOrEmpty(monolithId)) return null;
            MonolithDef def;
            _registry.TryGetValue(monolithId, out def);
            return def;
        }

        public static List<MonolithDef> GetAllMonoliths()
        {
            return new List<MonolithDef>(_registry.Values);
        }

        public static MonolithInstance CreateInstance(string monolithId, float worldX)
        {
            var def = GetDef(monolithId);
            if (def == null) return null;
            return new MonolithInstance(def, worldX);
        }

        /// <summary>
        /// Evaluates a rhythm command against a nearby monolith.
        /// </summary>
        public static MonolithInteractionResult InteractWithMonolith(MonolithInstance monolith, float armyFrontX, CommandId issuedCommand, SaveData save = null)
        {
            var result = new MonolithInteractionResult();
            if (monolith == null || monolith.IsSolved || monolith.Definition == null)
            {
                result.Message = "Monolith is already solved or invalid.";
                return result;
            }

            // Check proximity (within 220 units)
            float dist = Math.Abs(armyFrontX - monolith.WorldX);
            if (dist > 220f)
            {
                result.Message = "Army is too far from the Ancient Monolith.";
                return result;
            }

            monolith.IsActive = true;
            CommandId required = monolith.CurrentRequiredCommand;

            if (issuedCommand == required)
            {
                monolith.CurrentStage++;
                monolith.RiseHeightPercent = (float)monolith.CurrentStage / monolith.TotalStages;
                result.StageAdvanced = true;

                if (monolith.CurrentStage >= monolith.TotalStages)
                {
                    monolith.IsSolved = true;
                    result.MonolithSolved = true;
                    result.RewardGold = monolith.Definition.RewardGold;
                    result.RewardItemId = monolith.Definition.RewardItemId;
                    result.UnlockedMissionId = monolith.Definition.UnlockMissionId;
                    result.Message = string.Format("The Ancient Monolith has fully risen from the earth! Unsealed ancient treasures!", monolith.Definition.Name);

                    // Apply rewards to save if provided
                    if (save != null)
                    {
                        save.Gold += result.RewardGold;
                        if (!string.IsNullOrEmpty(result.RewardItemId))
                        {
                            save.AddItem(result.RewardItemId, 1);
                        }
                        if (!string.IsNullOrEmpty(result.UnlockedMissionId) && !save.MissionsUnlocked.Contains(result.UnlockedMissionId))
                        {
                            save.MissionsUnlocked.Add(result.UnlockedMissionId);
                        }
                    }
                }
                else
                {
                    result.Message = string.Format("The Monolith resonates and rises higher ({0}/{1})!", monolith.CurrentStage, monolith.TotalStages);
                }
            }
            else
            {
                result.Message = string.Format("The Monolith vibrates disharmoniously. Expected {0} but received {1}.", required, issuedCommand);
            }

            return result;
        }
    }
}
