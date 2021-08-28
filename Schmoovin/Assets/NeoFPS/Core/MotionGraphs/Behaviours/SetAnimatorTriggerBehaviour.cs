using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using System.Collections;

namespace NeoFPS.CharacterMotion
{
    [MotionGraphElement("Animation/SetAnimatorTrigger", "SetAnimatorTriggerBehaviour")]
    public class SetAnimatorTriggerBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("A parameter that points to the transform of the character's animator component.")]
        private TransformParameter m_AnimatorTransform = null;
        [SerializeField, Tooltip("The name of the animator parameter to write to.")]
        private string m_ParameterName = string.Empty;
        [SerializeField, Tooltip("The action to perform on entering the state / subgraph.")]
        private Action m_OnEnter = Action.Set;
        [SerializeField, Tooltip("The action to perform on exiting the state / subgraph.")]
        private Action m_OnExit = Action.Ignore;

        private Animator m_Animator = null;
        private int m_ParameterHash = -1;

        public enum Action
        {
            Set,
            Reset,
            Ignore
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            if (m_AnimatorTransform != null && !string.IsNullOrWhiteSpace(m_ParameterName))
            {
                m_AnimatorTransform.onValueChanged += OnAnimatorTransformChanged;
                OnAnimatorTransformChanged(m_AnimatorTransform.value);

                m_ParameterHash = Animator.StringToHash(m_ParameterName);
            }
            else
                enabled = false;
        }

        void OnAnimatorTransformChanged(Transform t)
        {
            if (t != null)
            {
                m_Animator = t.GetComponent<Animator>();
                enabled = (m_Animator != null);
            }
            else
            {
                m_Animator = null;
                enabled = false;
            }
        }

        public override void OnEnter()
        {
            switch(m_OnEnter)
            {
                case Action.Set:
                    m_Animator.SetTrigger(m_ParameterHash);
                    break;
                case Action.Reset:
                    m_Animator.ResetTrigger(m_ParameterHash);
                    break;
            }
        }

        public override void OnExit()
        {
            switch (m_OnExit)
            {
                case Action.Set:
                    m_Animator.SetTrigger(m_ParameterHash);
                    break;
                case Action.Reset:
                    m_Animator.ResetTrigger(m_ParameterHash);
                    break;
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_AnimatorTransform = map.Swap(m_AnimatorTransform);
        }
    }
}