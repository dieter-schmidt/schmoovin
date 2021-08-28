using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Float", "My Float")]
    public class FloatParameter : MotionGraphParameter
    {
        [SerializeField] private float m_StartingValue = 0f;

        public UnityAction<float> onValueChanged;

        private bool m_Reset = true;

        private float m_Value = 0f;
        public float value
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
                if (!Mathf.Approximately(m_Value, value))
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
            if (onValueChanged != null && !Mathf.Approximately(m_Value, m_StartingValue))
                onValueChanged(m_StartingValue);
        }
    }
}