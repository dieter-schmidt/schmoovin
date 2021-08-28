using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(AddForceBehaviour))]
    public class AddForceBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            var whenProp = serializedObject.FindProperty("m_When");
            EditorGUILayout.PropertyField(whenProp);

            if (whenProp.enumValueIndex == 2)
            {
                ++EditorGUI.indentLevel;
                MotionGraphEditorGUI.ParameterDropdownField<TriggerParameter>(
                    owner.container,
                    serializedObject.FindProperty("m_Trigger"),
                    new GUIContent("Trigger")
                    );
                --EditorGUI.indentLevel;
            }

            var forceParam = serializedObject.FindProperty("m_ForceParameter");
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                forceParam,
                new GUIContent("Force Parameter")
                );

            if (forceParam.objectReferenceValue == null)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Force"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ForceMode"));
        }
    }
}
