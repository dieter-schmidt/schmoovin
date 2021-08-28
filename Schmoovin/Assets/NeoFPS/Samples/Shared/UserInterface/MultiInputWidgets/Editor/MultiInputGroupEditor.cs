using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputGroup), true)]
	[CanEditMultipleObjects]
	public class MultiInputGroupEditor : MultiInputWidgetEditor
	{
		private ReorderableList m_ContentsList = null;

        protected override void OnEnable ()
		{
			base.OnEnable ();

			m_ContentsList = new ReorderableList (
				serializedObject,
				serializedObject.FindProperty ("m_Contents"),
				true, true, true, true
			);

			m_ContentsList.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField (rect, "Contents");
			};

			m_ContentsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				// Shift down by 2 (compensate for single line height)
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;

				// Get the element
				var element = m_ContentsList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			};
		}

		public override void OnChildInspectorGUI ()
		{
			EditorGUILayout.LabelField ("Group", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ExpandButton"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_StartExpanded"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_IconImage"));

			m_ContentsList.DoLayoutList ();
		}
	}
}