namespace NeoFPS.ModularFirearms
{
	public interface IMuzzleEffect
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

		void Fire ();
		void StopContinuous ();
		void FireContinuous ();
    }
}