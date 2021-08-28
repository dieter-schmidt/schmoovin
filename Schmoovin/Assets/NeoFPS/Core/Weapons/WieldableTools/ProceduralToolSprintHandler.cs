using NeoFPS.WieldableTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(WieldableTool))]
    public class ProceduralToolSprintHandler : ProceduralSprintAnimationHandler
    {
        private WieldableTool m_WieldableTool = null;
        
        protected override void Awake()
        {
            base.Awake();

            m_WieldableTool = GetComponent<WieldableTool>();
            m_WieldableTool.onPrimaryActionStart += OnPrimaryActionStart;
            m_WieldableTool.onPrimaryActionEnd += OnPrimaryActionEnd;
            m_WieldableTool.onSecondaryActionStart += OnSecondaryActionStart;
            m_WieldableTool.onSecondaryActionEnd += OnSecondaryActionEnd;
        }

        private void OnPrimaryActionStart()
        {
            AddAnimationBlocker();
        }

        private void OnPrimaryActionEnd()
        {
            RemoveAnimationBlocker();
        }

        private void OnSecondaryActionStart()
        {
            AddAnimationBlocker();
        }

        private void OnSecondaryActionEnd()
        {
            RemoveAnimationBlocker();
        }
    }
}