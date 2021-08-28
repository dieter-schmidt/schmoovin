namespace NeoFPS.ModularFirearms
{
	public interface IEjector
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

		bool ejectOnFire { get; }

		void Eject ();
    }
}