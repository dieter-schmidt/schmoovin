using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/BlockTrigger", "BlockTriggerBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-blocktriggerparameterbehaviour.html")]
    public class BlockTriggerParameterBehaviour : MotionGraphBehaviour
    {
		[SerializeField, Tooltip("The parameter to modify.")]
        private TriggerParameter m_Parameter = null;

        [SerializeField, Tooltip("Whether to block or unblock the parameter on entering the state.")]
        private BlockValue m_OnEnter = BlockValue.Block;

        [SerializeField, Tooltip("Whether to block or unblock the parameter on exiting the state.")]
        private BlockValue m_OnExit = BlockValue.Unblock;

        public enum BlockValue
        {
            Block,
            Unblock,
            Nothing
        }

        public override void OnEnter()
        {
            if (m_Parameter != null)
            {
                // Change value
                switch (m_OnEnter)
                {
                    case BlockValue.Block:
                        m_Parameter.AddBlocker();
                        return;
                    case BlockValue.Unblock:
                        m_Parameter.RemoveBlocker();
                        return;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Parameter != null)
            {
                switch (m_OnExit)
                {
                    case BlockValue.Block:
                        m_Parameter.AddBlocker();
                        return;
                    case BlockValue.Unblock:
                        m_Parameter.RemoveBlocker();
                        return;
                }
            }
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_Parameter = map.Swap(m_Parameter);
        }
    }
}