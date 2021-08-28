#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Dashes/Anim-Curve Wall Dash", "Wall Dash (Anim Curve)")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-animcurvewalldashstate.html")]
    public class AnimCurveWallDashState : MotionGraphState
    {
        [SerializeField, Tooltip("The target speed for the dash to reach. This will be layered on top of the control speed.")]
        private FloatDataReference m_DashSpeed = new FloatDataReference(50f);
        [SerializeField, Tooltip("The wall normal parameter, as used by the wall run states.")]
        private VectorParameter m_WallNormal = null;
        [SerializeField, Tooltip("The amount of time it takes to reach the dash speed. At this point, the animation curve kicks in to ease out of the dash. A Dash In Time of 0 is instant.")]
        private float m_DashInTime = 0.05f;
        [SerializeField, Tooltip("The amount of time it takes to reach the dash speed. At this point, the animation curve kicks in to ease out of the dash. A Dash In Time of 0 is instant.")]
        private float m_DashOutTime = 0.5f;
        [SerializeField, Tooltip("The angle offset for the dash direction. For example, yaw relative and an angle of 90 will dash to the right. -90 will dash to the left.")]
        private AnimationCurve m_DashOutCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0f, 1f), new Keyframe(0.1f, 1f), new Keyframe(0.7f, -0.05f), new Keyframe(0.9f, 0.02f), new Keyframe(1f, 0f)
        });
        [SerializeField, Tooltip("Should the yaw direction of the character be turned if the character must change directions to curve with the wall. It's easy to become disoriented with this disabled.")]
        private bool m_YawWithCurve = true;

        private const float k_TinyValue = 0.001f;

        private Vector3 m_DashHeading = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;
        private float m_LerpIn = 0f;
        private float m_LerpOut = 0f;
        private float m_EntrySpeed = 0f;
        private bool m_DashForwards = true;
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
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            var keys = m_DashOutCurve.keys;
            if (keys.Length < 2)
            {
                var newKeys = new Keyframe[2];
                if (keys.Length == 1)
                    newKeys[0] = keys[0];
                else
                    newKeys[0] = new Keyframe(0f, 1f);
                newKeys[1] = new Keyframe(1f, 0f);
                m_DashOutCurve.keys = newKeys;
            }
            else
            {
                keys[0].time = 0f;
                keys[keys.Length - 1].time = 1f;
            }

            m_DashInTime = Mathf.Clamp(m_DashInTime, 0f, 10f);
            m_DashOutTime = Mathf.Clamp(m_DashOutTime, 0.05f, 10f);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_LerpOut = 0f;
            m_Completed = false;
            m_DashForwards = true;

            // Reset lerp in (skip if lerp takes less than 1 frame)
            m_LerpIn = 0f;

            // Get heading
            m_DashHeading = characterController.velocity.normalized;

            // Get velocity data
            m_OutVelocity = Vector3.ProjectOnPlane(characterController.velocity, characterController.up);
            if (m_WallNormal != null)
                m_OutVelocity = Vector3.ProjectOnPlane(m_OutVelocity, m_WallNormal.value);
            m_DashHeading = m_OutVelocity.normalized;
            m_EntrySpeed = m_OutVelocity.magnitude;
        }

        public override void OnExit()
        {
            base.OnExit();

            m_Completed = false;
            m_DashHeading = Vector3.zero;
            m_OutVelocity = Vector3.zero;
            m_LerpOut = 0f;
        }

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            m_DashOutCurve.preWrapMode = WrapMode.ClampForever;
            m_DashOutCurve.postWrapMode = WrapMode.ClampForever;
        }

        public override void Update()
        {
            base.Update();

            var up = characterController.up;

            //Sort dash speed
            float dashSpeed = 0f;
            if (m_LerpIn < 1f)
            {
                if (m_DashInTime < Time.fixedDeltaTime)
                    m_LerpIn = 1f;
                else
                {
                    m_LerpIn += Time.deltaTime / m_DashInTime;
                    if (m_LerpIn > 1f)
                        m_LerpIn = 1f;
                }

                dashSpeed = EasingFunctions.EaseInQuadratic(m_LerpIn) * m_DashSpeed.value;
            }
            else
            {
                m_LerpOut += Time.deltaTime / m_DashOutTime;
                if (m_LerpOut > 1f)
                {
                    m_LerpOut = 1f;
                    m_Completed = true;
                }

                dashSpeed = m_DashOutCurve.Evaluate(m_LerpOut) * m_DashSpeed.value;

            }

            // Get new dash direction (based on curved walls)
            if (m_WallNormal != null && m_WallNormal.value.sqrMagnitude > 0.25f)
            {
                // Perform cast to check for contact
                RaycastHit hit;
                bool didHit = controller.characterController.RayCast(0.25f, -m_WallNormal.value, Space.World, out hit, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore);
                if (didHit)
                    m_WallNormal.value = hit.normal;
            }

            // Rotated direction based on deflections, etc
            var velocity = characterController.velocity;
            if (m_DashForwards && velocity.sqrMagnitude > 0.001f)
            {
                Vector3 newHeading = Vector3.ProjectOnPlane(characterController.velocity, up).normalized;

                // Turn character
                if (m_YawWithCurve)
                {
                    float angle = Vector3.SignedAngle(m_DashHeading, newHeading, up);
                    controller.aimController.AddYaw(angle);
                }

                m_DashHeading = newHeading;
            }

            // Record dash direction for next frame
            m_DashForwards = (dashSpeed >= 0f);

            // Get velocity from speed and heading
            m_OutVelocity = m_DashHeading * (dashSpeed + m_EntrySpeed);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_DashSpeed.CheckReference(map);
            m_WallNormal = map.Swap(m_WallNormal);

            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_HeadingKey = new NeoSerializationKey("heading");
        private static readonly NeoSerializationKey k_OutVelKey = new NeoSerializationKey("outVel");
        private static readonly NeoSerializationKey k_LerpInKey = new NeoSerializationKey("lerpIn");
        private static readonly NeoSerializationKey k_LerpOutKey = new NeoSerializationKey("lerpOut");
        private static readonly NeoSerializationKey k_EntrySpeedKey = new NeoSerializationKey("entrySpeed");
        private static readonly NeoSerializationKey k_ForwardsKey = new NeoSerializationKey("forwards");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_HeadingKey, m_DashHeading);
            writer.WriteValue(k_OutVelKey, m_OutVelocity);
            writer.WriteValue(k_LerpInKey, m_LerpIn);
            writer.WriteValue(k_LerpOutKey, m_LerpOut);
            writer.WriteValue(k_EntrySpeedKey, m_EntrySpeed);
            writer.WriteValue(k_ForwardsKey, m_DashForwards);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_HeadingKey, out m_DashHeading, m_DashHeading);
            reader.TryReadValue(k_OutVelKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_LerpInKey, out m_LerpIn, m_LerpIn);
            reader.TryReadValue(k_LerpOutKey, out m_LerpOut, m_LerpOut);
            reader.TryReadValue(k_EntrySpeedKey, out m_EntrySpeed, m_EntrySpeed);
            reader.TryReadValue(k_ForwardsKey, out m_DashForwards, m_DashForwards);
        }

        #endregion
    }
}