#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Jump (Directional V2 Variable)", "Jump Directional Variable")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-jumpdirectionv2state.html")]
    public class JumpDirectionV2VariableState : MotionGraphState
    {
        [SerializeField, Tooltip("The charge property is a float with value 0 to 1 that defines how high to jump. If no graph property is selected, the jump will always be performed with full strength.")]
        private FloatParameter m_ChargeParameter = null;

        [SerializeField, Tooltip("The horizontal speed of the jump.")]
        private FloatDataReference m_MaxHorizontalSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The upward speed of the jump.")]
        private FloatDataReference m_MaxVerticalSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The amount of influence the ground has over the direction of the jump. 0 = up, 1 = ground normal.")]
        private FloatDataReference m_GroundInfluence = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("How should the velocity be applied. Additive will add the jump velocity to the original velocity. Absolute will ignore the original velocity. Minimum will boost the character velocity if it is less than the jump speed in the jump direction.")]
        private VelocityMode m_VelocityMode = VelocityMode.Minimum;

        [SerializeField, Tooltip("How the jump power is determined. Constant will apply max power.  Variable will scale.")]
        private JumpPowerMode m_JumpPowerMode = JumpPowerMode.Constant;

        private const float k_TinyValue = 0.001f;
        private float m_Charge = 1f;

        public enum VelocityMode
        {
            Additive,
            Absolute,
            Minimum
        }

        public enum JumpPowerMode
        {
            Constant,
            Variable
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
            //DS
            if (m_ChargeParameter != null)
            {
                m_Charge = m_ChargeParameter.value;
                m_ChargeParameter.value = 0f;
            }
            //DS
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
                Vector3 jumpVector;
                // Get the jump speed
                //float jumpSpeed = Mathf.Lerp(m_MinJumpSpeed, m_MaxJumpSpeed, m_Charge);
                if (m_JumpPowerMode == JumpPowerMode.Constant)
                {
                    //Debug.Log(Mathf.Lerp(0f, m_MaxHorizontalSpeed.value, m_Charge));
                    jumpVector = Vector3.up * m_MaxVerticalSpeed.value;
                    jumpVector += characterController.forward * controller.inputMoveDirection.y * m_MaxHorizontalSpeed.value;
                    jumpVector += characterController.right * controller.inputMoveDirection.x * m_MaxHorizontalSpeed.value;
                }
                else
                {
                    //Debug.Log(Mathf.Lerp(0f, m_MaxHorizontalSpeed.value, m_Charge));
                    //Debug.Log(Mathf.Lerp(0f, m_MaxVerticalSpeed.value, m_Charge));
                    jumpVector = Vector3.up * Mathf.Lerp(0f, m_MaxVerticalSpeed.value, m_Charge);//Mathf.Lerp(0f, m_MaxHorizontalSpeed.value, Mathf.Max(m_Charge, 0.5f));
                    jumpVector += characterController.forward * controller.inputMoveDirection.y * Mathf.Lerp(0f, m_MaxHorizontalSpeed.value, m_Charge);
                    jumpVector += characterController.right * controller.inputMoveDirection.x * Mathf.Lerp(0f, m_MaxHorizontalSpeed.value, m_Charge);
                }

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
            m_ChargeParameter = map.Swap(m_ChargeParameter);
            m_MaxHorizontalSpeed.CheckReference(map);
            m_MaxVerticalSpeed.CheckReference(map);
            m_GroundInfluence.CheckReference(map);
        }
    }
}