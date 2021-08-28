using UnityEditor;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(DrainStaminaBehaviour))]
    public class DrainStaminaBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrainRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScaleByInput"));

            var limitProp = serializedObject.FindProperty("m_LimitDrain");
            EditorGUILayout.PropertyField(limitProp);

            if (limitProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrainTarget"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DrainFalloff"));
            }
        }
    }
}
