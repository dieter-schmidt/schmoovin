using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ModifyVectorParameter", "ModifyVectorParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifyvectorparameterbehaviour.html")]
    public class ModifyVectorParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private VectorParameter m_Parameter = null;

        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("How should the parameter be modified.")]
        private What m_What = What.Set;

        [SerializeField, Tooltip("The value to set to, add or subtract based on the \"What\" parameter.")]
        private Vector3 m_Value = Vector3.zero;

        [SerializeField, Tooltip("The value to set to, add or subtract based on the \"What\" parameter.")]
        private float m_Multiplier = 1f;
        
        [SerializeField, Tooltip("The value to set to, add or subtract based on the \"What\" parameter.")]
        private float m_Clamp = 1f;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public enum What
        {
            Set,
            Reset,
            Add,
            Subtract,
            Multiply,
            Normalize,
            Flatten,
            ClampMagnitude
        }

        public override void OnEnter()
        {
            if (m_Parameter != null && (m_When == When.OnEnter || m_When == When.Both))
            {
                switch (m_What)
                {
                    case What.Set:
                        m_Parameter.value = m_Value;
                        return;
                    case What.Reset:
                        m_Parameter.ResetValue();
                        return;
                    case What.Add:
                        m_Parameter.value += m_Value;
                        return;
                    case What.Subtract:
                        m_Parameter.value -= m_Value;
                        return;
                    case What.Multiply:
                        m_Parameter.value *= m_Multiplier;
                        return;
                    case What.Normalize:
                        m_Parameter.value.Normalize();
                        return;
                    case What.Flatten:
                        m_Parameter.value = Vector3.ProjectOnPlane(m_Parameter.value, controller.characterController.up);
                        return;
                    case What.ClampMagnitude:
                        m_Parameter.value = Vector3.ClampMagnitude(m_Parameter.value, m_Clamp);
                        return;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null && (m_When == When.OnExit || m_When == When.Both))
            {
                switch (m_What)
                {
                    case What.Set:
                        m_Parameter.value = m_Value;
                        return;
                    case What.Reset:
                        m_Parameter.ResetValue();
                        return;
                    case What.Add:
                        m_Parameter.value += m_Value;
                        return;
                    case What.Subtract:
                        m_Parameter.value -= m_Value;
                        return;
                    case What.Multiply:
                        m_Parameter.value *= m_Multiplier;
                        return;
                    case What.Normalize:
                        m_Parameter.value.Normalize();
                        return;
                    case What.Flatten:
                        m_Parameter.value = Vector3.ProjectOnPlane(m_Parameter.value, controller.characterController.up);
                        return;
                    case What.ClampMagnitude:
                        m_Parameter.value = Vector3.ClampMagnitude(m_Parameter.value, m_Clamp);
                        return;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}