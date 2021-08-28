using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(SteepSlideState))]
    [HelpURL("http://docs.neofps.com/manual/motiongraphref-mgs-steepslidestate.html")]
    public class SteepSlideStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var root = container;
            EditorGUILayout.LabelField("Motion Data", EditorStyles.boldLabel);
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_SlideAngle"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_SpeedMinimum"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_SpeedMaximum"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_AccelerationMinimum"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_AccelerationMaximum"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_HorizontalSpeedLimit"));
            MotionGraphEditorGUI.FloatDataReferenceField(root, serializedObject.FindProperty("m_HorizontalAcceleration"));
        }
    }
}