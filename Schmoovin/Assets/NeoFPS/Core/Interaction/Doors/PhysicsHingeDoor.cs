using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-physicshingedoor.html")]
    public class PhysicsHingeDoor : DoorBase
    {
		[SerializeField, Tooltip("Reverses the door opening direction.")]
        private bool m_ReverseDirection = false;

		[SerializeField, Tooltip("The open limit of the door. When opening, force will be applied until the door reaches this angle.")]
        private float m_OpenAngle = 120f;

		[SerializeField, Tooltip("The target speed to move the door.")]
        private float m_OpenVelocity = 250f;

		[SerializeField, Tooltip("The force applied to the door when opening or closing.")]
        private float m_OpenForce = 20f;

		[SerializeField, Tooltip("A time limit for force to be applied. If the door is blocked, it would never reach full open or closed and this value prevents it trying forever.")]
        private float m_Timeout = 5f;

		[SerializeField, Tooltip("The normalised position (0 to 1 translates to closed to full open angle) at which the door will latch when closing.")]
        private float m_AutoLatchPosition = 0.01f;

		[SerializeField, Tooltip("Prevent latching for a short perios when opened.")]
        private float m_AutoLatchBlockTime = 0.5f;

		[SerializeField, NeoObjectInHierarchyField(true), Tooltip("The hinge joint of the door.")]
        private HingeJoint m_Hinge = null;

        [SerializeField, Tooltip("An optional animated door handle. This will turn and release when the door is opened.")]
        private AnimatedDoorHandle m_Handle = null;

        [Header("Audio")]

        [SerializeField, Tooltip("The audio to play when the door opens.")]
        private AudioClip m_AudioOpen = null;

        [SerializeField, Tooltip("The audio to play when the door closes.")]
        private AudioClip m_AudioClose = null;

        [SerializeField, Tooltip("The audio to play when attempting to open the door while locked.")]
        private AudioClip m_AudioLocked = null;

        [SerializeField, Tooltip("The audio to play when unlocking or locking the door.")]
        private AudioClip m_AudioUnlock = null;

        private static readonly NeoSerializationKey k_DoorTimerKey = new NeoSerializationKey("doorTimer");

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_Hinge == null)
                m_Hinge = GetComponentInChildren<HingeJoint>();

            m_OpenAngle = Mathf.Clamp(m_OpenAngle, 90f, 180f);
            m_OpenVelocity = Mathf.Clamp(m_OpenVelocity, 50f, 5000f);
            m_OpenForce = Mathf.Clamp(m_OpenForce, 1f, 500f);
            m_Timeout = Mathf.Clamp(m_Timeout, 1f, 30f);
            m_AutoLatchPosition = Mathf.Clamp(m_AutoLatchPosition, 0.001f, 0.1f);
            m_AutoLatchBlockTime = Mathf.Clamp(m_AutoLatchBlockTime, 0.1f, 0.5f);
        }
#endif

        private bool m_DisableAutoLatch = false;
        private float m_AngleLimit = 120f;
        private float m_DoorTimer = 0f;
        private Coroutine m_AnimationCoroutine = null;
        private JointMotor m_OpenMotor;
        private JointMotor m_CloseMotor;
        private AudioSource m_AudioSource = null;

        public override float normalisedOpen
        {
            get { return m_Hinge.angle / m_AngleLimit; }
            protected set {}
        }

        void Awake()
        {
            m_AudioSource = GetComponentInChildren<AudioSource>();
            // Check if reverse
            float sign = (m_ReverseDirection) ? 1f : -1f;
            // Create motors
            m_OpenMotor = m_Hinge.motor;
            m_OpenMotor.force = m_OpenForce;
            m_OpenMotor.targetVelocity = m_OpenVelocity * sign;
            m_CloseMotor = m_Hinge.motor;
            m_CloseMotor.force = m_OpenForce;
            m_CloseMotor.targetVelocity = m_OpenVelocity * -sign;
            // Get angle limit
            m_AngleLimit = m_OpenAngle * sign;
            var limits = m_Hinge.limits;
            limits.min = 0f;
            limits.max = 0f;
            m_Hinge.limits = limits;
        }

		void FixedUpdate()
		{
            if (state != DoorState.Closed && !m_DisableAutoLatch)
            {
                if (normalisedOpen < m_AutoLatchPosition)
                {
                    LatchDoor();
                    state = DoorState.Closed;
                }
            }
        }

        protected override void OnOpen(bool reverse = true)
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            if (state == DoorState.Closed)
            {
                UnlatchDoor();
                if (m_Handle != null)
                    m_Handle.Twist();
            }

            m_AnimationCoroutine = StartCoroutine(PhysicsOpen(0f));
            state = DoorState.Opening;
        }

        protected override void OnClose()
        {
            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            m_AnimationCoroutine = StartCoroutine(PhysicsClose(0f));
            state = DoorState.Closing;
        }

        protected override void OnTryOpenLocked()
        {
            // Play audio clip
            if (m_AudioSource != null && m_AudioLocked != null)
                m_AudioSource.PlayOneShot(m_AudioLocked);
            // Turn handle
            if (m_Handle != null)
                m_Handle.Jiggle();
        }

        protected override void OnLockedStateChanged(bool locked)
        {
            // Play audio clip
            if (m_AudioSource != null && m_AudioUnlock != null)
                m_AudioSource.PlayOneShot(m_AudioUnlock);
        }

        IEnumerator PhysicsOpen(float startTime)
        {
			m_Hinge.motor = m_OpenMotor;
            m_Hinge.useMotor = true;
            m_DisableAutoLatch = true;

            m_DoorTimer = startTime;
            while (m_DoorTimer < m_Timeout)
            {
                // Yield 1 frame
                yield return null;
                m_DoorTimer += Time.deltaTime;

				// Enable auto latch
				if (m_DisableAutoLatch && m_DoorTimer > m_AutoLatchBlockTime)
                    m_DisableAutoLatch = false;

				// Check if it will hit the angle limit next frame
                float frameMove = m_Hinge.velocity * Time.deltaTime;
                if (Mathf.Abs(m_Hinge.angle + frameMove) > m_OpenAngle)
                    break;
            }

            m_Hinge.useMotor = false;
            m_DisableAutoLatch = false;

            state = DoorState.Open;
            m_AnimationCoroutine = null;
        }

        IEnumerator PhysicsClose(float startTime)
        {
            m_Hinge.motor = m_CloseMotor;
            m_Hinge.useMotor = true;

            m_DoorTimer = startTime;
            bool completed = false;
            while (m_DoorTimer < m_Timeout)
            {
                float frameMove = m_Hinge.velocity * Time.deltaTime;
                if (m_ReverseDirection)
                {
                    if (m_Hinge.angle + frameMove < 0f)
                    	completed = true;
                }
                else
                {
                    if (m_Hinge.angle + frameMove > 0f)
                        completed = true;
                }

                // Yield 1 frame
                yield return null;
                m_DoorTimer += Time.deltaTime;

                if (completed)
					break;
            }

            if (completed)
            {
                LatchDoor();
                state = DoorState.Closed;
            }
            else
                state = DoorState.Open;

            m_Hinge.useMotor = false;
            m_AnimationCoroutine = null;
        }

        void LatchDoor()
        {
            var limits = m_Hinge.limits;
            limits.min = 0f;
            limits.max = 0f;
            m_Hinge.limits = limits;

            // Play audio clip
            if (m_AudioSource != null && m_AudioClose != null)
                m_AudioSource.PlayOneShot(m_AudioClose);
        }

        void UnlatchDoor()
        {
            var limits = m_Hinge.limits;
            limits.min = Mathf.Min(0f, m_AngleLimit);
            limits.max = Mathf.Max(0f, m_AngleLimit);
            m_Hinge.limits = limits;

            // Play audio clip
            if (m_AudioSource != null && m_AudioOpen != null)
                m_AudioSource.PlayOneShot(m_AudioOpen);
        }
        
        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_DoorTimerKey, m_DoorTimer);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            reader.TryReadValue(k_DoorTimerKey, out m_DoorTimer, m_DoorTimer);
            switch (state)
            {
                case DoorState.Opening:
                    m_AnimationCoroutine = StartCoroutine(PhysicsOpen(m_DoorTimer));
                    break;
                case DoorState.Closing:
                    m_AnimationCoroutine = StartCoroutine(PhysicsClose(m_DoorTimer));
                    break;
            }
        }
    }
}