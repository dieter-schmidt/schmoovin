using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Boost Pad", "BoostPad")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-boostpadstate.html")]
    public class BoostPadState : MotionGraphState
    {
        [SerializeField, Tooltip("The movement velocity to apply.")]
		private VectorParameter m_BoostVector = null;
        [SerializeField, Tooltip("A multiplier for the movement velocity.")]
		private FloatDataReference m_Multiplier = new FloatDataReference(1f);
        [SerializeField, Tooltip("How the boost vector is applied to the character. Options are: " +
            "Absolute sets the character velocity, " +
            "Additive adds the boost to the character velocity, " +
            "MaintainPerpendicular sets the velocity in the direction of the boost, but keeps any velocity along the plane perpendicular to the boost.")]
        private BoostMode m_BoostMode = BoostMode.Absolute;

        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;

        enum BoostMode
        {
            Absolute,
            Additive,
            MaintainPerpendicular
        }

        public override bool completed
        {
            get { return m_Completed; }
        }

        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return false; }
        }

        public override bool applyGroundingForce
        {
            get { return false; }
        }

        public override bool ignorePlatformMove
        {
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Completed = false;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_Completed = false;
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            if (!m_Completed)
            {
                if (m_BoostVector != null)
                {
                    switch (m_BoostMode)
                    {
                        case BoostMode.Absolute:
                            m_OutVelocity = m_BoostVector.value * m_Multiplier.value;
                            break;
                        case BoostMode.Additive:
                            m_OutVelocity = characterController.velocity + m_BoostVector.value * m_Multiplier.value;
                            break;
                        case BoostMode.MaintainPerpendicular:
                            var perpendicular = Vector3.ProjectOnPlane(characterController.velocity, m_BoostVector.value.normalized);
                            m_OutVelocity = m_BoostVector.value * m_Multiplier.value + perpendicular;
                            break;
                    }

                    m_BoostVector.value = Vector3.zero;
                }
                m_Completed = true;
            }
            else
                m_OutVelocity = characterController.velocity;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_BoostVector = map.Swap(m_BoostVector);
            m_Multiplier.CheckReference(map);
            base.CheckReferences(map);
        }
    }
}