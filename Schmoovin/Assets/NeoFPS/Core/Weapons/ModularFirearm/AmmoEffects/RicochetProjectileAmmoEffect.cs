using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class RicochetProjectileAmmoEffect : BaseAmmoEffect
    {
        [Header("HitEffect")]

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it hits something.")]
        private BaseAmmoEffect m_HitEffect = null;
        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it hits something after ricocheting.")]
        private BaseAmmoEffect m_RicochetEffect = null;

        [Header("Ricochet")]

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Range(0f, 1f), Tooltip("A blending value based on ricocheting directly along the hit normal (0) vs the reflection of the inbound ray (1)")]
        private float m_NormalVsDeflection = 1f;

        [SerializeField, Tooltip("Randomises the deflected bullet direction within this cone angle")]
        private float m_MaxScatterAngle = 15f;

        [SerializeField, Tooltip("The number of shots to split the ricochet into (1 is no split).")]
        private int m_SplitCount = 1;

        [Header("Projectile")]

        [SerializeField, NeoPrefabField(typeof (IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, Tooltip("How the ricochet speed of the projectile is calculated.")]
        private RicochetSpeedMode m_SpeedMode = RicochetSpeedMode.Multiplier;

        [SerializeField, Tooltip("The minimum speed setting of the projectile (dependent on the mode).")]
        private float m_MinSpeedSetting = 0.75f;

        [SerializeField, Tooltip("The maximum speed setting of the projectile (dependent on the mode).")]
        private float m_MaxSpeedSetting = 0.75f;

        [SerializeField, Tooltip("The gravity for the projectile.")]
        private float m_Gravity = 9.8f;

        public enum RicochetSpeedMode
        {
            Multiplier,
            FixedSpeed,
            AngleBasedMultiplier,
            AngleBasedSpeed
        }

        public override bool isModuleValid
        {
            get { return m_HitEffect != null && m_RicochetEffect != null; }
        }

        void OnValidate()
        {
            // Check shell prefab is valid
            if (m_ProjectilePrefab != null && m_ProjectilePrefab.GetComponent<IProjectile>() == null)
            {
                Debug.Log("Projectile prefab must have BallisticProjectile component attached: " + m_ProjectilePrefab.name);
                m_ProjectilePrefab = null;
            }

            if (m_HitEffect == this)
            {
                Debug.LogError("You want an infinitely recursive ricochet? Are you insane??!?");
                m_HitEffect = null;
            }
            if (m_RicochetEffect == this)
            {
                Debug.LogError("You want an infinitely recursive ricochet? Are you insane??!?");
                m_RicochetEffect = null;
            }

            m_SplitCount = Mathf.Clamp(m_SplitCount, 1, 100);
            m_MaxScatterAngle = Mathf.Clamp(m_MaxScatterAngle, 0f, 45f);

            switch (m_SpeedMode)
            {
                case RicochetSpeedMode.Multiplier:
                    m_MinSpeedSetting = Mathf.Clamp(m_MinSpeedSetting, 0.0001f, 1f);
                    break;
                case RicochetSpeedMode.FixedSpeed:
                    if (m_MinSpeedSetting < 0.1f)
                        m_MinSpeedSetting = 0.1f;
                    break;
                case RicochetSpeedMode.AngleBasedMultiplier:
                    m_MinSpeedSetting = Mathf.Clamp(m_MinSpeedSetting, 0.0001f, 1f);
                    m_MaxSpeedSetting = Mathf.Clamp(m_MaxSpeedSetting, 0.0001f, 1f);
                    break;
                case RicochetSpeedMode.AngleBasedSpeed:
                    if (m_MaxSpeedSetting < 0.1f)
                        m_MaxSpeedSetting = 0.1f;
                    if (m_MaxSpeedSetting < 0.1f)
                        m_MaxSpeedSetting = 0.1f;
                    break;
            }
        }

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Apply the hit effect
            if (m_HitEffect != null)
                m_HitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

            if (m_ProjectilePrefab != null)
            {
                // Calculate the ricochet direction
                Vector3 ricochetDirection = hit.normal;
                if (m_NormalVsDeflection > 0.0001f)
                {
                    Vector3 reflected = Vector3.Reflect(rayDirection, hit.normal);
                    if (m_NormalVsDeflection < 0.999f)
                        ricochetDirection = Vector3.Slerp(ricochetDirection, reflected, m_NormalVsDeflection);
                    else
                        ricochetDirection = reflected;
                }

                // Calculate the speed
                float outSpeed = 0f;
                switch(m_SpeedMode)
                {
                    case RicochetSpeedMode.Multiplier:
                        if (speed == float.PositiveInfinity)
                        {
                            Debug.LogError("Cannot use a multiplier based ricochet speed with a hitscan shooter as the input speed is infinite");
                            outSpeed = 50f;
                        }
                        else
                            outSpeed = speed * m_MinSpeedSetting;
                        break;
                    case RicochetSpeedMode.FixedSpeed:
                        outSpeed = m_MinSpeedSetting;
                        break;
                    case RicochetSpeedMode.AngleBasedMultiplier:
                        {
                            if (speed == float.PositiveInfinity)
                            {
                                Debug.LogError("Cannot use a multiplier based ricochet speed with a hitscan shooter as the input speed is infinite");
                                outSpeed = 50f;
                            }
                            else
                            {
                                var dot = -Vector3.Dot(rayDirection, hit.normal);
                                outSpeed = speed * Mathf.Lerp(m_MaxSpeedSetting, m_MinSpeedSetting, dot);
                            }
                        }
                        break;
                    case RicochetSpeedMode.AngleBasedSpeed:
                        {
                            var dot = -Vector3.Dot(rayDirection, hit.normal);
                            outSpeed = Mathf.Lerp(m_MaxSpeedSetting, m_MinSpeedSetting, dot);
                        }
                        break;
                }

                // Spawn the projectiles
                for (int i = 0; i < m_SplitCount; ++i)
                {
                    var outDirection = ricochetDirection;

                    // Add randomisation
                    if (m_MaxScatterAngle > 0.01f)
                    {
                        Quaternion randomRot = UnityEngine.Random.rotationUniform;
                        outDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxScatterAngle / 360f) * outDirection;

                        // Check it's not pointing into the surface
                        var dot = Vector3.Dot(outDirection, hit.normal);
                        if (dot < 0f)
                            outDirection = (outDirection + hit.normal * (0.01f - dot)).normalized;
                    }

                    IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                    InitialiseRicochetProjectile(projectile);

                    projectile.Fire(hit.point, outDirection * outSpeed, m_Gravity, m_RicochetEffect, null, m_Layers, damageSource);
                    projectile.gameObject.SetActive(true);
                }
            }
        }

        protected virtual void InitialiseRicochetProjectile(IProjectile projectile)
        { }
    }
}
