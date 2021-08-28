using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-smb-animstateobjectactivator.html")]
    public class AnimStateObjectActivator : StateMachineBehaviour
	{
		[SerializeField, Tooltip("The path to the relevant object in the object heirarchy the animator is attached to.")]
		private string m_ObjectPath = "Object";

		[SerializeField, Tooltip("Should the object be active when the animation state is entered.")]
		private bool m_ActiveOnEnter = true;

		[SerializeField, Tooltip("Should the object be active when the animation state is exited.")]
		private bool m_ActiveOnExit = false;

		[SerializeField, Range (0f, 1f), Tooltip("The normalised offset (0 to 1) from entry of the state for the object state to be changed (0 is the start, 1 is the end).")]
		private float m_StartOffsetNormalised = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("The normalised offset (0 to 1) from the end of the state for the object state to be changed (0 is the end, 1 is the start).")]
		private float m_EndOffsetNormalised = 0f;

        private bool m_Started = false;
        private bool m_Ended = false;
		private GameObject m_Object = null;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			m_Started = m_Ended = false;

			if (m_Object == null)
			{
				Transform t = animator.transform.Find (m_ObjectPath);
				if (t == null)
					Debug.LogError ("Object not found by animator behaviour on object. Path: " + m_ObjectPath, animator.gameObject);
				else
					m_Object = t.gameObject;
			}

			if (m_Object != null && m_StartOffsetNormalised == 0f)
			{
				m_Object.SetActive (m_ActiveOnEnter);
				m_Started = true;
			}
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (m_Object != null)
			{
				if (!m_Started && stateInfo.normalizedTime > m_StartOffsetNormalised)
				{
					m_Object.SetActive (m_ActiveOnEnter);
					m_Started = true;
				}

				if (!m_Ended && m_EndOffsetNormalised != 0f && stateInfo.normalizedTime > (1f - m_EndOffsetNormalised))
				{
					m_Object.SetActive (m_ActiveOnExit);
					m_Ended = true;
				}
			}
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (m_Object != null && !m_Ended)
				m_Object.SetActive (m_ActiveOnExit);
		}
	}
}