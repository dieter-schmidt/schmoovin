using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Switch", "My Switch")]
    public class SwitchParameter : MotionGraphParameter
    {
        [SerializeField] private bool m_StartingValue = false;

        public UnityAction<bool> onValueChanged;

        private bool m_Reset = true;
        private int m_Blockers = 0;

        private bool m_Hold = false;
        private bool m_On = false;
        public bool on
        {
            get
            {
                if (m_Reset)
                {
                    m_Reset = false;
                    // Record value
                    bool previous = on;
                    // Set value
                    m_On = m_StartingValue;
                    // Fire changed event
                    if (on != previous && onValueChanged != null)
                        onValueChanged(on);
                }
               
                if (m_Blockers > 0)
                    return false;
                else
                    return m_On || m_Hold;
            }
            set
            {
                // Record value
                bool previous = on;
                // Set value
                m_On = value;
                // Prevent reset
                m_Reset = false;
                // Fire changed event
                if (on != previous && onValueChanged != null)
                    onValueChanged(on);
            }
        }

        public void Toggle()
        {
            // Record value
            bool previous = on;
            // Set value
            on = !on;
            // Prevent reset
            m_Reset = false;
            // Fire changed event
            if (on != previous && onValueChanged != null)
                onValueChanged(on);
        }

        public void Hold(bool hold = true)
        {
            // Record value
            bool previous = on;
            // Set value
            m_Hold = hold;
            // Prevent reset
            m_Reset = false;
            // Fire changed event
            if (on != previous && onValueChanged != null)
                onValueChanged(on);
        }

        public void SetInput(bool toggle, bool hold)
        {
            // Record value
            bool previous = on;
            // Set value
            if (toggle)
                on = !on;
            m_Hold = hold;
            // Prevent reset
            m_Reset = false;
            // Fire changed event
            if (on != previous && onValueChanged != null)
                onValueChanged(on);
        }

        public void AddBlocker()
        {
            // Record value
            bool previous = on;
            // Increment blockers
            ++m_Blockers;
            // Fire changed event
            if (on != previous && onValueChanged != null)
                onValueChanged(on);
        }

        public void RemoveBlocker()
        {
            // Record value
            bool previous = on;
            // Decrement blockers
            --m_Blockers;
            if (m_Blockers < 0)
            {
                Debug.LogError("Negative number of blockers on motion graph switch property: " + name);
                m_Blockers = 0;
            }
            // Fire changed event
            if (on != previous && onValueChanged != null)
                onValueChanged(on);
        }

        public override void ResetValue ()
        {
            m_Reset = true;
        }
    }
}