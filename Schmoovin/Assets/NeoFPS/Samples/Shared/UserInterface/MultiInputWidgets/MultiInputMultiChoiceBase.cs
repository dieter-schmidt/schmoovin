using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeoFPS.Samples
{
	public abstract class MultiInputMultiChoiceBase : MultiInputFocusableWidget, IPointerClickHandler
	{
		[SerializeField] private RectTransform m_PrevButton = null;
        [SerializeField] private RectTransform m_NextButton = null;
        [SerializeField] private Text m_Readout = null;
        [SerializeField] private string[] m_Options = new string[0];

		public string[] options
		{
			get { return m_Options; }
			set
			{
				m_Options = value;
				CheckOptions ();
			}
		}

		private int m_Index = -1;
		public int index
		{
			get { return m_Index; }
			set
			{
				int old = m_Index;
				m_Index = value;
				CheckOptions ();
				if (m_Index != old)
					OnIndexChanged (m_Index);
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			if (m_Options != null && m_Readout != null && m_Options.Length != 0)
				m_Readout.text = m_Options [0];
		}
		#endif

		protected override void Start ()
		{
			base.Start ();
			CheckOptions ();
		}

		void CheckOptions ()
		{
			// Check for empty
			if (m_Options == null || m_Options.Length == 0)
			{
				m_Options = new string[] { "Undefined" };
				CheckIndex ();
			}
			else
			{
				CheckIndex ();
			}
		}

		void CheckIndex ()
		{
			m_Index = Mathf.Clamp (m_Index, 0, m_Options.Length - 1);
			if (m_Readout != null)
				m_Readout.text = m_Options[m_Index];
		}

		protected virtual void SetStartingIndex (int to)
		{
            m_Index = to;
            CheckOptions ();
		}

		protected virtual void OnIndexChanged (int to)
		{
		}

		public override void FocusLeft ()
		{
			Previous ();
		}

		public override void FocusRight ()
		{
			Next ();
		}

		public override void OnSubmit (UnityEngine.EventSystems.BaseEventData eventData)
		{
			if (widgetState == WidgetState.Focussed)
			{
				widgetState = WidgetState.Highlighted;
				PlayAudio (MenuAudio.ClickValid);
			}
			else
				base.OnSubmit (eventData);
		}

		public void Next ()
		{
			if (index < m_Options.Length - 1)
			{
				++index;
				PlayAudio (MenuAudio.ClickValid);
				// Highlight left button
			}
			else
				PlayAudio (MenuAudio.ClickInvalid);
		}

		public void Previous ()
		{
			if (index > 0)
			{
				--index;
				PlayAudio (MenuAudio.ClickValid);
				// Highlight right button
			}
			else
				PlayAudio (MenuAudio.ClickInvalid);
		}

		public void CycleSelection ()
		{
			int i = index + 1;
			if (i >= m_Options.Length)
				i -= m_Options.Length;
			index = i;
			PlayAudio (MenuAudio.ClickValid);
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			Vector2 pressPosition = eventData.pressPosition;

			// Check for increment / decrement buttons
			if (RectTransformUtility.RectangleContainsScreenPoint (m_PrevButton, pressPosition))
			{
				Previous ();
				return;
			}
			if (RectTransformUtility.RectangleContainsScreenPoint (m_NextButton, pressPosition))
			{
				Next ();
				return;
			}
		}

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(DelayedAlign());
        }

        IEnumerator DelayedAlign()
        {
            yield return null;
            // Fix child rects randomly resizing
            Transform t = transform;
            if (t.childCount == 2)
            {
                RectTransform rt = (RectTransform)t.GetChild(1);
                rt.anchoredPosition = new Vector2(1f, 0f);
            }
        }
    }
}