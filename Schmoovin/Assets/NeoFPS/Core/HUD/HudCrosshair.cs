using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudcrosshair.html")]
    [RequireComponent(typeof(RectTransform))]
	public class HudCrosshair : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The parent rect transform of the crosshairs (will be expanded and contracted based on accuracy).")]
        private RectTransform m_CrosshairRect = null;

        [SerializeField, Tooltip("The individual crosshairs.")]
        private CanvasGroup[] m_Crosshairs = new CanvasGroup[FpsCrosshair.count];

        [SerializeField, Tooltip("The size the UI element will reach at 100% accuracy.")]
		private float m_MinimumSize = 8f;

		[SerializeField, Tooltip("The size the UI element will reach at 0% accuracy.")]
		private float m_MaximumSize = 200f;

        [SerializeField, Tooltip("The default crosshair to use (when no valid crosshair driver is being wielded).")]
        private FpsCrosshair m_DefaultCrosshair = FpsCrosshair.Default;

        private FpsInventoryBase m_InventoryBase = null;
        private ICrosshairDriver m_Driver = null;
        private FpsCrosshair m_CurrentCrosshair = FpsCrosshair.Default;

        // Future development:
        // - Add a critical hit marker

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            m_MinimumSize = Mathf.Clamp(m_MinimumSize, 4f, 100f);
            m_MaximumSize = Mathf.Clamp(m_MaximumSize, 8f, 400f);

            // Get crosshair rect
            if (m_CrosshairRect == null)
                m_CrosshairRect = (RectTransform)transform;

            // Resize to match constants
            int targetCount = FpsCrosshair.count;
            if (m_Crosshairs.Length != targetCount)
            {
                // Allocate replacement array of correct size
                CanvasGroup[] replacement = new CanvasGroup[targetCount];

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

        protected override void Awake ()
        {
            base.Awake();

            // Add event handlers
            FpsSettings.gameplay.onCrosshairColorChanged += SetColour;
			SetColour (FpsSettings.gameplay.crosshairColor);
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

            if (character as Component != null)
                m_InventoryBase = character.inventory as FpsInventoryBase;
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
            }
            else
            {
                // Default behaviour
                SetCrosshair(m_DefaultCrosshair);
                SetAccuracy(1f);
            }
        }

        protected virtual void SetColour (Color colour)
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>();
            for (int i = 0; i < graphics.Length; ++i)
				graphics [i].color = colour;
		}

        protected void SetAccuracy (float accuracy)
		{
			float size = Mathf.Lerp (m_MaximumSize, m_MinimumSize, Mathf.Clamp01(accuracy));
            m_CrosshairRect.sizeDelta = new Vector2 (size, size);
		}

        protected void SetCrosshair (FpsCrosshair crosshair)
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
                    m_Crosshairs[m_CurrentCrosshair].gameObject.SetActive(true);
            }
        }
	}
}