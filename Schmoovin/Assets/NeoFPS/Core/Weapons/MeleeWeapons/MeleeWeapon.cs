using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-meleeweapon.html")]
    [RequireComponent(typeof(AudioSource))]
	public class MeleeWeapon : MonoBehaviour, IMeleeWeapon, IWieldable, IDamageSource, ICrosshairDriver, IPoseHandler, INeoSerializableComponent
    {
        [SerializeField, NeoObjectInHierarchyField(true), Tooltip("The animator component of the weapon.")]
		private Animator m_Animator = null;

        [SerializeField, Tooltip("The damage the weapon does.")]
		private float m_Damage = 50f;

		[SerializeField, Tooltip("The force to impart on the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler on the hit object.")]
		private float m_ImpactForce = 15f;

		[SerializeField, Tooltip("The range that the melee weapon can reach.")]
		private float m_Range = 1f;

		[SerializeField, Tooltip("The delay from starting the attack to checking for an impact. Should be synced with the striking point in the animation.")]
		private float m_Delay = 0.6f;

		[SerializeField, Tooltip("The recovery time after a hit.")]
		private float m_RecoverTime = 1f;

        [SerializeField, Tooltip("The crosshair to show when the weapon is drawn.")]
        private FpsCrosshair m_Crosshair = FpsCrosshair.Default;

        [Header ("Animation Triggers")]

		[SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The animation trigger for the attack animation.")]
		private string m_TriggerAttack = "Attack";

		[SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The animation trigger for the attack hit animation.")]
		private string m_TriggerAttackHit = "AttackHit";

        [SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Bool), Tooltip("The animation bool parameter for the block animation."), FormerlySerializedAs("m_TriggerBlock")]
        private string m_BoolBlock = "Block";

        [SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The animation trigger for the raise animation.")]
        private string m_TriggerDraw = "Draw";

        [SerializeField, Tooltip("The time taken to lower the item on deselection.")]
        private float m_RaiseDuration = 0.5f;

        [SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The trigger for the weapon lower animation (blank = no animation).")]
        private string m_TriggerLower = string.Empty;

        [SerializeField, Tooltip("The time taken to lower the item on deselection.")]
        private float m_LowerDuration = 0f;

        [Header("Audio")]

		[SerializeField, Tooltip("The audio clip when raising the weapon.")]
        private AudioClip m_AudioSelect = null;

        [SerializeField, Tooltip("The audio clip when attacking.")]
        private AudioClip m_AudioAttack = null;

        [SerializeField, Tooltip("The audio clip when bringing the weapon into block position.")]
        private AudioClip m_AudioBlockRaise = null;

        [SerializeField, Tooltip("The audio clip when bringing the weapon out of block position.")]
        private AudioClip m_AudioBlockLower = null;

        private static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);

        private int m_AnimHashDraw = 0;
        private int m_AnimHashLower = 0;
        private int m_AnimHashBlock = 0;
		private int m_AnimHashAttack = 0;
		private int m_AnimHashAttackHit = 0;

        private DeselectionWaitable m_DeselectionWaitable = null;
        private RaycastHit m_Hit = new RaycastHit();
        private AudioSource m_AudioSource = null;
        private ICharacter m_Wielder = null;
        private float m_CooldownTimer = 0f;
        private float m_DelayTimer = 0f;

        public event UnityAction onAttack;
        public event UnityAction<bool> onBlockStateChange;
        public event UnityAction<ICharacter> onWielderChanged;

        public class DeselectionWaitable : Waitable
        {
            private float m_Duration = 0f;
            private float m_StartTime = 0f;

            public DeselectionWaitable(float duration)
            {
                m_Duration = duration;
            }

            public void ResetTimer()
            {
                m_StartTime = Time.time;
            }

            protected override bool CheckComplete()
            {
                return (Time.time - m_StartTime) > m_Duration;
            }
        }

        public enum HitDetectType
		{
			Raycast,
			Spherecast,
			Custom
        }

        public ICharacter wielder
        {
            get { return m_Wielder; }
            private set
            {
                if (m_Wielder != value)
                {
                    m_Wielder = value;
                    if (onWielderChanged != null)
                        onWielderChanged(m_Wielder);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate ()
		{
			if (m_Animator == null)
				m_Animator = GetComponentInChildren<Animator> ();
            
            // Limit values
            if (m_Damage < 0f)
                m_Damage = 0f;
            if (m_ImpactForce < 0f)
                m_ImpactForce = 0f;
            if (m_Delay < 0f)
                m_Delay = 0f;
            if (m_RecoverTime < 0f)
                m_RecoverTime = 0f;
            m_Range = Mathf.Clamp(m_Range, 0.1f, 5f);
            m_RaiseDuration = Mathf.Clamp(m_RaiseDuration, 0f, 5f);
            m_LowerDuration = Mathf.Clamp(m_LowerDuration, 0f, 5f);
        }
		#endif

		void Awake ()
		{
			m_AnimHashDraw = Animator.StringToHash (m_TriggerDraw);
            m_AnimHashLower = Animator.StringToHash(m_TriggerLower);
            m_AnimHashBlock = Animator.StringToHash (m_BoolBlock);
			m_AnimHashAttack = Animator.StringToHash (m_TriggerAttack);
			m_AnimHashAttackHit = Animator.StringToHash (m_TriggerAttackHit);
            
            // Get the audio source
            m_AudioSource = GetComponent<AudioSource>();
            
            // Set up deselection waitable
            if (m_LowerDuration > 0.001f)
                m_DeselectionWaitable = new DeselectionWaitable(m_LowerDuration);

            // Set up pose handler
            m_PoseHandler = new PoseHandler(transform, Vector3.zero, Quaternion.identity);
        }

        protected virtual void Start()
        {
            if (wielder == null)
                Destroy(gameObject);
        }

        void OnEnable()
        {
            wielder = GetComponentInParent<ICharacter>();

            // Play draw audio
            if (m_AudioSelect != null)
                m_AudioSource.PlayOneShot(m_AudioSelect);

            // Trigger draw animation
            if (m_AnimHashDraw != -1 && m_Animator != null)
            {
                m_Animator.SetTrigger(m_AnimHashDraw);

                // Start cooldown to prevent input until raised
                if (m_RaiseDuration > 0f)
                    m_CooldownCoroutine = StartCoroutine(Cooldown(m_RaiseDuration));
            }
        }

        void OnDisable ()
		{
			blocking = false;

            // Clear coroutines
            if (m_CooldownCoroutine != null)
            {
                StopCoroutine(m_CooldownCoroutine);
                m_CooldownCoroutine = null;
            }
            if (m_DoRaycastCoroutine != null)
            {
                StopCoroutine(m_DoRaycastCoroutine);
                m_DoRaycastCoroutine = null;
            }

            // Reset pose
            m_PoseHandler.OnDisable();
        }

		public void Attack ()
		{
			if (m_CooldownCoroutine == null)
			{
				if (!blocking)
				{
					m_Animator.SetTrigger (m_AnimHashAttack);
					m_DoRaycastCoroutine = StartCoroutine (DoRaycast (m_Delay));
					m_CooldownCoroutine = StartCoroutine (Cooldown (m_RecoverTime));

                    if (m_AudioAttack != null)
                        m_AudioSource.PlayOneShot(m_AudioAttack);

                    if (onAttack != null)
                        onAttack();

                }
			}
        }

        public void Select()
        {
            // Play lower animation
            if (m_AnimHashDraw != -1 && m_Animator != null)
                m_Animator.SetTrigger(m_AnimHashDraw);
        }

        public void DeselectInstant()
        { }

        public Waitable Deselect()
        {
            // Play lower animation
            if (m_AnimHashLower != 0 && m_Animator != null)
                m_Animator.SetTrigger(m_AnimHashLower);

            // Wait for deselection
            if (m_DeselectionWaitable != null)
                m_DeselectionWaitable.ResetTimer();

            return m_DeselectionWaitable;
        }

        Coroutine m_CooldownCoroutine = null;
        IEnumerator Cooldown (float timer)
		{
            m_CooldownTimer = timer;
            while (m_CooldownTimer > 0f)
            {
                yield return null;
                m_CooldownTimer -= Time.deltaTime;
            }

			m_CooldownCoroutine = null;
		}

		Coroutine m_DoRaycastCoroutine = null;
        IEnumerator DoRaycast (float timer)
        {
            m_DelayTimer = timer;
            while (m_DelayTimer > 0f)
            {
                yield return null;
                m_DelayTimer -= Time.deltaTime;
            }

            Vector3 direction = transform.forward;

            // Get root game object to prevent impacts with body
            Transform ignoreRoot = null;
            if (wielder != null)
                ignoreRoot = wielder.gameObject.transform;

			if (PhysicsExtensions.RaycastNonAllocSingle (
				    new Ray (transform.position, direction),
					out m_Hit,
				    m_Range,
				    PhysicsFilter.Masks.BulletBlockers,
                    ignoreRoot
			))
			{
				// Show effect
				SurfaceManager.ShowBulletHit (m_Hit, direction, 1f, m_Hit.rigidbody != null);

				// Apply damage
				m_Hit.transform.GetComponents(s_DamageHandlers);
                for (int i = 0; i < s_DamageHandlers.Count; ++i)
                    s_DamageHandlers[i].AddDamage (m_Damage, m_Hit, this);
                s_DamageHandlers.Clear();

                // Apply force
                if (m_Hit.rigidbody != null)
					m_Hit.rigidbody.AddForceAtPosition (direction * m_ImpactForce, m_Hit.point, ForceMode.Impulse);
				else
				{
					IImpactHandler impactHandler = m_Hit.transform.GetComponent<IImpactHandler>();
					if (impactHandler != null)
						impactHandler.HandlePointImpact (m_Hit.point, direction * m_ImpactForce);
				}
				
				// Trigger hit animation
				m_Animator.SetTrigger (m_AnimHashAttackHit);
			}

			m_DoRaycastCoroutine = null;
		}

        private bool m_Blocking = false;
		public bool blocking
		{
			get { return m_Blocking; }
			set
			{
				if (m_Blocking != value)
				{
					m_Blocking = value;
					m_Animator.SetBool (m_AnimHashBlock, value);

                    if (m_Blocking)
                    {
                        if (m_AudioBlockRaise != null)
                            m_AudioSource.PlayOneShot(m_AudioBlockRaise);
                    }
                    else
                    {
                        if (m_AudioBlockLower != null)
                            m_AudioSource.PlayOneShot(m_AudioBlockLower);
                    }

                    if (onBlockStateChange != null)
                        onBlockStateChange(m_Blocking);
                }
			}
        }

        #region POSE

        private PoseHandler m_PoseHandler = null;

        public void SetPose(Vector3 position, Quaternion rotation, float duration)
        {
            m_PoseHandler.SetPose(position, rotation, duration);
        }

        public void SetPose(Vector3 position, CustomPositionInterpolation posInterp, Quaternion rotation, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.SetPose(position, posInterp, rotation, rotInterp, duration);
        }

        public void ResetPose(float duration)
        {
            m_PoseHandler.ResetPose(duration);
        }

        public void ResetPose(CustomPositionInterpolation posInterp, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.ResetPose(posInterp, rotInterp, duration);
        }

        void Update()
        {
            m_PoseHandler.UpdatePose();
        }

        #endregion

        #region IDamageSource implementation

        private DamageFilter m_OutDamageFilter = DamageFilter.AllDamageAllTeams;
        public DamageFilter outDamageFilter
        {
            get
            {
                return m_OutDamageFilter;
            }
            set
            {
                m_OutDamageFilter = value;
            }
        }

        public IController controller
        {
            get
            {
                if (wielder != null)
                    return wielder.controller;
                else
                    return null;
            }
        }

        public Transform damageSourceTransform
        {
            get
            {
                return transform;
            }
        }

        public string description
        {
            get
            {
                return name;
            }
        }

        #endregion

        #region ICrosshairDriver IMPLEMENTATION

        private bool m_HideCrosshair = false;

        public FpsCrosshair crosshair
        {
            get { return m_Crosshair; }
            //private set
            //{
            //    m_Crosshair = value;
            //    if (onCrosshairChanged != null)
            //        onCrosshairChanged(m_Crosshair);
            //}
        }

        private float m_Accuracy = 1f;
        public float accuracy
        {
            get { return m_Accuracy; }
            private set
            {
                m_Accuracy = value;
                if (onAccuracyChanged != null)
                    onAccuracyChanged(m_Accuracy);
            }
        }

        public event UnityAction<FpsCrosshair> onCrosshairChanged;
        public event UnityAction<float> onAccuracyChanged;

        public void HideCrosshair()
        {
            if (!m_HideCrosshair)
            {
                bool triggerEvent = (onCrosshairChanged != null && crosshair == FpsCrosshair.None);

                m_HideCrosshair = true;

                if (triggerEvent)
                    onCrosshairChanged(FpsCrosshair.None);
            }
        }

        public void ShowCrosshair()
        {
            if (m_HideCrosshair)
            {
                // Reset
                m_HideCrosshair = false;

                // Fire event
                if (onCrosshairChanged != null && crosshair != FpsCrosshair.None)
                    onCrosshairChanged(crosshair);
            }
        }

        #endregion

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_CooldownKey = new NeoSerializationKey("cooldown");
        private static readonly NeoSerializationKey k_DelayTimerKey = new NeoSerializationKey("delayTimer");
        private static readonly NeoSerializationKey k_BlockingKey = new NeoSerializationKey("blocking");
        private static readonly NeoSerializationKey k_AccuracyKey = new NeoSerializationKey("accuracy");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                // Write coroutines if relevant
                if (m_CooldownCoroutine != null)
                    writer.WriteValue(k_CooldownKey, m_CooldownTimer);
                if (m_DoRaycastCoroutine != null)
                    writer.WriteValue(k_DelayTimerKey, m_DelayTimer);

                // Write properties
                writer.WriteValue(k_BlockingKey, blocking);
                writer.WriteValue(k_AccuracyKey, accuracy);
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read properties
            float floatResult = 0f;
            if (reader.TryReadValue(k_AccuracyKey, out floatResult, 1f))
                accuracy = floatResult;

            bool boolResult = false;
            if (reader.TryReadValue(k_BlockingKey, out boolResult, false))
                blocking = boolResult;

            // Read and start coroutines if relevant
            if (reader.TryReadValue(k_DelayTimerKey, out floatResult, 0f))
                m_DoRaycastCoroutine = StartCoroutine(DoRaycast(floatResult));
            if (reader.TryReadValue(k_CooldownKey, out floatResult, 0f))
                m_CooldownCoroutine = StartCoroutine(Cooldown(floatResult));
        }

        #endregion
    }
}