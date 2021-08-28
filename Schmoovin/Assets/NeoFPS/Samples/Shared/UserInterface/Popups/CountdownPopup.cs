using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;


namespace NeoFPS.Samples
{
	public class CountdownPopup : BasePopup
	{
		[SerializeField] private Text m_MessageText = null;

        private static CountdownPopup m_Instance = null;

        private UnityAction m_OnComplete = null;
        private float m_Duration = 0f;

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
			// Do nothing
		}

		public override void Show ()
		{
			base.Show ();
			StartCoroutine (WaitForCompletion ());
		}

		void Update ()
		{
		}

		IEnumerator WaitForCompletion ()
		{
			// Wait for minimum timeout
			if (m_Duration > 0f)
			{
				float timeout = m_Duration;
				while (timeout > 0f)
				{
					yield return null;
					timeout -= Time.unscaledDeltaTime;
				}
			}

			// Complete
			if (m_OnComplete != null)
			{
				m_OnComplete.Invoke ();
				m_OnComplete = null;
			}
			m_Instance.menu.ShowPopup (null);
		}

		public static void ShowPopup (string message, UnityAction onComplete, float duration)
		{
			if (m_Instance == null)
			{
				Debug.LogError ("No countdown pop-up in current menu. Defaulting to negative response.");
				if (onComplete != null)
					onComplete.Invoke ();
				return;
			}

			m_Instance.m_OnComplete = onComplete;
			m_Instance.m_Duration = duration;
			m_Instance.m_MessageText.text = message;
			m_Instance.menu.ShowPopup (m_Instance);
		}
	}
}