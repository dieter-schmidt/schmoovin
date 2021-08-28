using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-smb-animstateobjectswapper.html")]
    public class AnimStateObjectSwapper : StateMachineBehaviour
	{
		[SerializeField, Tooltip("The path to the object containing the [AnimatedObjectSwapper][1] relative to the object heirarchy the animator is attached to.")]
		private string m_ObjectPath = "Object";

        [SerializeField, Range(0f, 1f), Tooltip("The normalised offset (0 to 1) from entry of the state for the object swap to occur. (0 is the start, 1 is the end).")]
		private float m_NormalisedSwapTime = 0.5f;

        private bool m_Swapped = false;
		private AnimStateObjectSwapperTarget m_Swapper = null;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			m_Swapped = false;
			if (m_Swapper == null)
			{
				Transform t = animator.transform.Find (m_ObjectPath);
				if (t != null)
					m_Swapper = t.GetComponent<AnimStateObjectSwapperTarget> ();
				if (m_Swapper == null)
					Debug.LogError ("Swapper not found by animator behaviour on object: " + animator.gameObject.name);
			}

			if (m_Swapper != null && m_NormalisedSwapTime == 0f)
			{
				m_Swapper.Swap ();
				m_Swapped = true;
			}
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!m_Swapped && m_Swapper != null && stateInfo.normalizedTime > m_NormalisedSwapTime)
			{
				m_Swapper.Swap ();
				m_Swapped = true;
			}
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!m_Swapped && m_Swapper != null)
			{
				m_Swapper.Swap ();
				m_Swapped = true;
			}
		}
	}
}