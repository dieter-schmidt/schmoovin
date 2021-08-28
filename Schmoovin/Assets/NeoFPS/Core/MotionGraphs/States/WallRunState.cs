#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Wall Movement/Wall Run", "WallRun")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-wallrunstate.html")]
    public class WallRunState : MotionGraphState
    {
        [SerializeField, Tooltip("The vector parameter containing the wall normal. This will be read AND written to each frame")]
        private VectorParameter m_WallNormal = null;
        [SerializeField, Tooltip("A multiplier applied to gravity acceleration when moving up the wall.")]
        private FloatDataReference m_ClimbGravityMultiplier = new FloatDataReference(1f);
        [SerializeField, Tooltip("A multiplier applied to gravity acceleration when moving down the wall. If set to 0, the vertical character velocity will be clamped to >= 0")]
        private FloatDataReference m_FallGravityMultiplier = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("How the vertical speed is calculated when entering the wall run.")]
        private VerticalStartSpeed m_VerticalMode = VerticalStartSpeed.VerticalBoost;
        [SerializeField, Tooltip("The target vertical speed.")]
        private FloatDataReference m_VerticalTarget = new FloatDataReference(0f);
        [SerializeField, Tooltip("An upward speed boost when first entering the state.")]
        private FloatDataReference m_VerticalBoost = new FloatDataReference(2f);
        [SerializeField, Tooltip("The maximum downward speed the character can reach while wall running.")]
        private FloatDataReference m_MaxFallSpeed = new FloatDataReference(20f);
        [SerializeField, Tooltip("Should the downwards speed be limited.")]
        private bool m_CapFallSpeed = false;

        [SerializeField, Tooltip("How the horizontal wall run speed is calculated.")]
        private HorizontalSpeed m_HorizontalMode = HorizontalSpeed.MaintainExisting;
        [SerializeField, Tooltip("The target horizontal speed.")]
        private FloatDataReference m_HorizontalSpeed = new FloatDataReference(10f);
        [SerializeField, Tooltip("The acceleration up to target horizontal speed.")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);
        [SerializeField, Tooltip("The deceleration down to target horizontal speed.")]
        private FloatDataReference m_Deceleration = new FloatDataReference(10f);
        [SerializeField, Range(0f, 1f), Tooltip("The amount of damping to apply when changing direction or speed")]
        private float m_HorizontalDamping = 0.25f;

        public enum VerticalStartSpeed
        {
            VerticalBoost,
            CappedBoost,
            Minimum,
            MaintainExisting,
            FixedSpeed
        }

        public enum HorizontalSpeed
        {
            MaintainExisting,
            TargetSpeed,
            MinimumSpeed
        }

        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;
        private bool m_ContactFrame = false;

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
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_Completed = false;
            m_ContactFrame = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_Completed = false;
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            // Check if valid
            if (m_WallNormal == null)
            {
                m_Completed = true;
                return;
            }

            if (m_WallNormal.value.sqrMagnitude < 0.25f)
            {
                Debug.LogError("Zero wall normal. ID: " + m_WallNormal.GetInstanceID());
                return;
            }
            
            // Perform cast to check for contact
            RaycastHit hit;
            bool didHit = controller.characterController.RayCast(0.25f, -m_WallNormal.value, Space.World, out hit, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore);
            if (didHit)
            {
                //Debug.Log("didHit = true");
                m_WallNormal.value = hit.normal;

                m_OutVelocity = characterController.velocity;
                var wallUp = Vector3.ProjectOnPlane(characterController.up, m_WallNormal.value).normalized;

                if (m_ContactFrame)
                {
                    switch (m_VerticalMode)
                    {
                        case VerticalStartSpeed.VerticalBoost:
                            // Add the vertical boost
                            m_OutVelocity += wallUp * m_VerticalBoost.value;
                            break;
                        case VerticalStartSpeed.CappedBoost:
                            {
                                // Check if up-speed is below target and boost up to the target
                                Vector3 up = characterController.up;
                                float upSpeed = Vector3.Dot(m_OutVelocity, up);
                                if (upSpeed < m_VerticalTarget.value)
                                {
                                    m_OutVelocity -= up * upSpeed;
                                    m_OutVelocity += up * Mathf.Min(upSpeed + m_VerticalBoost.value, m_VerticalTarget.value);
                                }
                            }
                            break;
                        case VerticalStartSpeed.Minimum:
                            {
                                // Check if up-speed is below target and add the difference if so
                                Vector3 up = characterController.up;
                                float upSpeed = Vector3.Dot(m_OutVelocity, up);
                                if (upSpeed < m_VerticalTarget.value)
                                    m_OutVelocity += up * (m_VerticalTarget.value - upSpeed);
                            }
                            break;
                        case VerticalStartSpeed.FixedSpeed:
                            {
                                // Set up speed to fixed values
                                Vector3 up = characterController.up;
                                float upSpeed = Vector3.Dot(m_OutVelocity, up);
                                m_OutVelocity += up * (m_VerticalTarget.value - upSpeed);
                            }
                            break;
                    }


                    m_ContactFrame = false;
                }
                else
                {
                    // Decompose veocity
                    Vector3 up = characterController.up;
                    float upSpeed = Vector3.Dot(m_OutVelocity, up);
                    Vector3 horizontal = m_OutVelocity - up * upSpeed;
                    
                    // Get custom gravity effect
                    if (upSpeed <= 0.0001f)
                    {
                        if (m_FallGravityMultiplier.value > 0.0001f)
                        {
                            // Check if the fall speed is capped
                            if (m_CapFallSpeed && upSpeed < -m_MaxFallSpeed.value)
                            {
                                upSpeed = -m_MaxFallSpeed.value;
                                m_OutVelocity = up * upSpeed;
                            }
                            else
                            {
                                m_OutVelocity = up * upSpeed;
                                m_OutVelocity += Vector3.Project(characterController.gravity, wallUp) * Time.deltaTime * m_FallGravityMultiplier.value;
                            }
                        }
                        else
                            m_OutVelocity = Vector3.zero;
                    }
                    else
                    {
                        if (m_ClimbGravityMultiplier.value > 0.0001f)
                        {
                            m_OutVelocity = up * upSpeed;
                            m_OutVelocity += Vector3.Project(characterController.gravity, wallUp) * Time.deltaTime * m_ClimbGravityMultiplier.value;
                        }
                        else
                            m_OutVelocity = Vector3.zero;
                    }

                    // Calculate horizontal velocity
                    switch (m_HorizontalMode)
                    {
                        case HorizontalSpeed.TargetSpeed:
                            {
                                // Damp the velocity to the target
                                float hSpeed = horizontal.magnitude;
                                float target = m_HorizontalSpeed.value;
                                float acceleration = 0f;
                                if (hSpeed < target)
                                    acceleration = m_Acceleration.value;
                                else
                                    acceleration = m_Deceleration.value;

                                Vector3 targetHorizontal = horizontal * (target / hSpeed);
                                horizontal = Vector3.SmoothDamp(horizontal, targetHorizontal, ref m_MotorAcceleration, Mathf.Lerp(0.05f, 0.25f, m_HorizontalDamping), acceleration);
                            }
                            break;
                        case HorizontalSpeed.MinimumSpeed:
                            {
                                // Boost the velocity if below the target
                                float hSpeed = horizontal.magnitude;
                                float target = m_HorizontalSpeed.value;
                                if (hSpeed < target)
                                {
                                    Vector3 targetHorizontal = horizontal * (target / hSpeed);
                                    horizontal = Vector3.SmoothDamp(horizontal, targetHorizontal, ref m_MotorAcceleration, Mathf.Lerp(0.05f, 0.25f, m_HorizontalDamping), m_Acceleration.value);
                                }
                            }
                            break;
                    }

                    // Add the revised horizontal back onto the velocity
                    m_OutVelocity += horizontal;
                }
            }
            else
            {
                //Debug.Log("didHit = false");
                m_Completed = true;
                m_OutVelocity = characterController.velocity;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WallNormal = map.Swap(m_WallNormal);
            m_ClimbGravityMultiplier.CheckReference(map);
            m_FallGravityMultiplier.CheckReference(map);
            m_VerticalTarget.CheckReference(map);
            m_VerticalBoost.CheckReference(map);
            m_MaxFallSpeed.CheckReference(map);
            m_HorizontalSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
            m_Deceleration.CheckReference(map);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_CompletedKey = new NeoSerializationKey("completed");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
            writer.WriteValue(k_CompletedKey, m_Completed);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_CompletedKey, out m_Completed, m_Completed);
        }

        #endregion
    }
}