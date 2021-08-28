#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Direction")]
    public class DirectionCondition : MotionGraphCondition
    {
        [SerializeField] private VectorParameter m_VectorParameter = null;
        [SerializeField] private float m_Angle = 45f;
        [SerializeField] private bool m_LessThan = true;
        [SerializeField] private ComparisonType m_Comparison = ComparisonType.YawVsHorizontal;

        private const float k_MinSpeedSquared = 0.0001f;
        private const float k_MinVectorLength = 0.0001f;
        private const float k_MinInputScale = 0.025f;

        public enum ComparisonType
        {
            YawVsVector,
            YawVsHorizontal,
            AimVsVector,
            VelocityVsVector,
            VelocityVsHorizontal,
            InputVsHorizontal
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_Angle = Mathf.Clamp(m_Angle, 0f, 180f);
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_VectorParameter != null && m_VectorParameter.value.sqrMagnitude > k_MinVectorLength)
            {
                // Get comparison vector 1
                Vector3 v1 = Vector3.zero;
                Vector3 v2 = Vector3.zero;

                switch (m_Comparison)
                {
                    // Compare character body forwards to vector
                    case ComparisonType.YawVsVector:
                        v1 = m_VectorParameter.value;
                        v2 = controller.characterController.forward;
                        break;
                    // Compare character body forwards to vector projected on character horizontal
                    case ComparisonType.YawVsHorizontal:
#if NEOFPS_LIGHTWEIGHT
                        v1 = m_VectorParameter.value;
                        v1.y = 0f;
#else
                        v1 = Vector3.ProjectOnPlane(m_VectorParameter.value, controller.characterController.up);
#endif
                        v2 = controller.characterController.forward;
                        break;
                    // Compare character aim direction to vector
                    case ComparisonType.AimVsVector:
                        v1 = m_VectorParameter.value;
                        v2 = controller.aimController.forward;
                        break;
                    // Compare character move direction to vector (not moving = false)
                    case ComparisonType.VelocityVsVector:
                        v1 = m_VectorParameter.value;
                        v2 = controller.characterController.velocity;
                        if (v2.sqrMagnitude < k_MinSpeedSquared)
                            return false;
                        break;
                    // Compare character horizontal move to horizontal projected vector (not moving = false)
                    case ComparisonType.VelocityVsHorizontal:
#if NEOFPS_LIGHTWEIGHT
                        v1 = m_VectorParameter.value;
                        v2 = controller.characterController.velocity;
                        v1.y = v2.y = 0f;
#else
                        v1 = Vector3.ProjectOnPlane(m_VectorParameter.value, controller.characterController.up);
                        v2 = Vector3.ProjectOnPlane(controller.characterController.velocity, controller.characterController.up);
#endif
                        if (v2.sqrMagnitude < k_MinSpeedSquared)
                            return false;
                        break;
                    // Compare character input direction (takes yaw into account) to vector projected on character horizontal
                    case ComparisonType.InputVsHorizontal:
                        if (controller.inputMoveScale < k_MinInputScale)
                            return false;
                        else
                        {
#if NEOFPS_LIGHTWEIGHT
                            v1 = m_VectorParameter.value;
                            v1.y = 0f;
#else
                            v1 = Vector3.ProjectOnPlane(m_VectorParameter.value, controller.characterController.up);
#endif
                            v2 = new Vector3(controller.inputMoveDirection.x, 0f, controller.inputMoveDirection.y);
                            v2 = controller.localTransform.rotation * v2;
                        }
                        break;
                }

                // Get angle between
                float angleBetween = Vector3.Angle(v1, v2);

                // Compare
                if (m_LessThan)
                    return angleBetween < m_Angle;
                else
                    return angleBetween > m_Angle;
            }

            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_VectorParameter = map.Swap(m_VectorParameter);
        }
    }
}