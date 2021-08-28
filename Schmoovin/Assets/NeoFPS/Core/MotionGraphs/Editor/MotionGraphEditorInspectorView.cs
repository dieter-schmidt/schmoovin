using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditorInspectorView
    {
        private readonly char[] k_PathSeparators = new char[] { '/', '\\' };
        private readonly string[] k_TabNames = new string[] {"Parameters", "Motion Data"};

        private Dictionary<int, MotionGraphPropertyDrawer> m_Drawers = new Dictionary<int, MotionGraphPropertyDrawer>();
        private Rect m_InspectorRect = Rect.zero;
        private Rect m_MenuBarRect = Rect.zero;
        private int m_CurrentTab = 0;
        private Vector2 m_Scroll = Vector2.zero;

        public MotionGraphEditor editor { get; private set; }

        public MotionGraphEditorInspectorView (MotionGraphEditor e)
        {
            editor = e;
        }

        public void Reset ()
        {
            ResetParameters();
            ResetData();
        }

        public void Draw (Rect rect)
        {
            m_MenuBarRect = rect;
            m_MenuBarRect.height = 20f;
            m_InspectorRect = rect;
            m_InspectorRect.position += new Vector2 (4f, 20f);
            m_InspectorRect.height -= 20f;
            m_InspectorRect.width -= 8f;

            GUI.Box(rect, "", MotionGraphEditorStyles.inspector);
            GUILayout.BeginArea (m_InspectorRect);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            GUILayout.BeginVertical ();

            EditorGUIUtility.labelWidth = 100f;

            m_CurrentTab = GUILayout.Toolbar (m_CurrentTab, k_TabNames);
            EditorGUILayout.Space ();

            if (editor.currentGraph == null)
                EditorGUILayout.HelpBox ("\nNo motion graph selected for editing.\n\nPlease select one in the project view or using the field at the top of this window.\n", MessageType.Error);
            else {          
                switch (m_CurrentTab) {
                    case 0:
                        DrawParameters();
                        break;
                    case 1:
                        DrawMotionData();
                        break;
                }
            }

            GUILayout.EndVertical ();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea ();

            Handles.BeginGUI ();
            Handles.color = Color.gray;
            Handles.DrawLine (new Vector3 (rect.xMax, 0f, 0f), new Vector3 (rect.xMax, rect.height, 0f));
            Handles.color = Color.white;
            Handles.EndGUI ();

            DrawTopBar ();
        }

        void DrawTopBar ()
        {
            GUILayout.BeginArea (m_MenuBarRect, EditorStyles.toolbar);
            if (editor.currentController != null && Application.isPlaying)
            {
                EditorGUI.LabelField(m_MenuBarRect, "Inspecting run-time graph instance", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUILayout.BeginHorizontal();

                // Only show new & saved controls when not running
                if (!Application.isPlaying)
                {
                    // Create a new motion graph
                    if (GUILayout.Button(new GUIContent("New", "Create a new motion graph asset in the project root directory"), EditorStyles.toolbarButton, GUILayout.Width(40)))
                    {
                        var asset = MotionGraphEditorFactory.CreateMotionGraphAsset();
                        Selection.activeObject = asset;
                    }

                    // Save assets (including currently edited graph)
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent("Clone", "Save the current motion graph"), EditorStyles.toolbarButton, GUILayout.Width(40)))
                        CloneGraph();
                    GUILayout.Space(5);
                }

                // Show currently edited motion graph
                var g = EditorGUILayout.ObjectField("", editor.currentGraph, typeof(MotionGraphContainer), false, GUILayout.Height(14f), GUILayout.Width(192)) as MotionGraphContainer;
                if (editor.currentGraph != g)
                    editor.currentGraph = g;

                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea ();
        }

        void CloneGraph ()
        {
            if (editor.currentGraph == null)
                return;

            // Get unique path
            string path = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(editor.currentGraph));

            // Clone and create asset
            var clone = editor.currentGraph.DeepCopy();
            AssetDatabase.CreateAsset(clone, path);

            // Add subgraphs
            List<MotionGraph> graphs = new List<MotionGraph>();
            clone.rootNode.CollectGraphs(graphs);
            foreach (var e in graphs)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }
            // Add states
            List<MotionGraphState> states = new List<MotionGraphState>();
            clone.rootNode.CollectStates(states);
            foreach (var e in states)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }
            // Add connections
            List<MotionGraphConnection> connections = new List<MotionGraphConnection>();
            clone.rootNode.CollectConnections(connections);
            foreach (var e in connections)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }
            // Add conditions
            List<MotionGraphCondition> conditions = new List<MotionGraphCondition>();
            clone.rootNode.CollectConditions(conditions);
            foreach (var e in conditions)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }
            // Add behaviours
            List<MotionGraphBehaviour> behaviours = new List<MotionGraphBehaviour>();
            clone.rootNode.CollectBehaviours(behaviours);
            foreach (var e in behaviours)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }

            // Add properties
            List<MotionGraphParameter> parameters = new List<MotionGraphParameter>();
            clone.CollectParameters(parameters);
            foreach (var e in parameters)
            {
                AssetDatabase.AddObjectToAsset(e, clone);
                e.hideFlags |= HideFlags.HideInInspector;
            }
            // Add data
            List<MotionGraphDataBase> data = new List<MotionGraphDataBase>();
            clone.CollectData(data);
            foreach (var e in data)
            {
                if (e == null)
                {
                    Debug.LogError("Null data entry");
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(e, clone);
                    e.hideFlags |= HideFlags.HideInInspector;
                }
            }

            // Save
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(clone);
        }

        public void ProcessEvent (Event e)
        {
        }

        #region PARAMETERS

        private ReorderableList m_ParameterList;

        void ResetParameters ()
        {
            m_Drawers.Clear();
                       
            if (editor.currentGraph == null)
                m_ParameterList = null;
            else
            {
                var so = new SerializedObject(editor.currentGraph);
                m_ParameterList = new ReorderableList (
                    so,
                    so.FindProperty ("m_Parameters"),
                    true,
                    true,
                    true,
                    true
                );
                m_ParameterList.drawHeaderCallback = DrawParameterListHeader;
                m_ParameterList.drawElementCallback = DrawParameterListElements;
                m_ParameterList.onAddDropdownCallback = OnParameterListAddDropdown;
                m_ParameterList.onRemoveCallback = OnParameterListRemoved;
                m_ParameterList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            }
        }
        
        void DrawParameters ()
        {
            if (editor.currentGraph == null)
                Reset();

            if (m_ParameterList == null)
                ResetParameters();
            else
            {
                var so = m_ParameterList.serializedProperty.serializedObject;
                if (so.targetObject != null)
                    so.UpdateIfRequiredOrScript();
                else
                    ResetParameters();
            }

            m_ParameterList.DoLayoutList();
        }

        void DrawParameterListHeader (Rect rect)
        {
            EditorGUI.LabelField (rect, "Parameters");

            // Draw help button and link to docs
            rect.x += rect.width - 13;
            rect.width = 16;
            rect.height = 15;
            if (GUI.Button(rect, "?", EditorStyles.miniButton))
                Application.OpenURL("http://docs.neofps.com/manual/motiongraph-parameters.html");
        }

        void OnParameterListAddDropdown (Rect buttonRect, ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            var menu = MotionGraphEditorFactory.GetParametersMenu(list.serializedProperty.serializedObject);
            menu.ShowAsContext ();
        }

        void OnParameterListRemoved (ReorderableList list)
        {
            list.serializedProperty.serializedObject.UpdateIfRequiredOrScript();

            // Get the parameter to remove
            var parameter = SerializedArrayUtility.GetItemAtIndex<MotionGraphParameter>(list.serializedProperty, list.index);

            // Remove from the list
            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
            list.index = -1;

            // Reset the parameter drawers
            m_Drawers.Clear();

            // Destroy the old parameter
            Undo.DestroyObjectImmediate(parameter);
        }
        
        void DrawParameterListElements (Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 1;
            rect.height -= 4;

            var obj = m_ParameterList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;
            if (obj == null)
            {
                EditorGUI.LabelField(rect, "Missing Parameter");
                return;
            }

            MotionGraphPropertyDrawer drawer;
            if (!m_Drawers.TryGetValue(obj.GetInstanceID(), out drawer))
                drawer = MotionGraphEditorFactory.GetPropertyDrawer(obj, editor);

            if (drawer == null)
            {
                EditorGUI.LabelField(rect, "Parameter Drawer Not Found");
                return;
            }

            drawer.Draw(rect);
        }

        #endregion

        #region MOTION DATA

        private ReorderableList m_DataList;

        void ResetData()
        {
            if (editor.currentGraph == null)
                m_DataList = null;
            else
            {
                var so = new SerializedObject(editor.currentGraph);
                m_DataList = new ReorderableList(
                    so,
                    so.FindProperty("m_Data"),
                    true,
                    true,
                    true,
                    true
                );
                m_DataList.drawHeaderCallback = DrawDataListHeader;
                m_DataList.drawElementCallback = DrawDataListElements;
                m_DataList.onAddDropdownCallback = OnDataListAddDropdown;
                m_DataList.onRemoveCallback = OnDataListRemoved;
                m_DataList.elementHeightCallback = GetDataListElementHeight;
            }
        }

        void DrawMotionData()
        {
            if (m_DataList == null)
                ResetData();

            if (!editor.editingRuntimeGraph)
            {
                if (GUILayout.Button("Create Override Asset"))
                    CreateDataOverrideScriptableObject();
                EditorGUILayout.Space();
            }

            m_DataList.serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            m_DataList.DoLayoutList();
            m_DataList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        void DrawDataListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Data");

            // Draw help button and link to docs
            rect.x += rect.width - 13;
            rect.width = 16;
            rect.height = 15;
            if (GUI.Button(rect, "?", EditorStyles.miniButton))
                Application.OpenURL("http://docs.neofps.com/manual/motiongraph-data.html");
        }

        void OnDataListAddDropdown(Rect buttonRect, ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            var menu = MotionGraphEditorFactory.GetDataEntryMenu(list.serializedProperty.serializedObject);
            menu.ShowAsContext();
        }

        void OnDataListRemoved(ReorderableList list)
        {
            list.serializedProperty.serializedObject.UpdateIfRequiredOrScript();

            // Get the data entry to remove
            var data = SerializedArrayUtility.GetItemAtIndex<MotionGraphDataBase>(list.serializedProperty, list.index);

            // Remove from the list
            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
            list.index = -1;

            // Reset the motion data drawers
            m_Drawers.Clear();

            // Destroy the old data entry
            Undo.DestroyObjectImmediate(data);
        }

        private float GetDataListElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }

        void DrawDataListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 1;
            rect.height -= 4;

            var obj = m_DataList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;
            if (obj == null)
            {
                EditorGUI.LabelField(rect, "Missing Data");
                return;
            }

            MotionGraphPropertyDrawer drawer;
            if (!m_Drawers.TryGetValue(obj.GetInstanceID(), out drawer))
                drawer = MotionGraphEditorFactory.GetPropertyDrawer(obj, editor);

            if (drawer == null)
            {
                EditorGUI.LabelField(rect, "Data Drawer Not Found");
                return;
            }

            drawer.Draw(rect);
        }

        void CreateDataOverrideScriptableObject()
        {
            // Derive asset path from graph asset
            string path = AssetDatabase.GetAssetPath(editor.currentGraph);
            path = path.Replace(".asset", "DataOverride.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            // Create the override
            var asset = ScriptableObject.CreateInstance<MotionGraphDataOverrideAsset>();

            // Assign the new name
            var tokens = path.Split(k_PathSeparators);
            asset.name = tokens[tokens.Length - 2];

            // Assign the graph
            var assetSO = new SerializedObject(asset);
            assetSO.FindProperty("m_Graph").objectReferenceValue = editor.currentGraph;
            assetSO.ApplyModifiedProperties();
            asset.CheckOverrides();

            // Create the asset file
            AssetDatabase.CreateAsset(asset, path);
        }

        #endregion
    }
}