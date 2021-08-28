using NeoSaveGames;
using NeoSaveGames.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace NeoFPS.Samples
{
	public class SpManualSave : MenuPanel
	{
        [SerializeField] private MultiInputManualSaveEntry m_Prototype = null;

        private MultiInputManualSaveEntry[] m_Entries = null;

        private void OnValidate()
        {
            if (m_Prototype == null)
                m_Prototype = GetComponentInChildren<MultiInputManualSaveEntry>();
        }

        private void OnDestroy()
        {
            // Cancel loading meta
            SaveGameManager.CancelLoadingFileMetaData();
        }

        public override void Show()
        {
            // Rebuild entries
            if (m_Prototype != null)
            {
                // Get metadata
                var meta = SaveGameManager.LoadFileMetaData(SaveGameTypeFilter.Manual);

                // Reset entries
                m_Entries = new MultiInputManualSaveEntry[meta.Length];
                for (int i = 0; i < meta.Length; ++i)
                {
                    var entry = Instantiate(m_Prototype, m_Prototype.transform.parent);
                    m_Entries[i] = entry;
                }

                base.Show();

                // Assign meta
                m_Prototype.Initialise();
                for (int i = 0; i < meta.Length; ++i)
                    m_Entries[i].Initialise(meta[i]);
            }
            else
                base.Show();
        }

        public override void Hide()
        {
            base.Hide();

            // Cancel loading meta
            SaveGameManager.CancelLoadingFileMetaData();

            if (m_Entries != null)
            {
                for (int i = 0; i < m_Entries.Length; ++i)
                    Destroy(m_Entries[i].gameObject);
                m_Entries = null;
            }
        }
    }
}