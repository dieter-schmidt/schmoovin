using System;
using UnityEngine;

namespace NeoFPS
{
    /// <summary>
    /// KeyBindingContext is used to track which FpsInputButton can clash and which must have unique key bindings.
    /// For example, right-click would be used for aiming with some weapons and secondary action with others.
    /// You can add to this if requried and then set the context in the m_KeyCodes entries.
    /// </summary>
    public enum KeyBindingContext
    {
        Default, // The majority of FPS actions. These can overlap with other contexts like driving / flying
        WeaponAim, // The key cannot be used with other aim or default inputs
        WeaponSecondary, // The key cannot be used with other secondary or default inputs
        Driving, // A unique context that doesn't clash with others
        Aircraft, // A unique context that doesn't clash with others
        Helicopter // A unique context that doesn't clash with others
    }

    // This is a temporary workaround until switching to the new Unity input system

    public static class KeyBindingContextMatrix
    {
        public static bool CanOverlap (KeyBindingContext a, KeyBindingContext b)
        {
            if (a == b) // Can't overlap within the same context
                return false;

            if ((a == KeyBindingContext.WeaponAim && b == KeyBindingContext.WeaponSecondary) ||
                (a == KeyBindingContext.WeaponSecondary && b == KeyBindingContext.WeaponAim))
                return true; // Aimable & dual action can overlap

            if ((a == KeyBindingContext.Default && (b == KeyBindingContext.WeaponAim || b == KeyBindingContext.WeaponSecondary)) ||
                (b == KeyBindingContext.Default && (a == KeyBindingContext.WeaponAim || a == KeyBindingContext.WeaponSecondary)))
                return false; // Default can't overlap with aimable or dual action

            // Other contexts can overlap
            return true;
        }

        public static bool CanOverlap(FpsInputButton a, FpsInputButton b)
        {
            if (a == b) // Can't overlap the same button
                return false;

            return CanOverlap(NeoFpsInputManager.GetKeyBindingContext(a), NeoFpsInputManager.GetKeyBindingContext(b));
        }
    }
}