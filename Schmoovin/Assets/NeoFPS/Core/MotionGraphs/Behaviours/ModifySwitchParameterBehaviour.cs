using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ModifySwitchParameter", "ModifySwitchParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifyswitchparameterbehaviour.html")]
    public class ModifySwitchParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to set.")]
        private SwitchParameter m_Parameter = null;

        [SerializeField, Tooltip("How should the parameter be modified on entering the state.")]
        private ChangeValue m_OnEnter = ChangeValue.Unchanged;

        [SerializeField, Tooltip("How should the parameter be modified on entering the state.")]
        private ChangeValue m_OnExit = ChangeValue.Unchanged;

        private bool m_Previous = false;

        public enum ChangeValue
        {
            Unchanged,
            True,
            False,
            Toggle,
            Reset,
            Previous
        }

        public override void OnEnter()
        {
            if (m_Parameter != null)
            {
                // Record old value
                if (m_OnEnter != ChangeValue.Unchanged)
                    m_Previous = m_Parameter.on;

                // Change value
                switch (m_OnEnter)
                {
                    case ChangeValue.True:
                        m_Parameter.on = true;
                        return;
                    case ChangeValue.False:
                        m_Parameter.on = false;
                        return;
                    case ChangeValue.Toggle:
                        m_Parameter.on = !m_Parameter.on;
                        return;
                    case ChangeValue.Reset:
                        m_Parameter.ResetValue();
                        return;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null)
            {
                switch (m_OnExit)
                {
                    case ChangeValue.True:
                        m_Parameter.on = true;
                        return;
                    case ChangeValue.False:
                        m_Parameter.on = false;
                        return;
                    case ChangeValue.Toggle:
                        m_Parameter.on = !m_Parameter.on;
                        return;
                    case ChangeValue.Reset:
                        m_Parameter.ResetValue();
                        return;
                    case ChangeValue.Previous:
                        if (m_OnEnter != ChangeValue.Unchanged)
                            m_Parameter.on = m_Previous;
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