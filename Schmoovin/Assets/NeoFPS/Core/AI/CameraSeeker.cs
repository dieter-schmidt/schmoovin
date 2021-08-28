using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/samplesref-mb-cameraseeker.html")]
    public class CameraSeeker : AiSeeker, IHealthManager
	{
		[Header ("Transforms")]
		[SerializeField, Tooltip("The transform used to detect look angles (should be the last in the chain, eg. camera body).")]
        private Transform m_LookTransform = null;
		[SerializeField, Tooltip("The transform used to rotate on the horizontal axis.")]
		private Transform m_HorizontalServoTransform = null;
		[SerializeField, Tooltip("The transform used to rotate on the vertical axis.")]
		private Transform m_VerticalServoTransform = null;

		[Header ("Idle")]
		[SerializeField, Tooltip("The camera rotation speed when idle (degrees per second).")]
		private float m_RotationSpeedIdle = 45f;
		[SerializeField, Tooltip("The length of the pause at each extreme of rotation while idle.")]
		private float m_PauseIdle = 2f;
		[SerializeField, Tooltip("The rotation points for the camera when idling.")]
		private Vector2[] m_IdleRotations = new Vector2[]
		{
			new Vector2 (-30f, -5f),
			new Vector2 (30f, -5f)
		};

		[Header ("Hostile")]
		[SerializeField, Tooltip("The camera rotation speed when hostile (degrees per second).")]
		private float m_RotationSpeedHostile = 90f;
		[SerializeField, Tooltip("The length of the pause at each extreme of rotation when hostile (degrees per second).")]
		private float m_PauseHostile = 0.5f;
		[SerializeField, Tooltip("The duration the camera will be suspicious before engaging.")]
		private float m_SuspiciousTime = 2f;
		[SerializeField, Tooltip("The duration the camera will stay in hunting mode before going idle.")]
		private float m_HuntingTime = 10f;

		[Header ("Ranges")]
		[SerializeField, Tooltip("The maximum range of the camera in meters.")]
		private float m_DetectionRange = 10f;
		[SerializeField, Tooltip("The minimum angles the camera can reach (negative y is down).")]
		private Vector2 m_MinAngles = new Vector2 (-90, -45);
		[SerializeField, Tooltip("The maximum angles the camera can reach (negative y is down).")]
		private Vector2 m_MaxAngles = new Vector2 (90, 45);

		[Header ("States")]
		[SerializeField, Tooltip("The halo glow for the camera.")]
		private Light m_CameraLight = null;
		[SerializeField, Tooltip("The halo colour for the idle state.")]
		private Color m_ColourIdle = Color.green;
		[SerializeField, Tooltip("The halo colour for the suspicious state.")]
		private Color m_ColourSuspicious = Color.yellow;
		[SerializeField, Tooltip("The halo colour for the engaged state.")]
		private Color m_ColourEngaged = Color.red;

        [Header("Events")]
        [SerializeField, Tooltip("An event invoked when the seeker enters the idle state.")]
        private UnityEvent m_OnIdle = null;
        [SerializeField, Tooltip("An event invoked when the seeker enters the suspicious state.")]
        private UnityEvent m_OnSuspicious = null;
        [SerializeField, Tooltip("An event invoked when the seeker enters the engaged state.")]
        private UnityEvent m_OnEngaged = null;
        [SerializeField, Tooltip("An event invoked when the seeker enters the hunting state.")]
        private UnityEvent m_OnHunting = null;
        [SerializeField, Tooltip("An event invoked when the seeker is killed.")]
        private UnityEvent m_OnKilled = null;

        private static readonly NeoSerializationKey k_IdleTargetKey = new NeoSerializationKey("idleTarget");
        private static readonly NeoSerializationKey k_PauseKey = new NeoSerializationKey("pause");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");
        private static readonly NeoSerializationKey k_CurrentRotKey = new NeoSerializationKey("currentRot");
        private static readonly NeoSerializationKey k_TargetRotKey = new NeoSerializationKey("targetRot");
        private static readonly NeoSerializationKey k_InDamageFilterKey = new NeoSerializationKey("inDamageFilter");
        private static readonly NeoSerializationKey k_HealthMaxKey = new NeoSerializationKey("healthMax");
        private static readonly NeoSerializationKey k_HealthKey = new NeoSerializationKey("health");

        private struct SeekerTarget
		{
			public Transform transform;
			public float sqrDistance;

			public SeekerTarget (Transform t, float sqrD)
			{
				transform = t;
				sqrDistance = sqrD;
			}
		}

        Vector2 m_TargetRotation = Vector2.zero;
        private int m_CurrentIdleTarget = 0;
		private float m_Pause = 0f;
        private float m_Timer = 0f;

		private Vector3 m_PivotOffset = Vector3.zero;
		private Vector3[] m_FustrumNormals = null;
	
		private RaycastHit m_RaycastHit = new RaycastHit();
		private Collider[] m_ColliderBuffer = new Collider[32];
		private List<SeekerTarget> m_SortedTargets = new List<SeekerTarget> (32);
		private WaitForFixedUpdate m_FixedUpdateYield = new WaitForFixedUpdate ();

		public Transform currentTarget
		{
			get
			{
				if (m_SortedTargets.Count == 0)
					return null;
				return m_SortedTargets [0].transform;
			}
		}

		private Vector2 m_CurrentRotation = Vector2.zero;
		public Vector2 currentRotation
		{
			get { return m_CurrentRotation; }
			set
			{
				value.x = Mathf.Clamp (value.x, m_MinAngles.x, m_MaxAngles.x);
				value.y = Mathf.Clamp (value.y, m_MinAngles.y, m_MaxAngles.y);
				m_CurrentRotation = value;
				m_HorizontalServoTransform.localRotation = Quaternion.Euler (0f, m_CurrentRotation.x, 0f);
				m_VerticalServoTransform.localRotation = Quaternion.Euler (-m_CurrentRotation.y, 0f, 0f);
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();

            // Check values
            m_RotationSpeedIdle = Mathf.Clamp(m_RotationSpeedIdle, 1f, 90f);
            m_RotationSpeedHostile = Mathf.Clamp(m_RotationSpeedHostile, 1f, 90f);
            if (m_PauseIdle < 0f)
                m_PauseIdle = 0f;
            if (m_PauseHostile < 0f)
                m_PauseHostile = 0f;
            if (m_SuspiciousTime < 0f)
                m_SuspiciousTime = 0f;
            if (m_HuntingTime < 1f)
                m_HuntingTime = 1f;
            // Check angle limits
            if (m_MinAngles.x > m_MaxAngles.x)
                m_MinAngles.x = m_MaxAngles.x;
            if (m_MinAngles.y > m_MaxAngles.y)
                m_MinAngles.y = m_MaxAngles.y;
            if (m_MaxAngles.x < m_MinAngles.x)
                m_MaxAngles.x = m_MinAngles.x;
            if (m_MaxAngles.y < m_MinAngles.y)
                m_MaxAngles.y = m_MinAngles.y;
            // Check range minimum
            if (m_DetectionRange < 1f)
				m_DetectionRange = 1f;
            // Check the idle rotations
            if (m_IdleRotations == null || m_IdleRotations.Length == 0)
				m_IdleRotations = new Vector2[] { Vector2.zero };
			// Check light
			if (m_CameraLight == null)
				m_CameraLight = GetComponentInChildren<Light> ();
			// Check health
			if (m_StartingHealth <= 0f)
				m_StartingHealth = 1f;
		}
		#endif

		protected override void Start ()
		{
			currentRotation = m_IdleRotations [0];

			// Clamp idle rotations
			for (int i = 0; i < m_IdleRotations.Length; ++i)
			{
				Vector2 r = m_IdleRotations[i];
				r.x = Mathf.Clamp(r.x, m_MinAngles.x, m_MaxAngles.x);
				r.y = Mathf.Clamp(r.y, m_MinAngles.y, m_MaxAngles.y);
				m_IdleRotations[i] = r;
			}

			// Calculate fustrum normals (planes of camera view, pointing in toward center)
			m_FustrumNormals = new Vector3[4];
			m_FustrumNormals [0] = Quaternion.Euler (0f, m_MinAngles.x, 0f) * Vector3.right;
			m_FustrumNormals [1] = Quaternion.Euler (0f, m_MaxAngles.x, 0f) * Vector3.left;
			m_FustrumNormals [2] = Quaternion.Euler (-m_MinAngles.y, 0f, 0f) * Vector3.up;
			m_FustrumNormals [3] = Quaternion.Euler (-m_MaxAngles.y, 0f, 0f) * Vector3.down;

			m_PivotOffset = transform.InverseTransformPoint (m_LookTransform.position);

			base.Start ();

			inDamageFilter = DamageFilter.AllDamageAllTeams;
			healthMax = m_StartingHealth;
			health = m_StartingHealth;
		}

        protected override void OnStateChanged(State from, State to)
        {
            base.OnStateChanged(from, to);
            
            switch(to)
            {
                case State.Idle:
                    m_OnIdle.Invoke();
                    break;
                case State.Suspicious:
                    m_OnSuspicious.Invoke();
                    break;
                case State.Engaged:
                    m_OnEngaged.Invoke();
                    break;
                case State.Hunting:
                    m_OnHunting.Invoke();
                    break;
                case State.Dead:
                    m_OnKilled.Invoke();
                    break;
            }
        }
        
        protected override IEnumerator IdleCoroutine ()
		{
			m_CameraLight.color = m_ColourIdle;
			m_CurrentIdleTarget = 0;
			m_Pause = 0f;

			// Moving is used to skip calculations with only 1 idle rotation target
			bool moving = true;

			while (true)
			{
				yield return m_FixedUpdateYield;

				if (CheckVisibleColliders ())
					state = State.Suspicious;
				else
				{
					if (moving)
					{
						if (m_Pause > 0f)
						{
							m_Pause -= Time.deltaTime;
							if (m_Pause < 0f)
								m_Pause = 0f;
						}
						else
						{
							currentRotation = Vector2.MoveTowards (m_CurrentRotation, m_IdleRotations [m_CurrentIdleTarget], m_RotationSpeedIdle * Time.deltaTime);

							if (Mathf.Approximately (m_CurrentRotation.x, m_IdleRotations [m_CurrentIdleTarget].x) &&
							   Mathf.Approximately (m_CurrentRotation.y, m_IdleRotations [m_CurrentIdleTarget].y))
							{
								if (m_IdleRotations.Length == 1)
									moving = false;
								else
								{
									m_Pause = m_PauseIdle;

									// Move to next target (could have setting for repeat)
									++m_CurrentIdleTarget;
									if (m_CurrentIdleTarget == m_IdleRotations.Length)
										m_CurrentIdleTarget = 0;
								}
							}
						}
					}
				}
			}
		}

		protected override IEnumerator SuspiciousCoroutine ()
		{
			m_CameraLight.color = m_ColourSuspicious;
			m_Pause = 0f;

			m_Timer = m_SuspiciousTime;

			while (true)
			{
				yield return m_FixedUpdateYield;

				if (!CheckVisibleColliders ())
					state = State.Idle;
				else
				{
					m_Timer -= Time.deltaTime;
					if (m_Timer <= 0f)
						state = State.Engaged;
					else
					{
						currentRotation = Vector2.MoveTowards (m_CurrentRotation, GetRotationToTarget (), m_RotationSpeedHostile * Time.deltaTime);
					}
				}
			}
		}

		protected override IEnumerator EngagedCoroutine ()
		{
			m_CameraLight.color = m_ColourEngaged;
			m_Pause = 0f;

			while (true)
			{
				yield return m_FixedUpdateYield;

				if (!CheckVisibleColliders ())
					state = State.Hunting;
				else
				{
					currentRotation = Vector2.MoveTowards (m_CurrentRotation, GetRotationToTarget (), m_RotationSpeedHostile * Time.deltaTime);
				}
			}
		}

		protected override IEnumerator HuntingCoroutine ()
		{
			m_CameraLight.color = m_ColourSuspicious;
			m_TargetRotation = GetRandomRotation ();
			m_Pause = 0f;
			m_Timer = m_HuntingTime;

			while (true)
			{
				yield return m_FixedUpdateYield;

				if (CheckVisibleColliders ())
					state = State.Engaged;
				else
				{
					m_Timer -= Time.deltaTime;
					if (m_Timer <= 0f)
						state = State.Idle;
					else
					{
						if (m_Pause > 0f)
						{
							m_Pause -= Time.deltaTime;
							if (m_Pause < 0f)
								m_Pause = 0f;
						}
						else
						{
							currentRotation = Vector2.MoveTowards (m_CurrentRotation, m_TargetRotation, m_RotationSpeedHostile * Time.deltaTime);

							if (Mathf.Approximately (m_CurrentRotation.x, m_TargetRotation.x) &&
							   Mathf.Approximately (m_CurrentRotation.y, m_TargetRotation.y))
							{
								m_Pause = m_PauseHostile;
								m_TargetRotation = GetRandomRotation ();
							}
						}
					}
				}
			}
		}

		protected override IEnumerator DeadCoroutine ()
		{
			// Switch off light
			m_CameraLight.gameObject.SetActive (false);

			// Drop camera
			Vector2 deadRotation = currentRotation;
			deadRotation.y = m_MinAngles.y;
			while (!Mathf.Approximately (m_CurrentRotation.y, deadRotation.y))
			{
				yield return m_FixedUpdateYield;
				currentRotation = Vector2.MoveTowards (m_CurrentRotation, deadRotation, m_RotationSpeedHostile * Time.deltaTime);
			}

			// Disable
			enabled = false;
		}

		Vector2 GetRandomRotation ()
		{
			return new Vector2 (
				UnityEngine.Random.Range (m_MinAngles.x, m_MaxAngles.x),
				UnityEngine.Random.Range (m_MinAngles.y, m_MaxAngles.y)
			);
		}

		bool CheckVisibleColliders ()
		{
			m_SortedTargets.Clear ();

			Vector3 p = m_LookTransform.position;
			Quaternion q = m_LookTransform.rotation;

			int numVisibleColliders = Physics.OverlapSphereNonAlloc (p, m_DetectionRange, m_ColliderBuffer, PhysicsFilter.LayerFilter.AiVisibility);
			if (numVisibleColliders == 0)
				return false;

			// Check fustrum
			int discarded = 0;
			for (int i = 0; i < numVisibleColliders; ++i)
			{
				Vector3 diff = m_ColliderBuffer [i].transform.position - p;
				if (Vector3.Dot (q * m_FustrumNormals[0], diff) < 0f)
				{
					m_ColliderBuffer [i] = null;
					++discarded;
					continue;
				}
				if (Vector3.Dot (q * m_FustrumNormals[1], diff) < 0f)
				{
					m_ColliderBuffer [i] = null;
					++discarded;
					continue;
				}
				if (Vector3.Dot (q * m_FustrumNormals[2], diff) < 0f)
				{
					m_ColliderBuffer [i] = null;
					++discarded;
					continue;
				}
				if (Vector3.Dot (q * m_FustrumNormals[3], diff) < 0f)
				{
					m_ColliderBuffer [i] = null;
					++discarded;
					continue;
				}

				// Check for raycast hits
				bool hit = PhysicsExtensions.RaycastNonAllocSingle (
					           new Ray (p, diff),
					           out m_RaycastHit,
					           m_DetectionRange,
					           PhysicsFilter.Masks.AiVisibilityCheck
				           );
				if (hit && m_RaycastHit.collider.gameObject.layer == PhysicsFilter.LayerIndex.AiVisibility)
					m_SortedTargets.Add (new SeekerTarget (m_ColliderBuffer [i].transform, diff.sqrMagnitude));
				else
				{
					m_ColliderBuffer [i] = null;
					++discarded;
					continue;
				}
			}

			// False if none visible, sort if not
			if (m_SortedTargets.Count == 0)
				return false;
			else
			{
				// Sort targets based on distance (anything else?)
				m_SortedTargets.Sort (
					(SeekerTarget x, SeekerTarget y) =>
					{
						int nameCompare = x.transform.name.CompareTo (y.transform.name);
						if (nameCompare == 0)
							return x.sqrDistance.CompareTo (y.sqrDistance);
						else
							return nameCompare;
					}
				);
			}

			return true;
		}

		Vector2 GetRotationToTarget ()
		{
			Transform targetTransform = m_SortedTargets [0].transform;

			Vector3 relativePosition = transform.InverseTransformPoint (targetTransform.position) - m_PivotOffset;
			Vector3 euler = Quaternion.FromToRotation (Vector3.forward, relativePosition).eulerAngles;
			if (euler.x > 180f)
				euler.x -= 360f;
			if (euler.y > 180f)
				euler.y -= 360f;

			return new Vector2 (
				Mathf.Clamp (euler.y, m_MinAngles.x, m_MaxAngles.x),
				Mathf.Clamp (-euler.x, m_MinAngles.y, m_MaxAngles.y)
			);
		}

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            // Write state coroutine variables
            writer.WriteValue(k_IdleTargetKey, m_CurrentIdleTarget);
            writer.WriteValue(k_PauseKey, m_Pause);
            writer.WriteValue(k_TimerKey, m_Timer);
            writer.WriteValue(k_CurrentRotKey, m_CurrentRotation);
            writer.WriteValue(k_TargetRotKey, m_TargetRotation);

            // Write damage filter
            if (m_InDamageFilter != DamageFilter.AllDamageAllTeams)
                writer.WriteValue(k_InDamageFilterKey, m_InDamageFilter);

            // Write health
            writer.WriteValue(k_HealthMaxKey, healthMax);
            writer.WriteValue(k_HealthKey, health);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            // Read state coroutine variables
            reader.TryReadValue(k_IdleTargetKey, out m_CurrentIdleTarget, m_CurrentIdleTarget);
            reader.TryReadValue(k_PauseKey, out m_Pause, m_Pause);
            reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
            reader.TryReadValue(k_CurrentRotKey, out m_CurrentRotation, m_CurrentRotation);
            reader.TryReadValue(k_TargetRotKey, out m_TargetRotation, m_TargetRotation);

            // Read damage filter
            int intResult = 0;
            if (reader.TryReadValue(k_InDamageFilterKey, out intResult, 0))
                inDamageFilter = (DamageFilter)intResult;

            // Read health
            float floatResult = 0f;
            if (reader.TryReadValue(k_HealthMaxKey, out floatResult, m_HealthMax))
                healthMax = floatResult;
            if (reader.TryReadValue(k_HealthKey, out floatResult, m_Health))
                health = floatResult;
        }

        #region IHealthManager implementation

        [SerializeField] private float m_StartingHealth = 50f;
        
#pragma warning disable 0067

        public event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        public event HealthDelegates.OnHealthChanged onHealthChanged;
        public event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

#pragma warning restore 0067

        private DamageFilter m_InDamageFilter = DamageFilter.AllDamageAllTeams;
		public DamageFilter inDamageFilter 
		{
			get { return m_InDamageFilter; }
			set
			{
				m_InDamageFilter = value;
				IDamageHandler[] damageHandlers = GetComponentsInChildren<IDamageHandler>();
				for (int i = 0; i < damageHandlers.Length; ++i)
					damageHandlers[i].inDamageFilter = m_InDamageFilter;
			}
		}

		public void AddDamage (float damage)
		{
			AddDamage (damage, false, null);
		}

		public void AddDamage (float damage, bool critical)
		{
			AddDamage (damage, critical, null);
		}

		public void AddDamage (float damage, IDamageSource source)
		{
			AddDamage (damage, false, source);
		}

		public void AddDamage (float damage, bool critical, IDamageSource source)
		{
			if (health > 0f)
				health -= damage;
		}

		public void AddDamage(float damage, bool critical, RaycastHit hit)
		{
			AddDamage(damage, critical, null);
		}

		public void AddDamage(float damage, bool critical, IDamageSource source, RaycastHit hit)
		{
			AddDamage(damage, critical, source);
		}

		public void AddHealth (float h)
		{
			AddHealth (h, null);
		}

		public void AddHealth (float h, IDamageSource source)
		{
			health += h;
		}

		public bool isAlive
		{
			get;
			private set;
		}

		private float m_Health = 100f;
		public float health
		{
			get { return m_Health; }
			set
			{
				m_Health = Mathf.Clamp (value, 0f, m_HealthMax);
				if (Mathf.Approximately (m_Health, 0f))
					state = State.Dead;
			}
		}

		private float m_HealthMax = 100f;
		public float healthMax
		{
			get { return m_HealthMax; }
			set
			{
				m_HealthMax = value;
				if (m_HealthMax < 0f)
					m_HealthMax = 0f;
				if (health > m_HealthMax)
					health = m_HealthMax;
			}
        }

        public float normalisedHealth
        {
            get { return health / healthMax; }
            set { health = value * healthMax; }
        }

        #endregion
    }
}