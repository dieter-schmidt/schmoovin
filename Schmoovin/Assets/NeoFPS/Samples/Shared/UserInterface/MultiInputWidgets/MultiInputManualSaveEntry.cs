using NeoSaveGames;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    public class MultiInputManualSaveEntry : MultiInputWidget, ISubmitHandler, IPointerClickHandler
    {
        [SerializeField, Tooltip("The raw image component to display the loaded thumbnail")]
        private RawImage m_ThumbnailImage = null;
        [SerializeField, Tooltip("The UI text to display the save title")]
        private Text m_TitleText = null;
        [SerializeField, Tooltip("The UI text to display the save date")]
        private Text m_DateText = null;

        [Header("Spinner")]
        [SerializeField, Tooltip("The spinner rotates to show that the save meta data is being loaded")]
        private Transform m_Spinner = null;
        [SerializeField, Tooltip("The turn rate of the loading spinner")]
        private float m_DegreesPerSecond = 90f;

        private SaveFileMetaData m_MetaData = null;

        protected override bool customHeight
        {
            get { return true; }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_ThumbnailImage == null)
                m_ThumbnailImage = GetComponentInChildren<RawImage>();
        }
#endif

        public void Initialise()
        {
            // Set text to placeholder
            if (m_TitleText != null)
                m_TitleText.text = "Create A New Save";
            if (m_DateText != null)
                m_DateText.text = "???";

            // Assign the metadata
            m_MetaData = null;
        }

        public void Initialise(SaveFileMetaData meta)
        {
            // Set text to placeholder
            if (m_TitleText != null)
                m_TitleText.text = "???";
            if (m_DateText != null)
                m_DateText.text = "Date: ???";

            // Assign the metadata
            m_MetaData = meta;

            // Wait for meta to load
            StartCoroutine(WaitForLoad());
        }

        public void OnSubmit(BaseEventData eventData)
        {
            OnSaveGame();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSaveGame();
        }

        void OnSaveGame()
        {
            PlayAudio(MenuAudio.ClickValid);
#if UNITY_STANDALONE
            var mainScene = SaveGameManager.mainScene;
            if (mainScene != null)
            {
                TextFieldPopup.ShowPopup(
                    "Enter a name for the new save:",
                    mainScene.displayName,
                    "Save Game",
                    (string saveName) =>
                    {
                        if (m_MetaData != null)
                            SaveGameManager.SaveGame(saveName, m_MetaData.saveFile);
                        else
                            SaveGameManager.SaveGame(saveName);
                        HideMenu();
                    }
                );
            }
#else
            var mainScene = SaveGameManager.mainScene;
            if (mainScene != null)
            {
                if (m_MetaData == null)
                {
                    SaveGameManager.SaveGame(mainScene.displayName);
                    HideMenu();
                }
                else
                {
                    ConfirmationPopup.ShowPopup(
                        "Are you sure you want to overwrite the existing save?",
                        () =>
                        {
                            SaveGameManager.SaveGame(mainScene.displayName);
                            HideMenu();
                        },
                        null);
                }
            }
#endif
        }

        void HideMenu()
        {
            var m = GetComponentInParent<BaseMenu>();
            m.HidePanel();
        }

        IEnumerator WaitForLoad()
        {
            // Rotate spinner until loaded
            while (!m_MetaData.loaded)
            {
                if (m_Spinner != null)
                    m_Spinner.Rotate(0f, 0f, m_DegreesPerSecond * Time.deltaTime, Space.Self);
                yield return null;
            }

            // Set title
            if (m_TitleText != null)
                m_TitleText.text = m_MetaData.title;
            // Set date & time
            if (m_DateText != null)
                m_DateText.text = string.Format("{0} {1}", m_MetaData.saveTime.ToShortDateString(), m_MetaData.saveTime.ToLongTimeString());
            // Set thumbnail
            if (m_ThumbnailImage != null && m_MetaData.thumbnail != null)
                m_ThumbnailImage.texture = m_MetaData.thumbnail;
            // Disable spinner
            if (m_Spinner != null)
                m_Spinner.gameObject.SetActive(false);
        }
    }
}
