using System;
using UnityEngine;
using NeoCC;
using NeoFPS.CharacterMotion;

namespace NeoFPS
{
	public interface ICharacter
	{
		event CharacterDelegates.OnControllerChange onControllerChanged;
		event CharacterDelegates.OnIsAliveChange onIsAliveChanged;
        
		IController controller { get; set; }

		FirstPersonCamera fpCamera { get; }
        IMotionController motionController { get; }
		IAimController aimController { get; }
		ICharacterAudioHandler audioHandler { get; }
        
		AdditiveTransformHandler headTransformHandler { get; }
        AdditiveTransformHandler bodyTransformHandler { get; }

        IInventory inventory { get; }
		IQuickSlots quickSlots { get; }
		
		bool isAlive { get; }
		bool isPlayerControlled { get; }
		bool isLocalPlayerControlled { get; }
		bool isRemotePlayerControlled { get; }

		void Kill ();

        event CharacterDelegates.OnHitTarget onHitTarget;
        void ReportTargetHit(bool critical);

        // Add monobehaviour methods to remove need for casting if required
        GameObject gameObject { get; }
        Transform transform { get; }
		T GetComponent<T> ();
		T GetComponentInChildren<T> ();
		T GetComponentInParent<T> ();
		T[] GetComponents<T> ();
		T[] GetComponentsInChildren<T> (bool includeInactive = false);
		T[] GetComponentsInParent<T> (bool includeInactive = false);		
		Component GetComponent(Type t);
		Component GetComponentInChildren(Type t);
		Component GetComponentInParent(Type t);
		Component[] GetComponents(Type t);
		Component[] GetComponentsInChildren(Type t, bool includeInactive = false);
		Component[] GetComponentsInParent(Type t, bool includeInactive = false);
	}

	public static class CharacterDelegates
	{
		public delegate void OnControllerChange (ICharacter character, IController controller);
		public delegate void OnIsAliveChange (ICharacter character, bool alive);
        public delegate void OnHitTarget(ICharacter character, bool critical);
    }
}