using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames;
using NeoFPS.Samples;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputgame.html")]
	public class InputGame : FpsInput
    {
        protected override void UpdateInput()
        {
            if (GetButtonDown(FpsInputButton.QuickSave))
                SaveGameManager.QuickSave();

            if (GetButtonDown(FpsInputButton.QuickLoad))
                SaveGameManager.QuickLoad();

            if (GetButtonDown(FpsInputButton.QuickMenu))
                QuickOptionsPopup.ToggleVisible();
        }
    }
}