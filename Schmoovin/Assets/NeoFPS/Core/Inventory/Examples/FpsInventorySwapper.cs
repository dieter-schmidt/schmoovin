using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class FpsInventorySwapper : IQuickSlotItem
    {
        // Use list with max size
        private IQuickSlotItem[] m_Contents = null;
        private bool m_IsSelected = false;
        private int m_SelectedIndex = 0;
        private int m_ActiveSlots = 0;
        
        public IWieldable wieldable
        {
            get
            {
                if (m_Contents[m_SelectedIndex] != null)
                    return m_Contents[m_SelectedIndex].wieldable;
                else
                    return null;
            }
        }

        public IQuickSlotItem GetSlotItem(int subIndex)
        {
            return m_Contents [subIndex];
        }

        public void SetSlotItem(int subIndex, IQuickSlotItem item)
        {
            if (m_ActiveSlots == 0)
            {
                if (item == null)
                    return;
                else
                    ++m_ActiveSlots;
            }
            else
            {
                if (m_Contents[subIndex] == null && item != null)
                    ++m_ActiveSlots;
                if (m_Contents[subIndex] != null && item == null)
                    --m_ActiveSlots;
            }

            m_Contents [subIndex] = item;
        }
        
        public FpsInventorySwapper(IQuickSlots owner, int index, int max)
        {
            slots = owner;
            quickSlot = index;
            m_Contents = new IQuickSlotItem[max];
        }
        
        public IQuickSlots slots { get; private set; }
        public int quickSlot { get; private set; }

        public Sprite displayImage
        {
            get { return m_Contents[m_SelectedIndex].displayImage; }
        }
        
        public bool isSelected
        {
            get { return m_Contents[m_SelectedIndex].isSelected; }
        }

        public bool isSelectable
        {
            get { return m_Contents[m_SelectedIndex].isSelectable; }
        }

        public bool isDroppable
        {
            get { return m_Contents[m_SelectedIndex].isDroppable; }
        }
        
        public void OnSelect()
        {
            if (!m_IsSelected)
            {
                // Check for 0 entries
                m_IsSelected = true;
                m_Contents[m_SelectedIndex].OnSelect();
            }
            else
            {
                // Check for only 0/1 entries
                
                if (isSelected)
                    m_Contents[m_SelectedIndex].OnDeselect();
                
                // Get next item
                ++m_SelectedIndex;
                if (m_SelectedIndex >= m_Contents.Length)
                    m_SelectedIndex -= m_Contents.Length;
                
                // Select new item
                m_Contents[m_SelectedIndex].OnSelect();
            }
        }

        public void OnDeselectInstant()
        {
            m_IsSelected = false;

            // Deselect selected child
            m_Contents[m_SelectedIndex].OnDeselectInstant();
        }

        public Waitable OnDeselect()
        {
            m_IsSelected = false;
            
            // Deselect selected child
            return m_Contents[m_SelectedIndex].OnDeselect();
        }

        public bool DropItem(Vector3 position, Vector3 forward, Vector3 velocity)
        {
            return m_Contents[m_SelectedIndex].DropItem(position, forward, velocity);
        }

        public GameObject gameObject { get { return null; } }
        public Transform transform { get { return null; } }
        public T GetComponent<T>() { return default(T); }
        public T GetComponentInChildren<T>() { return default(T); }
        public T GetComponentInParent<T>() { return default(T); }
        public T[] GetComponents<T>() { return null; }
        public T[] GetComponentsInChildren<T>(bool includeInactive = false) { return null; }
        public T[] GetComponentsInParent<T>(bool includeInactive = false) { return null; }
        public Component GetComponent(Type t) { return null; }
        public Component GetComponentInChildren(Type t) { return null; }
        public Component GetComponentInParent(Type t) { return null; }
        public Component[] GetComponents(Type t) { return null; }
        public Component[] GetComponentsInChildren(Type t, bool includeInactive = false) { return null; }
        public Component[] GetComponentsInParent(Type t, bool includeInactive = false) { return null; }
    }
}