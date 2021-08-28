using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputWidget), true)]
	[CanEditMultipleObjects]
	public class MultiInputWidgetEditor : Editor
	{
		private ReorderableList m_BackgroundList = null;
        private ReorderableList m_ControlsTextList = null;

        protected virtual void OnEnable ()
		{
			m_BackgroundList = new ReorderableList (
				serializedObject,
				serializedObject.FindProperty ("m_Backgrounds"),
				true, true, true, true
			);
			m_ControlsTextList = new ReorderableList (
				serializedObject,
				serializedObject.FindProperty ("m_ControlsText"),
				true, true, true, true
			);

			m_BackgroundList.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField (rect, "Background Images");
			};
			m_ControlsTextList.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField (rect, "Controls Text");
			};

			m_BackgroundList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				// Shift down by 2 (compensate for single line height)
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;

				// Get the element
				var element = m_BackgroundList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			};
			m_ControlsTextList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				// Shift down by 2 (compensate for single line height)
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;

				// Get the element
				var element = m_ControlsTextList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			};
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

			EditorGUILayout.LabelField ("Widget Base", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Interactable"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Style"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_LabelText"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DescriptionText"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Label"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Description"));

			m_BackgroundList.DoLayoutList ();
			m_ControlsTextList.DoLayoutList ();

			OnChildInspectorGUI ();

			serializedObject.ApplyModifiedProperties ();
		}

		public virtual void OnChildInspectorGUI ()
		{
		}
	}
}