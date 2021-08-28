using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-sharedpoolammo.html")]
	public class SharedPoolAmmo : BaseAmmoBehaviour
    {
        [Header("Ammo Settings")]

        [SerializeField, ComponentOnObject(typeof(BaseAmmoEffect)), Tooltip("The effect of the ammo when it hits something.")]
		private BaseAmmoEffect m_Effect = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The ammo type to use.")]
        private SharedAmmoType m_AmmoType = null;

        private IInventory m_Inventory = null;
        private IInventoryItem m_FirearmItem = null;

        public override string printableName
		{
			get { return m_AmmoType.printableName; }
		}

		public override IAmmoEffect effect
		{
			get { return m_Effect; }
		}

		public override int maxAmmo
		{
			get { return m_AmmoType.maxQuantity; }
		}

		private IInventoryItem m_InventoryAmmo = null;
        public IInventoryItem inventoryAmmo
		{
			get { return m_InventoryAmmo; }
			private set
			{
				if (m_InventoryAmmo != null)
					m_InventoryAmmo.onQuantityChange -= OnQuantityChange;

				m_InventoryAmmo = value;

				if (m_InventoryAmmo != null)
					m_InventoryAmmo.onQuantityChange += OnQuantityChange;

				SendAmmoChangeEvent ();
			}
		}

		public override int currentAmmo
		{
			get
			{
				if (inventoryAmmo != null)
					return inventoryAmmo.quantity;
				else
					return 0;
			}
		}

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_Effect == null)
                m_Effect = GetComponent<BaseAmmoEffect>();
        }
#endif

        public override bool isModuleValid
        {
            get { return m_Effect != null && m_AmmoType != null; }
        }

        void Start()
        {
            if (m_AmmoType == null)
                Debug.LogError("No ammo type set for weapon", this);
            else
            {
                m_FirearmItem = (firearm as MonoBehaviour).GetComponentInParent<IInventoryItem>();
                m_FirearmItem.onAddToInventory += OnInventoryChanged;
                m_FirearmItem.onRemoveFromInventory += OnInventoryChanged;
                OnInventoryChanged();
            }
        }

		public override void DecrementAmmo (int amount)
		{
			if (inventoryAmmo != null)
				inventoryAmmo.quantity -= amount;
			SendAmmoChangeEvent ();
		}

		public override void IncrementAmmo (int amount)
		{
			if (inventoryAmmo != null)
				inventoryAmmo.quantity += amount;
			SendAmmoChangeEvent ();
		}

		void OnQuantityChange ()
		{
			SendAmmoChangeEvent ();
		}

        void OnInventoryChanged ()
        {
            if (m_Inventory != null)
            {
                m_Inventory.onItemAdded -= OnItemAddedToInventory;
                m_Inventory.onItemRemoved -= OnItemRemovedFromInventory;
            }

            m_Inventory = m_FirearmItem.inventory;

            if (m_Inventory == null)
                inventoryAmmo = null;
            else
            {
                m_FirearmItem.inventory.onItemAdded += OnItemAddedToInventory;
                m_FirearmItem.inventory.onItemRemoved += OnItemRemovedFromInventory;
                inventoryAmmo = m_FirearmItem.inventory.GetItem(m_AmmoType.itemIdentifier);
            }
        }

        void OnItemAddedToInventory (IInventoryItem item)
        {
            if (item.itemIdentifier == m_AmmoType.itemIdentifier)
                inventoryAmmo = item;
        }

        void OnItemRemovedFromInventory(IInventoryItem item)
        {
            if (item.itemIdentifier == m_AmmoType.itemIdentifier)
                inventoryAmmo = null;
        }
    }
}