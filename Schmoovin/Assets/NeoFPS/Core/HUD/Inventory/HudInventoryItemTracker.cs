using NeoFPS.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public abstract class HudInventoryItemTracker : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The item to track")]
        private FpsInventoryKey m_ItemKey = FpsInventoryKey.Undefined;

        private FpsInventoryBase m_Inventory = null;

        protected IInventoryItem item
        {
            get;
            private set;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Detach from old inventory
            if (m_Inventory != null)
            {
                m_Inventory.onItemAdded -= OnItemAdded;
                m_Inventory.onItemRemoved -= OnItemRemoved;
            }

            // Detach from old item
            if (item != null)
            {
                item.onQuantityChange -= OnQuantityChanged;
                item = null;
            }

            // Set new inventory
            if (character as Component != null)
                m_Inventory = character.inventory as FpsInventoryBase;
            else
                m_Inventory = null;

            // Attach to new inventory
            if (m_Inventory != null)
            {
                m_Inventory.onItemAdded += OnItemAdded;
                m_Inventory.onItemRemoved += OnItemRemoved;

                item = m_Inventory.GetItem(m_ItemKey);
                if (item != null)
                    item.onQuantityChange += OnQuantityChanged;

                OnQuantityChanged();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected abstract void OnQuantityChanged();

        private void OnItemAdded(IInventoryItem added)
        {
            if (added.itemIdentifier == m_ItemKey)
            {
                if (item != null)
                    item.onQuantityChange -= OnQuantityChanged;
                item = added;
                if (item != null)
                    item.onQuantityChange += OnQuantityChanged;
            }

            OnQuantityChanged();
        }

        private void OnItemRemoved(IInventoryItem removed)
        {
            if (removed.itemIdentifier == m_ItemKey)
            {
                if (item != null)
                    item.onQuantityChange -= OnQuantityChanged;
                item = null;
            }

            OnQuantityChanged();
        }
    }
}
