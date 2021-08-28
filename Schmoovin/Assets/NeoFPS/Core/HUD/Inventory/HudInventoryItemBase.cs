using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    public abstract class HudInventoryItemBase : MonoBehaviour
	{
		[SerializeField, Tooltip("The UI image that is used to display the correct UI icon for the item.")]
		private Image m_IconImage = null;

		[SerializeField, Tooltip("The UI text to display the slot number.")]
		private Text m_SlotText = null;
		
		public IQuickSlotItem item
		{
			get;
			private set;
		}

		public Image iconImage
		{
			get { return m_IconImage; }
		}
		
		public Text slotText
		{
			get { return m_SlotText; }
		}

		private int m_SlotIndex = 0;
		public int slotIndex
		{
			get { return m_SlotIndex; }
			set
			{
				m_SlotIndex = value;
				int i = m_SlotIndex + 1;
				if (i >= 10)
					i -= 10;
				m_SlotText.text = i.ToString();
			}
		}

		public virtual void SetItem (IQuickSlotItem to)
		{
			item = to;
			if (to != null)
			{
				m_IconImage.sprite = to.displayImage;
				m_IconImage.gameObject.SetActive (true);
			}
			else
			{
				m_IconImage.sprite = null;
				m_IconImage.gameObject.SetActive (false);
			}
		}

		private bool m_Selected = false;
		public bool selected
		{
			get { return m_Selected; }
			set
			{
				if (!m_Selected && value)
					OnSelect ();
				if (m_Selected && !value)
					OnDeselect ();
				m_Selected = value;
			}
		}

		protected virtual void OnSelect ()
		{
		}

		protected virtual void OnDeselect ()
		{
		}
	}
}

