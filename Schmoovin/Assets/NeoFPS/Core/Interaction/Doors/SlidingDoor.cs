using System;
using System.Collections;
using System.Collections.Generic;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-slidingdoor.html")]
    public class SlidingDoor : DoorBase
    {
		[SerializeField, Range(0.1f, 10f), Tooltip("The time it takes to go from fully closed to fully open and vice versa.")]
        private float m_OpenDuration = 1.5f;

		[SerializeField, Tooltip("The interpolation curve for animating the door sections.")]
        private AnimationCurve m_AnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[SerializeField, Tooltip("One or more door sections. All will move when the door opens and closes.")]
        private DoorSection[] m_Sections = new DoorSection[0];

        [Header("Audio")]

        [SerializeField, Tooltip("The audio to play when the door opens.")]
        private AudioClip m_AudioOpen = null;

        [SerializeField, Tooltip("The audio to play when the door closes.")]
        private AudioClip m_AudioClose = null;

        [SerializeField, Tooltip("The audio to play when attempting to open the door while locked.")]
        private AudioClip m_AudioLocked = null;

        [SerializeField, Tooltip("The audio to play when unlocking or locking the door.")]
        private AudioClip m_AudioUnlock = null;

        [Serializable]
        public class DoorSection
        {
			[Tooltip("The door section transform.")]
            public Transform transform;

			[Tooltip("The offset from starting (closed) position when opened.")]
            public Vector3 offset;

            [NonSerialized]
            public Vector3 originalPosition;
        }

        private Coroutine m_AnimationCoroutine = null;
        private AudioSource m_AudioSource = null;

        protected virtual void Awake()
        {
            m_AudioSource = GetComponentInChildren<AudioSource>();

            // Set up sections
            for (int i = 0; i < m_Sections.Length; ++i)
            {
                if (m_Sections[i] == null || m_Sections[i].transform == null)
                    continue;
                m_Sections[i].originalPosition = m_Sections[i].transform.localPosition;
            }
        }

        private float m_NormalisedOpen = 0f;
        public override float normalisedOpen
        {
            get { return m_NormalisedOpen; }
            protected set
            {
				m_NormalisedOpen = Mathf.Clamp01(value);

                if (Mathf.Approximately(m_NormalisedOpen, 0f))
                {
                    for (int i = 0; i < m_Sections.Length; ++i)
                    {
                        // Get the door section
                        DoorSection section = m_Sections[i];
                        if (section == null || section.transform == null)
                            continue;

						// Reset the position
                        section.transform.localPosition = section.originalPosition;
                    }
                }
                else
                {
                    float lerp = m_AnimationCurve.Evaluate(m_NormalisedOpen);
                    for (int i = 0; i < m_Sections.Length; ++i)
                    {
						// Get the door section
                        DoorSection section = m_Sections[i];
						if (section == null || section.transform == null)
                            continue;

						// Lerp the position
                        section.transform.localPosition = Vector3.Lerp(section.originalPosition, section.originalPosition + section.offset, lerp);
                    }
                }
            }
        }

        protected override void OnOpen(bool reverse = true)
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

			m_AnimationCoroutine = StartCoroutine(Animation(1f, DoorState.Open));
            state = DoorState.Opening;

            // Play audio clip
            if (m_AudioSource != null && m_AudioOpen != null)
                m_AudioSource.PlayOneShot(m_AudioOpen);
        }

        protected override void OnClose()
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            m_AnimationCoroutine = StartCoroutine(Animation(0f, DoorState.Closed));
            state = DoorState.Closing;

            // Play audio clip
            if (m_AudioSource != null && m_AudioClose != null)
                m_AudioSource.PlayOneShot(m_AudioClose);
        }

        protected override void OnTryOpenLocked()
        {
            // Play audio clip
            if (m_AudioSource != null && m_AudioLocked != null)
                m_AudioSource.PlayOneShot(m_AudioLocked);
        }

        protected override void OnLockedStateChanged(bool locked)
        {
            // Play audio clip
            if (m_AudioSource != null && m_AudioUnlock != null)
                m_AudioSource.PlayOneShot(m_AudioUnlock);
        }

        IEnumerator Animation(float target, DoorState endState)
        {
            float inverseDuration = 1f / m_OpenDuration;

            if (normalisedOpen < target)
            {
                while (true)
                {
                    yield return null;
                    float newValue = normalisedOpen + Time.deltaTime * inverseDuration;
                    if (newValue > target)
                    {
                        normalisedOpen = target;
                        break;
                    }
                    else
                        normalisedOpen = newValue;
                }
            }
            else
            {
                while (true)
                {
                    yield return null;
                    float newValue = normalisedOpen - Time.deltaTime * inverseDuration;
                    if (newValue < target)
                    {
                        normalisedOpen = target;
                        break;
                    }
                    else
                        normalisedOpen = newValue;
                }
            }

            state = endState;
            m_AnimationCoroutine = null;
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            switch (state)
            {
                case DoorState.Opening:
                    m_AnimationCoroutine = StartCoroutine(Animation(1f, DoorState.Open));
                    break;
                case DoorState.Closing:
                    m_AnimationCoroutine = StartCoroutine(Animation(0f, DoorState.Closed));
                    break;
            }
        }
    }
}