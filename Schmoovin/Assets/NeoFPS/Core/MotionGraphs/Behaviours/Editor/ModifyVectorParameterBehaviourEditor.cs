using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyVectorParameterBehaviour))]
    public class ModifyVectorParameterBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(
                owner.container,
                serializedObject.FindProperty("m_Parameter"),
                new GUIContent("Parameter")
                );

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var what = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(what);

            //Set,
            //Reset,
            //Add,
            //Subtract,
            //Multiply,
            //Normalize,
            //Flatten,
            //ClampMag

            switch (what.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Value"));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Value"));
                    break;
                case 3:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Value"));
                    break;
                case 4:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Multiplier"));
                    break;
                case 7:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Clamp"));
                    break;
            }
        }
    }
}
