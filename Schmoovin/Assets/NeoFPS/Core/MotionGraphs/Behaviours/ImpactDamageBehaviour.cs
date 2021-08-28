using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/ImpactDamage", "ImpactDamageBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-impactdamagebehaviour.html")]
    public class ImpactDamageBehaviour : MotionGraphBehaviour
    {
        [Header("Fall Damage")]
        [SerializeField] private DamageSetting m_FallDamageOnEnter = DamageSetting.Disable;
        [SerializeField] private DamageSetting m_FallDamageOnExit = DamageSetting.Enable;

        [Header("Body Impact Damage")]
        [SerializeField] private DamageSetting m_BodyImpactDamageOnEnter = DamageSetting.Ignore;
        [SerializeField] private DamageSetting m_BodyImpactDamageOnExit = DamageSetting.Ignore;

        [Header("Head Impact Damage")]
        [SerializeField] private DamageSetting m_HeadImpactDamageOnEnter = DamageSetting.Ignore;
        [SerializeField] private DamageSetting m_HeadImpactDamageOnExit = DamageSetting.Ignore;


        public enum DamageSetting
        {
            Enable,
            Disable,
            Ignore
        }

        private BaseCharacter m_Character = null;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            // Get the character component
            m_Character = controller.GetComponent<BaseCharacter>();
        }

        public override void OnEnter()
        {
            if (m_Character != null)
            {
                // Set fall damage
                switch (m_FallDamageOnEnter)
                {
                    case DamageSetting.Enable:
                        m_Character.applyFallDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyFallDamage = false;
                        break;
                }

                // Set body impact damage
                switch (m_BodyImpactDamageOnEnter)
                {
                    case DamageSetting.Enable:
                        m_Character.applyBodyImpactDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyBodyImpactDamage = false;
                        break;
                }

                // Set head impact damage
                switch (m_HeadImpactDamageOnEnter)
                {
                    case DamageSetting.Enable:
                        m_Character.applyHeadImpactDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyHeadImpactDamage = false;
                        break;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Character != null)
            {
                // Set fall damage
                switch (m_FallDamageOnExit)
                {
                    case DamageSetting.Enable:
                        m_Character.applyFallDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyFallDamage = false;
                        break;
                }

                // Set body impact damage
                switch (m_BodyImpactDamageOnExit)
                {
                    case DamageSetting.Enable:
                        m_Character.applyBodyImpactDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyBodyImpactDamage = false;
                        break;
                }

                // Set head impact damage
                switch (m_HeadImpactDamageOnExit)
                {
                    case DamageSetting.Enable:
                        m_Character.applyHeadImpactDamage = true;
                        break;
                    case DamageSetting.Disable:
                        m_Character.applyHeadImpactDamage = false;
                        break;
                }
            }
        }
    }
}