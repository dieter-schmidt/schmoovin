using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventoryitemstandard.html")]
	public class HudInventoryItemStandard : HudInventoryItemBase
	{
		[SerializeField, Tooltip("An offset to highlight quick slot items when selected.")]
		private Vector2 m_SelectionOffset = new Vector2 (0f, -12f);

		protected override void OnSelect ()
		{
			RectTransform rt = transform.GetChild (0) as RectTransform;
			rt.anchoredPosition = m_SelectionOffset;
		}

		protected override void OnDeselect ()
		{
			RectTransform rt = transform.GetChild (0) as RectTransform;
			rt.anchoredPosition = Vector2.zero;
		}
	}
}

