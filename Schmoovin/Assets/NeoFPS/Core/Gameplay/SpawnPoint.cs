using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoCC;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcharactersref-mb-spawnpoint.html")]
	public class SpawnPoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the spawn point be registered immediately on awake? It can be hard to enforce an order of registration for multiple spawn points with this.")]
        private bool m_RegisterOnAwake = true;

        [SerializeField, Tooltip("How long before the spawn point can be used again.")]
        private float m_ReuseDelay = 0f;

        [SerializeField, Tooltip("The collider volume type for checking if the spawn point is clear or overlapped by another object.")]
		private OverlapTest m_OverlapTest = OverlapTest.Box;

		[SerializeField, Tooltip("The vertical height of the bounding volume for overlap checks.")]
		private float m_BoundsHeight = 2.5f;

		[SerializeField, Tooltip("The horizontal dimension of the bounding volume for overlap checks.")]
		private float m_BoundsHorizontal = 1.2f;

        [SerializeField, Tooltip("Should the character's gravity be reoriented to match the spawn point. If the spawn is tilted on one side, this will make the character's down direction equal to the spawn point's")]
        private bool m_ReorientGravity = false;

        [SerializeField, Tooltip("A UnityEvent fired when a character is spawned at this point. Allows for simple triggering of spawn audio and visual effects.")]
		private UnityEvent m_OnSpawn = new UnityEvent();

#if UNITY_EDITOR
        public OrderedSpawnPointGroup group = null;
#endif

        private Coroutine m_CooldownCoroutine = null;
		private WaitForSeconds m_ReuseYield = null;

		public enum OverlapTest
		{
			Box,
			Capsule,
			None
		}

		private Transform m_SpawnTransform = null;
		public Transform spawnTransform
		{
			get
			{ 
				if (m_SpawnTransform == null)
					m_SpawnTransform = transform;
				return m_SpawnTransform;
			}
		}
        
		public event UnityAction onSpawn
		{
			add { m_OnSpawn.AddListener (value); }
			remove { m_OnSpawn.RemoveListener (value); }
		}

		public bool cooldownActive
		{
			get { return m_CooldownCoroutine != null; }
		}

		private bool m_Registered = false;
		public bool registered
		{
			get { return m_Registered; }
		}

#if UNITY_EDITOR
        void OnValidate()
        {
            m_BoundsHeight = Mathf.Clamp(m_BoundsHeight, 1f, 5f);
            m_BoundsHorizontal = Mathf.Clamp(m_BoundsHorizontal, 0.5f, 5f);
            if (m_ReuseDelay < 0f)
                m_ReuseDelay = 0f;

            // Check group settings
            if (group != null)
            {

                bool found = false;
                for (int i = 0; i < group.spawnPoints.Length; ++i)
                {
                    if (group.spawnPoints[i] == this)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    m_RegisterOnAwake = false;
                else
                    group = null;
            }
        }
#endif

        void Awake ()
		{
            if (m_RegisterOnAwake)
                Register();
			m_ReuseYield = new WaitForSeconds (m_ReuseDelay);
		}

		void OnEnable ()
		{
            if (m_RegisterOnAwake)
                Register();
		}

		void OnDisable ()
		{
            Unregister();
			m_CooldownCoroutine = null;
		}

        public void Register()
        {
            if (!m_Registered)
            {
                SpawnManager.AddSpawnPoint(this);
                m_Registered = true;
            }
        }

        public void Unregister()
        {
            if (m_Registered)
            {
                SpawnManager.RemoveSpawnPoint(this);
                m_Registered = false;
            }
        }

        public virtual bool CanSpawnCharacter()
        {
            // Check timeout
            if (m_CooldownCoroutine != null)
                return false;

            // Check overlap
            switch (m_OverlapTest)
            {
                case OverlapTest.Box:
                    {
                        float halfX = m_BoundsHorizontal * 0.5f;
                        float halfY = m_BoundsHeight * 0.5f;
                        if (Physics.CheckBox(
                            spawnTransform.position + (Vector3.up * halfY),
                            new Vector3(halfX, halfX, halfY),
                            spawnTransform.rotation,
                            PhysicsFilter.Masks.SpawnBlockers,
                            QueryTriggerInteraction.Ignore
                        ))
                            return false;
                    }
                    break;
                case OverlapTest.Capsule:
                    {
                        float radius = m_BoundsHorizontal * 0.5f;
                        float top = m_BoundsHeight * -radius;
                        Vector3 position = spawnTransform.position;
                        if (Physics.CheckCapsule(
                            position + (Vector3.up * radius),
                            position + (Vector3.up * top),
                            radius,
                            PhysicsFilter.Masks.SpawnBlockers,
                            QueryTriggerInteraction.Ignore
                        ))
                            return false;
                    }
                    break;
            }
            return true;
        }

		public virtual ICharacter SpawnCharacter (ICharacter characterPrototype, IController player, bool force = false, NeoSerializedScene scene = null)
		{
            // Check if can spawn
            if (force)
            {
                if (m_CooldownCoroutine != null)
                {
                    StopCoroutine(m_CooldownCoroutine);
                    m_CooldownCoroutine = null;
                }
            }
            else
            {
                if (!CanSpawnCharacter())
                    return null;
            }

			// Get character
			ICharacter character = CreateCharacterFromPrototype (characterPrototype, scene);
			if (character == null)
				return null;

			// Place character
			Transform ct = character.gameObject.transform;
			ct.position = spawnTransform.position;
            if (m_ReorientGravity)
            {
                ct.rotation = spawnTransform.rotation;
                var neoCC = ct.GetComponent<INeoCharacterController>();
                if (neoCC != null)
                {
                    neoCC.characterGravity.gravity = spawnTransform.rotation * neoCC.gravity;
                    if (!neoCC.characterGravity.orientUpWithGravity)
                        neoCC.characterGravity.up = spawnTransform.rotation * neoCC.up;
                }
            }
            else
            {
                var spawnForward = Vector3.ProjectOnPlane(spawnTransform.forward, Vector3.up);
                if (spawnForward.sqrMagnitude < 0.0001f)
                    spawnForward = Vector3.forward;
                else
                    spawnForward.Normalize();

                ct.rotation = Quaternion.LookRotation(spawnForward);
            }

			// Set controller
			character.controller = player;

			// Fire event
			m_OnSpawn.Invoke ();

			// Start cooldown
			if (m_ReuseDelay > 0f)
				m_CooldownCoroutine = StartCoroutine (Cooldown ());

			return character;
		}

		protected virtual ICharacter CreateCharacterFromPrototype (ICharacter prototype, NeoSerializedScene scene = null)
		{
			ICharacter result = null;

			MonoBehaviour behaviour = prototype as MonoBehaviour;
			if (behaviour != null)
			{
                GameObject go = null;
                if (scene != null)
                    go = scene.InstantiatePrefab(behaviour).gameObject;
                else
                    go = Instantiate(behaviour.gameObject);
                //go.SetActive(true);
				result = go.GetComponent<ICharacter>();
			}

			return result;
		}

		IEnumerator Cooldown ()
		{
			yield return m_ReuseYield;
			m_CooldownCoroutine = null;
		}

        private void OnDrawGizmos()
        {
            ExtendedGizmos.DrawArrowMarker2D(transform.position, transform.rotation.eulerAngles.y, 1f, Color.red);

            switch(m_OverlapTest)
            {
                case OverlapTest.Box:
                    ExtendedGizmos.DrawCuboidMarker(transform.position, m_BoundsHorizontal, m_BoundsHeight, transform.rotation, Color.red);
                    break;
                case OverlapTest.Capsule:
                    ExtendedGizmos.DrawCapsuleMarker(m_BoundsHorizontal * 0.5f, m_BoundsHeight, transform.position + (Vector3.up * m_BoundsHeight * 0.5f), Color.red);
                    break;
            }
        }
    }
}