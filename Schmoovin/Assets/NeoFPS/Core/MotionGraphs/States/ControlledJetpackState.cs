#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Airborne/Jetpack (Controlled)", "Controlled Jetpack")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-controlledjetpackstate.html")]
    public class ControlledJetpackState : FallingState
    {
        [SerializeField, Tooltip("An optional parameter that acts as a multiplier for the jetpack force (0.5 is half power, etc).")]
        private FloatParameter m_JetpackPower = null;
        [SerializeField, Tooltip("An acceleration force (ignores mass) upwards for the jetpack.")]
        private FloatDataReference m_JetpackForce = new FloatDataReference(15f);

        private Vector3 m_JetpackVelocity = Vector3.zero;
        
        public override bool completed
        {
            get { return false; }
        }
        public override Vector3 moveVector
        {
            get { return base.moveVector + (m_JetpackVelocity * Time.deltaTime); }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            m_JetpackVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();
            
            float force = m_JetpackForce.value;
            if (m_JetpackPower != null)
                force *= m_JetpackPower.value;

            m_JetpackVelocity = (characterController.up * force * Time.deltaTime);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_JetpackPower = map.Swap(m_JetpackPower);
            m_JetpackForce.CheckReference(map);
            base.CheckReferences(map);
        }

#region SAVE / LOAD

        private static readonly NeoSerializationKey k_JetpackVelocityKey = new NeoSerializationKey("jetpackV");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);

            writer.WriteValue(k_JetpackVelocityKey, m_JetpackVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);
            
            reader.TryReadValue(k_JetpackVelocityKey, out m_JetpackVelocity, m_JetpackVelocity);
        }

#endregion
    }
}