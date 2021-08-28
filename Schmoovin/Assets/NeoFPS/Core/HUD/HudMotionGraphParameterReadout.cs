using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudmotiongraphparameterreadout.html")]
    public class HudMotionGraphParameterReadout : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The UI text element to print the value")]
        private Text m_ReadoutText = null;
        [SerializeField, Tooltip("The name of the parameter on the character motion graph to track")]
        private string m_ParameterKey = "myFloat";
        [SerializeField, Tooltip("The C# formatting style of the parameter value")]
        private string m_Formatting = "F2";

        private FloatParameter m_Parameter = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old parameter
            if (m_Parameter != null)
                m_Parameter.onValueChanged -= OnValueChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old parameter
            if (m_Parameter != null)
                m_Parameter.onValueChanged -= OnValueChanged;

            // Subscribe to new parameter
            if (character != null)
            {
                var m = character.GetComponent<MotionController>();
                if (m != null)
                {
                    m_Parameter = m.motionGraph.GetFloatProperty(m_ParameterKey);
                    if (m_Parameter != null)
                    {
                        m_Parameter.onValueChanged += OnValueChanged;
                        OnValueChanged(m_Parameter.value);
                        gameObject.SetActive(true);
                    }
                    else
                        gameObject.SetActive(false);
                }
            }
        }

        protected virtual void OnValueChanged(float to)
        {
            if (m_ReadoutText != null)
                m_ReadoutText.text = to.ToString(m_Formatting);
        }
    }
}