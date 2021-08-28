using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputthrownweapon.html")]
	[RequireComponent (typeof (IThrownWeapon))]
	public class InputThrownWeapon : FpsInput
	{
		private IThrownWeapon m_ThrownWeapon = null;
        private ICharacter m_Character = null;
        private bool m_IsPlayer = false;
		private bool m_IsAlive = false;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }

        protected override void OnAwake()
        {
            m_ThrownWeapon = GetComponent<IThrownWeapon>();
		}

        protected override void OnEnable ()
		{
			// Get the wielding character
			IInventoryItem invItem = GetComponent<IInventoryItem>();
			if (invItem != null)
				m_Character = invItem.owner;
			else
				m_Character = null;

			// Check character found
			if (m_Character == null)
				return;

			// Attach event handlers
			m_Character.onControllerChanged += OnControllerChanged;
			m_Character.onIsAliveChanged += OnIsAliveChanged;
			OnControllerChanged (m_Character, m_Character.controller);
			OnIsAliveChanged (m_Character, m_Character.isAlive);
		}

		protected override void OnDisable ()
		{
			base.OnDisable();

			if (m_Character != null)
			{
				m_Character.onControllerChanged -= OnControllerChanged;
				m_Character.onIsAliveChanged -= OnIsAliveChanged;
			}
		}

		void OnControllerChanged (ICharacter character, IController controller)
		{
			m_IsPlayer = (controller != null && controller.isPlayer);
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}	

		void OnIsAliveChanged (ICharacter character, bool alive)
		{
			m_IsAlive = alive;
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

        protected override void UpdateInput()
        {
			if (GetButtonDown (FpsInputButton.PrimaryFire))
				m_ThrownWeapon.ThrowHeavy ();

			if (GetButtonDown (FpsInputButton.SecondaryFire))
				m_ThrownWeapon.ThrowLight ();
		}
	}
}