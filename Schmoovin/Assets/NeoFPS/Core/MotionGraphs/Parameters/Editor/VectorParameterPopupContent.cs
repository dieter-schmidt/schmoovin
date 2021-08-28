using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    public class VectorParameterPopupContent : PopupWindowContent
    {
        private SerializedObject m_SerializedObject = null;

        public VectorParameterPopupContent(SerializedObject so)
        {
            m_SerializedObject = so;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(240, EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3);
        }

        public override void OnGUI(Rect rect)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.x += 2;
            rect.width -= 6;
            EditorGUI.PropertyField(rect, m_SerializedObject.FindProperty("m_StartingValue"));
            m_SerializedObject.ApplyModifiedProperties();
        }
    }
}
