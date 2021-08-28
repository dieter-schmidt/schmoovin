using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsInventorySwappable), true)]
    public class FpsInventoryEditorSwappable : FpsInventoryEditor
    {
        protected override void OnExtendedInspectorGUI ()
        {
            EditorGUILayout.LabelField("Swappable Inventory", EditorStyles.boldLabel);

            var groupSizesProperty = serializedObject.FindProperty("m_GroupSizes");
            var slotCountProperty = serializedObject.FindProperty("m_SlotCount");
            int oldSlotCount = slotCountProperty.intValue;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SwapAction"), true);

            EditorGUILayout.LabelField("Category sizes:");
            if (groupSizesProperty.arraySize > 0)
			{
				++EditorGUI.indentLevel;
				for (int i = 0; i < groupSizesProperty.arraySize; ++i)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(FpsSwappableCategory.names[i]);
					EditorGUILayout.PropertyField(groupSizesProperty.GetArrayElementAtIndex(i), new GUIContent());
					EditorGUILayout.EndHorizontal();
				}
				--EditorGUI.indentLevel;

				int newSlotCount = GetSlotCount(groupSizesProperty);
				if (newSlotCount != oldSlotCount)
					slotCountProperty.intValue = newSlotCount;

				if (newSlotCount <= 0 || newSlotCount > 10)
					EditorGUILayout.HelpBox("Total slot count must be greater than zero and less than or equal to 10", MessageType.Error);
			}
			else
				EditorGUILayout.HelpBox("Number of categories is zero. Please check the FpsSwappableCategory generated constant.", MessageType.Error);
        }

        int GetSlotCount(SerializedProperty groupSizesProperty)
        {
            int result = 0;
            for (int i = 0; i < groupSizesProperty.arraySize; ++i)
                result += groupSizesProperty.GetArrayElementAtIndex(i).intValue;
            return result;
        }
    }
}