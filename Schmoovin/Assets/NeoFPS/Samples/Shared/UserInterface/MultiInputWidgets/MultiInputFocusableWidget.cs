using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public abstract class MultiInputFocusableWidget : MultiInputWidget, ISubmitHandler
	{
		public virtual void OnSubmit (BaseEventData eventData)
		{
			if (widgetState == WidgetState.Highlighted)
			{
				currentlyFocussed = this;
				widgetState = WidgetState.Focussed;
				PlayAudio (MenuAudio.ClickValid);
			}
		}

		public override void OnPointerUp (PointerEventData eventData)
		{
			base.OnPointerUp (eventData);
			// Remove focus (used for keyboard / gamepad input only)
			RemoveFocus ();
		}
	}
}

