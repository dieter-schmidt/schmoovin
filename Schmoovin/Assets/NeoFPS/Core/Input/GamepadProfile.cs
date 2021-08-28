using NeoFPS.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [Serializable]
    public class GamepadProfile
    {
        [SerializeField, Tooltip("The name of the profile")]
        private string m_Name = string.Empty;
        [SerializeField, Tooltip("The analogue control order (look vs move)")]
        private AnalogueSetup m_AnalogueSetup = AnalogueSetup.LeftMoveRightLook;
        [SerializeField, Tooltip("The button mappings")]
        private ButtonMappings[] m_ButtonMappings = { };

#if UNITY_EDITOR

        [SerializeField]
        private FpsInputButton[] m_Buttons = { };

        public bool expanded = true;
#endif

        [Serializable]
        class ButtonMappings
        {
            public FpsInputButton[] m_Buttons;

            public ButtonMappings()
            {
                m_Buttons = new FpsInputButton[0];
            }

            public ButtonMappings(FpsInputButton b)
            {
                m_Buttons = new FpsInputButton[] { b };
            }
        }

        public enum AnalogueSetup
        {
            LeftMoveRightLook,
            RightMoveLeftLook
        }

        public string name
        {
            get { return m_Name; }
        }

        public AnalogueSetup analogueSetup
        {
            get { return m_AnalogueSetup; }
        }

        public FpsInputButton[] GetInputButtonsForGamepadButton(GamepadButton gpb)
        {
            return m_ButtonMappings[(int)gpb].m_Buttons;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            // Check for old buttons mappings and use to set up current
            if (m_Buttons.Length != 0 && m_ButtonMappings.Length == 0)
            {
                m_ButtonMappings = new ButtonMappings[m_Buttons.Length];
                for (int i = 0; i < m_Buttons.Length; ++i)
                    m_ButtonMappings[i] = new ButtonMappings(m_Buttons[i]);
            }

            int count = (int)GamepadButton.Count;
            if (m_Buttons.Length < count)
            {
                var swap = new FpsInputButton[count];

                int i = 0;
                for (; i < swap.Length && i < m_Buttons.Length; ++i)
                    swap[i] = m_Buttons[i];
                for (; i < swap.Length; ++i)
                    swap[i] = FpsInputButton.None;

                m_Buttons = swap;
            }
        }
#endif
    }
}