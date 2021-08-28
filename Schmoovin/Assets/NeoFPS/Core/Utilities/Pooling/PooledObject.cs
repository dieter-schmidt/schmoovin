using NeoSaveGames.Serialization;
using System;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-pooledobject.html")]
	public class PooledObject : MonoBehaviour
	{
		private Transform m_LocalTransform = null;
        private Transform m_PoolTransform = null;
        private NeoSerializedGameObject m_LocalNsgo = null;
        private NeoSerializedGameObject m_PoolNsgo = null;

        public Transform poolTransform
		{
			get { return m_PoolTransform; }
			set
            {
                if (m_PoolTransform == null)
                {
                    m_PoolTransform = value;
                    m_PoolNsgo = value.GetComponent<NeoSerializedGameObject>();
                }
                else
                    Debug.LogError("Cannot change pool transform for object once it is already set.", gameObject);
            }
		}

		void Awake ()
		{
			m_LocalTransform = transform;
            m_LocalNsgo = GetComponent<NeoSerializedGameObject>();
        }

		public void ReturnToPool ()
		{
			if (poolTransform == null)
				Destroy (gameObject);
			else
			{
				gameObject.SetActive (false);
                if (m_LocalNsgo != null && m_PoolNsgo != null)
                    m_LocalNsgo.SetParent(m_PoolNsgo);
                else
                    m_LocalTransform.SetParent(poolTransform);
			}
		}
	}
}

