using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputButtonGroup), true)]
	[CanEditMultipleObjects]
	public class MultiInputButtonGroupEditor : MultiInputWidgetEditor
	{
		public override void OnChildInspectorGUI ()
		{
			EditorGUILayout.LabelField ("Buttons", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DefocusOnPress"), true);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Buttons"), true);
		}
	}
}