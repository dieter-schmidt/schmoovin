using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-pooledobjectswapper.html")]
	public class PooledObjectSwapper : MonoBehaviour
	{
        [SerializeField, Tooltip("The objects to swap.")]
        private Transform[] m_TargetTransforms = new Transform[0];

        [SerializeField, Tooltip("The pooled object to swap the target objects with.")]
		private PooledObject m_PooledObjectPrototype = null;

        [SerializeField, Tooltip("Swap the target objects if they are disabled.")]
		private bool m_SwapIfDisabled = false;

		public void Swap ()
		{
			for (int i = 0; i < m_TargetTransforms.Length; ++i)
			{
				if (m_TargetTransforms [i] == null || (!m_SwapIfDisabled && !m_TargetTransforms [i].gameObject.activeSelf))
					continue;
				
				PoolManager.GetPooledObject<PooledObject> (m_PooledObjectPrototype, m_TargetTransforms [i].position, m_TargetTransforms [i].rotation);
				m_TargetTransforms [i].gameObject.SetActive (false);
			}
		}
	}
}