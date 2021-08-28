using UnityEngine;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/LockInventorySelection", "LockInventorySelection")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-lockinventoryselectionbehaviour.html")]
    public class LockInventorySelectionBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should the inventory selection be set.")]
        private When m_When = When.OnEnter;

        [SerializeField, Tooltip("What to lock the inventory selection to.")]
        private LockSelectionTo m_LockSelectionTo = LockSelectionTo.Nothing;

        [SerializeField, Tooltip("If silent, then the inventory will not fire selection changed events, preventing changes to the HUD etc.")]
        private bool m_Silent = true;

        [SerializeField, Tooltip("The quick slot to lock the selection to.")]
        private int m_SlotIndex = 0;

        private IQuickSlots m_QuickSlots = null;

        public enum When
        {
            OnEnter,
            OnExit
        }

        public enum LockSelectionTo
        {
            Nothing,
            BackupItem,
            SlotIndex
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            m_QuickSlots = controller.GetComponent<IQuickSlots>();
        }

        public override void OnEnter()
        {
            if (m_When == When.OnEnter && (m_QuickSlots as Component) != null)
            {
                switch(m_LockSelectionTo)
                {
                    case LockSelectionTo.Nothing:
                        m_QuickSlots.LockSelectionToNothing(controller.motionGraph, m_Silent);
                        break;
                    case LockSelectionTo.BackupItem:
                        m_QuickSlots.LockSelectionToBackupItem(controller.motionGraph, m_Silent);
                        break;
                    case LockSelectionTo.SlotIndex:
                        m_QuickSlots.LockSelectionToSlot(m_SlotIndex, controller.motionGraph);
                        break;
                }
            }
        }

        public override void OnExit()
        {
            if (m_When == When.OnExit && (m_QuickSlots as Component) != null)
            {
                switch (m_LockSelectionTo)
                {
                    case LockSelectionTo.Nothing:
                        m_QuickSlots.LockSelectionToNothing(controller.motionGraph, m_Silent);
                        break;
                    case LockSelectionTo.BackupItem:
                        m_QuickSlots.LockSelectionToBackupItem(controller.motionGraph, m_Silent);
                        break;
                    case LockSelectionTo.SlotIndex:
                        m_QuickSlots.LockSelectionToSlot(m_SlotIndex, controller.motionGraph);
                        break;
                }
            }
        }
    }
}
