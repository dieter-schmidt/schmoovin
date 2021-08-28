using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public abstract class MenuNavControls : Focusable
	{
        [SerializeField] private UnityEvent m_OnBackButtonPressed = new UnityEvent();

		private MultiInputWidgetList m_List = null;

        public BaseMenu menu { get; private set; }

		public virtual void Initialise (BaseMenu menu)
		{
			this.menu = menu;
			m_List = GetComponentInParent<MultiInputWidgetList> ();
		}

		public virtual void Show ()
		{
			gameObject.SetActive (true);
            NeoFpsInputManager.PushEscapeHandler(Back);
		}

		void OnEnable ()
		{
			if (m_List != null)
				m_List.ResetWidgetNavigation ();
		}

		public virtual void Hide ()
		{
			gameObject.SetActive (false);
            NeoFpsInputManager.PopEscapeHandler(Back);
		}

		public virtual void Back ()
		{
			m_OnBackButtonPressed.Invoke ();
		}
	}
}