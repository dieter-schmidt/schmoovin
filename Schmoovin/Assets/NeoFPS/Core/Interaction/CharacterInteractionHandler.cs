using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-characterinteractionhandler.html")]
	public class CharacterInteractionHandler : MonoBehaviour
	{
		[SerializeField, Range(0.25f, 5f), Tooltip("The maximum distance from the camera to trigger interactions.")]
		private float m_MaxDistance = 2f;

		[SerializeField, Range (1, 60), Tooltip("How frequently does the handler cast forward to check for an object. Smaller numbers mean more responsive but more wasted calculations.")]
		private int m_TickRate = 1;

		[SerializeField, Tooltip("The layers that will be checked against when casting for valid interaction targets.")]
		private LayerMask m_Layers = PhysicsFilter.Masks.Interactable;

		[SerializeField, Tooltip("The character audio clip to play for an invalid interaction.")]
		private FpsCharacterAudio m_ErrorAudio = FpsCharacterAudio.Negative;

        public delegate void HighlightedChangedDelegate(ICharacter character, IInteractiveObject highlighted);
        public delegate void InteractionSucceededDelegate(ICharacter character, IInteractiveObject interactable);
        public delegate void InteractionStartedDelegate(ICharacter character, IInteractiveObject interactable, float delay);
        public delegate void InteractionCancelledDelegate(ICharacter character, IInteractiveObject interactable);

        public event HighlightedChangedDelegate onHighlightedChanged;
        public event InteractionSucceededDelegate onInteractionSucceeded;
        public event InteractionStartedDelegate onInteractionStarted;
        public event InteractionCancelledDelegate onInteractionCancelled;

        private IInteractiveObject m_Highlighted = null;
        public IInteractiveObject highlighted
		{
			get { return m_Highlighted; }
			set
			{
                if (m_Highlighted != value)
                {
                    if (m_Highlighted != null)
                        m_Highlighted.highlighted = false;

                    m_Highlighted = value;

                    if (m_Highlighted != null)
                        m_Highlighted.highlighted = true;

                    if (onHighlightedChanged != null)
                        onHighlightedChanged(m_Character, m_Highlighted);
                }
			}
		}
		
		public LayerMask layers
		{
			get { return m_Layers; }
			set { m_Layers = value; }
		}

		private ICharacterAudioHandler m_AudioHandler = null;
        private ICharacter m_Character = null;
        private Transform m_CameraTransform = null;
        private Transform m_LocalTransform = null;
        private RaycastHit m_Hit = new RaycastHit();
        private int m_Tick = 0;

		void Start ()
		{
            m_LocalTransform = transform;
			m_Character = GetComponent<ICharacter>();
            if (m_Character == null)
            {
                Debug.LogError("CharacterInteractionHandler requires a behaviour that inherits from ICharacter. Removing.");
                Destroy(this);
            }
            else
            {
                m_AudioHandler = m_Character.audioHandler;
                m_CameraTransform = m_Character.fpCamera.unityCamera.transform;
            }
        }

        private void OnDisable()
        {
            highlighted = null;
        }

        void FixedUpdate ()
		{
			if (++m_Tick >= m_TickRate)
			{
				m_Tick = 0;

				// Get the interactable the crosshair is currently over
				IInteractiveObject over = GetValidInteractable ();
				if (highlighted != over)
				{
					// Cancel any current interaction chargeup
					if (m_DelayedInteractCoroutine != null)
					{
						StopCoroutine (m_DelayedInteractCoroutine);
						m_DelayedInteractCoroutine = null;

						if (onInteractionCancelled != null)
							onInteractionCancelled (m_Character, highlighted);
					}

					// Set the new highlighted object
					highlighted = over;
				}
			}
		}

		public void InteractPress ()
		{
			if (highlighted != null)
			{
				float holdDuration = highlighted.holdDuration;
				if (highlighted.holdDuration > 0f)
				{
					m_DelayedInteractCoroutine = StartCoroutine (DelayedInteract (holdDuration));
					if (onInteractionStarted != null)
						onInteractionStarted (m_Character, highlighted, holdDuration);
				}
				else
					OnInteractSucceeded (highlighted);
			}
			else
				OnInteractFailed ();
		}

		public void InteractRelease ()
		{
			if (m_DelayedInteractCoroutine != null)
			{
				StopCoroutine (m_DelayedInteractCoroutine);
				m_DelayedInteractCoroutine = null;

				if (onInteractionCancelled != null)
					onInteractionCancelled (m_Character, highlighted);
			}
		}

		Coroutine m_DelayedInteractCoroutine = null;
        IEnumerator DelayedInteract (float holdDuration)
		{
			// Wait for hold duration
			// (Don't use WaitForSeconds since that requires garbage collection)
			float timer = 0f;
			while (timer < holdDuration)
			{
				yield return null;
				timer += Time.deltaTime;
			}

			OnInteractSucceeded (highlighted);
			m_DelayedInteractCoroutine = null;
		}

		IInteractiveObject GetValidInteractable ()
		{
			// Check for raycast hit
			Ray ray = new Ray(m_CameraTransform.position, m_CameraTransform.forward);
			if (PhysicsExtensions.RaycastNonAllocSingle (ray, out m_Hit, m_MaxDistance, m_Layers, m_LocalTransform, QueryTriggerInteraction.Collide) && m_Hit.collider.isTrigger)
                return m_Hit.collider.GetComponentInParent<IInteractiveObject>();
			else
				return null;
		}

		protected virtual void OnInteractSucceeded (IInteractiveObject interactable)
		{
			interactable.Interact (m_Character);
			if (onInteractionSucceeded != null)
				onInteractionSucceeded (m_Character, interactable);
		}

		protected virtual void OnInteractFailed ()
		{
			if (m_ErrorAudio != FpsCharacterAudio.Undefined && m_AudioHandler != null)
				m_AudioHandler.PlayAudio (m_ErrorAudio);
		}
	}
}