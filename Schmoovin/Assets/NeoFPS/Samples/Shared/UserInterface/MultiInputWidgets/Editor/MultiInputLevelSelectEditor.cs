using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.Samples;
using NeoSaveGames.Serialization;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputLevelSelect))]
	[CanEditMultipleObjects]
	public class MultiInputLevelSelectEditor : MultiInputWidgetEditor
	{
		public override void OnChildInspectorGUI ()
		{
			base.OnChildInspectorGUI ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ScreenshotImage"));
        }
	}
}