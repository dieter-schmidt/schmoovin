using UnityEngine;
using System.Collections.Generic;

namespace NeoFPS
{
    //[CreateAssetMenu(fileName = "FpsManager_InventoryDatabase", menuName = "NeoFPS/Managers/Inventory Database", order = NeoFpsMenuPriorities.manager_inventory)]
    [HelpURL("https://docs.neofps.com/manual/inputref-so-neofpsinventorydatabase.html")]
    public class NeoFpsInventoryDatabase : NeoFpsManager<NeoFpsInventoryDatabase>, ISerializationCallbackReceiver
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadInventoryDatabase()
        {
            GetInstance("FpsManager_InventoryDatabase", false);
        }

        [SerializeField, Tooltip("")]
        private FpsInventoryDbTableBase[] m_Tables = { };

        private Dictionary<int, FpsInventoryDatabaseEntry> m_Dictionary = new Dictionary<int, FpsInventoryDatabaseEntry>();

        public override bool IsValid()
        {
            return true;
        }

        void OnValidate()
        {
            // Strip out null entries
            int missing = 0;
            for (int i = 0; i < m_Tables.Length; ++i)
            {
                if (m_Tables[i] == null)
                    ++missing;
            }

            if (missing > 0)
            {
                var temp = new FpsInventoryDbTableBase[m_Tables.Length - missing];
                for (int itr = 0, i = 0; i < m_Tables.Length; ++i)
                {
                    if (m_Tables[i] != null)
                        temp[itr++] = m_Tables[i];
                }
                m_Tables = temp;
            }
        }

#if UNITY_EDITOR

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            for (int i = 0; i < m_Tables.Length; ++i)
                m_Tables[i].editorOnContentsChanged = OnTableChanged;
            OnTableChanged();
        }

        void OnTableChanged()
        {
            m_Dictionary = new Dictionary<int, FpsInventoryDatabaseEntry>();
            for (int i = 0; i < m_Tables.Length; ++i)
            {
                if (m_Tables[i] == null)
                    continue;

                var entries = m_Tables[i].entries;
                for (int j = 0; j < entries.Length; ++j)
                {
                    if (entries[j] != null)
                        m_Dictionary.Add(entries[j].id, entries[j]);
                }
            }
        }

        public static FpsInventoryDbTableBase[] tables
        {
            get
            {
                if (CheckInstance())
                    return instance.m_Tables;
                else
                    return null;
            }
        }

        public static FpsInventoryDatabaseEntry GetEntry(int id)
        {
            if (CheckInstance() && instance.m_Dictionary.TryGetValue(id, out FpsInventoryDatabaseEntry result))
                return result;
            else
                return null;
        }

        public static string GetEntryName(int id)
        {
            var entry = GetEntry(id);
            if (entry != null)
                return entry.displayName;
            else
                return string.Empty;
        }

        public static bool ContainsEntry(int id)
        {
            return CheckInstance() && instance.m_Dictionary.ContainsKey(id);
        }

        public static FpsInventoryDatabaseEntry GetByName(string n)
        {
            if (CheckInstance())
            {
                foreach (var entry in instance.m_Dictionary.Values)
                {
                    if (entry.displayName == n)
                        return entry;
                }
            }

            return null;
        }

        public static bool CheckInstance()
        {
            GetInstance("FpsManager_InventoryDatabase", false);
            return instance != null;
        }

#else

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            m_Dictionary = new Dictionary<int, FpsInventoryDatabaseEntry>();
            for (int i = 0; i < m_Tables.Length; ++i)
            {
                if (m_Tables[i] == null)
                    continue;

                var entries = m_Tables[i].entries;
                for (int j = 0; j < entries.Length; ++j)
                {
                    if (entries[j] != null)
                        m_Dictionary.Add(entries[j].id, entries[j]);
                }
            }
        }
        
        public static FpsInventoryDbTableBase[] tables
        {
            get { return instance.m_Tables; }
        }

        public static FpsInventoryDatabaseEntry GetEntry(int id)
        {
            if (instance.m_Dictionary.TryGetValue(id, out FpsInventoryDatabaseEntry result))
                return result;
            else
                return null;
        }

        public static string GetEntryName(int id)
        {
            var entry = GetEntry(id);
            if (entry != null)
                return entry.displayName;
            else
                return string.Empty;
        }

        public static bool ContainsEntry(int id)
        {
            return instance.m_Dictionary.ContainsKey(id);
        }

        public static FpsInventoryDatabaseEntry GetByName(string n)
        {
            foreach (var entry in instance.m_Dictionary.Values)
            {
                if (entry.displayName == n)
                    return entry;
            }

            return null;
        }
        
        public static bool CheckInstance()
        {
            Debug.LogError("NeoFpsInventoryDatabase.CheckInstance() should not be called from runtime code.");
            return true;
        }

#endif

    }
}