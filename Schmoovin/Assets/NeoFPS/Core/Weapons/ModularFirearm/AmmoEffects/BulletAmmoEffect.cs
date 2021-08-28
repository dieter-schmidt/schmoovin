using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-bulletammoeffect.html")]
	public class BulletAmmoEffect : BaseAmmoEffect
	{
		[SerializeField, Tooltip("The damage the bullet does.")]
		private float m_Damage = 25f;

		[SerializeField, Tooltip("The size of the bullet. Used to size decals.")]
		private float m_BulletSize = 1f;

		[SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler.")]
		private float m_ImpactForce = 15f;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Damage < 0f)
				m_Damage = 0f;
            if (m_BulletSize < 0.1f)
				m_BulletSize = 0.1f;
            if (m_ImpactForce < 0f)
                m_ImpactForce = 0f;
        }
		#endif

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Show effect
            SurfaceManager.ShowBulletHit(hit, rayDirection, m_BulletSize, hit.rigidbody != null);

            // Apply damage
            if (m_Damage > 0f)
            {
                // Apply damage
                hit.collider.GetComponents(s_DamageHandlers);
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                    s_DamageHandlers[i].AddDamage(m_Damage, hit, damageSource);
                s_DamageHandlers.Clear();
            }
			
            // Apply force (nb check collider in case the damage resulted in the object being destroyed)
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
        }
    }
}