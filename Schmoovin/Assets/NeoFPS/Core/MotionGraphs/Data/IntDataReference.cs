using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    [Serializable]
    public struct IntDataReference
    {
        [SerializeField] private IntData m_Data;
        [SerializeField] private int m_Value;

        public int value
        {
            get
            {
                if (m_Data == null)
                    return m_Value;
                else
                    return m_Data.value;
            }
        }

        public IntDataReference(int v)
        {
            m_Data = null;
            m_Value = v;
        }
        
        public void ClampValue(int min, int max)
        {
            m_Value = Mathf.Clamp(m_Value, min, max);
        }

        public void CheckReference(IMotionGraphMap map)
        {
            m_Data = map.Swap(m_Data);
        }
    }
}