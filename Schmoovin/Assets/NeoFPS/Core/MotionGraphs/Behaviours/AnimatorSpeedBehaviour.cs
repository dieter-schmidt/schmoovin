using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using System.Collections;

namespace NeoFPS.CharacterMotion
{
    [MotionGraphElement("Animation/AnimatorSpeed", "AnimatorSpeedBehaviour")]
    public class AnimatorSpeedBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("A parameter that points to the transform of the character's animator component.")]
        private TransformParameter m_AnimatorTransform = null;
        [SerializeField, Tooltip("The animator parameter name the speed value should be written to.")]
        private string m_SpeedParamName = "speed";

        private Animator m_Animator = null;
        private int m_SpeedParamHash = -1;

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            if (m_AnimatorTransform != null)
            {
                m_AnimatorTransform.onValueChanged += OnAnimatorTransformChanged;
                OnAnimatorTransformChanged(m_AnimatorTransform.value);

                m_SpeedParamHash = Animator.StringToHash(m_SpeedParamName);
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

            m_Animator.SetFloat(m_SpeedParamHash, controller.characterController.velocity.magnitude);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_AnimatorTransform = map.Swap(m_AnimatorTransform);
        }
    }
}