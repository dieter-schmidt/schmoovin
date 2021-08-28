using System;
using UnityEngine;

namespace NeoFPS
{
    [Serializable]
    public struct PoolInfo
    {
        [Tooltip("The prefab object to spawn.")]
        public PooledObject prototype;
        [Tooltip("The number of objects to instantiate for the pool.")]
        public int count;
    }
}