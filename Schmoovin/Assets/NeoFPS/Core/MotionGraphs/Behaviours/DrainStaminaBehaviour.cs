using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/DrainStamina", "DrainStaminaBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-drainstaminabehaviour.html")]
    public class DrainStaminaBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The rate to drain the stamina at (bear in mind the stamina system also refreshes at a certain rate too, so these can cancel out).")]
        private float m_DrainRate = 15f;

        [SerializeField, Tooltip("Should the controller's move input scale also scale the stamina drain.")]
        private bool m_ScaleByInput = true;

        [SerializeField, Tooltip("Is there a lower limit that the behaviour will drain stamina to before the drain rate falls off.")]
        private bool m_LimitDrain = false;

        [SerializeField, Tooltip("The minimum level that the behaviour can drain stamina to.")]
        private float m_DrainTarget = 0f;

        [SerializeField, Tooltip("The stamina drain falls away to 0 as it approaches the target level, starting at this falloff value above it.")]
        private float m_DrainFalloff = 0f;

        private IStaminaSystem m_StaminaSystem = null;
        private float m_InverseDrainFalloff = 1f;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            // Get the stamina system
            m_StaminaSystem = controller.GetComponent<IStaminaSystem>();
            // Calculate drain falloff
            if (m_DrainFalloff > 1f)
                m_InverseDrainFalloff = 1f / m_DrainFalloff;
        }

        public override void OnEnter()
        {
            if (m_StaminaSystem != null)
                m_StaminaSystem.AddStaminaDrain(GetStaminaDrain);
        }

        public override void OnExit()
        {
            if (m_StaminaSystem != null)
                m_StaminaSystem.RemoveStaminaDrain(GetStaminaDrain);
        }

        private float GetStaminaDrain(IStaminaSystem s, float modifiedStamina)
        {
            float rate = m_DrainRate;

            // Scale drain by input
            if (m_ScaleByInput)
                rate *= controller.inputMoveScale;

            // Limit the drain based on the input stamina
            if (m_LimitDrain)
                rate *= Mathf.Clamp01((modifiedStamina - m_DrainTarget) * m_InverseDrainFalloff);

            return rate * Time.deltaTime;
        }
    }
}