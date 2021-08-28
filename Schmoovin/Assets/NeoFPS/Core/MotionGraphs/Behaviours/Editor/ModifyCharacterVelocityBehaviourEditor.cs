using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(ModifyCharacterVelocityBehaviour))]
    public class ModifyCharacterVelocityBehaviourEditor : MotionGraphBehaviourEditor
    {
        private GUIContent m_LabelSetLocal = null;
        GUIContent labelSetLocal
        {
            get
            {
                if (m_LabelSetLocal == null)
                    m_LabelSetLocal = new GUIContent("Local Velocity", "The target velocity of the character controller relative to its direction.");
                return m_LabelSetLocal;
            }
        }

        private GUIContent m_LabelSetWorld = null;
        GUIContent labelSetWorld
        {
            get
            {
                if (m_LabelSetWorld == null)
                    m_LabelSetWorld = new GUIContent("World Velocity", "The target velocity of the character controller in world space.");
                return m_LabelSetWorld;
            }
        }

        private GUIContent m_LabelClampSpeed = null;
        GUIContent labelClampSpeed
        {
            get
            {
                if (m_LabelClampSpeed == null)
                    m_LabelClampSpeed = new GUIContent("Max Speed", "The maximum speed the character can travel at. If above this, the character's velocity will be clamped.");
                return m_LabelClampSpeed;
            }
        }

        private GUIContent m_LabelMultiply = null;
        GUIContent labelMultiply
        {
            get
            {
                if (m_LabelMultiply == null)
                    m_LabelMultiply = new GUIContent("Multiplier", "A multiplier to apply to the character's velocity.");
                return m_LabelMultiply;
            }
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_When"));

            var what = serializedObject.FindProperty("m_What");
            EditorGUILayout.PropertyField(what);

            switch (what.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_VectorValue"), labelSetLocal);
                    break;
                case 1:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_VectorValue"), labelSetWorld);
                    break;
                case 2:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FloatValue"), labelClampSpeed);
                    break;
                case 3:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FloatValue"), labelMultiply);
                    break;
            }
        }
    }
}
