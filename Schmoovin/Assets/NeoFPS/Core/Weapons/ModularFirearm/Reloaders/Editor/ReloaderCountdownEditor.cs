using NeoFPS.ModularFirearms;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ReloaderCountdown))]
    public class ReloaderCountdownEditor : Editor
    {
        const float k_LabelWidth = 60f;

        private ReorderableList m_List = null;

        private void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("m_CountdownAudio"), true, true, true, true);
            m_List.drawElementCallback = DrawCountdownElement;
            m_List.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Countdown Audio (Top = Last Round)");
            };
            //m_List.onRemoveCallback = RemoveListEntryAtIndex;
        }

        void DrawCountdownElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Shift down by 2 (compensate for single line height)
            rect.y += 1;
            rect.height -= 4;

            // Draw the clip
            rect.width *= 0.5f;
            EditorGUI.PropertyField(rect, m_List.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("clip"), GUIContent.none);

            // Draw the volume label
            rect.x += rect.width;
            Rect labelRect = rect;
            labelRect.width = k_LabelWidth;
            EditorGUI.LabelField(labelRect, "volume");

            // Draw the volume readout
            rect.x += k_LabelWidth;
            rect.width -= k_LabelWidth;
            EditorGUI.PropertyField(rect, m_List.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("volume"), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExtendSequence"));

            m_List.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}