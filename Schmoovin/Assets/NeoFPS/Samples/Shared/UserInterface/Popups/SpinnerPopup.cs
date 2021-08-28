using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;


namespace NeoFPS.Samples
{
	public class SpinnerPopup : BasePopup
	{
		[SerializeField] private Text m_MessageText = null;
        [SerializeField] private Transform m_SpinnerTransform = null;
        [SerializeField] private float m_SpinRate = 0f;

		private static SpinnerPopup m_Instance = null;

        private UnityAction m_OnComplete = null;
        private Func<bool> m_CheckComplete = null;
        private float m_MinTime = 0f;

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
			m_SpinnerTransform.localRotation = Quaternion.identity;
			StartCoroutine (WaitForCompletion ());
		}

		void Update ()
		{
			m_SpinnerTransform.Rotate (0f, 0f, m_SpinRate * Time.unscaledDeltaTime);
		}

		IEnumerator WaitForCompletion ()
		{
			// Wait for minimum timeout
			if (m_MinTime > 0f)
			{
				float timeout = m_MinTime;
				while (timeout > 0f)
				{
					yield return null;
					timeout -= Time.unscaledDeltaTime;
				}
			}

			// Check for completion
			if (m_CheckComplete != null)
			{
				while (m_CheckComplete () != true)
				{
					yield return null;
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

		public static void ShowPopup (string message, Func<bool> checkComplete, UnityAction onComplete, float minTime = 0f)
		{
			if (m_Instance == null)
			{
				Debug.LogError ("No spinner pop-up in current menu. Defaulting to negative response.");
				if (onComplete != null)
					onComplete.Invoke ();
				return;
			}

			m_Instance.m_OnComplete = onComplete;
			m_Instance.m_CheckComplete = checkComplete;
			m_Instance.m_MinTime = minTime;
			m_Instance.m_MessageText.text = message;
			m_Instance.menu.ShowPopup (m_Instance);
		}
	}
}