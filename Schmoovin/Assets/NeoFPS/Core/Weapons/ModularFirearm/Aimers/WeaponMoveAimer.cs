using System.Collections;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using UnityEngine.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-weaponmoveaimer.html")]
	public class WeaponMoveAimer : BaseAimerBehaviour
    {
        [SerializeField, Tooltip("A transform that represents where the camera should be aligned while aiming down sights.")]
        private Transform m_AimOffset = null;

        [SerializeField, Tooltip("The aim offset relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        private Vector3 m_AimPosition = Vector3.zero;

        [SerializeField, Tooltip("The aim rotation relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        private Vector3 m_AimRotation = Vector3.zero;

        [SerializeField, Range(0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
        private float m_FovMultiplier = 0.75f;

        [SerializeField, Range(0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
        private float m_InputMultiplier = 0f;

        [SerializeField, Range(0f, 2f), Tooltip("The time it takes to reach full aim, or return to zero aim.")]
        private float m_AimTime = 0.25f;

        [SerializeField, FormerlySerializedAs("m_Transition"), Tooltip("The transitions easing to apply. Note: custom transition requires inheriting a new class and overriding the custom transition methods.")]
        private PositionTransition m_PositionTransition = PositionTransition.EaseIn;
        [SerializeField, Tooltip("The transitions easing to apply. Note: custom transition requires inheriting a new class and overriding the custom transition methods.")]
        private RotationTransition m_RotationTransition = RotationTransition.EaseIn;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon procedural position (to reduce severity while aiming).")]
        private float m_PositionSpringMultiplier = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon procedural rotation (to reduce severity while aiming).")]
        private float m_RotationSpringMultiplier = 0.5f;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The animator parameter key for a bool used to control aiming state in animations.")]
        private string m_AimAnimBool = "";

        [SerializeField, Tooltip("If true then the gun cannot fire while transitioning in and out of aim mode. This is used to prevent gunshots interrupting the animation.")]
        private bool m_BlockTrigger = true;

        [SerializeField, Tooltip("The crosshair to use when aiming down sights.")]
        private FpsCrosshair m_CrosshairUp = FpsCrosshair.None;

        [SerializeField, Tooltip("The crosshair to use when not aiming down sights.")]
        private FpsCrosshair m_CrosshairDown = FpsCrosshair.Default;

#if UNITY_EDITOR
        [HideInInspector]
        public bool lockInputToFoV = true;
#endif

        private IPoseHandler m_PoseHandler = null;
        private Vector3 m_PosePosition = Vector3.zero;
        private Quaternion m_PoseRotation = Quaternion.identity;
        private float m_Lerp = 0f;
        private float m_LerpMultiplier = 0f;
        private int m_AimAnimBoolHash = -1;

        public enum PositionTransition
        {
            Lerp,
            SwingUp,
            SwingAcross,
            EaseInOut,
            Overshoot,
            OvershootIn,
            Spring,
            SpringIn,
            Bounce,
            BounceIn,
            Custom,
            EaseIn,
            EaseOut
        }

        public enum RotationTransition
        {
            Lerp,
            EaseIn,
            EaseOut,
            EaseInOut,
            Overshoot,
            OvershootIn,
            Spring,
            SpringIn,
            Bounce,
            BounceIn,
            Custom
        }

        public override float fovMultiplier
        {
            get { return m_FovMultiplier; }
        }

        public bool blockTriggerOnTransition
        {
            get { return m_AimAnimBoolHash != -1 && m_BlockTrigger; }
        }

        public override float aimUpDuration
        {
            get { return m_AimTime; }
        }

        public override float aimDownDuration
        {
            get { return m_AimTime; }
        }

        private void OnValidate()
        {
            if (m_InputMultiplier == 0f)
                m_InputMultiplier = m_FovMultiplier;
        }

        protected override void OnEnable()
        {
            crosshair = m_CrosshairDown;

            // Set starting values
            m_Lerp = 0f;
            firearm.SetRecoilMultiplier(1f, 1f);

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            firearm.RemoveTriggerBlocker(this);

            if (m_CurrentAimCoroutine != null)
            {
                StopCoroutine(m_CurrentAimCoroutine);
                m_CurrentAimCoroutine = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_LerpMultiplier = 1f / m_AimTime;
            // Get pose handler
            m_PoseHandler = firearm.GetComponent<IPoseHandler>();
            // Set crosshair
            crosshair = m_CrosshairDown;
            // Get animation hash
            if (!string.IsNullOrEmpty(m_AimAnimBool))
                m_AimAnimBoolHash = Animator.StringToHash(m_AimAnimBool);

            // Build aim offset position and rotation
            bool buildFromOffsets = true;
            if (m_AimOffset != null)
            {
                var firearm = GetComponentInParent<IModularFirearm>();
                if (firearm != null)
                {
                    var firearmTransform = firearm.transform;

                    // Set the aim pose (with transition)
                    Quaternion inverse = Quaternion.Inverse(firearmTransform.rotation);
                    m_PoseRotation = Quaternion.Inverse(inverse * m_AimOffset.rotation);
                    m_PosePosition = m_PoseRotation * -firearmTransform.InverseTransformPoint(m_AimOffset.position);

                    buildFromOffsets = false;
                }
            }

            if (buildFromOffsets)
            {
                // Set the aim pose (with transition)
                m_PoseRotation = Quaternion.Inverse(Quaternion.Euler(m_AimRotation));
                m_PosePosition = m_PoseRotation * m_AimPosition;
            }
        }

        protected override void AimInternal()
        {
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
                StopCoroutine(m_CurrentAimCoroutine);
            // Set the camera fov
            if (firearm.wielder != null)
                firearm.wielder.fpCamera.SetFov(fovMultiplier, m_InputMultiplier, m_AimTime);
            // Set recoil multiplier
            firearm.SetRecoilMultiplier(m_PositionSpringMultiplier, m_RotationSpringMultiplier);
            // Start aim up coroutine
            if (gameObject.activeInHierarchy)
                m_CurrentAimCoroutine = StartCoroutine(AimCoroutine(true));
            // Set animator bool parameter
            if (firearm.animator != null && m_AimAnimBoolHash != -1 && firearm.animator.isActiveAndEnabled)
                firearm.animator.SetBool(m_AimAnimBoolHash, true);
        }

        protected override void StopAimInternal(bool instant)
        {
            // Set animator bool parameter
            if (m_AimAnimBoolHash != -1 && firearm.animator != null && firearm.animator.isActiveAndEnabled)
                firearm.animator.SetBool(m_AimAnimBoolHash, false);
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
            {
                StopCoroutine(m_CurrentAimCoroutine);
                m_CurrentAimCoroutine = null;
            }
            // Reset the camera fov
            if (firearm.wielder != null)
                firearm.wielder.fpCamera.ResetFov(m_AimTime);
            // Reset the recoil multiplier
            firearm.SetRecoilMultiplier(1f, 1f);
            // Insant vs animated
            if (instant)
            {
                // Reset weapon pose
                m_PoseHandler.ResetPose(0f);
                m_Lerp = 0f;
                // Set to hip fire
                SetAimState(FirearmAimState.HipFire);
                // Set crosshair
                crosshair = m_CrosshairDown;
            }
            else
            {
                // Start aim down coroutine
                if (gameObject.activeInHierarchy)
                    m_CurrentAimCoroutine = StartCoroutine(AimCoroutine(false));
            }
        }
        
        private Coroutine m_CurrentAimCoroutine = null;
        IEnumerator AimCoroutine(bool up)
        {
            // Block the trigger (prevents shot interrupting raise / lower)
            if (blockTriggerOnTransition)
                firearm.AddTriggerBlocker(this);

            if (up)
            {
                SetAimState(FirearmAimState.EnteringAim);

                // Set crosshair
                crosshair = m_CrosshairUp;

                CustomPositionInterpolation posInterp = GetPositionInterpolationRaise();
                CustomRotationInterpolation rotInterp = GetRotationInterpolationRaise();                

                // Set the aim pose (with transition)
                m_PoseHandler.SetPose(m_PosePosition, posInterp, m_PoseRotation, rotInterp, m_AimTime);

                // Wait for timer
                while (m_Lerp < 1f)
                {
                    yield return null;
                    m_Lerp += Time.deltaTime * m_LerpMultiplier;
                    if (m_Lerp > 1f)
                    {
                        m_Lerp = 1f;
                        SetAimState(FirearmAimState.Aiming);
                    }
                }
            }
            else
            {
                CustomPositionInterpolation posInterp = GetPositionInterpolationLower();
                CustomRotationInterpolation rotInterp = GetRotationInterpolationLower();

                // Set the aim pose (with transition)
                m_PoseHandler.ResetPose(posInterp, rotInterp, m_AimTime);
                
                SetAimState(FirearmAimState.ExitingAim);

                while (m_Lerp > 0f)
                {
                    yield return null;
                    m_Lerp -= Time.deltaTime * m_LerpMultiplier;
                    if (m_Lerp < 0f)
                    {
                        m_Lerp = 0f;
                        SetAimState(FirearmAimState.HipFire);
                    }
                }
                // Set crosshair
                crosshair = m_CrosshairDown;
            }

            // Unblock the trigger
            if (blockTriggerOnTransition)
                firearm.RemoveTriggerBlocker(this);

            m_CurrentAimCoroutine = null;
        }

        CustomPositionInterpolation GetPositionInterpolationRaise()
        {
            // Get the position interpolation
            switch (m_PositionTransition)
            {
                case PositionTransition.Lerp:
                    return PoseTransitions.PositionLerp;
                case PositionTransition.SwingUp:
                    return PoseTransitions.PositionSwingUp;
                case PositionTransition.SwingAcross:
                    return PoseTransitions.PositionSwingAcross;
                case PositionTransition.Overshoot:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.OvershootIn:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.Spring:
                    return PoseTransitions.PositionSpringIn;
                case PositionTransition.SpringIn:
                    return PoseTransitions.PositionSpringIn;
                case PositionTransition.Bounce:
                    return PoseTransitions.PositionBounceIn;
                case PositionTransition.BounceIn:
                    return PoseTransitions.PositionBounceIn;
                case PositionTransition.Custom:
                    return CustomTransitionRaise;
                case PositionTransition.EaseIn:
                    return PoseTransitions.PositionEaseInCubic;
                case PositionTransition.EaseOut:
                    return PoseTransitions.PositionEaseOutCubic;
                default:
                    return null;
            }
        }

        CustomPositionInterpolation GetPositionInterpolationLower()
        {
            // Get the position interpolation
            switch (m_PositionTransition)
            {
                case PositionTransition.Lerp:
                    return PoseTransitions.PositionLerp;
                case PositionTransition.SwingUp:
                    return PoseTransitions.PositionSwingAcross;
                case PositionTransition.SwingAcross:
                    return PoseTransitions.PositionSwingUp;
                case PositionTransition.Overshoot:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.OvershootIn:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.Spring:
                    return PoseTransitions.PositionSpringIn;
                case PositionTransition.SpringIn:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.Bounce:
                    return PoseTransitions.PositionBounceIn;
                case PositionTransition.BounceIn:
                    return PoseTransitions.PositionOvershootIn;
                case PositionTransition.Custom:
                    return CustomTransitionLower;
                case PositionTransition.EaseIn:
                    return PoseTransitions.PositionEaseInCubic;
                case PositionTransition.EaseOut:
                    return PoseTransitions.PositionEaseOutCubic;
                default:
                    return null;
            }
        }

        CustomRotationInterpolation GetRotationInterpolationRaise()
        {
            // Get the rotation interpolation
            switch (m_RotationTransition)
            {
                case RotationTransition.Lerp:
                    return PoseTransitions.RotationLerp;
                case RotationTransition.EaseIn:
                    return PoseTransitions.RotationEaseInCubic;
                case RotationTransition.EaseOut:
                    return PoseTransitions.RotationEaseOutCubic;
                case RotationTransition.EaseInOut:
                    return PoseTransitions.RotationEaseInOutCubic;
                case RotationTransition.Overshoot:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.OvershootIn:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.Spring:
                    return PoseTransitions.RotationSpringIn;
                case RotationTransition.SpringIn:
                    return PoseTransitions.RotationSpringIn;
                case RotationTransition.Bounce:
                    return PoseTransitions.RotationBounceIn;
                case RotationTransition.BounceIn:
                    return PoseTransitions.RotationBounceIn;
                case RotationTransition.Custom:
                    return CustomTransitionRotateIn;
                default:
                    return null;
            }
        }

        CustomRotationInterpolation GetRotationInterpolationLower()
        {
            // Get the rotation interpolation
            switch (m_RotationTransition)
            {
                case RotationTransition.Lerp:
                    return PoseTransitions.RotationLerp;
                case RotationTransition.EaseIn:
                    return PoseTransitions.RotationEaseInCubic;
                case RotationTransition.EaseOut:
                    return PoseTransitions.RotationEaseOutCubic;
                case RotationTransition.EaseInOut:
                    return PoseTransitions.RotationEaseInOutCubic;
                case RotationTransition.Overshoot:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.OvershootIn:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.Spring:
                    return PoseTransitions.RotationSpringIn;
                case RotationTransition.SpringIn:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.Bounce:
                    return PoseTransitions.RotationBounceIn;
                case RotationTransition.BounceIn:
                    return PoseTransitions.RotationOvershootIn;
                case RotationTransition.Custom:
                    return CustomTransitionRotateIn;
                default:
                    return null;
            }
        }

        protected virtual Vector3 CustomTransitionRaise(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, lerp);
		}

        protected virtual Vector3 CustomTransitionLower(Vector3 source, Vector3 target, float lerp)
        {
            return Vector3.Lerp(source, target, lerp);
        }

        protected virtual Quaternion CustomTransitionRotateIn(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, lerp);
        }

        protected virtual Quaternion CustomTransitionRotateOut(Quaternion source, Quaternion target, float lerp)
        {
            return Quaternion.Lerp(source, target, lerp);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            // Check if firearm is null (eg if the aimer is on a child object that was never activated)
            if (firearm != null)
            {
                if (isAiming)
                {
                    m_Lerp = 1f;
                    crosshair = m_CrosshairUp;
                    firearm.SetRecoilMultiplier(m_PositionSpringMultiplier, m_RotationSpringMultiplier);
                    if (firearm.wielder != null)
                        firearm.wielder.fpCamera.SetFov(fovMultiplier, 0f);
                }
                else
                {
                    m_Lerp = 0f;
                    crosshair = m_CrosshairDown;
                    firearm.SetRecoilMultiplier(1f, 1f);
                }
            }
        }
    }
}