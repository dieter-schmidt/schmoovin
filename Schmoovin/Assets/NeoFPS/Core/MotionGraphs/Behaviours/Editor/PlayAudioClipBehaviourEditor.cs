using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(PlayAudioClipBehaviour))]
    public class PlayAudioClipBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Clip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Volume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Where"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));
        }
    }
}
