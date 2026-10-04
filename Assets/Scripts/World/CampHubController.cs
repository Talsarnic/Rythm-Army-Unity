using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmArmy.World
{
    public enum CampStation
    {
        TownSquare,
        Barracks,
        Blacksmith,
        SpiritAltar,
        SacredTree,
        CampfireFeast,
        HeroShrine,
        MerchantMarket,
        MissionWarTable
    }

    public class CampHubController : MonoBehaviour
    {
        [SerializeField] private string campaignMapScene = "WorldMap";
        [SerializeField] private string defaultCampScene = "Camp";

        public CampStation CurrentStation { get; private set; } = CampStation.TownSquare;

        public event Action<CampStation> StationOpened;
        public event Action<string> CampaignRequested;

        public void OpenStation(CampStation station)
        {
            CurrentStation = station;
            StationOpened?.Invoke(station);
        }

        public void OpenTownSquare() => OpenStation(CampStation.TownSquare);
        public void OpenBarracks() => OpenStation(CampStation.Barracks);
        public void OpenBlacksmith() => OpenStation(CampStation.Blacksmith);
        public void OpenSpiritAltar() => OpenStation(CampStation.SpiritAltar);
        public void OpenSacredTree() => OpenStation(CampStation.SacredTree);
        public void OpenCampfireFeast() => OpenStation(CampStation.CampfireFeast);
        public void OpenHeroShrine() => OpenStation(CampStation.HeroShrine);
        public void OpenMerchantMarket() => OpenStation(CampStation.MerchantMarket);
        public void OpenMissionWarTable() => OpenStation(CampStation.MissionWarTable);

        public void ChooseNextCampaignLevel(string levelId)
        {
            var level = CampaignLevelDatabase.Get(levelId);
            if (level == null) return;

            CampaignRequested?.Invoke(levelId);
            if (!string.IsNullOrEmpty(level.SceneName))
                SceneManager.LoadScene(level.SceneName);
        }

        public void OpenCampaignMap()
        {
            if (!string.IsNullOrEmpty(campaignMapScene))
                SceneManager.LoadScene(campaignMapScene);
        }

        public void ReturnToCamp()
        {
            if (!string.IsNullOrEmpty(defaultCampScene))
                SceneManager.LoadScene(defaultCampScene);
        }
    }
}
