using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class KeyRingPickupSetup : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("The keyring prefab to add to the character inventory if not found.")]
        public KeyRing keyRingPrefab = null;

        [SerializeField, Tooltip("The keys contained in this pickup.")]
        public string[] keyCodes = { "demo_key" };
        
        public override string displayName
        {
            get { return "Key-Ring Pickup Setup"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return keyRingPrefab != null;
        }
        
        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            NeoFpsEditorGUI.RequiredPrefabComponentField<KeyRing>(serializedObject.FindProperty("keyRingPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyCodes"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Key-Ring Prefab", keyRingPrefab);
            WizardGUI.ObjectListSummary("Key Codes", keyCodes);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }
    }
}