using UnityEngine;

namespace NeoFPS
{
    public abstract class NeoFpsManager<T> : NeoFpsManagerBase where T : NeoFpsManager<T>
    {
        // You can use the NEOFPS_LOADONDEMAND scripting define (player settings) to disable loading the managers on start.
        // Instead they will be loaded the first time they are accessed. This adds a tiny bit of overhead when accessing
        // the managers, but can be useful in projects that are not specifically centered around NeoFPS.
#if NEOFPS_LOAD_ON_DEMAND

        private static string s_ManagerName = null;
        private static T s_Instance = null;

        public static T instance
        {
            get
            {
                if (s_Instance == null)
                {
                    // Load or create instance
                    s_Instance = Resources.Load<T>(s_ManagerName);
                    if (s_Instance == null)
                        s_Instance = CreateInstance<T>();

                    // Initialise
                    instance.Initialise();

                }
                return s_Instance;
            }
        }
        
        protected static void GetInstance(string managerName, bool allowTemporary = true)
        {
            s_ManagerName = managerName;
        }
        
        protected virtual void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

#else

        public static T instance
        {
            get;
            private set;
        }

        protected static T GetInstance(string managerName, bool allowTemporary = true)
        {
            // Load or create instance
            instance = Resources.Load<T>(managerName);
            if (instance == null && allowTemporary)
                instance = CreateInstance<T>();

            // Initialise
            if (instance != null)
                instance.Initialise();

            return instance;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

#endif


        protected static B GetBehaviourProxy<B>() where B : MonoBehaviour
        {
            // Get or create the manager object
            var go = GetSharedGameobject();

            // Add the component
            var result = go.AddComponent<B>();

            return result;
        }

        protected virtual void Initialise()
        {

        }
    }

    public abstract class NeoFpsManagerBase : ScriptableObject
    {
        private static GameObject s_SharedGameobject = null;

        protected static GameObject GetSharedGameobject()
        {
            // Create the object if it doesn't exist
            if (s_SharedGameobject == null)
            {
                s_SharedGameobject = new GameObject("NeoFpsManagers");
                DontDestroyOnLoad(s_SharedGameobject);
            }

            return s_SharedGameobject;
        }

        public static void DisableRuntimeManagers()
        {
            var go = GetSharedGameobject();
            go.SetActive(false);
        }

        public static void EnableRuntimeManagers()
        {
            var go = GetSharedGameobject();
            go.SetActive(true);
        }

        public abstract bool IsValid();
    }
}
