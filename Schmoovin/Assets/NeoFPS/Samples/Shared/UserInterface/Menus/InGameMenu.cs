using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    [RequireComponent (typeof (InGameMenuBackground))]
	public class InGameMenu : BaseMenu
	{
		[SerializeField] private MenuNavControls m_StartingNavControls = null;
        [SerializeField] private CanvasGroup m_HudGroup = null;
        [SerializeField] private InGameMenuBackground m_Background = null;
        [SerializeField] private int m_MainMenuScene = 0;
		[SerializeField][Range (0f, 1f)] private float m_HudAlpha = 0.25f;
        [SerializeField] private bool m_PauseGame = false;

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Background == null)
				m_Background = GetComponentInChildren<InGameMenuBackground> ();
		}
		#endif

		protected override void Start ()
		{
			base.Start ();
			NeoFpsInputManager.PushEscapeHandler (Show);
			if (m_Background != null)
				m_Background.gameObject.SetActive (false);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			NeoFpsInputManager.PopEscapeHandler (Show);

            if (m_PauseGame && NeoFpsTimeScale.isPaused)
                NeoFpsTimeScale.ResumeTime();
        }

		public override void Show ()
        {
            NeoFpsInputManager.PushEscapeHandler(Hide);
			ShowNavControls (m_StartingNavControls);
			HidePanel ();
			base.Show ();
			CaptureInput ();
			// Fade Hud
			if (m_HudGroup != null)
				m_HudGroup.alpha = m_HudAlpha;

            if (m_PauseGame)
                NeoFpsTimeScale.FreezeTime();
        }

		public override void Hide ()
        {
			base.Hide ();
			ReleaseInput ();
			// Show Hud
			if (m_HudGroup != null)
				m_HudGroup.alpha = 1f;
            NeoFpsInputManager.PopEscapeHandler(Hide);

            if (m_PauseGame && NeoFpsTimeScale.isPaused)
                NeoFpsTimeScale.ResumeTime();
        }

		public void OnPressExit ()
		{
			ConfirmationPopup.ShowPopup ("Are you sure you want to quit?", OnExitYes, OnExitNo);
		}

		void OnExitYes ()
		{
			SceneManager.LoadScene (m_MainMenuScene);
		}

		void OnExitNo ()
		{
		}

		void CaptureInput ()
		{
			if (m_Background != null)
				m_Background.gameObject.SetActive (true);
		}

		void ReleaseInput ()
		{
			m_Background.gameObject.SetActive (false);
		}
	}
}