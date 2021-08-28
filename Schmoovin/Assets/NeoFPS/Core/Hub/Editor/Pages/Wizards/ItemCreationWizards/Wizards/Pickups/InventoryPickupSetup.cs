using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class InventoryPickupSetup : NeoFpsWizardStep
    {
        [Tooltip("The item to add to the character inventory.")]
        public FpsInventoryItemBase inventoryItem = null;
        [Tooltip("Should the pickup spawn an item and make itself visible immediately on awake, or manually via scripts.")]
        public bool spawnOnAwake = true;
        [Tooltip("What to do to the pickup once the item has been taken.")]
        public int consumeResult = 0;
        [Tooltip("What to do to the pickup once the item has been taken.")]
        public float respawnDuration = 5f;
        
        static readonly string[] consumeOptions =
        {
            "Destroy. This completely removes the pickup from the world when consumed.",
            "Disable. This disables the render geo and trigger colliders, allowing you to re-enable them later.",
            "Respawn. This disables the render geo and trigger colliders, and then re-enables them after a set duration."
        };

        static readonly string[] consumeSummary =
        {
            "Destroy",
            "Disable",
            "Respawn"
        };

        public override string displayName
        {
            get { return "Inventory Pickup Setup"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return inventoryItem != null;
        }

        void OnValidate()
        {
            respawnDuration = Mathf.Clamp(respawnDuration, 1f, 3600f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            NeoFpsEditorGUI.RequiredPrefabComponentField<FpsInventoryItemBase>(serializedObject.FindProperty("inventoryItem"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnOnAwake"));
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("consumeResult"), consumeOptions);
            if (consumeResult == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("respawnDuration"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Inventory Item", inventoryItem);
            WizardGUI.DoSummary("Spawn On Awake", spawnOnAwake);
            WizardGUI.MultiChoiceSummary("Consume Result", consumeResult, consumeSummary);
            if (consumeResult == 2)
                WizardGUI.DoSummary("Respawn Duration", respawnDuration);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }
    }
}