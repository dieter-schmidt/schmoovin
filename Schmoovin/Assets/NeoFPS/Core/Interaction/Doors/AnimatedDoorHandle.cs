using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-animateddoorhandle.html")]
    public class AnimatedDoorHandle : MonoBehaviour, INeoSerializableComponent
    {
        [Header("Twist Open")]

		[SerializeField, Tooltip("The desired euler angles when twisting the handle.")]
        private Vector3 m_TwistAngle = Vector3.zero;

		[SerializeField, Tooltip("How long does the twist and release last."), FormerlySerializedAs("m_AnimationDuration")]
        private float m_TwistDuration = 1f;

		[SerializeField, Tooltip("The animation curve for the twist."), FormerlySerializedAs("m_AnimationCurve")]
        private AnimationCurve m_TwistCurve = AnimationCurve.EaseInOut (0f, 1f, 1f, 0f);

        [Header("Locked Jiggle")]

        [SerializeField, Tooltip("The maximum euler angles when shaking the handle (if the door is locked).")]
        private Vector3 m_JiggleAngle = Vector3.zero;

        [SerializeField, Tooltip("How long does the locked door jiggle last.")]
        private float m_JiggleDuration = 1f;

        [SerializeField, Tooltip("The animation curve for the locked door handle jiggle.")]
        private AnimationCurve m_JiggleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private static readonly NeoSerializationKey k_ProgressKey = new NeoSerializationKey("progress");
        private static readonly NeoSerializationKey k_ModeKey = new NeoSerializationKey("mode");

        private Coroutine m_AnimationCoroutine = null;
        private Transform m_LocalTransform = null;
        private float m_Lerp = 0f;
        private Mode m_Mode = Mode.Idle;

        enum Mode : int
        {
            Idle,
            Twist,
            Jiggle
        }

        void OnValidate()
        {
            m_TwistDuration = Mathf.Clamp(m_TwistDuration, 0f, 5f);
            m_JiggleDuration = Mathf.Clamp(m_JiggleDuration, 0f, 5f);
        }

        void Awake()
        {
            m_LocalTransform = transform;
        }

        public void Twist()
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            m_Mode = Mode.Twist;
            m_AnimationCoroutine = StartCoroutine(Animation(0f));
        }

        public void Jiggle()
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            if (m_JiggleDuration > 0f)
            {
                m_Mode = Mode.Jiggle;
                m_AnimationCoroutine = StartCoroutine(Animation(0f));
            }
            else
                m_Mode = Mode.Idle;
        }

        IEnumerator Animation(float start)
        {
            var angles = (m_Mode == Mode.Jiggle) ? m_JiggleAngle : m_TwistAngle;
            var curve = (m_Mode == Mode.Jiggle) ? m_JiggleCurve : m_TwistCurve;
            float inverseDuration = 1f / ((m_Mode == Mode.Jiggle) ? m_JiggleDuration : m_TwistDuration);

            m_Lerp = start;

            while (m_Lerp < 1f)
            {
                yield return null;
                
                // Get the euler angles
                m_Lerp = Mathf.Clamp01 (m_Lerp + Time.deltaTime * inverseDuration);
                Vector3 euler = curve.Evaluate(m_Lerp) * angles;

                // Rotate the handle
                m_LocalTransform.localRotation = Quaternion.Euler(euler);
            }

            m_AnimationCoroutine = null;
            m_Mode = Mode.Idle;
        }
        
        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_Mode != Mode.Idle)
            {
                writer.WriteValue(k_ModeKey, (int)m_Mode);
                writer.WriteValue(k_ProgressKey, m_Lerp);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            int m;
            if (reader.TryReadValue(k_ModeKey, out m, 0))
            {
                if (reader.TryReadValue(k_ProgressKey, out m_Lerp, m_Lerp))
                {
                    m_Mode = (Mode)m;
                    m_AnimationCoroutine = StartCoroutine(Animation(m_Lerp));
                }
            }
        }
    }
}