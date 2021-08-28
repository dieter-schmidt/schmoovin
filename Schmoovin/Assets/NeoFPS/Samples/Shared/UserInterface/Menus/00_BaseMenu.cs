using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeoFPS.Constants;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
	public abstract class BaseMenu : FpsInput
	{
		[SerializeField] private GameObject m_PopupSurround = null;
        [SerializeField] private bool m_ShowOnStart = false;

		private MenuNavControls m_CurrentNavControls = null;
        private MenuPanel m_CurrentPanel = null;
        private BasePopup m_CurrentPopup = null;
        private bool m_MenuPopup = false;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Menu; }
        }

        protected override void OnEnable()
        {
            // Remove base OnEnable to prevent pushing context
        }

        protected virtual void Start ()
        {
            // Hide popup surround
            m_PopupSurround.SetActive(false);

            // Initialised in Start in case panels rely on objects being awake
            MenuNavControls[] navControls = GetComponentsInChildren<MenuNavControls> (true);
			for (int i = 0; i < navControls.Length; ++i)
			{
				navControls [i].Initialise (this);
				navControls [i].gameObject.SetActive (false);
			}
			MenuPanel[] panels = GetComponentsInChildren<MenuPanel> (true);
			for (int i = 0; i < panels.Length; ++i)
			{
				panels [i].Initialise (this);
				panels [i].gameObject.SetActive (false);
			}
			BasePopup[] popups = GetComponentsInChildren<BasePopup> (true);
			for (int i = 0; i < popups.Length; ++i)
			{
				popups [i].Initialise (this);
				popups [i].gameObject.SetActive (false);
			}

			// Show if required
			if (m_ShowOnStart)
				Show ();
		}

		protected virtual void OnDestroy ()
		{
		}

		public virtual void Cancel ()
        {
			if (m_CurrentPopup != null)
				m_CurrentPopup.Back ();
			else
			{
				if (m_CurrentPanel != null)
					m_CurrentPanel.Back ();
				else
					m_CurrentNavControls.Back ();
			}
		}

		public virtual void Show ()
		{
			HidePopup ();
			ResetFocus ();
            
			PushContext();
		}

		public virtual void Hide ()
		{
			HidePanel ();
			HideNavControls ();
			HidePopup ();
			
			PopContext();
		}

        protected override void UpdateInput()
        {
            if (GetButtonDown(FpsInputButton.Cancel) || GetButtonDown(FpsInputButton.Back))
                Cancel();
        }

        public void ShowNavControls (MenuNavControls navControls)
		{
			if (m_CurrentNavControls != navControls)
			{
				if (m_CurrentNavControls != null)
					m_CurrentNavControls.Hide ();
				m_CurrentNavControls = navControls;
				if (m_CurrentNavControls != null)
					m_CurrentNavControls.Show ();
				ResetFocus ();
			}
		}

		public void HideNavControls ()
		{
			if (m_CurrentNavControls != null)
			{
				m_CurrentNavControls.Hide ();
				m_CurrentNavControls = null;
				ResetFocus ();
			}
		}

		public void ShowPanel (MenuPanel panel)
		{
			if (m_CurrentPanel != panel)
			{
				if (m_CurrentPanel != null)
					m_CurrentPanel.Hide ();
				m_CurrentPanel = panel;
				if (m_CurrentPanel != null)
					m_CurrentPanel.Show ();
				ResetFocus ();
			}
		}

		public void HidePanel ()
		{
			if (m_CurrentPanel != null)
			{
				m_CurrentPanel.Hide ();
				m_CurrentPanel = null;
				ResetFocus ();
			}
		}

		public void ShowPopup (BasePopup popup, bool dimBackground = true)
		{
			if (m_CurrentPopup != popup)
			{
                // Hide the previous popup
                HidePopup();

                // Set the new popup
                m_CurrentPopup = popup;
				if (m_CurrentPopup != null)
				{
					// Show the popup surround
					m_PopupSurround.SetActive (true);
					// Get the background image
					var bg = m_PopupSurround.GetComponent<Image>();
					if (bg != null)
						bg.enabled = dimBackground;
					// Show the new popup
					m_CurrentPopup.Show ();
                    // Push menu context if not returning to menu
                    m_MenuPopup = currentContext == FpsInputContext.Menu;
                    if (!m_MenuPopup)
                        PushContext();

					// Hide HUD elements
					HudHider.HideHUD();
                }

				ResetFocus ();
			}
		}

		public void HidePopup ()
		{
			if (m_CurrentPopup != null)
			{
				m_CurrentPopup.Hide ();
				m_CurrentPopup = null;

                // Pop menu context if not returning to menu
                if (!m_MenuPopup)
                    PopContext();

                // Hide the popup surround
                m_PopupSurround.SetActive(false);

				// Show HUD elements
				HudHider.ShowHUD();
			}
		}

		public void OnPopupSurroundPressed ()
		{
			if (m_CurrentPopup != null)
				m_CurrentPopup.Back ();
		}

		public void SetFocus (Focusable focusable)
		{
			if (focusable != null)
				focusable.hasFocus = true;
			else
				Focusable.ClearFocus ();
		}

		public void ResetFocus ()
		{
			if (m_CurrentPopup != null)
				m_CurrentPopup.hasFocus = true;
			else
			{
				if (m_CurrentPanel != null)
					m_CurrentPanel.hasFocus = true;
				else
				{
					if (m_CurrentNavControls != null)
						m_CurrentNavControls.hasFocus = true;
				}
			}
		}
	}
}