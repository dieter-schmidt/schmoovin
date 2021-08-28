#define NEOFPS_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NeoFPSEditor.Hub.Pages;
using NeoFPS;
using NeoSaveGames;
using NeoSaveGames.SceneManagement;
using NeoFPS.Hub;
using System;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards;

namespace NeoFPSEditor.Hub
{
    public class NeoFpsHubEditor : EditorWindow
    {
        private static readonly char[] k_DefinesSplitter = { ';' };

        [MenuItem("Tools/NeoFPS/NeoFPS Hub", priority = 0)]
        public static void ShowWindow()
        {
            NeoFpsHubEditor window = GetWindow<NeoFpsHubEditor>();
            window.titleContent = new GUIContent("NeoFPS Hub");
            window.minSize = new Vector2(896, 120);// 660);
            instance = window;
        }

        public static void ShowOnStartup(Action onComplete)
        {
            // Check scripting defines
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            var split = defines.Split(k_DefinesSplitter);
            if (Array.IndexOf(split, "NEOFPS") == -1)
            {
                defines += ";NEOFPS";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            }

            // Do settings check
            if (UnitySettingsPage.CheckIsOutOfDate())
            {
                ShowPage("unity_settings");
            }
            else
            {
                // Do prefs check
                if (NeoFpsEditorPrefs.showHub || NeoFpsEditorPrefs.firstRun)
                    ShowWindow();
            }

            if (onComplete != null)
                onComplete();
        }

        [SerializeField] private Vector2 m_PageScroll = Vector2.zero;
        [SerializeField] private string m_CurrentPageID = string.Empty;

        public static NeoFpsHubEditor instance
        {
            get;
            private set;
        }

        public HubPage frontPage
        {
            get;
            private set;
        }
        
        private HubPage m_CurrentPage = null;
        public HubPage currentPage
        {
            get { return m_CurrentPage; }
            set
            {
                // Store the old page ID
                string oldID = m_CurrentPageID;

                // Disable old page
                if (m_CurrentPage != null)
                    m_CurrentPage.OnDisable();

                // Set new page
                m_CurrentPage = value;

                // Enable new page and store ID
                if (m_CurrentPage != null)
                {
                    m_CurrentPage.OnEnable();

                    // Get ID
                    m_CurrentPageID = string.Empty;
                    foreach (var pair in m_Pages)
                    {
                        if (pair.Value == m_CurrentPage)
                        {
                            m_CurrentPageID = pair.Key;
                            break;
                        }
                    }                        
                }
                else
                    m_CurrentPageID = string.Empty;

                // Reset the page scroll if the page has changed
                if (oldID != m_CurrentPageID)
                    m_PageScroll = Vector2.zero;

                // Refresh the table of contents
                RefreshToC();
            }
        }

        private GUIStyle m_HeaderStyle = null;
        public GUIStyle headerStyle
        {
            get
            {
                if (m_HeaderStyle == null)
                {
                    m_HeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                    m_HeaderStyle.alignment = TextAnchor.MiddleCenter;
                }
                return m_HeaderStyle;
            }
        }

        private GUIStyle m_TocStyle = null;
        private GUIStyle tocStyle
        {
            get
            {
                if (m_TocStyle == null)
                {
                    m_TocStyle = new GUIStyle(GUI.skin.box);
                    if (EditorGUIUtility.isProSkin)
                        m_TocStyle.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.4f, 0.4f, 0.4f));
                    else
                        m_TocStyle.normal.background = NeoFpsEditorUtility.GetColourTexture(new Color(0.8f, 0.8f, 0.8f));
                }
                return m_TocStyle;
            }
        }

        public static void ResetTextures()
        {
            instance.m_TocStyle = null;
        }

        void Awake()
        {
            EditorApplication.quitting += Quitting;
            //Initialise();
        }

        void OnDestroy()
        {
            EditorApplication.quitting -= Quitting;
            instance = null;
        }

        void Quitting()
        {
            Close();
        }

        private void OnEnable()
        {
            Initialise();
            instance = this;
            
            // Sort the list
            RefreshToC();

            // Set the starting page
            if (string.IsNullOrEmpty(m_CurrentPageID))
                currentPage = frontPage;
            else
                ShowPage(m_CurrentPageID);
        }

        void OnGUI()
        {
#if !NEOFPS_DEBUG
            try
            {
#endif
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawTableOfContents();
                DrawCurrentPage();
            }
#if !NEOFPS_DEBUG
            }
            catch (Exception e)
            {
                if (e is ExitGUIException)
                    throw e;
                else
                    Debug.Log("Error drawing hub: " + e.Message);
            }
#endif
        }

        void DrawCurrentPage()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(4);

                // Draw current page
                if (currentPage != null)
                {
#if !NEOFPS_DEBUG
                    try
                    {
#endif
                    EditorGUILayout.LabelField(currentPage.pageHeader, headerStyle);
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_PageScroll, GUIStyle.none, GUI.skin.verticalScrollbar))
                        {
                            m_PageScroll = scrollView.scrollPosition;
                            currentPage.OnGUI();
                        }
#if !NEOFPS_DEBUG
                    }
                    catch (Exception e)
                    {
                        if (e is ExitGUIException)
                            throw e;
                        else
                            Debug.Log("Caught: " + e.Message);
                    }
#endif
                }
                else
                    EditorGUILayout.LabelField("<No Page Selected>", headerStyle);
            }
        }

        void Initialise()
        {
            m_Toc.Clear();

            // Add main pages
            frontPage = new FrontPage();
            AddPage("Front Page", "front_page", frontPage, 0);
            AddPage("Unity Settings", "unity_settings", new UnitySettingsPage(), 1);

            InitialiseUpgradeNotes();
            InitialiseQuickStarts();
            InitialiseDemoScenes();
            InitialiseGameSettings();
            InitialiseManagers();
            InitialiseWizards();
            InitialiseIntegrations();
            InitialiseStandaloneTools();

            // Call awake on entries
            for (int i = 0; i < m_Toc.Count; ++i)
                m_Toc[i].Awake();
        }

        void InitialiseUpgradeNotes()
        {
            var upgradeNotes = LoadAsset<UpgradeNotesReadme>();
            if (upgradeNotes != null)
                AddPage("Upgrade Notes", "upgrade_notes", new UpgradeNotesPage(upgradeNotes), 2);
        }

        void InitialiseDemoScenes()
        {
            AddPage("Demo Scenes", "demo_scenes", new DemoScenesPage(), 4);

            // Add quick starts
            AddPage("Demo Scenes/Scene Info", "demo_info", new DemoSceneInfoPage(), 0);
        }

        void InitialiseQuickStarts()
        {
            // Add section readme page
            var readme = LoadAsset<HubSectionReadme>("QuickStartReadme");
            if (readme != null)
                AddPage("Quick Start", "quickstart_index", new ReadmePage(readme), 3);
            else
                AddPageGroup("Quick Start", 3);

            // Find quickstart assets
            var guids = AssetDatabase.FindAssets("t:ReadmeAsset Quickstart*");
            for (int i = 0; i < guids.Length; ++i)
            {
                var qs = AssetDatabase.LoadAssetAtPath<QuickstartReadme>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (qs.subFolder == string.Empty)
                    AddPage("Quick Start/" + qs.header.title, qs.pageName, new ReadmePage(qs), i + 1);
                else
                {
                    AddPageGroup("Quick Start/" + qs.subFolder, 0);
                    AddPage(string.Format("Quick Start/{0}/{1}", qs.subFolder, qs.header.title), qs.pageName, new ReadmePage(qs), i);
                }
            }
        }

        void InitialiseGameSettings ()
        {
            // Add section readme page
            var readme = LoadAsset<HubSectionReadme>("GameSettingsReadme");
            if (readme != null)
                AddPage("Game Settings", "game_settings_index", new ReadmePage(readme), 5);
            else
                AddPageGroup("Game Settings", 5);

            var guids = AssetDatabase.FindAssets("t:SettingsContextBase");
            for (int i = 0; i < guids.Length; ++i)
            {
                var settingsAsset = AssetDatabase.LoadAssetAtPath<SettingsContextBase>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (settingsAsset != null)
                    AddPage("Game Settings/" + settingsAsset.tocName, settingsAsset.tocID, new SettingsPage(settingsAsset), i);
            }
        }

        void InitialiseManagers()
        {
            // Add section readme page
            var readme = LoadAsset<HubSectionReadme>("ManagersReadme");
            if (readme != null)
                AddPage("Managers", "managers_index", new ReadmePage(readme), 6);
            else
                AddPageGroup("Managers", 6);

            int index = 0;

            var audioManager = LoadAsset<NeoFpsAudioManager>();
            if (audioManager != null)
                AddPage("Managers/Audio", "manager_audio", new ManagerPage_Audio(audioManager), index++);

            var generatedConstants = LoadAsset<ConstantsSettings>("NeoFPSConstants");
            if (generatedConstants != null)
                AddPage("Managers/Generated Constants", "manager_generatedconstants", new ManagerPage_GeneratedConstants(generatedConstants), index++);

            var inputManager = LoadAsset<NeoFpsInputManager>();
            if (inputManager != null)
                AddPage("Managers/Input", "manager_input", new ManagerPage_Input(inputManager), index++);

            var inventoryDatabase = LoadAsset<NeoFpsInventoryDatabase>();
            if (inventoryDatabase != null)
                AddPage("Managers/Inventory Database", "manager_inventory", new ManagerPage_Inventory(inventoryDatabase), index++);

            var poolManager = LoadAsset<PoolManager>();
            if (poolManager != null)
                AddPage("Managers/Pooling", "manager_pooling", new ManagerPage_Pooling(poolManager), index++);

            var saveManager = LoadAsset<SaveGameManager>();
            if (saveManager != null)
                AddPage("Managers/Save System", "manager_savegames", new ManagerPage_SaveSystem(saveManager), index++);

            var sceneManager = LoadAsset<NeoSceneManager>();
            if (sceneManager != null)
                AddPage("Managers/Scene Manager", "manager_scenes", new ManagerPage_SceneManager(sceneManager), index++);

            var surfaceManager = LoadAsset<SurfaceManager>();
            if (surfaceManager != null)
                AddPage("Managers/Surfaces", "manager_surfaces", new ManagerPage_SurfaceManager(surfaceManager), index++);
        }

        void InitialiseWizards()
        {
            // Add section readme page
            var readme = LoadAsset<HubSectionReadme>("WizardsReadme");
            if (readme != null)
                AddPage("Wizards", "wizards_index", new ReadmePage(readme), 7);
            else
                AddPageGroup("Wizards", 7);

            int index = 0;

            AddPage("Wizards/Player Character", "wizard_player_character", new NeoFpsWizardPage<PlayerCharacterWizard>("Player Character Wizard", "Create a new player character", WizardTemplateDescriptions.playerCharacterTemplates), index++);
            AddPage("Wizards/Modular Firearm", "wizard_firearm", new NeoFpsWizardPage<ModularFirearmWizard>("Modular Firearm Wizard", "Create a new firearm", WizardTemplateDescriptions.firearmTemplates), index++);
            //AddPage("Wizards/Firearm Animator", "wizard_firearmanim", new NeoFpsWizardPage<FirearmAnimatorWizard>("Firearm Animator Wizard", "Set up a firearm animator controller", WizardTemplateDescriptions.firearmTemplates), index++);
            AddPage("Wizards/Melee Weapon", "wizard_melee", new NeoFpsWizardPage<MeleeWeaponWizard>("Melee Weapon Wizard", "Create a new melee weapon", WizardTemplateDescriptions.meleeTemplates), index++);
            AddPage("Wizards/Thrown Weapon", "wizard_thrown", new NeoFpsWizardPage<ThrownWeaponWizard>("Thrown Weapon Wizard", "Create a new thrown weapon", WizardTemplateDescriptions.thrownTemplates), index++);
            AddPage("Wizards/Pickup", "wizard_pickup", new NeoFpsWizardPage<PickupWizard>("Pickup Wizard", "Create a new pickup", WizardTemplateDescriptions.pickupTemplates), index++);
            AddPage("Wizards/Interactive Object", "wizard_interactive", new NeoFpsWizardPage<InteractiveObjectWizard>("Interactive Object Wizard", "Create a new interactive object", WizardTemplateDescriptions.interactiveTemplates), index++);

            var scriptGenerator = LoadAsset<NeoFpsScriptCreationWizard>();
            if (scriptGenerator != null)
                AddPage("Wizards/Custom Scripts", "wizard_customscripts", new NeoFpsScriptCreationWizardPage(scriptGenerator), index++);
        }

        void InitialiseIntegrations()
        {
            var readme = LoadAsset<ReadmeAsset>("IntegrationsReadme");
            if (readme != null)
                AddPage("Integrations", "integrations", new ReadmePage(readme), 8);
        }

        void InitialiseStandaloneTools()
        {
            var readme = LoadAsset<ReadmeAsset>("StandaloneToolsReadme");
            if (readme != null)
                AddPage("Standalone Tools", "standalone_tools", new ReadmePage(readme), 9);
        }

        public static T LoadAsset<T>() where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids == null || guids.Length == 0)
                return null;
            else
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(string.Format("t:{0} {1}", typeof(T).Name, assetName));
            if (guids == null || guids.Length == 0)
                return null;
            else
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        
#region TABLE OF CONTENTS

        private const float k_TocWidth = 200;

        private static readonly char[] k_PathSeparators = new char[] { '\\', '/' };

        private List<HubTocEntry> m_Toc = new List<HubTocEntry>();

        private Dictionary<string, HubPage> m_Pages = new Dictionary<string, HubPage>();
        
        public void AddPageGroup(string path, int order)
        {
            AddPage(path, string.Empty, null, order);
        }

        public void AddPage(string path, string id, HubPage page, int order)
        {
            string[] split = path.Split(k_PathSeparators);

            List<HubTocEntry> currentList = m_Toc;

            // Record page
            if (page != null && !string.IsNullOrEmpty(id))
                m_Pages.Add(id, page);

            int index = 0;
            while (index < split.Length)
            {
                // Check if last subString (the page name)
                bool last = (index == split.Length - 1);

                if (!string.IsNullOrEmpty(split[index]))
                {
                    // Get lowercase for comparison
                    string lower = split[index].ToLower();

                    // Check if entry exists
                    bool found = false;
                    for (int i = 0; i < currentList.Count; ++i)
                    {
                        if (currentList[i].tocLower == lower)
                        {
                            if (last)
                            {
                                // Set page
                                currentList[i].page = page;
                                currentList[i].order = order;
                            }
                            else
                            {
                                // Switch to children
                                currentList = currentList[i].children;
                            }

                            found = true;
                            break;
                        }
                    }

                    // If not found, create a new entry
                    if (!found)
                    {
                        HubTocEntry temp = last ? new HubTocEntry(split[index], page, order) : new HubTocEntry(split[index], null, 0);
                        currentList.Add(temp);
                        currentList = temp.children;
                    }
                }

                ++index;
            }
        }

        public void RemovePage(HubPage page)
        {
            for (int i = 0; i < m_Toc.Count; ++i)
            {
                if (m_Toc[i].page == page)
                {
                    m_Toc.RemoveAt(i);
                    break;
                }
                else
                {
                    if (m_Toc[i].RemoveChild(page))
                        break;
                }
            }
        }

        public void RemovePage(string path)
        {
            string[] split = path.Split(k_PathSeparators);

            List<HubTocEntry> currentList = m_Toc;

            int index = 0;
            while (index < split.Length)
            {
                // Check if last subString (the page name)
                bool last = (index == split.Length - 1);

                if (!string.IsNullOrEmpty(split[index]))
                {
                    // Get lowercase for comparison
                    string lower = split[index].ToLower();

                    // Check if entry exists
                    bool found = false;
                    for (int i = 0; i < currentList.Count; ++i)
                    {
                        if (currentList[i].tocLower == lower)
                        {
                            if (last)
                            {
                                // Remove the entry
                                currentList.RemoveAt(i);
                                return;
                            }
                            else
                            {
                                // Switch to children
                                currentList = currentList[i].children;
                            }

                            found = true;
                            break;
                        }
                    }

                    // If not found, give up
                    if (!found)
                        return;
                }

                ++index;
            }
        }

        public void RefreshToC()
        {
            m_Toc.Sort();
            for (int i = 0; i < m_Toc.Count; ++i)
            {
                m_Toc[i].SortChildren();
                m_Toc[i].ResetExpandedState();
            }
        }

        public static void ShowPage(string id)
        {
            if (instance == null)
                ShowWindow();
            if (instance.m_Pages.ContainsKey(id))
                instance.currentPage = instance.m_Pages[id];
        }

        private Vector2 m_TocScroll = Vector2.zero;
        
        public void DrawTableOfContents()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(k_TocWidth));

            GUILayout.Space(4);
            EditorGUILayout.LabelField("Contents", headerStyle);

            // Contents are drawn inside a scroll view
            m_TocScroll = EditorGUILayout.BeginScrollView(m_TocScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, tocStyle);

            for (int i = 0; i < m_Toc.Count; ++i)
                m_Toc[i].Draw(k_TocWidth - 20, 0);

            EditorGUILayout.EndScrollView();

            GUILayout.Space(6);
            EditorGUILayout.EndVertical();
        }

#endregion
    }
}