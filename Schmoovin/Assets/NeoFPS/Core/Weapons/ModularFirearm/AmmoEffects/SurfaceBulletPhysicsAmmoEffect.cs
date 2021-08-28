using NeoFPS.Constants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class SurfaceBulletPhysicsAmmoEffect : BaseAmmoEffect
    {
        [Header("HitEffect")]

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it hits something.")]
        private BaseAmmoEffect m_HitEffect = null;

        [Header("Surface Physics")]

        [SerializeField, RequiredObjectProperty, Tooltip("The per-surface bullet physics info")]
        private SurfaceBulletPhysicsInfo m_SurfacePhysics = null;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Randomises the deflected bullet direction within this cone angle, dependent on surface settings")]
        private float m_MaxRicochetScatter = 30f;

        [SerializeField, Tooltip("Randomises the penetrating bullet direction within this cone angle, dependent on surface settings")]
        private float m_MaxPenetrationDeflect = 15f;

        [SerializeField, Tooltip("Uses the surface system to show a bullet hit effect on exit. Set this to zero if you don't want it to happen.")]
        private float m_ExitEffectSize = 1f;

        [SerializeField, Tooltip("Should the bullet keep ricocheting / penetrating after the first time until it has slowed or travelled far enough")]
        private bool m_Recursive = true;

        [Header("Projectile")]

        [SerializeField, NeoPrefabField(typeof(IProjectile), required = true), Tooltip("The projectile to spawn.")]
        private PooledObject m_ProjectilePrefab = null;

        [SerializeField, Tooltip("The gravity for the projectile.")]
        private float m_Gravity = 9.8f;

        public override bool isModuleValid
        {
            get { return m_HitEffect != null && m_SurfacePhysics != null; }
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
        }

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Apply the hit effect
            if (m_HitEffect != null)
                m_HitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

            // Apply surface physics
            if (m_SurfacePhysics != null)
            {
                // Get the angle from the surface normal
                var angleFromNormal = Vector3.Angle(-rayDirection, hit.normal);

                // Get the hit surface
                FpsSurfaceMaterial surfaceMaterial = FpsSurfaceMaterial.Default;
                BaseSurface surface = hit.transform.GetComponent<BaseSurface>();
                if (surface != null)
                    surfaceMaterial = surface.GetSurface(hit);

                // Check if can penetrate
                if (m_SurfacePhysics.penetration[surfaceMaterial].canPenetrate &&
                    angleFromNormal < m_SurfacePhysics.penetration[surfaceMaterial].maxPenetrationAngle)
                {
                    // Get the max penetration based on speed
                    float maxPenetration = m_SurfacePhysics.penetration[surfaceMaterial].penetrationDepth * speed / m_SurfacePhysics.penetrationSpeed;

                    // Calculate the ricochet direction
                    Vector3 outDirection = rayDirection;

                    // Add randomisation
                    if (m_MaxPenetrationDeflect > 0.01f && m_SurfacePhysics.penetration[surfaceMaterial].maxDeflection > 0.0001f)
                    {
                        Quaternion randomRot = Random.rotationUniform;
                        outDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxPenetrationDeflect * m_SurfacePhysics.penetration[surfaceMaterial].maxDeflection / 360f) * outDirection;

                        // Check it's not pointing back out of the surface
                        var dot = Vector3.Dot(outDirection, hit.normal);
                        if (dot > 0f)
                            outDirection = (outDirection - hit.normal * (0.01f + dot)).normalized;
                    }
                    
                    // Move into the surface by max penetration amount and reverse test (must hit the opposite side of the object to penetrate
                    RaycastHit exitHit;
                    if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(hit.point + outDirection * maxPenetration, -outDirection), out exitHit, maxPenetration, m_Layers, null, QueryTriggerInteraction.Ignore))
                    {
                        // Get the exiting bullet speed based on distance penetrated
                        float speedMultiplier = exitHit.distance / maxPenetration;
                        speed *= speedMultiplier;

                        // Spawn & fire the penetrated projectile
                        IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                        InitialiseProjectile(projectile);
                        projectile.Fire(exitHit.point, outDirection * speed, m_Gravity, m_Recursive ? this : m_HitEffect, null, m_Layers, damageSource, true);
                        projectile.gameObject.SetActive(true);

                        // Show exit effect
                        if (m_ExitEffectSize > Mathf.Epsilon)
                            SurfaceManager.ShowBulletHit(exitHit, rayDirection, m_ExitEffectSize * speedMultiplier, exitHit.rigidbody != null);
                    }
                }
                else
                {
                    float minAngle = m_SurfacePhysics.ricochet[surfaceMaterial].minRicochetAngle;

                    // Check if can ricochet
                    if (m_SurfacePhysics.ricochet[surfaceMaterial].canRicochet &&
                        speed > m_SurfacePhysics.ricochet[surfaceMaterial].minRicochetSpeed && 
                        angleFromNormal > minAngle &&
                        hit.distance > 0.01f)
                    {
                        float strongAngle = m_SurfacePhysics.ricochet[surfaceMaterial].strongRicochetAngle;

                        // Check for ricochet based on angle
                        bool doRicochet = false;
                        float ricochetLerp = 0f;
                        if (angleFromNormal > strongAngle)
                        {
                            ricochetLerp = 1f;
                            doRicochet = true;
                        }
                        else
                        {
                            // Ricochet chance based on min vs strong angle
                            ricochetLerp = (angleFromNormal - minAngle) / (strongAngle - minAngle);
                            doRicochet = Random.Range(0f, 1f) < ricochetLerp;
                        }

                        // Ricochet is a-go
                        if (doRicochet)
                        {
                            // Get the out speed
                            float outSpeed = speed * Mathf.Lerp(m_SurfacePhysics.ricochet[surfaceMaterial].minSpeedMultiplier, m_SurfacePhysics.ricochet[surfaceMaterial].maxSpeedMultiplier, ricochetLerp);
                            
                            // Calculate the ricochet direction
                            Vector3 ricochetDirection = Vector3.Reflect(rayDirection, hit.normal);
                            float friction = m_SurfacePhysics.ricochet[surfaceMaterial].surfaceFriction;
                            if (friction > 0.0001f)
                            {
                                // Push towards normal
                                float deflectionLerp = 1f - (angleFromNormal / 100f);
                                deflectionLerp *= friction;
                                ricochetDirection = Vector3.Slerp(ricochetDirection, hit.normal, deflectionLerp);

                                // Scatter
                                Quaternion randomRot = Random.rotationUniform;
                                ricochetDirection = Quaternion.Slerp(Quaternion.identity, randomRot, friction * m_MaxRicochetScatter / 360f) * ricochetDirection;

                                // Check it's not pointing into the surface
                                var dot = Vector3.Dot(ricochetDirection, hit.normal);
                                if (dot < 0f)
                                    ricochetDirection = (ricochetDirection + hit.normal * (0.01f - dot)).normalized;
                            }

                            // Spawn and fire the projectile
                            IProjectile projectile = PoolManager.GetPooledObject<IProjectile>(m_ProjectilePrefab, false);
                            InitialiseProjectile(projectile);
                            projectile.Fire(hit.point, ricochetDirection * outSpeed, m_Gravity, m_Recursive ? this : m_HitEffect, null, m_Layers, damageSource, true);
                            projectile.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        protected virtual void InitialiseProjectile(IProjectile projectile)
        { }
    }
}