using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-mb-fpscharacteraudiohandler.html")]
	public class FpsCharacterAudioHandler : MonoBehaviour, ICharacterAudioHandler
	{
		[SerializeField, Tooltip("The character audio library to use.")]
        private FpsCharacterAudioData m_AudioData = null;

        [SerializeField, Tooltip("The mixer group for character sound effects.")]
        private AudioMixerGroup m_MixerGroup = null;

        [SerializeField]
        private AudioSource[] m_OneShotSources = new AudioSource[FpsCharacterAudioSource.count];

        [SerializeField]
        private AudioSource[] m_LoopSources = new AudioSource[FpsCharacterAudioSource.count];

        private float[] m_Timers = null;

        void OnValidate()
        {
            CheckValidity();
        }

        public void CheckValidity ()
        {
            // Check one shot sources length
            if (m_OneShotSources.Length != FpsCharacterAudioSource.count)
            {
                AudioSource[] replacement = new AudioSource[FpsCharacterAudioSource.count];
                for (int i = 0; i < Mathf.Min (m_OneShotSources.Length, replacement.Length); ++i)
                    replacement[i] = m_OneShotSources[i];
                m_OneShotSources = replacement;
            }
            // Check loop sources length
            if (m_LoopSources.Length != FpsCharacterAudioSource.count)
            {
                AudioSource[] replacement = new AudioSource[FpsCharacterAudioSource.count];
                for (int i = 0; i < Mathf.Min(m_LoopSources.Length, replacement.Length); ++i)
                    replacement[i] = m_LoopSources[i];
                m_LoopSources = replacement;
            }
        }

        void Awake ()
		{
            // Initialise audio sources
            CheckValidity();
            for (int i = 0; i < m_OneShotSources.Length; ++i)
            {
                if (m_OneShotSources[i] != null)
                {
                    m_OneShotSources[i].outputAudioMixerGroup = m_MixerGroup;
                    m_OneShotSources[i].playOnAwake = false;
                    m_OneShotSources[i].loop = false;
                }
            }
            for (int i = 0; i < m_LoopSources.Length; ++i)
            {
                if (m_LoopSources[i] != null)
                {
                    m_LoopSources[i].outputAudioMixerGroup = m_MixerGroup;
                    m_LoopSources[i].playOnAwake = false;
                    m_LoopSources[i].loop = true;
                }
            }
            m_Timers = new float[FpsCharacterAudio.count];
        }

        void Update ()
        {
            // Decrement timers (to prevent audio spamming)
            for (int i = 0; i < m_Timers.Length; ++i)
            {
                if (m_Timers[i] > 0f)
                {
                    m_Timers[i] -= Time.deltaTime;
                    if (m_Timers[i] < 0f)
                        m_Timers[i] = 0f;
                }
            }
        }

        public void PlayClip (AudioClip clip, float volume = 1f)
		{
            PlayClip (clip, FpsCharacterAudioSource.Head, volume);
		}
        
        public void PlayClip (AudioClip clip, FpsCharacterAudioSource source, float volume = 1f)
		{
            if (m_OneShotSources[source] != null && m_OneShotSources[source].isActiveAndEnabled)
                m_OneShotSources[source].PlayOneShot (clip, volume);
		}

        public void PlayAudio(FpsCharacterAudio id)
        {
            PlayAudio (id, FpsCharacterAudioSource.Head);
        }

        public void PlayAudio(FpsCharacterAudio id, FpsCharacterAudioSource source)
        {
            if (m_AudioData == null || m_Timers[id] > Mathf.Epsilon)
                return;
                
			// Get audio data for id
            var data = m_AudioData.GetAudioData(id);
            if (data != null)
            {
                // Get rotator and clip
                AudioClip clip = data.GetRandomAudioClip();
                if (m_OneShotSources[source] != null && clip != null)
                {
                    // Play and add timeout
                    m_OneShotSources[source].PlayOneShot(clip, data.volume);
                    m_Timers[id] = data.minSpacing;
                }
            }
        }

        public void StartLoop (AudioClip clip, FpsCharacterAudioSource source, float volume = 1f, float pitch = 1f)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
            {
                loop.clip = clip;
                loop.pitch = pitch;
                loop.volume = volume;
                loop.Play();
            }
        }

        public void StopLoop (FpsCharacterAudioSource source)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
                loop.Stop();
        }

        public float GetLoopPitch (FpsCharacterAudioSource source)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
                return loop.pitch;
            else
                return 1f;
        }
        
        public void SetLoopPitch (FpsCharacterAudioSource source, float pitch)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
                loop.pitch = pitch;
        }

        public float GetLoopVolume(FpsCharacterAudioSource source)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
                return loop.volume;
            else
                return 1f;
        }

        public void SetLoopVolume(FpsCharacterAudioSource source, float volume)
        {
            AudioSource loop = m_LoopSources[source];
            if (loop != null)
                loop.volume = volume;
        }
    }
}