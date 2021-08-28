using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using UnityEngine.Events;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-lockedinteractiveobject.html")]
    public class LockedInteractiveObject : MonoBehaviour, IInteractiveObject
    {
        [SerializeField, Tooltip("A unique ID for this lock. The player must have an equivalent key in their inventory to unlock.")]
        private string m_LockID = "demo_lock";

        [SerializeField, Tooltip("The name of the item in the HUD tooltip.")]
        private string m_TooltipName = string.Empty;

        [SerializeField, Tooltip("A description of the action for use in the HUD tooltip, eg pick up.")]
        private string m_TooltipAction = string.Empty;

        [SerializeField, Tooltip("Can the object be interacted with immediately.")]
        private bool m_InteractableOnStart = true;

        [SerializeField, Tooltip("An event that is triggered when the object is used.")]
        private UnityEvent m_OnUsed = new UnityEvent();

        [SerializeField, Tooltip("An event that is triggered when the player looks directly at the object.")]
        private UnityEvent m_OnCursorEnter = new UnityEvent();

        [SerializeField, Tooltip("An event that is triggered when the player looks away from the object.")]
        private UnityEvent m_OnCursorExit = new UnityEvent();

        private static readonly NeoSerializationKey k_InteractableKey = new NeoSerializationKey("interactable");

        private Collider m_Collider = null;

        public event UnityAction onTooltipChanged;

        public string tooltipName
        {
            get { return m_TooltipName; }
            protected set
            {
                m_TooltipName = value;
                if (onTooltipChanged != null)
                    onTooltipChanged();
            }
        }

        public string tooltipAction
        {
            get { return m_TooltipAction; }
            protected set
            {
                m_TooltipAction = value;
                if (onTooltipChanged != null)
                    onTooltipChanged();
            }
        }

        public event UnityAction onUsed
        {
            add { m_OnUsed.AddListener(value); }
            remove { m_OnUsed.RemoveListener(value); }
        }
        public event UnityAction onCursorEnter
        {
            add { m_OnCursorEnter.AddListener(value); }
            remove { m_OnCursorEnter.RemoveListener(value); }
        }
        public event UnityAction onCursorExit
        {
            add { m_OnCursorExit.AddListener(value); }
            remove { m_OnCursorExit.RemoveListener(value); }
        }

        public UnityEvent onUsedUnityEvent
        {
            get { return m_OnUsed; }
        }
        public UnityEvent onCursorEnterUnityEvent
        {
            get { return m_OnCursorEnter; }
        }
        public UnityEvent onCursorExitUnityEvent
        {
            get { return m_OnCursorExit; }
        }

        private bool m_Highlighted = false;
        public bool highlighted
        {
            get { return m_Highlighted; }
            set
            {
                if (m_Highlighted != value)
                {
                    m_Highlighted = value;
                    OnHighlightedChanged(value);
                }
            }
        }

        private bool m_Interactable = false;
        public bool interactable
        {
            get { return m_Interactable; }
            set
            {
                m_Interactable = value;
                if (m_Collider != null)
                    m_Collider.enabled = value;
            }
        }

        public float holdDuration
        {
            get { return 0f; }
        }

        protected virtual void Awake()
        {
            m_Collider = GetComponent<Collider>();
            if (m_Collider != null)
                m_Collider.enabled = m_Interactable;
        }

        protected virtual void Start()
        {
            if (string.IsNullOrWhiteSpace(m_LockID))
            {
                Debug.LogWarning("LockedInteractiveObject has invalid lock ID. Disabling interaction." + gameObject);
                interactable = false;
            }
            else
                interactable = m_InteractableOnStart;
        }

        protected virtual void OnHighlightedChanged(bool h)
        {
            if (h)
                m_OnCursorEnter.Invoke();
            else
                m_OnCursorExit.Invoke();
        }

        public virtual void Interact(ICharacter character)
        {
            var inventory = character.GetComponent<IInventory>();
            if (inventory != null)
            {
                var keyRing = inventory.GetItem(FpsInventoryKey.KeyRing) as IKeyRing;
                if (keyRing != null && keyRing.ContainsKey(m_LockID))
                    m_OnUsed.Invoke();
            }
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_InteractableKey, interactable);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            bool result = true;
            if (reader.TryReadValue(k_InteractableKey, out result, true))
                interactable = result;
        }
    }
}