using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Samples;

namespace NeoFPSEditor.Samples
{
	[CustomEditor (typeof (MultiInputMultiChoiceBase))]
	[CanEditMultipleObjects]
	public class MultiInputMultiChoiceBaseEditor : MultiInputWidgetEditor
	{
		private ReorderableList m_OptionList = null;

        protected override void OnEnable ()
		{
			base.OnEnable ();

			m_OptionList = new ReorderableList (
				serializedObject,
				serializedObject.FindProperty ("m_Options"),
				true, true, true, true
			);

			m_OptionList.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField (rect, "Options");
			};

			m_OptionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				// Shift down by 2 (compensate for single line height)
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;

				// Get the element
				var element = m_OptionList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			};
		}

		public override void OnChildInspectorGUI ()
		{
			EditorGUILayout.LabelField ("Multi-Choice", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_PrevButton"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_NextButton"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Readout"));

			m_OptionList.DoLayoutList ();
		}
	}
}