using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class RicochetHitscanAmmoEffect : BaseAmmoEffect
    {
        [Header("HitEffect")]

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it hits something.")]
        private BaseAmmoEffect m_HitEffect = null;
        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect), false), Tooltip("The effect of the ammo when it hits something after ricocheting.")]
        private BaseAmmoEffect m_RicochetEffect = null;

        [Header("Ricochet")]

        [SerializeField, Tooltip("The maximum distance that the weapon will register a hit (includes the distance travelled up to the ricochet).")]
        private float m_MaxDistance = 1000f;

        [SerializeField, Tooltip("The layers bullets will collide with.")]
        private LayerMask m_Layers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Range(0f, 1f), Tooltip("A blending value based on ricocheting directly along the hit normal (0) vs the reflection of the inbound ray (1)")]
        private float m_NormalVsDeflection = 1f;

        [SerializeField, Tooltip("Randomises the deflected bullet direction within this cone angle")]
        private float m_MaxScatterAngle = 15f;

        [SerializeField, Tooltip("The number of shots to split the ricochet into (1 is no split).")]
        private int m_SplitCount = 1;

        [Header("Tracer")]

        [SerializeField, NeoPrefabField(typeof(IPooledHitscanTrail)), Tooltip("The optional pooled tracer prototype to use (must implement the IPooledHitscanTrail interface)")]
        private PooledObject m_TracerPrototype = null;

        [SerializeField, Tooltip("How size (thickness/radius) of the tracer line")]
        private float m_TracerSize = 0.05f;

        [SerializeField, Tooltip("How long the tracer line will last")]
        private float m_TracerDuration = 0.05f;

        private RaycastHit m_Hit = new RaycastHit();

        public override bool isModuleValid
        {
            get { return m_HitEffect != null && m_RicochetEffect != null; }
        }

        void OnValidate()
        {
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
            m_SplitCount = Mathf.Clamp(m_SplitCount, 1, 10000);
            m_MaxScatterAngle = Mathf.Clamp(m_MaxScatterAngle, 0f, 45f);
        }

        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            // Apply the hit effect
            if (m_HitEffect != null)
                m_HitEffect.Hit(hit, rayDirection, totalDistance, speed, damageSource);

            // Get the remaining shot distanec
            float remainingDistance = m_MaxDistance - totalDistance;
            if (remainingDistance > 0.001f)
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

                    // Fire the ricochet
                    Vector3 hitPoint;
                    if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(hit.point, outDirection), out m_Hit, remainingDistance, m_Layers, null, QueryTriggerInteraction.Ignore))
                    {
                        hitPoint = m_Hit.point;
                        if (m_RicochetEffect != null)
                            m_RicochetEffect.Hit(m_Hit, outDirection, m_Hit.distance + totalDistance, speed, damageSource);
                    }
                    else
                        hitPoint = hit.point + outDirection * remainingDistance;

                    // Draw the tracer
                    if (m_TracerPrototype != null)
                    {
                        var tracer = PoolManager.GetPooledObject<IPooledHitscanTrail>(m_TracerPrototype);
                        tracer.Show(hit.point, hitPoint, m_TracerSize, m_TracerDuration);
                    }
                }
            }
        }
    }
}
