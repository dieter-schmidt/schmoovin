using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudslowmocharge.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudSlowMoCharge : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The rect transform of the filled bar.")]
        private RectTransform m_BarRect = null;

        private ISlowMoSystem m_SlowMoSystem = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnPlayerCharacterChanged(null);
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old slow-mo system
            if (m_SlowMoSystem != null)
                m_SlowMoSystem.onChargeChanged -= OnChargeChanged;

            // Get new slow-mo system
            if (character as Component != null)
                m_SlowMoSystem = character.GetComponent<ISlowMoSystem>();
            else
                m_SlowMoSystem = null;

            // Subscribe to new slow-mo system
            if (m_SlowMoSystem != null)
            {
                m_SlowMoSystem.onChargeChanged += OnChargeChanged;
                OnChargeChanged(m_SlowMoSystem.charge);
                gameObject.SetActive(true);
            }
            else
                gameObject.SetActive(false);
        }
        
        private void OnChargeChanged(float charge)
        {
            m_BarRect.localScale = new Vector2(charge, 1f);
        }
    }
}