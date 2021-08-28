using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    public abstract class MultiInputWidget : Selectable
	{
		[SerializeField] private UiStyle m_Style = null;
        [SerializeField] private Image[] m_Backgrounds = new Image[0];
		[SerializeField] private Text m_LabelText = null;
        [SerializeField] private Text m_DescriptionText = null;
        [SerializeField] private Text[] m_ControlsText = new Text[0];
        [SerializeField] private string m_Label = "Label";
        [SerializeField] private string m_Description = string.Empty;

		private MenuAudioPlayer m_MenuAudioPlayer = null;

        private static MultiInputWidget m_CurrentlyFocussed = null;
        public static MultiInputWidget currentlyFocussed
		{
			get { return m_CurrentlyFocussed; }
			protected set
			{
				if (m_CurrentlyFocussed != null)
					m_CurrentlyFocussed.OnLoseFocus();
				m_CurrentlyFocussed = value;
				if (m_CurrentlyFocussed != null)
					m_CurrentlyFocussed.OnTakeFocus();
			}
		}

		public static MultiInputWidget lastSelected
		{
			get;
			private set;
		}

		public MultiInputWidgetList parentList
		{
			get;
			private set;
		}
		public MultiInputWidgetLayout parentLayout
		{
			get;
			private set;
		}

		public static bool usingMoveEvents
		{
			get;
			private set;
		}

		public enum MenuAudio
		{
			Move,
			ClickValid,
			ClickInvalid
		}

		public enum WidgetState
		{
			Normal,
			Highlighted,
			Focussed,
			Pressed,
			Disabled
		}

		private WidgetState m_WidgetState = WidgetState.Normal;
        public WidgetState widgetState
		{
			get { return m_WidgetState; }
			protected set
			{
                if (m_Style != null)
                {
                    m_WidgetState = value;
                    switch (m_WidgetState)
                    {
                        case WidgetState.Normal:
                            for (int i = 0; i < m_Backgrounds.Length; ++i)
                            {
                                if (m_Backgrounds[i] != null)
                                    m_Backgrounds[i].color = m_Style.colours.normal;
                            }
                            break;
                        case WidgetState.Highlighted:
                            for (int i = 0; i < m_Backgrounds.Length; ++i)
                            {
                                if (m_Backgrounds[i] != null)
                                    m_Backgrounds[i].color = m_Style.colours.highlighted;
                            }
                            break;
                        case WidgetState.Focussed:
                            for (int i = 0; i < m_Backgrounds.Length; ++i)
                            {
                                if (m_Backgrounds[i] != null)
                                    m_Backgrounds[i].color = m_Style.colours.focussed;
                            }
                            break;
                        case WidgetState.Disabled:
                            for (int i = 0; i < m_Backgrounds.Length; ++i)
                            {
                                if (m_Backgrounds[i] != null)
                                    m_Backgrounds[i].color = m_Style.colours.disabled;
                            }
                            break;
                    }
                }
			}
        }

        protected override void Awake ()
		{
			base.Awake ();
			m_MenuAudioPlayer = GetComponentInParent<MenuAudioPlayer> ();
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

		public string label
		{
			get { return m_Label; }
			set
			{
				m_Label = value;
				if (m_LabelText != null)
					m_LabelText.text = value;
			}
		}

		public string description
		{
			get { return m_Description; }
			set
			{
				m_Description = value;
				if (m_DescriptionText != null)
				{
					m_DescriptionText.text = value;
					m_DescriptionText.gameObject.SetActive (value != string.Empty);
					if (!customHeight && m_Style != null)
						CheckHeight ();
				}
			}
		}

		private Image m_FocussedSelection = null;
        protected Image focussedSelection
		{
			get { return m_FocussedSelection; }
			set { m_FocussedSelection = value; }
		}

		protected virtual bool customHeight
		{
			get { return false; }
		}


		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			transition = Transition.None;
			if (navigation.mode != Navigation.Mode.Explicit)
				navigation = new Navigation { mode = Navigation.Mode.Explicit };
			if (m_LabelText != null)
				m_LabelText.text = m_Label;
			if (m_DescriptionText != null)
			{
				m_DescriptionText.text = m_Description;
				m_DescriptionText.gameObject.SetActive (m_Description != string.Empty);
			}
			//SetUiStyle ();
		}
		#endif

		protected override void OnEnable ()
		{
			base.OnEnable ();
			parentList = GetComponentInParent<MultiInputWidgetList> ();
			parentLayout = GetComponentInParent<MultiInputWidgetLayout> ();
			SetUiStyle ();
			widgetState = IsInteractable () ? WidgetState.Normal : WidgetState.Disabled;
		}

		public override void OnSelect (BaseEventData eventData)
		{
			base.OnSelect (eventData);
			if (parentLayout != null)
				parentLayout.OnWidgetSelected (this);
			widgetState = WidgetState.Highlighted;
			lastSelected = this;
		}

		public override void OnDeselect (BaseEventData eventData)
		{
			base.OnDeselect (eventData);
			if (IsInteractable ())
				widgetState = WidgetState.Normal;
			else
				widgetState = WidgetState.Disabled;
		}

		protected override void OnDisable ()
		{
			base.OnDisable ();
			if (lastSelected == this)
				lastSelected = null;
		}
		
		protected virtual void OnTakeFocus ()
        {
            NeoFpsInputManager.PushEscapeHandler(RemoveFocus);
		}

		protected virtual void OnLoseFocus ()
        {
            NeoFpsInputManager.PopEscapeHandler(RemoveFocus);
		}
		
		public void RemoveFocus ()
		{
			if (currentlyFocussed == this)
				currentlyFocussed = null;
			if (widgetState == WidgetState.Focussed)
			{
				if (EventSystem.current.currentSelectedGameObject == gameObject)
					widgetState = WidgetState.Highlighted;
				else
					widgetState = WidgetState.Normal;
			}
        }
        
        public void RefreshInteractable()
        {
            if (IsInteractable())
                widgetState = WidgetState.Normal;
            else
            {
                widgetState = WidgetState.Disabled;

                if (parentList != null)
                    parentList.SelectFirst();

                //if (currentlyFocussed == this)
                //{
                //    currentlyFocussed = null;
                //    SelectDirection(MoveDirection.Up);
                //}
                //else
                //{
                //    if (widgetState == WidgetState.Highlighted)
                //        SelectDirection(MoveDirection.Up);
                //}
            }
        }

        public override void OnMove (AxisEventData eventData)
		{
			usingMoveEvents = true;
			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
					if (widgetState == WidgetState.Focussed)
						FocusLeft ();
					else
						SelectDirection (MoveDirection.Left);
					break;
				case MoveDirection.Right:
					if (widgetState == WidgetState.Focussed)
						FocusRight ();
					else
						SelectDirection (MoveDirection.Right);
					break;
				default:
					if (widgetState != WidgetState.Focussed)
					{
						PlayAudio (MenuAudio.Move);
						base.OnMove (eventData);
					}
					break;
			}
		}

		public override void OnPointerEnter (PointerEventData eventData)
		{
			base.OnPointerEnter (eventData);
			if (IsActive () && IsInteractable () && EventSystem.current != null)
				EventSystem.current.SetSelectedGameObject (gameObject);
		}

		public virtual void SelectDirection (MoveDirection dir)
		{
			if (dir == MoveDirection.Left || dir == MoveDirection.Right)
			{
				if (parentList != null)
				{
					parentList.SelectDirection (dir);
					PlayAudio (MenuAudio.Move);
				}
			}
			else
			{
				if (dir == MoveDirection.Up)
				{
					if (navigation.selectOnUp != null)
						EventSystem.current.SetSelectedGameObject (navigation.selectOnUp.gameObject);
				}
				else
				{
					if (navigation.selectOnDown != null)
						EventSystem.current.SetSelectedGameObject (navigation.selectOnDown.gameObject);
				}
			}
		}

		public virtual void FocusLeft ()
		{
		}

		public virtual void FocusRight ()
		{
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

		void SetUiStyle ()
		{
			if (m_Style == null)
				return;
			
			// Set up backgrounds
			for (int i = 0; i < m_Backgrounds.Length; ++i)
			{
				if (m_Backgrounds [i] != null)
				{ 
					m_Backgrounds [i].sprite = m_Style.background;
					m_Backgrounds [i].color = m_Style.colours.normal;
				}
			}

			// Set up label text
			if (m_LabelText != null)
			{
				m_LabelText.font = m_Style.textInfo.font;
				m_LabelText.fontSize = m_Style.textInfo.headerFontSize;
				m_LabelText.color = m_Style.textInfo.headerColour;
			}

			// Set up desecription text
			if (m_DescriptionText != null)
			{
				m_DescriptionText.font = m_Style.textInfo.font;
				m_DescriptionText.fontSize = m_Style.textInfo.descriptionFontSize;
				m_DescriptionText.color = m_Style.textInfo.descriptionColour;
			}

			// Set up controls text
			for (int i = 0; i < m_ControlsText.Length; ++i)
			{
				if (m_ControlsText [i] != null)
				{ 
					m_ControlsText [i].font = m_Style.textInfo.font;
					m_ControlsText [i].fontSize = m_Style.textInfo.controlFontSize;
					m_ControlsText [i].color = m_Style.textInfo.controlColour;
				}
			}

			if (!customHeight)
				CheckHeight ();
		}

		void CheckHeight ()
		{
			RectTransform rt = transform as RectTransform;
			Vector2 sizeDelta = rt.sizeDelta;
			if (m_DescriptionText != null && m_DescriptionText.gameObject.activeSelf)
				sizeDelta.y = m_Style.heightLarge;
			else
				sizeDelta.y = m_Style.heightSmall;
			rt.sizeDelta = sizeDelta;
		}


		private Coroutine m_PulseColourCoroutine = null;
        private float m_PulseTimer = 0f;
		private Image m_PulseImage = null;
        private Color m_PulseCompleteColour = Color.white;

        protected void PulseColour (Image i, Color pulseColour, Color completeColor)
		{
			if (m_PulseImage != null)
				m_PulseImage.color = m_PulseCompleteColour;

			m_PulseImage = i;
			if (m_PulseImage != null)
			{
				m_PulseImage.color = pulseColour;
				m_PulseCompleteColour = completeColor;
				m_PulseTimer = style.inputPulseDuration;

				if (m_PulseColourCoroutine == null)
					m_PulseColourCoroutine = StartCoroutine (PulseColourCoroutine ());
			}
		}

		IEnumerator PulseColourCoroutine ()
		{
			while (m_PulseTimer > 0f)
			{
				yield return null;
				m_PulseTimer -= Time.unscaledDeltaTime;
			}

			m_PulseImage.color = m_PulseCompleteColour;
			m_PulseTimer = 0f;
			m_PulseImage = null;

			m_PulseColourCoroutine = null;
		}
	}
}