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
    [MotionGraphElement("Ladders/Contact Ladder", "Ladder")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-contactladderstate.html")]
    public class ContactLadderState : MotionGraphState
    {
        [SerializeField, Tooltip("The transform property on the graph that is used to attach to ladder transforms in the scene.")]
        private TransformParameter m_TransformParameter = null;

        [SerializeField, Tooltip("The maximum climb speed (based on input and aim)")]
        private FloatDataReference m_ClimbSpeed = new FloatDataReference(2f);

        [SerializeField, Tooltip("The top movement speed with ground under foot (top and bottom of the ladder)")]
        private FloatDataReference m_GroundSpeed = new FloatDataReference(5f);

        [SerializeField, Tooltip("The multiplier applied to the max movement speed when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);

        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);

        [SerializeField, Tooltip("The acceleration while climbing the ladder or dismounting")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);

        [SerializeField, Tooltip("Should the direction of the camera aim influence the vertical move direction? IgnoreAimer = No; AimerAbsolute = Only direction; AimerSmooth = Direction and speed; AimerHeading = Direction and speed based on yaw (uses strafe); AimerAllAxes = Direction and speed based on yaw and pitch (uses strafe)")]
        private MovementStyle m_UseAimerV = MovementStyle.AimerAbsolute;

        [SerializeField, Tooltip("For AimerAbsolute, the angle past the horizontal you need to aim to flip directions. For AimerSmooth or AimerAllAxes, the angle past the horizontal that reaches full speed.")]
        private float m_CenterZone = 15f;

        [SerializeField, Tooltip("Should the aimer influence the horizontal move direction? IgnoreAimer = No; AimerAbsolute = Looking away switches axes; AimerSmooth = Direction and speed; AimerHeading/AimerAllAxes = As AimerSmooth, but forward / back input counts also.")]
        private MovementStyle m_UseAimerH = MovementStyle.AimerSmooth;

        // NOTES:
        // - Could also grab the jump trigger property and use it to correctly jump away from the ladder
        // - Could make single sided by setting complete / null as soon as the character passes behind the ladder
        // - This would mean 1 frame hitch of being in ladder state (just maintain velocity?)
        // - Could inherit from standard movement to for a better version of the grounded move

        private const float k_TinyValue = 0.001f;

        private ILadder m_Ladder = null;
        private Vector3 m_OutVelocity = Vector3.zero;
        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_RadiusOffset = Vector3.zero;
        private float m_Radius = 0f;
        private float m_QuarterCircleLength = 0f;
        private float m_SinDeadzone = 0f;
        private bool m_LookingUp = false;
        private bool m_WasUpLocked = false;

        public enum MovementStyle
        {
            IgnoreAimer,    // W/S = up/down OR A/D = left/right
            AimerAbsolute,  // Direction of aim changes direction of move
            AimerSmooth,    // Direction of aim changes direction and speed of move
            AimerHeading,   // Direction and speed based on yaw only (factors in both input axes)
            AimerAllAxes    // Direction and speed based on yaw and pitch (factors in both input axes)
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

            m_GroundSpeed.ClampValue(0.1f, 50f);
            m_StrafeMultiplier.ClampValue(0f, 2f);
            m_ReverseMultiplier.ClampValue(0f, 2f);
            m_Acceleration.ClampValue(0f, 1000f);

            if (m_UseAimerV == MovementStyle.AimerSmooth)
                m_CenterZone = Mathf.Clamp(m_CenterZone, 5f, 90f);
            else
                m_CenterZone = Mathf.Clamp(m_CenterZone, 0f, 60f);
        }
#endif
        
        public override void OnEnter()
        {
            base.OnEnter();

            m_MotorAcceleration = Vector3.zero;

            // Get ladder component from transform in property
            if (m_TransformParameter != null && m_TransformParameter.value != null)
                m_Ladder = m_TransformParameter.value.GetComponent<ILadder>();

            // Get character radius etc
            m_Radius = characterController.radius + 0.05f;
            m_RadiusOffset = new Vector3(0f, m_Radius, 0f);
            m_QuarterCircleLength = 0.5f * Mathf.PI * m_Radius;

            // Get relative position, etc
            Quaternion correction = Quaternion.Inverse((m_Ladder as MonoBehaviour).transform.rotation);
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
                //aimPitchRad -= angle * Mathf.Deg2Rad;

                // Check if the character is looking up the ladder
                Vector3 wrappedUp = Quaternion.AngleAxis(angle, m_Ladder.across) * m_Ladder.up;
                m_LookingUp = Vector3.Dot(controller.aimController.forward, wrappedUp) > 0f;
            }
            else
            {
                // Check if the character is looking up the ladder
                m_LookingUp = Vector3.Dot(controller.aimController.forward, m_Ladder.up) > 0f;
            }

            // Calculate the deadzone for aiming
            m_SinDeadzone = Mathf.Sin(m_CenterZone * Mathf.Deg2Rad);

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
            Vector3 aimForward = correction * controller.aimController.forward;
            Vector3 aimHeading = correction * controller.aimController.heading;

            // Check if at the top of the ladder and wrap around
            if (relativePosition.y > 0f)
            {
                // Get the angle from forwards (don't just use trig as the character is not guaranteed to be on the surface)
                float angle = Vector3.Angle(Vector3.forward, new Vector3(0f, relativePosition.y, relativePosition.z));
                if (angle > 90f)
                    angle = 90f;

                // Wrap vectors around 
                Quaternion wrapRotation = Quaternion.AngleAxis(angle, Vector3.right);
                aimForward = wrapRotation * aimForward;
                relativePosition.y = m_Radius * angle * Mathf.Deg2Rad;
            }

            // Get dot products of character aim against axes
            Vector3 aimDots = new Vector3(
                    Vector3.Dot(aimHeading, Vector3.right),
                    Vector3.Dot(aimForward, Vector3.up),
                    Vector3.Dot(aimHeading, Vector3.forward)
                );

            // Get vertical movement
            Vector3 targetVelocity = Vector3.zero;
            switch (m_UseAimerV)
            {
                case MovementStyle.IgnoreAimer:
                    {
                        targetVelocity.y += controller.inputMoveDirection.y * maxClimbSpeed;
                        break;
                    }
                case MovementStyle.AimerAbsolute:
                    {
                        // Check if looking up (with deadzone)
                        if (m_CenterZone <= Mathf.Epsilon)
                        {
                            // No deadzone
                            m_LookingUp = aimDots.y >= 0f;
                        }
                        else
                        {
                            // Only flip if outside deadzone
                            if (m_LookingUp && aimDots.y < -m_SinDeadzone)
                                m_LookingUp = false;
                            if (!m_LookingUp && aimDots.y > m_SinDeadzone)
                                m_LookingUp = true;
                        }

                        // Get the climb speed
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed;
                        if (!m_LookingUp)
                            speed *= -1f;
                        targetVelocity.y += speed;

                        break;
                    }
                case MovementStyle.AimerSmooth:
                    {
                        // Get the aim speed based on center zone
                        float aimAngle = Mathf.Asin(aimDots.y) * Mathf.Rad2Deg;
                        float aimSpeed = Mathf.Clamp(aimAngle / m_CenterZone, -1f, 1f);

                        // Get the climb speed
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed * aimSpeed;
                        targetVelocity.y += speed;

                        break;
                    }
                case MovementStyle.AimerHeading:
                    {
                        // Get the climb speed (W/S)
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed * Mathf.Abs(aimDots.z);
                        
                        // Get the climb speed (A/D)
                        speed += controller.inputMoveDirection.x * maxClimbSpeed * aimDots.x;

                        targetVelocity.y += speed;

                        break;
                    }
                case MovementStyle.AimerAllAxes:
                    {
                        // Get the aim speed based on center zone
                        float aimAngle = Mathf.Asin(aimDots.y) * Mathf.Rad2Deg;
                        float aimSpeed = Mathf.Clamp(aimAngle / m_CenterZone, -1f, 1f);

                        // Get the climb speed (W/S)
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed * Mathf.Abs(aimDots.z) * aimSpeed;

                        // Get the climb speed (A/D)
                        speed += controller.inputMoveDirection.x * maxClimbSpeed * aimDots.x * aimSpeed;

                        targetVelocity.y += speed;

                        break;
                    }
            }

            // Check if at the base of the ladder and moving down, and "walk" as normal if so.
            if (relativePosition.y < 0f && targetVelocity.y < 0f && IsCharacterGrounded())
            {
                SimpleBaseWalk();
                return;
            }

            // Get lateral movement
            switch (m_UseAimerH)
            {
                case MovementStyle.IgnoreAimer:
                    {
                        targetVelocity.x -= controller.inputMoveDirection.x * maxClimbSpeed * m_StrafeMultiplier.value;
                        break;
                    }
                case MovementStyle.AimerAbsolute:
                    {
                        // Get the climb speed
                        float speed = controller.inputMoveDirection.x * maxClimbSpeed;
                        if (aimDots.z < 0f)
                            speed *= -1f;
                        targetVelocity.x += speed;

                        break;
                    }
                case MovementStyle.AimerSmooth:
                    {
                        // Get the climb speed
                        float speed = controller.inputMoveDirection.x * maxClimbSpeed * aimDots.z;
                        targetVelocity.x += speed;

                        break;
                    }
                case MovementStyle.AimerHeading:
                    {
                        // Get the climb speed (W/S)
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed * aimDots.x;

                        // Get the climb speed (A/D)
                        speed += controller.inputMoveDirection.x * maxClimbSpeed * aimDots.z;

                        targetVelocity.x += speed;

                        break;
                    }
                case MovementStyle.AimerAllAxes:
                    {
                        // Get the climb speed (W/S)
                        float speed = controller.inputMoveDirection.y * maxClimbSpeed * aimDots.x;

                        // Get the climb speed (A/D)
                        speed += controller.inputMoveDirection.x * maxClimbSpeed * aimDots.z;

                        // Scale based on aim
                        speed *= 1f - Mathf.Abs(aimDots.y) * 0.75f;

                        targetVelocity.x += speed;

                        break;
                    }
            }

            // Get the target world position
            relativePosition += targetVelocity * Time.deltaTime;

            // Check if off top and detach if so
            if (relativePosition.y > m_QuarterCircleLength && targetVelocity.y > 0.25f * maxClimbSpeed)
            {
                m_TransformParameter.value = null;
            }

            if (relativePosition.y > 0f)
            {
                // Get overrun from top of curve
                float remainder = relativePosition.y - m_QuarterCircleLength;

                // Get angle
                float angle = relativePosition.y / (Mathf.Deg2Rad * m_Radius);
                angle = Mathf.Clamp(angle, 0f, 90f);
                angle *= Mathf.Deg2Rad;

                // relativePosition = Quaternion.AngleAxis(angle, Vector3.left) * relativePosition;
                relativePosition.y = Mathf.Sin(angle) * m_Radius;
                // Add overrun back on
                if (remainder > 0f)
                    relativePosition.z = -remainder;
                else
                    relativePosition.z = Mathf.Cos(angle) * m_Radius;
            }
            else
            {
                relativePosition.z = m_Radius;
            }
            Vector3 targetWorldPosition = m_Ladder.worldTop + (rotation * relativePosition);

            Vector3 targetWorldVelocity = Vector3.ClampMagnitude((targetWorldPosition - (controller.localTransform.position + m_RadiusOffset)) / Time.deltaTime, maxClimbSpeed);

            float accel = m_Acceleration.value;
            if (accel < k_TinyValue)
                m_OutVelocity = targetWorldVelocity;
            else
                m_OutVelocity = Vector3.SmoothDamp(characterController.velocity, targetWorldVelocity, ref m_MotorAcceleration, 0.01f, accel);

            float mag = m_OutVelocity.magnitude;
            if (mag > maxClimbSpeed)
            {
                float ratio = maxClimbSpeed / mag;
                m_OutVelocity *= ratio;
            }
        }

        private void SimpleBaseWalk()
        {
            //m_TransformParameter.value = null;
            //return;

            // Update the current velocity
            Vector3 currentVelocity = characterController.velocity;
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, characterController.up);

            // Calculate speed based on move direction
            float directionMultiplier = 1f;
            if (controller.inputMoveDirection.y < 0f)
                directionMultiplier *= Mathf.Lerp(1f, m_ReverseMultiplier.value, -controller.inputMoveDirection.y);
            directionMultiplier *= Mathf.Lerp(1f, m_StrafeMultiplier.value, Mathf.Abs(controller.inputMoveDirection.x));

            // Get target velocity
            float groundSpeed = m_GroundSpeed.value;
            Vector3 targetVelocity = Vector3.zero;
            targetVelocity += controller.localTransform.forward * controller.inputMoveDirection.y * groundSpeed * directionMultiplier;
            targetVelocity += controller.localTransform.right * controller.inputMoveDirection.x * groundSpeed * directionMultiplier;
            
            // Accelerate if required
            if (targetVelocity != currentVelocity)
            {
                // Get maximum acceleration
                float maxAccel = m_Acceleration.value * directionMultiplier;
                // Accelerate the velocity
                m_OutVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref m_MotorAcceleration, 0.01f, maxAccel);
            }

            // Groundiness
            if (characterController.velocity.y < m_OutVelocity.y)// 0f)
                m_OutVelocity.y = characterController.velocity.y;
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
            m_GroundSpeed.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);
            m_Acceleration.CheckReference(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_LookingUpKey = new NeoSerializationKey("lookingUp");
        private static readonly NeoSerializationKey k_WasUpLockedKey = new NeoSerializationKey("wasUpLocked");
        private static readonly NeoSerializationKey k_AccelerationKey = new NeoSerializationKey("acceleration");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);

            writer.WriteValue(k_LookingUpKey, m_LookingUp);
            writer.WriteValue(k_WasUpLockedKey, m_WasUpLocked);
            writer.WriteValue(k_AccelerationKey, m_MotorAcceleration);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            Debug.Log("base.ReadProperties");

            base.ReadProperties(reader);

            Debug.Log("ReadProperties internal");

            reader.TryReadValue(k_LookingUpKey, out m_LookingUp, m_LookingUp);
            reader.TryReadValue(k_WasUpLockedKey, out m_WasUpLocked, m_WasUpLocked);
            reader.TryReadValue(k_AccelerationKey, out m_MotorAcceleration, m_MotorAcceleration);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);

            Debug.Log("ReadProperties done");
        }

        #endregion
    }
}