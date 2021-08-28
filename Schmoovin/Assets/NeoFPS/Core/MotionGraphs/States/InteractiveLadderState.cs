#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;
using NeoCC;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Ladders/Interactive Ladder", "Ladder")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-interactiveladderstate.html")]
    public class InteractiveLadderState : MotionGraphState
    {
        [SerializeField, Tooltip("The transform parameter on the graph that is used to attach to ladder transforms in the scene.")]
        private TransformParameter m_TransformParameter = null;

        [SerializeField, Tooltip("The maximum climb speed (based on input and aim)")]
        private FloatDataReference m_ClimbSpeed = new FloatDataReference(2f);

        [SerializeField, Tooltip("The acceleration when on the ladder or attching / dismounting.")]
        private FloatDataReference m_Acceleration = new FloatDataReference(100f);

        [SerializeField, Tooltip("Should the aimer influence the vertical move direction? IgnoreAimer = No; AimerUpDown = Only direction; AimerSmooth = Direction and speed.")]
        private VerticalMode m_UseAimerV = VerticalMode.AimerUpDown;

        [SerializeField, Tooltip("For AimerUpDown, the angle past the horizontal you need to aim to flip directions. For AimerSmooth, the angle past the horizontal that reaches full speed.")]
        private float m_CenterZone = 15f;

        [SerializeField, Range(0f, 4f), Tooltip("How long is the character blocked from stepping straight off the ladder using up/down? For example, if they attach to the top of the ladder then they should not immediately step off by pressing up.")]
        private float m_DismountDelay = 1f;

        [SerializeField, Range(0.5f, 10f), Tooltip("If true, constrain the camera to a maximum range from looking directly at the ladder")]
        private bool m_ConstrainCamera = true;

        [SerializeField, Range(0f, 270f), Tooltip("The angle range (degrees) that the camera can turn from looking straight at the ladder (180 = 90 degrees to either side).")]
        private float m_LookRange = 180f;

        private ILadder m_Ladder = null;
        private Vector3 m_OutVelocity = Vector3.zero;
        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_RadiusOffset = Vector3.zero;
        private float m_Radius = 0f;
        private float m_QuarterCircleLength = 0f;
        private float m_DismountTimer = 0f;
        private CharacterState m_CharacterState = CharacterState.Climbing;
        private bool m_LookingUp = false;
        private float m_SinDeadzone = 0f;
        private bool m_WasUpLocked = false;

        public enum CharacterState
        {
            Climbing,
            MountingTop,
            Dismounting
        }

        public enum VerticalMode
        {
            IgnoreAimer,
            AimerUpDown,
            AimerSmooth
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

        public override bool ignoreExternalForces
        {
            get { return false; } // Simple ladder does not lock you in place
        }

        public override bool completed
        {
            get { return m_Ladder == null || m_TransformParameter.value == null; }
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();

            m_ClimbSpeed.ClampValue(0.1f, 50f);
            m_Acceleration.ClampValue(1f, 1000f);

            if (m_Ladder != null)
            {
                if (m_ConstrainCamera)
                    controller.aimController.SetYawConstraints(-m_Ladder.forward, m_LookRange);
                else
                    controller.aimController.ResetYawConstraints();
            }
        }
#endif

        public override void OnEnter()
        {
            base.OnEnter();

            m_MotorAcceleration = Vector3.zero;

            // Get ladder component from transform in property
            if (m_TransformParameter != null && m_TransformParameter.value != null)
                m_Ladder = m_TransformParameter.value.GetComponent<ILadder>();
            if (m_Ladder == null)
                return;

            m_Radius = characterController.radius + 0.05f;
            m_RadiusOffset = new Vector3(0f, m_Radius, 0f);
            m_QuarterCircleLength = 0.5f * Mathf.PI * m_Radius;

            // Get relative position, etc
            Quaternion correction = Quaternion.Inverse(m_Ladder.localTransform.rotation);
            Vector3 relativePosition = correction * (controller.localTransform.position + m_RadiusOffset - m_Ladder.worldTop);
            //float aimPitchRad = (correction * controller.aimController.rotation).eulerAngles.x * Mathf.Deg2Rad;

            // Check if at top of ladder
            if (relativePosition.y > 0f)
            {
                // Get the angle from forwards (don't just use trig as the character is not guaranteed to be on the surface)
                float angle = Vector3.Angle(Vector3.forward, new Vector3(0f, relativePosition.y, relativePosition.z));

                // Wrap the position
                relativePosition.y = m_Radius * angle * Mathf.Deg2Rad;

                // Set to mount and block dismount until complete
                m_CharacterState = CharacterState.MountingTop;
                m_DismountTimer = m_DismountDelay;

                // Check if the character is looking up the ladder
                Vector3 wrappedUp = Quaternion.AngleAxis(angle, m_Ladder.across) * m_Ladder.up;
                m_LookingUp = Vector3.Dot(controller.aimController.forward, wrappedUp) > 0f;
            }
            else
            {
                // Climbing
                m_CharacterState = CharacterState.Climbing;

                // Check if bottom of ladder and prevent accidental dismount
                if (IsCharacterGrounded())
                    m_DismountTimer = m_DismountDelay;
                else
                    m_DismountTimer = 0f;

                // Check if the character is looking up the ladder
                m_LookingUp = Vector3.Dot(controller.aimController.forward, m_Ladder.up) > 0f;
            }

            // Set if looking up ladder (for deadzone)
            m_SinDeadzone = Mathf.Sin(m_CenterZone * Mathf.Deg2Rad);

            // Set yaw constraints
            if (m_ConstrainCamera)
                controller.aimController.SetYawConstraints(-m_Ladder.forward, m_LookRange);

            // Make sure the character doesn't rotate on the ladder
            var variableUp = characterController as INeoCharacterVariableGravity;
            if (variableUp != null)
            {
                m_WasUpLocked = variableUp.lockUpVector;
                variableUp.lockUpVector = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
            m_Ladder = null;
            
            // Reset camera constraints
            if (m_ConstrainCamera)
                controller.aimController.ResetYawConstraints();

            // Allow the character to change up vector again
            var variableUp = characterController as INeoCharacterVariableGravity;
            if (variableUp != null)
                variableUp.lockUpVector = m_WasUpLocked;
        }

        public override void Update()
        {
            base.Update();

            if (completed)
                return;

            float maxClimbSpeed = m_ClimbSpeed.value;

            // Get the ladder rotations
            Quaternion rotation = m_Ladder.localTransform.rotation;
            Quaternion correction = Quaternion.Inverse(rotation);

            // Get the character details relative to the ladder
            Vector3 relativePosition = correction * (controller.localTransform.position + m_RadiusOffset - m_Ladder.worldTop);
            Vector3 ladderUp = m_Ladder.up;

            relativePosition.x = 0f;
            if (relativePosition.y > 0f)
            {
                // Get the angle from forwards (don't just use trig as the character is not guaranteed to be on the surface)
                float angle = Vector3.Angle(Vector3.forward, relativePosition.normalized);
                if (angle > 90f)
                    angle = 90f;

                // Wrap vectors around 
                relativePosition.y = m_Radius * angle * Mathf.Deg2Rad;
                ladderUp = Quaternion.AngleAxis(angle, m_Ladder.across) * ladderUp;
            }
            
            // Handle movement in "ladder space"
            switch (m_CharacterState)
            {
                case CharacterState.Dismounting:
                    {
                        // Move up
                        relativePosition.y += maxClimbSpeed * Time.deltaTime;

                        // Check if dismount complete
                        if (relativePosition.y > m_QuarterCircleLength)
                            m_TransformParameter.value = null;
                    }
                    break;
                case CharacterState.MountingTop:
                    {
                        // Move down
                        relativePosition.y -= maxClimbSpeed * Time.deltaTime;

                        // If now on main ladder section, switch state
                        if (relativePosition.y < -0.05f)
                            m_CharacterState = CharacterState.Climbing;
                    }
                    break;
                case CharacterState.Climbing:
                    {
                        // Get input up / down
                        float verticalInput = 0f;
                        switch (m_UseAimerV)
                        {
                            case VerticalMode.IgnoreAimer:
                                {
                                    verticalInput = controller.inputMoveDirection.y;
                                    break;
                                }
                            case VerticalMode.AimerUpDown:
                                {
                                    float aimUp = Vector3.Dot(controller.aimController.forward, ladderUp);
                                    // Check if looking up (with deadzone)
                                    if (m_CenterZone <= Mathf.Epsilon)
                                    {
                                        // No deadzone
                                        m_LookingUp = aimUp >= 0f;
                                    }
                                    else
                                    {
                                        // Only flip if outside deadzone
                                        if (m_LookingUp && aimUp < -m_SinDeadzone)
                                            m_LookingUp = false;
                                        if (!m_LookingUp && aimUp > m_SinDeadzone)
                                            m_LookingUp = true;
                                    }

                                    verticalInput = controller.inputMoveDirection.y;
                                    if (!m_LookingUp)
                                        verticalInput *= -1f;

                                    break;
                                }
                            case VerticalMode.AimerSmooth:
                                {
                                    // Get the aim speed based on center zone
                                    float aimUp = Vector3.Dot(controller.aimController.forward, ladderUp);
                                    float aimAngle = Mathf.Asin(aimUp) * Mathf.Rad2Deg;
                                    float aimSpeed = Mathf.Clamp(aimAngle / m_CenterZone, -1f, 1f);

                                    verticalInput = controller.inputMoveDirection.y * aimSpeed;
                                    break;
                                }
                        }

                        // Check if at bottom of ladder and trying to step down
                        if (IsCharacterGrounded() && verticalInput <= 0f)
                        {
                            if (m_DismountTimer <= 0f)
                            {
                                m_TransformParameter.value = null;
                            }
                            break;
                        }

                        // Move up / down
                        relativePosition.y += maxClimbSpeed * verticalInput * Time.deltaTime;

                        // Check if at top of ladder and trying to dismount
                        if (relativePosition.y > Mathf.Epsilon)
                        {
                            if (m_DismountTimer <= 0f)
                            {
                                m_CharacterState = CharacterState.Dismounting;
                                break;
                            }
                            else
                                relativePosition.y = 0f;
                        }

                        // If relative position < bottom, detach
                        if (relativePosition.y < -m_Ladder.length)
                        {
                            m_TransformParameter.value = null;
                            break;
                        }

                        // Decrement dismount delay if required
                        if (m_DismountTimer > 0f)
                            m_DismountTimer -= Time.deltaTime;

                        break;
                    }
            }

            if (relativePosition.y > 0f)
            {
                // Get overrun from top of curve
                float remainder = relativePosition.y - m_QuarterCircleLength;

                // Get angle
                float angle = relativePosition.y / (Mathf.Deg2Rad * m_Radius);
                angle = Mathf.Clamp(angle, 0f, 90f);
                angle *= Mathf.Deg2Rad;

                relativePosition.y = Mathf.Sin(angle) * m_Radius;
                // Add overrun back on
                if (remainder > 0f)
                    relativePosition.z = -remainder;
                else
                    relativePosition.z = Mathf.Cos(angle) * m_Radius;
            }
            else
                relativePosition.z = m_Radius;
            Vector3 targetWorldPosition = m_Ladder.worldTop + (rotation * relativePosition);

            // Get out velocity
            Vector3 targetWorldVelocity = Vector3.ClampMagnitude((targetWorldPosition - (controller.localTransform.position + m_RadiusOffset)) / Time.deltaTime, maxClimbSpeed);
            m_OutVelocity = Vector3.SmoothDamp(characterController.velocity, targetWorldVelocity, ref m_MotorAcceleration, 0.01f, m_Acceleration.value);
        }

        private bool IsCharacterGrounded()
        {
            return characterController.isGrounded && Vector3.Angle(characterController.up, characterController.groundNormal) < characterController.slopeLimit;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_TransformParameter = map.Swap(m_TransformParameter);
            m_ClimbSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_LookingUpKey = new NeoSerializationKey("lookingUp");
        private static readonly NeoSerializationKey k_WasUpLockedKey = new NeoSerializationKey("wasUpLocked");
        private static readonly NeoSerializationKey k_AccelerationKey = new NeoSerializationKey("acceleration");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_DismountTimerKey = new NeoSerializationKey("dismountTimer");
        private static readonly NeoSerializationKey k_CharStateKey = new NeoSerializationKey("charState");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);

            writer.WriteValue(k_LookingUpKey, m_LookingUp);
            writer.WriteValue(k_WasUpLockedKey, m_WasUpLocked);
            writer.WriteValue(k_AccelerationKey, m_MotorAcceleration);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
            writer.WriteValue(k_DismountTimerKey, m_DismountTimer);
            writer.WriteValue(k_CharStateKey, (int)m_CharacterState);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);

            reader.TryReadValue(k_LookingUpKey, out m_LookingUp, m_LookingUp);
            reader.TryReadValue(k_WasUpLockedKey, out m_WasUpLocked, m_WasUpLocked);
            reader.TryReadValue(k_AccelerationKey, out m_MotorAcceleration, m_MotorAcceleration);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_DismountTimerKey, out m_DismountTimer, m_DismountTimer);

            int stateIndex = 0;
            reader.TryReadValue(k_CharStateKey, out stateIndex, (int)m_CharacterState);
            m_CharacterState = (CharacterState)stateIndex;
        }

        #endregion
    }
}