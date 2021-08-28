using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using NeoCC;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-weaponaimamplifier.html")]
	public class WeaponAimAmplifier : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation side to side")]
        private float m_HorizontalMultiplier = 1f;

        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation up and down")]
        private float m_VerticalMultiplier = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("How sensitive the sway is to camera rotation. Higher sensitivity means the sway approaches its peak with slower rotations")]
        private float m_Sensitivity = 0.5f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Approximately the time it will take to reach the target rotation. A smaller value will reach the target faster")]
		private float m_DampingTime = 0.25f;

        private static readonly NeoSerializationKey k_RotationKey = new NeoSerializationKey("rotation");
        private static readonly NeoSerializationKey k_PrevyawKey = new NeoSerializationKey("prevYaw");
        private static readonly NeoSerializationKey k_PrevPitchKey = new NeoSerializationKey("prevPitch");
        private static readonly NeoSerializationKey k_EulerKey = new NeoSerializationKey("euler");
        private static readonly NeoSerializationKey k_RotDeltaKey = new NeoSerializationKey("rotDelta");
        private static readonly NeoSerializationKey k_CurrentStrengthKey = new NeoSerializationKey("currentStr");
        private static readonly NeoSerializationKey k_TargetStrengthKey = new NeoSerializationKey("targetStr");

        private IAdditiveTransformHandler m_Handler = null;
        private IAimController m_AimController = null;
        private Vector3 m_PreviousYawForward = Vector3.forward;
		private Vector3 m_EulerAngles = Vector3.zero;
		private Vector3 m_AngleDelta = Vector3.zero;
        private float m_PreviousPitch = 0f;
        private float m_CurrentUserStrength = 0f;
        private float m_TargetUserStrength = 1f;
        private bool m_SkipReset = false;

        public Quaternion rotation
		{
            get;
            private set;
		}

		public Vector3 position
		{
			get { return Vector3.zero; }
        }

        public float strength
        {
            get { return m_TargetUserStrength; }
            set { m_TargetUserStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        void Awake ()
		{
			m_Handler = GetComponent<IAdditiveTransformHandler>();
		}

		void OnEnable ()
        {
            m_AimController = GetComponentInParent<IAimController>();
            if (m_AimController != null)
            {
                m_Handler.ApplyAdditiveEffect(this);

                if (!m_SkipReset)
                {
                    m_EulerAngles = Vector3.zero;
                    m_AngleDelta = Vector3.zero;

                    // Calculate forward
                    m_PreviousYawForward = m_AimController.heading;
                    m_PreviousPitch = m_AimController.pitch;
                }
                else
                    m_SkipReset = false;
            }
        }

		void OnDisable ()
        {
            if (m_AimController != null)
                m_Handler.RemoveAdditiveEffect (this);
		}

		public void UpdateTransform ()
        {
            // Interpolate user strength
            m_CurrentUserStrength = Mathf.Lerp(m_CurrentUserStrength, m_TargetUserStrength, Time.deltaTime * 5f);

            // Get horizontal angle diff
            Vector3 yawForward = m_AimController.heading;
            float hDiff = Vector3.SignedAngle(m_PreviousYawForward, yawForward, m_AimController.yawUp);
            m_PreviousYawForward = yawForward;

            // Get vertical angle diff
            float pitch = m_AimController.pitch;
            float vDiff = pitch - m_PreviousPitch;
            m_PreviousPitch = pitch;

            // Get multiplier from pitch
            float cosPitch = Mathf.Cos(pitch * Mathf.Deg2Rad);
            cosPitch = Mathf.Lerp(0.25f, 1f, cosPitch);

            float hInputRotationScale = Mathf.Lerp(0.01f, 0.1f, m_Sensitivity);
            float vInputRotationScale = Mathf.Lerp(0.02f, 0.2f, m_Sensitivity);

            // Damp the input rotation
            m_EulerAngles = Vector3.SmoothDamp(
                m_EulerAngles,
                new Vector3(
                    hDiff * cosPitch * hInputRotationScale,
                    vDiff * cosPitch * vInputRotationScale,
                    0f),
                ref m_AngleDelta,
                m_DampingTime
            );

            // Use the damped input rotation to get a logarithmic position offset
            float hOutScale = 7.5f * m_HorizontalMultiplier * m_CurrentUserStrength;
            float vOutScale = 7.5f * m_VerticalMultiplier * m_CurrentUserStrength;
            rotation = Quaternion.Euler(
                -Mathf.Log(Mathf.Abs(m_EulerAngles.y) + 1) * Mathf.Sign(m_EulerAngles.y) * vOutScale,
                Mathf.Log(Mathf.Abs(m_EulerAngles.x) + 1) * Mathf.Sign(m_EulerAngles.x) * hOutScale,
                0f
                );
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (gameObject.activeSelf)
            {
                writer.WriteValue(k_RotationKey, rotation);
                //writer.WriteValue(k_EulerKey, m_EulerAngles);
                //writer.WriteValue(k_RotDeltaKey, m_AngleDelta);
                //writer.WriteValue(k_PrevyawKey, m_PreviousYawForward);
                //writer.WriteValue(k_PrevPitchKey, m_PreviousPitch);
            }
            writer.WriteValue(k_CurrentStrengthKey, m_CurrentUserStrength);
            writer.WriteValue(k_TargetStrengthKey, m_TargetUserStrength);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Quaternion r;
            if (reader.TryReadValue(k_RotationKey, out r, Quaternion.identity))
            {
                rotation = r;

                //reader.TryReadValue(k_EulerKey, out m_EulerAngles, m_EulerAngles);
                //reader.TryReadValue(k_RotDeltaKey, out m_AngleDelta, m_AngleDelta);
                //reader.TryReadValue(k_PrevyawKey, out m_PreviousYawForward, m_PreviousYawForward);
                //reader.TryReadValue(k_PrevPitchKey, out m_PreviousPitch, m_PreviousPitch);
                //m_SkipReset = true;
            }    

            reader.TryReadValue(k_CurrentStrengthKey, out m_CurrentUserStrength, m_CurrentUserStrength);
            reader.TryReadValue(k_TargetStrengthKey, out m_TargetUserStrength, m_TargetUserStrength);
        }
    }
}
