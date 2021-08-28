using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion.Parameters;
using NeoCC;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Collision Flags")]
    public class CollisionFlagsCondition : MotionGraphCondition
    {
        [SerializeField] private NeoCharacterCollisionFlags m_Filter = NeoCharacterCollisionFlags.None;
        [SerializeField] private FilterType m_FilterType = FilterType.Include;

        public enum FilterType
        {
            Include,
            Exclude
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            // Filter the character collision flags from the last frame
            var filtered = controller.characterController.collisionFlags & m_Filter;

            // Check the value
            switch(m_FilterType)
            {
                case FilterType.Include:
                    return filtered == m_Filter;
                case FilterType.Exclude:
                    return filtered == NeoCharacterCollisionFlags.None;
            }

            return false;
        }
    }
}