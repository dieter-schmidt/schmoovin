using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-pooledexplosionspawner.html")]
    public class PooledExplosionSpawner : MonoBehaviour
    {
        [Header("Explosion")]

        [SerializeField, NeoPrefabField(required = true), Tooltip("The explosion object to spawn")]
        private PooledExplosion m_Explosion = null;

        [SerializeField, Tooltip("An offset from the object's origin that the explosion will spawn.")]
        private Vector3 m_Offset = Vector3.zero;

        [SerializeField, Tooltip("The damage the explosion does at its center (drops off to zero at full radius).")]
        private float m_Damage = 25f;

        [SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a Rigidbody or an impact handler.")]
        private float m_MaxForce = 15f;

        private PooledObject m_Prototype = null;

        private void Awake()
        {
            m_Prototype = m_Explosion.GetComponent<PooledObject>();
        }

        public void SpawnExplosion()
        {
            if (m_Prototype != null)
            {
                Transform t = transform;
                Quaternion rotation = t.rotation;
                var explosion = PoolManager.GetPooledObject<PooledExplosion>(m_Prototype, t.position + rotation * m_Offset, rotation);
                explosion.Explode(m_Damage, m_MaxForce, null);
            }
        }
    }
}