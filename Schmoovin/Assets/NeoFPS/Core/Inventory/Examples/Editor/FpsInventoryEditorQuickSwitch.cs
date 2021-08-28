using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor (typeof (FpsInventoryQuickSwitch), true)]
    public class FpsInventoryEditorQuickSwitch : FpsInventoryEditor
    {
        protected override void OnExtendedInspectorGUI ()
        {
            EditorGUILayout.LabelField("Quick-Switch Inventory", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlotCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DuplicateBehaviour"));
        }
    }
}