using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub.Pages
{
    public abstract class EmbeddedEditorPage<T> : HubPage where T : Object
    {
        private Editor m_Editor = null;

        public T target
        {
            get;
            private set;
        }

        public EmbeddedEditorPage(T targetObject)
        {
            target = targetObject;
            if (target != null)
                m_Editor = Editor.CreateEditor(targetObject);
        }

        public virtual string editorNotFoundErrorMessage
        {
            get { return "Editor or object not found"; }
        }

        public override MessageType notification
        {
            get
            {
                if (m_Editor == null)
                    return MessageType.Error;
                else
                    return MessageType.None;
            }
        }

        public virtual void PreEditorGUI() { }
        public virtual void PostEditorGUI() { }

        public override void OnGUI()
        {            
            if (m_Editor != null)
                m_Editor.OnInspectorGUI();
            else
                EditorGUILayout.HelpBox(editorNotFoundErrorMessage, MessageType.Error);
        }

        public override void OnDestroy()
        {
            if (m_Editor != null)
            {
                Object.DestroyImmediate(m_Editor);
                m_Editor = null;
            }
        }
    }
}
