using NeoFPS;
using NeoFPS.Constants;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public static class WizardUtility
    {
        public static T GetRelativeComponent<T>(GameObject prefab, GameObject instance, T component) where T : class
        {
            var cast = component as Component;
            if (cast != null)
            {
                var result = GetRelativeGameObject(prefab, instance, cast.gameObject);
                if (result != null)
                    return result.GetComponent<T>();
            }
            return null;
        }

        static int GetSafeSiblingIndex (Transform t)
        {
            if (t == null || t.parent == null)
                return -1;

            var parent = t.parent;

            var children = parent.GetComponents<Transform>();
            for (int i = 0; i < children.Length; ++i)
            {
                if (children[i] == t)
                    return i;
            }

            return -1;
        }

        public static GameObject GetRelativeGameObject(GameObject prefab, GameObject instance, GameObject target)
        {
            // Check if null    
            if (target == null || prefab == null || instance == null)
                return null;

            // Check if root            
            if (prefab == target)
                return instance;
            
            // Index list
            List<int> indices = new List<int>();

            // Crawl from source object to source root
            var itr = target.transform;
            while (itr != prefab && itr != null && itr.parent != null)
            {
                //int index = GetSafeSiblingIndex(itr);
                int index = itr.GetSiblingIndex();
                //Debug.Log(string.Format("Object {0} is child index {1} of {2}", itr.name, index, itr.parent.name));
                indices.Add(index);
                itr = itr.parent;
            }

            //string printString = "indices: ";
            //for (int i = indices.Count - 1; i >= 0; --i)
            //    printString += indices[i] + " ";
            //Debug.Log(printString);

            // Reverse for destination
            itr = instance.transform;                
            for (int i = indices.Count - 1; i >= 0; --i)
                itr = itr.GetChild(indices[i]);

            return itr.gameObject;
        }

        public static GameObject AddWieldableSpringObject(GameObject rootObject)
        {
            // Set up spring animation
            GameObject springObject = new GameObject("LocalWeapon");
            springObject.transform.SetParent(rootObject.transform, false);
            springObject.AddComponent<AdditiveTransformHandler>();
            springObject.AddComponent<WeaponMomentumSway>();
            springObject.AddComponent<BreathingEffect>();
            var bob = springObject.AddComponent<PositionBob>();
            var bobSO = new SerializedObject(bob);
            bobSO.FindProperty("m_BobType").enumValueIndex = 1;

            // Add default bob data to position bob
            var guids = AssetDatabase.FindAssets("DefaultBobData");
            if (guids != null && guids.Length > 0)
            {
                bobSO.FindProperty("m_BobData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PositionBobData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            bobSO.ApplyModifiedPropertiesWithoutUndo();
            return springObject;
        }

        public static string GetPrefabOutputFolder()
        {
            // Get the save folder
            var folderPath = EditorUtility.SaveFolderPanel("Choose Prefab Output Folder", "Assets/", string.Empty);

            // Check if cancelled
            if (folderPath == null || folderPath.Length == 0)
                return null;

            // Check if invalid
            if (!folderPath.StartsWith(Application.dataPath))
            {
                Debug.LogError("Folder must be in the project's Asset hierarchy");
                return null;
            }
            folderPath = folderPath.Remove(0, folderPath.LastIndexOf("Assets"));

            return folderPath;
        }

        public static AudioSource AddAudioSource(GameObject targetObject)
        {
            // Add audio source
            var source = targetObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            // Set output mixer group from audio manager
            var guids = AssetDatabase.FindAssets("t:NeoFpsAudioManager");
            if (guids != null && guids.Length != 0)
            {
                var audioMgr = AssetDatabase.LoadAssetAtPath<NeoFpsAudioManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
                var audioMgrSO = new SerializedObject(audioMgr);
                source.outputAudioMixerGroup = audioMgrSO.FindProperty("m_SpatialEffectsGroup").objectReferenceValue as AudioMixerGroup;
            }

            return source;
        }

        public static void AddSimpleSurface(GameObject gameObject, FpsSurfaceMaterial surface)
        {
            var simpleSurface = gameObject.AddComponent<SimpleSurface>();
        }

        public static bool InspectOutputInfo(SerializedObject serializedObject)
        {
            bool valid = NeoFpsEditorGUI.RequiredStringField(serializedObject.FindProperty("prefabName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoPrefix"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overwriteExisting"));

            return valid;
        }

        public static string GetPrefabPath(string prefabName, string folderPath, bool overwrite)
        {
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!overwrite)
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            return path;
        }

        public static float GetExitTime(AnimationClip clip, float duration)
        {
            if (clip == null)
                return 1f;
            return Mathf.Clamp01(1f - (duration / clip.length));
        }
    }
}
