using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ClampIntParameter", "ClampIntParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-clampintbehaviour.html")]
    public class ClampIntParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private IntParameter m_Parameter = null;

        [SerializeField, Tooltip("The minimum value of the parameter (inclusive).")]
        private int m_From = 0;

        [SerializeField, Tooltip("The maximum value of the parameter (inclusive).")]
        private int m_To = 1;

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