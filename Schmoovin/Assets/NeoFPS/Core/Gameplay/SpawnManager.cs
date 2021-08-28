using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcharactersref-mb-spawnmanager.html")]
	public class SpawnManager : MonoBehaviour, INeoSerializableComponent
	{
		[SerializeField, Tooltip("How the next spawn point is chosen: **RoundRobin** picks each spawn point in sequence, **FirstValid** starting at the first registered spawwn point and iterating until a valid one is found, **Random** picked at random until a valid point is found.")]
		private SpawnMode m_SpawnMode = SpawnMode.RoundRobin;

        private static readonly NeoSerializationKey k_SpawnModeKey = new NeoSerializationKey("spawnMode");
        private static readonly NeoSerializationKey k_LastIndexKey = new NeoSerializationKey("lastIndex");

        public enum SpawnMode
		{
			RoundRobin,
			FirstValid,
			Random
		}

		void Awake ()
		{
			spawnMode = m_SpawnMode;

            // Check for null spawn points
            // (should only be possible in the editor, but no harm doing anyway)
            for (int i = m_SpawnPoints.Count - 1; i >= 0; --i)
            {
                if (m_SpawnPoints[i] == null)
                    m_SpawnPoints.RemoveAt(i);
            }
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_SpawnModeKey, (int)m_SpawnModeInternal);
            writer.WriteValue(k_LastIndexKey, m_LastIndex);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            int result = 0;
            if (reader.TryReadValue(k_SpawnModeKey, out result, 0))
                spawnMode = (SpawnMode)result;

            reader.TryReadValue(k_LastIndexKey, out m_LastIndex, m_LastIndex);
        }

        #region STATIC

        private static int m_LastIndex = -1;

		private static SpawnMode m_SpawnModeInternal = SpawnMode.RoundRobin;
		public static SpawnMode spawnMode
		{
			get { return m_SpawnModeInternal; }
			set { m_SpawnModeInternal = value; }
		}

		private static List<SpawnPoint> m_SpawnPoints = new List<SpawnPoint> ();
		public static List<SpawnPoint> spawnPoints
		{
			get { return m_SpawnPoints; }
		}

		public static void AddSpawnPoint (SpawnPoint sp)
		{
			if (sp != null)
				m_SpawnPoints.Add (sp);
		}

		public static void RemoveSpawnPoint (SpawnPoint sp)
		{
			if (m_SpawnPoints.Contains (sp))
				m_SpawnPoints.Remove (sp);
			// Move last index if it clashes?
		}

		public static ICharacter SpawnCharacter (ICharacter characterPrototype, IController controller, bool force, NeoSerializedScene scene = null)
		{
            // Get the next spawn point & spawn the character
            var spawnPoint = GetNextSpawnPoint(force);
            if (spawnPoint != null)
                return spawnPoint.SpawnCharacter(characterPrototype, controller, true, scene);

			return null;
		}

        public static SpawnPoint GetNextSpawnPoint(bool force)
        {
            switch (m_SpawnModeInternal)
            {
                case SpawnMode.FirstValid:
                    {
                        for (int i = 0; i < m_SpawnPoints.Count; ++i)
                        {
                            if (force || m_SpawnPoints[i].CanSpawnCharacter())
                                return m_SpawnPoints[i];
                        }
                    }
                    break;
                case SpawnMode.RoundRobin:
                    {
                        for (int i = 0; i < m_SpawnPoints.Count; ++i)
                        {
                            // Get wrapped index
                            int index = 1 + m_LastIndex + i;
                            while (index >= m_SpawnPoints.Count)
                                index -= m_SpawnPoints.Count;

                            // Check spawn
                            if (force || m_SpawnPoints[index].CanSpawnCharacter())
                            {
                                m_LastIndex = index;
                                return m_SpawnPoints[index];
                            }
                        }
                    }
                    break;
                case SpawnMode.Random:
                    {
                        // Clone list to check each one
                        List<SpawnPoint> untried = new List<SpawnPoint>(m_SpawnPoints.Count);
                        for (int i = 0; i < m_SpawnPoints.Count; ++i)
                            untried.Add(m_SpawnPoints[i]);
                        // Try at random until none left
                        while (untried.Count > 0)
                        {
                            // Get random index
                            int index = Random.Range(0, untried.Count);
                            // Spawn character
                            // Check spawn
                            if (force || untried[index].CanSpawnCharacter())
                                return untried[index];
                            else
                            {
                                // Remove invalid spawn point from pool
                                untried.RemoveAt(index);
                            }
                        }
                    }
                    break;
            }
            return null;
        }

        #endregion
    }
}