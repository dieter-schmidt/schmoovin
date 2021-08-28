using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	[RequireComponent (typeof (PooledObject))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-guidedballisticprojectile.html")]
    public class GuidedBallisticProjectile : MonoBehaviour, IProjectile, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The minimum distance before the projectile will appear.")]
		private float m_MinVisibleDistance = 0f;

		[SerializeField, Tooltip("Forget the character's \"ignore root\", meaning it can detonate on the character collider.")]
        private bool m_ForgetIgnoreRoot = false;

        [SerializeField, Tooltip("Should the shot be tested against trigger colliders.")]
        private bool m_QueryTriggerColliders = false;

        [SerializeField, Tooltip("The time after the bullet hits an object before it is returned to the pool (allows trail renderers to complete).")]
        private float m_RecycleDelay = 0.5f;

        [SerializeField, Tooltip("This will cause the projectile logic to be calculated instantly on firing. This will be more responsive, but bullet trails, etc may start far ahead of the gun depending on muzzle speed. Works best in first person.")]
        private bool m_Instant = true;

        private const float k_MaxDistance = 10000f;

        public event UnityAction onTeleported;
        public event UnityAction onHit;

        private IGuidedProjectileTargetTracker m_Tracker = null;
        private IGuidedProjectileMotor m_Motor = null;
		private IAmmoEffect m_AmmoEffect = null;
        private IDamageSource m_DamageSource = null;
        private Transform m_IgnoreRoot = null;
        private LayerMask m_Layers;
        private PooledObject m_PooledObject = null;
        private MeshRenderer m_MeshRenderer = null;
        private Rigidbody m_Rigidbody = null;
        private AudioSource m_AudioSource = null;
        private bool m_Release = false;
        private bool m_PassedMinimum = false;
        private float m_Distance = 0f;
		private float m_LerpTime = -1f;
        private float m_Timeout = 0f;
        private Vector3 m_LerpFromPosition = Vector3.zero;
        private Vector3 m_LerpToPosition = Vector3.zero;

		private RaycastHit m_Hit = new RaycastHit();

		public Transform localTransform
		{
			get;
			private set;
		}

        public float distanceTravelled
        {
            get { return m_Distance; }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MinVisibleDistance < 0f)
                m_MinVisibleDistance = 0f;
            if (m_RecycleDelay < 0f)
                m_RecycleDelay = 0f;
        }
#endif

        protected virtual void Awake ()
		{
			localTransform = transform;
			m_PooledObject = GetComponent<PooledObject> ();
			m_MeshRenderer = GetComponentInChildren<MeshRenderer> ();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_AudioSource = GetComponent<AudioSource>();
            m_Tracker = GetComponent<IGuidedProjectileTargetTracker>();
            m_Motor = GetComponent<IGuidedProjectileMotor>();
            if (m_Motor == null)
                Debug.LogError("Guided projectile does not have a drive system component attached.");
            if (m_MeshRenderer != null)
                m_MeshRenderer.enabled = false;
            if (m_Rigidbody != null)
                m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

		public virtual void Fire (Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false)
		{
			m_AmmoEffect = effect;
			m_DamageSource = damageSource;
            m_IgnoreRoot = ignoreRoot;
            m_Layers = layers;

            // Set the starting position
            if (m_Rigidbody != null)
            {
                localTransform.position = position;
                localTransform.LookAt(position + velocity);

                m_Rigidbody.velocity = velocity;
                m_LerpFromPosition = m_LerpToPosition = position;
            }
            else
            {
                localTransform.position = position;
                localTransform.LookAt(position + velocity);
                m_LerpFromPosition = m_LerpToPosition = position;
            }

            // Reset distance
            m_Distance = 0;
            m_PassedMinimum = false;

            // Reset pooling
            m_Release = false;
            m_Timeout = m_RecycleDelay;

            // Hide the mesh for the first frame
            if (m_MeshRenderer != null)
                m_MeshRenderer.enabled = false;

            // Initialise driver
            m_Motor.SetStartingVelocity(velocity);

            // Update immediately
            if (m_Instant && !wait1)
                FixedUpdate ();
		}

		void FixedUpdate ()
		{
			if (m_Release)
			{
                if (m_RecycleDelay <= 0f)
                    ReleaseProjectile();
                else
                {
                    if (m_MeshRenderer != null && m_MeshRenderer.enabled)
                        m_MeshRenderer.enabled = false;
                    m_Timeout -= Time.deltaTime;
                    if (m_Timeout < 0f)
                        ReleaseProjectile();
                }
			}
			else
			{
				// Reset interpolation for Update() frames before next fixed
				m_LerpTime = Time.fixedTime;

                // Update position variables
                if (m_Rigidbody == null)
                    localTransform.position = m_LerpToPosition;
                m_LerpFromPosition = m_LerpToPosition;

                // Get target position and velocity
                Vector3 velocity = Vector3.zero;
                Vector3 targetPosition = Vector3.zero;
                if (m_Tracker != null && m_Tracker.GetTargetPosition(out targetPosition))
                    velocity = m_Motor.GetVelocity(m_LerpFromPosition, targetPosition);
                else
                    velocity = m_Motor.GetVelocity(m_LerpFromPosition);

                Vector3 offset = velocity * Time.deltaTime;
                Vector3 desiredPosition = m_LerpFromPosition + offset;
                float distance = offset.magnitude;

                // Enable renderer if travelled far enough (check based on from position due to lerp)
                if (!m_PassedMinimum && m_Distance > m_MinVisibleDistance)
                {
                    m_PassedMinimum = true;

                    if (m_ForgetIgnoreRoot)
                        m_IgnoreRoot = null;

                    // Display the render mesh
                    if (m_MeshRenderer != null && m_MeshRenderer.enabled == false)
                        m_MeshRenderer.enabled = true;

                    // Start the projectile audio
                    if (m_AudioSource != null)
                        m_AudioSource.Play();
                }

                Ray ray = new Ray (m_LerpFromPosition, offset.normalized);
                var queryTriggers = m_QueryTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
                if (PhysicsExtensions.RaycastNonAllocSingle (ray, out m_Hit, distance, m_Layers, m_IgnoreRoot, queryTriggers))
				{
                    // Set lerp target
                    m_LerpToPosition = m_Hit.point;
                    if (m_Rigidbody != null)
                        m_Rigidbody.MovePosition(m_Hit.point);

					// Release back to pool 
					m_Release = true;

                    // Update distance travelled
                    m_Distance += m_Hit.distance;

                    m_AmmoEffect.Hit (m_Hit, localTransform.forward, m_Distance, velocity.magnitude, m_DamageSource);

                    // Stop the projectile audio
                    if (m_AudioSource != null)
                        m_AudioSource.Stop();

                    OnHit();

                    if (onHit != null)
                        onHit();
                }
				else
				{
                    // Set lerp target
                    m_LerpToPosition = desiredPosition;
                    if (m_Rigidbody != null)
                        m_Rigidbody.MovePosition(desiredPosition);

					// Update distance travelled
					m_Distance += distance;

					// Should the bullet just give up and retire?
					if (m_Distance > k_MaxDistance)
						ReleaseProjectile ();
				}

                // Rotate the projectile
                var rot = Quaternion.LookRotation(velocity);
                if (m_Rigidbody != null)
                    m_Rigidbody.MoveRotation(rot);
                else
                    localTransform.rotation = rot;
            }
		}

        protected virtual void OnHit() {}

		void ReleaseProjectile ()
		{
			m_PooledObject.ReturnToPool ();
		}

		void Update ()
        {
            if (m_Rigidbody == null)
            {
                // Get lerp value
                float elapsed = Time.time - m_LerpTime;
                float lerp = elapsed / Time.fixedDeltaTime;

                // Lerp the position towards the target
                localTransform.position = Vector3.Lerp(m_LerpFromPosition, m_LerpToPosition, lerp);
            }
		}

        public void Teleport(Vector3 position, Quaternion rotation, bool relativeRotation = true)
        {
            // Notify modules
            if (m_Motor != null)
                m_Motor.OnTeleport(position, rotation, relativeRotation);

            // Update the position
            m_LerpFromPosition = m_LerpToPosition = position;
            localTransform.position = position;

            // Update the rotation and velocity direction
            if (relativeRotation)
                localTransform.rotation *= rotation;
            else
                localTransform.rotation = rotation;

            // Fire event
            if (onTeleported != null)
                onTeleported();
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_LayersKey = new NeoSerializationKey("layers");
        private static readonly NeoSerializationKey k_ReleaseKey = new NeoSerializationKey("release");
        private static readonly NeoSerializationKey k_DistanceKey = new NeoSerializationKey("distance");
        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
        private static readonly NeoSerializationKey k_DamageSourceKey = new NeoSerializationKey("damageSource");
        private static readonly NeoSerializationKey k_AmmoEffectKey = new NeoSerializationKey("ammoEffect");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_LayersKey, m_Layers);
            writer.WriteValue(k_DistanceKey, m_Distance);
            writer.WriteValue(k_PositionKey, m_LerpToPosition);

            writer.WriteComponentReference(k_AmmoEffectKey, m_AmmoEffect, nsgo);
            writer.WriteComponentReference(k_DamageSourceKey, m_DamageSource, nsgo);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
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
                if (m_Rigidbody != null)
                    m_Rigidbody.position = position;
            }

            IAmmoEffect serializedAmmoEffect;
            if (reader.TryReadComponentReference(k_AmmoEffectKey, out serializedAmmoEffect, nsgo))
                m_AmmoEffect = serializedAmmoEffect;
            IDamageSource serializedDamageSource;
            if (reader.TryReadComponentReference(k_DamageSourceKey, out serializedDamageSource, nsgo))
                m_DamageSource = serializedDamageSource;
        }

        #endregion
    }
}