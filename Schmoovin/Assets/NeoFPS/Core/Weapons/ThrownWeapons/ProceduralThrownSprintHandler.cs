using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(ThrownWeapon))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-proceduralthrownsprinthandler.html")]
    public class ProceduralThrownSprintHandler : ProceduralSprintAnimationHandler
    {
        private ThrownWeapon m_ThrownWeapon = null;
        private float m_LightTimer = 0f;
        private float m_HeavyTimer = 0f;
        
        protected override void Awake()
        {
            base.Awake();

            m_ThrownWeapon = GetComponent<ThrownWeapon>();
            m_ThrownWeapon.onThrowLight += OnThrowLight;
            m_ThrownWeapon.onThrowHeavy += OnThrowHeavy;
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
            m_LightTimer = m_ThrownWeapon.durationLight;
        }

        void OnThrowHeavy()
        {
            AddAnimationBlocker();
            m_HeavyTimer = m_ThrownWeapon.durationHeavy;
        }
    }
}