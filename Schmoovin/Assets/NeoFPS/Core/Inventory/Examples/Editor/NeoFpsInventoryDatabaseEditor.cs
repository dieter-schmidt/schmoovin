using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(NeoFpsInventoryDatabase), true)]
    public class NeoFpsInventoryDatabaseEditor : Editor
    {
        private GUIStyle m_HScrollBar = null;
        private GUIStyle m_VScrollBar = null;
        private GUIStyle m_OptionsButton = null;
        private GUIContent m_OptionsContent = null;
        private Vector2 m_ScrollPosition = Vector2.zero;
        private FpsInventoryDbTableBase m_SelectedTable = null;
        private Editor m_SelectedTableEditor = null;
        private int m_SelectedTableIndex = -1;

        [MenuItem("Assets/Create/NeoFPS/Managers/Inventory Database", priority = NeoFpsMenuPriorities.manager_inventory)]
        public static void CreateMyAsset()
        {
            string filePath;
            if (Selection.assetGUIDs.Length == 0)
                filePath = "Assets/NeoFPS/Resources/FpsManager_InventoryDatabase.asset";
            else
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]) + "/FpsManager_InventoryDatabase.asset";

            // Create the database asset
            var database = CreateInstance<NeoFpsInventoryDatabase>();
            AssetDatabase.CreateAsset(database, filePath);

            // Create the key constants table
            var keyTable = CreateInstance<FpsInventoryKeyDbTable>();
            keyTable.name = "FpsInventoryKey Constants";
            AssetDatabase.AddObjectToAsset(keyTable, database);

            var databaseSO = new SerializedObject(database);
            SerializedArrayUtility.Add(databaseSO.FindProperty("m_Tables"), keyTable, true);
            databaseSO.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = database;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawTableList();
            EditorGUILayout.Space();
            DrawSelectTableEditor();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawTableList()
        {
            // Check scrollbar styles
            if (m_HScrollBar == null)
                m_HScrollBar = new GUIStyle(GUI.skin.horizontalScrollbar);
            if (m_VScrollBar == null)
                m_VScrollBar = new GUIStyle(GUI.skin.verticalScrollbar);
            if (m_OptionsButton == null)
                m_OptionsButton = new GUIStyle();
            if (m_OptionsContent == null)
            {
                var guids = AssetDatabase.FindAssets("EditorImage_OptionsIcon");
                if (guids != null && guids.Length > 0)
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    m_OptionsContent = new GUIContent(tex);
                }
            }

            // Get the entries property
            var tables = serializedObject.FindProperty("m_Tables");

            // Draw add tables field
            var newTable = EditorGUILayout.ObjectField("Add Table Asset", null, typeof(FpsInventoryDbTableBase), false);
            if (newTable != null)
                SerializedArrayUtility.Add(tables, newTable, false);
            EditorGUILayout.Space();

            // Get scroll view rect
            var rect = EditorGUILayout.GetControlRect(false, 200f);
            var contentRect = new Rect(0f, 0f, rect.width - m_VScrollBar.fixedWidth, tables.arraySize * EditorGUIUtility.singleLineHeight + (tables.arraySize + 1) * EditorGUIUtility.standardVerticalSpacing);

            // Begin the scroll view
            m_ScrollPosition = GUI.BeginScrollView(rect, m_ScrollPosition, contentRect, false, true, m_HScrollBar, m_VScrollBar);
            {
                // Draw the background
                contentRect.y += m_ScrollPosition.y;
                contentRect.height = 200f;
                GUI.Box(contentRect, GUIContent.none, "InnerShadowBg");

                // Get the first line
                var line = new Rect(2f, EditorGUIUtility.standardVerticalSpacing, contentRect.width - 3f, EditorGUIUtility.singleLineHeight);

                // Draw the relevant tables
                for (int i = 0; i < tables.arraySize; ++i)
                {
                    // Draw table entry
                    var table = tables.GetArrayElementAtIndex(i).objectReferenceValue as FpsInventoryDbTableBase;
                    if (table != null)
                    {
                        var label = table.tableName;
                        if (string.IsNullOrWhiteSpace(label))
                            label = table.name + "(Missing Table Name)";

                        // Shorten line to make space for options
                        Rect r1 = line;
                        r1.width -= EditorGUIUtility.singleLineHeight + 4f;

                        if (GUI.Button(r1, label, m_SelectedTableIndex == i ? EditorStyles.boldLabel : EditorStyles.label))
                            SetSelectedTable(table, i);

                        if (i != 0)
                        {
                            // Options button rect
                            r1.x = line.x + line.width - EditorGUIUtility.singleLineHeight;
                            r1.width = EditorGUIUtility.singleLineHeight;
                            r1.y += 2;
                            r1.width -= 4;
                            r1.height -= 4;

                            // Show options menu
                            if (GUI.Button(r1, m_OptionsContent, m_OptionsButton))
                            {
                                var menu = new GenericMenu();

                                // Ping table
                                menu.AddItem(new GUIContent("Highlight Asset"), false, (index) =>
                                {
                                    var obj = serializedObject.FindProperty("m_Tables").GetArrayElementAtIndex((int)index).objectReferenceValue;
                                    EditorGUIUtility.PingObject(obj);
                                },
                                i);

                                // Remove table
                                menu.AddItem(new GUIContent("Remove Table"), false, (index) =>
                                {
                                    SerializedArrayUtility.RemoveAt(serializedObject.FindProperty("m_Tables"), (int)index);
                                    serializedObject.ApplyModifiedProperties();
                                },
                                i);

                                // Move up
                                if (i < 2)
                                    menu.AddDisabledItem(new GUIContent("Move Up"), false);
                                else
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (index) =>
                                    {
                                        int from = (int)index;
                                        SerializedArrayUtility.Move(serializedObject.FindProperty("m_Tables"), from, from - 1);
                                        serializedObject.ApplyModifiedProperties();
                                    },
                                    i);
                                }

                                // Move down
                                if (i >= tables.arraySize - 1)
                                    menu.AddDisabledItem(new GUIContent("Move Down"), false);
                                else
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (index) =>
                                    {
                                        int from = (int)index;
                                        SerializedArrayUtility.Move(serializedObject.FindProperty("m_Tables"), from, from + 1);
                                        serializedObject.ApplyModifiedProperties();
                                    },
                                    i);
                                }

                                menu.ShowAsContext();
                            }
                        }
                    }
                    else
                        EditorGUI.HelpBox(line, "Missing Table", MessageType.Error);

                    line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            GUI.EndScrollView();
        }

        void DrawSelectTableEditor()
        {
            if (m_SelectedTableEditor != null)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Selected Table", EditorStyles.boldLabel);
                    EditorGUILayout.Space();
                    m_SelectedTableEditor.OnInspectorGUI();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a table above to inspect its contents", MessageType.Info);
            }
        }

        void SetSelectedTable(FpsInventoryDbTableBase table, int index)
        {
            m_SelectedTableIndex = index;
            if (m_SelectedTable != table)
            {
                m_SelectedTable = table;
                m_SelectedTableEditor = CreateEditor(table);
            }
        }
    }
}
