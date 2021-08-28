using NeoSaveGames;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    public class MultiInputSaveBrowserEntry : MultiInputWidget, ISubmitHandler, IPointerClickHandler
    {
        [SerializeField, Tooltip("The raw image component to display the loaded thumbnail")]
        private RawImage m_ThumbnailImage = null;
        [SerializeField, Tooltip("The UI text to display the save type")]
        private Text m_TypeText = null;
        [SerializeField, Tooltip("The UI text to display the save date")]
        private Text m_DateText = null;
        [SerializeField, Tooltip("The UI rect of the load button (to check clicks)")]
        private RectTransform m_LoadButton = null;

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

        public void SetMetaData(SaveFileMetaData meta)
        {
            // Set text to placeholder
            label = "???";
            if (m_TypeText != null)
                m_TypeText.text = "Save Type: ???";
            if (m_DateText != null)
                m_DateText.text = "Date: ???";

            // Assign the metadata
            m_MetaData = meta;

            // Wait for meta to load
            StartCoroutine(WaitForLoad());
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (m_MetaData != null && m_MetaData.loaded)
            {
                PlayAudio(MenuAudio.ClickValid);
                SaveGameManager.LoadGame(m_MetaData.saveFile);
            }
            else
                PlayAudio(MenuAudio.ClickInvalid);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Check for increment / decrement buttons
            if (RectTransformUtility.RectangleContainsScreenPoint(m_LoadButton, eventData.pressPosition))
            {
                if (m_MetaData != null && m_MetaData.loaded)
                {
                    PlayAudio(MenuAudio.ClickValid);
                    SaveGameManager.LoadGame(m_MetaData.saveFile);
                }
                else
                    PlayAudio(MenuAudio.ClickInvalid);
            }
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
            label = m_MetaData.title;
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
    }
}
