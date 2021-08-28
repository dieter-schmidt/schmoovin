using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NeoFPS
{
	public abstract class HudInventory : PlayerCharacterHudBase
	{
		[SerializeField, Tooltip("Does the HUD inventory stay visible at all times, or fade out?")]
		private bool m_Persistent = false;
		
		[SerializeField, Tooltip("How long does the HUD inventory stay fully visible before fading out?")]
		private float m_Timeout = 5f;
		
		[SerializeField, Range(0f, 2f), Tooltip("How long does the fade out last?")]
		private float m_TransitionDuration = 1f;

        private CanvasGroup m_CanvasGroup = null;
		private float m_InverseTransitionDuration = 0f;
		private WaitForSeconds m_TimeoutYield = null;
        private Coroutine m_TimeoutCoroutine = null;
        private bool m_TimeoutPending = false;
        
		protected FpsInventoryBase inventory
        {
            get;
            private set;
        }

        protected bool persistent
        {
            get { return m_Persistent; }
        }

        public float visibility
		{
			get { return m_CanvasGroup.alpha; }
			set
			{
                value = Mathf.Clamp01 (value);
                if (isActiveAndEnabled && value == 0f)
                    gameObject.SetActive(false);
                if (!isActiveAndEnabled && value != 0f)
                    gameObject.SetActive(true);
                m_CanvasGroup.alpha = value;
			}
		}
        
        protected virtual void OnValidate ()
        {
            m_Timeout = Mathf.Clamp(m_Timeout, 0.1f, 60f);
        }

        protected override void Awake()
        {
            if (m_TransitionDuration == 0f)
                m_InverseTransitionDuration = 10000f;
            else
                m_InverseTransitionDuration = 1f / m_TransitionDuration;
			m_TimeoutYield = new WaitForSeconds (m_Timeout);
            m_CanvasGroup = GetComponent<CanvasGroup>();

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Detach from old inventory
            if (inventory != null)
            {
                inventory.onSlotItemChanged -= OnSlotItemChanged;
                inventory.onSelectionChanged -= OnSelectionChanged;
            }
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
			// Detach from old inventory
			if (inventory != null)
			{
				inventory.onSlotItemChanged -= OnSlotItemChanged;
				inventory.onSelectionChanged -= OnSelectionChanged;
                ClearContents();
			}

            // Set new inventory
            if (character as Component != null)
                inventory = character.inventory as FpsInventoryBase;
            else
                inventory = null;

            // Attach to new inventory
            if (inventory != null && InitialiseSlots())
            {
                // Attach event handlers
                inventory.onSlotItemChanged += OnSlotItemChanged;
                inventory.onSelectionChanged += OnSelectionChanged;

                // Add items
                for (int i = 0; i < inventory.numSlots; ++i)
                    SetSlotItem(i, inventory.GetSlotItem(i));

                // Select starting item
                if (inventory.selected == null)
                    OnSelectSlot(-1);
                else
                    OnSelectSlot(inventory.selected.quickSlot);

                TriggerTimeout();
		    }
			else
            {
                inventory = null;
                visibility = 0f;	
			}
        }
        
        protected abstract bool InitialiseSlots();
        
        protected abstract void ClearContents();
        
        protected abstract void SetSlotItem(int slot, IQuickSlotItem item);

		private void OnSlotItemChanged (int slot, IQuickSlotItem item)
		{
            SetSlotItem (slot, item);
            TriggerTimeout (); // Don't time out if null?
		}

		private void OnSelectionChanged (IQuickSlotItem selection)
		{
			if (selection != null)
				OnSelectSlot (selection.quickSlot);
			else
				OnSelectSlot (-1);
		}
        
        protected virtual void OnSelectSlot (int index)
		{
            TriggerTimeout();
		}
        
		protected virtual void TriggerTimeout ()
        {
            if (m_TimeoutPending)
                return;

            // Stop old timeout
            if (m_TimeoutCoroutine != null)
                StopCoroutine(m_TimeoutCoroutine);

            // Set to visible
            visibility = 1f;

            // Start new timeout
            if (!persistent)
            {
                m_TimeoutPending = true;
                if (isActiveAndEnabled)
                    m_TimeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            }
		}

		private IEnumerator TimeoutCoroutine ()
		{
			// Prevent multiple stop/start on one frame
			yield return null;
            m_TimeoutPending = false;

			// Wait for timeout
            yield return m_TimeoutYield;

			// Fade out
			while (visibility > 0f)
			{
				visibility -= m_InverseTransitionDuration * Time.unscaledDeltaTime;
				yield return null;
			}

			// Completed
			m_TimeoutCoroutine = null;
		}
	}
}