using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    public class FloatDataWizard : DataWizard
    {
        private string m_DataName = "myFloat";
        private float m_DataValue = 0f;

        public override string dataName
        {
            get { return m_DataName; }
            set { m_DataName = value; }
        }

        public override Type dataType
        {
            get { return typeof(FloatData); }
        }

        public override void DrawValueField(float width)
        {
            m_DataValue = EditorGUILayout.FloatField("", m_DataValue, GUILayout.Width(width));
        }

        protected override void GetInitialValue()
        {
            m_DataValue = property.FindPropertyRelative("m_Value").floatValue;
        }

        protected override void SetNewDataProperties(SerializedObject so)
        {
            so.FindProperty("m_Value").floatValue = m_DataValue;
        }
    }
}
