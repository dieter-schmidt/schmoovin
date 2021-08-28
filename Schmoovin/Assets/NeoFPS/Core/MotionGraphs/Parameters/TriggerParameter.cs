using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Trigger", "My Trigger")]
    public class TriggerParameter : MotionGraphParameter
    {
        private bool m_Triggered = false;
        private int m_Blockers = 0;

        public bool wasChecked
        {
            get;
            private set;
        }

        public void Trigger ()
        {
            if (m_Blockers == 0)
                m_Triggered = true;
        }

        public bool PeekTrigger ()
        {
            return m_Triggered;
        }

        public bool CheckTrigger ()
        {
            bool result = m_Triggered;
            wasChecked = true;
            return result;
        }

        public void AddBlocker()
        {
            ++m_Blockers;
            m_Triggered = false;
        }

        public void RemoveBlocker()
        {
            --m_Blockers;
            if (m_Blockers < 0)
            {
                Debug.LogError("Negative number of blockers on motion graph trigger: " + name);
                m_Blockers = 0;
            }
        }

        public override void ResetValue ()
        {
            m_Triggered = false;
        }
    }
}