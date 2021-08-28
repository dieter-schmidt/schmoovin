using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NeoSaveGames.Serialization
{
    public class NeoSerializedGameObjectProcessor : IPreprocessBuildWithReport
    { 
        public void OnPreprocessBuild(BuildReport report)
        {
            CheckPrefabs();
        }

        public int callbackOrder { get { return 0; } }

        [InitializeOnLoadMethod]
        static void CheckSerializationKeys()
        {
            CheckPrefabs();

            // Add event handler to check scene contents
            EditorSceneManager.sceneOpened += OnSceneOpened;

            // Check already open scenes if valid
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isLoaded)
                    OnSceneOpened(scene, OpenSceneMode.Additive);
            }
        }

        static void CheckPrefabs()
        {
            // Scan prefabs
            var guids = AssetDatabase.FindAssets("t:GameObject");
            if (guids != null)
            {
                foreach (var guid in guids)
                {
                    // Load prefab object & get NeoSerializedGameObject
                    var o = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    if (o != null)
                    {
                        var nsgo = o.GetComponent<NeoSerializedGameObject>();
                        if (nsgo != null)
                        {
                            // Check & validate
                            nsgo.CheckPrefabID();
                            nsgo.OnValidate();
                        }
                    }
                }
            }
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            int found = 0;
            var objects = scene.GetRootGameObjects();
            foreach (var obj in objects)
            {
                var nss = obj.GetComponent<NeoSerializedScene>();
                if (nss != null)
                {
                    // Check for multiple scene save objects
                    if (++found == 2)
                        Debug.LogError("Multiple NeoSerializedScene objects found in scene. This will create duplicate data in your saves.");

                    // Validate the scene (fills all serialized object containers and generates keys)
                    nss.OnValidate();
                }
            }
        }
    }
}
