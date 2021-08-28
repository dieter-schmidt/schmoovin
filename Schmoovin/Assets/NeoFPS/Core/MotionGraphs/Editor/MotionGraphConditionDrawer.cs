using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphConditionDrawer
    {
        static readonly char[] k_PathSeparators = new char[] { '\\', '/' };

        public MotionGraphContainer graphRoot { get; private set; }
        public MotionGraphCondition condition { get; private set; }
        public SerializedObject serializedObject { get; private set; }
        protected float lineOffset { get; private set; }

        private string m_DisplayName = string.Empty;
        private string m_HelpURL = string.Empty;

        public void Initialise (MotionGraphContainer graph, MotionGraphCondition c)
        {
            graphRoot = graph;
            condition = c;
            serializedObject = new SerializedObject(c);
            lineOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Get help URL
            var conditionType = c.GetType();
            var attributes = conditionType.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                var element = attr as MotionGraphElementAttribute;
                if (element != null)
                {
                    var split = element.menuPath.Split(k_PathSeparators);
                    m_DisplayName = split[split.Length - 1] + " Condition";
                    continue;
                }

                var help = attr as HelpURLAttribute;
                if (help != null)
                    m_HelpURL = help.URL;
            }
        }

        public float GetHeight ()
        {
            return lineOffset * (numLines + 1) + EditorGUIUtility.standardVerticalSpacing;
            //return EditorGUIUtility.singleLineHeight + (numLines * lineOffset);
        }

        protected virtual string displayName
        {
            get { return m_DisplayName; }
        }

        protected virtual int numLines
        {
            get { return 1; }
        }

        public void Draw (Rect rect)
        {
            serializedObject.UpdateIfRequiredOrScript();

            rect.height = EditorGUIUtility.singleLineHeight;

            // DrawLabel
            EditorGUI.LabelField(rect, displayName, EditorStyles.boldLabel);

            // Draw help URL link
            if (!string.IsNullOrEmpty(m_HelpURL))
            {
                Rect helpRect = rect;
                float height = helpRect.height;
                helpRect.x += helpRect.width - height;
                helpRect.width = height;
                if (GUI.Button(helpRect, "", MotionGraphEditorStyles.helpButton))
                    Application.OpenURL(m_HelpURL);
            }

            // Inspect properties
            if (numLines > 0)
            {
                rect.y += lineOffset;
                Inspect(rect);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void Inspect (Rect line1)
        {

        }
    }
}