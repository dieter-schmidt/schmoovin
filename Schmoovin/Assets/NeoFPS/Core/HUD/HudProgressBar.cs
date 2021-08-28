using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.SinglePlayer;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/hudref-mb-hudprogressbar.html")]
	public class HudProgressBar : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The image for the completed progress (overlaps the empty bar. Should be scaled so 100 wide fills the empty bar.")]
        private Image m_FullBar = null;

        [SerializeField, Tooltip("The image for the empty bar.")]
        private Image m_EmptyBar = null;
        
		private RectTransform m_BarTransform = null;
		private CharacterInteractionHandler m_Interact = null;

        protected override void Awake()
        {
            base.Awake();
            if (m_FullBar != null)
                m_BarTransform = m_FullBar.transform as RectTransform;
		}

        protected override void Start()
        {
            base.Start();
            if (m_FullBar != null)
            {
                // Add event handlers
                FpsSettings.gameplay.onCrosshairColorChanged += SetColour;
                SetColour(FpsSettings.gameplay.crosshairColor);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove event handlers
            FpsSettings.gameplay.onCrosshairColorChanged -= SetColour;

            // Unsubscribe from character
            if (m_Interact != null)
            {
                m_Interact.onInteractionSucceeded -= OnInteractionSucceeded;
                m_Interact.onInteractionStarted -= OnInteractionStarted;
                m_Interact.onInteractionCancelled -= OnInteractionCancelled;
            }
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_BarTransform != null)
            {
                if (m_Interact != null)
                {
                    m_Interact.onInteractionSucceeded -= OnInteractionSucceeded;
                    m_Interact.onInteractionStarted -= OnInteractionStarted;
                    m_Interact.onInteractionCancelled -= OnInteractionCancelled;
                }

                if (character as Component == null)
                    m_Interact = null;
                else
                    m_Interact = character.GetComponent<CharacterInteractionHandler>();

                if (m_Interact != null)
                {
                    m_Interact.onInteractionSucceeded += OnInteractionSucceeded;
                    m_Interact.onInteractionStarted += OnInteractionStarted;
                    m_Interact.onInteractionCancelled += OnInteractionCancelled;
                }
            }
			gameObject.SetActive (false);
		}

		public void SetColour (Color colour)
		{
			m_FullBar.color = colour;
			colour.a = 0.25f;
			m_EmptyBar.color = colour;
		}

		IEnumerator ShowProgress (float duration)
		{
			float percent = 0f;
			float increment = 100f / duration;

			while (percent < 100f)
			{
				m_BarTransform.sizeDelta = new Vector2 (percent, 1f);
				yield return null;
				percent = Mathf.Clamp (percent + (increment * Time.deltaTime), 0f, 100f);
			}
		}

		protected virtual void OnInteractionStarted (ICharacter character, IInteractiveObject interactable, float delay)
		{
			gameObject.SetActive (true);
            if (gameObject.activeInHierarchy)
                StartCoroutine(ShowProgress(delay));
		}
		protected virtual void OnInteractionSucceeded (ICharacter character, IInteractiveObject interactable)
		{
			gameObject.SetActive (false);
		}
		protected virtual void OnInteractionCancelled (ICharacter character, IInteractiveObject interactable)
		{
			gameObject.SetActive (false);
		}
	}
}