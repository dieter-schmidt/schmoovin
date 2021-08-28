using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-pooledphysicsobjectswapper.html")]
	public class PooledPhysicsObjectSwapper : MonoBehaviour
	{
        [SerializeField, Tooltip("The objects to swap.")]
        private Transform[] m_TargetTransforms = new Transform[0];

        [SerializeField, Tooltip("The pooled object to swap the target objects with.")]
		private PooledObject m_PooledObjectPrototype = null;

        [SerializeField, Tooltip("Swap the target objects if they are disabled.")]
		private bool m_SwapIfDisabled = false;

        [SerializeField, Tooltip("The velocity to spawn the physics object at relative to the target object rotation.")]
		Vector3 m_SpawnVelocity = new Vector3 (0f, 1f, 0f);

        [SerializeField, Tooltip("The angular velocity of the spawned physics object.")]
		Vector3 m_SpawnAngularVelocity = new Vector3 (90f, 0f, 0f);

		public void Swap ()
		{
			for (int i = 0; i < m_TargetTransforms.Length; ++i)
			{
				if (m_TargetTransforms [i] == null || (!m_SwapIfDisabled && !m_TargetTransforms [i].gameObject.activeSelf))
					continue;

				Quaternion rotation = m_TargetTransforms [i].rotation;
				Rigidbody rb = PoolManager.GetPooledObject<Rigidbody> (m_PooledObjectPrototype, m_TargetTransforms [i].position, rotation);
				if (rb != null)
				{
					rb.velocity = rotation * m_SpawnVelocity;
					rb.angularVelocity = m_SpawnAngularVelocity;
				}

				m_TargetTransforms [i].gameObject.SetActive (false);
			}
		}
	}
}