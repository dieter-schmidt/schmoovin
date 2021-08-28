using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ReadmePage : HubPage
    {
        private ReadmeAsset m_Readme = null;
        private ReadmeAssetEditor m_ReadmeEditor = null;

        public override string pageHeader
        {
            get { return m_Readme.header.title; }
        }

        public ReadmePage(ReadmeAsset r)
        {
            m_Readme = r;
        }

        public override void OnEnable()
        {
            m_ReadmeEditor = Editor.CreateEditor(m_Readme) as ReadmeAssetEditor;
        }

        public override void OnDisable()
        {
            Object.DestroyImmediate(m_ReadmeEditor);
        }

        public override void OnGUI()
        {
            m_ReadmeEditor.LayoutEmbedded();
        }
    }
}
