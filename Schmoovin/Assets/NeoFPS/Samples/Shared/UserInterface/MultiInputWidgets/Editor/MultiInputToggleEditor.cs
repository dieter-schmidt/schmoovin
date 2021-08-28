using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputToggle))]
	[CanEditMultipleObjects]
	public class MultiInputToggleEditor : MultiInputMultiChoiceBaseEditor
	{
		public override void OnChildInspectorGUI ()
		{
			base.OnChildInspectorGUI ();

			EditorGUILayout.LabelField ("Toggle", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ToggleType"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_StartingValue"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnValueChanged"));
		}
	}
}