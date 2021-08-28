using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class DemoScenesPage : HubPage
    {
        private List<ScenesListEntry> m_Scenes = new List<ScenesListEntry>();

        private GUIStyle m_CategoryFoldout = null;
        public GUIStyle categoryFoldout
        {
            get
            {
                if (m_CategoryFoldout == null)
                {
                    m_CategoryFoldout = new GUIStyle(EditorStyles.boldLabel);
                    m_CategoryFoldout.fontSize = 20;
                    m_CategoryFoldout.fixedHeight = 28;
                }
                return m_CategoryFoldout;
            }
        }

        private class ScenesListEntry
        {
            public DemoScenesList list = null;
            public bool expanded = true;

            public ScenesListEntry(DemoScenesList l)
            {
                list = l;
            }
        }

        private ReadmeHeader m_Heading = null;
        public ReadmeHeader heading
        {
            get
            {
                if (m_Heading == null)
                    m_Heading = new ReadmeHeader(LoadIcon("EditorImage_NeoFpsIconRound", "EditorImage_NeoFpsCrosshair"), pageHeader);
                return m_Heading;
            }
        }

        public override string pageHeader
        {
            get { return "Demo Scenes"; }
        }

        public override void Awake()
        {
            var guids = AssetDatabase.FindAssets("t:DemoScenesList");
            if (guids == null)
                return;

            // Load scenes lists
            for (int i = 0; i < guids.Length; ++i)
            {
                var scenesList = AssetDatabase.LoadAssetAtPath<DemoScenesList>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (scenesList != null)
                    m_Scenes.Add(new ScenesListEntry(scenesList));
            }

            // Sort scenes lists
            m_Scenes.Sort((ScenesListEntry lhs, ScenesListEntry rhs) => { return rhs.list.priority.CompareTo(lhs.list.priority); });
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();

            // Check for assets setting
            if (m_Scenes == null)
            {
                EditorGUILayout.HelpBox("Scenes list asset not found", MessageType.Error);
                return;
            }

            foreach (var list in m_Scenes)
            {
                if (GUILayout.Button(list.expanded ? "– " + list.list.category : "+ " + list.list.category, categoryFoldout))
                    list.expanded = !list.expanded;

                if (list.expanded)
                {
                    // Draw list
                    foreach (var s in list.list.scenes)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                        var textureRect = EditorGUILayout.BeginVertical(GUILayout.Width(240), GUILayout.Height(135));
                        GUILayout.Label(GUIContent.none, GUILayout.Width(240), GUILayout.Height(135));
                        GUI.DrawTexture(textureRect, s.thumbnail);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.LabelField(s.title, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(s.description, EditorStyles.wordWrappedLabel);

                        if (GUILayout.Button("Load Demo Scene"))
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                var guids = AssetDatabase.FindAssets("t:SceneAsset " + s.loadName);
                                if (guids != null && guids.Length > 0)
                                {
                                    EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]));
                                    NeoFpsHubEditor.ShowPage("demo_info");
                                }
                                else
                                    Debug.LogError("Scene not found: " + s.loadName);
                            }
                        }

                        if (GUILayout.Button("Select Scene In Project Hierarchy"))
                        {
                            var guids = AssetDatabase.FindAssets("t:SceneAsset " + s.loadName);
                            if (guids != null && guids.Length > 0)
                            {
                                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                                EditorGUIUtility.PingObject(asset.GetInstanceID());
                            }
                            else
                                Debug.LogError("Scene not found: " + s.loadName);
                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(4);
                    }
                }
                EditorGUILayout.Space();
            }
        }
    }
}
