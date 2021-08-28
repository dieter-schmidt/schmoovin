using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    [MotionGraphElement("Boolean", "myBool")]
    public class BoolData : MotionGraphData<bool>
    {
        private List<Func<bool, bool>> m_Overrides = null;

        public override bool value
        {
            get
            {
                if (m_Overrides != null)
                {
                    bool currentValue = m_Value;
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
            var func = over.GetBoolOverride(this);
            if (func != null)
            {
                if (m_Overrides == null)
                    m_Overrides = new List<Func<bool, bool>>();
                m_Overrides.Add(func);
            }
        }

        public override void RemoveOverride(IMotionGraphDataOverride over)
        {
            var func = over.GetBoolOverride(this);
            if (func != null)
                m_Overrides.Remove(func);
        }
    }
}