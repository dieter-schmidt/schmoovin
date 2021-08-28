using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NeoFPS.Hub.Pages;

namespace NeoFPSEditor.Hub.Pages
{
    public class DemoSceneInfoPage : HubPage
    {
        private DemoSceneReadme m_Readme = null;
        private ReadmeBehaviourEditor m_ReadmeEditor = null;

        public DemoSceneReadme readme
        {
            get { return m_Readme; }
            set
            {
                if (m_Readme == value)
                    return;

                // Set value
                m_Readme = value;

                // Destroy old editor
                if (m_ReadmeEditor != null)
                    Object.DestroyImmediate(m_ReadmeEditor);

                // Get editor
                if (value == null)
                    m_ReadmeEditor = null;
                else
                    m_ReadmeEditor = Editor.CreateEditor(m_Readme) as ReadmeBehaviourEditor;
            }
        }

        public override string pageHeader
        {
            get { return "Demo Scene Info"; }
        }

        public override void OnEnable()
        {
            DemoSceneReadme.onCurrentSceneChanged += OnDemoSceneReadmeChanged;
            readme = DemoSceneReadme.current;
        }

        public override void OnDisable()
        {
            DemoSceneReadme.onCurrentSceneChanged -= OnDemoSceneReadmeChanged;
            readme = null;
        }

        void OnDemoSceneReadmeChanged(DemoSceneReadme r)
        {
            readme = r;
        }

        public override void OnGUI()
        {
            if (m_ReadmeEditor == null)
            {
                EditorGUILayout.HelpBox("Please load one of the demo scenes in order to view information about it.", MessageType.Info);

                if (GUILayout.Button("Select A Demo Scene"))
                    NeoFpsHubEditor.ShowPage("demo_scenes");
            }
            else
                m_ReadmeEditor.LayoutEmbedded();
        }
    }
}
