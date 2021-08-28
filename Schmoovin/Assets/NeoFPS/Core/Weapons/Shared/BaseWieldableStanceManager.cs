using System;
using UnityEngine;

namespace NeoFPS
{
    public abstract class BaseWieldableStanceManager : MonoBehaviour
    {
        [SerializeField]
        private Stance[] m_Stances = { };

        private int m_CurrentStance = -1;
        private IPoseHandler m_PoseHandler = null;
        private Animator m_Animator = null;
        private int m_Blockers = 0;

        enum PositionBlend
        {
            Lerp,
            EaseIn,
            EaseOut,
            EaseInOut,
            SwingAcross,
            SwingUp,
            Spring,
            Bounce,
            Overshoot
        }

        enum RotationBlend
        {
            Lerp,
            Slerp,
            EaseIn,
            EaseOut,
            EaseInOut,
            Spring,
            Bounce,
            Overshoot
        }

        [Serializable]
        struct Stance
        {
#pragma warning disable 0649

#if UNITY_EDITOR
            public bool expanded;
#endif

			[Tooltip("The name of the stance.")]
            public string name;
			[Tooltip("An optional name of a bool parameter in the weapon's animator.")]
            public string animatorBoolKey;
			[Tooltip("The position to move the weapon to in this stance.")]
            public Vector3 position;
			[Tooltip("The rotation of the weapon in this stance.")]
            public Vector3 rotation;
			[Tooltip("The easing method for blending between the source position and stance position on entering the stance.")]
            public PositionBlend inPositionBlend;
			[Tooltip("The easing method for blending between the source rotation and stance rotation on entering the stance.")]
            public RotationBlend inRotationBlend;
			[Tooltip("The time taken to enter the stance.")]
            public float inTime;
			[Tooltip("The easing method for blending between the stance position and idle position on exiting the stance.")]
            public PositionBlend outPositionBlend;
			[Tooltip("The easing method for blending between the stance rotation and Idle rotation on exiting the stance.")]
            public RotationBlend outRotationBlend;
			[Tooltip("The time taken to exit the stance.")]
            public float outTime;

#pragma warning restore 0649

            public int animatorBoolHash
            {
                get;
                private set;
            }

            public void Awake()
            {
                if (!string.IsNullOrEmpty(animatorBoolKey))
                    animatorBoolHash = Animator.StringToHash(animatorBoolKey);
                else
                    animatorBoolHash = -1;
            }

            public void OnValidate()
            {
                position.x = Mathf.Clamp(position.x, -1f, 1f);
                position.y = Mathf.Clamp(position.y, -1f, 1f);
                position.z = Mathf.Clamp(position.z, -1f, 1f);
                rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);
                rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
                rotation.z = Mathf.Clamp(rotation.z, -90f, 90f);
                inTime = Mathf.Clamp(inTime, 0f, 10f);
                outTime = Mathf.Clamp(outTime, 0f, 10f);
            }
        }

        public string currentStance
        {
            get
            {
                if (m_CurrentStance == -1)
                    return string.Empty;
                else
                    return m_Stances[m_CurrentStance].name;
            }
        }

        public bool isBlocked
        {
            get { return m_Blockers > 0; }
        }

        protected virtual void OnValidate()
        {
            for (int i = 0; i < m_Stances.Length; ++i)
                m_Stances[i].OnValidate();
        }

        protected virtual void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_PoseHandler = GetComponent<IPoseHandler>();
            if (m_PoseHandler == null)
                Debug.LogError("WieldableStanceManager requires a component that implements IPoseHandler to function");

            for (int i = 0; i < m_Stances.Length; ++i)
                m_Stances[i].Awake();
        }

        protected virtual void OnDisable()
        {
            m_Blockers = 0;
        }

        protected void AddBlocker()
        {
            ++m_Blockers;
            if (m_Blockers == 1)
                TransitionOut(m_CurrentStance);
        }

        protected void RemoveBlocker()
        {
            --m_Blockers;
            switch (m_Blockers)
            {
                case 0:
                    TransitionIn(m_CurrentStance);
                    break;
                case -1:
                    m_Blockers = 0;
                    break;
            }
        }

        void TransitionOut(int index)
        {
            if (index != -1)
            {
                // Get the position blend function
                CustomPositionInterpolation positionInterpolation = null;
                switch (m_Stances[index].outPositionBlend)
                {
                    case PositionBlend.Lerp:
                        positionInterpolation = PoseTransitions.PositionLerp;
                        break;
                    case PositionBlend.EaseIn:
                        positionInterpolation = PoseTransitions.PositionEaseInQuadratic;
                        break;
                    case PositionBlend.EaseOut:
                        positionInterpolation = PoseTransitions.PositionEaseOutQuadratic;
                        break;
                    case PositionBlend.SwingAcross:
                        positionInterpolation = PoseTransitions.PositionSwingAcross;
                        break;
                    case PositionBlend.SwingUp:
                        positionInterpolation = PoseTransitions.PositionSwingUp;
                        break;
                    case PositionBlend.Spring:
                        positionInterpolation = PoseTransitions.PositionSpringIn;
                        break;
                    case PositionBlend.Bounce:
                        positionInterpolation = PoseTransitions.PositionBounceIn;
                        break;
                    case PositionBlend.Overshoot:
                        positionInterpolation = PoseTransitions.PositionOvershootIn;
                        break;
                }

                // Get the rotation blend function
                CustomRotationInterpolation rotationInterpolation = null;
                switch (m_Stances[index].outRotationBlend)
                {
                    case RotationBlend.Lerp:
                        rotationInterpolation = PoseTransitions.RotationLerp;
                        break;
                    case RotationBlend.EaseIn:
                        rotationInterpolation = PoseTransitions.RotationEaseInQuadratic;
                        break;
                    case RotationBlend.EaseOut:
                        rotationInterpolation = PoseTransitions.RotationEaseOutQuadratic;
                        break;
                    case RotationBlend.Slerp:
                        rotationInterpolation = PoseTransitions.RotationSlerp;
                        break;
                    case RotationBlend.Spring:
                        rotationInterpolation = PoseTransitions.RotationSpringIn;
                        break;
                    case RotationBlend.Bounce:
                        rotationInterpolation = PoseTransitions.RotationBounceIn;
                        break;
                    case RotationBlend.Overshoot:
                        rotationInterpolation = PoseTransitions.RotationOvershootIn;
                        break;
                }

                // Apply the reset
                m_PoseHandler.ResetPose(positionInterpolation, rotationInterpolation, m_Stances[index].outTime);
            }
        }

        void TransitionIn(int index)
        {
            if (index != -1)
            {
                // Get the position blend function
                CustomPositionInterpolation positionInterpolation = null;
                switch (m_Stances[index].inPositionBlend)
                {
                    case PositionBlend.Lerp:
                        positionInterpolation = PoseTransitions.PositionLerp;
                        break;
                    case PositionBlend.EaseIn:
                        positionInterpolation = PoseTransitions.PositionEaseInQuadratic;
                        break;
                    case PositionBlend.EaseOut:
                        positionInterpolation = PoseTransitions.PositionEaseOutQuadratic;
                        break;
                    case PositionBlend.SwingAcross:
                        positionInterpolation = PoseTransitions.PositionSwingAcross;
                        break;
                    case PositionBlend.SwingUp:
                        positionInterpolation = PoseTransitions.PositionSwingUp;
                        break;
                    case PositionBlend.Spring:
                        positionInterpolation = PoseTransitions.PositionSpringIn;
                        break;
                    case PositionBlend.Bounce:
                        positionInterpolation = PoseTransitions.PositionBounceIn;
                        break;
                    case PositionBlend.Overshoot:
                        positionInterpolation = PoseTransitions.PositionOvershootIn;
                        break;
                }

                // Get the rotation blend function
                CustomRotationInterpolation rotationInterpolation = null;
                switch (m_Stances[index].inRotationBlend)
                {
                    case RotationBlend.Lerp:
                        rotationInterpolation = PoseTransitions.RotationLerp;
                        break;
                    case RotationBlend.EaseIn:
                        rotationInterpolation = PoseTransitions.RotationEaseInQuadratic;
                        break;
                    case RotationBlend.EaseOut:
                        rotationInterpolation = PoseTransitions.RotationEaseOutQuadratic;
                        break;
                    case RotationBlend.Slerp:
                        rotationInterpolation = PoseTransitions.RotationSlerp;
                        break;
                    case RotationBlend.Spring:
                        rotationInterpolation = PoseTransitions.RotationSpringIn;
                        break;
                    case RotationBlend.Bounce:
                        rotationInterpolation = PoseTransitions.RotationBounceIn;
                        break;
                    case RotationBlend.Overshoot:
                        rotationInterpolation = PoseTransitions.RotationOvershootIn;
                        break;
                }

                // Set pose
                m_PoseHandler.SetPose(m_Stances[index].position, positionInterpolation, Quaternion.Euler(m_Stances[index].rotation), rotationInterpolation, m_Stances[index].inTime);
            }
        }

        public void SetStance(string stanceName)
        {
            if (stanceName == string.Empty)
            {
                if (m_CurrentStance != -1)
                {
                    // Reset pose
                    if (m_PoseHandler != null && !isBlocked)
                        TransitionOut(m_CurrentStance);
                    // Reset animator bool parameter
                    if (m_Animator != null && m_Stances[m_CurrentStance].animatorBoolHash != -1)
                        m_Animator.SetBool(m_Stances[m_CurrentStance].animatorBoolHash, false);
                    // Set stance to idle
                    m_CurrentStance = -1;
                }
            }
            else
            {
                for (int i = 0; i < m_Stances.Length; ++i)
                {
                    if (m_Stances[i].name == stanceName)
                    {
                        if (m_CurrentStance != i)
                        {
                            if (m_PoseHandler != null && !isBlocked)
                                TransitionIn(i);

                            // Set animator bool parameter
                            if (m_Animator != null)
                            {
                                // Reset old
                                if (m_CurrentStance != -1 && m_Stances[m_CurrentStance].animatorBoolHash != -1)
                                    m_Animator.SetBool(m_Stances[m_CurrentStance].animatorBoolHash, false);
                                // Set new
                                if (m_Stances[i].animatorBoolHash != -1)
                                    m_Animator.SetBool(m_Stances[i].animatorBoolHash, true);
                            }

                            m_CurrentStance = i;
                        }
                        break;
                    }
                }
            }
        }

        public void ResetStance()
        {
            if (m_CurrentStance != -1)
            {
                // Reset pose
                if (m_PoseHandler != null)
                    m_PoseHandler.ResetPose(0f);
                // Reset animator bool parameter
                if (m_Animator != null && m_Stances[m_CurrentStance].animatorBoolHash != -1)
                    m_Animator.SetBool(m_Stances[m_CurrentStance].animatorBoolHash, false);
                // Set stance to idle
                m_CurrentStance = -1;
            }
        }
    }
}