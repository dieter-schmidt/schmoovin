using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (UiStyledButton), true)]
	[CanEditMultipleObjects]
	public class UiStyledButtonEditor : ButtonEditor
	{
		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Style"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Interactable"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TargetGraphic"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Navigation"), true);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnClick"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnCancel"));

			serializedObject.ApplyModifiedProperties ();
		}
	}
}