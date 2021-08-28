using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-basicdamagehandler.html")]
	public class BasicDamageHandler : MonoBehaviour, IDamageHandler
	{
		[SerializeField, Tooltip("The value to multiply any incoming damage by. Use to reduce damage to areas like feet, or raise it for areas like the head.")]
		private float m_Multiplier = 1f;

		[SerializeField, Tooltip("Does the damage count as critical. Used to change the feedback for the damage taker and dealer.")]
		private bool m_Critical = false;

		public IHealthManager healthManager
		{
			get;
			private set;
		}

		#if UNITY_EDITOR
		protected virtual void OnValidate ()
		{
			if (m_Multiplier < 0f)
				m_Multiplier = 0f;
		}
		#endif

		protected virtual void Awake ()
		{
			healthManager = GetComponentInParent<IHealthManager>();
		}

		#region IDamageHandler implementation

		private DamageFilter m_InDamageFilter = DamageFilter.AllDamageAllTeams;
		public DamageFilter inDamageFilter 
		{
			get { return m_InDamageFilter; }
			set { m_InDamageFilter = value; }
		}

		public virtual DamageResult AddDamage (float damage)
		{
			if (healthManager != null && m_Multiplier > 0f)
			{
				healthManager.AddDamage (damage * m_Multiplier, m_Critical);
				return m_Critical ? DamageResult.Critical : DamageResult.Standard;
			}
			else
				return DamageResult.Ignored;
		}

		public virtual DamageResult AddDamage (float damage, IDamageSource source)
        {
			// Apply damage
			if (healthManager != null && m_Multiplier > 0f && CheckDamageCollision(source))
			{
                damage *= m_Multiplier;

				healthManager.AddDamage (damage, m_Critical, source);

                // Report damage dealt
                if (damage > 0f && source != null && source.controller != null)
                    source.controller.currentCharacter.ReportTargetHit(m_Critical);

                return m_Critical ? DamageResult.Critical : DamageResult.Standard;
			}
			else
				return DamageResult.Ignored;
		}

        public virtual DamageResult AddDamage(float damage, RaycastHit hit)
		{
			if (healthManager != null && m_Multiplier > 0f)
			{
				healthManager.AddDamage(damage * m_Multiplier, m_Critical, hit);
				return m_Critical ? DamageResult.Critical : DamageResult.Standard;
			}
			else
				return DamageResult.Ignored;
		}

        public virtual DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
		{
			// Apply damage
			if (healthManager != null && m_Multiplier > 0f && CheckDamageCollision(source))
			{
				damage *= m_Multiplier;

				healthManager.AddDamage(damage, m_Critical, source, hit);

				// Report damage dealt
				if (damage > 0f && source != null && source.controller != null)
					source.controller.currentCharacter.ReportTargetHit(m_Critical);

				return m_Critical ? DamageResult.Critical : DamageResult.Standard;
			}
			else
				return DamageResult.Ignored;
		}

		bool CheckDamageCollision(IDamageSource source)
		{
			return !(source != null && !source.outDamageFilter.CollidesWith(inDamageFilter, FpsGameMode.friendlyFire));
		}

        #endregion
    }
}