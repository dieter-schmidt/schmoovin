using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(SteeringRateBehaviour))]
    public class SteeringRateBehaviourEditor : MotionGraphBehaviourEditor
    {
        enum OnExitProxy : int
        {
            Set,
            Ignore
        }

        protected override void OnInspectorGUI()
        {
            var entryProp = serializedObject.FindProperty("m_OnEnter");
            var exitProp = serializedObject.FindProperty("m_OnExit");

            EditorGUILayout.PropertyField(entryProp);

            if (entryProp.enumValueIndex == 0)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EntryValue"));
                --EditorGUI.indentLevel;

                EditorGUILayout.PropertyField(exitProp);
                if (exitProp.enumValueIndex == 0)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExitValue"));
                    --EditorGUI.indentLevel;
                }
            }
            else
            {
                // Get OnExit value, but constrained to not include reset if it was never set
                OnExitProxy proxy = exitProp.enumValueIndex == 0 ? OnExitProxy.Set : OnExitProxy.Ignore;
                var newProxy = (OnExitProxy)EditorGUILayout.EnumPopup(new GUIContent(exitProp.displayName, "What to do to the steering rate on exiting the state."), proxy);
                if (proxy != newProxy)
                    exitProp.enumValueIndex = newProxy == OnExitProxy.Set ? 0 : 1;

                if (exitProp.enumValueIndex == 0)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ExitValue"));
                    --EditorGUI.indentLevel;
                }
            }
        }
    }
}
