using UnityEngine;

namespace NeoFPS
{
    public interface IShieldManager
    {
        float shield { get; set; }
        float shieldChargeRate { get; set; }
        float shieldStepCapacity { get; set; }
        int shieldStepCount { get; set; }
        ShieldState shieldState { get; }

        event ShieldDelegates.OnShieldValueChanged onShieldValueChanged;
        event ShieldDelegates.OnShieldStateChanged onShieldStateChanged;
        event ShieldDelegates.OnShieldConfigChanged onShieldConfigChanged;

        float GetShieldedDamage(float damage, DamageType t);
        int FillShieldSteps(int count = 1);
        int EmptyShieldSteps(int count = 1);
    }

    public enum ShieldState
    {
        Stable,
        Empty,
        Recharging,
        Interrupted
    }

    public static class ShieldDelegates
    {
        public delegate void OnShieldStateChanged(IShieldManager shield, ShieldState state);
        public delegate void OnShieldValueChanged(IShieldManager shield, float from, float to);
        public delegate void OnShieldConfigChanged(IShieldManager shield);
    }
}
