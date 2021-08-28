using NeoFPS.SinglePlayer;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections;

namespace NeoFPS
{
    public abstract class FpsGameMode : MonoBehaviour, INeoSerializableComponent
    {
        public static event UnityAction<bool> onInGameChanged;

        public static FpsGameMode current
        {
            get;
            private set;
        }

        public static bool friendlyFire
        {
            get
            {
                if (current != null)
                    return current.GetFriendlyFire();
                else
                    return true;
            }
        }

        private static bool m_InGame = false;
        public static bool inGame
        {
            get { return m_InGame; }
            protected set
            {
                if (m_InGame != value)
                {
                    m_InGame = value;
                    // Send event
                    if (current != null)
                        current.OnInGameChanged(value);
                    if (onInGameChanged != null)
                        onInGameChanged(value);
                }
            }
        }

        protected virtual void Awake()
        {
            if (current != null)
            {
                // Destroy self as other may be a persistant game mode
                // Make sure to clean up after a game is done if this isn't the desired behaviour
                Destroy(gameObject);
            }
            else
            {
                current = this;
            }
        }

        private IEnumerator Start()
        {
            // Delay initialisation by 1 frame to allow savee games to be loaded
            // (can't reliably detect scene load before Start to rebuild objects)
            yield return null;
            
            // Load persistent objects
            if (m_HasPersistentObjects)
            {
                m_HasPersistentObjects = false;

                var container = GetPersistentObjectContainer();
                if (container == null)
                    Debug.LogError("Attempting to load persistent objects but no container provided");

                var objects = SaveGameManager.LoadGameObjectsFromBuffer(container);
                if (objects != null)
                    SetPersistentObjects(objects);
            }

            // Initialise
            OnStart();
        }

        protected virtual void OnStart()
        {

        }

        protected virtual void OnDestroy()
        {
            if (current == this)
                current = null;
        }
        
        protected virtual void OnInGameChanged(bool value)
        {
        }

        protected virtual bool GetFriendlyFire ()
        {
            return true;
        }
                
        public void Respawn(IController player)
        {
            if (!inGame)
            {
                Debug.LogError("Attempting to spawn character while not in game");
                return;
            }

            var nsgo = GetComponent<NeoSerializedGameObject>();
            var scene = (nsgo == null) ? null : nsgo.serializedScene;
            // Get the character prototype
            var prototype = GetPlayerCharacterPrototype(player);
            if (prototype != null)
            {
                // Spawn character
                // (spawning will set controller to player which will trigger event handlers, so don't need to set playerCharacter here)
                ICharacter spawned = SpawnManager.SpawnCharacter(prototype, player, false, scene);
                if (spawned == null)
                    Debug.LogError("No valid spawn points found");
            }
            else
            {
                Debug.LogError("Game mode failed to get a character prototype to spawn. Make sure you have it set in the inspector.", gameObject);
            }
        }

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
        }

        protected abstract void ProcessOldPlayerCharacter(ICharacter oldCharacter);
        protected abstract IController InstantiatePlayer();
        protected abstract ICharacter GetPlayerCharacterPrototype(IController player);

        #region PERSISTENCE

        private static bool m_HasPersistentObjects = false;
                
        public static void SavePersistentData()
        {
            m_HasPersistentObjects = false;
            if (current != null)
            {
                var objects = current.GetPersistentObjects();
                if (objects != null && objects.Length > 0)
                {
                    if (SaveGameManager.SaveGameObjectsToBuffer(objects, SaveMode.Persistence))
                        m_HasPersistentObjects = true;
                }
            }
        }

        protected virtual NeoSerializedGameObject[] GetPersistentObjects()
        {
            return null;
        }

        protected virtual void SetPersistentObjects(NeoSerializedGameObject[] objects)
        {
        }

        protected virtual NeoSerializedGameObjectContainerBase GetPersistentObjectContainer()
        {
            var scene = NeoSerializedScene.GetByPath(gameObject.scene.path);
            if (scene != null)
                return scene.sceneObjects;
            else
                return null;
        }
        
        protected virtual void OnPersistentDataLoaded(bool success)
        { }

        #endregion
    }
}