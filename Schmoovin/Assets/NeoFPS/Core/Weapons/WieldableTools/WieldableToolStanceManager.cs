using NeoFPS.WieldableTools;
using UnityEngine;

namespace NeoFPS
{
    public class WieldableToolStanceManager : BaseWieldableStanceManager
    {
        private WieldableTool m_WieldableTool = null;
        private float m_LightTimer = 0f;
        private float m_HeavyTimer = 0f;

        protected override void Awake()
        {
            base.Awake();

            m_WieldableTool = GetComponent<WieldableTool>();
            //m_ThrownWeapon.onThrowLight += OnThrowLight;
            //m_ThrownWeapon.onThrowHeavy += OnThrowHeavy;
        }

        void Update()
        {
            if (m_LightTimer > 0f)
            {
                m_LightTimer -= Time.deltaTime;
                if (m_LightTimer <= 0f)
                {
                    m_LightTimer = 0f;
                    RemoveBlocker();
                }
            }

            if (m_HeavyTimer > 0f)
            {
                m_HeavyTimer -= Time.deltaTime;
                if (m_HeavyTimer <= 0f)
                {
                    m_HeavyTimer = 0f;
                    RemoveBlocker();
                }
            }
        }

        void OnThrowLight()
        {
            AddBlocker();
            //m_LightTimer = m_ThrownWeapon.durationLight;
        }

        void OnThrowHeavy()
        {
            AddBlocker();
            //m_HeavyTimer = m_ThrownWeapon.durationHeavy;
        }
    }
}