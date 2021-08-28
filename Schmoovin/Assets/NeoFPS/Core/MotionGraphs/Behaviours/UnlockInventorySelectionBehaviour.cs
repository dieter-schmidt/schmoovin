using UnityEngine;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/UnlockInventorySelection", "UnlockInventorySelection")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-unlockinventoryselectionbehaviour.html")]
    public class UnlockInventorySelectionBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should the inventory selection be set.")]
        private When m_When = When.OnExit;

        private IQuickSlots m_QuickSlots = null;

        public enum When
        {
            OnEnter,
            OnExit
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            m_QuickSlots = controller.GetComponent<IQuickSlots>();
        }

        public override void OnEnter()
        {
            if (m_When == When.OnEnter && (m_QuickSlots as Component) != null)
                m_QuickSlots.UnlockSelection(controller.motionGraph);
        }

        public override void OnExit()
        {
            if (m_When == When.OnExit && (m_QuickSlots as Component) != null)
                m_QuickSlots.UnlockSelection(controller.motionGraph);
        }
    }
}
