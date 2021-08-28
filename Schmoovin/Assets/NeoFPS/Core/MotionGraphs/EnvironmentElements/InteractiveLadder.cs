using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/motiongraphref-mb-interactiveladder.html")]
    public class InteractiveLadder : InteractiveObject, ILadder
    {
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

        private int m_Hash = -1;

        protected override void Awake()
        {
            localTransform = transform;
            m_Hash = Animator.StringToHash(m_PropertyKey);
        }

        public override void Interact(ICharacter character)
        {
            base.Interact(character);
            var prop = character.motionController.motionGraph.GetTransformProperty(m_Hash);
            if (prop != null)
            {
                if (prop.value == localTransform)
                    prop.value = null;
                else
                {
                    if (!IsBlocked(character.motionController))
                        prop.value = localTransform;
                }
            }
        }

        protected virtual bool IsBlocked(IMotionController controller)
        {
            // Check if another character or object is blocking the nearest entry by overriding this function
            return false;
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
	}
}