using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    [MotionGraphPropertyDrawer(typeof(FloatParameter))]
    public class FloatParameterDrawer : MotionGraphPropertyDrawer
    {
        protected override void DrawValue(Rect r)
        {
            if (editor.editingRuntimeGraph)
            {
                var cast = serializedObject.targetObject as FloatParameter;
                EditorGUI.FloatField(r, new GUIContent(), cast.value);
            }
            else
                EditorGUI.PropertyField(r, serializedObject.FindProperty("m_StartingValue"), new GUIContent());
        }
    }
}