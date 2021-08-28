using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.ModularFirearms;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputabilityfirearm.html")]
	[RequireComponent (typeof (IModularFirearm))]
	public class InputAbilityFirearm : FpsInput
    {
		private IModularFirearm m_Firearm = null;
        private MonoBehaviour m_FirearmBehaviour = null;
        private bool m_IsPlayer = false;
		private bool m_IsAlive = false;
		private ICharacter m_Character = null;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }
		
		protected override void OnAwake()
		{
			m_Firearm = GetComponent<IModularFirearm>();
            m_FirearmBehaviour = m_Firearm as MonoBehaviour;
		}

        protected override void OnEnable()
        {
			m_Character = m_Firearm.wielder;
			if (m_Character != null && m_Character.motionController != null)
			{
				MotionGraphContainer motionGraph = m_Character.motionController.motionGraph;
				m_Character.onControllerChanged += OnControllerChanged;
				m_Character.onIsAliveChanged += OnIsAliveChanged;
				OnControllerChanged (m_Character, m_Character.controller);
				OnIsAliveChanged (m_Character, m_Character.isAlive);
			}
			else
			{
				m_IsPlayer = false;
				m_IsAlive = false;
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
		}

        protected override void OnLoseFocus()
        {
            m_Firearm.trigger.Release();
        }

        protected override void UpdateInput()
		{
			if (m_Firearm == null || !m_FirearmBehaviour.enabled)
				return;
			
            // Fire
            if (GetButtonDown(FpsInputButton.Ability))
            {
                if (m_Firearm.trigger.blocked && m_Firearm.reloader.interruptable)
                    m_Firearm.reloader.Interrupt();
                m_Firearm.trigger.Press();
            }
			if (GetButtonUp (FpsInputButton.Ability))
				m_Firearm.trigger.Release();
        }
	}
}