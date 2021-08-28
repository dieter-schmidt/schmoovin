using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Misc/Maintain Velocity", "MaintainVelocity")]
    public class MaintainVelocityState : MotionGraphState
    {
        [SerializeField, Tooltip("Should ground snapping be applied")]
        private bool m_GroundSnapping = false;
        [SerializeField, Tooltip("Should gravity force be applied")]
        private bool m_ApplyGravity = false;
        [SerializeField, Tooltip("Should the character inherit movement from platforms it's touching.")]
        private bool m_IgnorePlatforms = true;

        private Vector3 m_OutVelocity = Vector3.zero;
        
        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return m_ApplyGravity; }
        }

        public override bool applyGroundingForce
        {
            get { return m_GroundSnapping; }
        }

        public override bool ignorePlatformMove
        {
            get { return m_IgnorePlatforms; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
			m_OutVelocity = characterController.velocity;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }
    }
}