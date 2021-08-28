using System;
using UnityEngine;

namespace NeoCC
{
    [Flags]
    public enum NeoCharacterCollisionFlags
    {
        None = 0,
        Below = 1,
        Above = 1 << 2,
        Sides = 1 << 3,
        MaskFront = 1 << 4,
        MaskBack = 1 << 5,
        MaskLeft = 1 << 6,
        MaskRight = 1 << 7,
        Front = MaskFront | Sides,
        Back = MaskBack | Sides,
        Left = MaskLeft | Sides,
        Right = MaskRight | Sides,
        All = Below | Above | Front | Back | Left | Right
    }
}
