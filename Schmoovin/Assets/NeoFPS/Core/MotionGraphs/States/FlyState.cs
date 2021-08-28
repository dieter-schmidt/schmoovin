#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Airborne/Fly", "Fly")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-flystate.html")]
    public class FlyState : MotionGraphState
    {
        [SerializeField, Tooltip("The crouch hold parameter (used for flying down)")]
        private SwitchParameter m_CrouchHold = null;
        [SerializeField, Tooltip("The crouch hold parameter (used for flying up)")]
        private SwitchParameter m_JumpHold = null;
        [SerializeField, Tooltip("The top movement speed (for keyboard input or max analog input)")]
        private FloatDataReference m_TopSpeed = new FloatDataReference(5);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving up or down")]
        private FloatDataReference m_UpDownMultiplier = new FloatDataReference(0.25f);
        [SerializeField, Tooltip("The maximum acceleration")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);
        [SerializeField, Tooltip("How does the camera pitch affect the movement")]
        private PitchMode m_PitchMode = PitchMode.UpDownIgnores;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of damping to apply when changing direction or speed")]
        private float m_Damping = 0.25f;

        private const float k_TinyValue = 0.001f;

        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;

        public enum PitchMode
        {
            UpDownIgnores,
            AffectsAllAxes,
            Ignore
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
            m_TopSpeed.ClampValue(0.1f, 50f);
            m_StrafeMultiplier.ClampValue(0f, 2f);
            m_ReverseMultiplier.ClampValue(0f, 2f);
            m_UpDownMultiplier.ClampValue(0f, 2f);
            m_Acceleration.ClampValue(0f, 1000f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            // Get movement axes
            Vector3 up = characterController.up;
            Vector3 forward = characterController.forward;
            Vector3 right = characterController.right;
          
            // Modify axes based on pitch settings
            switch (m_PitchMode)
            {
                case PitchMode.UpDownIgnores:
                    forward = Quaternion.AngleAxis(-controller.aimController.pitch, right) * forward;
                    break;
                case PitchMode.AffectsAllAxes:
                    Quaternion pitchRotation = Quaternion.AngleAxis(-controller.aimController.pitch, right);
                    up = pitchRotation * up;
                    forward = pitchRotation * forward;
                    break;
            }
            
            // Calculate speed based on move direction
            float directionMultiplier = 1f;
            if (controller.inputMoveDirection.y < 0f)
                directionMultiplier *= Mathf.Lerp(1f, m_ReverseMultiplier.value, -controller.inputMoveDirection.y);
            directionMultiplier *= Mathf.Lerp(1f, m_StrafeMultiplier.value, Mathf.Abs(controller.inputMoveDirection.x));

            // Get target velocity
            float topSpeed = m_TopSpeed.value;
            Vector3 targetVelocity = Vector3.zero;
            targetVelocity += forward * controller.inputMoveDirection.y * topSpeed * directionMultiplier;
            targetVelocity += right * controller.inputMoveDirection.x * topSpeed * directionMultiplier;

            // Add up/down speed
            float upDown = 0f;
            if (m_JumpHold != null && m_JumpHold.on)
                upDown += 1f;
            if (m_CrouchHold != null && m_CrouchHold.on)
                upDown -= 1f;
            if (upDown != 0f)
                targetVelocity += up * upDown * m_UpDownMultiplier.value * m_TopSpeed.value;

            // Accelerate if required
            float acceleration = m_Acceleration.value;
            if (acceleration < k_TinyValue)
                m_OutVelocity = targetVelocity;
            else
            {
                var currentVelocity = characterController.velocity;
                if (targetVelocity != currentVelocity)
                {
                    // Get maximum acceleration
                    float maxAccel = acceleration * directionMultiplier;
                    // Accelerate the velocity
                    m_OutVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref m_MotorAcceleration, Mathf.Lerp(0.05f, 0.25f, m_Damping), maxAccel);
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_CrouchHold = map.Swap(m_CrouchHold);
            m_JumpHold = map.Swap(m_JumpHold);
            m_TopSpeed.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);
            m_UpDownMultiplier.CheckReference(map);
            m_Acceleration.CheckReference(map);
            base.CheckReferences(map);
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