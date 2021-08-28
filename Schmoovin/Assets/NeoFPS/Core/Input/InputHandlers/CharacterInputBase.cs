using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(ICharacter))]
    public abstract class CharacterInputBase : FpsInput
    {
        protected ICharacter m_Character = null;

        private bool m_IsPlayer = false;
        private bool m_IsAlive = false;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }

        protected override void OnAwake()
        {
            m_Character = GetComponent<ICharacter>();
            if (m_Character == null)
            {
                Debug.LogError("Input  requires a character component.", gameObject);
                return;
            }

            m_Character.onControllerChanged += OnControllerChanged;
            m_Character.onIsAliveChanged += OnIsAliveChanged;
        }

        protected override void OnEnable()
        {
            m_IsPlayer = m_Character.controller != null && m_Character.controller.isPlayer;
            m_IsAlive = m_Character.isAlive;
            if (m_IsPlayer && m_IsAlive)
                PushContext();
            else
                PopContext();
        }

        protected virtual void OnDestroy()
        {
            m_Character.onControllerChanged -= OnControllerChanged;
            m_Character.onIsAliveChanged -= OnIsAliveChanged;
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
    }
}
