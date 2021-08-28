using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputButton), true)]
	[CanEditMultipleObjects]
	public class MultiInputButtonEditor : MultiInputWidgetEditor
	{
		public override void OnChildInspectorGUI ()
		{
			EditorGUILayout.LabelField ("Button", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_OnClick"));
		}
	}
}