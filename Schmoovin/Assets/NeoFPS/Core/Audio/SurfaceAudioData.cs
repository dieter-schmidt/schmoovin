using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-so-surfaceaudiodata.html")]
    [CreateAssetMenu(fileName = "SurfaceAudioData", menuName = "NeoFPS/Surface Audio Data", order = NeoFpsMenuPriorities.ungrouped_surfaceAudio)]
	public class SurfaceAudioData : ScriptableObject
	{
		[SerializeField, Tooltip("Per-surface data")]
		private SurfaceAudioClips[] m_Data = new SurfaceAudioClips[0];

		[Serializable]
		private struct SurfaceAudioClips
		{
			[Range(0f, 1f), Tooltip("Clip volume")]
			public float volume;

			[Tooltip("Audio clips (will be picked at random)")]
			public AudioClip[] clips;
		}

		void OnValidate()
		{
			// Resize to match constants
			int targetCount = FpsSurfaceMaterial.count;
			if (m_Data.Length != targetCount)
			{
				// Allocate replacement array of correct size
				SurfaceAudioClips[] replacement = new SurfaceAudioClips[targetCount];

				// Copy data over
                int i = 0;
                for (; i < replacement.Length && i < m_Data.Length; ++i)
                {
                    replacement[i].volume = m_Data[i].volume;
                    replacement[i].clips = m_Data[i].clips;
                }

				// Set new entries volume to 1
                for (; i < replacement.Length; ++i)
                    replacement[i].volume = 1f;

				// Swap
                m_Data = replacement;
            }
		}

		public AudioClip GetAudioClip (FpsSurfaceMaterial surface)
		{
			// Try getting random clip from the array
			AudioClip result = GetClipFromArray(m_Data[surface].clips);
			if (result != null)
				return result;

			// Use default values
			return GetClipFromArray(m_Data[0].clips);
		}

		public AudioClip GetAudioClip (FpsSurfaceMaterial surface, out float volume)
		{
			// Try getting random clip from the array
			AudioClip result = GetClipFromArray(m_Data[surface].clips);
			if (result != null)
			{
				volume = m_Data[surface].volume;
				return result;
			}

            // Use default values
            volume = m_Data[0].volume;
			return GetClipFromArray(m_Data[0].clips);
		}

		AudioClip GetClipFromArray(AudioClip[] clips)
		{
			int count = clips.Length;
			switch (count)
			{
				case 0:
					return null;
				case 1:
					return clips[0];
				default:
					return clips[UnityEngine.Random.Range(0, count)];
			}
		}
	}
}