using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudfirearmoverheatbar.html")]
    public class HudFirearmOverheatBar : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The heat bar rect transform")]
        private RectTransform m_BarRect = null;
        [SerializeField, Tooltip("The rect transform of the cooldown marker")]
        private RectTransform m_CooldownMarkerRect = null;
        [SerializeField, Tooltip("The overheated label")]
        private GameObject m_OverheatedWarning = null;

        private FirearmOverheat m_Overheat = null;
        private FpsInventoryBase m_InventoryBase = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old inventory
            if (m_InventoryBase != null)
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            // Unsubscribe from old weapon
            if (m_Overheat != null)
                m_Overheat.onHeatValueChanged -= OnHeatValueChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_InventoryBase != null)
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            if (character as Component != null)
                m_InventoryBase = character.inventory as FpsInventoryBase;
            else
                m_InventoryBase = null;

            if (m_InventoryBase == null)
                gameObject.SetActive(false);
            else
            {
                m_InventoryBase.onSelectionChanged += OnSelectionChanged;
                OnSelectionChanged(m_InventoryBase.selected);
            }
        }

        protected void OnSelectionChanged(IQuickSlotItem item)
        {
            // Unsubscribe from old weapon
            if (m_Overheat != null)
                m_Overheat.onHeatValueChanged -= OnHeatValueChanged;

            if (item != null)
                m_Overheat = item.GetComponent<FirearmOverheat>();
            else
                m_Overheat = null;

            if (m_Overheat != null)
            {
                // Position the cooldown marker
                if (m_CooldownMarkerRect != null)
                {
                    if (m_Overheat.canOverheat && m_Overheat.coolingThreshold > 0f)
                    {
                        m_CooldownMarkerRect.anchorMin = new Vector2(m_Overheat.coolingThreshold, 0f);
                        m_CooldownMarkerRect.anchorMax = new Vector2(m_Overheat.coolingThreshold, 0f);
                        var anchoredPos = m_CooldownMarkerRect.anchoredPosition;
                        anchoredPos.x = 0f;
                        m_CooldownMarkerRect.anchoredPosition = anchoredPos;

                    }

                    m_CooldownMarkerRect.gameObject.SetActive(false);
                }

                // Attach the on change handler
                m_Overheat.onHeatValueChanged += OnHeatValueChanged;
                OnHeatValueChanged(m_Overheat.heat);

                gameObject.SetActive(true);
            }
            else
                gameObject.SetActive(false);
        }
        
        protected virtual void OnHeatValueChanged(float to)
        {
            // Scale the progress bar
            if (m_BarRect != null)
            {
                var localScale = m_BarRect.localScale;
                localScale.x = Mathf.Clamp01(to);
                m_BarRect.localScale = localScale;
            }

            // Show the warning
            if (m_Overheat != null)
            {
                if (m_OverheatedWarning != null)
                    m_OverheatedWarning.SetActive(m_Overheat.overheated);
                if (m_CooldownMarkerRect != null)
                    m_CooldownMarkerRect.gameObject.SetActive(m_Overheat.overheated);
            }
        }
    }
}