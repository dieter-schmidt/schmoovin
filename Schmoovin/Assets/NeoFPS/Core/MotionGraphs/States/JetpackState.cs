#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Airborne/Jetpack (Basic)", "Jetpack")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-jetpackstate.html")]
    public class JetpackState : MotionGraphState
    {
        [SerializeField, Tooltip("An optional parameter that acts as a multiplier for the jetpack force (0.5 is half power, etc).")]
		private FloatParameter m_Power = null;
        [SerializeField, Tooltip("An acceleration force (ignores mass) upwards for the jetpack.")]
		private FloatDataReference m_Force = new FloatDataReference(1f);

        private Vector3 m_OutVelocity = Vector3.zero;

        public override bool completed
        {
            get { return false; }
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
            get { return false; }
        }

        public override bool ignorePlatformMove
        {
            get { return true; }
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
            m_OutVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();
            
            float force = m_Force.value;
            if (m_Power != null)
                force *= m_Power.value;

            m_OutVelocity = characterController.velocity + (characterController.up * force * Time.deltaTime);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Power = map.Swap(m_Power);
            m_Force.CheckReference(map);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);
            writer.WriteValue(k_VelocityKey, m_OutVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            reader.TryReadValue(k_VelocityKey, out m_OutVelocity, m_OutVelocity);
        }

        #endregion
    }
}