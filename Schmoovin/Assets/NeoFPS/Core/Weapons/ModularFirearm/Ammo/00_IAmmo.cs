using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	public interface IAmmo
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

        string printableName { get; }

		int maxAmmo { get; }
		int currentAmmo { get; }
		bool available { get; }
		bool atMaximum { get; }

		event UnityAction<IModularFirearm, int> onCurrentAmmoChange;

		IAmmoEffect effect { get; }

		void DecrementAmmo (int amount);
		void IncrementAmmo (int amount);
    }
}