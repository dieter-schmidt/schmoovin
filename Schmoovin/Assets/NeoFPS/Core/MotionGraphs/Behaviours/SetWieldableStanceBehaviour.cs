using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/SetWieldableStance", "SetWieldableStanceBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-setwieldablestancebehaviour.html")]
    public class SetWieldableStanceBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("The name of the stance to use. The wieldable item needs a WieldableStanceManager component, with a stance that has this name.")]
        private string m_StanceName = string.Empty;

        private IQuickSlots m_QuickSlots = null;
        private BaseWieldableStanceManager m_StanceManager = null;
        private string m_PreviousStance = string.Empty;

        public BaseWieldableStanceManager stanceManager
        {
            get { return m_StanceManager; }
            set
            {
                if (m_StanceManager != null)
                    m_StanceManager.ResetStance(); // Based on aiming, etc

                m_StanceManager = value;

                if (m_StanceManager != null)
                {
                    m_PreviousStance = m_StanceManager.currentStance;
                    m_StanceManager.SetStance(m_StanceName);
                }
            }
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            m_QuickSlots = controller.GetComponent<IQuickSlots>();
        }

        void OnQuickSlotItemChanged(IQuickSlotItem item)
        {
            if (item != null)
                stanceManager = item.GetComponent<BaseWieldableStanceManager>();
            else
                stanceManager = null;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (m_QuickSlots != null)
            {
                m_QuickSlots.onSelectionChanged += OnQuickSlotItemChanged;
                OnQuickSlotItemChanged(m_QuickSlots.selected);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (m_QuickSlots != null)
            {
                m_QuickSlots.onSelectionChanged -= OnQuickSlotItemChanged;

                if (m_StanceManager != null)
                {
                    m_StanceManager.SetStance(m_PreviousStance);
                    m_StanceManager = null;
                }
            }
        }
    }
}