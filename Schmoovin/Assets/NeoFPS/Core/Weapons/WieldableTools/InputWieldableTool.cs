using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.WieldableTools;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputwieldabletool.html")]
	[RequireComponent(typeof(IWieldableTool))]
	public class InputWieldableTool : FpsInput
	{
		private IWieldableTool m_Consumable = null;
		private bool m_IsPlayer = false;
		private bool m_IsAlive = false;

		public override FpsInputContext inputContext
		{
			get { return FpsInputContext.Character; }
		}

		protected override void OnAwake()
		{
			m_Consumable = GetComponent<IWieldableTool>();
		}

		protected override void OnEnable()
		{
			// Attach event handlers
			var c = m_Consumable.wielder;
			if (c != null)
			{
				c.onControllerChanged += OnControllerChanged;
				c.onIsAliveChanged += OnIsAliveChanged;
				OnControllerChanged(c, c.controller);
				OnIsAliveChanged(c, c.isAlive);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			var c = m_Consumable.wielder;
			if (c != null)
			{
				c.onControllerChanged -= OnControllerChanged;
				c.onIsAliveChanged -= OnIsAliveChanged;
			}
		}

		void OnControllerChanged(ICharacter character, IController controller)
		{
			m_IsPlayer = (controller != null && controller.isPlayer);
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

		void OnIsAliveChanged(ICharacter character, bool alive)
		{
			m_IsAlive = alive;
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

		protected override void OnLoseFocus()
		{
			m_Consumable.PrimaryRelease();
		}

		protected override void UpdateInput()
		{
			// Primary
			if (GetButtonDown(FpsInputButton.PrimaryFire))
				m_Consumable.PrimaryPress();
			if (GetButtonUp(FpsInputButton.PrimaryFire))
				m_Consumable.PrimaryRelease();

			// Secondary
			if (GetButtonDown(FpsInputButton.SecondaryFire))
				m_Consumable.SecondaryPress();
			if (GetButtonUp(FpsInputButton.SecondaryFire))
				m_Consumable.SecondaryRelease();

			// Interrupt
			if (GetButtonDown(FpsInputButton.Reload))
				m_Consumable.Interrupt();

			// Flashlight
			if (GetButtonDown(FpsInputButton.Flashlight))
			{
				var flashlight = GetComponentInChildren<IWieldableFlashlight>(false);
				if (flashlight != null)
					flashlight.Toggle();
			}
		}
	}
}