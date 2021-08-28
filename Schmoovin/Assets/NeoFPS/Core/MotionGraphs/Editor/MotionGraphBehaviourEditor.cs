using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphBehaviourEditor
    {
        static readonly char[] k_PathSeparators = new char[] { '\\', '/' };
        static readonly GUIContent titleMoveUp = new GUIContent("Move Up");
        static readonly GUIContent titleMoveDown = new GUIContent("Move Down");
        static readonly GUIContent titleRemove = new GUIContent("Remove");
        static readonly GUIContent titleCopy = new GUIContent("Copy");
        static readonly GUIContent titlePaste = new GUIContent("Paste");
        static readonly GUIContent titleEditScript = new GUIContent("Edit Script");

        public MotionGraphBehaviour behaviour { get; private set; }
        public SerializedObject serializedObject { get; private set; }
        protected MotionGraphConnectableEditor owner { get; private set; }

        private static string m_Clipboard = string.Empty;

        private string m_DisplayName = string.Empty;
        private string m_HelpURL = string.Empty;
        private MonoScript m_Script = null;
        private GUIStyle m_Style = null;

        public virtual void Initialise (MotionGraphBehaviour b, MotionGraphConnectableEditor connectable)
        {
            behaviour = b;
            owner = connectable;
            serializedObject = new SerializedObject(behaviour);

            // Get help URL
            var behaviourType = b.GetType();
            var attributes = behaviourType.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                var element = attr as MotionGraphElementAttribute;
                if (element != null)
                {
                    var split = element.menuPath.Split(k_PathSeparators);
                    m_DisplayName = split[split.Length - 1];
                    continue;
                }

                var help = attr as HelpURLAttribute;
                if (help != null)
                    m_HelpURL = help.URL;
            }

            // Get script
            m_Script = MotionGraphEditorFactory.GetScriptForGraphElement(behaviourType);
        }

        public void DoLayout (MotionGraphConnectable owner, int index)
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (DrawHeader(owner, index))
                OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        void CheckFoldoutStyle()
        {
            if (m_Style == null)
            {
                m_Style = new GUIStyle(EditorStyles.foldout);
                m_Style.fontStyle = FontStyle.Bold;
            }
        }

        bool DrawHeader (MotionGraphConnectable owner, int index)
        {
            MotionGraphEditorStyles.DrawSeparator();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Active"), new GUIContent(""), GUILayout.Width(24));

            CheckFoldoutStyle();
            var expanded = serializedObject.FindProperty("expanded");
            bool result = EditorGUILayout.Foldout(expanded.boolValue, m_DisplayName, true, m_Style);
            if (expanded.boolValue != result)
                expanded.boolValue = result;

            if (!string.IsNullOrEmpty(m_HelpURL))
            {
                if (GUILayout.Button("", MotionGraphEditorStyles.helpButton))
                    Application.OpenURL(m_HelpURL);
            }

            if (GUILayout.Button("", MotionGraphEditorStyles.optionsButton))
            {
                GenericMenu menu = new GenericMenu();

                // Move up / down
                if (CheckCanMoveUp())
                    menu.AddItem(titleMoveUp, false, OnMenuMoveUp);
                else
                    menu.AddDisabledItem(titleMoveUp);

                if (CheckCanMoveDown())
                    menu.AddItem(titleMoveDown, false, OnMenuMoveDown);
                else
                    menu.AddDisabledItem(titleMoveDown);

                // Remove
                menu.AddSeparator("");
                menu.AddItem(titleRemove, false, OnMenuRemove);

                // Clipboard
                menu.AddSeparator("");
                menu.AddItem(titleCopy, false, OnMenuCopy);
                if (CheckCanPaste())
                    menu.AddItem(titlePaste, false, OnMenuPaste);
                else
                    menu.AddDisabledItem(titlePaste);

                // Script
                if (m_Script != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(titleEditScript, false, OnMenuEditScript);
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            return result;
        }

        bool CheckCanMoveUp()
        {
            int i = owner.connectable.behaviours.IndexOf(behaviour);
            return i > 0;
        }

        bool CheckCanMoveDown()
        {
            int i = owner.connectable.behaviours.IndexOf(behaviour);
            return i < owner.connectable.behaviours.Count - 1;
        }

        bool CheckCanPaste()
        {
            return m_Clipboard != string.Empty;
        }

        void OnMenuMoveUp()
        {
            owner.ReorderMotionGraphBehaviour(behaviour, -1);
        }

        void OnMenuMoveDown()
        {
            owner.ReorderMotionGraphBehaviour(behaviour, +1);
        }

        void OnMenuRemove()
        {
            owner.RemoveBehaviour(behaviour);
        }
        
        void OnMenuCopy()
        {
            m_Clipboard = JsonUtility.ToJson(behaviour);
        }

        void OnMenuPaste()
        {
            Undo.RecordObject(behaviour, "Paste Values");
            JsonUtility.FromJsonOverwrite(m_Clipboard, behaviour);
            serializedObject.Update();
            owner.Repaint();
        }

        void OnMenuEditScript()
        {
            AssetDatabase.OpenAsset(m_Script);
        }

        protected virtual void OnInspectorGUI ()
        {
            EditorGUILayout.HelpBox("No valid editor found for behaviour type: " + behaviour.GetType(), MessageType.Error);
        }
    }
}
