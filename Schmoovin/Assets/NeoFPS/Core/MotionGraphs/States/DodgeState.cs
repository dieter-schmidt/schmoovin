#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Instant/Dodge", "Dodge")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-dodgestate.html")]
    public class DodgeState : MotionGraphState
    {
        [SerializeField, Tooltip("The vertical dodge speed.")]
        private FloatDataReference m_VerticalSpeed = new FloatDataReference(3f);

        [SerializeField, Tooltip("The horizontal dodge speed")]
        private FloatDataReference m_HorizontalSpeed = new FloatDataReference(10f);

        [SerializeField, Tooltip("The key for the slide data entry (DodgeData)")]
        private IntParameter m_DodgeDirectionParameter = null;

        private static readonly NeoSerializationKey k_CompletedKey = new NeoSerializationKey("completed");

        // A "close-enough" value for cos 45 to use in calculations
        private const float k_Cos45 = 0.70710678119f;

        private Vector3 m_OutVelocity = Vector3.zero;
        private bool m_Completed = false;
        
        public enum DodgeDirection : int
        {
            None,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }

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

        public override void OnValidate()
        {
            base.OnValidate();
            m_VerticalSpeed.ClampValue(0f, 10f);
            m_HorizontalSpeed.ClampValue(1f, 25f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Completed = false;
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
                if (m_DodgeDirectionParameter != null && m_DodgeDirectionParameter.value != 0)
                {
                    // Get dodge vector from compass headings
                    Vector2 dodge = GetDirectionVector((DodgeDirection)m_DodgeDirectionParameter.value);
                    // Get original velocity
                    m_OutVelocity = Vector3.ProjectOnPlane(characterController.velocity, characterController.up);
                    // Add horizontal dodge velocity
                    float h = m_HorizontalSpeed.value;
                    m_OutVelocity += characterController.forward * dodge.y * h;
                    m_OutVelocity += characterController.right * dodge.x * h;
                    // Add vertical dodge velocity
                    m_OutVelocity += characterController.up * m_VerticalSpeed.value;

                    m_DodgeDirectionParameter.value = 0;
                }
                else
                    m_OutVelocity = characterController.velocity;

                m_Completed = true;
            }
        }

        Vector2 GetDirectionVector (DodgeDirection d)
        {
            switch (d)
            {
                case DodgeDirection.North:
                    return new Vector2(0f, 1f);
                case DodgeDirection.NorthEast:
                    return new Vector2(k_Cos45, k_Cos45);
                case DodgeDirection.East:
                    return new Vector2(1f, 0f);
                case DodgeDirection.SouthEast:
                    return new Vector2(k_Cos45, -k_Cos45);
                case DodgeDirection.South:
                    return new Vector2(0f, -1f);
                case DodgeDirection.SouthWest:
                    return new Vector2(-k_Cos45, -k_Cos45);
                case DodgeDirection.West:
                    return new Vector2(-1f, 0f);
                case DodgeDirection.NorthWest:
                    return new Vector2(-k_Cos45, k_Cos45);
            }
            return Vector2.zero;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_DodgeDirectionParameter = map.Swap(m_DodgeDirectionParameter);
            m_HorizontalSpeed.CheckReference(map);
            m_VerticalSpeed.CheckReference(map);
        }

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_CompletedKey, m_Completed);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_CompletedKey, out m_Completed, m_Completed);
        }
    }
}