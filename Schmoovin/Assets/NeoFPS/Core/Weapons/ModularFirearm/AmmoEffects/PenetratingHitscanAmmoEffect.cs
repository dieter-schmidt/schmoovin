using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class PenetratingHitscanAmmoEffect : BaseAmmoEffect
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

        [SerializeField, Tooltip("The maximum distance that the weapon will register a hit (includes the distance travelled up to the penetration).")]
        private float m_MaxDistance = 1000f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Randomises the deflected bullet direction within this cone angle")]
        private float m_MaxScatterAngle = 15f;

        [SerializeField, Tooltip("Uses the surface system to show a bullet hit effect on exit. Set this to zero if you don't want it to happen.")]
        private float m_ExitEffectSize = 1f;

        [Header("Tracer")]

        [SerializeField, NeoPrefabField(typeof(IPooledHitscanTrail)), Tooltip("The optional pooled tracer prototype to use (must implement the IPooledHitscanTrail interface)")]
        private PooledObject m_TracerPrototype = null;

        [SerializeField, Tooltip("How size (thickness/radius) of the tracer line")]
        private float m_TracerSize = 0.05f;

        [SerializeField, Tooltip("How long the tracer line will last")]
        private float m_TracerDuration = 0.05f;
        
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
            bool secondaryHit = false;

            // Get the remaining shot distance
            float remainingDistance = m_MaxDistance - totalDistance;
            if (remainingDistance > m_MaxPenetration)
            {
                // Calculate the ricochet direction
                Vector3 outDirection = rayDirection;

                // Add randomisation
                if (m_MaxScatterAngle > 0.01f)
                {
                    Quaternion randomRot = UnityEngine.Random.rotationUniform;
                    outDirection = Quaternion.Slerp(Quaternion.identity, randomRot, m_MaxScatterAngle / 360f) * outDirection;

                    // Check it's not pointing into the surface
                    var dot = Vector3.Dot(outDirection, hit.normal);
                    if (dot > 0f)
                        outDirection = (outDirection - hit.normal * (0.01f + dot)).normalized;
                }

                // First, check what the penetrating shot would actually hit
                RaycastHit endHit;
                bool penetrationHit = false;
                if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(hit.point + outDirection * k_1mm, outDirection), out endHit, remainingDistance, m_Layers, null, QueryTriggerInteraction.Ignore))
                    penetrationHit = true;
                else
                {
                    endHit.point = hit.point + outDirection * remainingDistance;
                    endHit.distance = remainingDistance;
                }

                // Second, cast back to the original hit point to check obstacle thinkness
                RaycastHit returnHit;
                if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(endHit.point + outDirection * -k_1mm, -outDirection), out returnHit, endHit.distance, m_Layers, null, QueryTriggerInteraction.Ignore))
                {
                    // Did the return hit travel far enough before hitting something
                    if (returnHit.distance > (endHit.distance - m_MaxPenetration))
                    {
                        secondaryHit = true;

                        // Apply the hit effect
                        if (m_InitialHitEffect != null)
                            m_InitialHitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

                        // Apply the penetrating bullet hit
                        if (penetrationHit && m_SecondHitEffect != null)
                            m_SecondHitEffect.Hit(endHit, outDirection, endHit.distance + totalDistance, speed, damageSource);

                        // Draw the tracer
                        if (m_TracerPrototype != null)
                        {
                            var tracer = PoolManager.GetPooledObject<IPooledHitscanTrail>(m_TracerPrototype);
                            tracer.Show(returnHit.point, endHit.point, m_TracerSize, m_TracerDuration);
                        }

                        // Show exit effect
                        if (m_ExitEffectSize > Mathf.Epsilon)
                            SurfaceManager.ShowBulletHit(returnHit, rayDirection, m_ExitEffectSize, returnHit.rigidbody != null);
                    }
                }
            }

            if (!secondaryHit)
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
    }
}