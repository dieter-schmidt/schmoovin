using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-slowmosystem.html")]
    public class SlowMoSystem : MonoBehaviour, ISlowMoSystem, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The (real, unscaled) time taken to transition time scales.")]
        private float m_TransitionDuration = 0.25f;
        [SerializeField, Tooltip("The amount of charge added per (real, unscaled) second.")]
        private float m_ChargeRate = 0.2f;
        [SerializeField, Tooltip("The sound to play on entering slow-mo.")]
        private AudioClip m_EnterSlowMoAudio = null;
        [SerializeField, Tooltip("The sound to play on exiting slow-mo.")]
        private AudioClip m_ExitSlowMoAudio = null;

        private static readonly NeoSerializationKey k_ChargeKey = new NeoSerializationKey("charge");
        private static readonly NeoSerializationKey k_LerpKey = new NeoSerializationKey("lerp");
        private static readonly NeoSerializationKey k_SourceTimeScaleKey = new NeoSerializationKey("sourceTimeScale");
        private static readonly NeoSerializationKey k_TargetTimeScaleKey = new NeoSerializationKey("targetTimeScale");
        private static readonly NeoSerializationKey k_SourceChargeRateKey = new NeoSerializationKey("sourceChargeRate");
        private static readonly NeoSerializationKey k_TargetChargeRateKey = new NeoSerializationKey("targetChargeRate");
        private static readonly NeoSerializationKey k_TimeScaleKey = new NeoSerializationKey("timeScale");
        private static readonly NeoSerializationKey k_ChargeRateKey = new NeoSerializationKey("chargeRate");

        public event UnityAction<float> onChargeChanged;

        private Coroutine m_LerpTimescaleCoroutine = null;
        private ICharacterAudioHandler m_CharacterAudioHandler = null;
        private float m_SourceTimeScale = 1f;
        private float m_TargetTimeScale = 1f;
        private float m_SourceChargeRate = 0f;
        private float m_TargetChargeRate = 0f;
        private float m_Lerp = 1f;
        private bool m_TimeIsScaled = false;
        private float m_Charge = 1f;
        private float m_CurrentChargeRate = 0f;

        public float charge
        {
            get { return m_Charge; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_Charge != value)
                {
                    m_Charge = value;
                    if (onChargeChanged != null)
                        onChargeChanged(m_Charge);
                }
            }
        }
        
        public bool isTimeScaled
        {
            get { return m_TimeIsScaled; }
        }

        void OnValidate()
        {
            m_TransitionDuration = Mathf.Clamp(m_TransitionDuration, 0f, 5f);
        }

        void Awake()
        {
            m_CharacterAudioHandler = GetComponent<ICharacterAudioHandler>();
        }
        
        public void SetTimeScale(float ts, float drainRate = 0f)
        {
            ts = Mathf.Clamp(ts, 0.01f, 5f);
            if (m_Charge > 0f)
            {
                m_TimeIsScaled = true;
                if (m_TransitionDuration < 0.01f)
                {
                    NeoFpsTimeScale.timeScale = ts;
                    m_CurrentChargeRate = -drainRate;
                }
                else
                {
                    m_SourceTimeScale = NeoFpsTimeScale.timeScale;
                    m_TargetTimeScale = ts;
                    m_SourceChargeRate = m_CurrentChargeRate;
                    m_TargetChargeRate = -drainRate;
                    m_Lerp = 0f;
                }

                if (m_EnterSlowMoAudio != null && m_CharacterAudioHandler != null)
                    m_CharacterAudioHandler.PlayClip(m_EnterSlowMoAudio);
            }
        }

        public void ResetTimescale()
        {
            m_TimeIsScaled = false;
            if (m_TransitionDuration < 0.01f)
            {
                NeoFpsTimeScale.timeScale = 1f;
                m_CurrentChargeRate = m_ChargeRate;
            }
            else
            {
                m_SourceTimeScale = NeoFpsTimeScale.timeScale;
                m_TargetTimeScale = 1f;
                m_SourceChargeRate = m_CurrentChargeRate;
                m_TargetChargeRate = m_ChargeRate;
                m_Lerp = 0f;
            }

            if (m_ExitSlowMoAudio != null && m_CharacterAudioHandler != null)
                m_CharacterAudioHandler.PlayClip(m_ExitSlowMoAudio);
        }

        void Update()
        {
            if (Time.timeScale != 0f)
            {
                // Transition if required
                if (m_Lerp < 1f)
                {
                    m_Lerp += Time.deltaTime / m_TransitionDuration;
                    if (m_Lerp > 1f)
                        m_Lerp = 1f;

                    m_CurrentChargeRate = Mathf.Lerp(m_SourceChargeRate, m_TargetChargeRate, m_Lerp);
                    NeoFpsTimeScale.timeScale = Mathf.Lerp(m_SourceTimeScale, m_TargetTimeScale, m_Lerp);
                }

                // Modify charge
                charge += Time.unscaledDeltaTime * m_CurrentChargeRate;
                if (charge == 0f && m_TimeIsScaled)
                    ResetTimescale();
            }
        }
        
        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ChargeKey, m_Charge);

            // Check if in transition
            if (m_LerpTimescaleCoroutine != null)
            {
                writer.WriteValue(k_LerpKey, m_Lerp);
                writer.WriteValue(k_SourceTimeScaleKey, m_SourceTimeScale);
                writer.WriteValue(k_TargetTimeScaleKey, m_TargetTimeScale);
            }
            else
            {
                if (m_TimeIsScaled)
                    writer.WriteValue(k_TimeScaleKey, Time.timeScale);
                writer.WriteValue(k_ChargeRateKey, m_CurrentChargeRate);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_ChargeKey, out m_Charge, m_Charge);
            reader.TryReadValue(k_ChargeKey, out m_Charge, m_Charge);

            // Check if in transition
            if (reader.TryReadValue(k_LerpKey, out m_Lerp, m_Lerp))
            {
                reader.TryReadValue(k_SourceTimeScaleKey, out m_SourceTimeScale, m_SourceTimeScale);
                reader.TryReadValue(k_TargetTimeScaleKey, out m_TargetTimeScale, m_TargetTimeScale);
                reader.TryReadValue(k_SourceChargeRateKey, out m_SourceChargeRate, m_SourceChargeRate);
                reader.TryReadValue(k_TargetChargeRateKey, out m_TargetChargeRate, m_TargetChargeRate);

                m_CurrentChargeRate = Mathf.Lerp(m_SourceChargeRate, m_TargetChargeRate, m_Lerp);
                NeoFpsTimeScale.timeScale = Mathf.Lerp(m_SourceTimeScale, m_TargetTimeScale, m_Lerp);
            }
            else
            {
                // Read the charge rate
                reader.TryReadValue(k_ChargeRateKey, out m_CurrentChargeRate, m_CurrentChargeRate);

                // Read and set the time-scale if required
                float ts = 1f;
                if (reader.TryReadValue(k_TimeScaleKey, out ts, ts))
                {
                    NeoFpsTimeScale.timeScale = ts;
                    m_TimeIsScaled = true;
                }
            }
        }
    }
}
