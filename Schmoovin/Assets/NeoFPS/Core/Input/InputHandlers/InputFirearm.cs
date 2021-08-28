using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.ModularFirearms;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputfirearm.html")]
	[RequireComponent (typeof (IModularFirearm))]
	public class InputFirearm : FpsInput
    {
		[SerializeField, Tooltip("The property key for the character motion graph (switch)")]
        private string m_AimingKey = "aiming";

		private IModularFirearm m_Firearm = null;
        private MonoBehaviour m_FirearmBehaviour = null;
        private bool m_IsPlayer = false;
		private bool m_IsAlive = false;
		private int m_AimingKeyHash = -1;
		private ICharacter m_Character = null;
        private SwitchParameter m_AimProperty = null;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }
		
		protected override void OnAwake()
		{
			m_Firearm = GetComponent<IModularFirearm>();
            m_FirearmBehaviour = m_Firearm as MonoBehaviour;
            m_AimingKeyHash = Animator.StringToHash(m_AimingKey);
		}

        protected override void OnEnable()
        {
			m_Character = m_Firearm.wielder;
			if (m_Character != null && m_Character.motionController != null)
			{
				MotionGraphContainer motionGraph = m_Character.motionController.motionGraph;
				m_AimProperty = motionGraph.GetSwitchProperty(m_AimingKeyHash);
				m_Character.onControllerChanged += OnControllerChanged;
				m_Character.onIsAliveChanged += OnIsAliveChanged;
				OnControllerChanged (m_Character, m_Character.controller);
				OnIsAliveChanged (m_Character, m_Character.isAlive);
			}
			else
			{
				m_IsPlayer = false;
				m_IsAlive = false;
				m_AimProperty = null;
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
            {
                PopContext();
				if (m_Firearm.trigger != null)
					m_Firearm.trigger.Release();
                if (m_AimProperty != null)
                    m_AimProperty.on = false;
            }
		}	

		protected override void OnDisable ()
		{
			base.OnDisable();

			if (m_Character != null)
			{
				m_Character.onControllerChanged -= OnControllerChanged;
				m_Character.onIsAliveChanged -= OnIsAliveChanged;
			}
			m_IsPlayer = false;
			m_IsAlive = false;
			m_AimProperty = null;

		}

        protected override void OnGainFocus()
        {
            base.OnGainFocus();
			FpsSettings.keyBindings.onRebind += OnRebindKeys;
		}

        protected override void OnLoseFocus()
        {
            m_Firearm.trigger.Release();
			FpsSettings.keyBindings.onRebind -= OnRebindKeys;
		}

		void OnRebindKeys(FpsInputButton button, bool primary, KeyCode to)
        {
			if (button == FpsInputButton.AimToggle)
				m_Firearm.aimToggleHold.on = false;
		}

        protected override void UpdateInput()
		{
			if (m_Firearm == null || !m_FirearmBehaviour.enabled)
				return;
			
            // Fire
            if (GetButtonDown(FpsInputButton.PrimaryFire))
            {
                if (m_Firearm.trigger.blocked && m_Firearm.reloader.interruptable)
                    m_Firearm.reloader.Interrupt();
                m_Firearm.trigger.Press();
            }
			if (GetButtonUp (FpsInputButton.PrimaryFire))
				m_Firearm.trigger.Release();
			if (GetButtonDown (FpsInputButton.SwitchWeaponModes))
				m_Firearm.SwitchMode();

            // Reload
            if (GetButtonDown(FpsInputButton.Reload))
            {
                if (m_Firearm.trigger.cancelOnReload)
                    m_Firearm.trigger.Cancel();
                else
                    m_Firearm.Reload();
            }

            // Aim
            m_Firearm.aimToggleHold.SetInput(
                GetButtonDown(FpsInputButton.AimToggle),
                GetButton(FpsInputButton.Aim)
                );
            if (m_AimProperty != null)
                m_AimProperty.on = m_Firearm.aimToggleHold.on;

			// Flashlight
			if (GetButtonDown(FpsInputButton.Flashlight))
            {
				var flashlight = GetComponentInChildren<IWieldableFlashlight>(false);
				if (flashlight != null)
					flashlight.Toggle();
            }

			// Optics
			if (GetButtonDown(FpsInputButton.OpticsLightPlus))
			{
				var optics = GetComponentInChildren<IOpticsBrightnessControl>(false);
				if (optics != null)
					optics.IncrementBrightness();
			}
            if (GetButtonDown(FpsInputButton.OpticsLightMinus))
            {
                var optics = GetComponentInChildren<IOpticsBrightnessControl>(false);
                if (optics != null)
                    optics.DecrementBrightness();
            }
        }
	}
}