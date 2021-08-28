using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(SurfaceAudioData))]
	public class SurfaceAudioDataEditor : Editor
	{
		SerializedProperty m_SurfacesArrayProperty = null;
        List<SurfaceDataEditor> m_Editors = new List<SurfaceDataEditor>();

		ClipboardData m_Clipboard = null;

        private class ClipboardData
		{
            public float volume = 1f;
			public AudioClip[] clips = null;

            public ClipboardData(float volume, AudioClip[] clips)
			{
				this.volume = volume;
				this.clips = clips;
			}
		}

		private class SurfaceDataEditor
		{
			SubInspectorTitlebar m_Titlebar = null;
            SerializedProperty m_VolumeProperty = null;
            ReorderableList m_ClipsList = null;
            SurfaceAudioDataEditor m_Editor = null;
            int m_Index = 0;

			public SurfaceDataEditor(SurfaceAudioDataEditor editor, SerializedObject so, int index)
			{
				m_Editor = editor;
				m_Index = index;

				m_Titlebar = new SubInspectorTitlebar();
				m_Titlebar.AddContextOption("Copy", Copy, null);
				m_Titlebar.AddContextOption("Paste", Paste, CanPaste);
				m_Titlebar.getLabel = GetLabel;
				
				SerializedProperty prop = so.FindProperty("m_Data").GetArrayElementAtIndex(index);

				m_ClipsList = new ReorderableList(
					so,
					prop.FindPropertyRelative("clips"),
					true,
					true,
					true,
					true
				);
				m_ClipsList.drawHeaderCallback = DrawClipsHeader;
				m_ClipsList.drawElementCallback = DrawClipsElement;

				m_VolumeProperty = prop.FindPropertyRelative("volume");
			}

			public void DoLayout(bool top)
			{
				bool expanded = m_Titlebar.DoLayout(!top);
				if (expanded)
				{
					EditorGUILayout.PropertyField(m_VolumeProperty);
					m_ClipsList.DoLayoutList();
				}
				EditorGUILayout.Space();
			}

			void Copy()
			{
				AudioClip[] clips = new AudioClip[m_ClipsList.count];
				for (int i = 0; i < m_ClipsList.count; ++i)
					clips[i] = m_ClipsList.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip;

				m_Editor.m_Clipboard = new ClipboardData(
					m_VolumeProperty.floatValue,
					clips
				);
			}

			void Paste()
			{
				m_VolumeProperty.floatValue = m_Editor.m_Clipboard.volume;
				m_ClipsList.serializedProperty.arraySize = m_Editor.m_Clipboard.clips.Length;
				for (int i = 0; i < m_Editor.m_Clipboard.clips.Length; ++i)
				{
					var prop = m_ClipsList.serializedProperty.GetArrayElementAtIndex(i);
					prop.objectReferenceValue = m_Editor.m_Clipboard.clips[i];
				}
			}

			bool CanPaste()
			{
				return m_Editor.m_Clipboard != null;
			}

			string GetLabel()
			{
				return FpsSurfaceMaterial.names[m_Index];
			}

			void DrawClipsHeader(Rect rect)
			{
				EditorGUI.LabelField(rect, "Audio Clips");
			}

			void DrawClipsElement(Rect rect, int index, bool isActive, bool isFocused)
			{
				rect.height -= 4f;
				rect.y += 1f;
				var element = m_ClipsList.serializedProperty.GetArrayElementAtIndex(index);
				EditorGUI.PropertyField(rect, element, GUIContent.none);
			}

			public void Reset()
			{
				m_VolumeProperty.floatValue = 1f;
				m_ClipsList.serializedProperty.ClearArray();
			}
		}

		void RebuildEditors()
		{
			m_Editors.Clear();
			for (int i = 0; i < m_SurfacesArrayProperty.arraySize; ++i)
				m_Editors.Add(new SurfaceDataEditor (this, serializedObject, i));
		}

        void OnEnable()
        {
            m_SurfacesArrayProperty = serializedObject.FindProperty("m_Data");

            RebuildEditors();
        }
        		
        public override void OnInspectorGUI()
        {
			if (m_Editors.Count > 0)
			{
				for (int i = 0; i < m_Editors.Count; ++i)
					m_Editors[i].DoLayout(i == 0);
                serializedObject.ApplyModifiedProperties();
            }
			else
			{
				EditorGUILayout.HelpBox ("FpsSurfaceMaterial incorrectly set up", MessageType.Error);
            }			
		}
	}
}