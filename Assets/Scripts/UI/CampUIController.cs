using System;
using System.Collections.Generic;
using RhythmArmy.Core.Data;
using RhythmArmy.Gameplay.Camp;

namespace RhythmArmy.UI
{
    /// <summary>
    /// UI Presentation Controller for the Camp Hub interface.
    /// Bridges CampHUDState, CampController, and user interactions across Barracks, Forge, Altar, and Mission Select.
    /// </summary>
    public class CampUIController
    {
        public CampController CampController { get; private set; }
        public CampHUDState HUDState
        {
            get { return CampController != null ? CampController.HUDState : null; }
        }

        // UI Event Callbacks
        public event Action<CampMenuTab> OnTabChanged;
        public event Action<UnitRosterCard> OnUnitSelected;
        public event Action<CraftingRecipeCard> OnRecipeSelected;
        public event Action<MissionStageCard> OnMissionSelected;

        public CampUIController(CampController campController)
        {
            if (campController == null) throw new ArgumentNullException("campController");
            CampController = campController;
        }

        public void SwitchTab(CampMenuTab tab)
        {
            if (HUDState == null) return;
            HUDState.CurrentTab = tab;
            if (OnTabChanged != null)
            {
                OnTabChanged(tab);
            }
        }

        public void SelectUnit(string unitId)
        {
            if (HUDState == null) return;
            HUDState.SelectUnit(unitId);
            if (HUDState.SelectedUnit != null && OnUnitSelected != null)
            {
                OnUnitSelected(HUDState.SelectedUnit);
            }
        }

        public void SelectRecipe(string recipeId)
        {
            if (HUDState == null) return;
            HUDState.SelectRecipe(recipeId);
            if (HUDState.SelectedRecipe != null && OnRecipeSelected != null)
            {
                OnRecipeSelected(HUDState.SelectedRecipe);
            }
        }

        public void SelectMission(string missionId)
        {
            if (HUDState == null) return;
            HUDState.SelectMission(missionId);
            if (HUDState.SelectedMission != null && OnMissionSelected != null)
            {
                OnMissionSelected(HUDState.SelectedMission);
            }
        }

        public bool RequestRecruit(UnitClass unitClass)
        {
            return CampController.RecruitUnit(unitClass);
        }

        public bool RequestCraft(string recipeId)
        {
            return CampController.CraftItem(recipeId);
        }

        public bool RequestEvolve(string unitId, Subspecies targetSpecies)
        {
            return CampController.EvolveUnit(unitId, targetSpecies);
        }

        public bool RequestAutoOptimize()
        {
            return CampController.AutoOptimizeAllUnits();
        }
    }
}
