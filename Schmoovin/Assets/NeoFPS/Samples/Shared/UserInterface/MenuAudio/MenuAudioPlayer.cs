using System;
using UnityEngine;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    [RequireComponent (typeof (RectTransform))]
	[RequireComponent (typeof (AudioSource))]
	public class MenuAudioPlayer : MonoBehaviour
	{
		private AudioSource[] m_Sources = null;
        private int m_CurrentIndex = 0;

		void Awake ()
		{
			m_Sources = GetComponents <AudioSource> ();
		}

		public void PlayClip (AudioClip clip)
		{
			// Get the source
			AudioSource source = m_Sources [m_CurrentIndex];
			++m_CurrentIndex;
			if (m_CurrentIndex >= m_Sources.Length)
				m_CurrentIndex = 0;

			// Play the clip
			source.PlayOneShot (clip);
		}
	}
}

