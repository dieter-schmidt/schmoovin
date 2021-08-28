using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.Parameters
{
    public class ParameterWizard : PopupWindowContent
    {
        private string m_ParameterName = "parameterName";

        public MotionGraphContainer container { get; set; }
        public SerializedProperty property { get; set; }
        public Type parameterType { get; set; }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200f, 42f);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50f));
            m_ParameterName = EditorGUILayout.TextField("", m_ParameterName, GUILayout.Width(139f));
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create"))
            {
                var so = MotionGraphEditorFactory.CreateParameter(new SerializedObject(container), parameterType);
                property.objectReferenceValue = so;
                property.serializedObject.ApplyModifiedProperties();

                SerializedObject parameterSO = new SerializedObject(so);
                parameterSO.FindProperty("m_Name").stringValue = m_ParameterName;
                parameterSO.ApplyModifiedPropertiesWithoutUndo();

                EditorWindow.GetWindow<PopupWindow>().Close();
            }
        }
    }
}
