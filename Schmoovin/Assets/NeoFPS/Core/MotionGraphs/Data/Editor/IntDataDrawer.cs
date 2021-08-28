using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    [MotionGraphPropertyDrawer(typeof(IntData))]
    public class IntDataDrawer : MotionGraphPropertyDrawer
    {
        readonly GUIContent k_BlankContent = new GUIContent();

        protected override void DrawValue(Rect r)
        {
            EditorGUI.PropertyField(r, serializedObject.FindProperty("m_Value"), k_BlankContent);
        }
    }
}