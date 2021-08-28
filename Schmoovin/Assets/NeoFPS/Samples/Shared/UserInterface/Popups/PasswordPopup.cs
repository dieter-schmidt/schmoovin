using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class PasswordPopup : BasePopup
	{
		[SerializeField] private Text m_MessageText = null;
        [SerializeField] private bool m_DefaultResult = false;

		private static PasswordPopup m_Instance = null;

        private UnityAction m_OnYes = null;
        private UnityAction m_OnNo = null;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			m_Instance = this;
		}

		void OnDestroy ()
		{
			if (m_Instance == this)
				m_Instance = null;
		}

		public override void Back ()
		{
			if (m_DefaultResult)
				OnConfirmationYes ();
			else
				OnConfirmationNo ();
		}

		public void OnConfirmationYes ()
		{
			if (m_OnYes != null)
				m_OnYes.Invoke ();
			m_OnYes = null;
			m_OnNo = null;
			m_Instance.menu.ShowPopup (null);
		}

		public void OnConfirmationNo ()
		{
			if (m_OnNo != null)
				m_OnNo.Invoke ();
			m_OnYes = null;
			m_OnNo = null;
			m_Instance.menu.ShowPopup (null);
		}

		public static void ShowPopup (string message, UnityAction onYes, UnityAction onNo)
		{
			if (m_Instance == null)
			{
				Debug.LogError ("No confirmation pop-up in current menu. Defaulting to negative response.");
				if (onNo != null)
					onNo.Invoke ();
				return;
			}

			m_Instance.m_OnYes = onYes;
			m_Instance.m_OnNo = onNo;
			m_Instance.m_MessageText.text = message;
			m_Instance.menu.ShowPopup (m_Instance);
		}
	}
}