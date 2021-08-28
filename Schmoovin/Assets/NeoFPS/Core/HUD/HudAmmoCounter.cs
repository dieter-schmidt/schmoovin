using UnityEngine;
using UnityEngine.UI;
using NeoFPS.ModularFirearms;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudammocounter.html")]
	public class HudAmmoCounter : PlayerCharacterHudBase
	{
		[Header ("Stacked Items")]

		[SerializeField, Tooltip("The group used for wieldable item stacks such as hand grenades.")]
		private GameObject m_StackedGroup = null;

		[SerializeField, Tooltip("The stack count text entry for displaying count and total.")]
		private Text m_StackCountText = null;

		[Header ("Firearms")]

		[SerializeField, Tooltip("The group used for firearms.")]
		private GameObject m_FirearmGroup = null;

		[SerializeField, Tooltip("The text entry for the current ammo in the firearm magazine.")]
		private Text m_MagazineText = null;

		[SerializeField, Tooltip("The text entry for the total ammo the character is carrying.")]
		private Text m_TotalText = null;

		[SerializeField, Tooltip("The text entry for displaying the ammo type.")]
		private Text m_AmmoTypeText = null;

		private int m_MagazineCount = -1;
		private int m_TotalCount = -1;
		private FpsInventoryBase m_InventoryBase = null;
		private IInventoryItem m_Wieldable = null;
        private IModularFirearm m_Firearm = null;
        private IReloader m_Reloader = null;
        private IAmmo m_Ammo = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old inventory
            if (m_InventoryBase != null)
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            // Unsubscribe from old weapon
            if (m_Firearm != null)
            {
                m_Firearm.onReloaderChange -= OnReloaderChange;
                m_Firearm.onAmmoChange -= OnAmmoChange;
            }
            else
            {
                if (m_Wieldable != null)
                    m_Wieldable.onQuantityChange -= OnWieldableQuantityChange;
            }

            // Unsubscribe from modules
            if (m_Reloader != null)
                m_Reloader.onCurrentMagazineChange -= OnCurrentMagazineChange;
            if (m_Ammo != null)
                m_Ammo.onCurrentAmmoChange -= OnCurrentAmmoChange;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
			if (m_InventoryBase != null)
				m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            if (character as Component != null)
                m_InventoryBase = character.inventory as FpsInventoryBase;
            else
                m_InventoryBase = null;

			if (m_InventoryBase == null)
			{
				m_FirearmGroup.SetActive (false);
				if (m_Firearm != null)
				{
					m_Firearm.onReloaderChange -= OnReloaderChange;
					m_Firearm.onAmmoChange -= OnAmmoChange;
					m_Firearm = null;
				}
			}
			else
			{
				m_InventoryBase.onSelectionChanged += OnSelectionChanged;
				OnSelectionChanged (m_InventoryBase.selected);
			}
		}

        protected void OnSelectionChanged (IQuickSlotItem item)
		{
			if (m_Firearm != null)
			{
				m_Firearm.onReloaderChange -= OnReloaderChange;
				m_Firearm.onAmmoChange -= OnAmmoChange;
			}
			else
			{
				if (m_Wieldable != null)
					m_Wieldable.onQuantityChange -= OnWieldableQuantityChange;
			}

			if (item == null)
			{
				m_FirearmGroup.SetActive (false);
				return;
			}
            
			m_Wieldable = item as IInventoryItem;
			if (m_Wieldable == null)
			{
				m_FirearmGroup.SetActive (false);
				return;
			}

			m_Firearm = m_Wieldable.GetComponentInChildren<IModularFirearm>();
			if (m_Firearm == null)
			{
				m_FirearmGroup.SetActive (false);

				if (m_Wieldable.maxQuantity > 1)
				{
					m_Wieldable.onQuantityChange += OnWieldableQuantityChange;
					OnWieldableQuantityChange ();

					m_StackedGroup.SetActive (true);
				}
				else
					m_StackedGroup.SetActive (false);
			}
			else
			{
				m_StackedGroup.SetActive (false);

				m_Firearm.onReloaderChange += OnReloaderChange;
				m_Firearm.onAmmoChange += OnAmmoChange;

				OnReloaderChange (m_Firearm, m_Firearm.reloader);
				OnAmmoChange (m_Firearm, m_Firearm.ammo);

				m_FirearmGroup.SetActive (true);
			}
		}

        protected void OnReloaderChange (IModularFirearm firearm, IReloader reloader)
		{
			if (m_Reloader != null)
				m_Reloader.onCurrentMagazineChange -= OnCurrentMagazineChange;

			if (reloader == null)
			{
				m_Reloader = null;
				m_MagazineText.text = "--";
			}
			else
			{
				m_Reloader = reloader;
				if (m_Reloader != null)
				{
					m_Reloader.onCurrentMagazineChange += OnCurrentMagazineChange;
					OnCurrentMagazineChange (firearm, m_Reloader.currentMagazine);
				}
			}
		}

        protected void OnCurrentMagazineChange (IModularFirearm firearm, int count)
		{
			if (m_MagazineCount != count)
			{
				m_MagazineText.text = count.ToString ();
				m_MagazineCount = count;
			}
		}

        protected void OnAmmoChange (IModularFirearm firearm, IAmmo ammo)
		{
			if (m_Ammo != null)
				m_Ammo.onCurrentAmmoChange -= OnCurrentAmmoChange;

			if (ammo == null)
			{
				m_TotalText.text = "---";
				m_AmmoTypeText.text = "---";
			}
			else
			{
				m_Ammo = ammo;
				if (m_Ammo != null)
				{
					m_Ammo.onCurrentAmmoChange += OnCurrentAmmoChange;
					OnCurrentAmmoChange (firearm, m_Ammo.currentAmmo);
					m_AmmoTypeText.text = m_Ammo.printableName;
				}
				else
					m_AmmoTypeText.text = "---";
			}
		}

        protected void OnCurrentAmmoChange (IModularFirearm firearm, int count)
		{
			if (m_TotalCount != count)
			{
				m_TotalText.text = count.ToString ();
				m_TotalCount = count;
			}
		}

        protected void OnWieldableQuantityChange ()
		{
			if (m_Wieldable != null)
				m_StackCountText.text = string.Format ("{0}/{1}", m_Wieldable.quantity, m_Wieldable.maxQuantity);
		}

		public void Enable ()
		{
			gameObject.SetActive (true);
		}
		public void Disable ()
		{
			gameObject.SetActive (false);
		}
	}
}