using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Scripted Component")]
    public class ScriptedComponentCondition : MotionGraphCondition
    {
        [SerializeField] private string m_Key = string.Empty;

        IScriptedComponentCondition m_Component = null;

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            if (m_Component != null)
                return m_Component.CheckCondition();
            return false;
        }

        public override void Initialise(IMotionController c)
        {
            base.Initialise(c);
            IScriptedComponentCondition[] components = c.GetComponents<IScriptedComponentCondition>();
            for (int i = 0; i < components.Length; ++i)
            {
                if (components[i].key == m_Key)
                {
                    m_Component = components[i];
                    break;
                }
            }
        }
    }
}