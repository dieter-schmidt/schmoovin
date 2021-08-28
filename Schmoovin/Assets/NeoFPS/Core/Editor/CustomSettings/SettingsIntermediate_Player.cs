using System;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public class SettingsIntermediate_Player : SettingsIntermediateBase
    {
        public int m_ActiveColorSpace = 0;

        protected override bool ApplySettingsInternal(SerializedObject serializedObject)
        {
            try
            {
                serializedObject.FindProperty("m_ActiveColorSpace").enumValueIndex = m_ActiveColorSpace;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to apply player settings. Error: " + e.Message);
                return false;
            }
        }
    }
}