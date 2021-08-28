using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    [Serializable]
    public struct FloatDataReference
    {
        [SerializeField] private FloatData m_Data;
        [SerializeField] private float m_Value;

        public float value
        {
            get
            {
                if (m_Data == null)
                    return m_Value;
                else
                    return m_Data.value;
            }
        }

        public FloatDataReference(float v)
        {
            m_Data = null;
            m_Value = v;
        }

        public void ClampValue(float min, float max)
        {
            m_Value = Mathf.Clamp(m_Value, min, max);
        }

        public void CheckReference(IMotionGraphMap map)
        {
            m_Data = map.Swap(m_Data);
        }
    }
}