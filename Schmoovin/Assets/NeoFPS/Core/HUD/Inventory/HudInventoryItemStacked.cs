using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventoryitemstacked.html")]
	public class HudInventoryItemStacked : HudInventoryItemBase
	{
		[SerializeField, Tooltip("A colour for the item in the HUD when selected.")]
		private Color m_ColourSelected = Color.cyan;

		[SerializeField, Tooltip("A colour for the item in the HUD when not selected.")]
		private Color m_ColourUnselected = Color.white;

		private CanvasGroup m_CanvasGroup = null;
		private bool m_primary = false;
		private bool m_StackSelected = false;

		public RectTransform m_LocalTransform
		{
			get;
			private set;
		}
		
		public bool primary
		{
			get { return m_primary; }
			set
			{
				m_primary = value;
				gameObject.SetActive(m_primary || (m_StackSelected && item != null));
				slotText.gameObject.SetActive (m_primary);
			}
		}

		public bool stackSelected
		{
			get { return m_StackSelected;  }
			set
			{
				m_StackSelected = value;
				if (m_StackSelected && !gameObject.activeSelf && item != null)
					gameObject.SetActive (true);
				if (!m_StackSelected && gameObject.activeSelf && !primary)
					gameObject.SetActive (false);
			}
		}

		public void SetAsLastSibling()
		{
			m_LocalTransform.SetAsLastSibling();
		}
		
		public override void SetItem(IQuickSlotItem item)
		{
			base.SetItem(item);
		}

		public void Initialise(HudInventoryStackedSlot stack, bool p, int stackIndex)
		{
			m_LocalTransform = transform as RectTransform;
			m_CanvasGroup = GetComponent<CanvasGroup>();
			primary = p;
			slotIndex = stackIndex;
			iconImage.color = m_ColourUnselected;
		}

		protected override void OnSelect ()
		{
			base.OnSelect ();
			iconImage.color = m_ColourSelected;
		}

		protected override void OnDeselect ()
		{
			base.OnDeselect ();
			iconImage.color = m_ColourUnselected;
		}

		public void SetVisibility(float v)
		{
			if (item == null)
				v = 0f;

			if (isActiveAndEnabled && v == 0f)
				gameObject.SetActive(false);
			if (!isActiveAndEnabled && v != 0f)
				gameObject.SetActive(true);

			if (m_CanvasGroup != null)
				m_CanvasGroup.alpha = v;
        }
	}
}

