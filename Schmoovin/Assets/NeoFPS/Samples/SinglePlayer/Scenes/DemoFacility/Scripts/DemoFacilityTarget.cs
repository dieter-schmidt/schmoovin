using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples.SinglePlayer
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-demofacilitytarget.html")]
	public class DemoFacilityTarget : MonoBehaviour, IHealthManager
	{
		[SerializeField, Tooltip("The target damage tracker.")]
		private DemoFacilityTargetDamageTracker m_Tracker = null;

        [SerializeField, Range(0, 4), Tooltip("The target index in the tracker.")]
        private int m_TargetIndex = 0;

        #region IHealthManager implementation

        #pragma warning disable 0067

        public event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        public event HealthDelegates.OnHealthChanged onHealthChanged;
        public event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

        #pragma warning restore 0067

        public bool isAlive
		{
			get;
			private set;
		}

		public float health
        {
            get;
            set;
		}

		public float healthMax
		{
			get;
			set;
        }

        public float normalisedHealth
        {
            get { return health / healthMax; }
            set { health = value * healthMax; }
        }

        public void AddDamage (float d)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, d);
		}

		public void AddDamage (float d, bool critical)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, d);
		}

		public void AddDamage (float d, IDamageSource source)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, d);
		}

		public void AddDamage (float d, bool critical, IDamageSource source)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, d);
		}

		public void AddDamage(float d, bool critical, RaycastHit hit)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage(m_TargetIndex, d);
		}

		public void AddDamage(float d, bool critical, IDamageSource source, RaycastHit hit)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage(m_TargetIndex, d);
		}

		public void AddHealth (float h)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, -h);
		}

		public void AddHealth (float h, IDamageSource source)
		{
			if (m_Tracker != null)
				m_Tracker.AddDamage (m_TargetIndex, -h);
		}

		#endregion
	}
}