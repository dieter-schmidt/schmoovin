using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Misc/Constant Move", "Constant Move")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-constantmovestate.html")]
    public class ConstantMoveState : MotionGraphState
    {
        [SerializeField, Tooltip("The direction to move the character in")]
        private VectorParameter m_MoveDirection = null;

        [SerializeField, Tooltip("The target movement speed")]
        private FloatDataReference m_MoveSpeed = new FloatDataReference(1f);

        [SerializeField, Tooltip("The maximum acceleration")]
        private FloatDataReference m_Acceleration = new FloatDataReference(50f);

        [SerializeField, Range(0f, 1f), Tooltip("The amount of damping to apply when changing direction or speed")]
        private float m_Damping = 0.25f;

        private const float k_TinyValue = 0.001f;

        private Vector3 m_MotorAcceleration = Vector3.zero;
        private Vector3 m_OutVelocity = Vector3.zero;

        public override bool applyGravity
        {
            get { return false; }
        }

        public override bool applyGroundingForce
        {
            get { return false; }
        }

        public override Vector3 moveVector
        {
            get { return m_OutVelocity * Time.deltaTime; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_MotorAcceleration = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            if (m_MoveDirection != null)
            {
                // Accelerate the velocity
                m_OutVelocity = Vector3.SmoothDamp(characterController.velocity, m_MoveDirection.value.normalized * m_MoveSpeed.value, ref m_MotorAcceleration, Mathf.Lerp(0.05f, 0.25f, m_Damping), m_Acceleration.value);
            }
            else
                m_OutVelocity = characterController.velocity;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_MoveDirection = map.Swap(m_MoveDirection);
            m_MoveSpeed.CheckReference(map);
            m_Acceleration.CheckReference(map);
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