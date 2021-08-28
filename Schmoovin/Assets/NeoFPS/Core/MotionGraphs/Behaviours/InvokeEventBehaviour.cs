using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Parameters/InvokeEvent", "InvokeEventBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-invokeeventbehaviour.html")]
    public class InvokeEventBehaviour : MotionGraphBehaviour
	{
		[SerializeField, Tooltip("The event property to invoke.")]
		private EventParameter m_Property;

        [SerializeField, Tooltip("When should event be invoked.")]
        private When m_When = When.OnEnter;

		public enum When
		{
			OnEnter,
			OnExit,
			OnEnterAndExit,
            Update
		}

		public override void OnEnter()
		{
			if (m_Property != null && (m_When == When.OnEnter || m_When == When.OnEnterAndExit))
				m_Property.Invoke();
		}

		public override void OnExit()
		{
			if (m_Property != null && (m_When == When.OnExit || m_When == When.OnEnterAndExit))
				m_Property.Invoke();
		}

        public override void Update()
        {
            if (m_Property != null && m_When == When.Update)
                m_Property.Invoke();
        }

        public override void CheckReferences(IMotionGraphMap map)
		{
			m_Property = map.Swap(m_Property);
		}
	}
}