using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    [RequireComponent (typeof (Button))]
	public class UiStyledButton : Button, ICancelHandler
	{
		[SerializeField] private UiStyle m_Style = null;
        [SerializeField] private UnityEvent m_OnCancel = new UnityEvent();

		private MenuAudioPlayer m_MenuAudioPlayer = null;

        public enum MenuAudio
		{
			Move,
			ClickValid,
			ClickInvalid
		}

		public UiStyle style
		{
			get { return m_Style; }
			set
			{
				m_Style = value;
				SetUiStyle ();
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			SetUiStyle ();
		}
		#endif

		protected override void Awake ()
		{
			base.Awake ();
			SetUiStyle ();
			m_MenuAudioPlayer = GetComponentInParent<MenuAudioPlayer> ();
		}

		public void PlayAudio (MenuAudio audio)
		{
			if (m_MenuAudioPlayer != null && m_Style != null)
			{
				switch (audio)
				{
					case MenuAudio.Move:
						if (m_Style.soundEffects.move != null)
							m_MenuAudioPlayer.PlayClip (m_Style.soundEffects.move);
						break;
					case MenuAudio.ClickValid:
						if (m_Style.soundEffects.press != null)
							m_MenuAudioPlayer.PlayClip (m_Style.soundEffects.press);
						break;
					case MenuAudio.ClickInvalid:
						if (m_Style.soundEffects.error != null)
							m_MenuAudioPlayer.PlayClip (m_Style.soundEffects.error);
						break;
				}
			}
		}

		public override void OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnPointerClick (eventData);
			PlayAudio (MenuAudio.ClickValid);
		}

		public override void OnSubmit (UnityEngine.EventSystems.BaseEventData eventData)
		{
			base.OnSubmit (eventData);
			PlayAudio (MenuAudio.ClickValid);
		}

		public void OnCancel (BaseEventData eventData)
		{
			m_OnCancel.Invoke ();
		}

		void Navigate(AxisEventData eventData, Selectable sel)
        {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;
        }
		
		public override void OnMove (AxisEventData eventData)
		{
			Selectable target = null;
            switch (eventData.moveDir)
            {
				case MoveDirection.Right:
					target = FindSelectableOnRight ();
                    break;
                case MoveDirection.Up:
					target = FindSelectableOnUp ();
                    break;
				case MoveDirection.Left:
					target = FindSelectableOnLeft ();
                    break;
				case MoveDirection.Down:
					target = FindSelectableOnDown ();
                    break;
            }
			if (target != null)
			{
				Navigate (eventData, target);
				PlayAudio (MenuAudio.Move);
			}
		}

		void SetUiStyle ()
		{
			if (m_Style == null)
				return;

			// Set colours
			transition = Transition.ColorTint;
			ColorBlock cb = colors;
			cb.normalColor = m_Style.colours.normal;
			cb.highlightedColor = m_Style.colours.highlighted;
			cb.pressedColor = m_Style.colours.pressed;
			cb.disabledColor = m_Style.colours.disabled;
			colors = cb;

            if (targetGraphic != null)
                targetGraphic.color = Color.white;
		}
	}
}