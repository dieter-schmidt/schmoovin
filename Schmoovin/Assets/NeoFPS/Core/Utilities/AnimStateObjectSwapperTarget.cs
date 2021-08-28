using System;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-animstateobjectswappertarget.html")]
	[RequireComponent (typeof (Rigidbody))]
	public class AnimStateObjectSwapperTarget : MonoBehaviour
	{
		[SerializeField, Tooltip("The object to swap.")]
        Transform m_TargetTransform = null;

        [SerializeField, Tooltip("The pooled object to swap the target object with.")]
        PooledObject m_PooledObject = null;

        [SerializeField, Tooltip("The velocity to spawn the physics object at relative to the target object rotation.")]
        Vector3 m_SpawnVelocity = new Vector3 (0f, 1f, 0f);

		[SerializeField, Tooltip("The angular velocity of the spawned physics object.")]
        Vector3 m_SpawnAngularVelocity = new Vector3 (90f, 0f, 0f);

		public void Swap ()
		{
			Rigidbody leverRb = PoolManager.GetPooledObject<Rigidbody> (m_PooledObject, m_TargetTransform.position, m_TargetTransform.rotation);
			if (leverRb != null)
			{
				leverRb.velocity = m_TargetTransform.rotation * m_SpawnVelocity;
				leverRb.angularVelocity = m_SpawnAngularVelocity * Mathf.Deg2Rad;
				m_TargetTransform.gameObject.SetActive (false);
			}
		}

		public void Reset ()
		{
			gameObject.SetActive (true);
		}
	}
}

