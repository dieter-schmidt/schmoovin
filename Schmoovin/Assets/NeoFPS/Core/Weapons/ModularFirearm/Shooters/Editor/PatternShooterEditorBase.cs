using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace NeoFPSEditor.ModularFirearms
{
    public class PatternShooterEditorBase : BaseFirearmModuleBehaviourEditor
    {
        public static void DrawPatternProperty(Object target, SerializedProperty patternProp, SerializedProperty distanceProp)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pattern", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Edit pattern button
            if (GUILayout.Button("Edit Pattern"))
                ShooterPatternEditorWindow.ShowWindow(target);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            // Show properties
            EditorGUILayout.PropertyField(distanceProp);
            EditorGUILayout.PropertyField(patternProp, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}