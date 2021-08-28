#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;
using UnityEngine.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Ground Movement/Movement", "Movement")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-movementstate.html")]
    public class MovementState : MotionGraphState
    {
        [SerializeField, Tooltip("An optional curve that defines the speed drop off when moving up or down slopes. If this is not set then the character uses the default NeoCharacterController slope speed handling")]
        private SlopeSpeedCurve m_SlopeSpeedCurve = null;

        [SerializeField, Tooltip("The top movement speed (for keyboard input or max analog input)")]
        private FloatDataReference m_TopSpeed = new FloatDataReference(5f);

        [SerializeField, Tooltip("The multiplier applied to the max movement speed when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);

        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);

        [SerializeField, Tooltip("The maximum acceleration")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);

        [SerializeField, Tooltip("The maximum deceleration (when no input is applied)."), FormerlySerializedAs("m_Acceleration")]
        private FloatDataReference m_Deceleration = new FloatDataReference(50f);

        [SerializeField, Tooltip("How should gravity be applied. AlwaysApply gives a more realistic effect and allows for sliding due to ground and ledge friction, but affects slope speed. WhenNotGrounded and NeverApply give a more predictable result, but less realistic")]
        private GravityMode m_GravityMode = GravityMode.AlwaysApply;

        [SerializeField, Range(0f,1f), Tooltip("The amount of damping to apply when changing direction or speed")]
        private float m_Damping = 0.25f;

        private const float k_TinyValue = 0.001f;
        private const float k_MinSlopeEffectAngle = 3f;

        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;

        public enum GravityMode
        {
            AlwaysApply,
            WhenNotGrounded,
            NeverApply
        }

        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get
            {
                switch (m_GravityMode)
                {
                    case GravityMode.AlwaysApply:
                        return true;
                    case GravityMode.WhenNotGrounded:
                        return !characterController.isGrounded;
                    case GravityMode.NeverApply:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_TopSpeed.ClampValue(0.1f, 50f);
            m_StrafeMultiplier.ClampValue(0f, 2f);
            m_ReverseMultiplier.ClampValue(0f, 2f);
            m_Acceleration.ClampValue(0f, 1000f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_MotorAcceleration = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }

        protected virtual Vector3 GetTargetVelocity(float directionMultiplier)
        {
            if (controller.inputMoveScale < 0.01f)
                return Vector3.zero;
            
            // Get slope directions
            Vector3 forward = characterController.forward;
            Vector3 right = characterController.right;
            Vector3 target = Vector3.zero;

            // Different approaches if using a slope speed profile vs not
            bool useSlopeSpeedCurve = (m_SlopeSpeedCurve != null && characterController.isGrounded);
            if (useSlopeSpeedCurve)
            {
                Vector3 up = characterController.up;
                Vector3 slopeNormal = characterController.groundSurfaceNormal;

                // Create slope plane
                Plane p = new Plane(slopeNormal, 0f);

                // Fit move directions to plane
                float yOffset;
                p.Raycast(new Ray(forward, up), out yOffset);
                Vector3 slopeForward = (forward + up * yOffset).normalized;
                p.Raycast(new Ray(right, up), out yOffset);
                Vector3 slopeRight = (right + up * yOffset).normalized;

                // Get target velocity
                float topSpeed = m_TopSpeed.value * controller.inputMoveScale * directionMultiplier;
                target += slopeForward * controller.inputMoveDirection.y * topSpeed;
                target += slopeRight * controller.inputMoveDirection.x * topSpeed;

                // Get the slope angle
                float slopeAngle = Vector3.Angle(up, slopeNormal);
                if (slopeAngle > k_MinSlopeEffectAngle)
                {
                    // Get the up-slope vector
                    Vector3 upSlope = Vector3.ProjectOnPlane(up, slopeNormal).normalized;

                    // Check how much of the target move is up the slope
                    float dotUp = Vector3.Dot(target.normalized, upSlope);
                    if (dotUp < 0f)
                    {
                        slopeAngle *= -1f;
                        dotUp *= -1f;
                    }

                    // Use to get speed multiplier and apply to target
                    float slopeMultiplier = Mathf.Lerp(1f, m_SlopeSpeedCurve.curve.Evaluate(Mathf.Clamp(slopeAngle / characterController.slopeLimit, -1f, 1f)), dotUp);
                    target *= slopeMultiplier;
                }
            }
            else
            {
                // Get target velocity from input and direction
                float topSpeed = m_TopSpeed.value * controller.inputMoveScale * directionMultiplier;
                target += forward * controller.inputMoveDirection.y * topSpeed;
                target += right * controller.inputMoveDirection.x * topSpeed;
            }

            return target;
        }

        public override void Update()
        {
            base.Update();
            
            // Update the current velocity
            Vector3 currentVelocity = characterController.velocity;

            // Calculate speed based on move direction
            float directionMultiplier = 1f;
            if (controller.inputMoveScale > 0.005f)
            {
                Vector2 inputScaled = controller.inputMoveDirection;
                inputScaled.x *= m_StrafeMultiplier.value;
                if (inputScaled.y < 0f)
                    inputScaled.y *= m_ReverseMultiplier.value;
                directionMultiplier = inputScaled.magnitude;
            }

            // Get the target velocity
            var targetVelocity = GetTargetVelocity(directionMultiplier);

            // Check if accelerating or decelerating
            float acceleration = (targetVelocity.sqrMagnitude >= currentVelocity.sqrMagnitude) ? m_Acceleration.value : m_Deceleration.value;

            // Accelerate if required
            if (acceleration < k_TinyValue)
                m_OutVelocity = targetVelocity;
            else
            {
                // Get maximum acceleration
                float maxAccel = acceleration * directionMultiplier;
                // Accelerate the velocity
                m_OutVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref m_MotorAcceleration, Mathf.Lerp(0.05f, 0.25f, m_Damping), maxAccel);
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_TopSpeed.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);
            m_Acceleration.CheckReference(map);
            m_Deceleration.CheckReference(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_AccelerationKey = new NeoSerializationKey("acceleration");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_AccelerationKey, m_MotorAcceleration);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_AccelerationKey, out m_MotorAcceleration, m_MotorAcceleration);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
        }

        #endregion
    }
}