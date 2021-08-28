using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudshieldmeter.html")]
    [RequireComponent(typeof(RectTransform))]
    public class HudShieldMeter : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("A shield meter step (in the object hierarchy) that can be duplicated for multiple steps.")]
        private HudShieldMeterStep m_StepPrototype = null;

        private List<HudShieldMeterStep> m_Steps = null;

        protected IShieldManager shield
        {
            get;
            private set;
        }

        private void OnValidate()
        {
            if (m_StepPrototype == null)
                m_StepPrototype = GetComponentInChildren<HudShieldMeterStep>(true);
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Remove old steps objects
            if (m_Steps != null)
            {
                for (int i = 1; i < m_Steps.Count; ++i)
                    Destroy(m_Steps[i].gameObject);
                m_Steps = null;
            }

            // Unsubscribe from old shield
            if (shield != null)
            {
                shield.onShieldValueChanged -= OnShieldValueChangedInternal;
                shield.onShieldStateChanged -= OnShieldStateChanged;
                shield.onShieldConfigChanged -= OnShieldConfigChanged;
            }

            // Get new shield
            if (character != null && m_StepPrototype != null)
            {
                shield = character.GetComponent<IShieldManager>();
                if (shield != null)
                {
                    // Subscribe
                    shield.onShieldValueChanged += OnShieldValueChangedInternal;
                    shield.onShieldStateChanged += OnShieldStateChanged;
                    shield.onShieldConfigChanged += OnShieldConfigChanged;

                    // Set up steps
                    var parent = m_StepPrototype.transform.parent;
                    m_Steps = new List<HudShieldMeterStep>(shield.shieldStepCount);
                    m_Steps.Add(m_StepPrototype);
                    for (int i = 1; i < shield.shieldStepCount; ++i)
                        m_Steps.Add(Instantiate(m_StepPrototype, parent));
                    for (int i = 0; i < m_Steps.Count; ++i)
                        m_Steps[i].ResetLayout(i, shield.shieldStepCount);

                    // Show HUD
                    gameObject.SetActive(true);

                    // Update
                    UpdateStepMeters();
                }
                else
                    gameObject.SetActive(false);
            }
            else
                gameObject.SetActive(false);
        }

        protected virtual void OnShieldValueChangedInternal(IShieldManager s, float from, float to)
        {
            UpdateStepMeters();
        }

        protected virtual void OnShieldStateChanged(IShieldManager s, ShieldState state)
        {
        }

        protected virtual void OnShieldConfigChanged(IShieldManager s)
        {
            // Resize if required
            if (s.shieldStepCount != m_Steps.Count)
                ResizeSteps(s.shieldStepCount);

            // Update step meters
            UpdateStepMeters();
        }

        void UpdateStepMeters()
        {
            // Get the number of filled shield steps & remainder
            int full = Mathf.FloorToInt((shield.shield + 0.01f) / shield.shieldStepCapacity);
            float remainder = shield.shield - (full * shield.shieldStepCapacity);

            // Full steps
            int i = 0;
            for (; i < full; ++i)
                m_Steps[i].fill = 1f;

            // Partial step
            if (remainder > 0.001f)
            {
                m_Steps[i].fill = remainder / shield.shieldStepCapacity;
                ++i;
            }

            // Empty steps
            for (; i < m_Steps.Count; ++i)
                m_Steps[i].fill = 0f;
        }

        void ResizeSteps(int to)
        {
            if (to > m_Steps.Count)
            {
                // Get the correct transform parent
                var parent = m_StepPrototype.transform.parent;
                // Resize the list
                m_Steps.Capacity = to;
                // Add new items
                while (m_Steps.Count < to)
                    m_Steps.Add(Instantiate(m_StepPrototype, parent));
            }
            else
            {
                // Trim steps
                for (int i = m_Steps.Count - 1; i >= to; --i)
                    Destroy(m_Steps[i].gameObject);
                m_Steps.RemoveRange(to, m_Steps.Count - to);
            }

            // Reset step layout
            for (int i = 0; i < m_Steps.Count; ++i)
                m_Steps[i].ResetLayout(i, to);
        }
    }
}