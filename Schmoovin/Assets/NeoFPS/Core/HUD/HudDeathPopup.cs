using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.SinglePlayer;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-huddeathpopup.html")]
    [RequireComponent (typeof (CanvasGroup))]
	public class HudDeathPopup : PlayerCharacterHudBase
    {
        private CanvasGroup m_CanvasGroup = null;
		private ICharacter m_Character = null;

        protected override void Awake()
        {
            base.Awake();
            m_CanvasGroup = GetComponent<CanvasGroup>();
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_Character != null)
                m_Character.onIsAliveChanged -= OnIsAliveChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
			if (m_Character != null)
				m_Character.onIsAliveChanged -= OnIsAliveChanged;

			m_Character = character;

			if (m_Character as Component != null)
			{
				m_Character.onIsAliveChanged += OnIsAliveChanged;
				OnIsAliveChanged (m_Character, m_Character.isAlive);
			}
			else
				gameObject.SetActive (false);
		}

		void OnIsAliveChanged (ICharacter character, bool alive)
		{
            if (alive)
                m_CanvasGroup.alpha = 0f;
            else
                m_CanvasGroup.alpha = 1f;
			gameObject.SetActive (!alive);
		}
	}
}