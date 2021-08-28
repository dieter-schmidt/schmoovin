using UnityEngine;
using UnityEditor;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public abstract class WieldableRootStep : NeoFpsWizardStep
    {
        [Tooltip("The type of inventory the weapon (and character) should use.")]
        public int inventoryType = -1;
        [Tooltip("The maximum number of this item that the player character can hold.")]
        public int maxQuantity = 5;
        [Tooltip("The image to use in the inventory HUD.")]
        public Sprite displayImage = null;
        [FpsInventoryKey(required = true), Tooltip("The inventory item key for the weapon. These can be changed via the generated constants manager.")]
        public int itemKey = 0;
        [Tooltip("The weapon slot index the weapon should use. This is the number key - 1.")]
        public int slotIndex = -1;
        [Tooltip("The stack that the weapon is placed in. This is the number key - 1.")]
        public int inventoryStackIndex = -1;
        [Tooltip("The which order in the stack the weapon should appear in. 0 is higher up the list.")]
        public int inventoryStackPriority = 0;
        [Tooltip("The swappable inventory category for the weapon.")]
        public FpsSwappableCategory category = FpsSwappableCategory.Melee;
        
        static readonly string[] inventoryOptions =
        {
            "Quick-switch. This is the standard inventory where each weapon is assigned a number that corresponds to a slot.",
            "Swappable. Each weapon is assigned a category. If the character tries to pick up a weapon and has the max number for that category, then they will drop the last selected.",
            "Stacked. Weapons are organised into stacks which correspond to a weapon slot. Selecting the same slot multiple times cycles through the weapons in that stack.",
            "Custom. Use your own inventory solution."
        };

        static readonly string[] inventorySummaries =
        {
            "Quick-switch",
            "Swappable",
            "Stacked",
            "Custom"
        };

        protected abstract FpsSwappableCategory GetDefaultCategory();
        protected abstract bool canHoldMultiple
        {
            get;
        }

        protected virtual void Awake()
        {
            category = GetDefaultCategory();
        }

        protected virtual void OnValidate()
        {
            if (maxQuantity < 1)
                maxQuantity = 1;
            slotIndex = Mathf.Clamp(slotIndex, -1, 10);
            inventoryStackIndex = Mathf.Clamp(inventoryStackIndex, -1, 10);
        }
        
        protected bool InspectInventoryOptions(SerializedObject serializedObject)
        {
            bool valid = NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("inventoryType"), inventoryOptions);
            switch (inventoryType)
            {
                case 0: // Quick-switch
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("itemKey"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("slotIndex"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("displayImage"));
                    if (canHoldMultiple)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxQuantity"));
                    else
                        maxQuantity = 1;
                    break;
                case 1: // Swappable
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("itemKey"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("category"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("displayImage"));
                    if (canHoldMultiple)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxQuantity"));
                    else
                        maxQuantity = 1;
                    break;
                case 2: // Stacked
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("itemKey"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryStackIndex"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryStackPriority"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("displayImage"));
                    if (canHoldMultiple)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxQuantity"));
                    else
                        maxQuantity = 1;
                    break;
            }

            if (serializedObject.FindProperty("itemKey").intValue == 0)
                valid = false;

            return valid;
        }

        protected void SummariseInventoryOptions()
        {
            WizardGUI.MultiChoiceSummary("Inventory Type", inventoryType, inventorySummaries);
            switch (inventoryType)
            {
                case 0: // Quick-switch
                    WizardGUI.DoSummary("Item Key", itemKey);
                    WizardGUI.DoSummary("Slot Index", slotIndex);
                    WizardGUI.ObjectSummary("Display Image", displayImage);
                    WizardGUI.DoSummary("Max Quantity", maxQuantity);
                    break;
                case 1: // Swappable
                    WizardGUI.DoSummary("Item Key", itemKey);
                    WizardGUI.DoSummary("Category", category);
                    WizardGUI.ObjectSummary("Display Image", displayImage);
                    WizardGUI.DoSummary("Max Quantity", maxQuantity);
                    break;
                case 2: // Stacked
                    WizardGUI.DoSummary("Item Key", itemKey);
                    WizardGUI.DoSummary("Inventory Stack Index", inventoryStackIndex);
                    WizardGUI.DoSummary("Inventory Stack Priority", inventoryStackPriority);
                    WizardGUI.ObjectSummary("Display Image", displayImage);
                    WizardGUI.DoSummary("Max Quantity", maxQuantity);
                    break;
            }
        }

        public void AddInventoryToObject(GameObject gameObject)
        {
            // Inventory
            switch (inventoryType)
            {
                case 0: // Quick-switch
                    {
                        var wieldable = gameObject.AddComponent<FpsInventoryWieldable>();
                        var wieldableSO = new SerializedObject(wieldable);
                        wieldableSO.FindProperty("m_ItemKey").FindPropertyRelative("m_Value").intValue = itemKey;
                        wieldableSO.FindProperty("m_QuickSlot").intValue = slotIndex;
                        wieldableSO.FindProperty("m_MaxQuantity").intValue = maxQuantity;
                        wieldableSO.FindProperty("m_DisplayImage").objectReferenceValue = displayImage;
                        wieldableSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
                case 1: // Swappable
                    {
                        var wieldable = gameObject.AddComponent<FpsInventoryWieldableSwappable>();
                        var wieldableSO = new SerializedObject(wieldable);
                        wieldableSO.FindProperty("m_ItemKey").FindPropertyRelative("m_Value").intValue = itemKey;
                        wieldableSO.FindProperty("m_Category").FindPropertyRelative("m_Value").intValue = category;
                        wieldableSO.FindProperty("m_MaxQuantity").intValue = maxQuantity;
                        wieldableSO.FindProperty("m_DisplayImage").objectReferenceValue = displayImage;
                        wieldableSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
                case 2: // Stacked
                    {
                        var wieldable = gameObject.AddComponent<FpsInventoryWieldable>();
                        var wieldableSO = new SerializedObject(wieldable);
                        wieldableSO.FindProperty("m_ItemKey").FindPropertyRelative("m_Value").intValue = itemKey;
                        wieldableSO.FindProperty("m_QuickSlot").intValue = 10 * inventoryStackIndex + inventoryStackPriority;
                        wieldableSO.FindProperty("m_MaxQuantity").intValue = maxQuantity;
                        wieldableSO.FindProperty("m_DisplayImage").objectReferenceValue = displayImage;
                        wieldableSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
            }
        }
    }
}