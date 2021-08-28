#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Ground Movement/Ski", "Ski")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-skistate.html")]
    public class SkiState : MotionGraphState
    {
        [SerializeField, Tooltip("The maximum turn rate in degrees per second.")]
        private FloatDataReference m_MaxTurnRate = new FloatDataReference(10f);
        [SerializeField, Tooltip("A constant deceleration while skiing. Once the character speed reaches zero, the state completes.")]
        private FloatDataReference m_Deceleration = new FloatDataReference(0.5f);
        [SerializeField, Tooltip("A multiplier for gravity which affects speed on slopes.")]
        private FloatDataReference m_GravityEffect = new FloatDataReference(1f);

        private const float k_TinyValue = 0.001f;

        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;
        private float m_StepHeight = 0f;
        private float m_SnapHeight = 0f;

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
            get { return true; }// false; }
        }

        public override bool applyGroundingForce
        {
            get { return false; }// true; }// m_Snap; }
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

            m_StepHeight = characterController.stepHeight;
            characterController.stepHeight = 0.025f;
            m_SnapHeight = characterController.groundSnapHeight;
            characterController.groundSnapHeight = 0.1f;

            characterController.slopeFriction = 0f;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_Completed = false;
            m_OutVelocity = Vector3.zero;

            characterController.stepHeight = m_StepHeight;
            characterController.groundSnapHeight = m_SnapHeight;

            characterController.slopeFriction = 1f;
        }

        public override void Update()
        {
            base.Update();

            if (m_Completed)
                return;

            // Get velocity & speed
            m_OutVelocity = Vector3.ProjectOnPlane(characterController.rawVelocity, characterController.groundSurfaceNormal);
            float speed = m_OutVelocity.magnitude;
			// ABOVE WILL BE WHY IT SPEEDS UP ON GRAVITY LOOP
			// RAW GAINS STEP HEIGHT AND FLOOR IS SLOPED

            float newSpeed = speed - m_Deceleration.value * Time.deltaTime;
            m_OutVelocity *= newSpeed / speed;

            // Complete state if near stationary
            if (newSpeed < 0.05f)
                m_Completed = true;
            else
            {
                // Apply turning input
                if (Mathf.Abs(controller.inputMoveDirection.x) > 0.01f)
                {
                    // Turning based on speed. Slower movement = faster turn
                    float turnMultiplier = 1f / newSpeed;
                    if (turnMultiplier > 1f)
                        turnMultiplier = 1f;
                    m_OutVelocity = Quaternion.AngleAxis(turnMultiplier * controller.inputMoveDirection.x * m_MaxTurnRate.value, characterController.groundSurfaceNormal) * m_OutVelocity;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_MaxTurnRate.CheckReference(map);
            m_Deceleration.CheckReference(map);
            m_GravityEffect.CheckReference(map);
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