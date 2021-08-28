using System;
using UnityEngine;

namespace NeoCC
{
    /// <summary>
    /// IVariableGravity is used to add the ability to change a character controller's up vector and gravity vector.
    /// </summary>
    public interface INeoCharacterVariableGravity
    {
        /// <summary>
        /// The up axis for the controller in world space.
        /// </summary>
        Vector3 up { get; set; }

        /// <summary>
        /// The amount of smoothing over time to apply to changes in the controller up vector. 0 is instantaneous, 1 is 1 second for a full 180 degree rotation.
        /// </summary>
        float upSmoothing { get; set; }

        /// <summary>
        /// The gravity vector applied to this character.
        /// </summary>
        Vector3 gravity { get; set; }

        /// <summary>
        /// Used to prevent the up vector changing in certain conditions, such as climbing ladders. Defaults to false.
        /// </summary>
        bool lockUpVector { get; set; }

        /// <summary>
        /// Should the controller automatically change the up vector to match gravity (treating gravity as a down vector).
        /// </summary>
        bool orientUpWithGravity { get; set; }
    }
}
