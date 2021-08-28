using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/TrackSteps", "TrackStepsBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-trackstepsbehaviour.html")]
    public class TrackStepsBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The travel distance for one stride.")]
        private float m_StrideLength = 3f;

        private ICharacterStepTracker m_StepTracker = null;
        private float m_OldStrideLength = 0f;

        public override void OnValidate()
        {
            base.OnValidate();

            // You may need to change these if you're trying to make the borrowers / mechwarrior
            m_StrideLength = Mathf.Clamp(m_StrideLength, 0.5f, 100f);
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            m_StepTracker = controller as ICharacterStepTracker;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (m_StepTracker != null)
                m_OldStrideLength = m_StepTracker.strideLength;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (m_StepTracker != null)
                m_StepTracker.strideLength = m_OldStrideLength;
        }

        public override void Update()
        {
            if (m_StepTracker != null)
                m_StepTracker.strideLength = m_StrideLength;
        }
    }
}