using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(WaypointMovingPlatform), true)]
    public class WaypointMovingPlatformEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var waypoints = serializedObject.FindProperty("m_Waypoints");
            var journeyTimes = serializedObject.FindProperty("m_JourneyTimes");
            var circular = serializedObject.FindProperty("m_Circular");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingWaypoint"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpeedCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Delay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnStart"));
            EditorGUILayout.PropertyField(circular);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnStartMoving"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnDestinationReached"));

            int wpCount = waypoints.arraySize;
            int jtCount = journeyTimes.arraySize;
            
            // Check if valid number of journey times
            if ((circular.boolValue && wpCount != jtCount) || (!circular.boolValue && wpCount != jtCount + 1))
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();
            for (int i = 0; i < wpCount; ++i)
            {
                EditorGUILayout.LabelField("Waypoint " + (i + 1), EditorStyles.boldLabel);

                var element = waypoints.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("position"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("rotation"));

                if (i == wpCount - 1)
                {
                    if (circular.boolValue)
                        EditorGUILayout.PropertyField(journeyTimes.GetArrayElementAtIndex(i), new GUIContent("Time to Waypoint 1"));
                }
                else
                    EditorGUILayout.PropertyField(journeyTimes.GetArrayElementAtIndex(i), new GUIContent("Time to Waypoint " + (i + 2)));

                // Transform controls
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Use Current Transform"))
                        UseCurrentTransform(waypoints, i);
                    if (GUILayout.Button("Move To Waypoint"))
                        MoveToWaypoint(waypoints, i);
                }

                // Organisation controls
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = (i > 0);
                    if (GUILayout.Button("Up"))
                        Swap(waypoints, i, i - 1);
                    GUI.enabled = (i < wpCount - 1);
                    if (GUILayout.Button("Down"))
                        Swap(waypoints, i, i + 1);
                    GUI.enabled = true;
                    if (GUILayout.Button("Insert After"))
                        InsertAfter(waypoints, i);
                    GUI.enabled = (wpCount > 1);
                    if (GUILayout.Button("Remove"))
                        Remove(waypoints, i);
                    GUI.enabled = true;
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Waypoint"))
                InsertAfter(waypoints, wpCount - 1);

            serializedObject.ApplyModifiedProperties();
        }

        void UseCurrentTransform (SerializedProperty prop, int index)
        {
            var element = prop.GetArrayElementAtIndex(index);
            var transform = ((MonoBehaviour)serializedObject.targetObject).transform;
            element.FindPropertyRelative("position").vector3Value = transform.position;
            element.FindPropertyRelative("rotation").vector3Value = transform.eulerAngles;
        }

        void MoveToWaypoint(SerializedProperty prop, int index)
        {
            var element = prop.GetArrayElementAtIndex(index);
            var transform = ((MonoBehaviour)serializedObject.targetObject).transform;
            transform.position = element.FindPropertyRelative("position").vector3Value;
            transform.rotation = Quaternion.Euler(element.FindPropertyRelative("rotation").vector3Value);
        }
        
        void Swap(SerializedProperty prop, int i1, int i2)
        {
            var e1 = prop.GetArrayElementAtIndex(i1);
            var e2 = prop.GetArrayElementAtIndex(i2);

            Vector3 pSwap = e1.FindPropertyRelative("position").vector3Value;
            Vector3 rSwap = e1.FindPropertyRelative("rotation").vector3Value;

            e1.FindPropertyRelative("position").vector3Value = e2.FindPropertyRelative("position").vector3Value;
            e1.FindPropertyRelative("rotation").vector3Value = e2.FindPropertyRelative("rotation").vector3Value;

            e2.FindPropertyRelative("position").vector3Value = pSwap;
            e2.FindPropertyRelative("rotation").vector3Value = rSwap;
        }

        void InsertAfter(SerializedProperty prop, int index)
        {
            int arraySize = prop.arraySize++;
            for (int i = arraySize; i > index; --i)
            {
                var e1 = prop.GetArrayElementAtIndex(i);
                var e2 = prop.GetArrayElementAtIndex(i - 1);
                e1.FindPropertyRelative("position").vector3Value = e2.FindPropertyRelative("position").vector3Value;
                e1.FindPropertyRelative("rotation").vector3Value = e2.FindPropertyRelative("rotation").vector3Value;
            }
        }

        void Remove(SerializedProperty prop, int index)
        {
            int arraySize = prop.arraySize;
            for (int i = index + 1; i < arraySize; ++i)
            {
                var e1 = prop.GetArrayElementAtIndex(i - 1);
                var e2 = prop.GetArrayElementAtIndex(i);
                e1.FindPropertyRelative("position").vector3Value = e2.FindPropertyRelative("position").vector3Value;
                e1.FindPropertyRelative("rotation").vector3Value = e2.FindPropertyRelative("rotation").vector3Value;
            }

            --prop.arraySize;

            serializedObject.ApplyModifiedProperties();

            throw new ExitGUIException();
        }
    }
}