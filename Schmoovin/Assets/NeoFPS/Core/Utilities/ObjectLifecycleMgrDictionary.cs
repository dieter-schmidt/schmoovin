using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-objectlifecyclemgrdictionary.html")]
    public class ObjectLifecycleMgrDictionary : MonoBehaviour, INeoSerializableComponent
	{
		[SerializeField, Tooltip("Should all the attached objects be enabled when this is.")]
		private bool m_ActiveOnEnabled = true;

        [SerializeField, Tooltip("Should all the attached objects be disabled when this is.")]
		private bool m_ActiveOnDisabled = true;

        [SerializeField, Tooltip("The objects being managed.")]
        private Entry[] m_Objects = new Entry[0];

        private static readonly NeoSerializationKey k_KeysKey = new NeoSerializationKey("keys");
        private static readonly NeoSerializationKey k_StatesKey = new NeoSerializationKey("states");
        private bool m_Initialised = false;

        [Serializable]
		public struct Entry
		{
			[Tooltip("A string key for the object.")]
			public string key;

			[Tooltip("The object to manage.")]
			public GameObject gameObject;
		}

        private Dictionary<string, GameObject> m_ObjectDictionary = null;
		public Dictionary<string, GameObject> objects
		{
			get { return m_ObjectDictionary; }
		}

		void Awake ()
		{
            if (!m_Initialised)
            {
                m_ObjectDictionary = new Dictionary<string, GameObject>(m_Objects.Length);
                for (int i = 0; i < m_Objects.Length; ++i)
                {
                    if (m_Objects[i].key != string.Empty && m_Objects[i].gameObject != null)
                        m_ObjectDictionary.Add(m_Objects[i].key, m_Objects[i].gameObject);
                }

                m_Initialised = true;
            }
		}

		void OnEnable ()
		{
			for (int i = 0; i < m_Objects.Length; ++i)
			{
				if (m_Objects [i].gameObject != null)
					m_Objects [i].gameObject.SetActive (m_ActiveOnEnabled);
			}
		}

		void OnDisable ()
		{
			for (int i = 0; i < m_Objects.Length; ++i)
			{
				if (m_Objects [i].gameObject != null)
					m_Objects [i].gameObject.SetActive (m_ActiveOnDisabled);
			}
		}

		// Simple functions for use with animation events

		public void EnableObject (string key)
		{
			GameObject go;
			if (m_ObjectDictionary.TryGetValue (key, out go))
				go.SetActive (true);
		}

		public void DisableObject (string key)
		{
			GameObject go;
			if (m_ObjectDictionary.TryGetValue (key, out go))
				go.SetActive (false);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            string[] keys = new string[m_ObjectDictionary.Count];
            bool[] states = new bool[m_ObjectDictionary.Count];

            int i = 0;
            foreach (var pair in m_ObjectDictionary)
            {
                keys[i] = pair.Key;
                states[i] = pair.Value.activeSelf;
                ++i;
            }

            writer.WriteValues(k_KeysKey, keys);
            writer.WriteValues(k_StatesKey, states);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Awake();

            string[] keys;
            reader.TryReadValues(k_KeysKey, out keys, new string[0]);
            bool[] states;
            reader.TryReadValues(k_StatesKey, out states, new bool[0]);

            for (int i = 0; i < keys.Length; ++i)
            {
                if (states[i])
                    EnableObject(keys[i]);
                else
                    DisableObject(keys[i]);
            }
        }
    }
}