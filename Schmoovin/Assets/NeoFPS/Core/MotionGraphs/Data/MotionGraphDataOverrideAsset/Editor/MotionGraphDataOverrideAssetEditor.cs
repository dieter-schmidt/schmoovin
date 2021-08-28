using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditorInternal;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion.MotionData
{
    [CustomEditor(typeof(MotionGraphDataOverrideAsset), true)]
    public class MotionGraphDataOverrideAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var graphProp = serializedObject.FindProperty("m_Graph");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(graphProp);
            GUI.enabled = true;
            if (graphProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Graph connection broken. Has the original motion graph asset been removed?", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();
            MotionGraphEditorStyles.DrawSeparator();
            EditorGUILayout.Space();

            var graphSO = new SerializedObject(graphProp.objectReferenceValue);
            var dataProp = graphSO.FindProperty("m_Data");

            var floatOverrides = serializedObject.FindProperty("m_FloatOverrides");
            var intOverrides = serializedObject.FindProperty("m_IntOverrides");
            var boolOverrides = serializedObject.FindProperty("m_BoolOverrides");

            for (int i = 0; i <dataProp.arraySize; ++i)
            {
                var sourceData = dataProp.GetArrayElementAtIndex(i).objectReferenceValue as MotionGraphDataBase;
                if (sourceData == null)
                    continue;

                if (sourceData is FloatData)
                {
                    DrawOverride(floatOverrides, sourceData.dataID, sourceData.name);
                    continue;
                }

                if (sourceData is IntData)
                {
                    DrawOverride(intOverrides, sourceData.dataID, sourceData.name);
                    continue;
                }

                DrawOverride(boolOverrides, sourceData.dataID, sourceData.name);
            }
        }

        void DrawOverride (SerializedProperty prop, int dataID, string name)
        {
            for (int i = 0; i < prop.arraySize; ++i)
            {
                var ovr = prop.GetArrayElementAtIndex(i);
                if (ovr.FindPropertyRelative("m_DataID").intValue == dataID)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(name);
                    EditorGUILayout.PropertyField(ovr.FindPropertyRelative("m_Value"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            var asset = target as MotionGraphDataOverrideAsset;
            asset.CheckOverrides();
        }
    }
}