using UnityEditor;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(PassiveSlideBehaviour))]
    public class PassiveSlideBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            var container = owner.container;
            MotionGraphEditorGUI.FloatDataReferenceField(container, serializedObject.FindProperty("m_SlideAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlideFriction"));
        }
    }
}
