using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    [CreateAssetMenu(fileName = "SlopeSpeedCurve", menuName = "NeoFPS/Motion Graph/Slope Speed Curve", order = NeoFpsMenuPriorities.motiongraph_slopespeedcurve)]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-so-slopespeedcurve.html")]
    public class SlopeSpeedCurve : ScriptableObject
    {
        [SerializeField, Tooltip("A curve that defines the speed drop off when moving up slopes. Y-Axis is the amount of the horizontal speed that will be redirected up the slope. X-Axis is the normalised ground slope from 0 degrees at 0 to slope limit at 1. Negative X is downhill, Positive is uphill.")]
        private AnimationCurve m_Curve = new AnimationCurve(new Keyframe[] { new Keyframe(-1f, 0.5f), new Keyframe(0f, 1f), new Keyframe(0.6f, 1f), new Keyframe(1f, 0f, 0f, 0f) });
        
        public AnimationCurve curve
        {
            get { return m_Curve; }
        }

        void OnValidate()
        {
            // Set animation curve modes
            m_Curve.preWrapMode = WrapMode.ClampForever;
            m_Curve.postWrapMode = WrapMode.ClampForever;
        }
    }
}
