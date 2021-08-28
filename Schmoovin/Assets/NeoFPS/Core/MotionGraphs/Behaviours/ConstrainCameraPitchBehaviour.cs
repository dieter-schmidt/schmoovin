using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Camera/ConstrainCameraPitchBehaviour", "ConstrainCameraPitchBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-constraincamerapitchbehaviour.html")]
    public class ConstrainCameraPitchBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Delayed, Tooltip("The minimum angle the camera can look down.")]
        private float m_MinimumPitch = -89f;
        [SerializeField, Delayed, Tooltip("The maximum angle the camera can look up.")]
        private float m_MaximumPitch = 89f;

        public override void OnEnter()
        {
            controller.aimController.SetPitchConstraints(m_MinimumPitch, m_MaximumPitch);
        }

        public override void OnExit()
        {
            controller.aimController.ResetPitchConstraints();
        }
    }
}