using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class NeoFpsScriptCreationWizardPage : HubPage
    {
        private NeoFpsScriptCreationWizard m_Generator = null;
        private NeoFpsScriptCreationWizardEditor m_Editor = null;

        public NeoFpsScriptCreationWizardPage(NeoFpsScriptCreationWizard targetObject)
        {
            m_Generator = targetObject;
        }

        public override string pageHeader
        {
            get { return "Custom Script Generator"; }
        }

        private ReadmeHeader m_Heading = null;
        public ReadmeHeader heading
        {
            get
            {
                if (m_Heading == null)
                    m_Heading = new ReadmeHeader(LoadIcon("EditorImage_NeoFpsIconRound", "EditorImage_NeoFpsCrosshair"), pageHeader);
                return m_Heading;
            }
        }

        public override void OnEnable()
        {
            m_Editor = Editor.CreateEditor(m_Generator) as NeoFpsScriptCreationWizardEditor;
        }

        public override void OnDisable()
        {
            if (m_Editor != null)
            {
                Object.DestroyImmediate(m_Editor);
                m_Editor = null;
            }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();

            if (m_Editor == null)
            {
                EditorGUILayout.HelpBox("NeoFpsScriptGenerator asset not found", MessageType.Error);
                return;
            }

            // Layout the editor (disable editing)
            m_Editor.DoLayout(false);
        }
    }
}