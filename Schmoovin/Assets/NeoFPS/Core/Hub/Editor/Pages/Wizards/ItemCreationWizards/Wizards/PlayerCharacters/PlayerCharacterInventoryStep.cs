using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;
using UnityEditorInternal;
using NeoFPS.Constants;
using System.Collections.Generic;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterInventoryStep : NeoFpsWizardStep
    {
        [Tooltip("The type of inventory the character should use.")]
        public int inventoryType = -1;
        [Range(2, 10), Tooltip("The number of item quick slots.")]
        public int slotCount = 10;
        [Tooltip("The selection method for the starting slot.")]
        public StartingSlot startingSlotChoice = StartingSlot.Ascending;
        [Tooltip("This array specifies the selection order on start. The highest on the list that exists will be the starting selection.")]
        public int[] startingOrder = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        [Tooltip("An item to use if no wieldables are in the inventory. This could be empty hands or an infinite weapon such as a knife.")]
        public FpsInventoryWieldable backupItem = null;
        [Tooltip("What to do when trying to add an item to the inventory that already exists.")]
        public DuplicateEntryBehaviour duplicateBehaviour = DuplicateEntryBehaviour.Reject;
        [Tooltip("The velocity of any dropped items relative to the character forward direction.")]
        public Vector3 dropVelocity = new Vector3(0f, 2f, 3f);

        [Range(2, 10), Tooltip("The maximum number of items that can stack on a single quick slot.")]
        public int maxStackSize = 10;

        [Range(2, 10), Tooltip("The number of item quick slots.")]
        public int swappableSlotCount = 10;
        [Tooltip("What to do when replacing an old item with a new one.")]
        public SwapAction swapAction = SwapAction.Drop;
        [Range(1, 10), Tooltip("The number of quick slots available for each category.")]
        public int[] groupSizes = new int[0];

        [Tooltip("A selection of inventory items to be added to the inventory on start.")]
        public FpsInventoryItemBase[] startingItemsStandard = new FpsInventoryItemBase[0];
        [Tooltip("A selection of inventory items to be added to the inventory on start.")]
        public FpsInventoryItemBase[] startingItemsSwappable = new FpsInventoryItemBase[0];

        private bool m_CanContinue = false;
        private ReorderableList m_StartingOrderList = null;
        private ReorderableList m_StartingItemsStandardList = null;
        private ReorderableList m_StartingItemsSwappableList = null;

        ReorderableList GetStartingOrderList(SerializedObject serializedObject)
        {
            if (m_StartingOrderList == null)
            {
                m_StartingOrderList = new ReorderableList(serializedObject, serializedObject.FindProperty("startingOrder"), true, true, false, false);
                m_StartingOrderList.drawHeaderCallback = DrawStartingOrderListHeader;
                m_StartingOrderList.drawElementCallback = DrawStartingOrderListElements;
            }
            return m_StartingOrderList;
        }

        static readonly string[] inventoryTypeOptions =
        {
            "Quick-switch. A traditional FPS inventory, where each weapon has a specific number on the keyboard. The quick-switch weapon cycles between the last selected weapons. Used in games like doom and duke nukem.",
            "Swappable. A more modern inventory that allows for a larger range of weapons. Each item is given a specific category, and the character can carry a set number of each category. Attempting to pick up a weapon when carrying the max of that category will drop the last selected weapon. Used in games like halo and call of duty.",
            "Stacked. A variant of the traditional inventory that allows for a larger number of weapons by stacking multiple on each number. Pressing the number multiple times cycles through the weapons in that stack. Used in games like half-life 2.",
            "None. The character should not use the inventory systems included with NeoFPS."
        };

        static readonly string[] inventoryTypeSummaries =
        {
            "Quick-switch.",
            "Swappable",
            "Stacked",
            "None"
        };

        public enum InventoryBehaviour
        {
            QuickSwitch,
            Swappable,
            Stacked,
            None
        }

        public InventoryBehaviour inventory
        {
            get
            {
                if (inventoryType == -1)
                    return InventoryBehaviour.None;
                else
                    return (InventoryBehaviour)inventoryType;
            }
        }

        public override string displayName
        {
            get { return "Inventory";  }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            int count = slotCount;
            switch (inventoryType)
            {
                case 1:
                    count = swappableSlotCount;
                    break;
                case 2:
                    count = slotCount * maxStackSize;
                    break;
            }

            if (startingOrder.Length != count)
            {
                int[] newStartingOrder = new int[count];

                if (count > startingOrder.Length)
                {
                    // Copy old values
                    int i = 0;
                    for (; i < startingOrder.Length; ++i)
                        newStartingOrder[i] = startingOrder[i];
                    // Set remaining values
                    for (; i < newStartingOrder.Length; ++i)
                        newStartingOrder[i] = i;
                }
                else
                {
                    // Pool available indices
                    List<int> unClaimed = new List<int>(count);
                    for (int i = 0; i < count; ++i)
                        unClaimed.Add(i);
                    // Store clashes
                    List<int> clashes = new List<int>(count);
                    // Iterate through and check/copy
                    for (int i = 0; i < count; ++i)
                    {
                        // Check if the old entry is valid
                        if (startingOrder[i] < count && unClaimed.Contains(startingOrder[i]))
                        {
                            // Apply and remove from unclaimed
                            newStartingOrder[i] = startingOrder[i];
                            unClaimed.Remove(startingOrder[i]);
                        }
                        else
                        {
                            // Store the clashing index
                            clashes.Add(i);
                        }
                    }
                    // Resolve the clashes using the unclaimed values
                    for (int i = 0; i < clashes.Count; ++i)
                    {
                        newStartingOrder[clashes[i]] = unClaimed[0];
                        unClaimed.RemoveAt(0);
                    }
                }

                startingOrder = newStartingOrder;
            }

            // Resize group count based on constant
            if (groupSizes.Length != FpsSwappableCategory.count)
            {
                int[] newGroupSizes = new int[FpsSwappableCategory.count];
                int i = 0;
                for (; i < newGroupSizes.Length && i < groupSizes.Length; ++i)
                    newGroupSizes[i] = groupSizes[i];
                for (; i < newGroupSizes.Length; ++i)
                    newGroupSizes[i] = 1;
                groupSizes = newGroupSizes;
            }
            // Clamp group sizes, as range does not work until edited in inspector
            for (int i = 0; i < groupSizes.Length; ++i)
                groupSizes[i] = Mathf.Clamp(groupSizes[i], 1, 10);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;
            
            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("inventoryType"), inventoryTypeOptions);
            switch (inventory)
            {
                case InventoryBehaviour.QuickSwitch:
                    {
                        NeoFpsEditorGUI.Header("Inventory Properties");
                        
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slotCount"));
                        NeoFpsEditorGUI.PrefabComponentField<FpsInventoryWieldable>(serializedObject.FindProperty("backupItem"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("duplicateBehaviour"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropVelocity"));

                        NeoFpsEditorGUI.Header("Starting Items");

                        if (m_StartingItemsStandardList == null)
                            m_StartingItemsStandardList = NeoFpsEditorGUI.GetPrefabComponentList<FpsInventoryItemBase>(serializedObject.FindProperty("startingItemsStandard"), (obj) => { return !(obj is FpsInventoryWieldableSwappable); });
                        m_StartingItemsStandardList.DoLayoutList();

                        NeoFpsEditorGUI.Header("Starting Slot Order");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingSlotChoice"));
                        if (startingSlotChoice == StartingSlot.CustomOrder)
                        {
                            var orderList = GetStartingOrderList(serializedObject);
                            orderList.DoLayoutList();
                        }
                    }
                    break;
                case InventoryBehaviour.Swappable:
                    {
                        NeoFpsEditorGUI.Header("Inventory Properties");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("swapAction"));
                        NeoFpsEditorGUI.PrefabComponentField<FpsInventoryWieldable>(serializedObject.FindProperty("backupItem"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropVelocity"));

                        var groupSizesProperty = serializedObject.FindProperty("groupSizes");
                        var slotCountProperty = serializedObject.FindProperty("swappableSlotCount");
                        int oldSlotCount = slotCountProperty.intValue;

                        EditorGUILayout.LabelField("Category sizes:");
                        if (groupSizesProperty.arraySize > 0)
                        {
                            ++EditorGUI.indentLevel;
                            for (int i = 0; i < groupSizesProperty.arraySize; ++i)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(FpsSwappableCategory.names[i]);
                                EditorGUILayout.PropertyField(groupSizesProperty.GetArrayElementAtIndex(i), new GUIContent());
                                EditorGUILayout.EndHorizontal();
                            }
                            --EditorGUI.indentLevel;

                            int newSlotCount = GetSlotCount(groupSizesProperty);
                            if (newSlotCount != oldSlotCount)
                                slotCountProperty.intValue = newSlotCount;

                            if (newSlotCount <= 0 || newSlotCount > 10)
                                EditorGUILayout.HelpBox("Total slot count must be greater than zero and less than or equal to 10", MessageType.Error);
                        }
                        else
                            EditorGUILayout.HelpBox("Number of categories is zero. Please check the FpsSwappableCategory generated constant.", MessageType.Error);

                        NeoFpsEditorGUI.Header("Starting Items");

                        if (m_StartingItemsSwappableList == null)
                            m_StartingItemsSwappableList = NeoFpsEditorGUI.GetPrefabComponentList<FpsInventoryItemBase>(serializedObject.FindProperty("startingItemsSwappable"), (obj) =>
                            {
                                if (obj is FpsInventoryWieldable)
                                    return (obj is FpsInventoryWieldableSwappable);
                                return true;
                            });
                        m_StartingItemsSwappableList.DoLayoutList();

                        NeoFpsEditorGUI.Header("Starting Slot Order");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingSlotChoice"));
                        if (startingSlotChoice == StartingSlot.CustomOrder)
                        {
                            var orderList = GetStartingOrderList(serializedObject);
                            orderList.DoLayoutList();
                        }
                    }
                    break;
                case InventoryBehaviour.Stacked:
                    {
                        NeoFpsEditorGUI.Header("Inventory Properties");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slotCount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStackSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("backupItem"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("duplicateBehaviour"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropVelocity"));

                        NeoFpsEditorGUI.Header("Starting Items");

                        if (m_StartingItemsStandardList == null)
                            m_StartingItemsStandardList = NeoFpsEditorGUI.GetPrefabComponentList<FpsInventoryItemBase>(serializedObject.FindProperty("startingItemsStandard"), (obj) => { return !(obj is FpsInventoryWieldableSwappable); });
                        m_StartingItemsStandardList.DoLayoutList();

                        NeoFpsEditorGUI.Header("Starting Slot Order");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingSlotChoice"));
                        if (startingSlotChoice == StartingSlot.CustomOrder)
                        {
                            var orderList = GetStartingOrderList(serializedObject);
                            orderList.DoLayoutList();
                        }
                    }
                    break;
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("inventoryType", inventoryType, inventoryTypeSummaries);
            switch (inventory)
            {
                case InventoryBehaviour.QuickSwitch:
                    {
                        WizardGUI.DoSummary("slotCount", slotCount);
                        WizardGUI.ObjectSummary("backupItem", backupItem);
                        WizardGUI.DoSummary("duplicateBehaviour", duplicateBehaviour.ToString());
                        WizardGUI.DoSummary("dropVelocity", dropVelocity);

                        GUILayout.Space(4);

                        WizardGUI.ObjectListSummary("startingItemsStandard", startingItemsStandard);

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("startingSlotChoice", startingSlotChoice.ToString());
                    }
                    break;
                case InventoryBehaviour.Swappable:
                    {
                        WizardGUI.DoSummary("swapAction", swapAction.ToString());
                        WizardGUI.DoSummary("groupSizes", groupSizes.ToString());
                        WizardGUI.ObjectSummary("backupItem", backupItem);
                        WizardGUI.DoSummary("dropVelocity", dropVelocity);
                        
                        GUILayout.Space(4);

                        WizardGUI.ObjectListSummary("startingItemsSwappable", startingItemsStandard);

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("startingSlotChoice", startingSlotChoice.ToString());
                    }
                    break;
                case InventoryBehaviour.Stacked:
                    {
                        WizardGUI.DoSummary("slotCount", slotCount);
                        WizardGUI.DoSummary("maxStackSize", maxStackSize);
                        WizardGUI.ObjectSummary("backupItem", backupItem);
                        WizardGUI.DoSummary("duplicateBehaviour", duplicateBehaviour.ToString());
                        WizardGUI.DoSummary("dropVelocity", dropVelocity);

                        GUILayout.Space(4);

                        WizardGUI.ObjectListSummary("startingItemsStandard", startingItemsStandard);

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("startingSlotChoice", startingSlotChoice.ToString());
                    }
                    break;
            }
        }


        void DrawStartingOrderListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Starting Order");
        }

        void DrawStartingOrderListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 4;

            if (inventoryType == 2)
            {
                int unsplit = m_StartingOrderList.serializedProperty.GetArrayElementAtIndex(index).intValue;
                int slot = (unsplit / maxStackSize);
                int stackPosition = unsplit - (slot * maxStackSize);
                EditorGUI.LabelField(rect, string.Format("Slot {0:D2} Item {1:D2}", slot + 1, stackPosition + 1));
            }
            else
            { 
                EditorGUI.LabelField(rect, "Slot " + (m_StartingOrderList.serializedProperty.GetArrayElementAtIndex(index).intValue + 1).ToString("D2"));
            }
        }

        int GetSlotCount(SerializedProperty groupSizesProperty)
        {
            int result = 0;
            for (int i = 0; i < groupSizesProperty.arraySize; ++i)
                result += groupSizesProperty.GetArrayElementAtIndex(i).intValue;
            return result;
        }
    }
}
