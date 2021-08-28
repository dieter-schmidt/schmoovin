using NeoSaveGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class InGameMenuRootNavControls : MenuNavControls
	{
        [SerializeField, Tooltip("The save game button (disabled if not valid)")]
        private MultiInputButton m_SaveGameButton = null;

        public override void Show()
        {
            base.Show();
            if (m_SaveGameButton != null)
            {
                m_SaveGameButton.interactable = (SaveGameManager.canManualSave);
                m_SaveGameButton.RefreshInteractable();
            }
        }
    }
}