using System;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public class SettingsIntermediate_Physics : SettingsIntermediateBase
    {
        public uint[] m_LayerCollisionMatrix = new uint[0];

        protected override bool ApplySettingsInternal(SerializedObject serializedObject)
        {
            try
            {
                var prop = serializedObject.FindProperty("m_LayerCollisionMatrix");

                prop.arraySize = m_LayerCollisionMatrix.Length;
                for (int i = 0; i < m_LayerCollisionMatrix.Length; ++i)
                    prop.GetArrayElementAtIndex(i).longValue = m_LayerCollisionMatrix[i];

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to apply physics settings. Error: " + e.Message);
                return false;
            }
        }
    }
}