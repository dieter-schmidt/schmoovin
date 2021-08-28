using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ModifyTriggerParameter", "ModifyTriggerParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifytriggerparameterbehaviour.html")]
    public class ModifyTriggerParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private TriggerParameter m_Parameter = null;

        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("What is the action to do.")]
        private What m_What = What.Set;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public enum What
        {
            Set,
            Reset
        }

        public override void OnEnter()
        {
            if (m_Parameter != null && m_When != When.OnExit)
            {
                if (m_What == What.Reset)
                    m_Parameter.ResetValue();
                else
                    m_Parameter.Trigger();
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null && m_When != When.OnEnter)
            {
                if (m_What == What.Reset)
                    m_Parameter.ResetValue();
                else
                    m_Parameter.Trigger();
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}