using System;
using System.Collections.Generic;
using System.Linq;
using RhythmArmy.Core.Combat;
using RhythmArmy.Core.Data;
using RhythmArmy.Core.Save;
using RhythmArmy.Gameplay.Battle;
using RhythmArmy.Gameplay.Camp;
using RhythmArmy.Visuals;

namespace RhythmArmy.UI
{
    public enum CampMenuTab
    {
        MainCamp,
        Barracks,
        Blacksmith,
        SpiritAltar,
        MissionSelect,
        InventoryView,
        Settings
    }

    public class MaterialCostEntry
    {
        public string ItemId;
        public string ItemName;
        public int Quantity;

        public MaterialCostEntry(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
            var item = ItemDatabase.Get(itemId);
            ItemName = item != null ? item.Name : itemId;
        }
    }

    public class UnitRosterCard
    {
        public string Id;
        public string Name;
        public UnitClass Class;
        public Subspecies Species;
        public int Level;
        public int GearScore;
        public float MaxHp;
        public float AttackPower;
        public float Defense;
        public string WeaponId;
        public string ShieldId;
        public string HelmetId;
        public bool IsEquipped;

        // Mater Tree Awakening
        public int AwakeningTier;
        public int AvailableMasteryPoints;
        public int AttackMastery;
        public int DefenseMastery;
        public int HealthMastery;
        public int SpeedMastery;
        public Subspecies SecondaryFusion;
    }

    public class CraftingRecipeCard
    {
        public string ResultItemId;
        public string ResultItemName;
        public ItemType ItemType;
        public ItemRarity Rarity;
        public int GearScore;
        public int GoldCost;
        public List<MaterialCostEntry> RequiredMaterials = new List<MaterialCostEntry>();
        public bool CanCraft;
    }

    public class EvolutionOptionCard
    {
        public Subspecies TargetSpecies;
        public string SpeciesName;
        public string StatPerksDescription;
        public int GoldCost;
        public List<MaterialCostEntry> RequiredMaterials = new List<MaterialCostEntry>();
        public bool CanEvolve;
    }

    public class MissionStageCard
    {
        public string MissionId;
        public string StageNumber;
        public string Title;
        public string Subtitle;
        public BiomeType Biome;
        public string BiomeName;
        public int RecommendedGearScore;
        public int GoldReward;
        public List<string> EnemyPreviews = new List<string>();
        public List<string> GuaranteedLoot = new List<string>();
        public bool IsUnlocked;
        public bool IsCleared;
    }

    public class CampHUDState
    {
        public CampMenuTab CurrentTab = CampMenuTab.MainCamp;
        public int CurrentGold;

        // Barracks state
        public List<UnitRosterCard> RosterUnits = new List<UnitRosterCard>();
        public UnitRosterCard SelectedUnit = null;

        // Blacksmith state
        public List<CraftingRecipeCard> AvailableRecipes = new List<CraftingRecipeCard>();
        public CraftingRecipeCard SelectedRecipe = null;

        // Spirit Altar state
        public List<EvolutionOptionCard> EvolutionBranches = new List<EvolutionOptionCard>();
        public EvolutionOptionCard SelectedEvolution = null;

        // Mission Select state
        public List<MissionStageCard> MissionCards = new List<MissionStageCard>();
        public MissionStageCard SelectedMission = null;

        // Boss Hunting Trials state
        public List<BossHuntOption> BossTrials = new List<BossHuntOption>();
        public BossHuntOption SelectedBossTrial = null;

        public void RefreshFromSave(SaveData save)
        {
            if (save == null) return;
            CurrentGold = save.Gold;

            // Refresh Roster
            RosterUnits.Clear();
            foreach (var member in save.Roster)
            {
                var stats = CombatFormulas.CalculateMemberStats(member);
                int gs = CombatFormulas.CalculateGearScore(member);

                var card = new UnitRosterCard
                {
                    Id = member.Id,
                    Name = member.Name,
                    Class = member.Class,
                    Species = member.Species,
                    Level = member.Level,
                    GearScore = gs,
                    MaxHp = stats.Hp,
                    AttackPower = stats.Attack,
                    Defense = stats.Defense,
                    WeaponId = member.WeaponId,
                    ShieldId = member.ShieldId,
                    HelmetId = member.HelmetId,
                    IsEquipped = true,
                    AwakeningTier = member.AwakeningTier,
                    AvailableMasteryPoints = MaterTreeAwakening.GetAvailablePoints(member),
                    AttackMastery = member.AttackMastery,
                    DefenseMastery = member.DefenseMastery,
                    HealthMastery = member.HealthMastery,
                    SpeedMastery = member.SpeedMastery,
                    SecondaryFusion = member.SecondaryFusion
                };

                RosterUnits.Add(card);
            }

            if (RosterUnits.Count > 0 && SelectedUnit == null)
            {
                SelectedUnit = RosterUnits[0];
            }

            // Refresh Blacksmith Recipes
            AvailableRecipes.Clear();
            foreach (var recipe in BlacksmithCrafting.AllRecipes)
            {
                var item = ItemDatabase.Get(recipe.ResultItemId);
                if (item == null) continue;

                bool canCraft = BlacksmithCrafting.CanCraft(recipe, save);
                var recipeCard = new CraftingRecipeCard
                {
                    ResultItemId = recipe.ResultItemId,
                    ResultItemName = item.Name,
                    ItemType = item.Type,
                    Rarity = item.Rarity,
                    GearScore = item.GearScore,
                    GoldCost = recipe.GoldCost,
                    CanCraft = canCraft
                };

                foreach (var mat in recipe.RequiredMaterials)
                {
                    recipeCard.RequiredMaterials.Add(new MaterialCostEntry(mat.Key, mat.Value));
                }

                AvailableRecipes.Add(recipeCard);
            }

            // Refresh Mission Cards
            MissionCards.Clear();
            var allMissions = MissionDatabase.GetAll();
            for (int i = 0; i < allMissions.Count; i++)
            {
                var m = allMissions[i];
                var biomeType = EnvironmentSystem.ParseBiome(m.Biome);
                var biomeTheme = EnvironmentSystem.CreateBiomeTheme(biomeType);

                bool isUnlocked = save.MissionsUnlocked.Contains(m.Id) || (i == 0);
                bool isCleared = save.MissionsCleared.Contains(m.Id);

                var stageCard = new MissionStageCard
                {
                    MissionId = m.Id,
                    StageNumber = string.Format("STAGE {0}", i + 1),
                    Title = m.Name,
                    Subtitle = m.Blurb,
                    Biome = biomeType,
                    BiomeName = biomeTheme.DisplayName,
                    RecommendedGearScore = m.RecommendedGearScore,
                    GoldReward = m.GoldReward,
                    IsUnlocked = isUnlocked,
                    IsCleared = isCleared,
                    GuaranteedLoot = new List<string>(m.LootTable)
                };

                foreach (var wave in m.Waves)
                {
                    foreach (var spawn in wave.Spawns)
                    {
                        string enemyName = spawn.Kind.ToString();
                        if (!stageCard.EnemyPreviews.Contains(enemyName))
                        {
                            stageCard.EnemyPreviews.Add(enemyName);
                        }
                    }
                }

                MissionCards.Add(stageCard);
            }

            if (MissionCards.Count > 0 && SelectedMission == null)
            {
                SelectedMission = MissionCards.FirstOrDefault(m => m.IsUnlocked) ?? MissionCards[0];
            }

            // Refresh Boss Trials
            BossTrials = BossHuntingSystem.GetAvailableBossHunts(save);
            if (BossTrials.Count > 0 && SelectedBossTrial == null)
            {
                SelectedBossTrial = BossTrials[0];
            }
        }

        public void SelectUnit(string unitId)
        {
            SelectedUnit = RosterUnits.FirstOrDefault(u => u.Id == unitId);
            RefreshAltarEvolutionBranches();
        }

        public void SelectRecipe(string recipeId)
        {
            SelectedRecipe = AvailableRecipes.FirstOrDefault(r => r.ResultItemId == recipeId);
        }

        public void SelectMission(string missionId)
        {
            SelectedMission = MissionCards.FirstOrDefault(m => m.MissionId == missionId);
        }

        public void SelectBossTrial(EnemyKind bossKind)
        {
            SelectedBossTrial = BossTrials.FirstOrDefault(b => b.BossKind == bossKind);
        }

        public void SelectTab(CampMenuTab tab)
        {
            CurrentTab = tab;
            if (tab == CampMenuTab.SpiritAltar)
            {
                RefreshAltarEvolutionBranches();
            }
        }

        private void RefreshAltarEvolutionBranches()
        {
            EvolutionBranches.Clear();
            if (SelectedUnit == null) return;

            var subspeciesList = (Subspecies[])Enum.GetValues(typeof(Subspecies));
            foreach (var sub in subspeciesList)
            {
                if (sub == Subspecies.Normal) continue;

                EvolutionRequirement recipe;
                if (AltarEvolution.EvolutionRecipes.TryGetValue(sub, out recipe))
                {
                    var evoCard = new EvolutionOptionCard
                    {
                        TargetSpecies = sub,
                        SpeciesName = sub.ToString(),
                        StatPerksDescription = recipe.PerksDescription,
                        GoldCost = recipe.GoldCost,
                        CanEvolve = (SelectedUnit.Species != sub)
                    };

                    foreach (var mat in recipe.Materials)
                    {
                        evoCard.RequiredMaterials.Add(new MaterialCostEntry(mat.Key, mat.Value));
                    }

                    EvolutionBranches.Add(evoCard);
                }
            }
        }
    }
}
