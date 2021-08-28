using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
	public class MultiInputButtonGroup : MultiInputFocusableWidget, IPointerClickHandler
	{
        [SerializeField] private ButtonInfo[] m_Buttons = new ButtonInfo[0];
		[SerializeField] private bool m_DefocusOnPress = false;

		private Image[] m_ButtonImages = null;

        private int m_CurrIndex = 0;
		public int currentIndex
		{
			get { return m_CurrIndex; }
			set
			{
				m_CurrIndex = Mathf.Clamp (value, 0, m_Buttons.Length);
				for (int i = 0; i < m_ButtonImages.Length; ++i)
				{
					if (i == currentIndex)
						m_ButtonImages [i].color = style.colours.focussed;
					else
						m_ButtonImages [i].color = style.colours.normal;
				}
			}
		}

		[Serializable]
		public struct ButtonInfo
		{
			public RectTransform rect;
			public UnityEvent onClick;
		}

		public ButtonInfo[] buttons
		{
			get { return m_Buttons; }
		}

		public override void FocusLeft ()
		{
			int newIndex = currentIndex - 1;
			if (newIndex < 0)
				newIndex = m_Buttons.Length - 1;
			currentIndex = newIndex;
			base.FocusLeft ();
		}

		public override void FocusRight ()
		{
			int newIndex = currentIndex + 1;
			if (newIndex >= m_Buttons.Length)
				newIndex = 0;
			currentIndex = newIndex;
			base.FocusRight ();
		}

		protected override void Awake ()
		{
			base.Awake ();

			m_ButtonImages = new Image[m_Buttons.Length];
			for (int i = 0; i < m_Buttons.Length; ++i)
			{
				if (m_Buttons [i].rect != null)
					m_ButtonImages [i] = m_Buttons [i].rect.GetComponent<Image> ();
			}
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			// Check press position against each button
			for (int i = 0; i < m_Buttons.Length; ++i)
			{
				if (m_Buttons[i].rect != null && RectTransformUtility.RectangleContainsScreenPoint (m_Buttons[i].rect, eventData.pressPosition))
				{
					m_Buttons [i].onClick.Invoke ();
					PlayAudio (MenuAudio.ClickValid);
					break;
				}
			}
		}

		public override void OnSubmit (BaseEventData eventData)
		{
			if (widgetState == WidgetState.Focussed)
			{
				// Invoke event handler
				m_Buttons [currentIndex].onClick.Invoke ();
				// Show press (and remove focus if required)
				if (m_DefocusOnPress)
				{
					PulseColour (m_ButtonImages [currentIndex], style.colours.pressed, style.colours.highlighted);
					RemoveFocus ();
				}
				else
					PulseColour (m_ButtonImages [currentIndex], style.colours.pressed, style.colours.focussed);
				// Play audio
				PlayAudio (MenuAudio.ClickValid);
			}
			else
				base.OnSubmit (eventData);
		}

		protected override void OnTakeFocus ()
		{
			base.OnTakeFocus ();
			currentIndex = 0;
		}

		protected override void OnLoseFocus ()
		{
			base.OnLoseFocus ();
			if (style != null)
			{
				for (int i = 0; i < m_ButtonImages.Length; ++i)
				{
					if (widgetState == WidgetState.Highlighted)
						m_ButtonImages [i].color = style.colours.highlighted;
					else
						m_ButtonImages [i].color = style.colours.normal;
				}
			}
		}
	}
}