using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyTriggerParameterBehaviour))]
    public class ModifyTriggerParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<TriggerParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_What"));
        }
    }
}
