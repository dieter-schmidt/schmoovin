using NeoFPS.CharacterMotion;
using NeoFPS.ModularFirearms;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using UnityEngine;

namespace NeoFPS
{
    public class RotationBob : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The angle range range around each axis.")]
        private Vector3 m_RotationRange = new Vector3(1f, 0.5f, 1f);

        [SerializeField, Tooltip("The curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_BobCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
            new Keyframe(2f, 0f)); // curve for head bob

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        private float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        private float m_MaxLerpSpeed = 2f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for the rotation when aiming down sights. This can be used to prevent excessive crosshair wander.")]
        private float m_AimingMultiplier = 0.25f;

        private static readonly NeoSerializationKey k_FadeKey = new NeoSerializationKey("fade");
        private const float k_FadeDuration = 1f;

        private IAdditiveTransformHandler m_Handler = null;
        private IModularFirearm m_Firearm = null;
        private IAimer m_Aimer = null;
        private MotionController m_Controller = null;
        private Vector3 m_Euler = Vector3.zero;
        private float m_FadeTime = 0f;
        private float m_CurrentStrength = 0f;
        private float m_TargetStrength = 1f;

        public Quaternion rotation
        {
            get { return Quaternion.Euler(m_Euler * m_CurrentStrength); }
        }

        public Vector3 position
        {
            get { return Vector3.zero; }
        }

        public float strength
        {
            get { return m_TargetStrength; }
            set { m_TargetStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MaxLerpSpeed < m_MinLerpSpeed + 0.1f)
                m_MaxLerpSpeed = m_MinLerpSpeed + 0.1f;
            m_RotationRange.x = Mathf.Clamp(m_RotationRange.x, -5f, 5f);
            m_RotationRange.y = Mathf.Clamp(m_RotationRange.y, -5f, 5f);
            m_RotationRange.z = Mathf.Clamp(m_RotationRange.z, -5f, 5f);
            m_BobCurve.preWrapMode = WrapMode.Loop;
            m_BobCurve.postWrapMode = WrapMode.Loop;
        }
#endif

        void Awake()
        {
            // Get relevant components
            m_Handler = GetComponent<IAdditiveTransformHandler>();
            m_Firearm = GetComponentInParent<IModularFirearm>();

            m_BobCurve.preWrapMode = WrapMode.Loop;
            m_BobCurve.postWrapMode = WrapMode.Loop;
        }

        private void OnAimerChange(IModularFirearm firearm, IAimer aimer)
        {
            m_Aimer = aimer;
        }

        void Start()
        {
            m_Controller = GetComponentInParent<MotionController>();
            if (m_Firearm != null)
            {
                m_Firearm.onAimerChange += OnAimerChange;
                OnAimerChange(m_Firearm, m_Firearm.aimer);
            }
        }

        void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
            m_CurrentStrength = 0f;
            if (m_Firearm != null)
            {
                m_Firearm.onAimerChange += OnAimerChange;
                OnAimerChange(m_Firearm, m_Firearm.aimer);
            }
        }

        void OnDisable()
        {
            m_Handler.RemoveAdditiveEffect(this);
            if (m_Firearm != null)
            {
                m_Firearm.onAimerChange -= OnAimerChange;
                m_Aimer = null;
            }
        }

        void FixedUpdate()
        {
            // Interpolate user strength
            float multiplier = (m_Aimer != null && m_Aimer.isAiming) ? m_AimingMultiplier : 1f;
            m_CurrentStrength = Mathf.Lerp(m_CurrentStrength, multiplier * m_TargetStrength, Time.deltaTime * 5f);
        }

        public void UpdateTransform()
        {
            if (m_Controller != null && m_Controller.strideLength != 0f)
            {
                float speed = m_Controller.smoothedStepRate;
                if (speed < m_MinLerpSpeed)
                    FadeOut();
                else
                {
                    // Get the bob amount
                    m_Euler = new Vector3(
                       m_BobCurve.Evaluate(m_Controller.stepCounter * 2f) * m_RotationRange.x * 0.5f,
                       m_BobCurve.Evaluate(m_Controller.stepCounter) * m_RotationRange.y * 0.5f,
                       m_BobCurve.Evaluate(m_Controller.stepCounter * 2f + 0.5f) * m_RotationRange.z * 0.5f
                       );

                    if (speed < m_MaxLerpSpeed)
                    {
                        float lerp = (m_Controller.smoothedStepRate - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);
                        m_Euler *= lerp;
                    }

                    m_FadeTime = 0f;
                }
            }
            else
                FadeOut();
        }

        void FadeOut()
        {
            m_FadeTime += Time.deltaTime;
            if (m_FadeTime > k_FadeDuration)
                m_Euler = Vector3.zero;
            else
                m_Euler *= 1f - Time.deltaTime;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_FadeKey, m_FadeTime);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_FadeKey, out m_FadeTime, m_FadeTime);
        }
    }
}