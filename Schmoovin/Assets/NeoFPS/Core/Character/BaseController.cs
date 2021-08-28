using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoCC;

namespace NeoFPS
{
    public abstract class BaseController : MonoBehaviour, IController, INeoSerializableComponent
    {
        private static readonly NeoSerializationKey k_CharacterKey = new NeoSerializationKey("character");

        #region IController implementation

        public event UnityAction<ICharacter> onCharacterChanged;

        public virtual bool isPlayer
        {
            get { return false; }
        }

        public virtual bool isLocalPlayer
        {
            get { return isPlayer; } // Since solo play only
        }

        private BaseCharacter m_CurrentCharacter = null;
        public ICharacter currentCharacter
        {
            get { return m_CurrentCharacter; }
            set
            {
                // Record previous character
                BaseCharacter old = m_CurrentCharacter;
                // Un-set controller
                if (old != null)
                    m_CurrentCharacter.controller = null;
                // Set value
                m_CurrentCharacter = value as BaseCharacter;
                // Set controller
                if (m_CurrentCharacter != null && (BaseController)m_CurrentCharacter.controller != this)
                    m_CurrentCharacter.controller = this;
                // Fire event
                OnCurrentCharacterChanged(old, m_CurrentCharacter);
            }
        }

        protected virtual void OnCurrentCharacterChanged(BaseCharacter from, BaseCharacter to)
        {
            if (onCharacterChanged != null)
                onCharacterChanged(to);
        }

        #endregion

        #region INeoSerializableComponent implementation

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
                writer.WriteComponentReference(k_CharacterKey, m_CurrentCharacter, nsgo);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            BaseCharacter result;
            if (reader.TryReadComponentReference(k_CharacterKey, out result, nsgo))
                currentCharacter = result;
        }

        #endregion
    }
}
