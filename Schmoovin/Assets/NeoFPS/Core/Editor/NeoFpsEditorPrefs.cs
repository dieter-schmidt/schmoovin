using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public class NeoFpsEditorPrefs : ScriptableObject
    {
        private static NeoFpsEditorPrefs s_Instance;

        [SerializeField]
        private bool m_FirstRun = true;
        [SerializeField, Tooltip ("Show the NeoFPS Hub on loading the project.")]
        private bool m_ShowHubOnStart = false;
        [SerializeField]
        private int m_CurrentLayerSettingsVersion = 0;
        [SerializeField]
        private int m_CurrentPhysicsSettingsVersion = 0;
        [SerializeField]
        private int m_CurrentInputSettingsVersion = 0;
        [SerializeField]
        private int m_CurrentPlayerSettingsVersion = 0;
        [SerializeField]
        private int m_CurrentBuildSettingsVersion = 0;
        [SerializeField]
        private int m_CurrentNeoFPSVersion = 0;
        [SerializeField]
        private int m_CurrentPackageDependenciesVersion = 0;

        private SerializedObject m_SerializedObject = null;

        static NeoFpsEditorPrefs GetPreferences()
        {
            if (s_Instance != null)
                return s_Instance;

            // Check if internal dev build
            var guids = AssetDatabase.FindAssets("t:MonoScript NeoFpsInternalStartup");
            if (guids != null && guids.Length > 0)
                return null;

            // Load the editor prefs if it's found
            guids = AssetDatabase.FindAssets("t:NeoFpsEditorPrefs");
            if (guids != null && guids.Length > 0)
            {
                s_Instance = AssetDatabase.LoadAssetAtPath<NeoFpsEditorPrefs>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            else
            {
                // Create the prefs asset
                s_Instance = CreateInstance<NeoFpsEditorPrefs>();

                // Get the path from the script file (allows renaming of root folder)
                guids = AssetDatabase.FindAssets("NeoFpsEditorPrefs t:MonoScript");
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                path = path.Remove(path.LastIndexOfAny(new char[] { '/', '\\' }));
                path += "/CustomSettings/NeoFpsEditorPrefs.asset";

                // Save the asset to file
                AssetDatabase.CreateAsset(s_Instance, path);
                Debug.Log("NeoFPS first run. Creating settings file.");
            }


            if (s_Instance != null)
                s_Instance.Initialise();

            return s_Instance;
        }

        private void Initialise()
        {
            m_SerializedObject = new SerializedObject(this);
        }

        public static bool firstRun
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_FirstRun;
                else
                    return false;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_FirstRun != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_FirstRun").boolValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static bool showHub
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_ShowHubOnStart;
                else
                    return false;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_ShowHubOnStart != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_ShowHubOnStart").boolValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentLayerSettingsVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentLayerSettingsVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentLayerSettingsVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentLayerSettingsVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentPhysicsSettingsVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentPhysicsSettingsVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentPhysicsSettingsVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentPhysicsSettingsVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentInputSettingsVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentInputSettingsVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentInputSettingsVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentInputSettingsVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentPlayerSettingsVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentPlayerSettingsVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentPlayerSettingsVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentPlayerSettingsVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentBuildSettingsVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentBuildSettingsVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentBuildSettingsVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentBuildSettingsVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentNeoFPSVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentNeoFPSVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentNeoFPSVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentNeoFPSVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public static int currentPackageDependenciesVersion
        {
            get
            {
                var prefs = GetPreferences();
                if (prefs != null)
                    return prefs.m_CurrentPackageDependenciesVersion;
                else
                    return int.MaxValue;
            }
            set
            {
                var prefs = GetPreferences();
                if (prefs != null && prefs.m_CurrentPackageDependenciesVersion != value)
                {
                    prefs.m_SerializedObject.FindProperty("m_CurrentPackageDependenciesVersion").intValue = value;
                    prefs.m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
}
