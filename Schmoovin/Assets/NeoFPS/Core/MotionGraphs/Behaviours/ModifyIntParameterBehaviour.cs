using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ModifyIntParameter", "ModifyIntParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifyintparameterbehaviour.html")]
    public class ModifyIntParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private IntParameter m_Parameter = null;

        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("How should the parameter be modified.")]
        private What m_What = What.Set;

        [SerializeField, Tooltip("The value to set to, add or subtract based on the \"What\" parameter.")]
        private int m_Value = 0;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public enum What
        {
            Set,
            Reset,
            Add,
            Subtract
        }

        public override void OnEnter()
        {
            if (m_Parameter != null && (m_When == When.OnEnter || m_When == When.Both))
            {
                switch (m_What)
                {
                    case What.Set:
                        m_Parameter.value = m_Value;
                        return;
                    case What.Reset:
                        m_Parameter.ResetValue();
                        return;
                    case What.Add:
                        m_Parameter.value += m_Value;
                        return;
                    case What.Subtract:
                        m_Parameter.value -= m_Value;
                        return;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null && (m_When == When.OnExit || m_When == When.Both))
            {
                switch (m_What)
                {
                    case What.Set:
                        m_Parameter.value = m_Value;
                        return;
                    case What.Reset:
                        m_Parameter.ResetValue();
                        return;
                    case What.Add:
                        m_Parameter.value += m_Value;
                        return;
                    case What.Subtract:
                        m_Parameter.value -= m_Value;
                        return;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}