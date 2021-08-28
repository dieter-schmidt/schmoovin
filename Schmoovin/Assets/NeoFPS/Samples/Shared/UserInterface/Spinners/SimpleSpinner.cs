using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples
{
    public class SimpleSpinner : UiSpinner
    {
        [SerializeField, Tooltip("The spinner transform for a visual indication of loading")]
        private Transform m_SpinTransform = null;
        [SerializeField, Tooltip("The spin rate of the spinner in degrees per second")]
        private float m_SpinRate = 90f;
        [SerializeField, Tooltip("Spin clockwise or anti-clockwise")]
        private bool m_Clockwise = true;

        private void OnValidate()
        {
            if (m_SpinTransform == null)
                m_SpinTransform = transform;
        }

        public override void Tick()
        {
            float direction = (m_Clockwise) ? -1f : 1f;
            m_SpinTransform.Rotate(0f, 0f, m_SpinRate * direction * Time.unscaledDeltaTime);
        }

        protected override IEnumerator ShowCoroutine()
        {
            yield return null;

        }

        protected override IEnumerator HideCoroutine()
        {
            yield return null;
        }
    }
}
