﻿#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Dashes/Directional Dash", "Dash (Directional)")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-directionaldashstate.html")]
    public class DirectionalDashState : MotionGraphState
    {
        [SerializeField, Tooltip("The target speed for the dash to reach.")]
        private FloatDataReference m_DashSpeed = new FloatDataReference(50f);
        [SerializeField, Tooltip("The acceleration to reach the target dash speed.")]
        private FloatDataReference m_Acceleration = new FloatDataReference(250f);
        [SerializeField, Tooltip("The distance to dash before the state completes.")]
        private FloatDataReference m_DashDistance = new FloatDataReference(0f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when strafing")]
        private FloatDataReference m_StrafeMultiplier = new FloatDataReference(0.75f);
        [SerializeField, Tooltip("The multiplier applied to the max movement speed when moving in reverse")]
        private FloatDataReference m_ReverseMultiplier = new FloatDataReference(0.5f);

        private const float k_TinyValue = 0.001f;
        private const float k_MinInput = 0.05f;

        public enum DashDirection
        {
            YawRelative,
            MoveRelative
        }

        private Vector3 m_DashHeading = Vector3.zero;
        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;
        private float m_TotalDashDistance = 0f;
        private float m_TargetSpeed = 0f;
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
            get { return true; }
        }

        public override bool applyGroundingForce
        {
            get { return true; }
        }

        public override bool ignorePlatformMove
        {
            get { return false; }
        }

        public bool isInstant
        {
            get { return m_Acceleration.value == 0f; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_Completed = false;
            m_TotalDashDistance = 0f;

            // Get the dash heading
            m_DashHeading = Vector3.forward;
            if (controller.inputMoveScale > k_MinInput)
                m_DashHeading = new Vector3(controller.inputMoveDirection.x, 0f, controller.inputMoveDirection.y);
            m_DashHeading = characterController.transform.rotation * m_DashHeading;

            // Get the dash speed
            float directionMultiplier = 1f;
            if (controller.inputMoveDirection.y < 0f)
                directionMultiplier *= Mathf.Lerp(1f, m_ReverseMultiplier.value, -controller.inputMoveDirection.y);
            directionMultiplier *= Mathf.Lerp(1f, m_StrafeMultiplier.value, Mathf.Abs(controller.inputMoveDirection.x));
            m_TargetSpeed = m_DashSpeed.value * directionMultiplier;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
            m_TotalDashDistance = 0f;
            m_Completed = false;
        }

        public override void Update()
        {
            base.Update();

            Vector3 flatHeading = m_DashHeading;

            // Apply ground slope effect
            if (characterController.isGrounded)
            {
                // Get the offset impulse on the ground plane
                Plane p = new Plane(characterController.groundSurfaceNormal, 0f);
                float yOffset = 0f;
                if (p.Raycast(new Ray(flatHeading, characterController.up), out yOffset) && yOffset > k_TinyValue)
                {
                    // Apply the offset and clamp the speed to the original value
                    flatHeading += characterController.up * yOffset;
                    flatHeading.Normalize();
                }
            }

            // Accelerate to target velocity
            if (m_Acceleration.value > 0f)
            {
                m_OutVelocity = Vector3.SmoothDamp(
                    characterController.velocity,
                    flatHeading * m_TargetSpeed,
                    ref m_MotorAcceleration,
                    0.01f,
                    m_Acceleration.value
                    );

                m_TotalDashDistance += m_OutVelocity.magnitude * Time.deltaTime;
                if (m_TotalDashDistance > m_DashDistance.value)
                    m_Completed = true;
            }
            else
            {
                m_OutVelocity = flatHeading * m_TargetSpeed;
                m_Completed = true;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_DashSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
            m_DashDistance.CheckReference(map);
            m_StrafeMultiplier.CheckReference(map);
            m_ReverseMultiplier.CheckReference(map);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_AccelerationKey = new NeoSerializationKey("acceleration");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_DashHeadingKey = new NeoSerializationKey("heading");
        private static readonly NeoSerializationKey k_TotalDashDistanceKey = new NeoSerializationKey("distance");
        
        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_AccelerationKey, m_MotorAcceleration);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
            writer.WriteValue(k_DashHeadingKey, m_DashHeading);
            writer.WriteValue(k_TotalDashDistanceKey, m_TotalDashDistance);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_AccelerationKey, out m_MotorAcceleration, m_MotorAcceleration);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_DashHeadingKey, out m_DashHeading, m_DashHeading);
            reader.TryReadValue(k_TotalDashDistanceKey, out m_TotalDashDistance, m_TotalDashDistance);
        }

        #endregion
    }
}