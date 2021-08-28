using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.States;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.States
{
    [CustomEditor(typeof(MaintainVelocityState))]
    //[HelpURL("")]
    public class MaintainVelocityStateEditor : MotionGraphStateEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GroundSnapping"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ApplyGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnorePlatforms"));
        }
    }
}