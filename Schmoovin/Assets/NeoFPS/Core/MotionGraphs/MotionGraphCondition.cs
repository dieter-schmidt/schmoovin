using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphCondition : ScriptableObject, IMotionGraphElement
    {
        public IMotionController controller
        {
            get;
            private set;
        }

        public virtual bool CheckCondition(MotionGraphConnectable connectable)
        {
            return false;
        }

        public virtual void Initialise (IMotionController c)
        {
            controller = c;
        }

        public virtual void CheckReferences(IMotionGraphMap map)
        {
        }

        public virtual void OnValidate ()
        {
        }
    }
}