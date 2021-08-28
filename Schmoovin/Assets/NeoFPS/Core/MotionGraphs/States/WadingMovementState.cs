#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Swimming/Wading Movement", "Wading")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-wadingstate.html")]
    public class WadingMovementState : MovementState
    {
        [SerializeField, Tooltip("The transform parameter which contains the transform of the water zone object")]
        private TransformParameter m_WaterZoneParameter = null;
        [SerializeField, Tooltip("The multiplier for the standard movement speed when submerged to min speed depth")]
        private FloatDataReference m_MinSpeedMultiplier = new FloatDataReference(0.25f);
        [SerializeField, Tooltip("The depth the character must be submerged to move at minimum speed")]
        private float m_MinSpeedDepth = 1f;
        [SerializeField, Tooltip("The submersion depth of the character where their speed is not affected")]
        private float m_MaxSpeedDepth = 0.3f;
        
        private Transform m_WaterZoneTransform = null;
        private IWaterZone m_WaterZone = null;

        public override void OnValidate()
        {
            base.OnValidate();
            m_MinSpeedMultiplier.ClampValue(0.05f, 0.95f);
        }

        protected override Vector3 GetTargetVelocity(float directionMultiplier)
        {            
            Vector3 result = base.GetTargetVelocity(directionMultiplier);

            CheckWaterZone();
            if (m_WaterZone != null)
            {
                var belowWater = -WaterZoneHelpers.CompareLowestToSurface(controller, m_WaterZone);
                belowWater -= m_MaxSpeedDepth;
                belowWater /= m_MinSpeedDepth - m_MaxSpeedDepth;

                float multiplier = Mathf.Lerp(1f, m_MinSpeedMultiplier.value, belowWater);
                result *= multiplier;
            }

            return result;
        }

        void CheckWaterZone()
        {
            if (m_WaterZoneParameter != null)
            {
                if (m_WaterZoneTransform != m_WaterZoneParameter.value)
                {
                    m_WaterZoneTransform = m_WaterZoneParameter.value;
                    m_WaterZone = m_WaterZoneTransform.GetComponent<IWaterZone>();
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            base.CheckReferences(map);
            m_WaterZoneParameter = map.Swap(m_WaterZoneParameter);
            m_MinSpeedMultiplier.CheckReference(map);
        }
    }
}