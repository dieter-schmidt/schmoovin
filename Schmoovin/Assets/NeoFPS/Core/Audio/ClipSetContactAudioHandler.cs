using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-mb-clipsetcontactaudiohandler.html")]
	[RequireComponent (typeof (AudioSource))]
	public class ClipSetContactAudioHandler : BaseContactAudioHandler
	{
        [SerializeField, Tooltip("The audio clips to choose from on impact")]
        private AudioClip[] m_Clips = new AudioClip[0];

        private AudioSource m_AudioSource = null;

        protected override void Awake()
        {
            m_AudioSource = GetComponent<AudioSource> ();
		}

        protected override void PlayContactAudio(Collision collision)
        {
            switch(m_Clips.Length)
            {
                case 0:
                    return;
                case 1:
                    if (m_Clips[0] != null)
                        m_AudioSource.PlayOneShot(m_Clips[0]);
                    return;
                default:
                    int index = Random.Range(0, m_Clips.Length);
                    if (m_Clips[index] != null)
                        m_AudioSource.PlayOneShot(m_Clips[index]);
                    return;
            }
        }
    }
}