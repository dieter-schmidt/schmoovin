using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

namespace NeoFPSEditor.Hub.Pages
{
    [CustomEditor(typeof(NeoFpsScriptCreationWizard))]
    public class NeoFpsScriptCreationWizardEditor : Editor
    {
        private readonly char[] k_Separators = { '/', '\\' };

        //[SerializeField] private int m_CurrentScript = 0;
        //[SerializeField] private string m_ClassName = string.Empty;
        //[SerializeField] private string m_Namespace = string.Empty;
        //[SerializeField] private string[] m_Properties = new string[0];
        [SerializeField] private bool m_IsValid = true;
        [SerializeField] private bool m_EditSettings = false;

        public NeoFpsScriptCreationWizard generator
        {
            get { return target as NeoFpsScriptCreationWizard; }
        }

        private void Awake()
        {
            if (generator.currentData.currentScript == -1)
            {
                generator.currentData.currentScript = 0;
                ResetCurrentScript();
            }
        }

        public override void OnInspectorGUI()
        {
            DoLayout(true);
        }

        public void DoLayout(bool allowEditing)
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Draw folders
            DrawTargetDirectoryGUI();
            EditorGUILayout.Space();

            // Check if should show inspector for editing data, or generator instead
            bool showEditorSettings = false;
            if (allowEditing)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_EditSettings = EditorGUILayout.Toggle("Edit Generator Data", m_EditSettings);
                EditorGUILayout.EndVertical();

                showEditorSettings = m_EditSettings;

                EditorGUILayout.Space();
            }

            // Draw editor / generator
            if (showEditorSettings)
                DoLayoutEditor();
            else
                DoLayoutGenerator();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTargetDirectoryGUI()
        {
            // Get current target folder object
            Object folderObject = null;
            Object prevFolderObject = null;
            SerializedProperty targetDir = serializedObject.FindProperty("m_TargetDirectory");
            if (targetDir.stringValue != string.Empty)
            {
                folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(targetDir.stringValue);
                prevFolderObject = folderObject;

                if (folderObject == null)
                    targetDir.stringValue = string.Empty;
            }

            // Show folder selection field
            folderObject = EditorGUILayout.ObjectField("Target Directory", folderObject, typeof(DefaultAsset), false, null);
            if (folderObject != prevFolderObject)
            {
                if (folderObject == null)
                    targetDir.stringValue = string.Empty;
                else
                    targetDir.stringValue = AssetDatabase.GetAssetPath(folderObject);
            }

            // Check if no folder selected and show error
            if (targetDir.stringValue == string.Empty)
                EditorGUILayout.HelpBox("Script will be placed in the root of the Assets folder.", MessageType.Info);

            // Get current target folder object
            folderObject = null;
            prevFolderObject = null;
            SerializedProperty editorDir = serializedObject.FindProperty("m_EditorDirectory");
            if (editorDir.stringValue != string.Empty)
            {
                folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(editorDir.stringValue);
                prevFolderObject = folderObject;

                if (folderObject == null)
                    editorDir.stringValue = string.Empty;
            }

            // Show folder selection field
            folderObject = EditorGUILayout.ObjectField("Editor Directory", folderObject, typeof(DefaultAsset), false, null);
            if (folderObject != prevFolderObject)
            {
                if (folderObject == null)
                    editorDir.stringValue = string.Empty;
                else
                {
                    string path = AssetDatabase.GetAssetPath(folderObject);
                    string[] split = path.Split(k_Separators);
                    if (ArrayUtility.Contains(split, "Editor"))
                        editorDir.stringValue = path;
                    else
                        Debug.LogError("Editor folder path must include a folder called \"Editor\"");
                }
            }

            // Check if no folder selected and show error
            if (editorDir.stringValue == string.Empty)
                EditorGUILayout.HelpBox("Script will be placed in Assets/Editor (this folder will be created if it does not exist).", MessageType.Info);
        }

        void DoLayoutEditor()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultProjectNamespace"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultEditorNamespace"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Data"), true);
        }

        void ResetCurrentScript()
        {
            //Debug.Log("Control: " + GUI.GetNameOfFocusedControl());
            //GUI.FocusControl(null);
            //GUIUtility.hotControl = 0;
            //GUIUtility.keyboardControl = 0;
            EditorGUI.FocusTextInControl(string.Empty);

            var currentScript = generator.data[generator.currentData.currentScript];

            generator.currentData.className = currentScript.className;
            generator.currentData.nameSpace = currentScript.nameSpace;
            generator.currentData.properties = new string[currentScript.properties.Length];
            for (int i = 0; i < generator.currentData.properties.Length; ++i)
                generator.currentData.properties[i] = currentScript.properties[i].defaultValue;

            Repaint();
        }

        bool CheckClassOrPropertyName(string toCheck)
        {
            // Use a regex to check the classname (alphanumeric and underscore only)
            bool result = Regex.IsMatch(toCheck, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
            m_IsValid &= result;
            return result;
        }

        bool CheckNamespace(string ns)
        {
            // Use a regex to check the namespace name (alphanumeric and underscore separated by '.')
            bool result = Regex.IsMatch(ns, @"^[a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*$");
            m_IsValid &= result;
            return result;
        }

        void OnScriptSelect(object o)
        {
            int index = (int)o;
            if (generator.currentData.currentScript != index)
            {
                generator.currentData.currentScript = index;
                ResetCurrentScript();
            }
        }

        void DoLayoutGenerator ()
        {
            var currentScript = generator.data[generator.currentData.currentScript];
            m_IsValid = true;

            // Show script selection
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Current Script Type: ", EditorStyles.boldLabel);


            GUIContent scriptLabel;
            if (!string.IsNullOrEmpty(currentScript.category))
                scriptLabel = new GUIContent(currentScript.category + "/" + currentScript.title);
            else
                scriptLabel = new GUIContent(currentScript.title);

            if (EditorGUILayout.DropdownButton(scriptLabel, FocusType.Passive))
            {
                var g = generator;
                var menu = new GenericMenu();

                for (int i = 0; i < g.data.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(g.data[i].category))
                        menu.AddItem(new GUIContent(g.data[i].category + "/" + g.data[i].title), false, OnScriptSelect, i);
                    else
                        menu.AddItem(new GUIContent(g.data[i].title), false, OnScriptSelect, i);
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Show class name controls
            if (!CheckClassOrPropertyName(generator.currentData.className))
                GUI.color = NeoFpsEditorGUI.errorRed;

            EditorGUILayout.BeginHorizontal();
            generator.currentData.className = EditorGUILayout.TextField("Class Name", generator.currentData.className);
            if (GUILayout.Button("Reset", GUILayout.Width(48)))
                generator.currentData.className = currentScript.className;
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;
            
            // Show namespace controls
            if (!CheckNamespace(generator.currentData.nameSpace))
                GUI.color = NeoFpsEditorGUI.errorRed;

            EditorGUILayout.BeginHorizontal();
            generator.currentData.nameSpace = EditorGUILayout.TextField("Namespace", generator.currentData.nameSpace);
            if (GUILayout.Button("Reset", GUILayout.Width(48)))
                generator.currentData.nameSpace = currentScript.nameSpace;
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;

            // Show properties
            int count = currentScript.properties.Length;
            if (count > 0)
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Properties:", EditorStyles.boldLabel);
                for (int i = 0; i < count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    generator.currentData.properties[i] = EditorGUILayout.TextField(currentScript.properties[i].propertyName, generator.currentData.properties[i]);
                    if (GUILayout.Button("Reset", GUILayout.Width(48)))
                        generator.currentData.properties[i] = currentScript.properties[i].defaultValue;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
            }

            // Generate script(s)
            if (!m_IsValid)
                GUI.enabled = false;
            if (GUILayout.Button("Generate Script"))
            {
                // Sub deferred method
                EditorApplication.update += GenerateScriptsDeferred;
            }
            GUI.enabled = true;
        }

        void GenerateScriptsDeferred()
        {
            // Generate
            var currentScript = generator.data[generator.currentData.currentScript];
            if (currentScript.script != null)
                GenerateScript();
            if (currentScript.editorScript != null)
                GenerateEditorScript();

            // Unsub
            EditorApplication.update -= GenerateScriptsDeferred;

            // Refresh to get new scripts
            AssetDatabase.Refresh();
        }

        void GenerateScript()
        {
            var currentScript = generator.data[generator.currentData.currentScript];
            string scriptText = currentScript.script.text;

            // Replace script tags
            scriptText = scriptText.Replace("%CLASS_NAME%", generator.currentData.className);
            scriptText = scriptText.Replace("%NAMESPACE%", generator.currentData.nameSpace);
            for (int i = 0; i < currentScript.properties.Length; ++i)
                scriptText = scriptText.Replace(string.Format("%{0}%", currentScript.properties[i].propertyTag), generator.currentData.properties[i]);

            // Get the target file path
            var targetDir = serializedObject.FindProperty("m_TargetDirectory").stringValue;
            if (string.IsNullOrEmpty(targetDir))
                targetDir = Application.dataPath;
            string path = targetDir + "/" + generator.currentData.className + ".cs";

            // Write the file
            Debug.Log("Writing script: " + path);
            File.WriteAllText(path, scriptText);
        }

        void GenerateEditorScript()
        {
            var currentScript = generator.data[generator.currentData.currentScript];
            string editorText = currentScript.editorScript.text;

            // Replace editor script tags
            editorText = editorText.Replace("%CLASS_NAME%", generator.currentData.className);
            editorText = editorText.Replace("%EDITOR_NAME%", generator.currentData.className + currentScript.editorSuffix);
            editorText = editorText.Replace("%NAMESPACE%", generator.currentData.nameSpace);
            for (int i = 0; i < currentScript.properties.Length; ++i)
                editorText = editorText.Replace(string.Format("%{0}%", currentScript.properties[i].propertyTag), generator.currentData.properties[i]);

            // Get the target file path
            var targetDir = serializedObject.FindProperty("m_EditorDirectory").stringValue;
            if (string.IsNullOrEmpty(targetDir))
                targetDir = Application.dataPath + "/Editor";
            string path = targetDir + "/" + generator.currentData.className + currentScript.editorSuffix + ".cs";

            // Check the directory exists
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // Write the file
            Debug.Log("Writing editor script: " + path);
            File.WriteAllText(path, editorText);
        }
    }
}
