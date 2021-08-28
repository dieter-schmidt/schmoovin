using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-advancedbulletammoeffect.html")]
	public class AdvancedBulletAmmoEffect : BaseAmmoEffect
	{
        [Header("Random Damage")]

        [SerializeField, Tooltip("Damage is randomised withing a set min/max range")]
        private bool m_RandomiseDamage = true;

		[SerializeField, Tooltip("The minimum damage the bullet does before falloff is applied.")]
		private float m_MinDamage = 15f;

        [SerializeField, Tooltip("The maximum damage the bullet does before falloff is applied.")]
        private float m_MaxDamage = 25f;

        [Header("Falloff")]

        [SerializeField, Tooltip("How to apply damage falloff (none, distance based or speed based).")]
        private FalloffMode m_FalloffMode = FalloffMode.Range;

        [SerializeField, FormerlySerializedAs("m_EffectiveRange"), Tooltip("Either range at full damage, or speed at 0 damage depending on mode.")]
        private float m_FalloffSettingLower = 100f;

        [SerializeField, FormerlySerializedAs("m_IneffectiveRange"), Tooltip("Either range where the bullet does 0 damage, or speed where it does full damage depending on mode.")]
        private float m_FalloffSettingUpper = 200f;

        [Header("Effect")]

        [SerializeField, Tooltip("The size of the bullet. Used to size decals.")]
		private float m_BulletSize = 1f;

		[SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler.")]
		private float m_ImpactForce = 15f;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);
        
        public enum FalloffMode
        {
            None,
            Range,
            Speed
        }

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_MinDamage < 0f)
                m_MinDamage = 0f;
            if (m_MaxDamage < 0f)
                m_MaxDamage = 0f;
            if (m_FalloffSettingLower < 0f)
                m_FalloffSettingLower = 0f;
            if (m_FalloffSettingUpper < 0.1f)
                m_FalloffSettingUpper = 0.1f;
            if (m_BulletSize < 0.1f)
				m_BulletSize = 0.1f;
            if (m_ImpactForce < 0f)
                m_ImpactForce = 0f;
        }
#endif

#if !UNITY_EDITOR
        private float m_DamageNormaliser = 0f;
        private void Awake()
        {
            m_DamageNormaliser = 1f / (m_FalloffSettingUpper - m_FalloffSettingLower);
        }
#endif

        public override void Hit (RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Apply distance falloff
            float falloffMultiplier = 1f;
            switch (m_FalloffMode)
            {
                case FalloffMode.Range:
                    {
                        // Quit out if it's gone past effective range
                        if (totalDistance > m_FalloffSettingUpper)
                            return;

                        if (totalDistance > m_FalloffSettingLower)
                        {
#if UNITY_EDITOR // In editor, calculate at runtime
                            float normalised = (totalDistance - m_FalloffSettingLower) / (m_FalloffSettingUpper - m_FalloffSettingLower);
#else // At runtime, use a precalculated multiplier
                            float normalised = (totalDistance - m_FalloffSettingLower) * m_DamageNormaliser;
#endif
                            falloffMultiplier = Mathf.Clamp01(1f - (normalised * normalised));
                        }
                    }
                    break;
                case FalloffMode.Speed:
                    {
                        if (float.IsPositiveInfinity(speed))
                            Debug.LogError("Can't use speed based damage falloff with hitscan shooters as the speed is always infinite");
                        else
                        {
#if UNITY_EDITOR // In editor, calculate at runtime
                            float normalised = (speed - m_FalloffSettingLower) / (m_FalloffSettingUpper - m_FalloffSettingLower);
#else // At runtime, use a precalculated multiplier
                            float normalised = (speed - m_FalloffSettingLower) * m_DamageNormaliser;
#endif
                            falloffMultiplier = Mathf.Clamp01(normalised);
                        }
                    }
                    break;
            }

            float impactForce = m_ImpactForce;

            // Show effect
            SurfaceManager.ShowBulletHit (hit, rayDirection, m_BulletSize, hit.rigidbody != null);
            
            // Get random damage
            float damage = m_MaxDamage;
            if (m_RandomiseDamage)
                damage = Random.Range(m_MinDamage, m_MaxDamage);

            // Apply falloff
            damage *= falloffMultiplier;
            impactForce *= falloffMultiplier;

            // Apply damage
            if (damage > 0f)
            {
                // Apply damage
                hit.collider.GetComponents(s_DamageHandlers);
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                    s_DamageHandlers[i].AddDamage(damage, hit, damageSource);
                s_DamageHandlers.Clear();
            }
			
            // Apply force (nb check collider in case the damage resulted in the object being destroyed)
            if (hit.collider != null && impactForce > 0f)
            {
                IImpactHandler impactHandler = hit.collider.GetComponent<IImpactHandler>();
                if (impactHandler != null)
                    impactHandler.HandlePointImpact(hit.point, rayDirection * impactForce);
                else
                {
                    if (hit.rigidbody != null)
                        hit.rigidbody.AddForceAtPosition(rayDirection * impactForce, hit.point, ForceMode.Impulse);
                }
            }
		}
	}
}