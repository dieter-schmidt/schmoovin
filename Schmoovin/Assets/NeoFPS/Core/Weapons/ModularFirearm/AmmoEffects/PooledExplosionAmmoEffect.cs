using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-splashparticleammoeffect.html")]
	public class PooledExplosionAmmoEffect : BaseAmmoEffect
	{
        [SerializeField, NeoPrefabField(required = true), Tooltip("The explosion object to spawn at the impact location")]
        private PooledExplosion m_Explosion = null;
        
        [SerializeField, Tooltip("The maximum damage the explosion will do at the center of its area of effect. Drops off to zero at the edge of its radius.")]
		private float m_Damage = 25f;

		[SerializeField, Tooltip("The maximum force to be imparted onto objects in the area of effect. Requires either a Rigidbody or an impact handler.")]
		private float m_MaxForce = 15f;

        [SerializeField, Tooltip("An offset from the hit point along its normal to spawn the explosion. Prevents the explosion from appearing embedded in the surface.")]
        private float m_NormalOffset = 0f;

        private PooledObject m_Prototype = null;

#if UNITY_EDITOR
        void OnValidate ()
        {
            if (m_Damage < 0f)
				m_Damage = 0f;
            if (m_MaxForce < 0f)
                m_MaxForce = 0f;
        }
#endif

        public override bool isModuleValid
        {
            get { return m_Explosion != null; }
        }

        private void Awake()
        {
            if (m_Explosion != null)
                m_Prototype = m_Explosion.GetComponent<PooledObject>();
        }

        public override void Hit (RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
		{
            // Spawn splash effect and deal damage
            var explosion = PoolManager.GetPooledObject<PooledExplosion>(m_Prototype, hit.point + hit.normal * m_NormalOffset, Quaternion.LookRotation(hit.normal));
            explosion.Explode(m_Damage, m_MaxForce, damageSource);
		}
	}
}