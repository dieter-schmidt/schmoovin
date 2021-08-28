using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-basicgameobjectmuzzleeffect.html")]
	public class BasicGameObjectMuzzleEffect : BaseMuzzleEffectBehaviour
    {
        [Header("Muzzle Effect Settings")]

        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("The muzzle flash game object")]
		private GameObject m_MuzzleFlash = null;

        [SerializeField, Range(0f, 1f), Tooltip("The duration the flash should remain visible in seconds.")]
		private float m_MuzzleFlashDuration = 0.05f;

        [SerializeField, Tooltip("The audio clips to use when firing. Chosen at random.")]
        private AudioClip[] m_FiringSounds = null;

		[SerializeField, Range(0f,1f), Tooltip("The volume that firing sounds are played at.")]
		private float m_ShotVolume = 1f;

		public GameObject muzzleFlash {
			get { return m_MuzzleFlash; }
			set
			{
				StopContinuous ();
				m_MuzzleFlash = value;
				StopContinuous ();
			}
		}

        public override bool isModuleValid
        {
            get { return m_MuzzleFlash != null; }
        }

        void OnDisable ()
		{
			StopContinuous ();
		}

		public override void Fire ()
		{
			if (m_MuzzleFlash != null)
			{
				if (m_MuzzleFlash.activeSelf == true)
				{
					CancelInvoke("Hide");
					m_MuzzleFlash.SetActive(false);
				}

				m_MuzzleFlash.SetActive (true);
				Invoke ("Hide", m_MuzzleFlashDuration);
            }

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

		public override void FireContinuous ()
		{
			if (m_MuzzleFlash != null)
				m_MuzzleFlash.SetActive (true);
		}

		public override void StopContinuous ()
		{
			if (m_MuzzleFlash != null)
				m_MuzzleFlash.SetActive (false);
		}

        void Hide ()
        {
            if (m_MuzzleFlash != null)
                m_MuzzleFlash.SetActive(false);
        }
    }
}