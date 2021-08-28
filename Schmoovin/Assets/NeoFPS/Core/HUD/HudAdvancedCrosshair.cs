using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudadvancedcrosshair.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudAdvancedCrosshair : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The individual crosshairs.")]
        private HudAdvancedCrosshairStyleBase[] m_Crosshairs = new HudAdvancedCrosshairStyleBase[FpsCrosshair.count];

        [SerializeField, Tooltip("The default crosshair to use (when no valid crosshair driver is being wielded).")]
        private FpsCrosshair m_DefaultCrosshair = FpsCrosshair.Default;

        private FpsInventoryBase m_InventoryBase = null;
        private ICrosshairDriver m_Driver = null;
        private ICharacter m_Character = null;
        private FpsCrosshair m_CurrentCrosshair = FpsCrosshair.Default;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // Resize to match constants
            int targetCount = FpsCrosshair.count;
            if (m_Crosshairs.Length != targetCount)
            {
                // Allocate replacement array of correct size
                HudAdvancedCrosshairStyleBase[] replacement = new HudAdvancedCrosshairStyleBase[targetCount];

                // Copy data over
                int i = 0;
                for (; i < replacement.Length && i < m_Crosshairs.Length; ++i)
                    replacement[i] = m_Crosshairs[i];

                // Set new entries to null
                for (; i < replacement.Length; ++i)
                    replacement[i] = null;

                // Swap
                m_Crosshairs = replacement;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            // Add event handlers
            FpsSettings.gameplay.onCrosshairColorChanged += SetColour;
            SetColour(FpsSettings.gameplay.crosshairColor);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove event handlers
            FpsSettings.gameplay.onCrosshairColorChanged -= SetColour;

            // Unsubscribe from old driver
            if (m_Driver != null)
            {
                m_Driver.onCrosshairChanged -= SetCrosshair;
                m_Driver.onAccuracyChanged -= SetAccuracy;
                //m_Driver.onHit -= ShowHitMarker;
            }

            // Unsubscribe from old inventory
            if (m_InventoryBase != null)
            {
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;
                m_InventoryBase = null;
            }
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_InventoryBase != null)
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            if (m_Character != null)
                m_Character.onHitTarget -= OnCharacterTargetHit;

            if (character as Component != null)
            {
                m_Character = character;
                m_InventoryBase = m_Character.inventory as FpsInventoryBase;
                m_Character.onHitTarget += OnCharacterTargetHit;
            }
            else
            {
                m_InventoryBase = null;
            }

            if (m_InventoryBase == null)
            {
                SetCrosshair(m_DefaultCrosshair);
                SetAccuracy(1f);
            }
            else
            {
                m_InventoryBase.onSelectionChanged += OnSelectionChanged;
                OnSelectionChanged(m_InventoryBase.selected);
            }
        }

        protected void OnSelectionChanged(IQuickSlotItem item)
        {
            // Unsubscribe from old driver
            if (m_Driver != null)
            {
                m_Driver.onCrosshairChanged -= SetCrosshair;
                m_Driver.onAccuracyChanged -= SetAccuracy;
                //m_Driver.onHit -= ShowHitMarker;
            }

            // Get new driver
            var mb = item as MonoBehaviour;
            if (mb != null)
                m_Driver = mb.GetComponent<ICrosshairDriver>();
            else
                m_Driver = null;

            if (m_Driver != null)
            {
                // Subscribe to new driver
                m_Driver.onCrosshairChanged += SetCrosshair;
                SetCrosshair(m_Driver.crosshair);
                m_Driver.onAccuracyChanged += SetAccuracy;
                SetAccuracy(m_Driver.accuracy);
                //m_Driver.onHit += ShowHitMarker;
            }
            else
            {
                // Default behaviour
                SetCrosshair(m_DefaultCrosshair);
                SetAccuracy(1f);
            }
        }

        protected virtual void SetColour(Color colour)
        {
            for (int i = 0; i < m_Crosshairs.Length; ++i)
            {
                if (m_Crosshairs[i] != null)
                    m_Crosshairs[i].SetColour(colour);
            }
        }

        protected void SetAccuracy(float accuracy)
        {
            if (m_Crosshairs[m_CurrentCrosshair] != null)
                m_Crosshairs[m_CurrentCrosshair].SetAccuracy(accuracy);
        }

        protected void SetCrosshair(FpsCrosshair crosshair)
        {
            if (crosshair != m_CurrentCrosshair)
            {
                // Disavle the old crosshair
                if (m_Crosshairs[m_CurrentCrosshair] != null)
                    m_Crosshairs[m_CurrentCrosshair].gameObject.SetActive(false);

                // Set the new crosshair
                m_CurrentCrosshair = crosshair;

                // Enable the new crosshair
                if (m_Crosshairs[m_CurrentCrosshair] != null)
                {
                    m_Crosshairs[m_CurrentCrosshair].gameObject.SetActive(true);
                    if (m_Driver != null)
                        m_Crosshairs[m_CurrentCrosshair].SetAccuracy(m_Driver.accuracy);
                }
            }
        }

        void OnCharacterTargetHit(ICharacter c, bool critical)
        {
            if (m_Crosshairs[m_CurrentCrosshair] != null)
                m_Crosshairs[m_CurrentCrosshair].ShowHitMarker(critical);
        }
    }
}