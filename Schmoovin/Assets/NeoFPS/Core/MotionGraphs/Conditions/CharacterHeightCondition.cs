using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Character Height")]
    public class CharacterHeightCondition : MotionGraphCondition
    {
        [SerializeField] private float m_CompareTo = 0.5f;
        [SerializeField] private HeightMode m_HeightMode = HeightMode.Multiplier;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.EqualTo;

        public enum HeightMode
        {
            Multiplier,
            ActualHeight
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
            float compare = (m_HeightMode == HeightMode.Multiplier) ?
                controller.GetHeightMultiplier() :
                controller.characterController.height;
            
            switch (m_ComparisonType)
            {
                case ComparisonType.EqualTo:
                    return Mathf.Approximately(compare, m_CompareTo);
                case ComparisonType.NotEqualTo:
                    return !Mathf.Approximately(compare, m_CompareTo);
                case ComparisonType.GreaterThan:
                    return compare > m_CompareTo;
                case ComparisonType.GreaterOrEqual:
                    return compare >= m_CompareTo;
                case ComparisonType.LessThan:
                    return compare < m_CompareTo;
                case ComparisonType.LessOrEqual:
                    return compare <= m_CompareTo;
            }
            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            //m_Property = map.Swap(m_Property);
        }
    }
}