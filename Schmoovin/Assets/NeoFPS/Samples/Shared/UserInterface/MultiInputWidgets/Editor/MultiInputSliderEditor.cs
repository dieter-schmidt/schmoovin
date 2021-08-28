using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputSlider))]
	[CanEditMultipleObjects]
	public class MultiInputSliderEditor : MultiInputWidgetEditor
	{
		public override void OnChildInspectorGUI ()
		{
			base.OnChildInspectorGUI ();

			EditorGUILayout.LabelField ("Slider", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_IncrementButton"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DecrementButton"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_SliderRect"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_SliderBarRect"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_SliderFillRect"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ReadoutRect"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Readout"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_MinValue"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_MaxValue"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Value"));

			EditorGUILayout.LabelField ("Input Field", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_SelectionColor"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_CaretBlinkRate"));

			EditorGUILayout.LabelField ("Events", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnValueChanged"));
		}
	}
}