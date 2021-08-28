using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Serialization;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/utilitiesref-mb-scenepoolinfo.html")]
	public class ScenePoolHandler : MonoBehaviour, INeoSerializableComponent
	{
        [SerializeField, FormerlySerializedAs("m_StartingPools"), Tooltip("The pools to set up at initialisation.")]
        private PoolInfo[] m_ScenePools = new PoolInfo[0];

        private bool m_Initialised = false;
        private NeoSerializedGameObject m_Nsgo = null;
        private Dictionary<PooledObject, Pool> m_Pools = new Dictionary<PooledObject, Pool>();

        struct Pool
        {
            public PooledObject prototype;
            public Transform poolTransform;
            public Transform activeTransform;
            public NeoSerializedGameObject poolNsgo;
            public NeoSerializedGameObject activeNsgo;
            public NeoSerializedGameObject prototypeNsgo;

            public int total
            {
                get { return poolTransform.childCount + activeTransform.childCount; }
            }

            public void Grow(int count)
            {
                int current = total;
                if (count > current)
                {
                    if (prototypeNsgo != null)
                    {
                        for (int i = current; i < count; ++i)
                        {
                            var obj = poolNsgo.InstantiatePrefab<PooledObject>(prototypeNsgo.prefabStrongID, i + 1);
                            obj.gameObject.SetActive(false);
                            obj.poolTransform = poolTransform;
                        }
                    }
                    else
                    {
                        for (int i = current; i < count; ++i)
                        {
                            PooledObject obj = Instantiate(prototype);
                            obj.gameObject.SetActive(false);
                            obj.transform.SetParent(poolTransform);
                            obj.poolTransform = poolTransform;
                        }
                    }
                } // Shrink too?
            }

            public Pool(PooledObject proto, int count, Transform pt, Transform at)
            {
                prototype = proto;
                poolTransform = pt;
                activeTransform = at;
                poolNsgo = null;
                activeNsgo = null;
                prototypeNsgo = null;

                for (int i = 0; i < count; ++i)
                {
                    PooledObject obj = Instantiate(prototype);
                    obj.gameObject.SetActive(false);
                    obj.transform.SetParent(poolTransform);
                    obj.poolTransform = poolTransform;
                }
            }

            public Pool(PooledObject proto, NeoSerializedGameObject protoNsgo, NeoSerializedGameObject pNsgo, NeoSerializedGameObject aNsgo)
            {
                prototype = proto;
                prototypeNsgo = protoNsgo;
                poolTransform = pNsgo.transform;
                activeTransform = aNsgo.transform;
                poolNsgo = pNsgo;
                activeNsgo = aNsgo;

                // Build hash map of active objects
                int highest = 0;
                HashSet<int> activeObjects = new HashSet<int>();
                for (int i = 0; i < activeTransform.childCount; ++i)
                {
                    int key = activeTransform.GetChild(i).GetComponent<NeoSerializedGameObject>().serializationKey;
                    if (key > highest)
                        highest = key;
                    activeObjects.Add(key);
                }

                // Fill out inactive to count, skipping active
                // Start() will fill out remaining capacity
                for (int i = 1; i < highest; ++i)
                {
                    if (!activeObjects.Contains(i))
                    {
                        var obj = poolNsgo.InstantiatePrefab<PooledObject>(prototypeNsgo.prefabStrongID, i);
                        obj.gameObject.SetActive(false);
                        obj.poolTransform = poolTransform;
                    }
                }
            }

            public Pool(PooledObject proto, int count, NeoSerializedGameObject pNsgo, NeoSerializedGameObject aNsgo)
            {
                prototype = proto;
                poolTransform = pNsgo.transform;
                activeTransform = aNsgo.transform;

                prototypeNsgo = proto.GetComponent<NeoSerializedGameObject>();
                if (prototypeNsgo != null)
                {
                    poolNsgo = pNsgo;
                    activeNsgo = aNsgo;

                    for (int i = 0; i < count; ++i)
                    {
                        var obj = poolNsgo.InstantiatePrefab<PooledObject>(prototypeNsgo.prefabStrongID, i + 1);
                        if (obj != null)
                        {
                            obj.gameObject.SetActive(false);
                            obj.poolTransform = poolTransform;
                        }
                    }
                }
                else
                {
                    poolNsgo = null;
                    activeNsgo = null;

                    for (int i = 0; i < count; ++i)
                    {
                        PooledObject obj = Instantiate(prototype);
                        obj.gameObject.SetActive(false);
                        obj.transform.SetParent(poolTransform);
                        obj.poolTransform = poolTransform;
                    }
                }
            }

            public void DestroyPool()
            {
                Destroy(poolTransform.gameObject);
                poolTransform = null;
                Destroy(activeTransform.gameObject);
                activeTransform = null;
                poolNsgo = null;
                activeNsgo = null;
                prototype = null;
                prototypeNsgo = null;
            }

            public T GetObject<T>(bool activate)
            {
                if (poolTransform == null || activeTransform == null)
                    return default(T);

                if (poolTransform.childCount > 0)
                {
                    Transform t = poolTransform.GetChild(poolTransform.childCount - 1);
                    T result = t.GetComponent<T>();

                    if (result != null)
                    {
                        if (prototypeNsgo != null)
                        {
                            var nsgo = t.GetComponent<NeoSerializedGameObject>();
                            nsgo.SetParent(activeNsgo);
                        }
                        else
                        {
                            t.SetParent(activeTransform);
                        }
                    }
                    return result;
                }
                else
                {
                    if (activeTransform.childCount > 0)
                    {
                        Transform t = activeTransform.GetChild(0);
                        T result = t.GetComponent<T>();

                        if (result != null)
                        {
                            t.gameObject.SetActive(false);
                            t.SetAsLastSibling();
                        }

                        return result;
                    }
                    else
                    {
                        Debug.LogError("Pooling system attempting to recycle an active pooled object, but none found. This shouldn't be possible");
                        return default(T);
                    }
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            for (int i = 0; i < m_ScenePools.Length; ++i)
            {
                if (m_ScenePools[i].count < 1)
                    m_ScenePools[i].count = 1;
            }
        }
#endif

        IEnumerator Start ()
        {
            yield return null;
            Initialise();
        }

        public void Initialise()
        {
            if (!m_Initialised)
            {
                // Get the NeoSerializedGameObject if appropriate
                m_Nsgo = GetComponent<NeoSerializedGameObject>();

                // Create the starting pools
                for (int i = 0; i < m_ScenePools.Length; ++i)
                {
                    // Get the prototype
                    PooledObject prototype = m_ScenePools[i].prototype;
                    if (prototype != null)
                        CreatePool(prototype, m_ScenePools[i].count);
                }

                PoolManager.SetCurrentScenePoolInfo(this);

                m_Initialised = true;
            }
        }

        void OnDestroy()
        {
        }
                
		public void CreatePool (PooledObject prototype, int count)
		{
            // Check invalid pool size
            if (count < 1)
                count = 1;

			if (m_Pools.ContainsKey (prototype))
			{
                m_Pools[prototype].Grow (count);
			}
			else
            {
                var prototypeNsgo = prototype.GetComponent<NeoSerializedGameObject>();
                if (m_Nsgo == null || prototypeNsgo == null || !NeoSerializedObjectFactory.IsPrefabRegistered(prototypeNsgo.prefabStrongID))
                {
                    // Create heirachy
                    Transform poolRoot = new GameObject(prototype.name).transform;
                    poolRoot.SetParent(transform);
                    Transform poolTransform = new GameObject("Pool").transform;
                    poolTransform.SetParent(poolRoot);
                    Transform activeTransform = new GameObject("Active").transform;
                    activeTransform.SetParent(poolRoot);

                    // Create and add the pool
                    m_Pools.Add(prototype, new Pool(prototype, count, poolTransform, activeTransform));
                }
                else
                {
                    // Create heirachy
                    var nsgo = m_Nsgo;
                    NeoSerializedGameObject poolRoot = nsgo.serializedChildren.CreateChildObject(prototype.name, prototypeNsgo.prefabStrongID);
                    NeoSerializedGameObject activeNsgo = poolRoot.serializedChildren.CreateChildObject("Active", 1);
                    NeoSerializedGameObject poolNsgo = poolRoot.serializedChildren.CreateChildObject("Pool", -1);
                    poolRoot.saveName = true;
                    activeNsgo.saveName = true;
                    poolNsgo.saveName = true;

                    // Set pool object not to serialize children
                    poolNsgo.filterChildObjects = NeoSerializationFilter.Include;

                    // Create and add the pool
                    var p = new Pool(prototype, count, poolNsgo, activeNsgo);
                    m_Pools.Add(prototype, p);
                }
			}
		}

		public void ReturnObjectToPool (PooledObject obj)
		{
            Pool pool;
			if (m_Pools.TryGetValue (obj, out pool))
			{
                var nsgo = obj.GetComponent<NeoSerializedGameObject>();
                if (nsgo != null)
                {
                    nsgo.gameObject.SetActive(false);
                    nsgo.SetParent(pool.poolNsgo);
                }
                else
                {
                    obj.gameObject.SetActive(false);
                    obj.transform.SetParent(pool.poolTransform);
                }
			}
			else
				Destroy (obj.gameObject);
		}

		public T GetPooledObject<T> (PooledObject prototype, bool activate = true)
        {
            Pool pool;
			if (m_Pools.TryGetValue (prototype, out pool))
			{
				T result = pool.GetObject<T> (activate);
                var comp = result as Component;
				if (comp != null)
				{
					Transform t = comp.transform;
					t.position = Vector3.zero;
					t.rotation = Quaternion.identity;
                    if (activate)
                        comp.gameObject.SetActive(true);
                }
                return result;
			}
			else
			{
                CreatePool (prototype, PoolManager.defaultPoolSize);
				return GetPooledObject<T> (prototype);
			}
		}

		public T GetPooledObject<T> (PooledObject prototype, Vector3 position, Quaternion rotation, bool activate = true)
        {
            Pool pool;
			if (m_Pools.TryGetValue (prototype, out pool))
			{
				T result = pool.GetObject<T> (activate);
                var comp = result as Component;
                if (comp != null)
                {
					Transform t = comp.transform;
					t.position = position;
					t.rotation = rotation;
                    if (activate)
                        comp.gameObject.SetActive(true);
                }
				return result;
			}
			else
			{
                CreatePool (prototype, PoolManager.defaultPoolSize);
				return GetPooledObject<T> (prototype, position, rotation);
			}
		}

        public T GetPooledObject<T>(PooledObject prototype, Vector3 position, Quaternion rotation, Vector3 scale, bool activate = true)
        {
            Pool pool;
            if (m_Pools.TryGetValue(prototype, out pool))
            {
                T result = pool.GetObject<T>(activate);
                var comp = result as Component;
                if (comp != null)
                {
                    Transform t = comp.transform;
                    t.position = position;
                    t.rotation = rotation;
                    t.localScale = scale;
                    if (activate)
                        comp.gameObject.SetActive(true);
                }
                return result;
            }
            else
            {
                CreatePool(prototype, PoolManager.defaultPoolSize);
                return GetPooledObject<T>(prototype, position, rotation);
            }
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Nothing needs writing - it's all handled by serializing child objects
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            m_Nsgo = nsgo;

            var childObjects = GetComponentsInChildren<NeoSerializedGameObject>();
            for (int i = 0; i < childObjects.Length; ++i)
            {
                var prefab = NeoSerializedObjectFactory.GetPrefab(childObjects[i].serializationKey);
                if (prefab != null)
                {
                    var pooledObject = prefab.GetComponent<PooledObject>();
                    if (pooledObject != null)
                    {
                        var activeNsgo = childObjects[i].serializedChildren.GetChildObject(1);
                        var inactiveNsgo = childObjects[i].serializedChildren.GetChildObject(-1);
                        if (activeNsgo != null && inactiveNsgo != null)
                        {
                            inactiveNsgo.filterChildObjects = NeoSerializationFilter.Include;
                            var pool = new Pool(pooledObject, prefab, inactiveNsgo, activeNsgo);
                            m_Pools.Add(pooledObject, pool);
                        }
                    }
                }
            }
        }
    }
}