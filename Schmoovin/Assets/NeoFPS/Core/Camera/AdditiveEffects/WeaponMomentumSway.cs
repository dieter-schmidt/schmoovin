using NeoCC;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-weaponmomentumsway.html")]
    public class WeaponMomentumSway : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the position offset left to right")]
        private float m_HorizontalMultiplier = 0.75f;

        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the position offset up and down")]
        private float m_VerticalMultiplier = -1f;

        [SerializeField, Range(0f, 1f), Tooltip("How sensitive the sway is to camera rotation. Higher sensitivity means the sway approaches its greatest offset with slower rotations")]
        private float m_Sensitivity = 0.5f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Approximately the time it will take to reach the target rotation. A smaller value will reach the target faster")]
        private float m_DampingTime = 0.25f;

        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
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

        public Quaternion rotation
        {
            get { return Quaternion.identity; }
        }

        public Vector3 position
        {
            get;
            private set;
        }

        public float strength
        {
            get { return m_TargetUserStrength; }
            set { m_TargetUserStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        void Awake()
        {
            m_Handler = GetComponent<IAdditiveTransformHandler>();
        }

        void OnEnable()
        {
            m_CurrentUserStrength = 0f;
            position = Vector3.zero;

            m_AimController = GetComponentInParent<IAimController>();
            if (m_AimController != null)
            {
                m_Handler.ApplyAdditiveEffect(this);

                m_EulerAngles = Vector3.zero;
                m_AngleDelta = Vector3.zero;

                // Calculate forward
                m_PreviousYawForward = m_AimController.heading;
                m_PreviousPitch = m_AimController.pitch;
            }
        }

        void OnDisable()
        {
            if (m_AimController != null)
                m_Handler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
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

            float inputRotationScale = Mathf.Lerp(0.01f, 0.075f, m_Sensitivity);

            // Damp the input rotation
            m_EulerAngles = Vector3.SmoothDamp(
                m_EulerAngles,
                new Vector3(
                    hDiff * cosPitch * inputRotationScale,
                    vDiff * cosPitch * inputRotationScale,
                    0f),
                ref m_AngleDelta,
                m_DampingTime
            );
            
            // Use the damped input rotation to get a logarithmic position offset
            float hOutScale = m_HorizontalMultiplier * 0.25f * m_CurrentUserStrength;
            float vOutScale = m_VerticalMultiplier * 0.25f * m_CurrentUserStrength;
            position = new Vector3(
                Mathf.Log(Mathf.Abs(m_EulerAngles.x) + 1) * Mathf.Sign(m_EulerAngles.x) * hOutScale,
                Mathf.Log(Mathf.Abs(m_EulerAngles.y) + 1) * Mathf.Sign(m_EulerAngles.y) * vOutScale,
                0f
                );

            // Interpolate user strength
            m_CurrentUserStrength = Mathf.Lerp(m_CurrentUserStrength, m_TargetUserStrength, Time.deltaTime * 5f);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (gameObject.activeSelf)
            {
                writer.WriteValue(k_PositionKey, position);
                writer.WriteValue(k_EulerKey, m_EulerAngles);
                writer.WriteValue(k_RotDeltaKey, m_AngleDelta);
            }
            writer.WriteValue(k_CurrentStrengthKey, m_CurrentUserStrength);
            writer.WriteValue(k_TargetStrengthKey, m_TargetUserStrength);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Vector3 p;
            if (reader.TryReadValue(k_PositionKey, out p, Vector3.zero))
            {
                position = p;

                reader.TryReadValue(k_EulerKey, out m_EulerAngles, m_EulerAngles);
                reader.TryReadValue(k_RotDeltaKey, out m_AngleDelta, m_AngleDelta);
            }
            reader.TryReadValue(k_CurrentStrengthKey, out m_CurrentUserStrength, m_CurrentUserStrength);
            reader.TryReadValue(k_TargetStrengthKey, out m_TargetUserStrength, m_TargetUserStrength);
        }
    }
}