using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [Serializable]
    public class FpsInventoryDatabaseEntry
    {
        [SerializeField, Tooltip("The name for the item, as used in the HUD.")]
        private string m_DisplayName;
        [SerializeField, Tooltip("The unique ID for the inventory item.")]
        private int m_Id;

        public string displayName { get { return m_DisplayName; } }
        public int id { get { return m_Id; } }

        public FpsInventoryDatabaseEntry(int id, string displayName)
        {
            m_DisplayName = displayName;
            m_Id = id;
        }

        public void ResetID()
        {
            Debug.LogWarning("Resetting ID on inventory database entry: " + m_DisplayName);
            m_Id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }
}