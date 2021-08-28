using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    [MotionGraphPropertyDrawer(typeof(IntParameter))]
    public class IntParameterDrawer : MotionGraphPropertyDrawer
    {
        protected override void DrawValue(Rect r)
        {
            if (editor.editingRuntimeGraph)
            {
                var cast = serializedObject.targetObject as IntParameter;
                EditorGUI.IntField(r, new GUIContent(), cast.value);
            }
            else
                EditorGUI.PropertyField(r, serializedObject.FindProperty("m_StartingValue"), new GUIContent());
        }
    }
}