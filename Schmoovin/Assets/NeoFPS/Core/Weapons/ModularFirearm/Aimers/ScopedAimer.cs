using UnityEngine;
using System.Collections;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-scopedaimer.html")]
	public class ScopedAimer : BaseAimerBehaviour
    {
        [SerializeField, Tooltip("The aim offset relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        private Vector3 m_AimOffset = Vector3.zero;

        [SerializeField, Tooltip("The HUD scope key")]
        private string m_HudScopeKey = string.Empty;

		[SerializeField, Range (0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
		private float m_FovMultiplier = 0.25f;
        
		[SerializeField, Range (0, 2f), Tooltip("The time it takes to reach full aim, or return to zero aim.")]
		private float m_AimTime = 0.25f;

		[SerializeField, Range (0f, 1f), Tooltip("A multiplier for weapon recoil position (to reduce kick while aiming).")]
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

        private IPoseHandler m_PoseHandler = null;
        private float m_Lerp = 0f;
        private float m_LerpMultiplier = 0f;
        private int m_ScopeKeyHash = 0;
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
        
        protected override void Awake ()
		{
			base.Awake ();
            // Get lerp multiplier
            m_LerpMultiplier = 1f / m_AimTime;
            // Get pose handler
            m_PoseHandler = firearm.GetComponent<IPoseHandler>();
            // Get hashes
            m_ScopeKeyHash = Animator.StringToHash(m_HudScopeKey);
            if (!string.IsNullOrEmpty(m_AimAnimBool))
                m_AimAnimBoolHash = Animator.StringToHash(m_AimAnimBool);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Set starting values
            crosshair = m_CrosshairDown;
            m_Lerp = 0f;
            firearm.SetRecoilMultiplier(1f, 1f);
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

        protected override void AimInternal ()
		{
			// Stop aim coroutine
			if (m_CurrentAimCoroutine != null)
				StopCoroutine (m_CurrentAimCoroutine);
			// Set the camera aim
			if (firearm.wielder != null)
				firearm.wielder.fpCamera.SetFov (fovMultiplier, m_AimTime);
			// Set recoil multiplier
			firearm.SetRecoilMultiplier (m_PositionSpringMultiplier, m_RotationSpringMultiplier);
            // Start aim down coroutine
            if (gameObject.activeInHierarchy)
                m_CurrentAimCoroutine = StartCoroutine(AimCoroutine(true));
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
				HideScope ();
				firearm.wielder.fpCamera.ResetFov (m_AimTime);
			}
			// Reset the recoil multiplier
			firearm.SetRecoilMultiplier (1f, 1f);
            // Insant vs animated
            if (instant)
            {
                // Reset weapon pose
                m_PoseHandler.ResetPose(m_AimTime);
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

		private void ShowScope ()
		{
			firearm.HideGeometry ();
			HudScope.Show (m_ScopeKeyHash);
		}

		private void HideScope ()
		{
			firearm.ShowGeometry ();
			HudScope.Hide ();
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

                // Set the aim pose
                m_PoseHandler.SetPose(m_AimOffset, null, Quaternion.identity, null, m_AimTime);
                
                // Wait for timer
                while (m_Lerp < 1f)
                {
                    yield return null;
                    m_Lerp += Time.deltaTime * m_LerpMultiplier;
                    if (m_Lerp > 1f)
                    {
                        m_Lerp = 1f;
                        ShowScope();
                        SetAimState(FirearmAimState.Aiming);
                    }
                }
            }
            else
            {
                // Reset weapon pose
                m_PoseHandler.ResetPose(m_AimTime);

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
        
        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            if (isAiming)
            {
                m_Lerp = 1f;
                crosshair = m_CrosshairUp;
                firearm.SetRecoilMultiplier(m_PositionSpringMultiplier, m_RotationSpringMultiplier);
                if (firearm.wielder != null)
                    firearm.wielder.fpCamera.SetFov(fovMultiplier, 0f);
                ShowScope();
            }
            else
            {
                m_Lerp = 0f;
                crosshair = m_CrosshairDown;
                firearm.SetRecoilMultiplier(1f, 1f);
                HideScope();
            }
        }
    }
}