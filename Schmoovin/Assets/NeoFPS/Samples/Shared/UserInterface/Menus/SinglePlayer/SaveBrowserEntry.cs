using NeoSaveGames;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    public class SaveBrowserEntry : MultiInputWidget, ISubmitHandler
    {
        [SerializeField, Tooltip("")]
        private RawImage m_ThumbnailImage = null;
        [SerializeField, Tooltip("")]
        private Text m_TitleText = null;
        [SerializeField, Tooltip("")]
        private Text m_TypeText = null;
        [SerializeField, Tooltip("")]
        private Text m_DateText = null;
        [SerializeField, Tooltip("The button which is clicked to load the save")]
        private MultiInputButton m_LoadButton = null;

        [Header("Spinner")]
        [SerializeField, Tooltip("The spinner rotates to show that the save meta data is being loaded")]
        private Transform m_Spinner = null;
        [SerializeField, Tooltip("The turn rate of the loading spinner")]
        private float m_DegreesPerSecond = 90f;

        private SaveFileMetaData m_MetaData = null;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_ThumbnailImage == null)
                m_ThumbnailImage = GetComponentInChildren<RawImage>();
            if (m_LoadButton == null)
                m_LoadButton = GetComponentInChildren<MultiInputButton>();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            // Add button event
            if (m_LoadButton != null)
                m_LoadButton.onClick.AddListener(OnLoadButtonClick);

            // Set text to placeholder
            if (m_TitleText != null)
                m_TitleText.text = "???";
            if (m_DateText != null)
                m_DateText.text = "???";
        }

        public void SetMetaData(SaveFileMetaData meta)
        {
            m_MetaData = meta;
            StartCoroutine(WaitForLoad());
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
            // Set type
            if (m_TypeText != null)
            {
                switch (m_MetaData.saveType)
                {
                    case SaveGameType.Quicksave:
                        m_TypeText.text = "Save Type: Quick-Save";
                        break;
                    case SaveGameType.Autosave:
                        m_TypeText.text = "Save Type: Autosave";
                        break;
                    case SaveGameType.Manual:
                        m_TypeText.text = "Save Type: Manual";
                        break;
                }
            }
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

        void OnLoadButtonClick()
        {
            if (m_MetaData != null && m_MetaData.loaded)
            {
                SaveGameManager.LoadGame(m_MetaData.saveFile);
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
