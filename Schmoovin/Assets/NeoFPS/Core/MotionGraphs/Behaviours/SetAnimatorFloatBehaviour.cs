using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using System.Collections;

namespace NeoFPS.CharacterMotion
{
    [MotionGraphElement("Animation/SetAnimatorFloat", "SetAnimatorFloatBehaviour")]
    public class SetAnimatorFloatBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("A parameter that points to the transform of the character's animator component.")]
        private TransformParameter m_AnimatorTransform = null;
        [SerializeField, Tooltip("The name of the animator parameter to write to.")]
        private string m_ParameterName = string.Empty;
        [SerializeField, Tooltip("When should the parameter be modified.")]
        private When m_When = When.OnEnter;
        [SerializeField, Tooltip("The value to write to the parameter on entering the state / subgraph.")]
        private float m_OnEnterValue = 0f;
        [SerializeField, Tooltip("The value to write to the parameter on exiting the state / subgraph.")]
        private float m_OnExitValue = 0f;

        private Animator m_Animator = null;
        private int m_ParameterHash = -1;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
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
            if (m_When != When.OnExit)
                m_Animator.SetFloat(m_ParameterHash, m_OnEnterValue);
        }

        public override void OnExit()
        {
            if (m_When != When.OnEnter)
                m_Animator.SetFloat(m_ParameterHash, m_OnExitValue);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_AnimatorTransform = map.Swap(m_AnimatorTransform);
        }
    }
}