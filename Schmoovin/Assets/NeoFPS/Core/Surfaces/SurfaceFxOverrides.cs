using NeoFPS.Constants;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/surfacesref-mb-surfacefxoverrides.html")]
    public class SurfaceFxOverrides : MonoBehaviour
    {
        [SerializeField, Tooltip("The impact special effects for things like bullet hits.")]
        private SurfaceHitFxData m_ImpactEffects = null;

        [SerializeField, Tooltip("The audio library for impact audio, eg. bullet hits.")]
        private SurfaceAudioData m_ImpactAudio = null;
        
        public SurfaceHitFxData impactEffects
        {
            get { return m_ImpactEffects; }
        }

        public SurfaceAudioData impactAudio
        {
            get { return m_ImpactAudio; }
        }

        void Awake ()
		{
            SurfaceManager.ApplyOverrides(this);
		}

        void OnDestroy()
        {
            SurfaceManager.RemoveOverrides(this);
        }
	}
}