#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Vector")]
    public class VectorCondition : MotionGraphCondition
    {
        [SerializeField] private VectorParameter m_Property = null;
        [SerializeField] private VectorComponent m_What = VectorComponent.magnitude;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.EqualTo;
        [SerializeField] private float m_CompareValue = 0f;

        public enum VectorComponent
        {
            magnitude,
            x,
            y,
            z,
            characterHorizontal,
            characterUp
        }

        public enum ComparisonType
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Property != null)
            {
                float lhs = 0f;
                bool squared = false;

                switch (m_What)
                {
                    case VectorComponent.magnitude:
                        lhs = m_Property.value.sqrMagnitude;
                        squared = true;
                        break;
                    case VectorComponent.x:
                        lhs = m_Property.value.x;
                        break;
                    case VectorComponent.y:
                        lhs = m_Property.value.y;
                        break;
                    case VectorComponent.z:
                        lhs = m_Property.value.z;
                        break;
#if NEOFPS_LIGHTWEIGHT
                    case VectorComponent.characterHorizontal:
                        var flattened = m_Property.value;
                        flattened.y = 0f;
                        lhs = flattened.sqrMagnitude;
                        squared = true;
                        break;
                    case VectorComponent.characterUp:
                        lhs = m_Property.value.y;
                        break;
#else
                    case VectorComponent.characterHorizontal:
                        lhs = Vector3.ProjectOnPlane(m_Property.value, controller.characterController.up).sqrMagnitude;
                        squared = true;
                        break;
                    case VectorComponent.characterUp:
                        lhs = Vector3.Dot(m_Property.value, controller.characterController.up);
                        break;
#endif
                }

                // Get right hand side of comparison
                float rhs = m_CompareValue;
                if (squared)
                    rhs *= rhs;

                switch (m_ComparisonType)
                {
                    case ComparisonType.EqualTo:
                        return Mathf.Approximately(lhs, rhs);
                    case ComparisonType.NotEqualTo:
                        return !Mathf.Approximately(lhs, rhs);
                    case ComparisonType.GreaterThan:
                        return lhs > rhs;
                    case ComparisonType.GreaterOrEqual:
                        return lhs >= rhs;
                    case ComparisonType.LessThan:
                        return lhs < rhs;
                    case ComparisonType.LessOrEqual:
                        return lhs <= rhs;
                }
            }
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Property = map.Swap(m_Property);
        }
    }
}