using NeoSaveGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoFPS
{
    public class CheckpointTrigger : CharacterTriggerZone
    {
        [SerializeField, Tooltip("Should the checkpoint trigger fire multiple times (eg allow back-tracking).")]
        private bool m_OneShot = false;

        [SerializeField, Tooltip("Save the game progress using the auto save feature.")]
        private bool m_AutoSave = true;

        [SerializeField, Tooltip("A list of spawn points to enable at this checkpoint.")]
        private SpawnPoint[] m_SpawnPoints = { };

        [SerializeField, Tooltip("Should previous spawn points be disabled (guaranteeing that the player spawns here).")]
        private bool m_DisableOldSpawns = true;

        private static CheckpointTrigger s_LastCheckpoint = null;
        private bool m_CheckpointActive = true;

        private void OnDestroy()
        {
            if (s_LastCheckpoint == this)
                s_LastCheckpoint = null;
        }

        protected override void OnCharacterEntered(ICharacter c)
        {
            base.OnCharacterEntered(c);

            if (m_CheckpointActive && s_LastCheckpoint != this)
            {
                // Record checkpoint (to prevent repeat firing)
                s_LastCheckpoint = this;

                if (m_SpawnPoints.Length > 0)
                {
                    // Disable old spawn points
                    if (m_DisableOldSpawns)
                    {
                        while (SpawnManager.spawnPoints.Count > 0)
                            SpawnManager.spawnPoints[0].gameObject.SetActive(false);
                    }

                    // Enable new spawn points
                    for (int i = 0; i < m_SpawnPoints.Length; ++i)
                    {
                        if (m_SpawnPoints[i] != null)
                            m_SpawnPoints[i].gameObject.SetActive(true);
                    }
                }

                // Save
                if (m_AutoSave)
                    SaveGameManager.AutoSave();

                // Deactivate if one-shot
                if (m_OneShot)
                    m_CheckpointActive = false;
            }
        }
    }
}