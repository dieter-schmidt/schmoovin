using NeoFPS.CharacterMotion;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-positionbob.html")]
    public class PositionBob : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The bob animation data, shared between the head and the item/weapon")]
        private PositionBobData m_BobData = null;
        [SerializeField, Tooltip("Is this bob being applied to the head or the item (allows the effect to blend between the 2 with similar results based on game settings).")]
        private BobType m_BobType = BobType.Head;
        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        private float m_MinLerpSpeed = 0.5f;
        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        private float m_MaxLerpSpeed = 2f;

        private static readonly NeoSerializationKey k_WeightKey = new NeoSerializationKey("weight");
        private const float k_FadeLerp = 0.05f;

        private IAdditiveTransformHandler m_Handler = null;
        private MotionController m_Controller = null;
        private Vector3 m_Position = Vector3.zero;
        private float m_TargetWeight = 0f;
        private float m_Weight = 1f;
        private bool m_Attached = false;

        public enum BobType
        {
            Head,
            Item
        }

        public Quaternion rotation
        {
            get { return Quaternion.identity; }
        }

        public Vector3 position
        {
            get { return m_Position * m_Weight; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MaxLerpSpeed < m_MinLerpSpeed + 0.1f)
                m_MaxLerpSpeed = m_MinLerpSpeed + 0.1f;
        }
#endif

        void Awake()
        {
            // Create default data if none is set
            if (m_BobData == null)
                m_BobData = ScriptableObject.CreateInstance<PositionBobData>();

            // Get relevant components
            m_Controller = GetComponentInParent<MotionController>();
            m_Handler = GetComponent<IAdditiveTransformHandler>();
        }

        void OnHeadBobSettingsChanged(float headWeight)
        {
            // Get the weight
            if (m_BobType == BobType.Item)
                m_Weight = 1f - headWeight;
            else
                m_Weight = headWeight;

            // Attach / detach if required
            if (m_Attached)
            {
                if (m_Weight == 0f)
                {
                    m_Handler.RemoveAdditiveEffect(this);
                    m_Attached = false;
                }
            }
            else
            {
                if (m_Weight > 0f && m_Controller != null && m_Handler != null)
                {
                    m_Handler.ApplyAdditiveEffect(this);
                    m_Attached = true;
                }
            }
        }

        void OnEnable()
        {
            // Attach to settings && get the head vs item weighting
            FpsSettings.gameplay.onHeadBobChanged += OnHeadBobSettingsChanged;
            OnHeadBobSettingsChanged(FpsSettings.gameplay.headBob);
        }

        void OnDisable()
        {
            if (m_Attached)
            {
                m_Handler.RemoveAdditiveEffect(this);
                m_Attached = false;
            }

            // Detach from settings
            FpsSettings.gameplay.onHeadBobChanged -= OnHeadBobSettingsChanged;
        }

        void FixedUpdate()
        {
            m_Weight = Mathf.Lerp(m_Weight, m_TargetWeight, k_FadeLerp);
        }

        public void UpdateTransform()
        {
            float speed = m_Controller.smoothedStepRate;
            if (m_Controller.strideLength == 0f || speed < m_MinLerpSpeed)
                m_TargetWeight = 0f;
            else
                m_TargetWeight = 1f;

            if (m_Weight > 0.0001f)
            {
                // Get the bob amount
                Vector2 bob = m_BobData.GetBobPositionAtTime(m_Controller.stepCounter) * m_Weight;
                if (m_BobType == BobType.Item)
                    bob *= -1f;

                m_Position = bob;

                if (speed < m_MaxLerpSpeed)
                {
                    float lerp = (m_Controller.smoothedStepRate - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);
                    m_Position *= lerp;
                }
            }
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_WeightKey, m_Weight);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_WeightKey, out m_Weight, m_Weight);
        }
    }
}