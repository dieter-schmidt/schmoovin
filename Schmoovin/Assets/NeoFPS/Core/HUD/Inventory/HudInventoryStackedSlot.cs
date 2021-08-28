using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudinventorystackedslot.html")]
    public class HudInventoryStackedSlot : MonoBehaviour
    {
		[SerializeField, Tooltip("The item slot prototype to duplicate for the stack.")]
        private HudInventoryItemStacked m_ItemPrototype = null;

		[SerializeField, Tooltip("The padding transform to pad the layout group and push the item slots together.")]
        private Transform m_PaddingTransform = null;

        private FpsInventoryStacked m_Inventory = null;
        private int m_StackIndex = 0;
        private bool m_PersistentHUD = false;

        private bool m_Selected = false;
        public bool selected
        {
            get { return m_Selected; }
            set
            {
                m_Selected = value;
                if (items != null)
                {
                    for (int i = 0; i < items.Length; ++i)
                        items[i].stackSelected = m_Selected;
                }

                if (m_PersistentHUD)
                {
                    if (m_Selected)
                        TriggerTimeout();
                    else
                    {
                        // Stop old timeout
                        if (m_TimeoutCoroutine != null)
                            StopCoroutine(m_TimeoutCoroutine);
                    }
                }
            }
        }
        
        public HudInventoryItemStacked[] items
        {
            get;
            private set;
        }

        public void ClearStack()
        {
            for (int i = 0; i < items.Length; ++i)
            {
                items[i].SetItem(null);
                items[i].selected = false;
            }
        }

        public void Initialise(FpsInventoryStacked inventory, int stackIndex, bool persistent)
        {
            m_Inventory = inventory;
            m_StackIndex = stackIndex;
            m_PersistentHUD = persistent;

            int stackSize = inventory.maxStackSize;

            selected = false;

            // Grab child items
            items = GetComponentsInChildren<HudInventoryItemStacked>(true);

            // First setup
            if (items == null)
            {
                Debug.Log("Setting size: " + stackSize);

                items = new HudInventoryItemStacked[stackSize];

                items[0] = m_ItemPrototype;
                items[0].Initialise (this, true, stackIndex);
                for (int i = 1; i < stackSize; ++i)
                {
                    items[i] = Instantiate(m_ItemPrototype, m_ItemPrototype.transform.parent);
                    items[i].Initialise (this, false, stackIndex);
                }
                
                // Select first group item
                RefreshItemOrder();

                return;
            }
            else
            {
                for (int i = 0; i < items.Length; ++i)
                    items[i].Initialise(this, false, stackIndex);
            }

            // Different size - reuse old entries
            if (items.Length != stackSize)
            {
                var oldSlots = items;
                items = new HudInventoryItemStacked[stackSize];

                // Swap old to new 
                int swapped = 0;
                for (; swapped < items.Length && swapped < oldSlots.Length; ++swapped)
                    items[swapped] = oldSlots[swapped];
                
                // Fill remaining gaps if not enough
                for (int i = swapped; i < items.Length; ++i)
                {
                    items[i] = Instantiate (m_ItemPrototype, m_ItemPrototype.transform.parent);
                    items[i].Initialise (this, false, stackIndex);
                }
                
                // Destroy old if too many
                for (int i = swapped; i < oldSlots.Length; ++i)
                    Destroy(oldSlots[i].gameObject);
                
                // Select first group item
                RefreshItemOrder();

                return;
            }

            // Select first group item
            RefreshItemOrder();
        }

        public void RefreshItemOrder()
        {
            if (m_Inventory == null || m_Inventory.stacks == null)
                return;

            var stack = m_Inventory.stacks[m_StackIndex];
            int maxStackSize = m_Inventory.maxStackSize;

            int front = Mathf.Clamp (stack.currentItemLocal, 0, maxStackSize - 1);
            for (int i = 0; i < maxStackSize; ++i)
            {
                int index = front + i;
                if (index >= maxStackSize)
                    index -= maxStackSize;
                items[index].SetAsLastSibling ();
                items[index].primary = (i == 0);
            }
            
            // Ensure padding stays at the bottom
            if (m_PaddingTransform != null)
                m_PaddingTransform.SetAsLastSibling ();
        }


        const float k_InverseFade = 2f;
        const float k_FadeDelay = 1f;

        private WaitForSeconds m_TimeoutYield = new WaitForSeconds(k_FadeDelay);
        private Coroutine m_TimeoutCoroutine = null;
        private bool m_TimeoutPending = false;

        private float m_Visibility = 0f;
        public float visibility
        {
            get { return m_Visibility; }
            set
            {
                m_Visibility = Mathf.Clamp01(value);
                for (int i = 0; i < items.Length; ++i)
                {
                    if (!items[i].primary)
                        items[i].SetVisibility(m_Visibility);
                    else
                        items[i].SetVisibility(1f);
                }
            }
        }

        protected virtual void TriggerTimeout()
        {
            if (m_TimeoutPending)
                return;

            // Stop old timeout
            if (m_TimeoutCoroutine != null)
                StopCoroutine(m_TimeoutCoroutine);

            // Set to visible
            visibility = 1f;

            // Start new timeout
            m_TimeoutPending = true;
            if (isActiveAndEnabled)
                m_TimeoutCoroutine = StartCoroutine(TimeoutCoroutine());
        }

        private IEnumerator TimeoutCoroutine()
        {
            // Prevent multiple stop/start on one frame
            yield return null;
            m_TimeoutPending = false;

            // Wait for timeout
            yield return m_TimeoutYield;

            // Fade out
            while (visibility > 0f)
            {
                visibility -= k_InverseFade * Time.unscaledDeltaTime;
                yield return null;
            }

            // Completed
            m_TimeoutCoroutine = null;
        }
    }
}