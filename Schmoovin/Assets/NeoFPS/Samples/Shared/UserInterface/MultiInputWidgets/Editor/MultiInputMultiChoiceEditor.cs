using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputMultiChoice))]
	[CanEditMultipleObjects]
	public class MultiInputMultiChoiceEditor : MultiInputMultiChoiceBaseEditor
	{
		public override void OnChildInspectorGUI ()
		{
			base.OnChildInspectorGUI ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnIndexChanged"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_StartingIndex"));
		}
	}
}