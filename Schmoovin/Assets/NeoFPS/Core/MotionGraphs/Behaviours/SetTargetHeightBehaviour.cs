using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/SetTargetHeight", "SetTargetHeightBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-settargetheightbehaviour.html")]
    public class SetTargetHeightBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.EnterOnly;

        [SerializeField, Tooltip("The character height multiplier (standing height) to set on entering this state (if when is set to EnterandExit or EnterOnly).")]
        private float m_OnEnterValue = 1f;

        [SerializeField, Tooltip("The character height multiplier (standing height) to set on exiting this state (if when is set to EnterandExit or ExitOnly).")]
        private float m_OnExitValue = 1f;

        [SerializeField, Tooltip("The time taken to change heights.")]
        private float m_ResizeDuration = 0.25f;

        [SerializeField, Tooltip("The character height multiplier (standing height) to set on exiting this state (if when is set to EnterandExit or ExitOnly).")]
        private CharacterResizePoint m_FromPoint = CharacterResizePoint.Automatic;

        public enum When
        {
            EnterAndExit,
            EnterOnly,
            ExitOnly
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_OnEnterValue = Mathf.Clamp(m_OnEnterValue, 0.25f, 2f);
            m_OnExitValue = Mathf.Clamp(m_OnExitValue, 0.25f, 2f);
            m_ResizeDuration = Mathf.Clamp(m_ResizeDuration, 0f, 10f);
        }

        public override void OnEnter()
        {
            if (m_When != When.ExitOnly)
                controller.SetHeightMultiplier(m_OnEnterValue, m_ResizeDuration, m_FromPoint);
        }

        public override void OnExit()
        {
            if (m_When != When.EnterOnly)
                controller.SetHeightMultiplier(m_OnExitValue, m_ResizeDuration, m_FromPoint);
        }
    }
}