using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public interface IStaminaSystem
    {
        float stamina { get; }

        float maxStamina { get; set; }

        float staminaRefreshRate { get; set; }

        float staminaNormalised { get; }

        bool isExhausted { get; }

        event UnityAction onStaminaChanged;

        void AddStaminaDrain(StaminaDrainDelegate drain);
        void RemoveStaminaDrain(StaminaDrainDelegate drain);
        void SetStamina(float amount, bool normalised = false);
        void IncrementStamina(float amount, bool isFactor = false);
        void DecrementStamina(float amount, bool isFactor = false);
    }

    /// <summary>
    /// A callback used to apply stamina drains for a stamina system
    /// </summary>
    /// <param name="system">The stamina system the drain is applied to.</param>
    /// <param name="modifiedStamina">The current stamina. This can differ from the `system.stamina` value as refresh and drains are applied.</param>
    /// <returns>The amount of stamina to subtract (should take time into account).</returns>
    public delegate float StaminaDrainDelegate(IStaminaSystem system, float modifiedStamina);
}
