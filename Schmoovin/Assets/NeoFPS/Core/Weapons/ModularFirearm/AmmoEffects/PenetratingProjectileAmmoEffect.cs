using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class PenetratingProjectileAmmoEffect : BaseAmmoEffect
    {
        [Header("HitEffect")]

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it first hits.")]
        private BaseAmmoEffect m_InitialHitEffect = null;
        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo after it has penetrated something.")]
        private BaseAmmoEffect m_SecondHitEffect = null;
        [SerializeField, Tooltip("Which ammo effect should be used if the shot fails to penetrate.")]
        private OnFailToPenetrate m_OnFailToPenetrate = OnFailToPenetrate.InitialEffect;

        [Header("Penetration")]

        [SerializeField, Tooltip("The maximum thickness of object the bullet can penetrate.")]
        private float m_MaxPenetration = 0.15f;

        [SerializeField, Tooltip("The speed at which max penetration will be reached. The speed will also be clamped to this so that (for example) hitscan weapons don't have infinite speed.")]
        private float m_PenetrationSpeed = 850f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Randomises the deflected bullet direction within this cone angle")]
        private float m_MaxScatterAngle = 15f;

        [SerializeField, Tooltip("Uses the surface system to show a bullet hit effect on exit. Set this to zero if you don't want it to happen.")]
        private float m_ExitEffectSize = 1f;

        [Header("Projectile")]

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, Tooltip("The gravity for the projectile.")]
        private float m_Gravity = 9.8f;

        private const float k_1mm = 0.001f;

        public enum OnFailToPenetrate
        {
            InitialEffect,
            SecondHitEffect,
            Both
        }

        public override bool isModuleValid
        {
            get { return m_InitialHitEffect != null && m_SecondHitEffect != null; }
        }

        void OnValidate()
        {
            if (m_InitialHitEffect == this)
            {
                Debug.LogError("You want infinitely recursive bullet penetration? Are you insane??!?");
                m_InitialHitEffect = null;
            }
            if (m_SecondHitEffect == this)
            {
                Debug.LogError("You want infinitely recursive bullet penetration? Are you insane??!?");
                m_SecondHitEffect = null;
            }
            if (m_ExitEffectSize < 0f)
                m_ExitEffectSize = 0f;
            m_MaxPenetration = Mathf.Clamp(m_MaxPenetration, 0.01f, 1f);
            m_MaxScatterAngle = Mathf.Clamp(m_MaxScatterAngle, 0f, 45f);
        }

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Clamp max speed and calculate max penetration
            speed = Mathf.Clamp(speed, 0f, m_PenetrationSpeed);
            float maxPenetration = m_MaxPenetration * speed / m_PenetrationSpeed;

            // Calculate the ricochet direction
            Vector3 outDirection = rayDirection;

            // Add randomisation
            if (m_MaxScatterAngle > 0.01f)
            {
                Quaternion randomRot = UnityEngine.Random.rotationUniform;
                outDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxScatterAngle / 360f) * outDirection;

                // Check it's not pointing back out of the surface
                var dot = Vector3.Dot(outDirection, hit.normal);
                if (dot > 0f)
                    outDirection = (outDirection - hit.normal * (0.01f + dot)).normalized;
            }

            // Move into the surface by max penetration amount and reverse test (must hit the opposite side of the object to penetrate
            RaycastHit exitHit;
            if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(hit.point + outDirection * maxPenetration, -outDirection), out exitHit, maxPenetration, m_Layers, null, QueryTriggerInteraction.Ignore))
            {
                // Apply the hit effect
                if (m_InitialHitEffect != null)
                    m_InitialHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

                // Get the exiting bullet speed based on distance penetrated
                speed *= exitHit.distance / maxPenetration;

                // Spawn & fire the penetrated projectile
                IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                InitialisePenetrationProjectile(projectile);
                projectile.Fire(exitHit.point, outDirection * speed, m_Gravity, m_SecondHitEffect, null, m_Layers, damageSource);
                projectile.gameObject.SetActive(true);

                // Show exit effect
                if (m_ExitEffectSize > Mathf.Epsilon)
                    SurfaceManager.ShowBulletHit(exitHit, rayDirection, m_ExitEffectSize, exitHit.rigidbody != null);
            }
            else
            {
                // Apply the hit effect
                switch (m_OnFailToPenetrate)
                {
                    case OnFailToPenetrate.InitialEffect:
                        {
                            if (m_InitialHitEffect != null)
                                m_InitialHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);
                        }
                        break;
                    case OnFailToPenetrate.SecondHitEffect:
                        {
                            if (m_SecondHitEffect != null)
                                m_SecondHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);
                        }
                        break;
                    case OnFailToPenetrate.Both:
                        {
                            if (m_InitialHitEffect != null)
                                m_InitialHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);
                            if (m_SecondHitEffect != null)
                                m_SecondHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);
                        }
                        break;
                }
            }
        }

        protected virtual void InitialisePenetrationProjectile(IProjectile projectile)
        { }
    }
}