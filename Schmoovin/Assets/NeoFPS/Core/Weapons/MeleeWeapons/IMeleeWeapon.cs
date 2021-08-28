using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
	public interface IMeleeWeapon
    {
        ICharacter wielder { get; }
        bool blocking { get; set; }
		void Attack ();

        event UnityAction onAttack;
        event UnityAction<bool> onBlockStateChange;
	}
}
