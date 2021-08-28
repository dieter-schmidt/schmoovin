using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/AddForceBehaviour", "AddForceBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-addforcebehaviour.html")]
    public class AddForceBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("A trigger that will be checked each frame, and the force applied if the trigger is set.")]
        private TriggerParameter m_Trigger = null;

        [SerializeField, Tooltip("The force to apply to the character.")]
        private VectorParameter m_ForceParameter = null;

        [SerializeField, Tooltip("When should the force be added to the character.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("The force to apply to the character.")]
        private Vector3 m_Force = Vector3.zero;

        [SerializeField, Tooltip("How should the force be applied.")]
        private ForceMode m_ForceMode = ForceMode.Impulse;

        public enum When
        {
            OnEnter,
            OnExit,
            WhenTriggered
        }

        public override void OnEnter()
        {
            if (m_When == When.OnEnter)
                ApplyForce();
        }

        public override void OnExit()
        {
            if (m_When == When.OnExit)
                ApplyForce();
        }

        public override void Update()
        {
            if (m_When == When.WhenTriggered && m_Trigger != null && m_Trigger.CheckTrigger())
                ApplyForce();
        }

        void ApplyForce()
        {
            Vector3 force = (m_ForceParameter != null) ? m_ForceParameter.value : m_Force;
            controller.characterController.AddForce(force, m_ForceMode, true);
        }
    }
}