#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoCC;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Water")]
    public class WaterCondition : MotionGraphCondition
    {
        [SerializeField] private TransformParameter m_WaterZoneTransform = null;
        [SerializeField] private CheckType m_CheckType = CheckType.FullySubmerged;
        [SerializeField] private float m_CheckValue = 0f;

        public enum CheckType
        {
            FullySubmerged,
            AboveWaterLessThan,
            AboveWaterGreaterThan,
            BelowWaterLessThan,
            BelowWaterGreaterThan
        }

        private Transform m_LastTransform = null;
        private IWaterZone m_WaterZone = null;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            // Check parameter
            if (m_WaterZoneTransform == null || m_WaterZoneTransform.value == null)
                return false;

            // Get waterzone
            if (m_WaterZoneTransform.value != m_LastTransform)
            {
                m_LastTransform = m_WaterZoneTransform.value;
                m_WaterZone = m_LastTransform.GetComponent<IWaterZone>();
            }

            // If waterzone is null, there's nothing to check
            if (m_WaterZone == null)
                return false;

            // Check relevant point
            switch (m_CheckType)
            {
                case CheckType.FullySubmerged:
                    return WaterZoneHelpers.CompareHighestToSurface(controller, m_WaterZone) < 0f;
                case CheckType.AboveWaterGreaterThan:
                    return WaterZoneHelpers.CompareHighestToSurface(controller, m_WaterZone) > m_CheckValue;
                case CheckType.AboveWaterLessThan:
                    return WaterZoneHelpers.CompareHighestToSurface(controller, m_WaterZone) < m_CheckValue;
                case CheckType.BelowWaterGreaterThan:
                    return -WaterZoneHelpers.CompareLowestToSurface(controller, m_WaterZone) > m_CheckValue;
                case CheckType.BelowWaterLessThan:
                    return -WaterZoneHelpers.CompareLowestToSurface(controller, m_WaterZone) < m_CheckValue;
            }

            return false;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_WaterZoneTransform = map.Swap(m_WaterZoneTransform);
        }
    }
}