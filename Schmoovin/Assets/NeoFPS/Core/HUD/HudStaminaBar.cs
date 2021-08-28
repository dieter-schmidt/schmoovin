using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudstaminabar.html")]
    public class HudStaminaBar : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The rect transform of the filled bar.")]
        private RectTransform m_BarRect = null;

        private StaminaSystem m_Stamina = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_Stamina != null)
                m_Stamina.onStaminaChanged -= OnStaminaChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old character
            if (m_Stamina != null)
                m_Stamina.onStaminaChanged -= OnStaminaChanged;

            // Subscribe to new parameter
            if (character as Component != null)
            {
                m_Stamina = character.GetComponent<StaminaSystem>();
                if (m_Stamina != null)
                {
                    gameObject.SetActive(true);
                    m_Stamina.onStaminaChanged += OnStaminaChanged;
                    OnStaminaChanged();
                }
                else
                    gameObject.SetActive(false);
            }
            else
                gameObject.SetActive(false);
        }

        protected virtual void OnStaminaChanged()
        {
            if (m_BarRect != null)
            {
                var localScale = m_BarRect.localScale;
                localScale.x = Mathf.Clamp01(m_Stamina.staminaNormalised);
                m_BarRect.localScale = localScale;
            }
        }
    }
}