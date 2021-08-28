using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Air Time")]
    public class AirTimeCondition : MotionGraphCondition
    {
        [SerializeField] private float m_CompareValue = 0f;
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.Greater;

        public enum ComparisonType
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            switch (m_ComparisonType)
            {
                case ComparisonType.Equal:
                    return Mathf.Approximately(controller.characterController.airTime, m_CompareValue);
                case ComparisonType.NotEqual:
                    return !Mathf.Approximately(controller.characterController.airTime, m_CompareValue);
                case ComparisonType.Greater:
                    return controller.characterController.airTime > m_CompareValue;
                case ComparisonType.GreaterOrEqual:
                    return controller.characterController.airTime >= m_CompareValue;
                case ComparisonType.Less:
                    return controller.characterController.airTime < m_CompareValue;
                case ComparisonType.LessOrEqual:
                    return controller.characterController.airTime <= m_CompareValue;
            }
            return false; 
        }
    }
}