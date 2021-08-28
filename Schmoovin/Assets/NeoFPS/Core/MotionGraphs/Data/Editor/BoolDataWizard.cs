using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    public class BoolDataWizard : DataWizard
    {
        private string m_DataName = "myBool";
        private bool m_DataValue = false;

        public override string dataName
        {
            get { return m_DataName; }
            set { m_DataName = value; }
        }

        public override Type dataType
        {
            get { return typeof(BoolData); }
        }

        public override void DrawValueField(float width)
        {
            m_DataValue = EditorGUILayout.Toggle("", m_DataValue, GUILayout.Width(width));
        }

        protected override void GetInitialValue()
        {
            m_DataValue = property.FindPropertyRelative("m_Value").boolValue;
        }

        protected override void SetNewDataProperties(SerializedObject so)
        {
            so.FindProperty("m_Value").boolValue = m_DataValue;
        }
    }
}
