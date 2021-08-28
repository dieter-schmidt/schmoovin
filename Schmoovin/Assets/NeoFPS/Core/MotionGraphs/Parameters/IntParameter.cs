using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Integer", "My Integer")]
    public class IntParameter : MotionGraphParameter
    {
        [SerializeField] private int m_StartingValue = 0;

        public UnityAction<int> onValueChanged;

        private bool m_Reset = true;

        private int m_Value = 0;
        public int value
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