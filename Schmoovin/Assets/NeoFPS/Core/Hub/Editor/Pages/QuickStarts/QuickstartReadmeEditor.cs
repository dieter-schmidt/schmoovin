using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
    [CustomEditor(typeof(QuickstartReadme))]
    public class QuickstartReadmeEditor : ReadmeAssetEditor
    {
        protected override void OnPreEdit()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subFolder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pageName"));
        }
    }
}