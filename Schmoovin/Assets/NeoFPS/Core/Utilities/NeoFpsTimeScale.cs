using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public static class NeoFpsTimeScale
    {
        private const float k_MaxTimeScale = 10f;
        private const float k_MinTimeScale = 0.001f;

        public static event UnityAction<float> onTimeScaleChanged;

        private static float m_ResumeTimeScale = 1f;

        public static bool isPaused
        {
            get { return Time.timeScale == 0f; }
        }

        public static float timeScale
        {
            get { return Time.timeScale; }
            set
            {
                value = Mathf.Clamp(value, k_MinTimeScale, k_MaxTimeScale);
                if (Time.timeScale != value)
                {
                    Time.timeScale = value;
                    if (onTimeScaleChanged != null)
                        onTimeScaleChanged(value);
                }
            }
        }

        public static void FreezeTime()
        {
            if (Time.timeScale != 0f)
            {
                m_ResumeTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        public static void ResumeTime()
        {
            if (Time.timeScale == 0f)
                Time.timeScale = m_ResumeTimeScale;
        }
    }
}