using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    [Serializable]
    public struct MotionGraphDataReference<ValueType>
    {
        [SerializeField] private MotionGraphData<ValueType> m_Data;
        [SerializeField] private ValueType m_Value;

        public ValueType value
        {
            get
            {
                if (m_Data == null)
                    return m_Value;
                else
                    return m_Data.value;
            }
        }

        public MotionGraphDataReference(ValueType v)
        {
            m_Data = null;
            m_Value = v;
        }
    }
}