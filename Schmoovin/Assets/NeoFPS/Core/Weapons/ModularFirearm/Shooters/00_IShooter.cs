using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
	public interface IShooter
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

		void Shoot (float accuracy, IAmmoEffect effect);

		event UnityAction<IModularFirearm> onShoot;
    }
}