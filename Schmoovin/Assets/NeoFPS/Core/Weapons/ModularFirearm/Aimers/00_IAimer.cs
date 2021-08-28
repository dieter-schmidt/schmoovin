using UnityEngine.Events;
using NeoFPS.Constants;

namespace NeoFPS.ModularFirearms
{
	public interface IAimer
	{
		IModularFirearm firearm { get; }

		void Enable ();
		void Disable ();

        float fovMultiplier { get; }

        float hipAccuracyCap { get; }
        float aimedAccuracyCap { get; }

        bool canAimWhileReloading { get; }

        FpsCrosshair crosshair { get; }
        event UnityAction<FpsCrosshair> onCrosshairChange;

        void Aim ();
		void StopAim ();
        void StopAimInstant();
        bool isAiming { get; }

        float aimUpDuration { get; }
        float aimDownDuration { get; }

        event UnityAction<IModularFirearm, FirearmAimState> onAimStateChanged;
    }

    public enum FirearmAimState
    {
        HipFire,
        EnteringAim,
        Aiming,
        ExitingAim
    }
}