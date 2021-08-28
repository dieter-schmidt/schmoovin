using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using System.Collections;

namespace NeoFPS.CharacterMotion
{
    [MotionGraphElement("Animation/AnimatorVelocity", "AnimatorVelocityBehaviour")]
    public class AnimatorVelocityBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("A parameter that points to the transform of the character's animator component.")]
        private TransformParameter m_AnimatorTransform = null;
        [SerializeField, Tooltip("The animator parameter name the forward input value should be written to.")]
        private string m_ForwardParamName = "forward";
        [SerializeField, Tooltip("The animator parameter name the strafe input value should be written to. (positive = right)")]
        private string m_StrafeParamName = "strafe";

        private Animator m_Animator = null;
        private int m_ForwardParamHash = -1;
        private int m_StrafeParamHash = -1;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            if (m_AnimatorTransform != null)
            {
                m_AnimatorTransform.onValueChanged += OnAnimatorTransformChanged;
                OnAnimatorTransformChanged(m_AnimatorTransform.value);

                m_ForwardParamHash = Animator.StringToHash(m_ForwardParamName);
                m_StrafeParamHash = Animator.StringToHash(m_StrafeParamName);
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

        public override void Update()
        {
            base.Update();

            // Options for ground slope, etc?
            // Do a vertical speed option for falling?

            float forward = Vector3.Dot(
                controller.characterController.velocity,
                controller.characterController.forward
                );

            float strafe = Vector3.Dot(
                controller.characterController.velocity,
                controller.characterController.right
                );

            m_Animator.SetFloat(m_ForwardParamHash, forward);
            m_Animator.SetFloat(m_StrafeParamHash, strafe);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_AnimatorTransform = map.Swap(m_AnimatorTransform);
        }
    }
}