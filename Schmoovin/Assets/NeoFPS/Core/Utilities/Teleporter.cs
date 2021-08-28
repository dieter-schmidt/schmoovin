using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-teleporter.html")]
	public class Teleporter : MonoBehaviour
	{
		[SerializeField, Tooltip("The target teleporter to teleport to.")]
		private Teleporter m_Target = null;

        [SerializeField, Tooltip("The interactive object that triggers the teleport. Is disabled for cooldown.")]
		private InteractiveObject m_Interactable = null;

        [SerializeField, Tooltip("The delay between triggering and teleport.")]
		private float m_Delay = 0.5f;

		[SerializeField, Tooltip("The cooldown blocks teleport until it has completed.")]
		private float m_Cooldown = 0.5f;
        
		[SerializeField, Range(0f, 0.5f), Tooltip("An amount to raise the character on teleport to prevent ground overlap.")]
		private float m_VerticalOffset = 0.1f;

        [SerializeField, Tooltip("Should the relative rotation be reversed on teleport. For example walking in to teleporter A translates to walking out of teleporter B.")]
        private bool m_ReverseRotation = false;

        private MotionController m_Subject = null;
        private WaitForSeconds m_DelayYield = null;
        private WaitForSeconds m_CooldownYield = null;

        private Coroutine m_DelayedTeleportCoroutine = null;
        private Coroutine m_CooldownCoroutine = null;

        public Transform localTransform { get; private set; }

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Interactable == null)
				m_Interactable = GetComponentInChildren<InteractiveObject> ();

            if (m_Delay < 0f)
                m_Delay = 0f;
            if (m_Cooldown < 0f)
                m_Cooldown = 0f;
        }
#endif

        void Start ()
		{
			localTransform = transform;
			m_DelayYield = new WaitForSeconds (m_Delay);
			m_CooldownYield = new WaitForSeconds (m_Cooldown);
		}

		void OnTriggerEnter (Collider other)
		{
			if (other.gameObject.CompareTag ("Player"))
                m_Subject = other.gameObject.GetComponent<MotionController> ();
		}

		void OnTriggerExit (Collider other)
		{
			if (m_Subject == null)
				return;
			if (m_Subject.gameObject == other.gameObject)
			{
				m_Subject = null;
				if (m_DelayedTeleportCoroutine != null)
					StopCoroutine (m_DelayedTeleportCoroutine);
			}
		}

		public void Teleport ()
		{
            if (m_CooldownCoroutine != null || m_Target.m_CooldownCoroutine != null)
                return;
			if (m_DelayedTeleportCoroutine != null)
				StopCoroutine (m_DelayedTeleportCoroutine);
			if (m_Subject != null)
				m_DelayedTeleportCoroutine = StartCoroutine (DelayedTeleport ());
		}

		IEnumerator DelayedTeleport ()
		{
			m_Interactable.interactable = false;

			yield return m_DelayYield;

			// Calculate position & direction relative to starting teleporter
			Vector3 relativePosition = localTransform.InverseTransformPoint (m_Subject.localTransform.position);
			//Vector3 relativeDirection = localTransform.InverseTransformDirection (m_Subject.localTransform.forward);
			//if (m_ReverseRotation)
			//	relativeDirection *= -1f;
			float forwardMult = (m_ReverseRotation) ? -1f : 1f;
			// 2do: Setting to prevent rotating character up vector

			// Add cooldown to prevent teleporting back again
			m_Target.m_Subject = m_Subject;
			m_Target.m_CooldownCoroutine = m_Target.StartCoroutine (m_Target.Cooldown ());

			// Teleport to new position & rotation
			m_Subject.characterController.Teleport (
				m_Target.transform.TransformPoint (relativePosition) + (Vector3.up * m_VerticalOffset),
				Quaternion.FromToRotation (localTransform.forward, m_Target.localTransform.forward * forwardMult)
			);
			m_Subject = null;

			m_Interactable.interactable = true;

			m_DelayedTeleportCoroutine = null;
		}

		IEnumerator Cooldown ()
		{
			m_Interactable.interactable = false;

			yield return m_CooldownYield;
			m_Subject = null;

			m_Interactable.interactable = true;

			m_CooldownCoroutine = null;
		}
	}
}