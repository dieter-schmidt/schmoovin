using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudmotiongraphparametermeter.html")]
    public class HudMotionGraphParameterMeter : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The rect transform of the filled bar.")]
        private RectTransform m_BarRect = null;
        [SerializeField, Tooltip("The parameter on the motion graph to track.")]
        private string m_ParameterKey = "myFloat";
        [SerializeField, Tooltip("The minimum value the parameter should reach (the filled section of the bar will be hidden).")]
        private float m_MinValue = 0f;
        [SerializeField, Tooltip("The maximum value the parameter should reach (the entire bar will be filled).")]
        private float m_MaxValue = 1f;

        private FloatParameter m_Parameter = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_Parameter != null)
                m_Parameter.onValueChanged -= OnValueChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old parameter
            if (m_Parameter != null)
                m_Parameter.onValueChanged -= OnValueChanged;

            // Subscribe to new parameter
            if (character as Component != null)
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
            if (m_BarRect != null)
            {
                to -= m_MinValue;
                to /= m_MaxValue - m_MinValue;

                var localScale = m_BarRect.localScale;
                localScale.x = Mathf.Clamp01(to);
                m_BarRect.localScale = localScale;
            }
        }
    }
}