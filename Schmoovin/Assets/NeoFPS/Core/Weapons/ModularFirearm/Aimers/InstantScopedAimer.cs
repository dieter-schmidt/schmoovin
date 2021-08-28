using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-instantscopedaimer.html")]
	public class InstantScopedAimer : BaseAimerBehaviour
	{
        [SerializeField, Tooltip("The HUD scope key")]
        private string m_HudScopeKey = string.Empty;

		[SerializeField, Range (0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
		private float m_FovMultiplier = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon recoil position (to reduce kick while aiming).")]
        private float m_PositionSpringMultiplier = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for weapon recoil rotation (to reduce kick while aiming).")]
        private float m_RotationSpringMultiplier = 0.5f;

        [SerializeField, Tooltip("The crosshair to use when aiming down sights.")]
        private FpsCrosshair m_CrosshairUp = FpsCrosshair.None;

        [SerializeField, Tooltip("The crosshair to use when not aiming down sights.")]
        private FpsCrosshair m_CrosshairDown = FpsCrosshair.Default;

        private int m_HashedKey = 0;
        
		public override float fovMultiplier
		{
            get { return m_FovMultiplier; }
        }

        public override float aimUpDuration
        {
            get { return 0f; }
        }

        public override float aimDownDuration
        {
            get { return 0f; }
        }

        protected override void Awake ()
		{
			base.Awake ();
			m_HashedKey = Animator.StringToHash (m_HudScopeKey);
            // Set crosshair
            crosshair = m_CrosshairDown;
		}

		protected override void AimInternal ()
        {
            ShowScope();
            // Set the camera aim
            if (firearm.wielder != null)
				firearm.wielder.fpCamera.SetFov (fovMultiplier, 0f);
			// Set recoil multiplier
			firearm.SetRecoilMultiplier (m_PositionSpringMultiplier, m_RotationSpringMultiplier);
            // Send event
            SetAimState(FirearmAimState.Aiming);
            // Set crosshair
            crosshair = m_CrosshairUp;
		}

		protected override void StopAimInternal (bool instant)
        {
            HideScope();
            // Reset the camera aim
            if (firearm.wielder != null)
				firearm.wielder.fpCamera.ResetFov (0f);
			// Reset the recoil multiplier
			firearm.SetRecoilMultiplier (1f, 1f);
            // Send event
            SetAimState(FirearmAimState.HipFire);
            // Set crosshair
            crosshair = m_CrosshairDown;
        }

		private void ShowScope ()
		{
            firearm.HideGeometry ();
			HudScope.Show (m_HashedKey);
		}

		private void HideScope ()
		{
			firearm.ShowGeometry ();
			HudScope.Hide ();
		}
        
        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            if (isAiming)
            {
                // Set the camera aim
                if (firearm.wielder != null)
                    firearm.wielder.fpCamera.SetFov(fovMultiplier, 0f);
                // Set recoil multiplier
                firearm.SetRecoilMultiplier(m_PositionSpringMultiplier, m_RotationSpringMultiplier);
                // Set crosshair
                crosshair = m_CrosshairUp;
            }
            else
            {
                // Reset the camera aim
                if (firearm.wielder != null)
                {
                    HideScope();
                    firearm.wielder.fpCamera.ResetFov(0f);
                }
                // Reset the recoil multiplier
                firearm.SetRecoilMultiplier(1f, 1f);
                // Set crosshair
                crosshair = m_CrosshairDown;
            }
        }
    }
}