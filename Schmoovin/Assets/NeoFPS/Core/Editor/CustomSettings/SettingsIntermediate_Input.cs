using System;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public class SettingsIntermediate_Input : SettingsIntermediateBase
    {
        public Axis[] m_Axes = new Axis[0];

        [Serializable]
        public class Axis
        {
            public string m_Name = string.Empty;
            public string descriptiveName = string.Empty;
            public string descriptiveNegativeName = string.Empty;
            public string negativeButton = string.Empty;
            public string positiveButton = string.Empty;
            public string altNegativeButton = string.Empty;
            public string altPositiveButton = string.Empty;
            public float gravity = 0f;
            public float dead = 0f;
            public float sensitivity = 0f;
            public bool snap = false;
            public bool invert = false;
			public int type = 0; // enum
            public int axis = 0; // enum
            public int joyNum = 0; // enum
        }

        protected override bool ApplySettingsInternal(SerializedObject serializedObject)
        {
            try
            {
                var prop = serializedObject.FindProperty("m_Axes");
                prop.arraySize = m_Axes.Length;

                for (int i = 0; i < m_Axes.Length; ++i)
                {
                    var axis = prop.GetArrayElementAtIndex(i);

                    axis.FindPropertyRelative("m_Name").stringValue = m_Axes[i].m_Name;
                    axis.FindPropertyRelative("descriptiveName").stringValue = m_Axes[i].descriptiveName;
                    axis.FindPropertyRelative("descriptiveNegativeName").stringValue = m_Axes[i].descriptiveNegativeName;
                    axis.FindPropertyRelative("negativeButton").stringValue = m_Axes[i].negativeButton;
                    axis.FindPropertyRelative("positiveButton").stringValue = m_Axes[i].positiveButton;
                    axis.FindPropertyRelative("altNegativeButton").stringValue = m_Axes[i].altNegativeButton;
                    axis.FindPropertyRelative("altPositiveButton").stringValue = m_Axes[i].altPositiveButton;
                    axis.FindPropertyRelative("gravity").floatValue = m_Axes[i].gravity;
                    axis.FindPropertyRelative("dead").floatValue = m_Axes[i].dead;
                    axis.FindPropertyRelative("sensitivity").floatValue = m_Axes[i].sensitivity;
                    axis.FindPropertyRelative("snap").boolValue = m_Axes[i].snap;
                    axis.FindPropertyRelative("invert").boolValue = m_Axes[i].invert;
                    axis.FindPropertyRelative("type").enumValueIndex = m_Axes[i].type;
                    axis.FindPropertyRelative("axis").enumValueIndex = m_Axes[i].axis;
                    axis.FindPropertyRelative("joyNum").enumValueIndex = m_Axes[i].joyNum;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to apply input settings. Error: " + e.Message);
                return false;
            }
        }
    }
}