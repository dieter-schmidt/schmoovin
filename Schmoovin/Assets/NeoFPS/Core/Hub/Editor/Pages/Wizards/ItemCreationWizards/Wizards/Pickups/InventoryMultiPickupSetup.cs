using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;
using UnityEditorInternal;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class InventoryMultiPickupSetup : NeoFpsWizardStep
    {
        [Tooltip("The items the pickup contains.")]
        public FpsInventoryItemBase[] inventoryItems = { };
        [Tooltip("Should the items that are collected be replenished immediately.")]
        public bool replenishItems = true;

        private ReorderableList m_ItemList = null;

        public override string displayName
        {
            get { return "Multi-Item Inventory Pickup Setup"; }
        }
        
        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return inventoryItems.Length > 0;
        }

        private void OnDisable()
        {
            m_ItemList = null;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            if (m_ItemList == null)
                m_ItemList = NeoFpsEditorGUI.GetPrefabComponentList< FpsInventoryItemBase>(serializedObject.FindProperty("inventoryItems"));
            m_ItemList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("replenishItems"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectListSummary("Inventory Items", inventoryItems);
            WizardGUI.DoSummary("Replenish Items", replenishItems);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }
    }
}