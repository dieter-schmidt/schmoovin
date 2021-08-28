using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/ModifyStamina", "ModifyStaminaBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifystaminabehaviour.html")]
    public class ModifyStaminaBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should stamina be modified.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("What should the modification be.")]
        private What m_What = What.Increment;

        [SerializeField, Tooltip("value to use for modifying the stamina.")]
        private float m_Amount = 0f;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public enum What
        {
            Increment,
            Decrement,
            IncrementNormalised,
            DecrementNormalised,
            SetToValue,
            SetToValueNormalised,
            SetToMax,
            SetToZero
        }

        private IStaminaSystem m_StaminaSystem = null;

        public override void OnValidate()
        {
            base.OnValidate();

            if (m_Amount < 0f)
                m_Amount = 0f;
            if (m_What == What.IncrementNormalised && m_Amount > 1f)
                m_Amount = 1f;
            if (m_What == What.DecrementNormalised && m_Amount > 1f)
                m_Amount = 1f;
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            // Get the stamina system
            m_StaminaSystem = controller.GetComponent<IStaminaSystem>();
        }

        public override void OnEnter()
        {
            if (m_StaminaSystem != null && m_When != When.OnExit)
            {
                switch (m_What)
                {
                    case What.Increment:
                        m_StaminaSystem.IncrementStamina(m_Amount, false);
                        break;
                    case What.Decrement:
                        m_StaminaSystem.DecrementStamina(m_Amount, false);
                        break;
                    case What.IncrementNormalised:
                        m_StaminaSystem.IncrementStamina(m_Amount, true);
                        break;
                    case What.DecrementNormalised:
                        m_StaminaSystem.DecrementStamina(m_Amount, true);
                        break;
                    case What.SetToValue:
                        m_StaminaSystem.SetStamina(m_Amount, false);
                        break;
                    case What.SetToValueNormalised:
                        m_StaminaSystem.SetStamina(m_Amount, true);
                        break;
                    case What.SetToMax:
                        m_StaminaSystem.SetStamina(1f, true);
                        break;
                    case What.SetToZero:
                        m_StaminaSystem.SetStamina(0f, true);
                        break;
                }
            }
        }

        public override void OnExit()
        {
            if (m_StaminaSystem != null && m_When != When.OnEnter)
            {
                switch (m_What)
                {
                    case What.Increment:
                        m_StaminaSystem.IncrementStamina(m_Amount, false);
                        break;
                    case What.Decrement:
                        m_StaminaSystem.DecrementStamina(m_Amount, false);
                        break;
                    case What.IncrementNormalised:
                        m_StaminaSystem.IncrementStamina(m_Amount, true);
                        break;
                    case What.DecrementNormalised:
                        m_StaminaSystem.DecrementStamina(m_Amount, true);
                        break;
                    case What.SetToValue:
                        m_StaminaSystem.SetStamina(m_Amount, false);
                        break;
                    case What.SetToValueNormalised:
                        m_StaminaSystem.SetStamina(m_Amount, true);
                        break;
                    case What.SetToMax:
                        m_StaminaSystem.SetStamina(1f, true);
                        break;
                    case What.SetToZero:
                        m_StaminaSystem.SetStamina(0f, true);
                        break;
                }
            }
        }
    }
}