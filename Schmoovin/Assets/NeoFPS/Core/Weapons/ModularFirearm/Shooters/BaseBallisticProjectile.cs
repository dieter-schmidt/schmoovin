using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-baseballisticprojectile.html")]
    [RequireComponent(typeof(PooledObject))]
    public abstract  class BaseBallisticProjectile : MonoBehaviour, IProjectile, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The minimum distance before the projectile will appear.")]
        private float m_MinDistance = 0f;

        [SerializeField, Tooltip("Should the projectile rotate so it is always facing down the curve.")]
        private bool m_FollowCurve = false;

        [SerializeField, Tooltip("Forget the character's \"ignore root\", meaning it can detonate on the character collider.")]
        private bool m_ForgetIgnoreRoot = false;

        [SerializeField, Tooltip("Should the shot be tested against trigger colliders.")]
        private bool m_QueryTriggerColliders = false;

        [SerializeField, Tooltip("This will cause the projectile logic to be calculated instantly on firing. This will be more responsive, but bullet trails, etc may start far ahead of the gun depending on muzzle speed. Works best in first person.")]
        private bool m_Instant = true;

        private const float k_MaxDistance = 10000f;

        public event UnityAction onTeleported;
        public event UnityAction onHit;

        private Vector3 m_Velocity = Vector3.zero;
        private IAmmoEffect m_AmmoEffect = null;
        private IDamageSource m_DamageSource = null;
        private Transform m_IgnoreRoot = null;
        private LayerMask m_Layers;
        private PooledObject m_PooledObject = null;
        private bool m_PassedMinimum = false;
        private float m_Distance = 0f;
        private float m_LerpTime = -1f;
        private Vector3 m_LerpFromPosition = Vector3.zero;
        private Vector3 m_LerpToPosition = Vector3.zero;

        private RaycastHit m_Hit = new RaycastHit();

        public Transform localTransform
        {
            get;
            private set;
        }

        public float gravity
        {
            get;
            private set;
        }

        public float distanceTravelled
        {
            get { return m_Distance; }
        }
        
        protected MeshRenderer meshRenderer
        {
            get;
            private set;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MinDistance < 0f)
                m_MinDistance = 0f;
        }
#endif

        protected virtual void Awake()
        {
            localTransform = transform;
            m_PooledObject = GetComponent<PooledObject>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null && m_MinDistance > 0f)
                meshRenderer.enabled = false;
        }

        public virtual void Fire(Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false)
        {
            enabled = true;

            m_Velocity = velocity;
            this.gravity = gravity;
            m_AmmoEffect = effect;
            m_DamageSource = damageSource;
            m_IgnoreRoot = ignoreRoot;
            m_Layers = layers;

            localTransform.position = position;
            if (m_FollowCurve)
                localTransform.LookAt(position + velocity);

            // Reset distance
            m_Distance = 0;
            if (m_MinDistance > 0f)
                m_PassedMinimum = false;

            // Hide the mesh for the first frame
            if (meshRenderer != null && m_MinDistance > 0f)
                meshRenderer.enabled = false;

            // Store the starting position
            m_LerpToPosition = localTransform.position;

            // Update immediately
            if (m_Instant && !wait1)
                FixedUpdate();
        }
        
        protected virtual void FixedUpdate()
        {
            float time = Time.deltaTime;

            // Set position to target
            localTransform.position = m_LerpToPosition;

            // Reset interpolation for Update() frames before next fixed
            m_LerpTime = Time.fixedTime;
            m_LerpFromPosition = m_LerpToPosition;

            Vector3 desiredPosition = m_LerpFromPosition + (m_Velocity * time);
            float distance = Vector3.Distance(m_LerpFromPosition, desiredPosition);
            localTransform.LookAt(desiredPosition);

            // Enable renderer if travelled far enough (check based on from position due to lerp)
            if (!m_PassedMinimum && m_Distance > m_MinDistance)
            {
                m_PassedMinimum = true;

                if (m_ForgetIgnoreRoot)
                    m_IgnoreRoot = null;

                if (meshRenderer != null && meshRenderer.enabled == false)
                    meshRenderer.enabled = true;
            }

            Ray ray = new Ray(localTransform.position, localTransform.forward);
            var queryTriggers = m_QueryTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            if (PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, distance, m_Layers, m_IgnoreRoot, queryTriggers))
            {
                // Set lerp target
                m_LerpToPosition = m_Hit.point;

                // Update distance travelled
                m_Distance += m_Hit.distance;

                m_AmmoEffect.Hit(m_Hit, localTransform.forward, m_Distance, m_Velocity.magnitude, m_DamageSource);

                OnHit(m_Hit);

                if (onHit != null)
                    onHit();

                if (meshRenderer != null && meshRenderer.enabled == false)
                    meshRenderer.enabled = true;
            }
            else
            {
                // Set lerp target
                m_LerpToPosition = desiredPosition;

                // Update distance travelled
                m_Distance += distance;

                // Should the bullet just give up and retire?
                if (m_Distance > k_MaxDistance)
                    ReleaseProjectile();
            }

            // Apply forces to the projectile
            m_Velocity = ApplyForces(m_Velocity);
        }

        protected abstract void OnHit(RaycastHit hit);

        protected virtual Vector3 ApplyForces(Vector3 v)
        {
            // Add gravity
            v.y -= gravity * Time.deltaTime;
            return v;
        }

        protected void ReleaseProjectile()
        {
            m_PooledObject.ReturnToPool();
        }

        protected virtual void Update()
        {
            // Get lerp value
            float elapsed = Time.time - m_LerpTime;
            float lerp = elapsed / Time.fixedDeltaTime;

            // Lerp the position towards the target
            localTransform.position = Vector3.Lerp(m_LerpFromPosition, m_LerpToPosition, lerp);
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true)
        {
            // Update the position
            m_LerpFromPosition = m_LerpToPosition = position;
            localTransform.position = position;

            // Update the rotation and velocity direction
            if (relativeRotation)
            {
                localTransform.rotation *= rotation;
                m_Velocity = rotation * m_Velocity;
            }
            else
            {
                var inverse = Quaternion.Inverse(localTransform.rotation);
                localTransform.rotation = rotation;
                m_Velocity = inverse * rotation * m_Velocity;
            }

            // Fire event
            if (onTeleported != null)
                onTeleported();
        }

        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_LayersKey = new NeoSerializationKey("layers");
        private static readonly NeoSerializationKey k_DistanceKey = new NeoSerializationKey("distance");
        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
        private static readonly NeoSerializationKey k_DamageSourceKey = new NeoSerializationKey("damageSource");
        private static readonly NeoSerializationKey k_AmmoEffectKey = new NeoSerializationKey("ammoEffect");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_VelocityKey, m_Velocity);
            writer.WriteValue(k_LayersKey, m_Layers);
            writer.WriteValue(k_DistanceKey, m_Distance);
            writer.WriteValue(k_PositionKey, m_LerpToPosition);

            writer.WriteComponentReference(k_AmmoEffectKey, m_AmmoEffect, nsgo);
            writer.WriteComponentReference(k_DamageSourceKey, m_DamageSource, nsgo);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_VelocityKey, out m_Velocity, m_Velocity);
            reader.TryReadValue(k_DistanceKey, out m_Distance, m_Distance);

            int layerMask = m_Layers;
            if (reader.TryReadValue(k_LayersKey, out layerMask, layerMask))
                m_Layers = layerMask;

            Vector3 position;
            if (reader.TryReadValue(k_PositionKey, out position, Vector3.zero))
            {
                m_LerpFromPosition = position;
                m_LerpToPosition = position;
                localTransform.position = position;
            }

            IAmmoEffect serializedAmmoEffect;
            if (reader.TryReadComponentReference(k_AmmoEffectKey, out serializedAmmoEffect, nsgo))
                m_AmmoEffect = serializedAmmoEffect;
            IDamageSource serializedDamageSource;
            if (reader.TryReadComponentReference(k_DamageSourceKey, out serializedDamageSource, nsgo))
                m_DamageSource = serializedDamageSource;
        }
    }
}