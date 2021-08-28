using NeoSaveGames.SceneManagement;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-neofpssceneswitcher.html")]
    public class NeoFpsSceneSwitcher : NeoSceneSwitcher
    {
        [Header ("NeoFPS")]
        [SerializeField, Tooltip("Should game data (eg, health and inventory) be persisted to the new scene?")]
        private bool m_PersistGameData = true;

        protected override void PreSceneSwitch()
        {
            // Save persistence data
            if (m_PersistGameData)
                FpsGameMode.SavePersistentData();
        }
    }
}