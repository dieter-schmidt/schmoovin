using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-additivekicker.html")]
	public class AdditiveKicker : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The time taken to ease into the kick.")]
        private float m_LeadIn = 0f;

        [SerializeField, Tooltip("The return spring is an animation curve that dictates how the kicker returns from the kick angle/position (1 on the y-axis) to its originak state (0 on the y axis).")]
        private AnimationCurve m_ReturnSpring = new AnimationCurve(
            new Keyframe(0f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, 0f, 0f)
        );

        private static readonly NeoSerializationKey k_PosElapsedKey = new NeoSerializationKey("posElapsed");
        private static readonly NeoSerializationKey k_PosDurationKey = new NeoSerializationKey("posDuration");
        private static readonly NeoSerializationKey k_PosTargetKey = new NeoSerializationKey("posTarget");
        private static readonly NeoSerializationKey k_RotElapsedKey = new NeoSerializationKey("rotElapsed");
        private static readonly NeoSerializationKey k_RotDurationKey = new NeoSerializationKey("rotDuration");
        private static readonly NeoSerializationKey k_RotTargetKey = new NeoSerializationKey("rotTarget");

        private Quaternion m_RotationStart = Quaternion.identity;
        private Quaternion m_RotationTarget = Quaternion.identity;
        private float m_RotationDuration = 0f;
		private float m_RotationElapsed = 1f;
        private bool m_RotationLeadIn = false;
        private Vector3 m_PositionStart = Vector3.zero;
        private Vector3 m_PositionTarget = Vector3.zero;
        private float m_PositionDuration = 0f;
		private float m_PositionElapsed = 1f;
        private bool m_PositionLeadIn = false;
        
        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }
        
		public Vector3 position
        {
            get;
            private set;
        }
        
		public Quaternion rotation
        {
            get;
            private set;
        }


        public bool bypassPositionMultiplier
        {
            get;
            set;
        }

        public bool bypassRotationMultiplier
        {
            get;
            set;
        }

        void OnValidate()
        {
            m_LeadIn = Mathf.Clamp(m_LeadIn, 0.001f, 1f);
        }

		void Awake ()
		{
			transformHandler = GetComponent<IAdditiveTransformHandler>();
		}

		void OnEnable ()
		{
			transformHandler.ApplyAdditiveEffect (this);
		}

		void OnDisable ()
		{
			transformHandler.RemoveAdditiveEffect (this);
		}

        public void UpdateTransform()
        {
            m_PositionElapsed += Time.deltaTime;
            if (m_PositionLeadIn)
            {
                if (m_PositionElapsed > m_LeadIn)
                {
                    position = m_PositionTarget;
                    m_PositionLeadIn = false;
                    m_PositionElapsed = 0f;
                }
                else
                {
                    float eased = EasingFunctions.EaseOutQuadratic(m_PositionElapsed / m_LeadIn);
                    position = Vector3.Lerp(m_PositionStart, m_PositionTarget, eased);
                }
            }
            else
            {
                if (m_PositionElapsed > m_PositionDuration)
                {
                    position = Vector3.zero;
                }
                else
                {
                    position = Vector3.LerpUnclamped(
                        Vector3.zero,
                        m_PositionTarget,
                        m_ReturnSpring.Evaluate(m_PositionElapsed / m_PositionDuration)
                    );
                }
            }

            m_RotationElapsed += Time.deltaTime;
            if (m_RotationLeadIn)
            {
                if (m_RotationElapsed > m_LeadIn)
                {
                    rotation = m_RotationTarget;
                    m_RotationLeadIn = false;
                    m_RotationElapsed = 0f;
                }
                else
                {
                    float eased = EasingFunctions.EaseOutQuadratic(m_RotationElapsed / m_LeadIn);
                    rotation = Quaternion.Lerp(m_RotationStart, m_RotationTarget, eased);
                }
            }
            else
            {
                if (m_RotationElapsed > m_RotationDuration)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    rotation = Quaternion.LerpUnclamped(
                        Quaternion.identity,
                        m_RotationTarget,
                        m_ReturnSpring.Evaluate(m_RotationElapsed / m_RotationDuration)
                    );
                }
            }
        }

		public void KickRotation (Quaternion target, float duration)
		{
            if (duration > 0.01f)
            {
                m_RotationLeadIn = (m_LeadIn > 0.001f);
                m_RotationStart = rotation;
                m_RotationTarget = rotation * target;
                m_RotationDuration = duration;
                m_RotationElapsed = 0f;
            }
            else
                Debug.LogError("Additive.KickRotation duration too small");
		}

		public void KickPosition (Vector3 target, float duration)
        {
            if (duration > 0.01f)
            {
                m_PositionLeadIn = (m_LeadIn > 0.001f);
                m_PositionStart = position;
                m_PositionTarget = position + target;
                m_PositionDuration = duration;
                m_PositionElapsed = 0f;
            }
            else
                Debug.LogError("Additive.KickPosition duration too small");
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_PositionElapsed <= m_PositionDuration)
            {
                writer.WriteValue(k_PosElapsedKey, m_PositionElapsed);
                writer.WriteValue(k_PosDurationKey, m_PositionDuration);
                writer.WriteValue(k_PosTargetKey, m_PositionTarget);
            }

            if (m_RotationElapsed <= m_RotationDuration)
            {
                writer.WriteValue(k_RotElapsedKey, m_RotationElapsed);
                writer.WriteValue(k_RotDurationKey, m_RotationDuration);
                writer.WriteValue(k_RotTargetKey, m_RotationTarget);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_PosElapsedKey, out m_PositionElapsed, m_PositionElapsed))
            {
                reader.TryReadValue(k_PosDurationKey, out m_PositionDuration, m_PositionDuration);
                reader.TryReadValue(k_PosTargetKey, out m_PositionTarget, m_PositionTarget);
            }
            else
                position = Vector3.zero;

            if (reader.TryReadValue(k_RotElapsedKey, out m_RotationElapsed, m_RotationElapsed))
            {
                reader.TryReadValue(k_RotDurationKey, out m_RotationDuration, m_RotationDuration);
                reader.TryReadValue(k_RotTargetKey, out m_RotationTarget, m_RotationTarget);
            }
            else
                rotation = Quaternion.identity;
        }
    }
}
