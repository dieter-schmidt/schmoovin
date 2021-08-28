using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-randomobjectmuzzleeffect.html")]
    public class RandomObjectMuzzleEffect : BaseMuzzleEffectBehaviour
    {
        [Header("Muzzle Effect Settings")]

        [SerializeField, Tooltip("The muzzle flash game objects")]
        private GameObject[] m_MuzzleFlashes = new GameObject[0];

        [SerializeField, Range(0.01f, 10f), Tooltip("The duration the flash should remain active. Keep this longer for objects with particle effects")]
        private float m_MuzzleFlashDuration = 0.5f;

        [SerializeField, Tooltip("The audio clips to use when firing. Chosen at random.")]
        private AudioClip[] m_FiringSounds = null;

        [SerializeField, Range(0f, 1f), Tooltip("The volume that firing sounds are played at.")]
        private float m_ShotVolume = 1f;

        private List<GameObject> m_Pool = null;
        private ActiveGameObject[] m_Active = null;
        private int m_OldestActive = 0;
        private int m_NextActive = 0;

        private struct ActiveGameObject
        {
            public GameObject gameObject;
            public float lifetime;
        }

        public override bool isModuleValid
        {
            get { return m_MuzzleFlashes.Length > 0; }
        }

        protected override void Awake()
        {
            base.Awake();

            m_Pool = new List<GameObject>(m_MuzzleFlashes.Length);
            for (int i = 0; i < m_MuzzleFlashes.Length; ++i)
            {
                if (m_MuzzleFlashes[i] != null)
                {
                    m_Pool.Add(m_MuzzleFlashes[i]);
                    m_MuzzleFlashes[i].SetActive(false);
                }
            }
            m_Active = new ActiveGameObject[m_Pool.Count];
        }

        void OnDisable()
        {
            DisableAll();
        }

        public override void Fire()
        {
            if (m_Pool.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, m_Pool.Count);

                m_Active[m_NextActive].gameObject = m_Pool[index];
                m_Active[m_NextActive].lifetime = m_MuzzleFlashDuration;

                m_Pool[index].SetActive(true);
                m_Pool.RemoveAt(index);

                ++m_NextActive;
                if (m_NextActive >= m_Active.Length)
                    m_NextActive -= m_Active.Length;
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

        void FixedUpdate()
        {
            int max = m_Active.Length;
            if (max == 0)
                return;

            // Reduce life-times
            for (int i = 0; i < max; ++i)
            {
                int index = m_OldestActive + i;
                if (index >= max)
                    index -= max;

                if (m_Active[index].gameObject != null)
                    m_Active[index].lifetime -= Time.deltaTime;
                else
                {
                    if (i == 0)
                        return;
                    else
                        break;
                }
            }

            // Remove dead flashes from active
            while (m_Active[m_OldestActive].gameObject != null && m_Active[m_OldestActive].lifetime < 0f)
            {
                // Deactivate
                m_Active[m_OldestActive].gameObject.SetActive(false);
                // Add to pool
                m_Pool.Add(m_Active[m_OldestActive].gameObject);
                // Set to null
                m_Active[m_OldestActive].gameObject = null;
                // Move to next
                ++m_OldestActive;
                if (m_OldestActive >= max)
                    m_OldestActive -= max;
            }
        }

        void DisableAll()
        {
            int max = m_Active.Length;
            if (max == 0)
                return;

            while (m_Active[m_OldestActive].gameObject != null)
            {
                m_Pool.Add(m_Active[m_OldestActive].gameObject);
                m_Active[m_OldestActive].gameObject = null;
                ++m_OldestActive;
                if (m_OldestActive >= max)
                    m_OldestActive -= max;
            }
        }

        public override void FireContinuous()
        {
            Debug.LogError("The RandomObjectMuzzleEffect firearm module is not intended for guns that fire continuously. Try the BasicGameObjectMuzzleEffect instead");
        }

        public override void StopContinuous()
        {
        }

        public int GetMinShotSpacing()
        {
            if (m_MuzzleFlashes.Length == 0)
                return -1;

            float framesPerFlash = m_MuzzleFlashDuration / Time.fixedDeltaTime;
            return Mathf.CeilToInt(framesPerFlash) / m_MuzzleFlashes.Length;
        }
    }
}