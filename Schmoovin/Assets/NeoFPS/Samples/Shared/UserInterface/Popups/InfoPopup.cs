using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class InfoPopup : BasePopup
	{
		[SerializeField] private Text m_MessageText = null;

        private static InfoPopup m_Instance = null;

        private UnityAction m_OnOK = null;

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
			OnOK ();
		}

		public void OnOK ()
		{
			if (m_OnOK != null)
			{
				m_OnOK.Invoke ();
				m_OnOK = null;
			}
			m_Instance.menu.ShowPopup (null);
		}

		public static void ShowPopup (string message, UnityAction onOK)
		{
			if (m_Instance == null)
			{
				Debug.LogError ("No info pop-up in current menu. Defaulting to negative response.");
				if (onOK != null)
					onOK.Invoke ();
				return;
			}

			m_Instance.m_OnOK = onOK;
			m_Instance.m_MessageText.text = message;
			m_Instance.menu.ShowPopup (m_Instance);
		}
	}
}