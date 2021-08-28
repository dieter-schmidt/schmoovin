using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NeoFPSEditor
{
    public abstract class BaseReorderableList
    {
        private ReorderableList m_List = null;

        public BaseReorderableList(SerializedProperty prop)
        {
            m_List = new ReorderableList(prop.serializedObject, prop, true, true, true, true);
            m_List.drawHeaderCallback = DrawHeaderCallback;
            m_List.drawElementCallback = DrawListElementInternal;
            m_List.onChangedCallback = OnChanged;
            m_List.onAddCallback = OnAdded;
            m_List.onRemoveCallback = OnRemoved;
            m_List.elementHeightCallback = GetElementHeight;
            m_List.onReorderCallbackWithDetails = OnReordered;
        }

        protected abstract string heading { get; }
        protected abstract void DrawListElement(Rect line1, int index);

        public SerializedProperty serializedProperty
        {
            get { return m_List.serializedProperty; }
        }

        protected SerializedProperty GetListElement(int index)
        {
            return m_List.serializedProperty.GetArrayElementAtIndex(index);
        }

        void DrawListElementInternal(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            DrawListElement(rect, index);
        }

        void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, heading);
        }

        protected virtual void OnAdded(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
        }

        protected virtual void OnRemoved(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        protected virtual void OnReordered(ReorderableList list, int oldIndex, int newIndex)
        { }

        protected virtual void OnChanged(ReorderableList list)
        { }

        protected virtual int GetNumLines(int index)
        {
            return 1;
        }

        float GetElementHeight(int index)
        {
            int lines = GetNumLines(index);
            return (EditorGUIUtility.singleLineHeight * lines) + (EditorGUIUtility.standardVerticalSpacing * lines) + 4;
        }

        public virtual void DoLayoutList()
        {
            m_List.DoLayoutList();
        }
    }
}