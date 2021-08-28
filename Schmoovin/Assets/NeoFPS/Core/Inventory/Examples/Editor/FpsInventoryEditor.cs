using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsInventoryBase), true)]
    public class FpsInventoryEditor : Editor
    {
        private ReorderableList m_StartingOrderList = null;
        private ReorderableList m_StartingItemsList = null;
        
        protected virtual void OnEnable ()
        {
            m_StartingOrderList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_StartingOrder"), true, true, false, false);
            m_StartingOrderList.drawHeaderCallback = DrawStartingOrderListHeader;
            m_StartingOrderList.drawElementCallback = DrawStartingOrderListElements;

            m_StartingItemsList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_StartingItems"), true, true, true, true);
            m_StartingItemsList.drawHeaderCallback = DrawStartingItemsListHeader;
            m_StartingItemsList.drawElementCallback = DrawStartingItemsListElements;
            m_StartingItemsList.onRemoveCallback = OnRemoveStartingItemsListElement;
        }

        protected virtual void OnDisable ()
        {
            m_StartingItemsList = null;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Inventory", EditorStyles.boldLabel);

            SerializedProperty rootProperty = serializedObject.FindProperty("m_WieldableRoot");
            SerializedProperty scaleProperty = serializedObject.FindProperty("m_WieldableRootScale");
            EditorGUILayout.PropertyField(rootProperty);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WieldableRootScale"));
            if (GUILayout.Button("Apply Scale") && rootProperty.objectReferenceValue != null)
            {
                float scale = scaleProperty.floatValue;
                ((Transform)rootProperty.objectReferenceValue).localScale = new Vector3(scale, scale, scale);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DropTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DropVelocity"));

            EditorGUILayout.Space();
            OnExtendedInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Starting State", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BackupItem"));
            m_StartingItemsList.DoLayoutList();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingSlotChoice"));
            int startingSlotChoice = serializedObject.FindProperty("m_StartingSlotChoice").enumValueIndex;
            if (startingSlotChoice == 2)
                m_StartingOrderList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnExtendedInspectorGUI ()
        {
        }

        protected virtual string GetStartingOrderLabel (int index)
        {
            return "Slot " + (index + 1).ToString("D2");
        }

        void DrawStartingOrderListHeader (Rect rect)
        {
            EditorGUI.LabelField (rect, "Starting Order");
        }

        void DrawStartingOrderListElements (Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 4;
            EditorGUI.LabelField(rect, GetStartingOrderLabel(m_StartingOrderList.serializedProperty.GetArrayElementAtIndex(index).intValue));
        }

        void DrawStartingItemsListHeader (Rect rect)
        {
            EditorGUI.LabelField (rect, "Starting Items");
        }

        void DrawStartingItemsListElements (Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 4;
            NeoFpsEditorGUI.PrefabField(rect, m_StartingItemsList.serializedProperty.GetArrayElementAtIndex(index));
        }

        void OnRemoveStartingItemsListElement (ReorderableList list)
        {
            int index = list.index;
            if (index != -1)
            {
                m_StartingItemsList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = null;
                m_StartingItemsList.serializedProperty.DeleteArrayElementAtIndex(index);
                list.index = -1;
            }
        }
    }
}