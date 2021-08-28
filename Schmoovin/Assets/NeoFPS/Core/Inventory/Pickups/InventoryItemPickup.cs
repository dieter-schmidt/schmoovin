using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inventoryref-mb-inventoryitempickup.html")]
	[RequireComponent (typeof (AudioSource))]
	public class InventoryItemPickup : Pickup, INeoSerializableComponent
    {
        [SerializeField, Tooltip("What to do to the pickup object once its item has been transferred to the character inventory.")]
        private PickupConsumeResult m_ConsumeResult = PickupConsumeResult.PoolOrDestroy;

		[SerializeField, Tooltip("The inventory item prefab to give to the character.")]
		private FpsInventoryItemBase m_ItemPrefab = null;

        [SerializeField, Tooltip("Should the pickup be spawned immediately, or triggered externally.")]
		private bool m_SpawnOnAwake = true;

		[SerializeField, Tooltip("How long to wait before respawning if the consume result is set to \"Respawn\".")]
        private float m_RespawnDuration = 20f;

		[SerializeField, Tooltip("The display mesh of the pickup. This should not be the same game object as this, so that if this is disabled the pickup will still respawn if required.")]
		private GameObject m_DisplayMesh = null;

        private static readonly NeoSerializationKey k_RespawnKey = new NeoSerializationKey("respawn");

        public enum PickupConsumeResult
		{
			PoolOrDestroy,
			Disable,
			Respawn
		}

        private NeoSerializedGameObject m_Nsgo = null;
        private AudioSource m_AudioSource = null;
        private Collider m_Collider = null;
        private IEnumerator m_DelayedSpawnCoroutine = null;
        private float m_RespawnTimer = 0f;

        public FpsInventoryItemBase item
        {
            get;
            private set;
        }

#if UNITY_EDITOR
        void OnValidate ()
        {
            m_RespawnDuration = Mathf.Clamp(m_RespawnDuration, 0.5f, 300f);

            // Get the display mesh object
            if (m_DisplayMesh == null)
            {
                var mesh = GetComponentInChildren<MeshRenderer>(true);
                if (mesh != null && mesh.gameObject != gameObject)
                    m_DisplayMesh = mesh.gameObject;
            }
        }
#endif

        void Awake ()
		{
			m_Collider = GetComponent<Collider> ();
			m_AudioSource = GetComponent<AudioSource> ();
            m_Nsgo = GetComponent<NeoSerializedGameObject>();
		}

        void Start()
        {
            if (!m_SpawnOnAwake || !SpawnItem())
                EnablePickup(false);
        }

		public override void Trigger (ICharacter character)
		{
			base.Trigger (character);
			if (item != null)
			{
				IInventory inventory = character.inventory;
				switch (inventory.AddItem (item))
				{
					case InventoryAddResult.Full:
						OnPickedUp ();
						break;
                    case InventoryAddResult.AppendedFull:
                        OnPickedUp();
                        break;
                    case InventoryAddResult.Partial:
						OnPickedUpPartial ();
						break;
				}
			}
		}

		protected virtual void OnPickedUp ()
		{
			if (m_DelayedSpawnCoroutine != null)
				StopCoroutine (m_DelayedSpawnCoroutine);
            // NB: The item will have been moved into the inventory heirarchy
            switch (m_ConsumeResult)
            {
                case PickupConsumeResult.PoolOrDestroy:
                    if (m_AudioSource.clip != null)
                        AudioSource.PlayClipAtPoint(m_AudioSource.clip, transform.position);
                    // Return to pool if it's a pooled object, destroy or not
                    var pooled = GetComponent<PooledObject>();
                    if (pooled != null)
                        pooled.ReturnToPool();
                    else
                        Destroy(gameObject);
                    break;
                case PickupConsumeResult.Disable:
                    m_AudioSource.Play();
                    EnablePickup(false);
                    item = null;
                    break;
                case PickupConsumeResult.Respawn:
                    m_AudioSource.Play();
                    EnablePickup(false);
                    item = null;
                    m_RespawnTimer = m_RespawnDuration;
                    m_DelayedSpawnCoroutine = DelayedSpawn();
                    StartCoroutine(m_DelayedSpawnCoroutine);
                    break;
            }
		}

		protected virtual void OnPickedUpPartial ()
		{
			m_AudioSource.Play ();
		}

		public virtual void EnablePickup (bool value)
        {
			// Enable the mesh
			if (m_DisplayMesh != null)
				m_DisplayMesh.SetActive (value);

            // Enable the collider
            m_Collider.enabled = value;
        }

        public virtual bool SpawnItem ()
		{
			if (item == null)
			{
                // Instantiate
                if (m_Nsgo != null)
                    item = m_Nsgo.InstantiatePrefab(m_ItemPrefab, Vector3.zero, Quaternion.identity);
                else
                    item = Instantiate(m_ItemPrefab, Vector3.zero, Quaternion.identity, transform);
                
				// Disable object
				item.gameObject.SetActive (false);

				// Enable pickup
				EnablePickup (true);

				return true;
			}
			return false;
		}

		IEnumerator DelayedSpawn ()
        {
            while (m_RespawnTimer > 0f)
            {
                yield return null;
                m_RespawnTimer -= Time.deltaTime;
            }
            SpawnItem ();
			m_DelayedSpawnCoroutine = null;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Write if respawning
            if (m_DelayedSpawnCoroutine != null)
                writer.WriteValue(k_RespawnKey, m_RespawnTimer);
            
            // Item quantity is handled by the item itself
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Respawn timer (start the coroutine if the property is found)
            float respawn = 0f;
            if (reader.TryReadValue(k_RespawnKey, out respawn, 0f))
            {
                m_RespawnTimer = respawn;
                m_DelayedSpawnCoroutine = DelayedSpawn();
                StartCoroutine(m_DelayedSpawnCoroutine);
            }

            // Item quantity is handled by the item itself
        }
    }
}