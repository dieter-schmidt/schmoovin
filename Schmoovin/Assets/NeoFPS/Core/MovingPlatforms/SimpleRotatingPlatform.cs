using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS
{
    class SimpleRotatingPlatform : BaseMovingPlatform
    {
        [SerializeField, Tooltip("The total rotation for each rotation phase (relative to the starting rotation of the phase, in world space).")]
        private Vector3 m_Rotation = Vector3.zero;

        [SerializeField, Tooltip("The time it takes to move.")]
        private float m_MovementDuration = 5f;

        [SerializeField, Tooltip("The pause before returning to original position or moving again.")]
        private float m_PauseDuration = 5f;

        private float m_Progress = 0f;
        private float m_ProgressIncrement = 0f;
        private float m_Timer = 0f;
        private float m_TimerIncrement = 0f;

#if UNITY_EDITOR
        void OnValidate()
        {
            m_MovementDuration = Mathf.Clamp(m_MovementDuration, 0.5f, 30f);
            m_PauseDuration = Mathf.Clamp(m_PauseDuration, 0.5f, 30f);

            if (Application.isPlaying)
            {
                m_ProgressIncrement = Time.fixedDeltaTime / m_MovementDuration;
                m_TimerIncrement = Time.fixedDeltaTime / m_PauseDuration;
            }
        }
#endif

        protected override void Start()
        {
            base.Start();

            m_ProgressIncrement = Time.fixedDeltaTime / m_MovementDuration;
            m_TimerIncrement = Time.fixedDeltaTime / m_PauseDuration;
        }

        protected override Vector3 GetNextPosition()
        {
            return fixedPosition;

        }

        protected override Quaternion GetNextRotation()
        {
            if (m_Timer < 1f)
            {
                // Increment the timer
                m_Timer += m_TimerIncrement;
                if (m_Timer > 1f)
                {
                    m_Timer = 1f;
                    m_Progress = 0f;
                }

                // Stay still
                return fixedRotation;
            }
            else
            {
                // Increment the progress
                float previousProgress = m_Progress;
                m_Progress += m_ProgressIncrement;

                // Check if past limits
                if (m_Progress > 1f)
                {
                    m_Progress = 1f;
                    m_Timer = 0f;
                }

                // Get the actual amount lerped
                float amount = m_Progress - previousProgress;
                if (amount > 0f)
                    return fixedRotation * Quaternion.Euler(m_Rotation * amount);
                else
                    return fixedRotation;
            }
        }

        private static readonly NeoSerializationKey k_ProgressKey = new NeoSerializationKey("progress");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            writer.WriteValue(k_ProgressKey, m_Progress);
            writer.WriteValue(k_TimerKey, m_Timer);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            reader.TryReadValue(k_ProgressKey, out m_Progress, m_Progress);
            reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
        }
    }
}
