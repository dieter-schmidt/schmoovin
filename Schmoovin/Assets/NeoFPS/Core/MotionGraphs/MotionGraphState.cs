using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphState : MotionGraphConnectable
    {
        #if UNITY_EDITOR

        [HideInInspector] public string stateName;

        #endif
        
        public virtual bool applyGravity { get { return true; } }

        public virtual bool applyGroundingForce { get { return true; } }

        public virtual bool ignorePlatformMove { get { return false; } }

        public virtual bool ignoreExternalForces { get { return false; } }

        public virtual Vector3 moveVector { get { return Vector3.zero; } }

        public virtual bool completed
        {
            get { return false; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public virtual void ChangeFrameOfReference (Vector3 deltaPos, Quaternion deltaRot)
        {
        }
    }
}