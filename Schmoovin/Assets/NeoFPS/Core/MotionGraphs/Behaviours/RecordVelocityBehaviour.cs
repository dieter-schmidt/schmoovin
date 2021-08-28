using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/RecordVelocityBehaviour", "RecordVelocityBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-recordvelocitybehaviour.html")]
    public class RecordVelocityBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private VectorParameter m_Parameter = null;

        [SerializeField, Tooltip("When should the velocity be stored.")]
        private When m_When = When.OnEnter;

        public enum When
        {
            OnEnter,
            OnExit,
            Always
        }

        public override void OnEnter()
        {
            if (m_Parameter != null && m_When != When.OnExit)
                m_Parameter.value = controller.characterController.velocity;
        }

        public override void Update()
        {
            if (m_Parameter != null && m_When == When.Always)
                m_Parameter.value = controller.characterController.velocity;
        }

        public override void OnExit()
        {
            if (m_Parameter != null && m_When != When.OnEnter)
                m_Parameter.value = controller.characterController.velocity;
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}