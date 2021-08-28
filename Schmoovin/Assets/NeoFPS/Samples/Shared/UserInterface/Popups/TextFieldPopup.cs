using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class TextFieldPopup : BasePopup
	{
		[SerializeField] private Text m_MessageText = null;
        [SerializeField] private InputField m_InputField = null;
        [SerializeField] private Text m_ConfirmButtonText = null;

        private static TextFieldPopup m_Instance = null;

        private UnityAction<string> m_OnYes = null;
        private bool m_AllowEmpty = false;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			m_Instance = this;
            m_Instance.m_InputField.onEndEdit.AddListener(OnEndEdit);
        }

		void OnDestroy ()
		{
			if (m_Instance == this)
				m_Instance = null;
		}

		public override void Back ()
		{
            OnCancelButton();
		}

		public void OnConfirmButton ()
		{
            m_InputField.DeactivateInputField();
            OnEndEdit(m_InputField.text);
		}

		public void OnCancelButton ()
        {
            m_InputField.DeactivateInputField();
            m_OnYes = null;
            StartCoroutine(ClosePopup());
        }

        void OnEndEdit(string text)
        {
            if (m_AllowEmpty || !string.IsNullOrEmpty(m_InputField.text))
            {
                if (m_OnYes != null)
                    m_OnYes.Invoke(text);
                m_OnYes = null;
                StartCoroutine(ClosePopup());
            }
        }

        IEnumerator ClosePopup()
        {
            yield return null;
            menu.ShowPopup(null);
        }

        public static void ShowPopup (string message, string startingText, string confirmText, UnityAction<string> onYes, bool allowEmpty = false)
		{
			if (m_Instance == null)
			{
				Debug.LogError ("No confirmation pop-up in current menu. Defaulting to negative response.");
				return;
			}

			m_Instance.m_OnYes = onYes;
            m_Instance.m_AllowEmpty = allowEmpty;
            m_Instance.m_MessageText.text = message;
            m_Instance.m_InputField.text = startingText;
            m_Instance.m_ConfirmButtonText.text = confirmText;
            m_Instance.menu.ShowPopup (m_Instance);
            m_Instance.m_InputField.ActivateInputField();
		}
	}
}