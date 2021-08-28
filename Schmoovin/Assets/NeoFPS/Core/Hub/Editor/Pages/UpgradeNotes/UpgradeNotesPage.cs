using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    public class UpgradeNotesPage : HubPage
    {
        private UpgradeNotesReadme m_Readme = null;
        private UpgradeNotesReadmeEditor m_ReadmeEditor = null;

        public override string pageHeader
        {
            get { return "Upgrade Notes"; }
        }

        public UpgradeNotesPage(UpgradeNotesReadme r)
        {
            m_Readme = r;
        }

        public override MessageType notification
        {
            get
            {
                if (!m_Readme.CheckIsUpToDate())
                    return MessageType.Info;
                else
                    return MessageType.None;
            }
        }

        public override void OnEnable()
        {
            m_ReadmeEditor = Editor.CreateEditor(m_Readme) as UpgradeNotesReadmeEditor;
            NeoFpsEditorPrefs.currentNeoFPSVersion = m_Readme.latest;
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