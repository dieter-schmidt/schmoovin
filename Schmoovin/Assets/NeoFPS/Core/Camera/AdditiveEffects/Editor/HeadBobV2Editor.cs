using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(HeadBobV2), true)]
    public class HeadBobV2Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinLerpSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxLerpSpeed"));

            // Draw bob curves
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bob Curves", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();

            // Horizontal bob
            var rect = EditorGUILayout.GetControlRect();
            rect.width -= 100;
            var curveProp = serializedObject.FindProperty("m_HorizontalCurve");
            var curveValue = curveProp.animationCurveValue;
            curveValue = EditorGUI.CurveField(rect, "Horizontal", curveValue, Color.red, new Rect(-1f, -1f, 2f, 2f));
            if (CheckCurve(curveValue))
                curveProp.animationCurveValue = curveValue;

            rect.x += rect.width;
            rect.width = 100;
            EditorGUI.PropertyField(rect, serializedObject.FindProperty("m_HorizontalDistance"), GUIContent.none);

            // Vertical bob
            rect = EditorGUILayout.GetControlRect();
            rect.width -= 100;
            curveProp = serializedObject.FindProperty("m_VerticalCurve");
            curveValue = curveProp.animationCurveValue;
            curveValue = EditorGUI.CurveField(rect, "Vertical", curveValue, Color.green, new Rect(-1f, -1f, 2f, 2f));
            if (CheckCurve(curveValue))
                curveProp.animationCurveValue = curveValue;

            rect.x += rect.width;
            rect.width = 100;
            EditorGUI.PropertyField(rect, serializedObject.FindProperty("m_VerticalDistance"), GUIContent.none);

            // Roll bob
            rect = EditorGUILayout.GetControlRect();
            rect.width -= 100;
            curveProp = serializedObject.FindProperty("m_RollCurve");
            curveValue = curveProp.animationCurveValue;
            curveValue = EditorGUI.CurveField(rect, "Roll", curveValue, Color.blue, new Rect(-1f, -1f, 2f, 2f));
            if (CheckCurve(curveValue))
                curveProp.animationCurveValue = curveValue;

            rect.x += rect.width;
            rect.width = 100;
            EditorGUI.PropertyField(rect, serializedObject.FindProperty("m_RollAngle"), GUIContent.none);

            EditorGUILayout.EndVertical();

            var aimCompensation = serializedObject.FindProperty("m_UseAimCompensation");
            EditorGUILayout.PropertyField(aimCompensation);
            if (aimCompensation.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTransform"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimLayers"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Damping"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        static bool CheckCurve(AnimationCurve curve)
        {
            if (curve.length >= 2)
            {
                curve.preWrapMode = WrapMode.Loop;
                curve.postWrapMode = WrapMode.Loop;
                SetKeyframeTime(curve, 0, -1f);
                SetKeyframeTime(curve, curve.length - 1, 1f);

                return true;
            }
            else
                return false;
        }

        static void SetKeyframeTime(AnimationCurve curve, int index, float time)
        {
            var k = curve.keys[index];
            k.time = time;
            curve.MoveKey(index, k);
        }
    }
}