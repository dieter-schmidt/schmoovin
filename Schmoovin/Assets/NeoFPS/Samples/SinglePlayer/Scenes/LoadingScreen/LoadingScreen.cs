using NeoSaveGames.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    public class LoadingScreen : MonoBehaviour
    {
        [Header("Hints")]
        [SerializeField, Tooltip("The UI text for the hints")]
        private Text m_HintText = null;
        [SerializeField, Tooltip("The object to enable if showing hints")]
        private GameObject m_HintObject = null;
        [SerializeField, Tooltip("The hints to display (chosen at random)")]
        private string[] m_Hints = new string[0];

        [Header("Save Warning")]
        [SerializeField, Tooltip("The object to enable if showing the save warning")]
        private GameObject m_SaveWarningObject = null;

        [Header("Audio Listener")]
        [SerializeField, Tooltip("The audio listener for the loading screen (disable when activating the main scene)")]
        private AudioListener m_AudioListener = null;

        private void Start()
        {
            // Check if first run
            bool firstRun = (m_HintObject == null) || PlayerPrefs.GetInt("loading.first", 1) == 1;
            PlayerPrefs.SetInt("loading.first", 0);

            if (firstRun)
                ShowSaveWarning();
            else
                ShowHint();

            NeoSceneManager.preSceneActivation += PreSceneActivation;
        }

        private void OnDestroy()
        {
            NeoSceneManager.preSceneActivation -= PreSceneActivation;
        }

        void PreSceneActivation()
        {
            StartCoroutine(DelayedDisableAudioListener());
        }

        IEnumerator DelayedDisableAudioListener()
        {
            yield return new WaitForEndOfFrame();
            if (m_AudioListener != null)
                m_AudioListener.enabled = false;
        }

        void ShowHint()
        {
            if (m_HintObject == null || m_Hints.Length == 0)
                ShowSaveWarning();
            else
            {
                // Hide save warning object
                if (m_SaveWarningObject != null)
                    m_SaveWarningObject.SetActive(false);

                // Show hint
                m_HintText.text = m_Hints[Random.Range(0, m_Hints.Length)];
                m_HintObject.SetActive(true);
            }
        }

        void ShowSaveWarning()
        {
            // Show save warning & hide hints
            if (m_HintObject != null)
                m_HintObject.SetActive(false);
            if (m_SaveWarningObject != null)
                m_SaveWarningObject.SetActive(true);
        }
    }
}