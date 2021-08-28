using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
	public interface IController
	{
		bool isPlayer { get; }
        bool isLocalPlayer { get; }

		event UnityAction<ICharacter> onCharacterChanged;

		ICharacter currentCharacter { get; set; }
	}
}