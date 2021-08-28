using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Parameters/Float")]
    public class FloatCondition : MotionGraphCondition 
    {
        [SerializeField] private FloatParameter m_Property = null;
        [SerializeField] private float m_CompareValue = 0f;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.EqualTo;

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
                switch (m_ComparisonType)
                {
                    case ComparisonType.EqualTo:
                        return Mathf.Approximately(m_Property.value, m_CompareValue);
                    case ComparisonType.NotEqualTo:
                        return !Mathf.Approximately(m_Property.value, m_CompareValue);
                    case ComparisonType.GreaterThan:
                        return m_Property.value > m_CompareValue;
                    case ComparisonType.GreaterOrEqual:
                        return m_Property.value >= m_CompareValue;
                    case ComparisonType.LessThan:
                        return m_Property.value < m_CompareValue;
                    case ComparisonType.LessOrEqual:
                        return m_Property.value <= m_CompareValue;
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