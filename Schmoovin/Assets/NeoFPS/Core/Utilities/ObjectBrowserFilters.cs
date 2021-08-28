using UnityEngine;

namespace NeoFPS
{
    public delegate bool GameObjectFilter(GameObject obj);
    public delegate bool ComponentFilter<T>(T component) where T : class;
    public delegate bool AssetFilter(ScriptableObject obj);
    public delegate bool AssetFilter<T>(T asset) where T : class;
}
