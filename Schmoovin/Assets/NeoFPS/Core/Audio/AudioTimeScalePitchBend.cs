using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-mb-audiotimescalepitchbend.html")]
    [RequireComponent(typeof(AudioSource))]
    public class AudioTimeScalePitchBend : MonoBehaviour
    {
        [SerializeField, Tooltip("The audio sources to scale based on time")]
        private AudioSource[] m_AudioSources = { };

        private void OnValidate()
        {
            if (m_AudioSources.Length == 0)
            {
                m_AudioSources = new AudioSource[1];
                m_AudioSources[0] = GetComponent<AudioSource>();
            }
        }
        
        void OnEnable()
        {
            NeoFpsTimeScale.onTimeScaleChanged += OnTimeScaleChanged;
            OnTimeScaleChanged(NeoFpsTimeScale.timeScale);
        }

        void OnDisable()
        {
            NeoFpsTimeScale.onTimeScaleChanged -= OnTimeScaleChanged;
            OnTimeScaleChanged(1f);
        }

        void OnTimeScaleChanged(float timeScale)
        {
            for (int i = 0; i < m_AudioSources.Length; ++i)
            {
                if (m_AudioSources[i] != null)
                    m_AudioSources[i].pitch = timeScale;
            }
        }
    }
}
