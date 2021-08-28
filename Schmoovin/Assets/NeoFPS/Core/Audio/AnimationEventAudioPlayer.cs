using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(Animator))]
    [HelpURL("https://docs.neofps.com/manual/audioref-mb-animationeventaudioplayer.html")]
    public class AnimationEventAudioPlayer : MonoBehaviour
    {
        [SerializeField, Tooltip("The audio source to play from")]
        private AudioSource m_AudioSource = null;

        [SerializeField, Tooltip("")]
        private AudioClipSet[] m_ClipSets = new AudioClipSet[0];
        
        public enum NextClip
        {
            Sequential,
            Random
        }

        [Serializable]
        public struct AudioClipSet
        {
            [Tooltip("The name of the clip set, used as the parameter of the animation events")]
            public string key;

            [Tooltip ("How the next clip to play is selected")]
            public NextClip nextClip;

            [Tooltip("The volume to play the clip at")]
            public float volume;

            [Tooltip("The audio clips to choose from")]
            public List<AudioClip> clips;

            private int m_Index;

            public void OnValidate()
            {
                // Clamp the volume
                volume = Mathf.Clamp01(volume);
            }

            public void Initialise()
            {
                // Reset index
                m_Index = -1;

                // Remove invalid clips
                for (int i = clips.Count; i > 0; --i)
                {
                    if (clips[i - 1] == null)
                        clips.RemoveAt(i - 1);
                }
            }

            public void PlayClip(AudioSource source)
            {
                if (clips.Count == 0)
                    return;

                // Get the clip index
                if (nextClip == NextClip.Sequential)
                {
                    if (++m_Index >= clips.Count)
                        m_Index -= clips.Count;
                }
                else
                {
                    if (clips.Count == 1)
                        m_Index = 0;
                    else
                        m_Index = UnityEngine.Random.Range(0, clips.Count);
                }

                // Play the clip
                source.PlayOneShot(clips[m_Index], volume);
            }
        }

        void OnValidate()
        {
            for (int i = 0; i < m_ClipSets.Length; ++i)
                m_ClipSets[i].OnValidate();
        }

        void Awake()
        {
            for (int i = 0; i < m_ClipSets.Length; ++i)
                m_ClipSets[i].Initialise();
        }

        public void PlayClip(string key)
        {
            if (m_AudioSource == null)
                return;

            for (int i = 0; i < m_ClipSets.Length; ++i)
            {
                if (m_ClipSets[i].key == key)
                {
                    m_ClipSets[i].PlayClip(m_AudioSource);
                    return;
                }
            }

            Debug.LogWarning("Audio clip set key not found: " + key);
        }
    }
}