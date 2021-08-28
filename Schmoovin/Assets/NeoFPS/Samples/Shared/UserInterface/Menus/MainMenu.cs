using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class MainMenu : BaseMenu
	{
		[SerializeField] private MenuNavControls m_StartingNavControls = null;

        public override void Show ()
		{
			ShowNavControls (m_StartingNavControls);
			HidePanel ();
			base.Show ();
		}

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public void OnPressQuit ()
		{
#if UNITY_STANDALONE
            ConfirmationPopup.ShowPopup ("Are you sure you want to quit?", OnQuitYes, OnQuitNo);
#endif
		}

		void OnQuitYes ()
		{
			Application.Quit ();
		}

		void OnQuitNo ()
		{
		}
	}
}