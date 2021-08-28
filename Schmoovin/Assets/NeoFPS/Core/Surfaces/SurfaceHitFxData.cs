using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/surfacesref-so-surfacehitfxdata.html")]
    [CreateAssetMenu(fileName = "SurfaceHitFxData", menuName = "NeoFPS/Surface Impact Effects Data", order = NeoFpsMenuPriorities.ungrouped_surfaceImpacts)]
    public class SurfaceHitFxData : ScriptableObject
    {
        [SerializeField, Tooltip("Per-surface data")]
        private BaseHitFxBehaviour[] m_Data = new BaseHitFxBehaviour[0];

        void OnValidate()
        {
            // Resize to match constants
            if (m_Data.Length != FpsSurfaceMaterial.count)
            {
                // Allocate replacement array of correct size
                BaseHitFxBehaviour[] replacement = new BaseHitFxBehaviour[FpsSurfaceMaterial.count];

                // Copy data over
                int i = 0;
                for (; i < replacement.Length && i < m_Data.Length; ++i)
                    replacement[i] = m_Data[i];

                // Set new entries to null
                for (; i < replacement.Length; ++i)
                    replacement[i] = null;

                // Swap
                m_Data = replacement;
            }
        }

        public BaseHitFxBehaviour GetImpactEffect(FpsSurfaceMaterial surface)
        {
            // Try getting random clip from the array
            BaseHitFxBehaviour result = m_Data[surface];
            if (result != null)
                return result;

            // Use default values
            return m_Data[0];
        }
    }
}