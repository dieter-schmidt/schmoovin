using System;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-so-fpsinventorykeydbtable.html")]
    public class FpsInventoryKeyDbTable : FpsInventoryDbTableBase
    {
        [SerializeField, HideInInspector]
        private FpsInventoryDatabaseEntry[] m_Entries = { };

        public override string tableName
        {
            get { return "Scripted Constants"; }
        }

        public override FpsInventoryDatabaseEntry[] entries
        {
            get { return m_Entries; }
        }

        public override int count
        {
            get { return FpsInventoryKey.count - 1; }
        }

        void OnValidate()
        {
            if (m_Entries.Length != count)
                m_Entries = new FpsInventoryDatabaseEntry[count];
            for (int i = 0; i < count; ++i)
            {
                int key = i + 1;
                string n = FpsInventoryKey.names[key];
                if (m_Entries[i].id != key || m_Entries[i].displayName != n)
                    m_Entries[i] = new FpsInventoryDatabaseEntry(key, n);
            }
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}