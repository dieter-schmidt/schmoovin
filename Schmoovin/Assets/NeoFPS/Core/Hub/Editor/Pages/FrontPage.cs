using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NeoFPSEditor.Hub.Pages
{
    public class FrontPage : HubPage
    {
        const string k_ShowOnStartKey = "neofps.hubstartup";
        const string k_UrlLearningTutorials = "https://neofps.com/tutorials";
        const string k_UrlLearningManual = "https://docs.neofps.com/manual/index.html";
        const string k_UrlLearningScriptingReference = "https://docs.neofps.com/api/index.html";
        const string k_UrlCommunityDiscord = "https://discord.neofps.com";
        const string k_UrlCommunityForum = "https://forum.unity.com/threads/released-neofps-a-first-person-shooter-toolkit-and-template.719579/";
        const string k_UrlReview = "https://assetstore.neofps.com#reviews";
        const string k_UrlSupportWebsite = "https://neofps.com/support";
        const string k_UrlSupportTrello = "https://trello.com/b/LDNb0DWN/bugs-issues";
        const string k_UrlSupportMailTo = "mailto:support@neofps.com";
        const string k_IntroMessage = "Thankyou for choosing NeoFPS as the basis for your FPS game.";
        const string k_LearningMessage = "It is important to us that you can get up and running with NeoFPS quickly and see a clear path to achieving your FPS vision. To this end, there are a number of learning resources available.";
        const string k_CommunityMessage = "NeoFPS has an active discord community where you can discuss game development, request features, get help and chat with other NeoFPS users. You can also keep up to date via the Unity Forums entry for NeoFPS";
        const string k_SupportMessage = "You can get support and report bugs via the NeoFPS discord. Alternatively, you can contact us via the support page on the website, or by emailing the support address. A list of known bugs and issues is maintained on the NeoFPS trello.";
        const string k_ReviewMessage = "If you're enjoying NeoFPS and want to support its development, then the single most useful thing you can do to help is to leave a review on the asset store. This helps the asset gain visibility in search results and increases the chances of it getting noticed by Unity.";
        const string k_ReviewNote = "*NB: Don't use the review system for support requests. It damages the asset and reduces the chance of a speedy response";
        const string k_SettingsInfoMessage = "NeoFPS requires a number of custom layer, physics and input settings to function properly. To learn more and to easily apply the required settings, take a look at the wizard.";
        const float k_ButtonWidth = 300f;
        const float k_SectionSpacing = 20;

        private bool m_SettingsChecked = false;
        private bool m_ShowOnStart = false;
        private bool m_ShowSettingsWarning = false;
        private string m_ShowSettingsMessage = string.Empty;
        private Texture2D m_Icon = null;
        private Texture2D m_InfoIcon = null;
        private GUIStyle m_InfoBoxStyleInternal = null;
        private GUIStyle m_SettingsButtonStyleInternal = null;
        private GUIContent m_VersionContent = null;
        
        public GUIStyle infoBoxStyle
        {
            get
            {
                if (m_InfoBoxStyleInternal == null)
                {
                    m_InfoBoxStyleInternal = new GUIStyle(EditorStyles.wordWrappedLabel);
                    m_InfoBoxStyleInternal.fontStyle = FontStyle.Bold;
                }
                return m_InfoBoxStyleInternal;
            }
        }

        public GUIStyle settingsButtonStyle
        {
            get
            {
                if (m_SettingsButtonStyleInternal == null)
                {
                    m_SettingsButtonStyleInternal = new GUIStyle(EditorStyles.miniButton);
                    m_SettingsButtonStyleInternal.padding = new RectOffset(4, 4, 4, 4);
                    m_SettingsButtonStyleInternal.fontSize = 12;
                    m_SettingsButtonStyleInternal.fontStyle = FontStyle.Bold;
                }
                return m_SettingsButtonStyleInternal;
            }
        }

        public override MessageType notification
        {
            get { return MessageType.Info; }
        }

        public override string pageHeader
        {
            get { return "Front Page"; }
        }
        
        public override void Awake()
        {
            // Check if show on start
            m_ShowOnStart = NeoFpsEditorPrefs.showHub;
        }

        public override void OnEnable()
        {
            // Load icon
            var guids = AssetDatabase.FindAssets("EditorImage_NeoFpsWelcome");
            if (guids != null && guids.Length > 0)
                m_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            else
                m_Icon = null;

            // Get current page
            guids = AssetDatabase.FindAssets("t: UpgradeNotesReadme");
            if (guids != null && guids.Length > 0)
            {
                var upgradeNotes = AssetDatabase.LoadAssetAtPath<UpgradeNotesReadme>(AssetDatabase.GUIDToAssetPath(guids[0]));
                int latest = upgradeNotes.latest;
                int major = latest / 1000;
                int minor = (latest - major * 1000) / 100;
                int revision = (latest - major * 1000 - minor * 100);
                m_VersionContent = new GUIContent(string.Format("You are currently using NeoFPS version: {0}.{1}.{2}", major, minor, revision));
            }

            // Reset settings check
            m_SettingsChecked = false;
        }

        void CheckSettings()
        {
            if (m_SettingsChecked)
                return;

            // Check if warning is required
            m_ShowSettingsWarning = UnitySettingsPage.ShowOutOfDateWarning(out m_ShowSettingsMessage);

            // Get warning or info icon based on above
            try
            {
                if (m_ShowSettingsWarning)
                    m_InfoIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
                else
                    m_InfoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
            }
            catch { }

            m_SettingsChecked = true;
        }

        public override void OnGUI()
        {
            // Show header image
            GUILayout.Space(8);
            if (m_Icon != null)
                GUILayout.Label(m_Icon, EditorStyles.centeredGreyMiniLabel);
            else
                GUILayout.Label("NeoFPS Logo Not Found", EditorStyles.centeredGreyMiniLabel);

            // Show intro
            EditorGUILayout.Space();
            GUILayout.Label(k_IntroMessage, ReadmeEditorUtility.bodyStyleCenter);

            // Show settings warning (if not up to date)
            CheckSettings();
            if (m_ShowSettingsWarning)
            {
                GUILayout.Space(k_SectionSpacing);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                if (m_InfoIcon != null)
                    GUILayout.Label(new GUIContent((m_ShowSettingsWarning) ? m_ShowSettingsMessage : k_SettingsInfoMessage, m_InfoIcon), infoBoxStyle);
                else
                    GUILayout.Label(new GUIContent((m_ShowSettingsWarning) ? m_ShowSettingsMessage : k_SettingsInfoMessage), infoBoxStyle);
                if (GUILayout.Button("Show Unity Settings", settingsButtonStyle))
                    NeoFpsHubEditor.ShowPage("unity_settings");
                GUILayout.EndVertical();
            }

            // Show learning section and links
            GUILayout.Space(k_SectionSpacing);
            GUILayout.Label(k_LearningMessage, ReadmeEditorUtility.bodyStyleCenter);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("Tutorials", k_UrlLearningTutorials);
            GUILayout.Space(20);
            ReadmeEditorUtility.DrawWebLink("Manual", k_UrlLearningManual);
            GUILayout.Space(20);
            ReadmeEditorUtility.DrawWebLink("Scripting Reference", k_UrlLearningScriptingReference);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            // Show community section and links
            GUILayout.Space(k_SectionSpacing);
            GUILayout.Label(k_CommunityMessage, ReadmeEditorUtility.bodyStyleCenter);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("Discord", k_UrlCommunityDiscord);
            GUILayout.Space(20);
            ReadmeEditorUtility.DrawWebLink("Unity Forum", k_UrlCommunityForum);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            // Show support section and links
            GUILayout.Space(k_SectionSpacing);
            GUILayout.Label(k_SupportMessage, ReadmeEditorUtility.bodyStyleCenter);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("Support Website", k_UrlSupportWebsite);
            GUILayout.Space(20);
            ReadmeEditorUtility.DrawWebLink("Support Email", k_UrlSupportMailTo);
            GUILayout.Space(20);
            ReadmeEditorUtility.DrawWebLink("Know Bugs & Issues", k_UrlSupportTrello);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            // Show review section and links
            GUILayout.Space(k_SectionSpacing);
            GUILayout.Label(k_ReviewMessage, ReadmeEditorUtility.bodyStyleCenter);
            GUILayout.Label(k_ReviewNote, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("Review NeoFPS", k_UrlReview);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            // Show version number
            if (m_VersionContent != null)
            {
                GUILayout.Space(k_SectionSpacing);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                GUILayout.Label(m_VersionContent, ReadmeEditorUtility.bodyStyleCenter);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }

            // Show on start toggle
            GUILayout.Space(k_SectionSpacing);
            GUILayout.Label("Startup Settings", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show NeoFPS hub on startup?", GUILayout.Width(200f));
            bool showOnStart = EditorGUILayout.Toggle("", m_ShowOnStart);
            if (showOnStart != m_ShowOnStart)
            {
                m_ShowOnStart = showOnStart;
                NeoFpsEditorPrefs.showHub = showOnStart;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("You can always find this window again via Tools/NeoFPS/NeoFPS Hub", EditorStyles.miniLabel);
        }
    }
}