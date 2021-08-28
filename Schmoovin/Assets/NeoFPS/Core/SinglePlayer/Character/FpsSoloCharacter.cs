using System;
using UnityEngine;
using UnityEngine.Events;
using NeoCC;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;

namespace NeoFPS.SinglePlayer
{
    [HelpURL("https://docs.neofps.com/manual/fpcharactersref-mb-fpssolocharacter.html")]
    public class FpsSoloCharacter : BaseCharacter
    {
        public static event UnityAction<FpsSoloCharacter> onLocalPlayerCharacterChange;

        private static FpsSoloCharacter m_LocalPlayerCharacter = null;
        public static FpsSoloCharacter localPlayerCharacter
        {
            get { return m_LocalPlayerCharacter; }
            set
            {
                m_LocalPlayerCharacter = value;
                if (onLocalPlayerCharacterChange != null)
                    onLocalPlayerCharacterChange(m_LocalPlayerCharacter);
            }
        }

        protected override void OnControllerChanged()
        {
            base.OnControllerChanged();
            
            // Check if controller is player
            if (controller != null)
            {
                if (controller.isPlayer)
                {
                    localPlayerCharacter = this;
                    SetFirstPerson(true);
                }
                else
                {
                    if (localPlayerCharacter == this)
                        localPlayerCharacter = null;
                    SetFirstPerson(false);
                }

                if ((FpsSoloCharacter)controller.currentCharacter != this)
                    controller.currentCharacter = this;

                gameObject.SetActive(((MonoBehaviour)controller).isActiveAndEnabled);
            }
            else
            {
                if (localPlayerCharacter == this)
                    localPlayerCharacter = null;
                SetFirstPerson(false);
                // Disable the object (needs a controller to function)
                gameObject.SetActive(false);
            }
        }
    }
}

