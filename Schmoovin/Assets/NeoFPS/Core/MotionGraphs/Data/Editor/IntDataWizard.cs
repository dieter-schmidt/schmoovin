using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    public class IntDataWizard : DataWizard
    {
        private string m_DataName = "myInt";
        private int m_DataValue = 0;

        public override string dataName
        {
            get { return m_DataName; }
            set { m_DataName = value; }
        }

        public override Type dataType
        {
            get { return typeof(IntData); }
        }

        public override void DrawValueField(float width)
        {
            m_DataValue = EditorGUILayout.IntField("", m_DataValue, GUILayout.Width(width));
        }

        protected override void GetInitialValue()
        {
            m_DataValue = property.FindPropertyRelative("m_Value").intValue;
        }

        protected override void SetNewDataProperties(SerializedObject so)
        {
            so.FindProperty("m_Value").intValue = m_DataValue;
        }
    }
}
