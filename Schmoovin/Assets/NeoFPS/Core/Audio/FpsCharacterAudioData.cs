using System;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-so-fpscharacteraudiodata.html")]
    [CreateAssetMenu(fileName = "FpsCharacterAudioData", menuName = "NeoFPS/FPS Character Audio Data", order = NeoFpsMenuPriorities.ungrouped_characterAudio)]
    public class FpsCharacterAudioData : ScriptableObject
    {
        [SerializeField]
        private AudioData[] m_Data = new AudioData[0];

        [Serializable]
        public class AudioData
        {
			[SerializeField, Tooltip("A selection of audio clips to pick from. Will be selected at random to prevent repetition.")]
            private AudioClip[] m_Clips = new AudioClip[0];

			[SerializeField, Range(0f, 1f), Tooltip("The volume to play the clip at.")]
            private float m_Volume = 1f;

			[SerializeField, Range(0f, 5f), Tooltip("New clips will be blocked from playing for this duration after a clip plays. Prevents rapid fire audio.")]
            private float m_MinSpacing = 0.25f;

            public float volume
            {
                get { return m_Volume; }
            }

            public float minSpacing
            {
                get { return m_MinSpacing; }
            }

            public AudioData()
            {
                m_Clips = new AudioClip[0];
                m_Volume = 1f;
                m_MinSpacing = 0.25f;
            }

            public AudioClip GetRandomAudioClip()
            {
                return m_Clips[UnityEngine.Random.Range(0, m_Clips.Length)];
            }
        }

        void OnValidate()
        {
            CheckValidity();
        }

        public void CheckValidity()
        {
            // Check if constants have been changed
            int count = FpsCharacterAudio.count;
            if (m_Data == null || count != m_Data.Length)
            {
                // Create new resized array
                AudioData[] resized = new AudioData[count];

                // Copy old contents onto new
                if (m_Data != null)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (i < m_Data.Length)
                            resized[i] = m_Data[i];
                        else
                            resized[i] = new AudioData();
                    }
                }

                // Assign new array
                m_Data = resized;
            }
        }

        public AudioData GetAudioData(FpsCharacterAudio id)
        {
            if (m_Data.Length > id)
                return m_Data[id];
            return null;
        }
    }
}