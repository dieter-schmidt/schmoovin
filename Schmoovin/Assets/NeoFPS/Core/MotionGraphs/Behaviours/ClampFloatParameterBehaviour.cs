using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ClampFloatParameter", "ClampFloatParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-clampfloatbehaviour.html")]
    public class ClampFloatParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private FloatParameter m_Parameter = null;

        [SerializeField, Tooltip("The minimum value of the parameter (inclusive).")]
        private float m_From = 0f;

        [SerializeField, Tooltip("The maximum value of the parameter (inclusive).")]
        private float m_To = 1f;

        public override void OnEnter()
        {
            if (m_Parameter != null)
                m_Parameter.value = Mathf.Clamp(m_Parameter.value, m_From, m_To);
        }

        public override void OnExit()
        {
            if (m_Parameter != null)
                m_Parameter.value = Mathf.Clamp(m_Parameter.value, m_From, m_To);
        }

        public override void Update()
        {
            if (m_Parameter != null)
                m_Parameter.value = Mathf.Clamp(m_Parameter.value, m_From, m_To);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}