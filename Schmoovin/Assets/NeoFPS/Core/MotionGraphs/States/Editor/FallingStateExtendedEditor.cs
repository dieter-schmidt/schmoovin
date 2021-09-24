using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(FallingStateExtended))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-fallingstate.html")]
    public class FallingStateExtendedEditorOG : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var root = container;

            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.ParameterDropdownField<VectorParameter>(container, serializedObject.FindProperty("m_WallNormal"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_HorizontalAcceleration"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_TopSpeed"));
            //START DS MOD
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_ForwardMultiplier"));
            //END DS MOD
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_StrafeMultiplier"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_ReverseMultiplier"));
            //START DS MOD
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_GravityMultiplier"));
            //END DS MOD
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_HorizontalDrag"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Falling Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClampSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Damping"));
        }
    }
}