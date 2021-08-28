using NeoSaveGames;
using NeoSaveGames.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace NeoFPS.Samples
{
	public class SpLoadFromSave : MenuPanel
	{
        [SerializeField] private MultiInputSaveBrowserEntry m_Prototype = null;

        private MultiInputSaveBrowserEntry[] m_Entries = null;

        private void OnValidate()
        {
            if (m_Prototype == null)
                m_Prototype = GetComponentInChildren<MultiInputSaveBrowserEntry>();
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
                var meta = SaveGameManager.LoadFileMetaData(SaveGameTypeFilter.All);

                if (meta.Length > 0)
                {
                    // Reset entries
                    m_Entries = new MultiInputSaveBrowserEntry[meta.Length];
                    m_Entries[0] = m_Prototype;
                    m_Entries[0].gameObject.SetActive(true);
                    for (int i = 1; i < meta.Length; ++i)
                    {
                        var entry = Instantiate(m_Prototype, m_Prototype.transform.parent);
                        m_Entries[i] = entry;
                    }

                    base.Show();

                    // Assign meta
                    for (int i = 0; i < meta.Length; ++i)
                        m_Entries[i].SetMetaData(meta[i]);
                }
                else
                    m_Prototype.gameObject.SetActive(false);
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
                for (int i = 1; i < m_Entries.Length; ++i)
                    Destroy(m_Entries[i].gameObject);
                m_Entries = null;

                m_Prototype.gameObject.SetActive(false);
            }
        }
    }
}