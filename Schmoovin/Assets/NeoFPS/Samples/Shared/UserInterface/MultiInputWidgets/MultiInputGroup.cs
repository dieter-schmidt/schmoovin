using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class MultiInputGroup : MultiInputWidget, ISubmitHandler, IPointerClickHandler
	{
		[SerializeField] private RectTransform m_ExpandButton = null;
        [SerializeField] private bool m_StartExpanded = true;
		[SerializeField] private Image m_IconImage = null;
        [SerializeField] private GameObject[] m_Contents = new GameObject[0];

        private bool m_Expanded = false;
		public bool expanded
		{
			get { return m_Expanded; }
			set
			{
				m_Expanded = value;
				// Show / hide contents
				for (int i = 0; i < m_Contents.Length; ++i)
				{
					if (m_Contents [i] != null)
						m_Contents [i].SetActive (m_Expanded);
				}
				// Reset navigation
				if (parentList != null)
					parentList.ResetWidgetNavigation ();
				// Set button icon
				if (m_Expanded)
					m_IconImage.sprite = style.icons.collapse;
				else
					m_IconImage.sprite = style.icons.expand;
			}
		}

		public GameObject[] contents
		{
			get { return m_Contents; }
			set
			{
				m_Contents = value;
				// Show / hide contents
				for (int i = 0; i < m_Contents.Length; ++i)
				{
					if (m_Contents [i] != null)
						m_Contents [i].SetActive (m_Expanded);
				}
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			for (int i = 0; i < m_Contents.Length; ++i)
			{
				if (m_Contents [i] != null)
				{
					if (m_Contents [i].transform.parent != transform.parent)
					{
						m_Contents [i] = null;
						Debug.Log ("MenuEntryGroup contents must be heirarchy siblings of the group");
					}
				}
			}
		}
		#endif

		protected override void Start ()
		{
			base.Start ();
			expanded = m_StartExpanded;
		}

		public void OnSubmit (BaseEventData eventData)
		{
			expanded = !expanded;
			PlayAudio (MenuAudio.ClickValid);
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			// Check for increment / decrement buttons
			if (RectTransformUtility.RectangleContainsScreenPoint (m_ExpandButton, eventData.pressPosition))
			{
				expanded = !expanded;
				PlayAudio (MenuAudio.ClickValid);
			}
		}
	}
}

