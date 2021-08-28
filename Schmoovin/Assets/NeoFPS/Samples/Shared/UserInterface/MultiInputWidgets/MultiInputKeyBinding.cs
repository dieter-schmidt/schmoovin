using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeoFPS.Constants;

namespace NeoFPS.Samples
{
	public class MultiInputKeyBinding : MultiInputButtonGroup
    {
        private EventSystem m_EventSystem = null;
        private Text m_PrimaryText = null;
        private Text m_SecondaryText = null;

		public FpsInputButton button
		{
			get;
			private set;
		}

		private Text primaryText
		{
			get
			{
				if (m_PrimaryText == null && buttons.Length > 0 && buttons[0].rect != null)
					m_PrimaryText = buttons[0].rect.GetComponentInChildren<Text>();
				return m_PrimaryText;
			}
		}

		private Text secondaryText
		{
			get
			{
				if (m_SecondaryText == null && buttons.Length > 1 && buttons[1].rect != null)
					m_SecondaryText = buttons[1].rect.GetComponentInChildren<Text> ();
				return m_SecondaryText;
			}
		}

        private KeyCode m_PrimaryKey = KeyCode.None;
		public KeyCode primary
		{
			get { return m_PrimaryKey; }
			set
			{
				m_PrimaryKey = value;
                if (primaryText != null)
                {
                    if (value == KeyCode.None)
                        primaryText.text = string.Empty;
                    else
                        primaryText.text = value.ToString();
                }
			}
		}

        private KeyCode m_SecondaryKey = KeyCode.None;
		public KeyCode secondary
		{
			get { return m_SecondaryKey; }
			set
			{
				m_SecondaryKey = value;
                if (secondaryText != null)
                {
                    if (value == KeyCode.None)
                        secondaryText.text = string.Empty;
                    else
                        secondaryText.text = value.ToString();
                }
			}
		}

		protected override void Awake ()
		{
			base.Awake ();
			buttons [0].onClick.AddListener (SetPrimaryBinding);
			buttons [1].onClick.AddListener (SetSecondaryBinding);
		}

		public void Initialise (FpsInputButton b, string title, KeyCode b1, KeyCode b2)
		{
			label = title;
			button = b;
			primary = b1;
			secondary = b2;
		}

		public void SetPrimaryBinding ()
		{
			primaryText.text = "???";
			m_EventSystem = EventSystem.current;
			m_EventSystem.enabled = false;
            FpsSettings.keyBindings.onRebind += OnRebindInput;
            FpsSettings.keyBindings.RebindInput (button, true);
			PlayAudio (MenuAudio.ClickValid);
		}

		public void SetSecondaryBinding ()
		{
			secondaryText.text = "???";
			m_EventSystem = EventSystem.current;
			m_EventSystem.enabled = false;
            FpsSettings.keyBindings.onRebind += OnRebindInput;
			FpsSettings.keyBindings.RebindInput (button, false);
			PlayAudio (MenuAudio.ClickValid);
		}

        void OnRebindInput(FpsInputButton button, bool isPrimary, KeyCode newKey)
        {
            m_EventSystem.enabled = true;
            m_EventSystem.SetSelectedGameObject(gameObject);
            FpsSettings.keyBindings.onRebind -= OnRebindInput;
        }
    }
}

