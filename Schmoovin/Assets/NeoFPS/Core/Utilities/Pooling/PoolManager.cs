using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace NeoFPS
{
    [CreateAssetMenu(fileName = "FpsManager_Pooling", menuName = "NeoFPS/Managers/Pool Manager", order = NeoFpsMenuPriorities.manager_pooling)]
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-so-poolmanager.html")]
    public class PoolManager : NeoFpsManager<PoolManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadNeoSceneManager()
        {
            GetInstance("FpsManager_Pooling");
        }

        [SerializeField, RequiredObjectProperty, Tooltip("A scene pool handler prefab to be instantiated in the scene if none already exists.")]
        private ScenePoolHandler m_ScenePoolHandlerPrefab = null;

        [SerializeField, FormerlySerializedAs("m_DefaultPoolSize"), Tooltip("The size of any new pools that are added to the array at runtime (after initialisation).")]
        private int m_DefaultRuntimePoolSize = 100;

        [SerializeField, Tooltip("The pools to set up at initialisation.")]
        private PoolInfo[] m_SharedPools = new PoolInfo[0];

        private static ScenePoolHandler s_CurrentScenePoolInfo = null;

        public static int defaultPoolSize
        {
            get
            {
                if (CheckScenePoolHandler())
                    return instance.m_DefaultRuntimePoolSize;
                else
                    return 100;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_DefaultRuntimePoolSize < 1)
                m_DefaultRuntimePoolSize = 1;

            for (int i = 0; i < m_SharedPools.Length; ++i)
            {
                if (m_SharedPools[i].count < 1)
                    m_SharedPools[i].count = 1;
            }
        }
#endif

        public override bool IsValid()
        {
            return m_ScenePoolHandlerPrefab != null;
        }

        static bool CheckScenePoolHandler()
        {
            if (instance == null)
                return false;

            if (s_CurrentScenePoolInfo == null)
            {
                if (instance.m_ScenePoolHandlerPrefab == null)
                {
                    Debug.LogError("Scene pool handler not found in PoolManager settings");
                    return false;
                }

                // Instantiate scene pool handler prefab (using save system if applicable)
                ScenePoolHandler result = null;
                if (SceneSaveInfo.currentActiveScene != null)
                    result = SceneSaveInfo.currentActiveScene.InstantiatePrefab(instance.m_ScenePoolHandlerPrefab);
                else
                    result = Instantiate(instance.m_ScenePoolHandlerPrefab);

                // Initialise (this calls SetCurrentScenePoolInfo)
                result.Initialise();
            }

            return true;
        }

        public static void SetCurrentScenePoolInfo(ScenePoolHandler to)
        {
            if (instance == null)
                return;

            // Check there's not too many
            if (s_CurrentScenePoolInfo != null && to != null && to != s_CurrentScenePoolInfo)
            {
                Debug.LogError("Attempting to set multiple scene pool handlers. Only one scene pool handler should exist in the main scene");
                return;
            }

            // Set current
            s_CurrentScenePoolInfo = to;

            // Add shared pools
            if (s_CurrentScenePoolInfo != null)
            {
                for (int i = 0; i < instance.m_SharedPools.Length; ++i)
                    s_CurrentScenePoolInfo.CreatePool(instance.m_SharedPools[i].prototype, instance.m_SharedPools[i].count);
            }
        }

        public static void CreatePool(PooledObject prototype)
        {
            if (CheckScenePoolHandler())
                s_CurrentScenePoolInfo.CreatePool(prototype, instance.m_DefaultRuntimePoolSize);
        }

        public static void CreatePool(PooledObject prototype, int count)
        {
            if (CheckScenePoolHandler())
                s_CurrentScenePoolInfo.CreatePool(prototype, count);
        }

        public static void ReturnObjectToPool(PooledObject obj)
        {
            // Do not create a pool handler if not found - just destroy the object instead
            if (s_CurrentScenePoolInfo != null)
                s_CurrentScenePoolInfo.ReturnObjectToPool(obj);
            else
                Destroy(obj.gameObject);
        }

        public static T GetPooledObject<T>(PooledObject prototype, bool activate = true)
        {
            if (CheckScenePoolHandler())
                return s_CurrentScenePoolInfo.GetPooledObject<T>(prototype, activate);
            else
                return default(T);
        }

        public static T GetPooledObject<T>(PooledObject prototype, Vector3 position, Quaternion rotation, bool activate = true)
        {
            if (CheckScenePoolHandler())
                return s_CurrentScenePoolInfo.GetPooledObject<T>(prototype, position, rotation, activate);
            else
                return default(T);
        }

        public static T GetPooledObject<T>(PooledObject prototype, Vector3 position, Quaternion rotation, Vector3 scale, bool activate = true)
        {
            if (CheckScenePoolHandler())
                return s_CurrentScenePoolInfo.GetPooledObject<T>(prototype, position, rotation, scale, activate);
            else
                return default(T);
        }
    }
}