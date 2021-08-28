using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class OrderedSpawnPointGroup : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the spawn points be registered immediately on awake?")]
        private bool m_RegisterOnAwake = true;

        [SerializeField, Tooltip("")]
        private SpawnPoint[] m_SpawnPoints = { };

#if UNITY_EDITOR
        public SpawnPoint[] spawnPoints
        {
            get { return m_SpawnPoints; }
        }

        void OnValidate()
        {
            int nullSpawns = 0;
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] == null)
                    ++nullSpawns;
            }

            if (nullSpawns > 0)
            {
                var temp = new List<SpawnPoint>(m_SpawnPoints.Length - nullSpawns);
                for (int i = 0; i < m_SpawnPoints.Length; ++i)
                {
                    if (m_SpawnPoints[i] != null)
                        temp.Add(m_SpawnPoints[i]);
                }
                m_SpawnPoints = temp.ToArray();
            }
        }
#endif

        void Awake()
        {
            if (m_RegisterOnAwake)
                Register();
        }

        void OnEnable()
        {
            if (m_RegisterOnAwake)
                Register();
        }

        void OnDisable()
        {
            Unregister();
        }

        public void Register()
        {
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] != null)
                    m_SpawnPoints[i].Register();
            }
        }

        public void Unregister()
        {
            for (int i = 0; i < m_SpawnPoints.Length; ++i)
            {
                if (m_SpawnPoints[i] != null)
                    m_SpawnPoints[i].Unregister();
            }
        }
    }
}