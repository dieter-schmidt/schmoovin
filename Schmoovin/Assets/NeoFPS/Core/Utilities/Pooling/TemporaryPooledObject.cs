using UnityEngine;

namespace NeoFPS
{
    public class TemporaryPooledObject : PooledObject
    {
        [SerializeField, Tooltip("The duration the object will stay active before returning to the pool")]
        private float m_Lifetime = 5f;

        private float m_Elapsed = 0f;

        private void OnValidate()
        {
            if (m_Lifetime < 0f)
                m_Lifetime = 0f;
        }

        private void OnEnable()
        {
            m_Elapsed = 0f;
        }

        private void Update()
        {
            m_Elapsed += Time.deltaTime;
            if (m_Elapsed > m_Lifetime)
                ReturnToPool();
        }
    }
}
