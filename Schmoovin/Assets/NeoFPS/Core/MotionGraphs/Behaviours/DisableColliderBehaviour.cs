using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/DisableCollider", "DisableColliderBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-disablecolliderbehaviour.html")]
    public class DisableColliderBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("What to do to the character collider on entering the state.")]
        private What m_OnEnter = What.Disable;

        [SerializeField, Tooltip("What to do to the character collider on exiting the state.")]
        private What m_OnExit = What.Enable;

        public enum What
        {
            Enable,
            Disable,
            Nothing
        }

        public override void OnEnter()
        {
            switch (m_OnEnter)
            {
                case What.Enable:
                    controller.characterController.collisionsEnabled = true;
                    return;
                case What.Disable:
                    controller.characterController.collisionsEnabled =false;
                    return;
            }
        }

        public override void OnExit()
        {
            switch (m_OnExit)
            {
                case What.Enable:
                    controller.characterController.collisionsEnabled = true;
                    return;
                case What.Disable:
                    controller.characterController.collisionsEnabled = false;
                    return;
            }
        }
    }
}