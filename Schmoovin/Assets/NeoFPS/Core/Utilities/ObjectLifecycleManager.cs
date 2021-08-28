using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-objectlifecyclemanager.html")]
	public class ObjectLifecycleManager : MonoBehaviour, INeoSerializableComponent
	{
		[SerializeField, Tooltip("Should all the attached objects be enabled when this is.")]
		private bool m_ActiveOnEnabled = true;

		[SerializeField, Tooltip("Should all the attached objects be disabled when this is.")]
		private bool m_ActiveOnDisabled = true;

        [SerializeField, Tooltip("The objects being managed.")]
        private GameObject[] m_Objects = new GameObject[0];

        private static readonly NeoSerializationKey k_ObjectsEnabledKey = new NeoSerializationKey("objectsEnabled");

        public GameObject[] objects
		{
			get { return m_Objects; }
			set
			{
				m_Objects = value;
				objectsEnabled = m_ObjectsEnabled;
			}
		}

        bool m_ObjectsEnabled = false;
		public bool objectsEnabled
		{
			get { return m_ObjectsEnabled; }
			set
			{
				m_ObjectsEnabled = value;
				for (int i = 0; i < m_Objects.Length; ++i)
				{
					if (m_Objects [i] != null)
						m_Objects [i].SetActive (m_ObjectsEnabled);
				}
			}
		}

		void OnEnable ()
		{
			objectsEnabled = m_ActiveOnEnabled;
		}

		void OnDisable ()
		{
			objectsEnabled = m_ActiveOnDisabled;
		}

		// Simple functions for use with animation events
		public void EnableObjects ()
		{
			objectsEnabled = true;
		}

		public void DisableObjects ()
		{
			objectsEnabled = false;
		}

		public void EnableSpecificObject (GameObject o)
		{
			if (o != null)
				o.SetActive (true);
		}

		public void DisableSpecificObject (GameObject o)
		{
			if (o != null)
				o.SetActive (false);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ObjectsEnabledKey, objectsEnabled);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            bool boolResult = false;
            if (reader.TryReadValue(k_ObjectsEnabledKey, out boolResult, objectsEnabled))
                objectsEnabled = boolResult;
        }
    }
}