using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
	public abstract class FpsInventoryItemBase : MonoBehaviour, IInventoryItem, INeoSerializableComponent
	{
		[SerializeField, Tooltip("The quantity of items in the stack.")]
		private int m_Quantity = 1;

        [SerializeField, Tooltip("An event that is invoked when the object is first added to the character inventory.")]
        private UnityEvent m_OnAddToInventory = new UnityEvent();

        [SerializeField, Tooltip("An event that is invoked when the object is completely removed from the character inventory.")]
        private UnityEvent m_OnRemoveFromInventory = new UnityEvent();

        [SerializeField, Tooltip("An event that is invoked when the quantity of objects in the stack changes.")]
        private UnityEvent m_OnQuantityChange = new UnityEvent();

        private static readonly NeoSerializationKey k_QuantityKey = new NeoSerializationKey("quantity");

        public event UnityAction<ICharacter, ICharacter> onOwnerChange;

    #if UNITY_EDITOR
        protected virtual void OnValidate ()
		{
			m_Quantity = Mathf.Clamp(m_Quantity, 1, maxQuantity);
		}
	#endif
		
		public event UnityAction onAddToInventory
		{
			add { m_OnAddToInventory.AddListener (value); }
			remove { m_OnAddToInventory.RemoveListener (value); }
		}

		public event UnityAction onRemoveFromInventory
		{
			add { m_OnRemoveFromInventory.AddListener (value); }
			remove { m_OnRemoveFromInventory.RemoveListener (value); }
		}

		public event UnityAction onQuantityChange
		{
			add { m_OnQuantityChange.AddListener (value); }
			remove { m_OnQuantityChange.RemoveListener (value); }
		}

		protected FpsInventoryBase fpsInventory
		{
            get;
            private set;
		}

        protected NeoSerializedGameObject neoSerializedGameObject
        {
            get;
            private set;
        }
        
        protected virtual void Awake()
        {
            neoSerializedGameObject = GetComponent<NeoSerializedGameObject>();
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_QuantityKey, quantity);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            int result = 0;
            if (reader.TryReadValue(k_QuantityKey, out result, 1))
                quantity = result;
        }

        #region IInventoryItem implementation

        private ICharacter m_Owner = null;
        public ICharacter owner
		{
			get { return m_Owner; }
			private set
			{
				ICharacter old = m_Owner;
				m_Owner = value;
				if (onOwnerChange != null)
					onOwnerChange.Invoke(old, m_Owner);
			}
		}

		public virtual int itemIdentifier
		{
			// Virtual to make it easy for you to replace with something different.
			get { return 0; }
		}

		public virtual void OnAddToInventory (IInventory i, InventoryAddResult addResult)
		{
			fpsInventory = i as FpsInventoryBase;
			if (fpsInventory != null)
			{
				owner = fpsInventory.GetComponentInParent<ICharacter>();
			}
			else
			{
				Debug.LogError ("FpsInventoryItems should be used with FpsInventory or a component that inherits from it");
				return;
			}

		}

		public virtual void OnRemoveFromInventory ()
		{
			owner = null;
		}
        
        public IInventory inventory
		{
			get { return fpsInventory; }
		}

		public int quantity
		{
			get { return m_Quantity; }
			set
			{
				m_Quantity = Mathf.Clamp (value, 0, maxQuantity + 1);
				if (m_Quantity == 0)
				{
					// Remove from inventory if it's inside one
                    if (fpsInventory != null)
                    {
                        if ((FpsInventoryItemBase)fpsInventory.selected == this)
                            fpsInventory.SwitchSelection();
                        fpsInventory.RemoveItem(this);
						Destroy(gameObject); // Is this the best way?
					}
				}
				else
					m_OnQuantityChange.Invoke ();
			}
		}

		public virtual int maxQuantity
		{
			get { return 1; }
		}

		#endregion
	}
}