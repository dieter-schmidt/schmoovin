using UnityEditor;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyStaminaBehaviour))]
    public class ModifyStaminaBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var what = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(what);

            if (what.enumValueIndex <= 5)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Amount"));
        }
    }
}
