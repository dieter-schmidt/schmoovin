#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Jump (Directional V2)", "Jump Directional")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-jumpdirectionv2state.html")]
    public class JumpDirectionV2State : MotionGraphState
    {
        [SerializeField, Tooltip("The horizontal speed of the jump.")]
        private FloatDataReference m_HorizontalSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The upward speed of the jump.")]
        private FloatDataReference m_VerticalSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The amount of influence the ground has over the direction of the jump. 0 = up, 1 = ground normal.")]
        private FloatDataReference m_GroundInfluence = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("How should the velocity be applied. Additive will add the jump velocity to the original velocity. Absolute will ignore the original velocity. Minimum will boost the character velocity if it is less than the jump speed in the jump direction.")]
        private VelocityMode m_VelocityMode = VelocityMode.Minimum;

        private const float k_TinyValue = 0.001f;

        public enum VelocityMode
        {
            Additive,
            Absolute,
            Minimum
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
            get { return false; }
        }

        public override bool ignorePlatformMove
        {
            get { return true; }
        }
        
        public override void OnEnter()
        {
            m_Completed = false;
            base.OnEnter();
        }

        public override void OnExit()
        {
            m_Completed = false;
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();

            if (!m_Completed)
            {
                // Calculate jump vector
                Vector3 jumpVector = Vector3.up * m_VerticalSpeed.value;
                jumpVector += characterController.forward * controller.inputMoveDirection.y * m_HorizontalSpeed.value;
                jumpVector += characterController.right * controller.inputMoveDirection.x * m_HorizontalSpeed.value;

                // Tilt based on ground slope
                if (m_GroundInfluence.value > 0.001f)
                {
                    Quaternion tilt = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(characterController.up, characterController.groundNormal), m_GroundInfluence.value);
                    jumpVector = tilt * jumpVector;
                }

                // Apply the jump
                switch (m_VelocityMode)
                {
                    case VelocityMode.Absolute:
                        m_OutVelocity = jumpVector;
                        break;
                    case VelocityMode.Additive:
                        m_OutVelocity = characterController.velocity + jumpVector;
                        break;
                    case VelocityMode.Minimum:
                        {
                            // Get jump strength and direction
                            Vector3 up = characterController.up;
                            float jumpUp = Vector3.Dot(jumpVector, up);
                            Vector3 jumpHorizontal = jumpVector - up * jumpUp;
                            float jumpStrength = jumpHorizontal.magnitude;
                            jumpHorizontal /= jumpStrength;

                            // Get current speed along jump direction
                            float currentStrength = Vector3.Dot(characterController.velocity, jumpHorizontal);

                            // Get jump out vector
                            m_OutVelocity = characterController.velocity + up * jumpUp;

                            // Boost current up to jump
                            if (currentStrength < jumpStrength)
                            {
                                float boost = jumpStrength - currentStrength;
                                m_OutVelocity += jumpHorizontal * boost;
                            }
                        }
                        break;
                }

                m_Completed = true;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_HorizontalSpeed.CheckReference(map);
            m_VerticalSpeed.CheckReference(map);
            m_GroundInfluence.CheckReference(map);
        }
    }
}