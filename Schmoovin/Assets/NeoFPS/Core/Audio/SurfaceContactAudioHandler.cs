using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/audioref-mb-surfacecontactaudiohandler.html")]
	public class SurfaceContactAudioHandler : BaseContactAudioHandler
	{
		private BaseSurface m_Surface = null;

        protected override void Awake()
        {
			m_Surface = GetComponentInParent<BaseSurface> ();
		}

        protected override void PlayContactAudio(Collision collision)
        {
            SurfaceManager.PlayImpactNoiseAtPosition(m_Surface.GetSurface(), transform.position, 1f);

            BaseSurface otherSurface = collision.transform.GetComponent<BaseSurface>();
            if (otherSurface != null)
                SurfaceManager.PlayImpactNoiseAtPosition(otherSurface.GetSurface(), transform.position, 1f);
        }
    }
}