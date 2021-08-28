using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/motiongraphref-mb-contactladder.html")]
	public class ContactLadder : MonoBehaviour, ILadder
	{
		[SerializeField, Tooltip("The trigger area for detecting contact with the ladder.")]
        private CharacterTriggerZone m_TriggerZone = null;

        [SerializeField, Tooltip("The box collider for the ladder geometry.")]
        private Collider m_RoughCollider = null;

        [SerializeField, Tooltip("The motion controller data entry for the relevant climb data.")]
        private string m_PropertyKey = "ladder";

        [SerializeField, Tooltip("The top of the ladder surface relative to the transform position.")]
        private Vector3 m_Top = Vector3.zero;

        [SerializeField, Tooltip("The spacing between rungs on the ladder.")]
        private float m_Spacing = 0.25f;

        [SerializeField, Tooltip("The length of the ladder along the ladder transform down axis from the top offset.")]
        private float m_Length = 4f;

        [SerializeField, Tooltip("The width of the ladder surface.")]
        private float m_Width = 1f;

        private int m_PropertyHash = -1;

        void OnValidate ()
		{
			if (m_TriggerZone == null)
				m_TriggerZone = GetComponentInChildren<CharacterTriggerZone> ();
		}

		void Awake ()
		{
            localTransform = transform;

            m_PropertyHash = Animator.StringToHash(m_PropertyKey);

            if (m_TriggerZone != null)
			{
				m_TriggerZone.onTriggerEnter += AttachToLadder;
				m_TriggerZone.onTriggerExit += DetachFromLadder;
			}            
			#if UNITY_EDITOR
			else
				Debug.LogError ("TriggerZoneLadder requires a trigger zone component on a child object", gameObject);
			#endif
		}

		void OnDestroy ()
		{
			if (m_TriggerZone != null)
			{
				m_TriggerZone.onTriggerEnter -= AttachToLadder;
				m_TriggerZone.onTriggerExit -= DetachFromLadder;
			}
		}

        void AttachToLadder (ICharacter c)
        {
            c.motionController.motionGraph.SetTransform(m_PropertyHash, transform);
        }

        void DetachFromLadder (ICharacter c)
        {
            c.motionController.motionGraph.SetTransform(m_PropertyHash, null);
        }

        #region ILadder implementation

        public Transform localTransform
        {
            get;
            private set;
        }

        public Collider boxCollider
        {
            get { return m_RoughCollider; }
        }

        public Vector3 top
        {
            get { return m_Top; }
        }

        public Vector3 worldTop
        {
            get { return localTransform.position + (localTransform.rotation * m_Top); }
        }

        public float spacing
        {
            get { return m_Spacing; }
        }

        public float length
        {
            get { return m_Length; }
        }

        public float width
        {
            get { return m_Width; }
        }

        public Vector3 up
        {
            get { return localTransform.up; }
        }

        public Vector3 forward
        {
            get { return localTransform.forward; }
        }

        public Vector3 across
        {
            get { return -localTransform.right; }
        }

        #endregion

        #if UNITY_EDITOR

        void OnDrawGizmos()
        {
            localTransform = transform;

            Quaternion rotation = localTransform.rotation;
            Vector3 ladderTop = worldTop;

            Vector3 bottom = ladderTop - rotation * new Vector3(0f, length, 0f);

            ExtendedGizmos.DrawArrowMarker3D(bottom, Quaternion.FromToRotation(Vector3.forward, forward), 0.5f, Color.blue);
            ExtendedGizmos.DrawArrowMarker3D(bottom, Quaternion.FromToRotation(Vector3.forward, across), 0.5f, Color.red);
            ExtendedGizmos.DrawArrowMarker3D(bottom, Quaternion.FromToRotation(Vector3.forward, up), 0.5f, Color.green);
        }

        #endif
	}
}