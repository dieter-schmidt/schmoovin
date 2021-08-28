using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    [CustomEditor(typeof(NeoFpsWizardStep), true)]
    public class NeoFpsWizardStepEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Cannot inspect wizard steps directly", MessageType.Error);
        }

        public void LayoutEditor(NeoFpsWizard wizard)
        {
            serializedObject.UpdateIfRequiredOrScript();
            var cast = target as NeoFpsWizardStep;
            cast.LayoutEditor(serializedObject, wizard);
            serializedObject.ApplyModifiedProperties();
        }

        public void LayoutSummary(NeoFpsWizard wizard, int indent, bool hub)
        {
            serializedObject.UpdateIfRequiredOrScript();
            var cast = target as NeoFpsWizardStep;
            cast.LayoutSummary(wizard, indent, hub);
            serializedObject.ApplyModifiedProperties();
        }
    }
}