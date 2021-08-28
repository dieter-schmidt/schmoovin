using System;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public class SettingsIntermediate_Layers : SettingsIntermediateBase
    {
        public string[] tags = new string[0];
        public string[] layers = new string[0];

        const int k_Reserved = 8;

        protected override bool ApplySettingsInternal(SerializedObject serializedObject)
        {
            try
            {
                var prop = serializedObject.FindProperty("tags");
                prop.arraySize = tags.Length;
                for (int i = 0; i < tags.Length; ++i)
                    prop.GetArrayElementAtIndex(i).stringValue = tags[i];

                prop = serializedObject.FindProperty("layers");
                for (int i = 0; i < layers.Length; ++i)
                    prop.GetArrayElementAtIndex(i + k_Reserved).stringValue = layers[i];

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to apply layer settings. Error: " + e.Message);
                return false;
            }
        }
    }
}