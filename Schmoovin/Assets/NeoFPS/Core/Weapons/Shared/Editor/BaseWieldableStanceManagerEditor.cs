using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor
{
    [CustomEditor(typeof (BaseWieldableStanceManager), true)]
    public class BaseWieldableStanceManagerEditor : Editor
    {
        private GUIStyle m_FoldoutStyle = null;
        public GUIStyle foldoutStyle
        {
            get
            {
                if (m_FoldoutStyle == null)
                {
                    m_FoldoutStyle = new GUIStyle(EditorStyles.foldout);
                    m_FoldoutStyle.fontStyle = FontStyle.Bold;
                }
                return m_FoldoutStyle;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            InspectWieldableStanceManager();

            InspectStances();

            serializedObject.ApplyModifiedProperties();
        }

        void InspectStances()
        {
            var stances = serializedObject.FindProperty("m_Stances");

            GUILayout.Space(2);
            if (GUILayout.Button("Add Stance"))
                AddStance(stances);

            for (int i = 0; i < stances.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Space(12);
                EditorGUILayout.BeginVertical();

                var stance = stances.GetArrayElementAtIndex(i);
                var expanded = stance.FindPropertyRelative("expanded");
                var nameProp = stance.FindPropertyRelative("name");

                expanded.boolValue = EditorGUILayout.Foldout(expanded.boolValue, nameProp.stringValue, true, foldoutStyle);
                if (expanded.boolValue)
                {
                    EditorGUILayout.PropertyField(nameProp);
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("animatorBoolKey"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("position"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("rotation"));

                    EditorGUILayout.LabelField("In Transition", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("inTime"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("inPositionBlend"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("inRotationBlend"));

                    EditorGUILayout.LabelField("Out Transition", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("outTime"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("outPositionBlend"));
                    EditorGUILayout.PropertyField(stance.FindPropertyRelative("outRotationBlend"));

                    EditorGUILayout.BeginHorizontal();
                    
                    GUI.enabled = i > 0;
                    bool blocked = false;
                    if (GUILayout.Button("Move Up"))
                    {
                        SerializedArrayUtility.Move(stances, i, i - 1);
                        blocked = true;
                    }

                    GUI.enabled = !blocked && (i < stances.arraySize - 1);
                    if (GUILayout.Button("Move Down"))
                    {
                        SerializedArrayUtility.Move(stances, i, i + 1);
                        blocked = true;
                    }

                    GUI.enabled = !blocked;
                    if (GUILayout.Button("Remove"))
                    {
                        SerializedArrayUtility.RemoveAt(stances, i);
                        blocked = true;
                    }
                    else
                        blocked = false;

                    EditorGUILayout.EndHorizontal();

                    if (blocked)
                        break;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Stance"))
                AddStance(stances);

            GUILayout.Space(2);
        }

        void AddStance(SerializedProperty prop)
        {
            ++prop.arraySize;
            var newStance = prop.GetArrayElementAtIndex(prop.arraySize - 1);
            newStance.FindPropertyRelative("name").stringValue = "newStance";
            newStance.FindPropertyRelative("expanded").boolValue = true;
            newStance.FindPropertyRelative("animatorBoolKey").stringValue = string.Empty;
            newStance.FindPropertyRelative("position").vector3Value = Vector3.zero;
            newStance.FindPropertyRelative("rotation").vector3Value = Vector3.zero;
            newStance.FindPropertyRelative("inTime").floatValue = 0.5f;
            newStance.FindPropertyRelative("inPositionBlend").enumValueIndex = 2;
            newStance.FindPropertyRelative("inRotationBlend").enumValueIndex = 3;
            newStance.FindPropertyRelative("outTime").floatValue = 0.5f;
            newStance.FindPropertyRelative("outPositionBlend").enumValueIndex = 2;
            newStance.FindPropertyRelative("outRotationBlend").enumValueIndex = 3;
        }

        protected virtual void InspectWieldableStanceManager()
        {
            var itr = serializedObject.GetIterator();
            while (itr.NextVisible(true))
            {
                if (itr.name == "m_Stances")
                {
                    if (!itr.NextVisible(false))
                        break;
                }
                else
                    EditorGUILayout.PropertyField(itr, true);
            }
        }
    }
}