using UnityEngine;
using System.Collections.Generic;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ModularFirearm))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmaimfatigue.html")]
    public class FirearmAimFatigue : MonoBehaviour
    {
        [SerializeField, Tooltip("The stamina loss per second when aiming down sights.")]
        private float m_StaminaLoss = 10f;
        [SerializeField, Tooltip("The stamina level to drain down to.")]
        private float m_StaminaTarget = 25f;
        [SerializeField, Tooltip("Stamina drain fades when approaching the target, starting at this falloff value above it.")]
        private float m_StaminaFalloff = 25f;

        private IStaminaSystem m_StaminaSystem = null;
        private IModularFirearm m_ModularFirearm = null;
        private IAimer m_Aimer = null;
        private float m_InverseDrainFalloff = 1f;
        private bool m_Aiming = false;

        private void Awake()
        {
            m_ModularFirearm = GetComponent<IModularFirearm>();

            // Calculate drain falloff
            if (m_StaminaFalloff > 1f)
                m_InverseDrainFalloff = 1f / m_StaminaFalloff;
        }

        void OnAimerChange(IModularFirearm firearm, IAimer aimer)
        {
            if (m_Aimer != null)
                m_Aimer.onAimStateChanged -= OnAimStateChanged;

            m_Aimer = aimer;

            if (m_Aimer != null)
                m_Aimer.onAimStateChanged += OnAimStateChanged;
        }

        void OnAimStateChanged(IModularFirearm firearm, FirearmAimState s)
        {
            if (m_Aiming)
            {
                if (s == FirearmAimState.HipFire || s == FirearmAimState.ExitingAim)
                {
                    m_Aiming = false;
                    m_StaminaSystem.RemoveStaminaDrain(GetStaminaDrain);
                }
            }
            else
            {
                if (s == FirearmAimState.Aiming || s == FirearmAimState.EnteringAim)
                {
                    m_Aiming = true;
                    m_StaminaSystem.AddStaminaDrain(GetStaminaDrain);
                }
            }
        }

        private void OnValidate()
        {
            m_StaminaLoss = Mathf.Clamp(m_StaminaLoss, 0f, 999f);
            m_StaminaTarget = Mathf.Clamp(m_StaminaTarget, 0f, 100f);
            m_StaminaFalloff = Mathf.Clamp(m_StaminaFalloff, 0f, 100f);
        }

        private void OnEnable()
        {
            if (m_ModularFirearm.wielder != null)
                m_StaminaSystem = m_ModularFirearm.wielder.GetComponent<IStaminaSystem>();
            else
                m_StaminaSystem = null;

            if (m_StaminaSystem != null)
            {
                // Attach to aimer
                m_ModularFirearm.onAimerChange += OnAimerChange;
                OnAimerChange(m_ModularFirearm, m_ModularFirearm.aimer);
            }
        }

        private float GetStaminaDrain (IStaminaSystem s, float modifiedStamina)
        {
            float multiplier = Mathf.Clamp01((modifiedStamina - m_StaminaTarget) * m_InverseDrainFalloff);
            return m_StaminaLoss * Time.deltaTime * multiplier;
        }

        private void OnDisable()
        {
            if (m_Aimer != null)
            {
                m_Aimer.onAimStateChanged -= OnAimStateChanged;
                m_Aimer = null;
            }

            if (m_StaminaSystem != null)
            {
                m_StaminaSystem.RemoveStaminaDrain(GetStaminaDrain);
                m_StaminaSystem = null;
            }
        }
    }
}
