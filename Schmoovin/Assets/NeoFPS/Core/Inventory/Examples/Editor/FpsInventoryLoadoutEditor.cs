using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FpsInventoryLoadout), true)]
    public class FpsInventoryLoadoutEditor : Editor
    {
        private ReorderableList m_ItemList = null;

        private void OnEnable()
        {
            m_ItemList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Items"), true, true, true, true);
            m_ItemList.drawElementCallback = DrawItemListElement;
            m_ItemList.drawHeaderCallback = DrawItemListHeader;
        }

        private void DrawItemListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Loadout Items");
        }

        private void DrawItemListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 4;
            NeoFpsEditorGUI.PrefabField(rect, m_ItemList.serializedProperty.GetArrayElementAtIndex(index));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            m_ItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
