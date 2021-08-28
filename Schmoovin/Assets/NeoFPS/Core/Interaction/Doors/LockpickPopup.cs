using NeoFPS.Constants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public abstract class LockpickPopup : MonoBehaviour
    {
        [SerializeField, Tooltip("")]
        private string m_LockpickID = "default";

        public static Dictionary<string, LockpickPopup> s_Instances = new Dictionary<string, LockpickPopup>();

        private ICharacter m_Character = null;
        private UnityAction m_OnCancel = null;
        private UnityAction m_OnUnlock = null;

        public float difficulty
        {
            get;
            private set;
        }

        public IInventoryItem pickItem
        {
            get;
            private set;
        }

        public static bool ShowLockpickPopup(string id, float difficulty, ICharacter character, UnityAction onUnlock, UnityAction onCancel)
        {
            LockpickPopup known;
            if (s_Instances.TryGetValue(id, out known) && known != null)
            {
                // Set callbacks
                known.m_OnUnlock = onUnlock;
                known.m_OnCancel = onCancel;

                // Track character (handle dying while lock-picking)
                known.m_Character = character;
                if (character != null)
                    character.onIsAliveChanged += known.OnIsAliveChanged;

                // Get the difficulty
                difficulty = Mathf.Clamp01(difficulty);
                known.difficulty = difficulty;

                // Get the lockpick item
                var inventory = character.GetComponent<IInventory>();
                if (inventory != null)
                    known.pickItem = inventory.GetItem(FpsInventoryKey.Lockpick);

                // Initialise
                known.Initialise(character);

                // Show the lockpick
                known.gameObject.SetActive(true);

                return true;
            }
            else
            {
                Debug.LogError("Lockpick not found with ID: " + id);
                return false;
            }
        }

        protected virtual void Awake()
        {
            // Store the lockpick for access later
            s_Instances.Add(m_LockpickID, this);
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            // Remove the lockpick from instances if known
            LockpickPopup known;
            if (s_Instances.TryGetValue(m_LockpickID, out known) && known == this)
                s_Instances.Remove(m_LockpickID);
        }

        void OnIsAliveChanged(ICharacter character, bool alive)
        {
            Cancel();
        }

        protected virtual void Unlock()
        {
            // Call event
            if (m_OnUnlock != null)
                m_OnUnlock();

            // Detach character is alive monitor
            if (m_Character != null)
            {
                m_Character.onIsAliveChanged -= OnIsAliveChanged;
                m_Character = null;
            }

            gameObject.SetActive(false);
        }

        public void Cancel()
        {
            // Call event
            if (m_OnCancel != null)
                m_OnCancel();

            // Detach character is alive monitor
            if (m_Character != null)
            {
                m_Character.onIsAliveChanged -= OnIsAliveChanged;
                m_Character = null;
            }

            gameObject.SetActive(false);
        }

        protected abstract void Initialise(ICharacter character);
    }
}