using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudadvancedcrosshair.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudOxygenMeter : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The rect transform of the filled bar.")]
        private RectTransform m_BarRect = null;

        private DrowningMotionGraphWatcher m_DrowningWatcher = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnPlayerCharacterChanged(null);
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old drowning system
            if (m_DrowningWatcher != null)
            {
                m_DrowningWatcher.onBreathRemainingChanged -= OnBreathRemainingChanged;
                m_DrowningWatcher.onUnderwaterChanged -= OnUnderwaterChanged;
            }

            // Get new drowning system
            if (character as Component != null)
                m_DrowningWatcher = character.GetComponent<DrowningMotionGraphWatcher>();
            else
                m_DrowningWatcher = null;

            // Subscribe to new drowning system
            if (m_DrowningWatcher != null)
            {
                m_DrowningWatcher.onBreathRemainingChanged += OnBreathRemainingChanged;
                m_DrowningWatcher.onUnderwaterChanged += OnUnderwaterChanged;
                OnUnderwaterChanged(m_DrowningWatcher.underwater);
                OnBreathRemainingChanged();
            }
            else
                gameObject.SetActive(false);
        }

        private void OnUnderwaterChanged(bool underwater)
        {
            gameObject.SetActive(underwater);
        }

        private void OnBreathRemainingChanged()
        {
            m_BarRect.localScale = new Vector2(m_DrowningWatcher.breathRemaining / m_DrowningWatcher.holdBreathDuration, 1f);
        }
    }
}