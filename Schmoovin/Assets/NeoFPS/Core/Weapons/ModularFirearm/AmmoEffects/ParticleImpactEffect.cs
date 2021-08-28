using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-particleimpacteffect.html")]
    [RequireComponent(typeof(PooledObject))]
    public class ParticleImpactEffect : MonoBehaviour
    {
        [SerializeField, Tooltip("Duration the object should remain active before being returned to the pool.")]
        private float m_Lifetime = 2f;

        private PooledObject m_PooledObject = null;
        private float m_Timer = 0f;

        private void OnValidate()
        {
            m_Lifetime = Mathf.Clamp(m_Lifetime, 0.25f, 100f);
        }

        private void Awake()
        {
            m_PooledObject = GetComponent<PooledObject>();
        }

        private void OnEnable()
        {
            m_Timer = 0f;
        }

        private void Update()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer > m_Lifetime)
                m_PooledObject.ReturnToPool();
        }
    }
}
