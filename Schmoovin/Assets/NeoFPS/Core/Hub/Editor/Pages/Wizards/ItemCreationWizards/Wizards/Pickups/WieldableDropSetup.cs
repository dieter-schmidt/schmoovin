using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class WieldableDropSetup : NeoFpsWizardStep
    {
        [Tooltip("The wieldable item prefab to add to the character inventory on pickup")]
        public GameObject wieldablePrefab = null;
        [Tooltip("Modify the inventory item component on the wieldable item prefab to point at this drop")]
        public bool addToWieldable = false;

        public override string displayName
        {
            get { return "Health Pack Setup"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return wieldablePrefab != null;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("wieldablePrefab"), FilterWieldable);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addToWieldable"));
        }
        
        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Wieldable Prefab", wieldablePrefab);
            WizardGUI.DoSummary("Add To Wieldable", addToWieldable);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }

        static bool FilterWieldable(GameObject obj)
        {
            return obj != null && obj.GetComponent<IWieldable>() != null && obj.GetComponent<FpsInventoryWieldable>() != null;
        }
    }
}