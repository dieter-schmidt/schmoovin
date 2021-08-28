using UnityEngine;
using UnityEngine.UI;
using NeoFPS.ModularFirearms;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudfirearmmode.html")]
	public class HudFirearmMode : PlayerCharacterHudBase
	{
		[SerializeField, Tooltip("The text entry for displaying the weapon fire mode.")]
		private Text m_FireModeText = null;

		private FpsInventoryBase m_InventoryBase = null;
        private IModularFirearm m_Firearm = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old inventory
            if (m_InventoryBase != null)
                m_InventoryBase.onSelectionChanged -= OnSelectionChanged;

            // Unsubscribe from old weapon
            if (m_Firearm != null)
                m_Firearm.onModeChange -= OnFirearmModeSwitch;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old inventory
            if (m_InventoryBase != null)
				m_InventoryBase.onSelectionChanged -= OnSelectionChanged;
            
            // Get new inventory
            if (character as Component != null)
                m_InventoryBase = character.inventory as FpsInventoryBase;
            else
                m_InventoryBase = null;

            // Update firearm
			if (m_InventoryBase == null)
			{
				if (m_Firearm != null)
				{
					m_Firearm.onModeChange -= OnFirearmModeSwitch;
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
				m_Firearm.onModeChange -= OnFirearmModeSwitch;

			if (item == null)
			{
				gameObject.SetActive (false);
				return;
			}
            
			m_Firearm = item.GetComponentInChildren<IModularFirearm>();
			if (m_Firearm == null)
			{
                gameObject.SetActive (false);
			}
			else
			{
				m_Firearm.onModeChange += OnFirearmModeSwitch;
				OnFirearmModeSwitch (m_Firearm, m_Firearm.mode);
			}
		}

        protected void OnFirearmModeSwitch (IModularFirearm firearm, string mode)
		{
			if (mode == string.Empty)
				gameObject.SetActive (false);
			else
			{
				m_FireModeText.text = mode;
                gameObject.SetActive (true);
			}
		}
	}
}