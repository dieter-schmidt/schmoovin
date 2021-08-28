using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-physicsbulletcasing.html")]
	[RequireComponent (typeof (Rigidbody))]
	[RequireComponent (typeof (MeshFilter))]
	[RequireComponent (typeof (PooledObject))]
	public class PhysicsBulletCasing : MonoBehaviour, IBulletCasing
	{
		[SerializeField, RequiredObjectProperty, Tooltip("The detail mesh to show while the bullet is in the first person view.")]
		private Mesh m_DetailMesh = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The low poly mesh to switch to when not in the first person view.")]
		private Mesh m_LowPolyMesh = null;

        [SerializeField, Tooltip("How long should the casing remain before being returned to the pool.")]
		private float m_Lifespan = 30f;

		private Rigidbody m_RigidBody = null;
        private MeshFilter m_MeshFilter = null;
        private PooledObject m_PooledObject = null;
        private IEnumerator m_Coroutine = null;
        private Vector3 m_Velocity = Vector3.zero;
        private Vector3 m_Angular = Vector3.zero;
        private float m_Timer = 0f;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Lifespan < 10f)
                m_Lifespan = 10f;
        }
#endif

        void Awake ()
		{
			m_RigidBody = GetComponent<Rigidbody> ();
			m_MeshFilter = GetComponent<MeshFilter> ();
			m_PooledObject = GetComponent<PooledObject> ();
			gameObject.layer = PhysicsFilter.LayerIndex.Effects;
		}

		public void Eject (Vector3 velocity, Vector3 angular, bool player)
		{
            m_Velocity = velocity;
            m_Angular = angular;

            // Stop existing lifespan coroutine (in case pool empty, so oldest living grabbed)
            if (m_Coroutine != null)
				StopCoroutine (m_Coroutine);

            if (player)
            {
                // Set to animated
                m_MeshFilter.mesh = m_DetailMesh;
                m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                m_RigidBody.isKinematic = true;
                m_RigidBody.detectCollisions = false;

                // Start animated coroutine
                m_Coroutine = AnimatedCoroutine();
                StartCoroutine(m_Coroutine);
            }
            else
            {
                // Set to physics
                m_MeshFilter.mesh = m_LowPolyMesh;
                m_RigidBody.isKinematic = false;
                m_RigidBody.detectCollisions = true;
                m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                m_RigidBody.interpolation = RigidbodyInterpolation.None;
                m_RigidBody.velocity = m_Velocity;
                m_RigidBody.angularVelocity = m_Angular;
            }
		}

        IEnumerator AnimatedCoroutine ()
        {
            Transform t = transform;

            m_Timer = 0f;
            while (m_Timer < 0.25f)
            {
                yield return null;

                // Move the object
                m_RigidBody.MovePosition(m_RigidBody.position + m_Velocity * Time.deltaTime);
                m_Velocity += Physics.gravity * Time.deltaTime;

                // Rotate the objet
                m_RigidBody.MoveRotation(Quaternion.Euler(m_Angular * Time.deltaTime) * m_RigidBody.rotation);
            }

            // Set to physics
            t.localScale = Vector3.one;
            m_MeshFilter.mesh = m_LowPolyMesh;
            m_RigidBody.isKinematic = false;
            m_RigidBody.detectCollisions = true;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_RigidBody.interpolation = RigidbodyInterpolation.None;
            m_RigidBody.velocity = m_Velocity;
            m_RigidBody.angularVelocity = m_Angular;

            // release coroutine
            m_Coroutine = null;
        }

        void Update ()
        {
            m_Timer += Time.deltaTime;

            if (m_Timer > m_Lifespan)
            {
                m_PooledObject.ReturnToPool();
                m_Coroutine = null;
            }
        }
	}
}
