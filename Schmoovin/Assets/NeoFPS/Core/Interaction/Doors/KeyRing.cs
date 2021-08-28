using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    public class KeyRing : FpsInventoryItemBase, IKeyRing
    {
        [SerializeField, Tooltip("A list of key IDs this keyring starts with.")]
        private string[] m_StartingKeys = { };

        private HashSet<string> m_Keys = new HashSet<string>();
        private bool m_Initialised = false;

        //public string[] meKeys = { };

        public override int itemIdentifier
        {
            get { return FpsInventoryKey.KeyRing; }
        }

        void Start()
        {
            if (!m_Initialised)
            {
                m_Initialised = true;

                for (int i = 0; i < m_StartingKeys.Length; ++i)
                    m_Keys.Add(m_StartingKeys[i]);
            }
        }

        public void AddKey(string id)
        {
            m_Keys.Add(id);
            //meKeys = GetKeys();
        }

        public void RemoveKey(string id)
        {
            m_Keys.Remove(id);
            //meKeys = GetKeys();
        }

        public bool ContainsKey(string id)
        {
            return m_Keys.Contains(id);
        }

        public string[] GetKeys()
        {
            var result = new string[m_Keys.Count];
            m_Keys.CopyTo(result);
            return result;
        }

        public void Merge(IKeyRing other)
        {
            var otherKeys = other.GetKeys();
            for (int i = 0; i < otherKeys.Length; ++i)
                m_Keys.Add(otherKeys[i]);
            //meKeys = GetKeys();
        }

        public override void OnAddToInventory(IInventory i, InventoryAddResult addResult)
        {
            base.OnAddToInventory(i, addResult);
        }

        #region INeoSerializableComponent IMPLEMENTATON

        private static readonly NeoSerializationKey k_KeysKey = new NeoSerializationKey("keys");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            writer.WriteValues(k_KeysKey, GetKeys());
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            string[] results;
            if (reader.TryReadValues(k_KeysKey, out results, null) && results != null)
            {
                m_Keys.Clear();

                for (int i = 0; i < results.Length; ++i)
                    m_Keys.Add(results[i]);

                m_Initialised = true;
            }
        }

        #endregion
    }
}