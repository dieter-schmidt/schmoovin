using System.Collections;
using System.Collections.Generic;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using UnityEngine;

namespace NeoFPS
{
    public abstract class ProceduralSprintAnimationHandler : BaseSprintAnimationHandler, IAdditiveTransform
    {
        [SerializeField, Tooltip("The neutral weapon / item position in sprint pose before bob is applied.")]
        private Vector3 m_SprintOriginPos = new Vector3(0.05f, -0.05f, 0.05f);
        [SerializeField, Tooltip("The neutral weapon / item rotation in sprint pose before bob is applied.")]
        private Vector3 m_SprintOriginRot = new Vector3(10f, -30f, 15f);
        [SerializeField, Tooltip("The peak position offset of the step cycle on the z and y axes (z does not change).")]
        private Vector2 m_SprintOffset = new Vector2(0.05f, 0.025f);
        [SerializeField, Tooltip("The peak rotation of the step cycle on each axis.")]
        private Vector3 m_SprintRotation = new Vector3(2.5f, 5f, 5f);
        [SerializeField, Range (-0.5f, 0.5f), Tooltip("The offset in terms of one full step cycle (left and right) for the timing of the rotation. Positive means after the position, Negative means before.")]
        private float m_RotationDesync = 0.1f;
        [SerializeField, Tooltip("The speed at which the full sprint animation strength is reached. This fades out the rotation aspect as the character slows down.")]
        private float m_FullStrengthSpeed = 10f;

        private ICharacterStepTracker m_StepTracker = null;
        private IPoseHandler m_PoseHandler = null;
        private IAdditiveTransformHandler m_AdditiveTransformHandler = null;
        private bool m_InSprintPose = false;

        public Quaternion rotation
        {
            get;
            private set;
        }

        public Vector3 position
        {
            get;
            private set;
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            m_FullStrengthSpeed = Mathf.Clamp(m_FullStrengthSpeed, 1f, 100f);

            // Clamp animation offsets
            m_SprintOriginPos.x = Mathf.Clamp(m_SprintOriginPos.x, -1f, 1f);
            m_SprintOriginPos.y = Mathf.Clamp(m_SprintOriginPos.y, -1f, 1f);
            m_SprintOriginPos.z = Mathf.Clamp(m_SprintOriginPos.z, -1f, 1f);
            m_SprintOriginRot.x = Mathf.Clamp(m_SprintOriginRot.x, -90f, 90f);
            m_SprintOriginRot.y = Mathf.Clamp(m_SprintOriginRot.y, -90f, 90f);
            m_SprintOriginRot.z = Mathf.Clamp(m_SprintOriginRot.z, -90f, 90f);
            m_SprintOffset.x = Mathf.Clamp(m_SprintOffset.x, -1f, 1f);
            m_SprintOffset.y = Mathf.Clamp(m_SprintOffset.y, -1f, 1f);
            m_SprintRotation.x = Mathf.Clamp(m_SprintRotation.x, -45f, 45f);
            m_SprintRotation.y = Mathf.Clamp(m_SprintRotation.y, -45f, 45f);
            m_SprintRotation.z = Mathf.Clamp(m_SprintRotation.z, -90f, 90f);
        }

        protected override void Awake()
        {
            m_PoseHandler = GetComponent<IPoseHandler>();
            m_AdditiveTransformHandler = GetComponentInChildren<IAdditiveTransformHandler>();

            base.Awake();
        }

        protected override void AttachToWielder(ICharacter wielder)
        {
            base.AttachToWielder(wielder);
            m_StepTracker = wielder.GetComponent<ICharacterStepTracker>();
        }

        protected override void Update()
        {
            base.Update();

        }

        protected override void OnSprintStateChanged(SprintState s)
        {
            switch (s)
            {
                case SprintState.EnteringSprint:
                    if (!m_InSprintPose)
                    {
                        m_PoseHandler.SetPose(m_SprintOriginPos, Quaternion.Euler(m_SprintOriginRot), inTime);
                        m_AdditiveTransformHandler.ApplyAdditiveEffect(this);
                        m_InSprintPose = true;
                    }
                    break;
                case SprintState.ExitingSprint:
                    if (m_InSprintPose)
                    {
                        m_PoseHandler.ResetPose(outTime);
                        m_InSprintPose = false;
                    }
                    break;
                case SprintState.Sprinting:
                    if (!m_InSprintPose)
                    {
                        m_PoseHandler.SetPose(m_SprintOriginPos, Quaternion.Euler(m_SprintOriginRot), 0f);
                        m_AdditiveTransformHandler.ApplyAdditiveEffect(this);
                        m_InSprintPose = true;
                    }
                    break;
                case SprintState.NotSprinting:
                    if (m_InSprintPose)
                    {
                        m_PoseHandler.ResetPose(0f);
                        m_InSprintPose = false;
                    }
                    m_AdditiveTransformHandler.RemoveAdditiveEffect(this);
                    break;
            }
        }

        public void UpdateTransform()
        {
            // Get sprint cycle (from motion graph or internally if not found)
            float stepCycle = 0f;
            if (m_StepTracker != null)
                stepCycle = m_StepTracker.stepCounter * 0.5f;

            if (sprintState != SprintState.NotSprinting)
            {
                float sin = Mathf.Sin(stepCycle * Mathf.PI * 2f);
                position = new Vector3(
                    m_SprintOffset.x * sin,
                    m_SprintOffset.y * Mathf.Abs(sin),
                    0f
                    );
                sin = Mathf.Sin((stepCycle + m_RotationDesync) * Mathf.PI * 2f);
                rotation = Quaternion.Euler(
                    m_SprintRotation.x * sin,
                    m_SprintRotation.y * Mathf.Abs(sin),
                    m_SprintRotation.z * sin
                    );

                if (sprintState == SprintState.EnteringSprint || sprintState == SprintState.ExitingSprint)
                    position *= sprintWeight * sprintWeight;

                float sprintStrength = sprintWeight * sprintWeight * Mathf.Clamp01(sprintSpeed / m_FullStrengthSpeed);
                rotation = Quaternion.Lerp(Quaternion.identity, rotation, sprintStrength);
            }
        }
    }
}