using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-so-fpsinventorydatabase.html")]
    [CreateAssetMenu(fileName = "FpsInventoryDbTable", menuName = "NeoFPS/Inventory/Database Table", order = NeoFpsMenuPriorities.inventory_database)]
    public class FpsInventoryDbTable : FpsInventoryDbTableBase
    {
        [SerializeField]
        private string m_TableName = "Table Name";

        [SerializeField]
        private FpsInventoryDatabaseEntry[] m_Entries = { };

        public override string tableName
        {
            get { return m_TableName; }
        }

        public override FpsInventoryDatabaseEntry[] entries
        {
            get { return m_Entries; }
        }

        public override int count
        {
            get { return m_Entries.Length; }
        }

        void OnValidate()
        {
            for (int i = 0; i < m_Entries.Length; ++i)
            {
                while (IsEntryIdInvalid(i) || !IsEntryIdUnique(i))
                    m_Entries[i].ResetID();
                //m_Entries[i].OnValidate();
            }
        }

        bool IsEntryIdInvalid(int index)
        {
            // 0-127 range is reserved for the constants
            return (m_Entries[index].id >= 0 && m_Entries[index].id <= 127);
        }

        bool IsEntryIdUnique(int index)
        {
            for (int i = 0; i < index; ++i)
            {
                if (m_Entries[i].id == m_Entries[index].id)
                    return false;
            }
            return true;
        }
    }
}