using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmArmy.World
{
    public class LevelFlowController : MonoBehaviour
    {
        [SerializeField] private string returnScene = "Camp";

        public string ActiveLevelId { get; private set; }
        public CampaignLevelDef ActiveLevel => CampaignLevelDatabase.Get(ActiveLevelId);

        public event Action<CampaignLevelDef> LevelStarted;
        public event Action<CampaignLevelDef> LevelCompleted;
        public event Action<CampaignLevelDef> LevelFailed;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void StartLevel(string levelId)
        {
            var level = CampaignLevelDatabase.Get(levelId);
            if (level == null) return;

            ActiveLevelId = levelId;
            LevelStarted?.Invoke(level);
        }

        public void CompleteLevel()
        {
            var level = ActiveLevel;
            if (level == null) return;

            LevelCompleted?.Invoke(level);
        }

        public void FailLevel()
        {
            var level = ActiveLevel;
            if (level == null) return;

            LevelFailed?.Invoke(level);
        }

        public void ReturnToCamp()
        {
            if (!string.IsNullOrEmpty(returnScene))
                SceneManager.LoadScene(returnScene);
        }
    }
}
