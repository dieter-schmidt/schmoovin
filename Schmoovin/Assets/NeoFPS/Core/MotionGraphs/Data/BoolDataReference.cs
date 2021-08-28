using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    [Serializable]
    public struct BoolDataReference
    {
        [SerializeField] private BoolData m_Data;
        [SerializeField] private bool m_Value;

        public bool value
        {
            get
            {
                if (m_Data == null)
                    return m_Value;
                else
                    return m_Data.value;
            }
        }

        public BoolDataReference(bool v)
        {
            m_Data = null;
            m_Value = v;
        }

        public void CheckReference(IMotionGraphMap map)
        {
            m_Data = map.Swap(m_Data);
        }
    }
}