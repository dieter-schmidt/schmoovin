#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Wall Movement/Mantle", "Mantle")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-mantlestate.html")]
    public class MantleState : MotionGraphState
    {
        [SerializeField, Tooltip("The normal of the wall to climb. This value will be read AND written to each frame")]
        private VectorParameter m_WallNormal = null;
        [SerializeField, Tooltip("The movement speed while climbing the surface")]
        private FloatDataReference m_ClimbSpeed = new FloatDataReference(5f);
        [SerializeField, Tooltip("The collision mask to use when checking the wall normal")]
        private LayerMask m_WallCollisionMask = PhysicsFilter.Masks.StaticCharacterBlockers;
        [SerializeField, Range(0.05f, 0.5f), Tooltip("The distance to perform the wall checks.")]
        private FloatDataReference m_WallCheckDistance = new FloatDataReference(0.25f);
        [SerializeField, Range(0f, 1f), Tooltip("The climb speed multiplier (for the data value above) on entering the state")]
        private float m_StartingSpeedMultiplier = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("The climb speed multiplier (for the data value above) on completing the ledge mantle")]
        private float m_EndingSpeedMultiplier = 0.5f;
        [SerializeField, Tooltip("The distance to move past the edge onto flat ground before completing")]
        private float m_OvershootDistance = 0.5f;

        private Vector3 m_OutVelocity = Vector3.zero;
        private Vector3 m_StartingWallNormal = Vector3.zero;
        private float m_ClimbMotorSpeed = 0f;
        private float m_ClimbMotorAcceleration = 0f;
        private float m_TopDistance = 0f;
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
            get { return elapsedTime > 0.25f; }
        }

        public override bool applyGroundingForce
        {
            get { return elapsedTime > 0.25f; }
        }

        public override bool ignorePlatformMove
        {
            get { return false; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_OvershootDistance = Mathf.Clamp(m_OvershootDistance, 0.01f, 2f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Completed = false;
            m_ClimbMotorSpeed = m_ClimbSpeed.value * m_StartingSpeedMultiplier;
            m_ClimbMotorAcceleration = 0f;
            m_StartingWallNormal = Vector3.zero;
            m_TopDistance = 0f;
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

            if (!m_Completed)
            {
                if (m_WallNormal != null && m_WallNormal.value.sqrMagnitude > 0.5f)
                {
                    // CAst to get updated wall normal
                    RaycastHit hit;
                    if (characterController.SphereCast(0f, -m_WallNormal.value * m_WallCheckDistance.value, Space.World, out hit, m_WallCollisionMask, QueryTriggerInteraction.Ignore))
                    {
                        // Check if at top
                        bool top = (m_TopDistance > 0f);
                        if (!top)
                        {
                            // Store hit normal
                            m_WallNormal.value = hit.normal;

                            // First frame, store it for end of climb
                            if (m_StartingWallNormal.sqrMagnitude < 0.25f)
                                m_StartingWallNormal = Vector3.ProjectOnPlane(hit.normal, characterController.up).normalized;
                            
                            // Get a speed multiplier from the 
                            float targetSpeedMultiplier = 1f;
                            if (characterController.isGrounded)
                            {
                                float angle = Vector3.Angle(hit.normal, characterController.groundSurfaceNormal);
                                targetSpeedMultiplier = Mathf.Lerp(m_EndingSpeedMultiplier, 1f, angle / 90f);

                                if (angle < 5f)
                                    top = true;
                            }

                            // Get the climb velocity
                            m_ClimbMotorSpeed = Mathf.SmoothDamp(m_ClimbMotorSpeed, m_ClimbSpeed.value * targetSpeedMultiplier, ref m_ClimbMotorAcceleration, 0.25f, 25f);
                            Vector3 upWall = Vector3.ProjectOnPlane(characterController.up, m_WallNormal.value).normalized;
                            m_OutVelocity = upWall * m_ClimbMotorSpeed;
                            m_OutVelocity += hit.normal * -0.1f;

                            // Record top distance for next frame
                            if (top)
                                m_TopDistance += m_ClimbMotorSpeed * Time.deltaTime;
                        }
                        else
                        {
                            // Move forwards onto ledge
                            float speed = m_ClimbSpeed.value * m_EndingSpeedMultiplier;
                            m_OutVelocity = Vector3.Lerp(characterController.velocity, m_StartingWallNormal * -speed, 0.25f);
                            m_OutVelocity += characterController.up * 0.01f;

                            // Increment top distance and set completed if (should have) moved correct amount
                            m_TopDistance += speed * Time.deltaTime;
                            if (m_TopDistance > m_OvershootDistance)
                                m_Completed = true;
                        }
                    }
                    else
                        m_Completed = true;
                }
                else
                    m_Completed = true;
            }

            // Give default values if completed
            if (m_Completed)
                m_OutVelocity = characterController.velocity;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WallNormal = map.Swap(m_WallNormal);
            m_ClimbSpeed.CheckReference(map);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_CompletedKey = new NeoSerializationKey("completed");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_StartNormalKey = new NeoSerializationKey("wallN");
        private static readonly NeoSerializationKey k_ClimbSpeedKey = new NeoSerializationKey("climbV");
        private static readonly NeoSerializationKey k_ClimbAccelerationKey = new NeoSerializationKey("climbA");
        private static readonly NeoSerializationKey k_TopDistanceKey = new NeoSerializationKey("topD");
        
        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
            writer.WriteValue(k_CompletedKey, m_Completed);
            writer.WriteValue(k_StartNormalKey, m_StartingWallNormal);
            writer.WriteValue(k_ClimbSpeedKey, m_ClimbMotorSpeed);
            writer.WriteValue(k_ClimbAccelerationKey, m_ClimbMotorAcceleration);
            writer.WriteValue(k_TopDistanceKey, m_TopDistance);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
            reader.TryReadValue(k_CompletedKey, out m_Completed, m_Completed);
            reader.TryReadValue(k_StartNormalKey, out m_StartingWallNormal, m_StartingWallNormal);
            reader.TryReadValue(k_ClimbSpeedKey, out m_ClimbMotorSpeed, m_ClimbMotorSpeed);
            reader.TryReadValue(k_ClimbAccelerationKey, out m_ClimbMotorAcceleration, m_ClimbMotorAcceleration);
            reader.TryReadValue(k_TopDistanceKey, out m_TopDistance, m_TopDistance);
        }

        #endregion
    }
}