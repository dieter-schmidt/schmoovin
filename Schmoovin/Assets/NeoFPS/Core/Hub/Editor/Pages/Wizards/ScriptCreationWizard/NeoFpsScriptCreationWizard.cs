using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    //[CreateAssetMenu(fileName = "NeoFpsScriptCreationWizard", menuName = "NeoFPS Internal/NeoFpsScriptCreationWizard")]
    public class NeoFpsScriptCreationWizard : ScriptableObject
    {
#pragma warning disable 0414

        [SerializeField, Tooltip("Where the generated constant script should be output.")]
        private string m_TargetDirectory = string.Empty;

        [SerializeField, Tooltip("Where the generated constant editor script should be output.")]
        private string m_EditorDirectory = string.Empty;

        [SerializeField, Tooltip("The namespace for the project itself")]
        private string m_DefaultProjectNamespace = "MyProject";

        [SerializeField, Tooltip("The editor namespace for the project itself")]
        private string m_DefaultEditorNamespace = "MyProject";

        [SerializeField, Tooltip("The individual scripts to generate.")]
        private ScriptGeneratorData[] m_Data = new ScriptGeneratorData[0];

        //[HideInInspector]
        public CurrentData currentData = new CurrentData();

#pragma warning restore 0414

        public ScriptGeneratorData[] data
        {
            get { return m_Data; }
        }

        [Serializable]
        public class ScriptGeneratorData
        {
            public string title = "ClassType";
            public string category = "Misc";
            public string className = "MyClass";
            public string editorSuffix = "Editor";
            public string nameSpace = "MyProject";
            public ReplaceString[] properties = new ReplaceString[0];
            public TextAsset script = null;
            public TextAsset editorScript = null;
        }

        [Serializable]
        public class ReplaceString
        {
            public string propertyName = "Property";
            public string propertyTag = "PROPERTY";
            public string defaultValue = "MyProperty";
        }

        [Serializable]
        public class CurrentData
        {
            public int currentScript = -1;
            public string className = string.Empty;
            public string nameSpace = string.Empty;
            public string[] properties = { };
        }
    }
}
