using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace NeoFPSEditor
{
    public class PackageDependencyChecker : ScriptableObject
    {
        private const int k_TargetVersion = 1;

        private const string k_JsonPackages = "NeoFpsPackages";

        private static PackageDependencyChecker s_Instance = null;

        public static void CheckPackages(Action onComplete)
        {
            if (NeoFpsEditorPrefs.currentPackageDependenciesVersion < k_TargetVersion)
            {
                // Load settings
                s_Instance = CreateInstance<PackageDependencyChecker>();
                if (!s_Instance.LoadFromJsonAsset(k_JsonPackages))
                {
                    // Destroy
                    DestroyImmediate(s_Instance);
                    s_Instance = null;

                    // Signal completion
                    if (onComplete != null)
                        onComplete();

                    return;
                }

                // Perform check
                s_Instance.CheckPackagesInternal(onComplete);

                // Update current version
                NeoFpsEditorPrefs.currentPackageDependenciesVersion = k_TargetVersion;
            }
            else
            {
                // Signal completion
                if (onComplete != null)
                    onComplete();
            }
        }

        public Package[] packages = new Package[0];

        [Serializable]
        public class Package
        {
            public string packageName = string.Empty;
            public string version = string.Empty;
        }

        private Action m_OnComplete = null;
        private int m_PackageIndex = 0;
        private SearchRequest m_SearchRequest = null;
        private AddRequest m_AddRequest = null;

        public bool LoadFromJsonAsset(string assetName)
        {
            string[] guids = AssetDatabase.FindAssets(assetName);
            if (guids.Length == 0)
                return false;

            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (json == null)
                return false;

            JsonUtility.FromJsonOverwrite(json.text, this);

            return true;
        }

        void CheckPackagesInternal(Action onComplete)
        {
            if (packages != null && packages.Length > 0)
            {
                Debug.Log("Checking NeoFPS Package Dependencies");

                // Store completion callback
                m_OnComplete = onComplete;

                // Check the first package
                m_PackageIndex = -1;
                CheckNextPackage();

                // Wait for check to complete
                EditorApplication.update += WaitForSearch;
            }
            else
            {
                // Signal completion
                if (onComplete != null)
                    onComplete();
            }
        }

        bool CheckNextPackage()
        {
            ++m_PackageIndex;
            if (m_PackageIndex < packages.Length)
            {
                // Search for the next package
                m_SearchRequest = Client.Search(packages[m_PackageIndex].packageName);
                return true;
            }
            else
                return false;
        }

        bool InstallPackage()
        {
            // Unsubscribe from event
            EditorApplication.update -= WaitForSearch;

            // Package not installed. Send add request
            if (packages[m_PackageIndex].version == string.Empty)
                m_AddRequest = Client.Add(packages[m_PackageIndex].packageName);
            else
                m_AddRequest = Client.Add(packages[m_PackageIndex].packageName + "@" + packages[m_PackageIndex].version);
            EditorApplication.update += WaitForInstall;

            return true;
        }

        void WaitForSearch()
        {
            if (!m_SearchRequest.IsCompleted)
                return;

            if (m_SearchRequest.Status == StatusCode.Success)
            {
                var package = m_SearchRequest.Result[0];
                if (package.status == PackageStatus.Available)
                {
                    if (packages[m_PackageIndex].version == string.Empty)
                    {
                        // Version not specified. Move to next
                        if (!CheckNextPackage())
                        {
                            EditorApplication.update -= WaitForSearch;
                            OnCompleted();
                        }
                    }
                    else
                    {
                        // Check version
                        Version vInstalled = new Version(package.version);
                        Version vRequired = new Version(packages[m_PackageIndex].version);
                        if (vRequired > vInstalled)
                        {
                            // Out of date. Install
                            if (InstallPackage())
                            {
                                EditorApplication.update -= WaitForSearch;
                                return;
                            }
                        }
                        else
                        {
                            // Up to date. Move to next
                            if (!CheckNextPackage())
                            {
                                EditorApplication.update -= WaitForSearch;
                                OnCompleted();
                            }
                        }
                    }
                }
                else
                {
                    if (!InstallPackage())
                    {
                        EditorApplication.update -= WaitForSearch;
                        return;
                    }
                }
            }
            else
            {
                Debug.Log("Incorrect packages in NeoFpsPackages.json file (Core/Editor/CustomSettings). Please check the ID is correct: " + packages[m_PackageIndex].packageName);
                if (!CheckNextPackage())
                {
                    EditorApplication.update -= WaitForSearch;
                    OnCompleted();
                }
            }
        }

        void WaitForInstall()
        {
            if (m_AddRequest.Status == StatusCode.InProgress)
            //if (!m_AddRequest.IsCompleted)
                return;
            
            EditorApplication.update -= WaitForInstall;

            if (CheckNextPackage())
                EditorApplication.update += WaitForSearch;
            else
                OnCompleted();
        }
        
        void OnCompleted()
        {
            // Signal completion
            if (m_OnComplete != null)
                m_OnComplete();

            DestroyImmediate(s_Instance);
            s_Instance = null;
        }
    }
}