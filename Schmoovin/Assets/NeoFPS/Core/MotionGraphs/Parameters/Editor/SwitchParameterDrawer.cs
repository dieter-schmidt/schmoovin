using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    [MotionGraphPropertyDrawer(typeof(SwitchParameter))]
    public class SwitchParameterDrawer : MotionGraphPropertyDrawer
    {
        protected override void DrawValue(Rect r)
        {
            if (editor.editingRuntimeGraph)
            {
                var cast = serializedObject.targetObject as SwitchParameter;
                EditorGUI.Toggle(r, new GUIContent(), cast.on);
            }
            else
                EditorGUI.PropertyField(r, serializedObject.FindProperty("m_StartingValue"), new GUIContent());
        }
    }
}