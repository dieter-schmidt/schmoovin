using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections.Generic;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-explosion.html")]
    [RequireComponent(typeof(PooledObject))]
    public class PooledExplosion : MonoBehaviour, IDamageSource, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The valid collision layers the explosion will affect")]
        private LayerMask m_CollisionLayers = PhysicsFilter.Masks.BulletBlockers;

        [SerializeField, Tooltip("Duration the object should remain active before being returned to the pool.")]
        private float m_Lifetime = 2f;

        [Header("Damage")]

        [SerializeField, Tooltip("A description of the damage, used when logging and displaying game events.")]
        private string m_PrintableName = "Explosion";

        [SerializeField, Tooltip("The damage type the explosion applies (enables filtering damage types).")]
        private DamageType m_DamageType = DamageType.Explosion;

        [SerializeField, Tooltip("The radius of the explosion")]
        private float m_Radius = 1f;

        [Header("Shake")]

        [SerializeField, Tooltip("The strength of the camera (and other) shake due to the explosion.")]
        private float m_ShakeStrength = 0.5f;

        [SerializeField, Tooltip("The inner shake radius of the explosion. Any shake handlers within this radius will be affected at full strength, falling off to 0 outside this based on the falloff distance.")]
        private float m_ShakeInnerRadius = 10f;

        [SerializeField, Tooltip("The distance beyond the inner radius where the shake effect drops off to 0.")]
        private float m_ShakeFalloffDistance = 10f;

        [SerializeField, Tooltip("The distance beyond the inner radius where the shake effect drops off to 0.")]
        private float m_ShakeDuration = 0.75f;

        const int k_MaxHits = 128;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(8);
        private static Collider[] s_HitColliders = new Collider[k_MaxHits];
        private static List<DamageHandlerInfo> s_DetectedDamageHandlers = new List<DamageHandlerInfo>(32);
        private static List<ImpactHandlerInfo> s_DetectedImpactHandlers = new List<ImpactHandlerInfo>(32);

        private PooledObject m_PooledObject = null;
        private float m_Timer = 0f;

        protected struct DamageHandlerInfo
        {
            public IDamageHandler damageHandler { get; set; }
            public float falloff { get; set; }

            public DamageHandlerInfo(IDamageHandler h, float f)
            {
                damageHandler = h;
                falloff = f;
            }
        }

        protected struct ImpactHandlerInfo
        {
            public Collider collider;
            public IImpactHandler impactHandler;
            public float falloff;
            public Vector3 direction;

            public ImpactHandlerInfo(Collider c, IImpactHandler h, float f, Vector3 dir)
            {
                collider = c;
                impactHandler = h;
                falloff = f;
                direction = dir;
            }
        }

        public DamageType damageType
        {
            get { return m_DamageType; }
            set { m_DamageType = value; }
        }

        public float radius
        {
            get { return m_Radius; }
            set { m_Radius = value; }
        }

        protected float maxDamage
        {
            get;
            private set;
        }

        protected float maxForce
        {
            get;
            private set;
        }

        protected Vector3 explosionCenter
        {
            get;
            private set;
        }

        protected virtual bool raycastCheck
        {
            get { return true; }
        }

        protected virtual void OnValidate()
        {
            if (m_Radius < 0.1f)
                m_Radius = 0.1f;
            if (m_ShakeInnerRadius < 0.1f)
                m_ShakeInnerRadius = 0.1f;
            if (m_ShakeFalloffDistance < 0f)
                m_ShakeFalloffDistance = 0f;
            m_ShakeStrength = Mathf.Clamp(m_ShakeStrength, 0f, 10f);
            m_Lifetime = Mathf.Clamp(m_Lifetime, 0.25f, 100f);
        }

        private void Awake()
        {
            m_PooledObject = GetComponent<PooledObject>();
        }

        private void OnEnable()
        {
            m_Timer = 0f;
        }

        private void Update()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer > m_Lifetime)
                m_PooledObject.ReturnToPool();
        }

        public virtual void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            this.maxDamage = maxDamage;
            this.maxForce = maxForce;

            // Set up damage source
            if (source != null)
            {
                controller = source.controller;
                m_OutDamageFilter = new DamageFilter(m_DamageType, source.outDamageFilter.GetTeamFilter());
            }
            else
            {
                controller = null;
                m_OutDamageFilter = new DamageFilter(m_DamageType, DamageTeamFilter.All);
            }

            explosionCenter = transform.position;

            // Check for colliders inside explosion radius
            int overlaps = Physics.OverlapSphereNonAlloc(explosionCenter, m_Radius, s_HitColliders);
            for (int i = 0; i < overlaps; ++i)
            {
                // Check if hit should be ignored
                if (ignoreRoot != null)
                {
                    bool ignore = false;

                    Transform itr = s_HitColliders[i].transform;
                    while (itr != null)
                    {
                        if (itr == ignoreRoot)
                        {
                            ignore = true;
                            break;
                        }
                        itr = itr.parent;
                    }

                    if (ignore)
                        continue;
                }

                CheckCollider(s_HitColliders[i], explosionCenter);
            }

            // Apply damage
            for (int i = 0; i < s_DetectedDamageHandlers.Count; ++i)
                ApplyExplosionDamageEffect(s_DetectedDamageHandlers[i]);
            s_DetectedDamageHandlers.Clear();

            // Apply force
            for (int i = 0; i < s_DetectedImpactHandlers.Count; ++i)
            {
                ApplyExplosionForceEffect(s_DetectedImpactHandlers[i], explosionCenter);
            }
            s_DetectedImpactHandlers.Clear();

            // Apply shake
            if (m_ShakeStrength > 0f)
                ShakeHandler.Shake(explosionCenter, m_ShakeInnerRadius, m_ShakeFalloffDistance, m_ShakeStrength, m_ShakeDuration);
        }

        protected virtual void CheckCollider(Collider c, Vector3 explosionCenter)
        {
            Vector3 targetPosition = c.bounds.center;//c.transform.position;
            Vector3 direction = targetPosition - explosionCenter;
            float distance = direction.magnitude;

            // Raycast against collider if required
            RaycastHit hit;
            bool gatherDamangeHandlers = false;
            if (raycastCheck)
            {
                if (PhysicsExtensions.RaycastNonAllocSingle(new Ray(explosionCenter, direction / distance), out hit, m_Radius, m_CollisionLayers, null, QueryTriggerInteraction.Ignore) && hit.collider == c)
                {
                    distance = hit.distance;
                    gatherDamangeHandlers = true;
                }
            }
            else
                gatherDamangeHandlers = true;

            // Get falloff
            float falloff = 1f - Mathf.Clamp01(distance / m_Radius);

            // Gather damage and impact handlers
            if (gatherDamangeHandlers)
            {
                // Get damage handlers for collider
                c.GetComponents(s_DamageHandlers);

                // Check each damage handler to see if already known
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                {
                    var damageHandler = s_DamageHandlers[i];
                    for (int j = 0; j < s_DetectedDamageHandlers.Count; ++j)
                    {
                        // if known, but lower falloff, update with this falloff
                        if (s_DetectedDamageHandlers[j].damageHandler == damageHandler)
                        {
                            if (s_DetectedDamageHandlers[j].falloff < falloff)
                                s_DetectedDamageHandlers[j] = new DamageHandlerInfo(damageHandler, falloff);

                            damageHandler = null;

                            break;
                        }
                    }

                    // Not known - track it
                    if (damageHandler != null)
                        s_DetectedDamageHandlers.Add(new DamageHandlerInfo(damageHandler, falloff));
                }

                // Check if impact handler / rigidbody is known
                int index = -1;
                for (int i = 0; i < s_DetectedImpactHandlers.Count; ++i)
                {
                    if (s_DetectedImpactHandlers[i].collider == c)
                    {
                        if (s_DetectedImpactHandlers[i].falloff < falloff)
                        {
                            var handler = s_DetectedImpactHandlers[i];
                            handler.falloff = falloff;
                            s_DetectedImpactHandlers[i] = handler;
                        }

                        index = i;
                        break;
                    }
                }

                // Not found
                if (index == -1)
                {
                    var impactHandler = c.GetComponent<IImpactHandler>();
                    var rigidbody = c.attachedRigidbody;
                    if (impactHandler != null || (rigidbody != null))// && !rigidbody.isKinematic))
                    {
                        var forceDirection = direction.normalized;
                        s_DetectedImpactHandlers.Add(new ImpactHandlerInfo(c, impactHandler, falloff, forceDirection));
                    }
                }
            }
        }

        protected virtual void ApplyExplosionDamageEffect(DamageHandlerInfo info)
        {
            float damage = maxDamage * info.falloff;
            // Apply damage
            if (info.damageHandler != null)
                info.damageHandler.AddDamage(damage, this);
        }

        protected virtual void ApplyExplosionForceEffect(ImpactHandlerInfo info, Vector3 center)
        {
            // Apply impact handler force
            if (info.impactHandler != null)
                info.impactHandler.HandlePointImpact(info.collider.bounds.center, info.direction * (info.falloff * maxForce));
            else
            {
                // Apply rigidbody force
                var rigidbody = info.collider.attachedRigidbody;
                if (rigidbody != null)
                    rigidbody.AddExplosionForce(maxForce, explosionCenter, m_Radius, 0.25f, ForceMode.Impulse);
            }
        }

        #region IDamageSource IMPLEMENTATION

        private DamageFilter m_OutDamageFilter = DamageFilter.DefaultAllTeams;
        public DamageFilter outDamageFilter
        { 
            get { return m_OutDamageFilter; }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get;
            private set;
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return m_PrintableName; }
        }

        #endregion

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_FilterKey = new NeoSerializationKey("outFilter");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_TimerKey, m_Timer);
            writer.WriteValue(k_FilterKey, outDamageFilter);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
            ushort filter;
            if (reader.TryReadValue(k_TimerKey, out filter, 0))
                outDamageFilter = filter;
        }

        #endregion
    }
}
