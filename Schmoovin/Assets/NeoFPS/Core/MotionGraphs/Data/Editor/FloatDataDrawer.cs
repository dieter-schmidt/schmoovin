using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    [MotionGraphPropertyDrawer(typeof(FloatData))]
    public class FloatDataDrawer : MotionGraphPropertyDrawer
    {
        readonly GUIContent k_BlankContent = new GUIContent();

        protected override void DrawValue(Rect r)
        {
            EditorGUI.PropertyField(r, serializedObject.FindProperty("m_Value"), k_BlankContent);
        }
    }
}