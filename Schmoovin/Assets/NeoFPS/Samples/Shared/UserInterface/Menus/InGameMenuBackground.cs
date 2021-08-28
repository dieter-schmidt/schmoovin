using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public class InGameMenuBackground : Selectable
	{
		public override void OnPointerDown (PointerEventData eventData)
		{
			base.OnPointerDown (eventData);
			if (EventSystem.current != null)
			{
				if (MultiInputWidget.lastSelected != null)
					EventSystem.current.SetSelectedGameObject (MultiInputWidget.lastSelected.gameObject);
				else
				{
					BaseMenu menu = GetComponentInParent<BaseMenu> ();
					if (menu != null)
						menu.ResetFocus ();
				}
			}
		}
	}
}

