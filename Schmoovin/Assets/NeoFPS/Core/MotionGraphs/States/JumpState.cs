#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Jump", "Jump")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-jumpstate.html")]
    public class JumpState : MotionGraphState
    {
        [SerializeField, Tooltip("The charge property is a float with value 0 to 1 that defines how high to jump. If no graph property is selected, the jump will always be performed with full strength.")]
        private FloatParameter m_ChargeParameter = null;

        [SerializeField, Tooltip("The height the player will jump when fully charged. Note: Changes to gravity or physics multiplier after start will affect this. Also does not take step height into account.")]
        private FloatDataReference m_MaximumHeight = new FloatDataReference(1f);

        [SerializeField, Tooltip("The smallest height the player will jump (at the equivalent of a zero tap - actually unattainable)")]
        private FloatDataReference m_MinimumHeight = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("The amount of influence the ground has over the direction of the jump. 0 = up, 1 = ground normal.")]
        private FloatDataReference m_GroundInfluence = new FloatDataReference(0.25f);

        [SerializeField, Tooltip("If true then the character's vertical velocity is set so they will achieve the jump height from their current position. If false, then the velocity that would achieve that height from standing is added to the current velocity.")]
        private bool m_IgnoreFallSpeed = true;

        [SerializeField, Tooltip("If true then the gravity calculations will only be done on initialisation and therefore changing gravity will change the actual jump height.")]
        private bool m_UseInitialGravity = true;

        private static readonly NeoSerializationKey k_CompletedKey = new NeoSerializationKey("completed");
        private static readonly NeoSerializationKey k_JumpPhysicsKey = new NeoSerializationKey("jumpPhysics");

        private float m_Charge = 1f;
        private float m_JumpPhysics = 0f;
        private Vector3 m_OutVelocity = Vector3.zero;
        private float m_MinJumpSpeed = 0f;
        private float m_MaxJumpSpeed = 0f;
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

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            m_JumpPhysics = 2f * -Vector3.Dot(Physics.gravity, Vector3.up);
        }

        public override void OnEnter()
        {
            m_Completed = false;
            if (m_ChargeParameter != null)
            {
                m_Charge = m_ChargeParameter.value;
                m_ChargeParameter.value = 0f;
            }
            base.OnEnter();
        }

        public override void OnExit()
        {
            m_Completed = false;
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();

            if (!m_Completed)
            {
                // Calculate the jump speeds to achieve height
                if (!m_UseInitialGravity)
                    m_JumpPhysics = 2f * -Vector3.Dot(Physics.gravity, Vector3.up);
                m_MinJumpSpeed = Mathf.Sqrt(m_MinimumHeight.value * m_JumpPhysics);
                m_MaxJumpSpeed = Mathf.Sqrt(m_MaximumHeight.value * m_JumpPhysics);

                // Get the direction vector based on jump ground influence
                Vector3 direction = Vector3.up;
                if (characterController.isGrounded)
                    direction = Vector3.Slerp(characterController.up, characterController.groundNormal, m_GroundInfluence.value);

                // Get the jump speed
                float jumpSpeed = Mathf.Lerp(m_MinJumpSpeed, m_MaxJumpSpeed, m_Charge);

                // Apply the jump
                if (m_IgnoreFallSpeed)
                {
                    m_OutVelocity = Vector3.ProjectOnPlane(characterController.velocity, characterController.up);
                    m_OutVelocity += direction * jumpSpeed;
                }
                else
                {
                    m_OutVelocity = characterController.velocity + direction * jumpSpeed;
                }

                m_Completed = true;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_ChargeParameter = map.Swap(m_ChargeParameter);
            m_MaximumHeight.CheckReference(map);
            m_MinimumHeight.CheckReference(map);
            m_GroundInfluence.CheckReference(map);
        }

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_CompletedKey, m_Completed);
            writer.WriteValue(k_JumpPhysicsKey, m_JumpPhysics);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_CompletedKey, out m_Completed, m_Completed);
            reader.TryReadValue(k_JumpPhysicsKey, out m_JumpPhysics, m_JumpPhysics);
        }
    }
}