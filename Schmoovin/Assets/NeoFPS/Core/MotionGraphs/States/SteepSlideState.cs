#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Ground Movement/Steep Slide", "Slide")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-steepslidestate.html")]
    public class SteepSlideState : MotionGraphState
    {
        [SerializeField, Tooltip("The angle above which a character loses motor control and is in pure slide mode.")]
        private FloatDataReference m_SlideAngle  = new FloatDataReference(60f);

        [SerializeField, Tooltip("The sliding speed the character will reach (downwards only) at the lowest slope angle for a full slide.")]
        private FloatDataReference m_SpeedMinimum = new FloatDataReference(25f);

        [SerializeField, Tooltip("The fastest possible sliding speed the character can reach (downwards only) during a near vertical slide.")]
        private FloatDataReference m_SpeedMaximum = new FloatDataReference(40f);

        [SerializeField, Tooltip("The down-slope acceleration multiplier applied to a character during a shallow slide.")]
        private FloatDataReference m_AccelerationMinimum = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("The down-slope acceleration multiplier applied to a character during a near vertical slide.")]
        private FloatDataReference m_AccelerationMaximum = new FloatDataReference(0.75f);

        [SerializeField, Tooltip("The top speed the character can reach against the slide (side to side).")]
        private FloatDataReference m_HorizontalSpeedLimit = new FloatDataReference(5f);

        [SerializeField, Tooltip("The across slope accleration when trying to redirect slide sideways (0 is instant).")]
        private FloatDataReference m_HorizontalAcceleration = new FloatDataReference(10f);

        private const float k_TinyValue = 0.001f;

        private Vector3 m_SlideVelocity = Vector3.zero;
        private float m_VerticalSlideSpeed = 0f;
        private float m_VerticalSlideAcceleration = 0f;
        private float m_HorizontalSlideSpeed = 0f;
        private float m_HorizontalSlideAcceleration = 0f;
        private float m_SlopeFriction = 0f;

        public override Vector3 moveVector
        {
            get { return m_SlideVelocity * Time.deltaTime; }
        }

        public override bool applyGravity
        {
            get { return false; }
        }

        public override bool applyGroundingForce
        {
            get { return true; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_SlideAngle.ClampValue(30f, 80f);
            m_SpeedMinimum.ClampValue(1f, 50f);
            m_SpeedMaximum.ClampValue(1f, 50f);
            m_AccelerationMinimum.ClampValue(0f, 1f);
            m_AccelerationMaximum.ClampValue(0f, 1f);
            m_HorizontalSpeedLimit.ClampValue(0f, 20f);
            m_HorizontalAcceleration.ClampValue(0f, 1000f);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_VerticalSlideAcceleration = 0f;
            m_HorizontalSlideAcceleration = 0f;

            Vector3 upSlope = Vector3.ProjectOnPlane(characterController.up, characterController.groundSurfaceNormal).normalized;
            m_VerticalSlideSpeed = Vector3.Dot(characterController.rawVelocity, upSlope);

            // Remove slope friction
            m_SlopeFriction = characterController.slopeFriction;
            characterController.slopeFriction = 0f;
        }

        public override void OnExit()
        {
            base.OnExit();

            m_SlideVelocity = Vector3.zero;

            // Reset slope friction
            characterController.slopeFriction = m_SlopeFriction;
        }

        public override void Update()
        {
            base.Update();

            // Get the up slope vector & angle
            Vector3 up = characterController.up;
            Vector3 upSlope = Vector3.ProjectOnPlane(up, characterController.groundSurfaceNormal).normalized;
            float slopeAngle = Vector3.Angle(up, characterController.groundSurfaceNormal);

            // Calculate the scale of the slide (0 to 1 = starting slide angle to straight down)
            float slopeMaxAngle = m_SlideAngle.value;
            float slideScale = Mathf.Clamp01 ((slopeAngle - slopeMaxAngle) / (90f - slopeMaxAngle));

            // Calculate target slide speed & acceleration based on slope
            float targetVerticalSlideSpeed = Mathf.Lerp(m_SpeedMinimum.value, m_SpeedMaximum.value, slideScale);
            float accelerationMultiplier = Mathf.Lerp(m_AccelerationMinimum.value, m_AccelerationMaximum.value, slideScale);

            // Update the current down-slope slide speed
            m_VerticalSlideSpeed = -Vector3.Dot (characterController.velocity, upSlope);
            // Prevent players using steep slopes to boost jumps (due to slower downward acceleration than gravity)
            if (m_VerticalSlideSpeed < 0f)
            {
                var upVel = Vector3.Project(characterController.velocity, up);
                m_VerticalSlideSpeed = -Vector3.Dot(upVel, upSlope);
                accelerationMultiplier += 1f;
            }

            // Accelerate down-slope towards slide speed
            if (!Mathf.Approximately(m_VerticalSlideSpeed, targetVerticalSlideSpeed))
                m_VerticalSlideSpeed = Mathf.SmoothDamp(m_VerticalSlideSpeed, targetVerticalSlideSpeed, ref m_VerticalSlideAcceleration, 0.01f, -Vector3.Dot(characterController.gravity, characterController.up) * accelerationMultiplier);
            m_VerticalSlideSpeed = Mathf.Clamp(m_VerticalSlideSpeed, -targetVerticalSlideSpeed, targetVerticalSlideSpeed);

            // Apply down-slope to move vector
            m_SlideVelocity = upSlope * -m_VerticalSlideSpeed;

            // Get the across slope vector
            Vector3 acrossSlope = Vector3.Cross(up, upSlope).normalized;

            // Check if across slope control is enabled
            float hSpeedLimit = m_HorizontalSpeedLimit.value;
            if (hSpeedLimit > 0f)
            {
                // Update the current across-slope slide speed
                m_HorizontalSlideSpeed = Vector3.Dot (characterController.velocity, acrossSlope);

                // Get the target across-slope speed (based on look & input)
                float targetHorizontalSlideSpeed = 0f;
                if (controller.inputMoveScale > 0f)
                {
                    Vector3 inputTarget = Vector3.zero;
                    inputTarget += controller.localTransform.forward * controller.inputMoveDirection.y;
                    inputTarget += controller.localTransform.right * controller.inputMoveDirection.x;

                    float acrossInput = Vector3.Dot(inputTarget, acrossSlope);
                    targetHorizontalSlideSpeed = acrossInput * hSpeedLimit;
                }

                // Accelerate across-slope towards target speed (positive = rhs)
                float hAcceleration = m_HorizontalAcceleration.value;
                if (hAcceleration < k_TinyValue)
                {
                    // Don't use acceleration (instant)
                    m_HorizontalSlideSpeed = targetHorizontalSlideSpeed;
                }
                else
                {
                    // Accelerate if required
                    if (!Mathf.Approximately (m_HorizontalSlideSpeed, targetHorizontalSlideSpeed))
                        m_HorizontalSlideSpeed = Mathf.SmoothDamp (m_HorizontalSlideSpeed, targetHorizontalSlideSpeed, ref m_HorizontalSlideAcceleration, 0.1f, hAcceleration);
                }

                // Apply to move vector
                m_SlideVelocity += acrossSlope * m_HorizontalSlideSpeed;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_SlideAngle.CheckReference(map);
            m_SpeedMinimum.CheckReference(map);
            m_SpeedMaximum.CheckReference(map);
            m_AccelerationMinimum.CheckReference(map);
            m_AccelerationMaximum.CheckReference(map);
            m_HorizontalSpeedLimit.CheckReference(map);
            m_HorizontalAcceleration.CheckReference(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_FrictionKey = new NeoSerializationKey("friction");
        private static readonly NeoSerializationKey k_SlideKey = new NeoSerializationKey("slide");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        
        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            
            writer.WriteValue(k_VelocityKey, m_SlideVelocity);
            writer.WriteValue(k_FrictionKey, m_SlopeFriction);
            writer.WriteValue(k_SlideKey, new Vector4(m_VerticalSlideSpeed, m_VerticalSlideAcceleration, m_HorizontalSlideSpeed, m_HorizontalSlideAcceleration));
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);

            reader.TryReadValue(k_VelocityKey, out m_SlideVelocity, m_SlideVelocity);
            reader.TryReadValue(k_FrictionKey, out m_SlopeFriction, m_SlopeFriction);

            Vector4 slide;
            if (reader.TryReadValue(k_SlideKey, out slide, Vector4.zero))
            {
                m_VerticalSlideSpeed = slide.x;
                m_VerticalSlideAcceleration = slide.y;
                m_HorizontalSlideSpeed = slide.z;
                m_HorizontalSlideAcceleration = slide.w;
            }
        }

        #endregion
    }
}