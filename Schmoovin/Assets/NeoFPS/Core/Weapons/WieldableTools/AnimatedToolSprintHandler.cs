using NeoFPS.WieldableTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(WieldableTool))]
    public class AnimatedToolSprintHandler : AnimatedSprintAnimationHandler
    {
        private WieldableTool m_WieldableTool = null;
        private float m_LightTimer = 0f;
        private float m_HeavyTimer = 0f;

        protected override void Awake()
        {
            base.Awake();

            m_WieldableTool = GetComponent<WieldableTool>();
            //m_WieldableTool.onThrowLight += OnThrowLight;
            //m_WieldableTool.onThrowHeavy += OnThrowHeavy;
        }

        protected override void Update()
        {
            base.Update();

            if (m_LightTimer > 0f)
            {
                m_LightTimer -= Time.deltaTime;
                if (m_LightTimer <= 0f)
                {
                    m_LightTimer = 0f;
                    RemoveAnimationBlocker();
                }
            }

            if (m_HeavyTimer > 0f)
            {
                m_HeavyTimer -= Time.deltaTime;
                if (m_HeavyTimer <= 0f)
                {
                    m_HeavyTimer = 0f;
                    RemoveAnimationBlocker();
                }
            }
        }

        void OnThrowLight()
        {
            AddAnimationBlocker();
            //m_LightTimer = m_ThrownWeapon.durationLight;
        }

        void OnThrowHeavy()
        {
            AddAnimationBlocker();
            //m_HeavyTimer = m_ThrownWeapon.durationHeavy;
        }
    }
}