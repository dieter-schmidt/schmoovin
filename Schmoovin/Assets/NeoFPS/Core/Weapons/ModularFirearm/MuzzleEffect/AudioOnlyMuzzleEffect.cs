using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-audioonlymuzzleeffect.html")]
	public class AudioOnlyMuzzleEffect : BaseMuzzleEffectBehaviour
    {
        [Header("Muzzle Effect Settings")]

        [SerializeField, Tooltip("The audio clips to use when firing. Chosen at random.")]
        private AudioClip[] m_FiringSounds = null;

        [SerializeField, Range(0f, 1f), Tooltip("The volume that firing sounds are played at.")]
        private float m_ShotVolume = 1f;

        void OnDisable ()
		{
			StopContinuous ();
		}

		public override void Fire ()
		{
            switch (m_FiringSounds.Length)
            {
                case 0:
                    return;
                case 1:
                    firearm.PlaySound(m_FiringSounds[0], m_ShotVolume);
                    return;
                default:
                    firearm.PlaySound(m_FiringSounds[UnityEngine.Random.Range(0, m_FiringSounds.Length)], m_ShotVolume);
                    return;
            }
        }

        public override void FireContinuous() {}

        public override void StopContinuous() {}
    }
}