using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoCC;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Pitch")]
    public class PitchCondition : MotionGraphCondition
    {
        [SerializeField] private ComparisonType m_ComparisonType = ComparisonType.EqualTo;
        [SerializeField, Range(-90, 90)] private float m_CompareValue = 0f;

        private IAimController m_AimController = null;

        public enum ComparisonType
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterOrEqual,
            LessThan,
            LessOrEqual
        }

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);

            // Get aim controller
            m_AimController = c.GetComponent<IAimController>();
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_AimController != null)
            {
                switch (m_ComparisonType)
                {
                    case ComparisonType.EqualTo:
                        return Mathf.Approximately(m_AimController.pitch, m_CompareValue);
                    case ComparisonType.NotEqualTo:
                        return !Mathf.Approximately(m_AimController.pitch, m_CompareValue);
                    case ComparisonType.GreaterThan:
                        return m_AimController.pitch > m_CompareValue;
                    case ComparisonType.GreaterOrEqual:
                        return m_AimController.pitch >= m_CompareValue;
                    case ComparisonType.LessThan:
                        return m_AimController.pitch < m_CompareValue;
                    case ComparisonType.LessOrEqual:
                        return m_AimController.pitch <= m_CompareValue;
                }
            }
            else
                Debug.Log("Naaah");
            return false;
        }
    }
}