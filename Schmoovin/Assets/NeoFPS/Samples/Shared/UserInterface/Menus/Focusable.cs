using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class Focusable : MonoBehaviour
	{
		[SerializeField] private Selectable m_StartingSelection = null;

        private static Focusable m_Focus = null;
        public bool hasFocus
		{
			get { return m_Focus == this; }
			set
			{
				if (value)
				{
					// Set selection
					if (m_StartingSelection != null && EventSystem.current != null)
						EventSystem.current.SetSelectedGameObject (m_StartingSelection.gameObject);
					// Take focus
					if (m_Focus != this)
					{
						m_Focus = this;
						OnTakeFocus ();
					}
				}
				else
				{
					// Lose focus
					if (m_Focus == this)
					{
						OnLoseFocus ();
						m_Focus = null;
					}
				}
			}
		}

        protected Selectable startingSelection
        {
            get { return m_StartingSelection; }
            set { m_StartingSelection = value; }
        }

		void OnDisable ()
		{
			if (hasFocus)
				hasFocus = false;
        }

		protected virtual void OnTakeFocus () {}
		protected virtual void OnLoseFocus () {}

		public static void ClearFocus ()
		{
			if (m_Focus != null)
				m_Focus.hasFocus = false;
		}
	}
}