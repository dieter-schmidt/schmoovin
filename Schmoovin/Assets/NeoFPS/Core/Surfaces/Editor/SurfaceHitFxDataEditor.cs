using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(SurfaceHitFxData))]
    public class SurfaceHitFxDataEditor : Editor
    {
        ReorderableList m_ImpactEffectsList = null;

        void OnEnable()
        {
            m_ImpactEffectsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_Data"),
                true,
                true,
                true,
                true
            );
            m_ImpactEffectsList.drawHeaderCallback = DrawClipsHeader;
            m_ImpactEffectsList.drawElementCallback = DrawClipsElement;
        }

        void DrawClipsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Surface Impact Effects");
        }

        void DrawClipsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= 4f;
            rect.y += 1f;
            var element = m_ImpactEffectsList.serializedProperty.GetArrayElementAtIndex(index);

            GUIContent label = new GUIContent(FpsSurfaceMaterial.names[index]);
            EditorGUI.PropertyField(rect, element, label);
        }

        public override void OnInspectorGUI()
        {
            m_ImpactEffectsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}