using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeoSaveGames.Serialization;
using System.Collections.Generic;

namespace NeoSaveGames
{
    [HelpURL("https://docs.neofps.com/manual/")]
    public class SceneSaveInfo : NeoSerializedScene
    {
        [SerializeField, Tooltip("")]
        private string m_DisplayName = "Unnamed Scene";
        
        [SerializeField, Tooltip("The thumbnail to use for saved scenes")]
        private Texture2D m_ThumbnailTexture = null;

        private static SceneSaveInfo s_Current = null;

        public string displayName
        {
            get { return m_DisplayName; }
        }

        public Texture2D thumbnailTexture
        {
            get { return m_ThumbnailTexture; }
        }

        public override bool isMainScene
        {
            get { return true; }
        }

        public static SceneSaveInfo currentActiveScene
        {
            get { return s_Current; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (s_Current != null)
                Debug.LogError("Multiple SceneSaveInfo objects detected. This isn't allowed.");
            else
                s_Current = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (s_Current == this)
                s_Current = null;
        }

        protected override void WriteProperties(INeoSerializer writer)
        { }

        protected override void ReadProperties(INeoDeserializer reader)
        { }
    }
}

