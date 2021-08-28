using System.Collections;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-headmoveaimer.html")]
	public class HeadMoveAimer : BaseAimerBehaviour
    {
        [SerializeField, Tooltip("A transform that represents where the camera should be aligned while aiming down sights.")]
        private Transform m_AimOffset = null;

        [SerializeField, Tooltip("The aim position offset relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        private Vector3 m_AimPositionOffset = Vector3.zero;

        [SerializeField, Tooltip("The aim rotation offset relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        private Vector3 m_AimRotationOffset = Vector3.zero;

        [SerializeField, Range (0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
		private float m_FovMultiplier = 0.75f;

        [SerializeField, Range(0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
        private float m_InputMultiplier = 0f;

        [SerializeField, Range (0f, 2f), Tooltip("The time it takes to reach full aim, or return to zero aim.")]
		private float m_AimTime = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon recoil position (to reduce kick while aiming).")]
        private float m_PositionSpringMultiplier = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon recoil rotation (to reduce kick while aiming).")]
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

        private Transform m_RootTransform = null;
        private Vector3 m_PosePosition = Vector3.zero;
        private Quaternion m_PoseRotation = Quaternion.identity;
        private bool m_AimUp = false;
        private int m_AimAnimBoolHash = -1;

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
                
        protected override void Awake()
        {
            base.Awake();
            // Set crosshair
            crosshair = m_CrosshairDown;
            // Get animation hash
            if (!string.IsNullOrEmpty(m_AimAnimBool))
                m_AimAnimBoolHash = Animator.StringToHash(m_AimAnimBool);

            bool buildFromOffsets = true;
            if (m_AimOffset != null)
            {
                var firearm = GetComponentInParent<IModularFirearm>();
                if (firearm != null)
                {
                    var firearmTransform = firearm.transform;

                    // Set the aim pose (with transition)
                    Quaternion inverse = Quaternion.Inverse(firearmTransform.rotation);
                    m_PoseRotation = inverse * m_AimOffset.rotation;// Quaternion.Inverse(inverse * m_AimOffset.rotation);
                    m_PosePosition = firearmTransform.InverseTransformPoint(m_AimOffset.position); //m_PoseRotation * firearmTransform.InverseTransformPoint(m_AimOffset.position);

                    buildFromOffsets = false;
                }
            }

            if (buildFromOffsets)
            {
                // Set the aim pose (with transition)
                m_PoseRotation = Quaternion.Euler(m_AimRotationOffset);
                m_PosePosition = m_AimPositionOffset;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            var firearm = GetComponentInParent<IModularFirearm>();
            if (firearm != null)
                m_RootTransform = firearm.transform;
            else
                m_RootTransform = transform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_AimUp)
            {
                // Set crosshair
                crosshair = m_CrosshairDown;
                // Reset the recoil multiplier
                firearm.SetRecoilMultiplier(1f, 1f);
                // Reset the camera aim
                if (firearm.wielder != null)
                {
                    firearm.wielder.fpCamera.ResetOffset(m_AimTime);
                    firearm.wielder.fpCamera.ResetFov(m_AimTime);
                }
            }

            firearm.RemoveTriggerBlocker(this);

            if (m_CurrentAimCoroutine != null)
            {
                StopCoroutine(m_CurrentAimCoroutine);
                m_CurrentAimCoroutine = null;
            }
        }

        protected override void AimInternal ()
		{
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
				StopCoroutine (m_CurrentAimCoroutine);
			// Set the camera aim
			if (firearm.wielder != null)
			{
                firearm.wielder.fpCamera.SetOffset(Vector3.Scale(m_RootTransform.lossyScale, m_PosePosition), m_PoseRotation, m_AimTime);
                firearm.wielder.fpCamera.SetFov(fovMultiplier, m_InputMultiplier, m_AimTime);
            }
			// Set recoil multiplier
			firearm.SetRecoilMultiplier (m_PositionSpringMultiplier, m_RotationSpringMultiplier);
			// Start aim up coroutine
			if (gameObject.activeInHierarchy)
				m_CurrentAimCoroutine = StartCoroutine (AimCoroutine (true));
            // Set animator bool parameter
            if (firearm.animator != null && m_AimAnimBoolHash != -1)
                firearm.animator.SetBool(m_AimAnimBoolHash, true);
        }

		protected override void StopAimInternal (bool instant)
        {
            // Set animator bool parameter
            if (firearm.animator != null && m_AimAnimBoolHash != -1)
                firearm.animator.SetBool(m_AimAnimBoolHash, false);
            // Stop aim coroutine
            if (m_CurrentAimCoroutine != null)
            {
                StopCoroutine(m_CurrentAimCoroutine);
                m_CurrentAimCoroutine = null;
            }
            // Reset the camera aim
            if (firearm.wielder != null)
			{
				firearm.wielder.fpCamera.ResetOffset (m_AimTime);
				firearm.wielder.fpCamera.ResetFov (m_AimTime);
			}
			// Reset the recoil multiplier
			firearm.SetRecoilMultiplier (1f, 1f);
            // Insant vs animated
            if (instant)
            {
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
        IEnumerator AimCoroutine (bool up)
        {
            // Block the trigger (prevents shot interrupting raise / lower)
            if (blockTriggerOnTransition)
                firearm.AddTriggerBlocker(this);

            m_AimUp = up;

            if (up)
            {
                SetAimState(FirearmAimState.EnteringAim);
                // Set crosshair
                crosshair = m_CrosshairUp;
            }
            else
                SetAimState(FirearmAimState.ExitingAim);

            float timer = 0f;
			while (timer < m_AimTime)
			{
				yield return null;
				timer += Time.deltaTime;
			}

            if (up)
            {
                SetAimState(FirearmAimState.Aiming);
            }
            else
            {
                SetAimState(FirearmAimState.HipFire);
                // Set crosshair
                crosshair = m_CrosshairDown;
            }

            // Unblock the trigger
            if (blockTriggerOnTransition)
                firearm.RemoveTriggerBlocker(this);

            m_CurrentAimCoroutine = null;
		}

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            if (isAiming)
            {
                crosshair = m_CrosshairUp;
                firearm.SetRecoilMultiplier(m_PositionSpringMultiplier, m_RotationSpringMultiplier);
                if (firearm.wielder != null)
                {
                    firearm.wielder.fpCamera.SetOffset(Vector3.Scale(m_RootTransform.lossyScale, m_PosePosition), m_PoseRotation, 0f);
                    firearm.wielder.fpCamera.SetFov(fovMultiplier, 0f);
                }
            }
            else
            {
                crosshair = m_CrosshairDown;
                firearm.SetRecoilMultiplier(1f, 1f);
                if (firearm.wielder != null)
                {
                    firearm.wielder.fpCamera.ResetOffset(0f);
                    firearm.wielder.fpCamera.ResetFov(0f);
                }
            }
        }
    }
}
