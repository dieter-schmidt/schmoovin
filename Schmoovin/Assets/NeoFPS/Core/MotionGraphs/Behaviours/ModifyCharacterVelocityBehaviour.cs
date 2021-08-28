using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/ModifyCharacterVelocity", "ModifyCharacterVelocityBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifycharactervelocitybehaviour.html")]
    public class ModifyCharacterVelocityBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should the character velocity be modified.")]
        private When m_When = When.EnterOnly;

        [SerializeField, Tooltip("The character height multiplier (standing height) to set on entering this state (if when is set to EnterandExit or EnterOnly).")]
        private What m_What = What.ClampSpeed;

        [SerializeField]
        private float m_FloatValue = 10f;

        [SerializeField]
        private Vector3 m_VectorValue = Vector3.zero;

        public enum When
        {
            EnterAndExit,
            EnterOnly,
            ExitOnly
        }

        public enum What
        {
            SetLocal,
            SetWorld,
            ClampSpeed,
            Multiply
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_FloatValue = Mathf.Clamp(m_FloatValue, -100f, 100f);
        }

        public override void OnEnter()
        {
            if (m_When != When.ExitOnly)
                ModifyVelocity();
        }

        public override void OnExit()
        {
            if (m_When != When.EnterOnly)
                ModifyVelocity();
        }

        void ModifyVelocity()
        {
            switch (m_What)
            {
                case What.SetLocal:
                    {
                        var localV = controller.localTransform.rotation* m_VectorValue;
                        controller.characterController.SetVelocity(localV);
                    }
                    break;
                case What.SetWorld:
                    {
                        controller.characterController.SetVelocity(m_VectorValue);
                    }
                    break;
                case What.ClampSpeed:
                    {
                        var v = controller.characterController.velocity;
                        var sqrMagnitude = v.sqrMagnitude;
                        if (sqrMagnitude > m_FloatValue * m_FloatValue)
                        {
                            v *= m_FloatValue / Mathf.Sqrt(sqrMagnitude);
                            controller.characterController.SetVelocity(v);
                        }
                    }
                    break;
                case What.Multiply:
                    {
                        var v = controller.characterController.velocity;
                        controller.characterController.SetVelocity(v * m_FloatValue);
                    }
                    break;
            }
        }
    }
}