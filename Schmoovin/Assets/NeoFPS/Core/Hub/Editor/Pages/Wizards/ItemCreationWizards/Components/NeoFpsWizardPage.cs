using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class NeoFpsWizardPage<T> : HubPage where T : NeoFpsWizard
    {
        [SerializeField]
        private NeoFpsWizard m_Wizard = null;
        private NeoFpsWizardEditor m_Editor = null;

        public string startingStep = null;

        private TemplateDescription[] m_TemplateDescriptions = null;
        private string m_PageHeader = "Item Creation Wizard";
        private string m_SubHeading = "Create a new item";
        private bool m_DrawTemplateSelection = false;

        public override string pageHeader
        {
            get { return m_PageHeader; }
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

        public NeoFpsWizardPage(string header, string subtitle, TemplateDescription[] templates)
        {
            m_PageHeader = header;
            m_TemplateDescriptions = templates;
        }

        public override void OnEnable()
        {
            if (m_Wizard != null)
            {
                m_Editor = Editor.CreateEditor(m_Wizard) as NeoFpsWizardEditor;
                m_Editor.currentStep = startingStep;
            }
        }

        public override void OnDisable()
        {
            if (m_Editor != null)
            {
                startingStep = m_Editor.currentStep;
                Object.DestroyImmediate(m_Editor);
                m_Editor = null;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_Editor != null)
            {
                m_Editor.onResetWizard -= OnResetWizard;
                m_Editor = null;
            }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();

            // 1 select a template or start a new firearm (instantiate from template, not use original)
            // - Create editor from wizard
            if (m_Wizard == null || m_DrawTemplateSelection == true)
            {
                m_DrawTemplateSelection = false;
                DrawTemplateSelectionOptions();
                DrawTemplateDescriptions();
            }
            else
            {
                m_Templates = null;

                if (m_Editor == null)
                {
                    m_Editor = Editor.CreateEditor(m_Wizard) as NeoFpsWizardEditor;
                    if (m_Editor == null)
                        Debug.LogError("Editor is null. Wizard: " + m_Wizard);
                    m_Editor.onResetWizard += OnResetWizard;
                    m_Editor.onSaveAsTemplate += OnSaveAsTemplate;
                    m_Templates = null; // Might as well do this here instead of every draw frame
                }

                m_Editor.DoLayoutInline(true);
            }
        }

        private void OnSaveAsTemplate()
        {
            m_Editor.onResetWizard -= OnResetWizard;
            m_Editor.onSaveAsTemplate -= OnSaveAsTemplate;

            m_Wizard = m_Wizard.Clone();
            Object.DestroyImmediate(m_Editor);

            m_Editor = null;

            throw new ExitGUIException();
        }

        private void OnResetWizard()
        {
            m_Editor.onResetWizard -= OnResetWizard;
            m_Editor.onSaveAsTemplate -= OnSaveAsTemplate;

            Object.DestroyImmediate(m_Wizard);
            Object.DestroyImmediate(m_Editor);

            m_Wizard = null;
            m_Editor = null;

            throw new ExitGUIException();
        }

        #region TEMPLATE SELECTION

        private int m_TemplateIndex = -1;
        private T[] m_Templates = null;

        void DrawTemplateSelectionOptions()
        {
            EditorGUILayout.LabelField(m_SubHeading, ReadmeEditorUtility.h3Style);

            EditorGUILayout.LabelField("You can select a template to start from, or create the item from scratch.");
            EditorGUILayout.LabelField("Once you have completed the wizard you will have the option of saving your choices as a new template to use as a starting point later.", EditorStyles.wordWrappedLabel);

            // Get existing templates
            if (m_Templates == null)
            {
                // Search for existing wizard assets
                var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                m_Templates = new T[guids.Length];
                for (int i = 0; i < guids.Length; ++i)
                    m_Templates[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            EditorGUILayout.Space();

            // Get dropdown label
            GUIContent dropdownLabel = null;
            if (m_TemplateIndex == -1)
                dropdownLabel = new GUIContent("<Select a Template>");
            else
                dropdownLabel = new GUIContent(m_Templates[m_TemplateIndex].name);

            // Template dropdown
            if (EditorGUILayout.DropdownButton(dropdownLabel, FocusType.Passive) && m_Templates.Length > 0)
            {
                var menu = new GenericMenu();

                // Loaded templates
                for (int i = 0; i < m_Templates.Length; ++i)
                    menu.AddItem(new GUIContent(m_Templates[i].name), false, OnTemplateSelected, i);

                menu.ShowAsContext();
            }

            // Start choosing options
            if (m_TemplateIndex == -1)
                GUI.enabled = false;
            if (GUILayout.Button("Start From Template"))
            {
                m_Wizard = m_Templates[m_TemplateIndex].Clone();
                //m_Wizard.CheckStartingState();
                if (Event.current.type == EventType.Layout)
                    m_DrawTemplateSelection = true;
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            // Start choosing options
            if (GUILayout.Button("Create From Scratch"))
            {
                m_Wizard = ScriptableObject.CreateInstance<T>();
                if (Event.current.type == EventType.Layout)
                    m_DrawTemplateSelection = true;
            }
        }

        void OnTemplateSelected(object o)
        {
            m_TemplateIndex = (int)o;
        }

        #endregion

        #region TEMPLATE DESCRIPTIONS

        void DrawTemplateDescriptions()
        {
            if (m_TemplateDescriptions != null)
            {
                EditorGUILayout.Space();
                NeoFpsEditorGUI.Separator();

                EditorGUILayout.LabelField("Example Templates", ReadmeEditorUtility.h2Style);
                EditorGUILayout.Space();

                foreach (var templateDescr in m_TemplateDescriptions)
                {
                    EditorGUILayout.LabelField(templateDescr.title, ReadmeEditorUtility.h3Style);
                    EditorGUILayout.LabelField(templateDescr.description, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.Space();
                }
            }
        }

        #endregion
    }

    public class TemplateDescription
    {
        public string title = string.Empty;
        public string description = string.Empty;

        public TemplateDescription(string title, string description)
        {
            this.title = title;
            this.description = description;
        }
    }
}
