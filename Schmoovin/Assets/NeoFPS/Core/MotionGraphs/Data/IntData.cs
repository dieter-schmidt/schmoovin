using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    [MotionGraphElement("Integer", "myInteger")]
    public class IntData : MotionGraphData<int>
    {
        private List<Func<int, int>> m_Overrides = null;

        public override int value
        {
            get
            {
                if (m_Overrides != null)
                {
                    int currentValue = m_Value;
                    for (int i = 0; i < m_Overrides.Count; ++i)
                        currentValue = m_Overrides[i].Invoke(currentValue);
                    return currentValue;
                }
                else
                    return m_Value;
            }
        }

        public override void AddOverride(IMotionGraphDataOverride over)
        {
            var func = over.GetIntOverride(this);
            if (func != null)
            {
                if (m_Overrides == null)
                    m_Overrides = new List<Func<int, int>>();
                m_Overrides.Add(func);
            }
        }

        public override void RemoveOverride(IMotionGraphDataOverride over)
        {
            var func = over.GetIntOverride(this);
            if (func != null)
                m_Overrides.Remove(func);
        }
    }
}