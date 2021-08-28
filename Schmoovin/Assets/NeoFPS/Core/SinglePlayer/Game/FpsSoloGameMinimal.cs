using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoFPS.SinglePlayer
{
    [HelpURL("https://docs.neofps.com/manual/neofpsref-mb-fpssologameminimal.html")]
    public class FpsSoloGameMinimal : FpsGameMode
    {
        [SerializeField, Tooltip("Should the game mode automatically spawn a player character immediately on start.")]
        private bool m_SpawnOnStart = true;

        [SerializeField, Tooltip("How long after dying does the game react (gives time for visual feedback).")]
        private float m_DeathSequenceDuration = 5f;

        [SerializeField, Tooltip("What to do when the player character dies.")]
        private DeathAction m_DeathAction = DeathAction.Respawn;

        [SerializeField, NeoPrefabField(required = true), Tooltip("The player prefab to instantiate if none exists.")]
        private FpsSoloPlayerController m_PlayerPrefab = null;

        [SerializeField, NeoPrefabField(required = true), Tooltip("The character prefab to use.")]
        private FpsSoloCharacter m_CharacterPrefab = null;

        [SerializeField, Tooltip("An optional inventory loadout for the character on spawn (this will replace their starting items).")]
        private FpsInventoryLoadout m_StartingLoadout = null;

        private WaitForSeconds m_DeathSequenceYield = null;

        public enum DeathAction
        {
            Respawn,
            ReloadScene,
            MainMenu,
            ContinueFromSave
        }

        private IController m_Player = null;
        public IController player
        {
            get { return m_Player; }
            protected set
            {
                // Unsubscribe from old player events
                if (m_Player != null)
                    m_Player.onCharacterChanged -= OnPlayerCharacterChanged;

                // Set new player
                m_Player = value;

                // Track player for persistence
                var playerComponent = m_Player as Component;
                if (playerComponent != null)
                {
                    var nsgo = playerComponent.GetComponent<NeoSerializedGameObject>();
                    if (nsgo.wasRuntimeInstantiated)
                        m_PersistentObjects[0] = nsgo;
                    else
                        m_PersistentObjects[0] = null;
                }
                else
                    m_PersistentObjects[0] = null;

                // Subscribe to player events
                if (m_Player != null)
                {
                    m_Player.onCharacterChanged += OnPlayerCharacterChanged;
                    OnPlayerCharacterChanged(m_Player.currentCharacter);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_DeathSequenceDuration = Mathf.Clamp(m_DeathSequenceDuration, 0f, 300f);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            m_DeathSequenceYield = new WaitForSeconds(m_DeathSequenceDuration);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (player == null)
            {
                // Instantiate player if requried
                if (FpsSoloPlayerController.localPlayer != null)
                    player = FpsSoloPlayerController.localPlayer;
                else
                    player = InstantiatePlayer();
            }

            inGame = true;

            // Spawn player character
            if (player.currentCharacter == null)
            {
                if (m_SpawnOnStart)
                    Respawn(player);

                // Apply inventory loadout
                if (m_StartingLoadout != null)
                {
                    var inventory = player.currentCharacter.GetComponent<IInventory>();
                    inventory.ApplyLoadout(m_StartingLoadout);
                }
            }
            else
            {
                var spawn = SpawnManager.GetNextSpawnPoint(false);
                var t = player.currentCharacter.transform;
                t.position = spawn.spawnTransform.position;
                t.rotation = spawn.spawnTransform.rotation;
            }
        }

        protected override void OnDestroy()
        {
            if (m_PlayerCharacter != null)
            {
                m_PlayerCharacter.onIsAliveChanged -= OnPlayerCharacterIsAliveChanged;
                m_PlayerCharacter = null;
            }
            base.OnDestroy();
        }

        protected override IController InstantiatePlayer ()
		{
            var nsgo = GetComponent<NeoSerializedGameObject>();
            if (nsgo != null && nsgo.serializedScene != null)
                return nsgo.serializedScene.InstantiatePrefab(m_PlayerPrefab);
            else
                return Instantiate(m_PlayerPrefab);
		}

		protected override ICharacter GetPlayerCharacterPrototype (IController player)
		{
			return m_CharacterPrefab;
		}

        protected virtual void OnPlayerCharacterIsAliveChanged(ICharacter character, bool alive)
        {
            if (inGame && !alive)
            {
                IController player = character.controller;
                StartCoroutine(DelayedDeathReactionCoroutine(player));
            }
        }

        private IEnumerator DelayedDeathReactionCoroutine(IController player)
        {
            // Wait for death sequence to complete
            if (m_DeathSequenceDuration > 0f)
                yield return m_DeathSequenceYield;
            else
                yield return null;

            // Respawn
            if (inGame)
            {
                switch (m_DeathAction)
                {
                    case DeathAction.Respawn:
                        Respawn(player);
                        break;
                    case DeathAction.ReloadScene:
                        SceneManager.LoadScene(gameObject.scene.name);
                        break;
                    case DeathAction.MainMenu:
                        SceneManager.LoadScene(0);
                        break;
                    case DeathAction.ContinueFromSave:
                        if (SaveGameManager.canContinue)
                            SaveGameManager.Continue();
                        else
                            SceneManager.LoadScene(0);
                        break;
                }
            }
        }

        protected override void ProcessOldPlayerCharacter(ICharacter oldCharacter)
        {
            if (oldCharacter != null)
                Destroy(oldCharacter.gameObject);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
        }

        #region PLAYER

        private ICharacter m_PlayerCharacter = null;
        public ICharacter playerCharacter
        {
            get { return m_PlayerCharacter; }
        }

        protected virtual void OnPlayerCharacterChanged(ICharacter character)
        {
            // Unsubscribe from old character events
            if (m_PlayerCharacter != null)
            {
                m_PlayerCharacter.onIsAliveChanged -= OnPlayerCharacterIsAliveChanged;
                ProcessOldPlayerCharacter(m_PlayerCharacter as FpsSoloCharacter);
            }

            // Set new character
            m_PlayerCharacter = character;

            // Track character for persistence
            var characterComponent = m_PlayerCharacter as Component;
            if (characterComponent != null)
            {
                var nsgo = characterComponent.GetComponent<NeoSerializedGameObject>();
                if (nsgo.wasRuntimeInstantiated)
                    m_PersistentObjects[1] = nsgo;
                else
                    m_PersistentObjects[1] = null;
            }
            else
                m_PersistentObjects[1] = null;

            // Subscribe to character events
            if (m_PlayerCharacter != null)
            {
                m_PlayerCharacter.onIsAliveChanged += OnPlayerCharacterIsAliveChanged;
                OnPlayerCharacterIsAliveChanged(m_PlayerCharacter, m_PlayerCharacter.isAlive);
            }
        }

        #endregion

        #region PERSISTENCE

        private NeoSerializedGameObject[] m_PersistentObjects = new NeoSerializedGameObject[2];

        protected override NeoSerializedGameObject[] GetPersistentObjects()
        {
            if (m_PersistentObjects[0] != null && m_PersistentObjects[1] != null)
                return m_PersistentObjects;
            else
            {
                Debug.Log("No Persistence Save Objects. Does the scene have a SceneSaveInfo object correctly set up?");
                Debug.Log("m_PersistentObjects[0] != null: " + (m_PersistentObjects[0] != null));
                Debug.Log("m_PersistentObjects[1] != null: " + (m_PersistentObjects[1] != null));
                return null;
            }
        }

        protected override void SetPersistentObjects(NeoSerializedGameObject[] objects)
        {
            var controller = objects[0].GetComponent<IController>();
            if (controller != null)
            {
                player = controller;

                var character = objects[1].GetComponent<ICharacter>();
                if (character != null)
                    player.currentCharacter = character;
            }
        }

        #endregion
    }
}