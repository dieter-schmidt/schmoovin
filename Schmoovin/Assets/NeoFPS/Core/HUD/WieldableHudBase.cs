using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    public abstract class WieldableHudBase : MonoBehaviour, IPlayerCharacterSubscriber
    {
        IPlayerCharacterWatcher m_Watcher = null;

        protected virtual bool isValid
        {
            get { return true; }
        }

        protected FpsInventoryWieldable currentWieldable
        {
            get;
            private set;
        }

        protected virtual FpsInventoryBase inventory
        {
            get;
            private set;
        }

        protected virtual void Awake()
        {
            if (isValid)
            {
                m_Watcher = GetComponentInParent<IPlayerCharacterWatcher>();
                if (m_Watcher == null)
                    Debug.LogError("Player inventory HUD items require a component that implements IPlayerCharacterWatcher in the parent heirarchy", gameObject);
            }
            else
                gameObject.SetActive(false);
        }

        protected virtual void Start()
        {
            if (m_Watcher != null)
                m_Watcher.AttachSubscriber(this);
        }

        protected virtual void OnDestroy()
        {
            if (m_Watcher != null)
                m_Watcher.ReleaseSubscriber(this);
            OnPlayerCharacterChanged(null);
        }

        public void OnPlayerCharacterChanged(ICharacter c)
        {
            if (inventory != null)
            {
                inventory.onSelectionChanged -= OnWieldableSelectionChanged;
                OnWieldableSelectionChanged(null);
            }

            if (c as Component != null)
                inventory = c.GetComponent<FpsInventoryBase>();
            else
                inventory = null;

            if (inventory != null)
            {
                inventory.onSelectionChanged += OnWieldableSelectionChanged;
                OnWieldableSelectionChanged(inventory.selected);
            }
        }
                                
        void OnWieldableSelectionChanged(IQuickSlotItem item)
        {
            // Unsubscribe from old wieldable
            if (currentWieldable != null)
                DetachFromSelection(currentWieldable);

            // Get new wieldable
            currentWieldable = item as FpsInventoryWieldable;

            // Subscribe to new wieldable
            if (currentWieldable != null)
                AttachToSelection(currentWieldable);

            ResetUI();
        }

        protected abstract void AttachToSelection(FpsInventoryWieldable wieldable);
        protected abstract void DetachFromSelection(FpsInventoryWieldable wieldable);
        protected abstract void ResetUI();
    }
}
