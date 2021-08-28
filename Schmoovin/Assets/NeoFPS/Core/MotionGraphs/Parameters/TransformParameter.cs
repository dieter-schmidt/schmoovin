using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.CharacterMotion.Parameters
{
    [MotionGraphElement("Transform", "My Transform")]
    public class TransformParameter : MotionGraphParameter
    {
        [SerializeField] private Transform m_Value = null;

        public UnityAction<Transform> onValueChanged;

        public Transform value
        {
            get { return m_Value; }
            set
            {
                if (m_Value != value)
                {
                    m_Value = value;
                    if (onValueChanged != null)
                        onValueChanged(m_Value);
                }
            }
        }

        public override void ResetValue ()
        {
            m_Value = null;
        }
    }
}

