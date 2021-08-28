using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class MultiInputButton : MultiInputWidget, ISubmitHandler, IPointerClickHandler
	{
        [SerializeField] private UnityEvent m_OnClick = new UnityEvent();

		public UnityEvent onClick
		{
			get { return m_OnClick; }
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			Press ();
		}
		public void OnSubmit (BaseEventData eventData)
		{
			Press ();
		}

		private void Press ()
		{
			if (IsInteractable ())
			{
				m_OnClick.Invoke ();
				PlayAudio (MenuAudio.ClickValid);
			}
			else
				PlayAudio (MenuAudio.ClickInvalid);
		}
	}
}

