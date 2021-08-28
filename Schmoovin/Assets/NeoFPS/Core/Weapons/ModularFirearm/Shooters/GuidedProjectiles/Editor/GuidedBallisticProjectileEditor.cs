using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.ModularFirearms;
using System;
using System.Collections.Generic;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof (GuidedBallisticProjectile))]
    public class GuidedBallisticProjectileEditor : Editor
    {
        GUIContent m_ChooseTrackerLabel = null;
        GUIContent chooseTrackerLabel
        {
            get
            {
                if (m_ChooseTrackerLabel == null)
                    m_ChooseTrackerLabel = new GUIContent("Set the tracker type");
                return m_ChooseTrackerLabel;
            }
        }

        GUIContent m_ChooseMotorLabel = null;
        GUIContent chooseMotorLabel
        {
            get
            {
                if (m_ChooseMotorLabel == null)
                    m_ChooseMotorLabel = new GUIContent("Set the motor type");
                return m_ChooseMotorLabel;
            }
        }

        [InitializeOnLoadMethod]
        static void RegisterKnownComponents()
        {
            RegisterTrackerType<NearestObjectWithTagTracker>("Ranged Tag Tracker");
            RegisterTrackerType<TargetingSystemTracker>("Target System Tracker");
            RegisterTrackerType<PlayerTracker>("Player Tracker");

            RegisterMotorType<SimpleSteeringMotor>("Simple Steering");
            RegisterMotorType<DrunkMissileMotor>("Drunk Missile");
        }

        class ComponentInfo
        {
            public Type componentType = null;
            public string name = string.Empty;

            public ComponentInfo(Type t, string n)
            {
                componentType = t;
                name = n;
            }
        }

        private static List<ComponentInfo> s_TrackerComponents = new List<ComponentInfo>();
        private static List<ComponentInfo> s_MotorComponents = new List<ComponentInfo>();

        public static void RegisterTrackerType<T>(string label) where T : IGuidedProjectileTargetTracker
        {
            s_TrackerComponents.Add(new ComponentInfo(typeof(T), label));
        }

        public static void RegisterMotorType<T>(string label) where T : IGuidedProjectileMotor
        {
            s_MotorComponents.Add(new ComponentInfo(typeof(T), label));
        }




        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinVisibleDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ForgetIgnoreRoot"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QueryTriggers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RecycleDelay"));
            EditorGUILayout.Space();
            // Dropdowns for tracker
            if (EditorGUILayout.DropdownButton(chooseTrackerLabel, FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var tracker in s_TrackerComponents)
                    menu.AddItem(new GUIContent(tracker.name), false, OnTrackerSelected, tracker.componentType);
                menu.ShowAsContext();
            }

            // Dropdowns for motor
            if (EditorGUILayout.DropdownButton(chooseMotorLabel, FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var motor in s_MotorComponents)
                    menu.AddItem(new GUIContent(motor.name), false, OnMotorSelected, motor.componentType);
                menu.ShowAsContext();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnTrackerSelected (object t)
        {
            // Get the projectile component
            var cast = serializedObject.targetObject as GuidedBallisticProjectile;

            // Remove existing tracker
            var existing = cast.GetComponent<IGuidedProjectileTargetTracker>();
            if (existing != null)
                DestroyImmediate(existing as MonoBehaviour);

            // Add new tracker
            cast.gameObject.AddComponent((Type)t);
        }

        void OnMotorSelected(object t)
        {
            // Get the projectile component
            var cast = serializedObject.targetObject as GuidedBallisticProjectile;

            // Remove existing motor
            var existing = cast.GetComponent<IGuidedProjectileMotor>();
            if (existing != null)
                DestroyImmediate(existing as MonoBehaviour);

            // Add new motor
            cast.gameObject.AddComponent((Type)t);
        }
    }
}