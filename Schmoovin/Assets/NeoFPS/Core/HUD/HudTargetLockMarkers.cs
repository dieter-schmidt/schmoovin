using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class HudTargetLockMarkers : WorldSpaceHudMarkerBase
    {
		[SerializeField, Tooltip("Where to get find the targeting system on the player character. If CharacterChildren is selected, then be careful not to use a targeting system on any weapons as these will also be found.")]
		private TargetLockSource m_Source = TargetLockSource.Weapon;

		public enum TargetLockSource
        {
			Weapon,
			Character,
			CharacterChildren
        }

		private Dictionary<int, IHudTargetLock> m_TargetMarkers = new Dictionary<int, IHudTargetLock>();
		private FpsInventoryBase m_Inventory = null;

		private ITargetLock m_TargetLock = null;
		protected ITargetLock targetLock
        {
			get { return m_TargetLock; }
			private set
            {
				if (m_TargetLock != value)
                {
					// Disconnect from old target lock
					if (m_TargetLock != null)
					{
						m_TargetLock.onTargetLock -= OnTargetLock;
                        m_TargetLock.onTargetLockBroken -= OnTargetLockBroken;
						m_TargetLock.onTargetLockStrengthChanged -= OnTargetLockStrengthChanged;

						// Clear existing target lock markers
						ClearMarkers();
					}

					// Set value
					m_TargetLock = value;

					// Connect to new target lock
					if (m_TargetLock != null)
					{
						m_TargetLock.onTargetLock += OnTargetLock;
                        m_TargetLock.onTargetLockBroken += OnTargetLockBroken;
						m_TargetLock.onTargetLockStrengthChanged += OnTargetLockStrengthChanged;
					}
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

			// Unsubscribe from old inventory
			if (m_Inventory != null)
			{
				m_Inventory.onSelectionChanged -= OnSelectionChanged;
				m_Inventory = null;
			}

			// Unsubscribe from old weapon
			targetLock = null;
		}

		public override void OnPlayerCharacterChanged(ICharacter character)
		{
			base.OnPlayerCharacterChanged(character);

			targetLock = null;

			switch (m_Source)
            {
				case TargetLockSource.Weapon:
					{
						if (m_Inventory != null)
							m_Inventory.onSelectionChanged -= OnSelectionChanged;

						if (character as Component != null)
							m_Inventory = character.inventory as FpsInventoryBase;
						else
							m_Inventory = null;

						if (m_Inventory != null)
						{
							m_Inventory.onSelectionChanged += OnSelectionChanged;
							OnSelectionChanged(m_Inventory.selected);
						}
					}
					break;
				case TargetLockSource.Character:
					{
						var characterComponent = character as Component;
						if (characterComponent != null)
							targetLock = characterComponent.GetComponent<ITargetLock>();
					}
					break;
				case TargetLockSource.CharacterChildren:
					{
						var characterComponent = character as Component;
						if (characterComponent != null)
							targetLock = characterComponent.GetComponentInChildren<ITargetLock>();
					}
					break;
			}
		}

		protected void OnSelectionChanged(IQuickSlotItem item)
		{
			if (item != null)
				targetLock = item.GetComponentInChildren<ITargetLock>();
			else
				targetLock = null;
		}

		void OnTargetLock (Collider target, bool partial)
		{
			var targetMarker = GetMarker() as IHudTargetLock;
			if (targetMarker != null)
			{
                if (partial)
                    targetMarker.SetPartialLockTarget(target, 0f);
                else
                    targetMarker.SetLockTarget(target);
                m_TargetMarkers.Add(target.GetInstanceID(), targetMarker);
			}
		}

        void OnTargetLockBroken(Collider target)
		{
			if (target != null)
			{
				int id = target.GetInstanceID();

				IHudTargetLock marker;
				if (m_TargetMarkers.TryGetValue(id, out marker))
				{
					ReleaseMarker(marker as WorldSpaceHudMarkerItemBase);
					m_TargetMarkers.Remove(id);
				}
			}
		}

		void OnTargetLockStrengthChanged(Collider target, float lockStrength)
		{
			IHudTargetLock marker;
			if (target != null && m_TargetMarkers.TryGetValue(target.GetInstanceID(), out marker))
			{
				marker.SetLockStrength(lockStrength);
			}
		}
	}
}