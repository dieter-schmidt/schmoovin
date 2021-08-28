using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-particleammoeffect.html")]
	public class ParticleAmmoEffect : BaseAmmoEffect
	{
        [SerializeField, NeoPrefabField(required = true), Tooltip("The object to spawn at the impact location")]
        private ParticleImpactEffect m_ImpactEffect = null;

		[SerializeField, Tooltip("The damage the bullet does.")]
		private float m_Damage = 25f;

		[SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a Rigidbody or an impact handler.")]
		private float m_ImpactForce = 15f;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);

        private PooledObject m_Prototype = null;

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Damage < 0f)
				m_Damage = 0f;
            if (m_ImpactForce < 0f)
                m_ImpactForce = 0f;
        }
#endif

        public override bool isModuleValid
        {
            get { return m_ImpactEffect != null; }
        }

        private void Awake()
        {
            m_Prototype = m_ImpactEffect.GetComponent<PooledObject>();
        }

        public override void Hit (RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
		{
            // Apply damage
            if (m_Damage > 0f)
            {
                // Apply damage
                hit.collider.GetComponents(s_DamageHandlers);
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                    s_DamageHandlers[i].AddDamage(m_Damage, hit, damageSource);
                s_DamageHandlers.Clear();
            }
			
            // Apply force
            if (hit.collider != null && m_ImpactForce > 0f)
            {
                IImpactHandler impactHandler = hit.collider.GetComponent<IImpactHandler>();
                if (impactHandler != null)
                    impactHandler.HandlePointImpact(hit.point, rayDirection * m_ImpactForce);
                else
                {
                    if (hit.rigidbody != null)
                        hit.rigidbody.AddForceAtPosition(rayDirection * m_ImpactForce, hit.point, ForceMode.Impulse);
                }
            }

            // Spawn impact particle effect
            PoolManager.GetPooledObject<ParticleImpactEffect>(m_Prototype, hit.point, Quaternion.LookRotation(hit.normal));
		}
	}
}