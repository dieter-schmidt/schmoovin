#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Wall Movement/Wall Run (Up)", "VertWallRun")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-verticalwallrunstate.html")]
    public class VerticalWallRunState : MotionGraphState
    {
        [SerializeField, Tooltip("A parameter containing the wall normal vector the character is running up")]
        private VectorParameter m_WallNormal = null;
        [SerializeField, Tooltip("An upward speed boost applied when entering the state")]
        private FloatDataReference m_UpBoost = new FloatDataReference(3f);
        [SerializeField, Tooltip("The upward speed can not be boosted above this value (though it can start higher than this)")]
        private FloatDataReference m_MaxBoostSpeed = new FloatDataReference(10f);
        [SerializeField, Tooltip("A multiplier that is used to reduce the effects of gravity when running up the wall")]
        private FloatDataReference m_GravityMultiplier = new FloatDataReference(0.5f);

        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Boost = false;
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
            get { return true; }
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            m_Completed = false;
            m_Boost = true;

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
                if (m_Boost)
                {
                    // Apply boost if up velocity is  below max
                    float upVelocity = Vector3.Dot(characterController.velocity, characterController.up);
                    upVelocity = Mathf.Min(m_MaxBoostSpeed.value, upVelocity + m_UpBoost.value);
                    m_OutVelocity = characterController.up * upVelocity;

                    m_Boost = false;
                }
                else
                {
                    // Test wall collision (completed if no collision)
                    if (m_WallNormal != null && m_WallNormal.value.sqrMagnitude > 0.5f)
                    {
                        RaycastHit hit;
                        if (characterController.SphereCast(0f, -m_WallNormal.value * 0.1f, Space.World, out hit, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore))
                            m_WallNormal.value = hit.normal;
                        else
                            m_Completed = true;
                    }

                    // Get wall velocity
                    m_OutVelocity = Vector3.ProjectOnPlane(characterController.velocity, m_WallNormal.value);

                    // Add reduced gravity effect
                    m_OutVelocity += (characterController.gravity * Time.deltaTime * m_GravityMultiplier.value);
                    
                    // Check if reached apex & set completed if true
                    if (Vector3.Dot(m_OutVelocity, characterController.up) < 0f)
                        m_Completed = true;
                }

            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WallNormal = map.Swap(m_WallNormal);
            m_UpBoost.CheckReference(map);
            m_MaxBoostSpeed.CheckReference(map);
            m_GravityMultiplier.CheckReference(map);
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