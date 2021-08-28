using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/SteeringRate", "SteeringRateBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-steeringratebehaviour.html")]
    public class SteeringRateBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("What to do to the steering rate on entering the state.")]
        private EntryAction m_OnEnter = EntryAction.Set;

        [SerializeField, Tooltip("What to do to the steering rate on exiting the state.")]
        private ExitAction m_OnExit = ExitAction.Reset;

        [SerializeField, Tooltip("The value to set the steering rate to on entering the state.")]
        private float m_EntryValue = 0.1f;

        [SerializeField, Tooltip("The value to set the steering rate to on exiting the state.")]
        private float m_ExitValue = 1f;

        public enum EntryAction
        {
            Set,
            Ignore
        }

        public enum ExitAction
        {
            Set,
            Ignore,
            Reset
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            if (m_OnExit == ExitAction.Reset && m_OnEnter != EntryAction.Set)
                m_OnExit = ExitAction.Ignore;
        }

        public override void OnEnter()
        {
            if (m_OnEnter == EntryAction.Set)
            {
                if (m_OnExit == ExitAction.Reset)
                    m_ExitValue = controller.aimController.steeringRate;
                controller.aimController.steeringRate = m_EntryValue;
            }
        }

        public override void OnExit()
        {
            if (m_OnExit != ExitAction.Ignore)
                controller.aimController.steeringRate = m_ExitValue;
        }
    }
}