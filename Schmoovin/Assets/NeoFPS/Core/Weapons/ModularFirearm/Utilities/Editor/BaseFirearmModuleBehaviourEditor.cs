using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(BaseFirearmModuleBehaviour), true)]
    public class BaseFirearmModuleBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartActive"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ActivationMode"));

            OnInspectorGUIInternal();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnInspectorGUIInternal()
        {
            var prop = serializedObject.FindProperty("m_ActivationMode");
            bool inspect = prop.NextVisible(false);
            while (inspect)
            {
                EditorGUILayout.PropertyField(prop);
                inspect = prop.NextVisible(prop.hasVisibleChildren);
            }
        }
    }
}