using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NeoFPS.Constants;

namespace NeoFPS.Samples
{
    public class LockpickPopupUI : MonoBehaviour, IPrefabPopup
    {
        [Header("Difficulty")]

        [SerializeField, Tooltip("The UI text element that will show the difficulty rating of the lock.")]
        private Text m_DifficultyText = null;
        [SerializeField, Tooltip("The pick difficulty prefix.")]
        private string m_DifficultyString = "Difficulty:";

        [Header("Pick Count")]

        [SerializeField, Tooltip("The parent object of the UI elements that show the pick count. If the lock does not use inventory picks, this object and its children will be hidden.")]
        private GameObject m_PickCountGroup = null;
        [SerializeField, Tooltip("The text readout for the pick count.")]
        private Text m_PickCountText = null;
        [SerializeField, Tooltip("The pick count string.")]
        private string m_PickString = "Lockpicks Remaining:";

        private IInventoryItem m_LockpickItem = null;

        public Selectable startingSelection
        {
            get { return null; }
        }

        public BaseMenu menu
        {
            get;
            private set;
        }

        public void OnShow(BaseMenu m)
        {
            menu = m;
        }

        public void Back()
        {
            //// Hide
            //menu.ShowPopup(null);
        }

        public void Initialise (LockpickPopup3D lockpickMechanism)
        {
            if (m_DifficultyText != null)
                m_DifficultyText.text = FormatDifficulty(lockpickMechanism.difficulty);
            SetLockpickItem(lockpickMechanism.pickItem);
        }

        protected virtual string FormatDifficulty(float difficulty)
        {
            return string.Format("{0} {1}/5", m_DifficultyString, Mathf.CeilToInt(difficulty * 5 - 0.001f));
        }

        void OnDisable()
        {
            SetLockpickItem(null);
        }

        private void SetLockpickItem(IInventoryItem item)
        {
            if (m_LockpickItem != null)
                m_LockpickItem.onQuantityChange -= OnLockpickQuantityChanged;

            m_LockpickItem = item;

            if (m_LockpickItem as Component != null)
            {
                m_LockpickItem.onQuantityChange += OnLockpickQuantityChanged;
                OnLockpickQuantityChanged();
                m_PickCountGroup.SetActive(true);
            }
            else
            {
                m_PickCountGroup.SetActive(false);
            }
        }

        private void OnLockpickQuantityChanged()
        {
            m_PickCountText.text = string.Format("{0} {1}", m_PickString, m_LockpickItem.quantity);
        }
    }
}