using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    public abstract class DataWizard : PopupWindowContent
    {
        private bool m_Initialised = false;

        public MotionGraphContainer container { get; set; }
        public SerializedProperty property { get; set; }

        public abstract string dataName { get; set; }
        public abstract Type dataType { get; }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200f, 62f);
        }

        public override void OnGUI(Rect rect)
        {
            if (!m_Initialised)
            {
                GetInitialValue();
                m_Initialised = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50f));
            dataName = EditorGUILayout.TextField("", dataName, GUILayout.Width(139f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value", GUILayout.Width(50f));
            DrawValueField(139f);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create"))
            {
                var so = MotionGraphEditorFactory.CreateMotionData(new SerializedObject(container), dataType);
                property.FindPropertyRelative("m_Data").objectReferenceValue = so;
                property.serializedObject.ApplyModifiedProperties();

                SerializedObject dataSO = new SerializedObject(so);
                dataSO.FindProperty("m_Name").stringValue = dataName;
                SetNewDataProperties(dataSO);
                dataSO.ApplyModifiedPropertiesWithoutUndo();

                EditorWindow.GetWindow<PopupWindow>().Close();
            }
        }

        public abstract void DrawValueField(float width);

        protected abstract void GetInitialValue();
        protected abstract void SetNewDataProperties(SerializedObject so);
    }
}
