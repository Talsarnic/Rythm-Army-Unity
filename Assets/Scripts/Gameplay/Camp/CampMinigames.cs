using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Rhythm;
using RhythmArmy.Core.Save;

namespace RhythmArmy.Gameplay.Camp
{
    public enum MinigameType
    {
        TreeOfLife,     // Sacred Tree rhythm dance (Herbs, Timbers, Sap)
        BlacksmithAnvil,// Rhythmic anvil forging (Ingots, Alloys, Mithril)
        CampChefStew    // Campfire culinary rhythm minigame (Tough Jerky, Sun Cabbage, Stews)
    }

    public class MinigameRoundPrompt
    {
        public DrumId[] Sequence;
        public string PromptText;
        public float Bpm;

        public MinigameRoundPrompt(DrumId[] sequence, string promptText, float bpm = 120f)
        {
            Sequence = sequence;
            PromptText = promptText;
            Bpm = bpm;
        }
    }

    public class MinigameResult
    {
        public MinigameType Type;
        public int RoundsCompleted;
        public int TotalRounds;
        public int PerfectHits;
        public int GoodHits;
        public int MissHits;
        public List<InventoryEntry> RewardsEarned = new List<InventoryEntry>();
        public bool IsSuccess { get { return RoundsCompleted > 0 && MissHits < 3; } }
    }

    /// <summary>
    /// Handles interactive Camp Rhythm Minigames for gathering crafting ingredients without spending gold.
    /// </summary>
    public class CampMinigameManager
    {
        public MinigameType CurrentMinigame { get; private set; }
        public bool IsActive { get; private set; }
        public int CurrentRoundIndex { get; private set; }
        public int TotalRounds { get; private set; }
        public MinigameRoundPrompt CurrentPrompt { get; private set; }
        public List<DrumId> PlayerInputBuffer { get; private set; }

        private readonly List<MinigameRoundPrompt> _treePrompts = new List<MinigameRoundPrompt>
        {
            new MinigameRoundPrompt(new[] { DrumId.Boom, DrumId.Tak, DrumId.Boom, DrumId.Tak }, "Tree rustles gently: BOOM - TAK - BOOM - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Boom, DrumId.Boom, DrumId.Tak, DrumId.Tak }, "Tree sways happily: BOOM - BOOM - TAK - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Rat, DrumId.Rat, DrumId.Boom, DrumId.Tak }, "Tree dances vibrantly: RAT - RAT - BOOM - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Ting, DrumId.Ting, DrumId.Boom, DrumId.Tak }, "Tree reaches to sky: TING - TING - BOOM - TAK")
        };

        private readonly List<MinigameRoundPrompt> _anvilPrompts = new List<MinigameRoundPrompt>
        {
            new MinigameRoundPrompt(new[] { DrumId.Boom, DrumId.Boom, DrumId.Boom, DrumId.Tak }, "Blacksmith strikes iron: BOOM - BOOM - BOOM - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Tak, DrumId.Tak, DrumId.Boom, DrumId.Tak }, "Hammering hot metal: TAK - TAK - BOOM - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Tak, DrumId.Tak, DrumId.Rat, DrumId.Rat }, "Tempering with bellows: TAK - TAK - RAT - RAT"),
            new MinigameRoundPrompt(new[] { DrumId.Ting, DrumId.Ting, DrumId.Ting, DrumId.Tak }, "Forging divine edge: TING - TING - TING - TAK")
        };

        private readonly List<MinigameRoundPrompt> _chefPrompts = new List<MinigameRoundPrompt>
        {
            new MinigameRoundPrompt(new[] { DrumId.Boom, DrumId.Rat, DrumId.Boom, DrumId.Rat }, "Chopping fresh ingredients: BOOM - RAT - BOOM - RAT"),
            new MinigameRoundPrompt(new[] { DrumId.Tak, DrumId.Boom, DrumId.Tak, DrumId.Boom }, "Stirring the bubbling cauldron: TAK - BOOM - TAK - BOOM"),
            new MinigameRoundPrompt(new[] { DrumId.Rat, DrumId.Rat, DrumId.Tak, DrumId.Tak }, "Adding secret seasonings: RAT - RAT - TAK - TAK"),
            new MinigameRoundPrompt(new[] { DrumId.Ting, DrumId.Boom, DrumId.Ting, DrumId.Tak }, "Simmering the gourmet feast: TING - BOOM - TING - TAK")
        };

        private int _perfectHits;
        private int _goodHits;
        private int _missHits;
        private readonly List<InventoryEntry> _collectedRewards = new List<InventoryEntry>();

        public event Action<MinigameRoundPrompt> OnRoundStarted;
        public event Action<bool, InventoryEntry> OnRoundResolved;
        public event Action<MinigameResult> OnMinigameFinished;

        public CampMinigameManager()
        {
            PlayerInputBuffer = new List<DrumId>();
        }

        public void StartMinigame(MinigameType type)
        {
            CurrentMinigame = type;
            IsActive = true;
            CurrentRoundIndex = 0;
            _perfectHits = 0;
            _goodHits = 0;
            _missHits = 0;
            _collectedRewards.Clear();
            PlayerInputBuffer.Clear();

            TotalRounds = type == MinigameType.TreeOfLife
                ? _treePrompts.Count
                : (type == MinigameType.BlacksmithAnvil ? _anvilPrompts.Count : _chefPrompts.Count);
            StartNextRound();
        }

        private void StartNextRound()
        {
            if (CurrentRoundIndex >= TotalRounds)
            {
                FinishMinigame();
                return;
            }

            PlayerInputBuffer.Clear();
            if (CurrentMinigame == MinigameType.TreeOfLife)
            {
                CurrentPrompt = _treePrompts[CurrentRoundIndex];
            }
            else if (CurrentMinigame == MinigameType.BlacksmithAnvil)
            {
                CurrentPrompt = _anvilPrompts[CurrentRoundIndex];
            }
            else
            {
                CurrentPrompt = _chefPrompts[CurrentRoundIndex];
            }

            if (OnRoundStarted != null)
            {
                OnRoundStarted.Invoke(CurrentPrompt);
            }
        }

        public bool SubmitDrumInput(DrumId drum)
        {
            if (!IsActive || CurrentPrompt == null) return false;

            PlayerInputBuffer.Add(drum);

            // Check if 4 beats completed
            if (PlayerInputBuffer.Count == 4)
            {
                EvaluateRoundInput();
                return true;
            }

            return false;
        }

        private void EvaluateRoundInput()
        {
            bool match = true;
            for (int i = 0; i < 4; i++)
            {
                if (PlayerInputBuffer[i] != CurrentPrompt.Sequence[i])
                {
                    match = false;
                    break;
                }
            }

            InventoryEntry roundReward = null;
            if (match)
            {
                _perfectHits += 4;
                roundReward = GenerateRoundReward(CurrentMinigame, CurrentRoundIndex);
                if (roundReward != null)
                {
                    _collectedRewards.Add(roundReward);
                }
            }
            else
            {
                _missHits += 2;
                _goodHits += 2;
            }

            CurrentRoundIndex++;

            if (OnRoundResolved != null)
            {
                OnRoundResolved.Invoke(match, roundReward);
            }

            StartNextRound();
        }

        private InventoryEntry GenerateRoundReward(MinigameType type, int roundIndex)
        {
            if (type == MinigameType.TreeOfLife)
            {
                switch (roundIndex)
                {
                    case 0: return new InventoryEntry("mat-timber", 4);
                    case 1: return new InventoryEntry("mat-healing-herb", 3);
                    case 2: return new InventoryEntry("mat-hardwood", 2);
                    case 3: return new InventoryEntry("mat-sacred-sap", 1);
                    default: return new InventoryEntry("mat-timber", 2);
                }
            }
            else if (type == MinigameType.BlacksmithAnvil)
            {
                switch (roundIndex)
                {
                    case 0: return new InventoryEntry("mat-stone", 4);
                    case 1: return new InventoryEntry("mat-iron-ingot", 3);
                    case 2: return new InventoryEntry("mat-steel-plate", 2);
                    case 3: return new InventoryEntry("mat-mithril", 1);
                    default: return new InventoryEntry("mat-iron-ingot", 2);
                }
            }
            else // CampChefStew
            {
                switch (roundIndex)
                {
                    case 0: return new InventoryEntry("mat-sun-cabbage", 4);
                    case 1: return new InventoryEntry("mat-jerky", 3);
                    case 2: return new InventoryEntry("food-simple-stew", 1);
                    case 3: return new InventoryEntry("food-divine-feast", 1);
                    default: return new InventoryEntry("food-simple-stew", 1);
                }
            }
        }

        private void FinishMinigame()
        {
            IsActive = false;
            var result = new MinigameResult
            {
                Type = CurrentMinigame,
                RoundsCompleted = CurrentRoundIndex,
                TotalRounds = TotalRounds,
                PerfectHits = _perfectHits,
                GoodHits = _goodHits,
                MissHits = _missHits,
                RewardsEarned = new List<InventoryEntry>(_collectedRewards)
            };

            if (OnMinigameFinished != null)
            {
                OnMinigameFinished.Invoke(result);
            }
        }

        public void ApplyRewardsToSave(SaveData save, MinigameResult result)
        {
            if (save == null || result == null || result.RewardsEarned == null) return;
            foreach (var reward in result.RewardsEarned)
            {
                save.AddItem(reward.ItemId, reward.Quantity);
            }
        }
    }
}
