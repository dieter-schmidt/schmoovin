#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Swimming/Swim Underwater (Stroke)", "SwimUnderwater")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-swimstrokeunderwaterstate.html")]
    public class SwimStrokeUnderwaterState : MotionGraphState
    {
        [SerializeField, Tooltip("The transform parameter which contains the transform of the water zone object")]
        private TransformParameter m_WaterZoneParameter = null;
        [SerializeField, Tooltip("A switch parameter which tracks if the crouch button is held (swim down)")]
        private SwitchParameter m_CrouchHold = null;
        [SerializeField, Tooltip("A switch parameter which tracks if the jump button is held (swim up)")]
        private SwitchParameter m_JumpHold = null;
        [SerializeField, Tooltip("The top movement speed (for keyboard input or max analog input)")]
        private FloatDataReference m_StrokeSpeed = new FloatDataReference(5f);
        [SerializeField, Tooltip("The maximum acceleration")]
        private FloatDataReference m_Acceleration = new FloatDataReference(25f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed and acceleration when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed and acceleration when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);
        [SerializeField, Tooltip("The multiplier applied to the acceleration when no input is detected")]
        private FloatDataReference m_IdleMultiplier = new FloatDataReference(0.5f);
        [SerializeField, Tooltip("The maximum movement speed and acceleration due to up or down input")]
        private FloatDataReference m_UpDownSpeed = new FloatDataReference(1f);
        [SerializeField, Range(0.25f, 3f), Tooltip("The length of time a swimming stroke lasts (unscaled).")]
        private float m_StrokeDuration = 0.25f;
        [SerializeField, Range(0.25f, 5f), Tooltip("The length of time in between swimming strkes (unscaled).")]
        private float m_RecoveryDuration = 0.75f;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the speed in between strokes.")]
        private float m_RecoverySpeedMultiplier = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the acceleration (and deceleration) in between strokes.")]
        private float m_RecoveryAccelerationMultiplier = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("At the minimum input amount, how much slower are strokes and recovery.")]
        private float m_SlowInputTimeScale = 0.25f;

        private const float k_TinyValue = 0.001f;

        private Transform m_WaterZoneTransform = null;
        private IWaterZone m_WaterZone = null;
        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;
        private Vector3 m_FlowAcceleration = Vector3.zero;
        private Vector3 m_FlowVelocity = Vector3.zero;
        private float m_StrokeTimer = 0f;

        public override Vector3 moveVector
        {
            get { return (m_OutVelocity + m_FlowVelocity) * Time.deltaTime; }
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
            m_StrokeSpeed.ClampValue(0.1f, 50f);
            m_UpDownSpeed.ClampValue(0.1f, 25f);
            m_StrafeMultiplier.ClampValue(0f, 2f);
            m_ReverseMultiplier.ClampValue(0f, 2f);
            m_IdleMultiplier.ClampValue(0f, 1f);
            m_Acceleration.ClampValue(0f, 100f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
            m_MotorAcceleration = Vector3.zero;
            m_OutVelocity = Vector3.zero;
            m_FlowAcceleration = Vector3.zero;
            m_FlowVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            // Get the water zone
            if (m_WaterZoneParameter != null)
            {
                if (m_WaterZoneTransform != m_WaterZoneParameter.value)
                {
                    m_WaterZoneTransform = m_WaterZoneParameter.value;
                    if (m_WaterZoneTransform != null)
                        m_WaterZone = m_WaterZoneTransform.GetComponent<IWaterZone>();
                }
            }

            // Get movement axes
            Vector3 up = characterController.up;
            Vector3 right = characterController.right;
            Vector3 forward = Quaternion.AngleAxis(-controller.aimController.pitch, right) * characterController.forward;

            // Get up / down input
            float upDown = 0f;
            if (m_JumpHold != null && m_JumpHold.on)
                upDown += 1f;
            if (m_CrouchHold != null && m_CrouchHold.on)
                upDown -= 1f;

            // Get the input direction multipliers
            bool idle = controller.inputMoveScale < 0.02f;
            float directionMultiplier = m_IdleMultiplier.value;
            if (!idle)
            {
                // Get the input vector
                Vector2 input = controller.inputMoveDirection;
                
                // Apply axis multipliers
                input.x *= m_StrafeMultiplier.value;
                if (input.y < 0f)
                    input.y *= m_ReverseMultiplier.value;

                // Direction multiplier is new magnitude
                directionMultiplier = input.magnitude;
            }

            // Get the stroke multipliers
            float strokeSpeedMultiplier = directionMultiplier;
            float strokeAccelMultiplier = 1f;
            if (m_StrokeTimer > 0f || !idle)// || upDown != 0f)
            {
                // Increment the stroke timer
                float timeLerp = controller.inputMoveScale * directionMultiplier;
                float timeMultiplier = Mathf.Lerp(m_SlowInputTimeScale, 1f, timeLerp);
                m_StrokeTimer += Time.deltaTime * timeMultiplier;

                // Check for stroke timer loop
                if (m_StrokeTimer > m_StrokeDuration + m_RecoveryDuration)
                {
                    if (idle && upDown == 0f)
                        m_StrokeTimer = 0f;
                    else
                        m_StrokeTimer -= m_StrokeDuration + m_RecoveryDuration;
                }
                
                // Reduce multiplier if in recovery
                if (m_StrokeTimer > m_StrokeDuration || (idle && m_StrokeTimer == 0f))
                {
                    strokeSpeedMultiplier *= m_RecoverySpeedMultiplier;
                    strokeAccelMultiplier *= m_RecoveryAccelerationMultiplier;
                }
            }

            // Get target input velocity
            Vector3 targetVelocity = Vector3.zero;
            if (!idle || upDown != 0f)
            {                
                // Get target velocity
                float topSpeed = m_StrokeSpeed.value * strokeSpeedMultiplier * controller.inputMoveScale;
                targetVelocity += forward * (controller.inputMoveDirection.y * topSpeed);
                targetVelocity += right * (controller.inputMoveDirection.x * topSpeed);
                targetVelocity += up * (upDown * m_UpDownSpeed.value);
            }
            
            // Accelerate if required
            float acceleration = m_Acceleration.value;
            if (acceleration < k_TinyValue)
                m_OutVelocity = targetVelocity;
            else
            {
                var currentVelocity = characterController.velocity - m_FlowVelocity;
                if (targetVelocity != currentVelocity)
                {
                    // Get maximum acceleration
                    float maxAccel = acceleration * strokeAccelMultiplier;
                    // Accelerate the velocity
                    m_OutVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref m_MotorAcceleration, 0.5f, maxAccel);
                }
            }

            // Add water flow
            if (m_WaterZone != null)
            {
                m_FlowVelocity = Vector3.SmoothDamp(
                    m_FlowVelocity,
                    m_WaterZone.FlowAtPosition(controller.localTransform.position + characterController.up * characterController.radius),
                    ref m_FlowAcceleration,
                    0.25f,
                    m_Acceleration.value
                    );
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WaterZoneParameter = map.Swap(m_WaterZoneParameter);
            m_CrouchHold = map.Swap(m_CrouchHold);
            m_JumpHold = map.Swap(m_JumpHold);
            m_StrokeSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);
            m_IdleMultiplier.CheckReference(map);
            m_UpDownSpeed.CheckReference(map);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_AccelerationKey = new NeoSerializationKey("acceleration");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_FlowAccelerationKey = new NeoSerializationKey("flowA");
        private static readonly NeoSerializationKey k_FlowVelocityKey = new NeoSerializationKey("flowV");
        private static readonly NeoSerializationKey k_StrokeTimerKey = new NeoSerializationKey("stroke");
        
        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_AccelerationKey, m_MotorAcceleration);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
            writer.WriteValue(k_FlowAccelerationKey, m_FlowAcceleration);
            writer.WriteValue(k_FlowVelocityKey, m_FlowVelocity);
            writer.WriteValue(k_StrokeTimerKey, m_StrokeTimer);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_AccelerationKey, out m_MotorAcceleration, m_MotorAcceleration);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_FlowAccelerationKey, out m_FlowAcceleration, m_FlowAcceleration);
            reader.TryReadValue(k_FlowVelocityKey, out m_FlowVelocity, m_FlowVelocity);
            reader.TryReadValue(k_StrokeTimerKey, out m_StrokeTimer, m_StrokeTimer);
        }

        #endregion
    }
}