using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(HudAdvancedCrosshair), true)]
    public class HudAdvancedCrosshairEditor : Editor
    {
        ReorderableList m_List = null;

        void OnEnable()
        {
            m_List = new ReorderableList(
                       serializedObject,
                       serializedObject.FindProperty("m_Crosshairs"),
                       false,
                       true,
                       false,
                       false
                   );
            m_List.drawHeaderCallback = DrawHeader;
            m_List.drawElementCallback = DrawElement;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The different crosshair entries here are based on the FpsCrosshair constant. You can add more crosshairs by modifying the constants settings file and regenerating this constant.", MessageType.Info);

            m_List.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultCrosshair"));

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Crosshairs");
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4f;
            rect.y += 1f;
            var element = m_List.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, new GUIContent(FpsCrosshair.names[index]));
        }
    }
}