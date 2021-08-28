using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudscope.html")]
	[RequireComponent (typeof (CanvasGroup))]
	public class HudScope : MonoBehaviour
	{
        [SerializeField, Tooltip("The canvas group of the HUD. This will be hidden while the scope is visible")]
        private CanvasGroup m_HudGroup = null;

        [SerializeField, Tooltip("The scope name. Used to allow different weapons to use different scopes.")]
		private string m_Key = string.Empty;

		private static Dictionary<int, HudScope> m_Available = new Dictionary<int, HudScope> ();
		private static HudScope m_CurrentlyActive = null;
        private static float m_HudAlpha = 1f;
        private CanvasGroup m_CanvasGroup = null;

        void Awake ()
		{
			m_CanvasGroup = GetComponent<CanvasGroup> ();
			m_CanvasGroup.alpha = 1f;
			m_Available.Add (Animator.StringToHash (m_Key), this);
			gameObject.SetActive (false);
		}

		void OnDestroy ()
		{
			int key = Animator.StringToHash (m_Key);
			if (m_Available.ContainsKey (key))
				m_Available.Remove (key);
		}

		public static bool Show (string key)
		{
			return Show (Animator.StringToHash (key));
		}

		public static bool Show (int key)
		{
			if (m_CurrentlyActive != null)
				return false;

			HudScope scope;
			if (m_Available.TryGetValue (key, out scope))
			{
				scope.gameObject.SetActive (true);
                
                // Hide the hud
                if (scope.m_HudGroup != null)
                {
                    m_HudAlpha = scope.m_HudGroup.alpha;
                    scope.m_HudGroup.alpha = 0f;
                }

                m_CurrentlyActive = scope;
				return true;
			}

			return false;
		}

		public static void Hide ()
		{
			if (m_CurrentlyActive != null)
			{
				m_CurrentlyActive.gameObject.SetActive (false);

                // Show the hud
                if (m_CurrentlyActive.m_HudGroup != null)
                    m_CurrentlyActive.m_HudGroup.alpha = m_HudAlpha;

                m_CurrentlyActive = null;
			}
		}
	}
}