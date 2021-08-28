using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-weaponbob.html")]
    [CreateAssetMenu(fileName = "PositionBobData", menuName = "NeoFPS/Position Bob Data", order = NeoFpsMenuPriorities.ungrouped_positionBobData)]
    public class PositionBobData : ScriptableObject
    {
        [SerializeField, Tooltip("The maximum position offset along the x-axis in either direction.")]
        private float m_HorizontalBobRange = 0.01f;

        [SerializeField, Tooltip("The maximum position offset along the y-axis in either direction.")]
        private float m_VerticalBobRange = 0.005f;

        [SerializeField, Tooltip("The curve over one step cycle for the bob effect.")]
        private AnimationCurve m_BobCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
            new Keyframe(2f, 0f)); // curve for head bob

#if UNITY_EDITOR
        void OnValidate()
        {
            m_HorizontalBobRange = Mathf.Clamp(m_HorizontalBobRange, 0f, 0.5f);
            m_VerticalBobRange = Mathf.Clamp(m_VerticalBobRange, 0f, 0.5f);
            m_BobCurve.preWrapMode = WrapMode.Loop;
            m_BobCurve.postWrapMode = WrapMode.Loop;
        }
#endif
        
        public Vector2 GetBobPositionAtTime(float stepCount)
        {
            return new Vector2(
                m_BobCurve.Evaluate(stepCount) * m_HorizontalBobRange,
                m_BobCurve.Evaluate (stepCount * 2f) * m_VerticalBobRange
                );
        }
    }
}