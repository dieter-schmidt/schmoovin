using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/ModifyTransformParameter", "ModifyTransformParameterBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-modifytransformparameterbehaviour.html")]
    public class ModifyTransformParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private TransformParameter m_Parameter = null;

        [SerializeField, Tooltip("When should the target height be set.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("What is the action to do.")]
        private What m_What = What.Nullify;

        [SerializeField, Tooltip("The path to the GameObject.")]
        private string m_Find = string.Empty;

        public enum When
        {
            OnEnter,
            OnExit,
            Both
        }

        public enum What
        {
            Nullify,
            Find
        }

        public override void OnEnter()
        {
            if (m_Parameter != null && (m_When == When.OnEnter || m_When == When.Both))
            {
                if (m_What == What.Nullify)
                    m_Parameter.ResetValue();
                else
                {
                    GameObject obj = GameObject.Find(m_Find);
                    if (obj != null)
                        m_Parameter.value = obj.transform;
                    else
                        m_Parameter = null;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null && (m_When == When.OnExit || m_When == When.Both))
            {
                if (m_What == What.Nullify)
                    m_Parameter.ResetValue();
                else
                {
                    GameObject obj = GameObject.Find(m_Find);
                    if (obj != null)
                        m_Parameter.value = obj.transform;
                    else
                        m_Parameter = null;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}