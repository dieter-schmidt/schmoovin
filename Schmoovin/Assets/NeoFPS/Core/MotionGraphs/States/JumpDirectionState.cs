#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Jump (Directional)", "Jump Directional")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-jumpdirectionstate.html")]
    public class JumpDirectionState : MotionGraphState
    {
        [SerializeField, Tooltip("The instant speed of the jump.")]
        private FloatDataReference m_JumpSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The angle the jump direction will be tilted in the character input direction when input scale is 1.")]
        private FloatDataReference m_MaxJumpAngle = new FloatDataReference(60f);

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
                // Get the direction vector based on jump ground influence
                Vector3 direction = Vector3.Slerp(characterController.up, characterController.groundNormal, m_GroundInfluence.value);

                // Rotate the direction based on input
                if (controller.inputMoveScale > 0.001f)
                {
                    // Get the rotation axis based on input
                    var axis = new Vector3(controller.inputMoveDirection.y, 0f, -controller.inputMoveDirection.x);
                    // Rotate from character local space to world
                    axis = controller.localTransform.rotation * axis;
                    // Get rotation from axis and input scale
                    var inputRotation = Quaternion.AngleAxis(m_MaxJumpAngle.value * controller.inputMoveScale, axis);
                    // Apply rotation to jump direction
                    direction = inputRotation * direction;
                }
                
                // Apply the jump
                switch (m_VelocityMode)
                {
                    case VelocityMode.Absolute:
                        m_OutVelocity = direction * m_JumpSpeed.value;
                        break;
                    case VelocityMode.Additive:
                        m_OutVelocity = characterController.velocity + direction * m_JumpSpeed.value;
                        break;
                    case VelocityMode.Minimum:
                        {
                            m_OutVelocity = characterController.velocity;
                            Vector3 jumpVelocity = direction * m_JumpSpeed.value;

                            if (direction.x > k_TinyValue)
                            {
                                if (m_OutVelocity.x < jumpVelocity.x)
                                    m_OutVelocity.x = jumpVelocity.x;
                            }
                            else if (direction.x < -k_TinyValue)
                            {
                                if (m_OutVelocity.x > jumpVelocity.x)
                                    m_OutVelocity.x = jumpVelocity.x;
                            }

                            if (direction.y > k_TinyValue)
                            {
                                if (m_OutVelocity.y < jumpVelocity.y)
                                    m_OutVelocity.y = jumpVelocity.y;
                            }
                            else if (direction.y < -k_TinyValue)
                            {
                                if (m_OutVelocity.y > jumpVelocity.y)
                                    m_OutVelocity.y = jumpVelocity.y;
                            }

                            if (direction.z > k_TinyValue)
                            {
                                if (m_OutVelocity.z < jumpVelocity.z)
                                    m_OutVelocity.z = jumpVelocity.z;
                            }
                            else if (direction.z < -k_TinyValue)
                            {
                                if (m_OutVelocity.z > jumpVelocity.z)
                                    m_OutVelocity.z = jumpVelocity.z;
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
            m_JumpSpeed.CheckReference(map);
            m_MaxJumpAngle.CheckReference(map);
            m_GroundInfluence.CheckReference(map);
        }
    }
}