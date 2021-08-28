using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Impulse", "Impulse")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-impulsestate.html")]
    public class ImpulseState : MotionGraphState
    {
        [SerializeField, Tooltip("The velocity impulse to apply")]
        private VectorParameter m_Impulse = null;
        [SerializeField, Tooltip("The coordinate space to apply the impulse in.")]
        private Space m_FrameOfReference = Space.Self;
        [SerializeField, Tooltip("How should the impulse be applied. Additive will add the impulse velocity to the original velocity. ReplaceVelocity will ignore the original velocity and use the impulse alone.")]
        private ImpulseMode m_ImpulseMode = ImpulseMode.Additive;
        [SerializeField, Tooltip("If true, the impulse vector will be aligned onto the ground plane")]
        private bool m_GroundConstrained = false;

        private const float k_TinyValue = 0.001f;

        public enum ImpulseMode
        {
            Additive,
            ReplaceVelocity
        }
        
        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;

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
            get { return m_GroundConstrained; }
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
                if (m_Impulse != null)
                {
                    // Align the impulse vector to the character if required
                    Vector3 impulse = m_Impulse.value;
                    if (m_FrameOfReference == Space.Self)
                        impulse = controller.localTransform.rotation * impulse;

                    // Constrain the impulse vector to the ground surface if required
                    if (m_GroundConstrained && characterController.isGrounded)
                    {
                        // Get the offset impulse on the ground plane
                        Plane p = new Plane(characterController.groundSurfaceNormal, 0f);
                        float yOffset = 0f;
                        p.Raycast(new Ray(impulse, characterController.up), out yOffset);

                        // Check if an offset is required
                        if (yOffset > k_TinyValue)
                        {
                            // Apply the offset and clamp the speed to the original value
                            float impulseMag = impulse.magnitude;
                            impulse += characterController.up * yOffset;
                            impulse = Vector3.ClampMagnitude(impulse, impulseMag);
                        }
                    }

                    // Calculate the velocity
                    if (m_ImpulseMode == ImpulseMode.Additive)
                        m_OutVelocity = characterController.velocity + impulse;
                    else
                        m_OutVelocity = impulse;
                }
                else
                    m_OutVelocity = characterController.velocity;

                m_Completed = true;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Impulse = map.Swap(m_Impulse);
            base.CheckReferences(map);
        }
    }
}