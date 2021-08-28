using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Camera/ConstrainCameraYawBehaviour", "ConstrainCameraYawBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-constraincamerayawbehaviour.html")]
    public class ConstrainCameraYawBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The vector parameter to use as the constraint direction.")]
        private VectorParameter m_Direction = null;        

        [SerializeField, Tooltip("The angle range to constrain to.")]
        private float m_AngleRange = 180f;

        [SerializeField, Tooltip("Flip the direction vector.")]
        private bool m_Flipped = false;

        [SerializeField, Tooltip("Should the constraints be updated each frame (if the vector parameter changes).")]
        private bool m_Continuous = false;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            if (m_Direction == null)
                enabled = false;
        }

        public override void OnEnter()
        {
            if (!m_Continuous)
                ConstrainYaw();
        }

        public override void OnExit()
        {
            controller.aimController.ResetYawConstraints();
        }

        public override void Update()
        {
            ConstrainYaw();
        }

        void ConstrainYaw()
        {
            float mult = m_Direction.value.magnitude;
            if (mult > 0.01f)
            {
                mult = 1 / mult;
                if (m_Flipped)
                    mult *= -1f;
                controller.aimController.SetYawConstraints(m_Direction.value * mult, m_AngleRange);
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Direction = map.Swap(m_Direction);
        }
    }
}