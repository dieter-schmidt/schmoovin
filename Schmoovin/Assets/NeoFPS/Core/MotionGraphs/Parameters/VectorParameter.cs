using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Vector", "My Vector")]
    public class VectorParameter : MotionGraphParameter
    {
        [SerializeField] private Vector3 m_StartingValue = Vector3.zero;

        public UnityAction<Vector3> onValueChanged;

        private bool m_Reset = true;

        private Vector3 m_Value = Vector3.zero;
        public Vector3 value
        {
            get
            {
                if (m_Reset)
                {
                    m_Value = m_StartingValue;
                    m_Reset = false;
                }
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                {
                    m_Value = value;
                    // Fire changed event
                    if (onValueChanged != null)
                        onValueChanged(m_Value);
                }
                m_Reset = false;
            }
        }

        public override void ResetValue ()
        {
            m_Reset = true;
            // Fire changed event
            if (onValueChanged != null && m_Value != m_StartingValue)
                onValueChanged(m_StartingValue);
        }
    }
}