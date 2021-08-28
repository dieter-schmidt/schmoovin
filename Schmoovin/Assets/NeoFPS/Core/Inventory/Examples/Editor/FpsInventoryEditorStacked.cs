using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsInventoryStacked), true)]
    public class FpsInventoryEditorStacked : FpsInventoryEditor
    {
        private readonly GUIContent k_StackCountLabel = new GUIContent(
            "Stack Count", "The number of stacks in the inventory (corresponds to the keyboard number keys)"
            );

        protected override void OnExtendedInspectorGUI ()
        {
            EditorGUILayout.LabelField("Stacked Inventory", EditorStyles.boldLabel);

            var slotCountProperty = serializedObject.FindProperty("m_SlotCount");

            int oldStackCount = slotCountProperty.intValue / 10;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DuplicateBehaviour"));
            int newStackCount = Mathf.Clamp(EditorGUILayout.IntField(k_StackCountLabel, oldStackCount), 1, 10);

            if (newStackCount != oldStackCount)
            {
                slotCountProperty.intValue = newStackCount * 10;
            }
        }
        
        protected override string GetStartingOrderLabel(int index)
        {
            int maxStackSize = serializedObject.FindProperty("m_MaxStackSize").intValue;
            int slot = (index / maxStackSize);
            int stackPosition = index - (slot * maxStackSize);
            return string.Format("Slot {0:D2} Item {1:D2}", slot + 1, stackPosition + 1);
        }
    }
}