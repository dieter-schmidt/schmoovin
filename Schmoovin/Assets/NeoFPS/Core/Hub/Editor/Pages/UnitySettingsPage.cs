using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class UnitySettingsPage : HubPage
    {
        // Constant used to check for changes. Each time
        private const int k_TargetLayersVersion = 2;
        private const int k_TargetPhysicsVersion = 6;
        private const int k_TargetInputVersion = 2;
        private const int k_TargetPlayerSettingsVersion = 1;
        private const int k_TargetBuildSettingsVersion = 4;

        // The custom settings json filenames
        private const string k_JsonLayers = "CustomSettings_Layers";
        private const string k_JsonPhysics = "CustomSettings_Physics";
        private const string k_JsonInput = "CustomSettings_Input";
        private const string k_JsonPlayer = "CustomSettings_Player";

        // The custom settings json filenames
        private const string k_ProjectSettingsLayers = "ProjectSettings/TagManager.asset";
        private const string k_ProjectSettingsPhysics = "ProjectSettings/DynamicsManager.asset";
        private const string k_ProjectSettingsInput = "ProjectSettings/InputManager.asset";
        private const string k_ProjectSettingsPlayer = "ProjectSettings/ProjectSettings.asset";

        // Links to the relevant documentation pages
        private const string k_DocsUrlLayers = "https://docs.neofps.com/manual/neofps-layers-and-tags.html";
        private const string k_DocsUrlPhysics = "https://docs.neofps.com/manual/neofps-layers-and-tags.html";
        private const string k_DocsUrlInput = "https://docs.neofps.com/manual/input-settings.html";
        private const string k_DocsUrlPlayer = "https://docs.neofps.com/manual/neofps-installation.html";
        private const string k_DocsUrlBuild = "https://docs.neofps.com/manual/neofps-installation.html";
        
        private ReadmeHeader m_Heading = null;
        public ReadmeHeader heading
        {
            get
            {
                if (m_Heading == null)
                    m_Heading = new ReadmeHeader(LoadIcon("EditorImage_UnityLogoBlack", "EditorImage_UnityLogoWhite"), pageHeader);
                return m_Heading;
            }
        }

        // Scene names
        private const string k_GroupedBuildScenes = "FeatureDemo_";
        private readonly string[] k_FixedBuildScenes = new string[]
        {
            "MainMenu",
            "Loading",
            "DemoFacility_Scene"
        };


        public override string pageHeader
        {
            get { return "Unity Settings";  }
        }

        private MessageType m_Notification = MessageType.None;
        public override MessageType notification
        {
            get { return m_Notification; }
        }

        public static int currentLayersVersion
        {
            get { return NeoFpsEditorPrefs.currentLayerSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentLayerSettingsVersion = value; }
        }

        public static int currentPhysicsVersion
        {
            get { return NeoFpsEditorPrefs.currentPhysicsSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentPhysicsSettingsVersion = value; }
        }

        public static int currentInputVersion
        {
            get { return NeoFpsEditorPrefs.currentInputSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentInputSettingsVersion = value; }
        }

        public static int currentPlayerSettingsVersion
        {
            get { return NeoFpsEditorPrefs.currentPlayerSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentPlayerSettingsVersion = value; }
        }

        public static int currentBuildSettingsVersion
        {
            get { return NeoFpsEditorPrefs.currentBuildSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentBuildSettingsVersion = value; }
        }
        
        public override void Awake()
        {
            RefreshNotification();
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();

            GUILayout.Label("NeoFPS requires various project settings to be applied in order to function correctly. This includes custom layers, custom input axes, and an optimised layer collision matrix.", ReadmeEditorUtility.bodyStyle);

            // Out of date warning
            string message;
            bool outOfDate = ShowOutOfDateWarning(out message);
            if (outOfDate)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                GUILayout.Space(2);
            }

            // Apply all
            GUILayout.Label("Easy Mode", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Hit the button to automatically apply all the latest settings that NeoFPS requires to function properly.", EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Apply All Required Settings") && EditorUtility.DisplayDialog("Warning", "This will overwrite a number of your project's settings.", "OK", "Cancel"))
                ApplyAllSettings();

            GUILayout.EndVertical();

            // Apply Individual Settings
            GUILayout.Label("Individual settings", EditorStyles.boldLabel);
            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            GUILayout.Label("If you are importing NeoFPS into an existing project then automatically applying the required settings could interfere with your project settings.\n\nIf you want, you can apply individual settings or learn about what NeoFPS requires. Hitting \"Apply Manually\" will flag the relevant settings as up to date and open the relevant unity settings for editing.\n", EditorStyles.wordWrappedLabel);

            // Layers and Tags
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layers and Tags:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestLayerSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowLayerSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlLayers);
            GUILayout.EndHorizontal();

            // Layers and Tags
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Physics:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestPhysicsSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowPhysicsSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlPhysics);
            GUILayout.EndHorizontal();

            // Input
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestInputSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowInputSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlInput);
            GUILayout.EndHorizontal();

            // Build Settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestBuildSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowBuildSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlBuild);
            GUILayout.EndHorizontal();

            // Player Settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestPlayerSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowPlayerSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlPlayer);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (outOfDate)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The NeoFPS hub will keep popping up on start until you update your settings either automatically or manually using the buttons above.", MessageType.Warning);
            }
        }

        void ApplyLatestLayerSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's existing layers and tags\n\nSorting layers will not be affected.", "OK", "Cancel"))
                return;

            // Create the layer settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Layers>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonLayers))
            {
                Debug.LogError("Couldn't load layers settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsLayers))
                currentLayersVersion = k_TargetLayersVersion;

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestPhysicsSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's layer collision matrix.\n\nOther physics settings will not be affected.", "OK", "Cancel"))
                return;

            // Create the physics settings intermediate
            var physicsIntermediate = ScriptableObject.CreateInstance<SettingsIntermediate_Physics>();

            // Load values from JSON
            if (!physicsIntermediate.LoadFromJsonAsset(k_JsonPhysics))
            {
                Debug.LogError("Couldn't load physics settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (physicsIntermediate.ApplySettings(k_ProjectSettingsPhysics))
                currentPhysicsVersion = k_TargetPhysicsVersion;

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestInputSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's input axes.", "OK", "Cancel"))
                return;

            // Create the settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Input>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonInput))
            {
                Debug.LogError("Couldn't load input settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsInput))
                currentInputVersion = k_TargetInputVersion;

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestPlayerSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite the active color space of the project.\n\nNo other settings will be affected.", "OK", "Cancel"))
                return;

            // Create the settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Player>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonPlayer))
            {
                Debug.LogError("Couldn't load player settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsPlayer))
                currentPlayerSettingsVersion = k_TargetPlayerSettingsVersion;

            // Reset hub textures (switching lighting modes messes them up
            NeoFpsHubEditor.ResetTextures();

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestBuildSettings(bool silent)
        {
            currentBuildSettingsVersion = k_TargetBuildSettingsVersion;

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool changed = false;
            string[] guids;

            // Get fixed build scenes
            for (int i = 0; i < k_FixedBuildScenes.Length; ++i)
            {
                // Get guid
                guids = AssetDatabase.FindAssets(k_FixedBuildScenes[i] + " t:Scene");
                if (guids.Length == 0)
                {
                    Debug.LogError("Couldn't find NeoFPS demo scene: " + k_FixedBuildScenes[i]);
                    break;
                }
                GUID guid = new GUID(guids[0]);

                // Check if it's already in the list
                int found = GetSceneIndex(buildScenes, guid);
                if (found == i)
                    continue;

                if (found == -1)
                {
                    // Not found. Add it at the correct index
                    buildScenes.Add(null);
                    for (int j = i + 1; j < buildScenes.Count; ++j)
                        buildScenes[j] = buildScenes[j - 1];
                    buildScenes[i] = new EditorBuildSettingsScene(guid, true);
                }
                else
                {
                    var swap = buildScenes[i];
                    buildScenes[i] = buildScenes[found];
                    buildScenes[found] = swap;
                }

                changed = true;
            }

            // Get grouped build scenes
            guids = AssetDatabase.FindAssets(k_GroupedBuildScenes + " t:Scene");
            if (guids.Length == 0)
            {
                Debug.LogError("No NeoFPS feature demo scenes found.");
                return;
            }

            // Check each one and add to list if not found
            for (int i = 0; i < guids.Length; ++i)
            {
                GUID guid = new GUID(guids[i]);

                int found = GetSceneIndex(buildScenes, guid);
                if (found == -1)
                {
                    buildScenes.Add(new EditorBuildSettingsScene(guid, true));
                    changed = true;
                }
            }

            if (changed)
            {
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            if (!silent)
                RefreshNotification();
        }

        int GetSceneIndex(List<EditorBuildSettingsScene> scenes, GUID guid)
        {
            for (int i = 0; i < scenes.Count; ++i)
                if (scenes[i].guid == guid)
                    return i;
            return -1;
        }

        void ShowLayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Tags and Layers");
            currentLayersVersion = k_TargetLayersVersion;
        }

        void ShowPhysicsSettings()
        {
            SettingsService.OpenProjectSettings("Project/Physics");
            currentPhysicsVersion = k_TargetPhysicsVersion;
        }

        void ShowInputSettings()
        {
            SettingsService.OpenProjectSettings("Project/Input");
            currentInputVersion = k_TargetInputVersion;
        }

        void ShowBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
            currentBuildSettingsVersion = k_TargetBuildSettingsVersion;
        }

        void ShowPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
            currentPlayerSettingsVersion = k_TargetPlayerSettingsVersion;
        }

        public void RefreshNotification ()
        {
            if (CheckIsOutOfDate())
                m_Notification = MessageType.Error;
            else
                m_Notification = MessageType.None;
        }

        public static bool CheckIsOutOfDate()
        {
            if (currentLayersVersion < k_TargetLayersVersion) return true;
            if (currentPhysicsVersion < k_TargetPhysicsVersion) return true;
            if (currentInputVersion < k_TargetInputVersion) return true;
            if (currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion) return true;
            return false;
        }

        public static bool ShowOutOfDateWarning(out string message)
        {
            bool show = false;
            show |= currentLayersVersion < k_TargetLayersVersion;
            show |= currentPhysicsVersion < k_TargetPhysicsVersion;
            show |= currentInputVersion < k_TargetInputVersion;
            show |= currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion;

            if (!show)
                message = string.Empty;
            else
            {
                string msg = "It looks like some of the Unity settings that NeoFPS requires have changed since you last ran the wizard.\n\nThe following settings need updating:";
                if (currentLayersVersion < k_TargetLayersVersion)
                    msg += "\n- Layers and Tags";
                if (currentPhysicsVersion < k_TargetPhysicsVersion)
                    msg += "\n- Physics";
                if (currentInputVersion < k_TargetInputVersion)
                    msg += "\n- Input";
                if (currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion)
                    msg += "\n- Player Settings";
                if (currentBuildSettingsVersion < k_TargetBuildSettingsVersion)
                    msg += "\n- Build Settings";
                message = msg;
            }
            return show;
        }

        void ApplyAllSettings()
        {
            ApplyLatestLayerSettings(true);
            ApplyLatestPhysicsSettings(true);
            ApplyLatestInputSettings(true);
            ApplyLatestBuildSettings(true);
            ApplyLatestPlayerSettings(true);
            RefreshNotification();
        }
    }
}