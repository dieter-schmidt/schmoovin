using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public abstract class NeoFpsWizardStep : ScriptableObject
    {
        [HideInInspector] public NeoFpsWizard wizard = null;

        public abstract string displayName
        {
            get;
        }

        public abstract bool CheckCanContinue(NeoFpsWizard wizard);
        public virtual void CheckStartingState(NeoFpsWizard wizard) { }

        public virtual string[] GetNextSteps()
        {
            return null;
        }

        public void LayoutEditor(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            serializedObject.UpdateIfRequiredOrScript();
            OnInspectorGUI(serializedObject, wizard);
            serializedObject.ApplyModifiedProperties();
        }

        public bool LayoutSummary(NeoFpsWizard wizard, int indent, bool hub)
        {
            if (this.wizard == null)
                this.wizard = wizard;

            bool result = false;
            if (hub)
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(EditorGUIUtility.currentViewWidth - 216)))
                    result = LayoutSummaryInternal(wizard, indent, hub);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                    result = LayoutSummaryInternal(wizard, indent, hub);
            }
            return result;
        }

        bool LayoutSummaryInternal(NeoFpsWizard wizard, int indent, bool hub)
        {
            bool result = false;

            GUILayout.Space(indent * 20f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    float w = EditorGUIUtility.currentViewWidth;

                    // Foldout
                    if (!hub)
                        GUILayout.Space(10f);
                    EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
                    // Space
                    //GUILayout.FlexibleSpace();
                    // Select button
                    if (GUILayout.Button("Edit", GUILayout.Width(72)))
                        result = true;
                }

                GUILayout.Space(4);

                // Show contents
                OnSummaryGUI(wizard);

                GUILayout.Space(4);
            }

            return result;
        }

        protected abstract void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard);
        protected abstract void OnSummaryGUI(NeoFpsWizard wizard);
    }
}