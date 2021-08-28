using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(MeleeWeapon))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-proceduralmeleesprinthandler.html")]
    public class ProceduralMeleeSprintHandler : ProceduralSprintAnimationHandler
    {
        [SerializeField, Tooltip("The amount of time the sprint animation will be paused after an attack.")]
        private float m_AttackTime = 0.5f;
        [SerializeField, Tooltip("The minimum amount of time the sprint animation will be paused when blocking (prevents tapping block).")]
        private float m_MinBlockTime = 0.5f;

        private MeleeWeapon m_MeleeWeapon = null;
        private bool m_Blocking = false;
        private float m_AttackTimer = 0f;
        private float m_BlockTimer = 0f;

        protected override void OnValidate()
        {
            base.OnValidate();

            m_AttackTime = Mathf.Clamp(m_AttackTime, 0.1f, 10f);
            m_MinBlockTime = Mathf.Clamp(m_MinBlockTime, 0f, 10f);
        }

        protected override void Awake()
        {
            base.Awake();

            m_MeleeWeapon = GetComponent<MeleeWeapon>();
            m_MeleeWeapon.onAttack += OnAttack;
            m_MeleeWeapon.onBlockStateChange += OnBlockStateChanged;
        }

        protected override void Update()
        {
            base.Update();

            if (m_AttackTimer > 0f)
            {
                m_AttackTimer -= Time.deltaTime;
                if (m_AttackTimer <= 0f)
                {
                    m_AttackTimer = 0f;
                    RemoveAnimationBlocker();
                }
            }

            if (m_BlockTimer > 0f)
            {
                m_BlockTimer -= Time.deltaTime;
                if (m_BlockTimer <= 0f)
                {
                    m_BlockTimer = 0f;
                    if (!m_Blocking)
                        RemoveAnimationBlocker();
                }
            }
        }

        void OnAttack()
        {
            AddAnimationBlocker();
            m_AttackTimer = m_AttackTime;
        }

        void OnBlockStateChanged(bool block)
        {
            if (block)
            {
                if (!m_Blocking && m_BlockTimer == 0f)
                    AddAnimationBlocker();
                m_BlockTimer = m_MinBlockTime;
            }
            if (!block && m_Blocking)
            {
                if (m_BlockTimer <= 0f)
                    RemoveAnimationBlocker();
            }

            m_Blocking = block;
        }
    }
}