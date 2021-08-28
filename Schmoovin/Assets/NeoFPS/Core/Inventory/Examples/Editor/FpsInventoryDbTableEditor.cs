using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FpsInventoryDbTable), true)]
    public class FpsInventoryDbTableEditor : Editor
    {
        private GUIStyle m_HScrollBar = null;
        private GUIStyle m_VScrollBar = null;
        private GUIStyle m_OptionsButton = null;
        private GUIContent m_OptionsContent = null;
        private Vector2 m_ScrollPosition = Vector2.zero;

        private string m_NewEntryName = string.Empty;
        private string m_Filter = string.Empty;
        //private int m_NewEntryMaxQuantity = 1;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            CheckIfRegistered();

            NeoFpsEditorGUI.RequiredStringField(serializedObject.FindProperty("m_TableName"));

            NeoFpsEditorGUI.Separator();

            DrawEntries();
            EditorGUILayout.Space();
            DrawNewEntryControl();
            EditorGUILayout.Space();
            DrawManageDatabaseControls();

            serializedObject.ApplyModifiedProperties();
        }

        void CheckIfRegistered()
        {
            var table = target as FpsInventoryDbTableBase;
            var tables = NeoFpsInventoryDatabase.tables;
            for (int i = 0; i < tables.Length; ++i)
            {
                if (tables[i] == table)
                    return;
            }

            EditorGUILayout.HelpBox("This database table is not currently registered with the inventory database. Use the button below to register the table now.", MessageType.Warning);
            if (GUILayout.Button ("Register Table With Database"))
            {
                var so = new SerializedObject(NeoFpsInventoryDatabase.instance);
                var t = so.FindProperty("m_Tables");
                SerializedArrayUtility.Add(t, table, false);
                so.ApplyModifiedProperties();
            }
            
            NeoFpsEditorGUI.Separator();
        }

        void DrawEntries()
        {
            // Draw filter
            m_Filter = EditorGUILayout.TextField("Filter", m_Filter);
            GUILayout.Space(4);

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
            var entries = serializedObject.FindProperty("m_Entries");

            // For headers, can use the GUIStyle: (GUIStyle)"dockareaOverlay"

            // Get scroll view rect
            var rect = EditorGUILayout.GetControlRect(false, 400f);
            var contentRect = new Rect(0f, 0f, rect.width - m_VScrollBar.fixedWidth, entries.arraySize * EditorGUIUtility.singleLineHeight + (entries.arraySize + 1) * EditorGUIUtility.standardVerticalSpacing);

            // Begin the scroll view
            m_ScrollPosition = GUI.BeginScrollView(rect, m_ScrollPosition, contentRect, false, true, m_HScrollBar, m_VScrollBar);
            {
                // Draw the background
                contentRect.y += m_ScrollPosition.y;
                contentRect.height = 400f;
                GUI.Box(contentRect, GUIContent.none, "InnerShadowBg");

                // Get the first line
                var line = new Rect(2f, EditorGUIUtility.standardVerticalSpacing, contentRect.width - 3f, EditorGUIUtility.singleLineHeight);

                // Draw the relevant entries
                for (int i = 0; i < entries.arraySize; ++i)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    if (DrawEntry(line, entry, i))
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            GUI.EndScrollView();
        }

        bool DrawEntry (Rect rect, SerializedProperty entry, int index)
        {
            var displayName = entry.FindPropertyRelative("m_DisplayName");

            if (!string.IsNullOrWhiteSpace(m_Filter) && !displayName.stringValue.ToLower().Contains(m_Filter.ToLower()))
                return false;

            Rect r1 = rect;
            //r1.width *= 0.75f;
            r1.width -= EditorGUIUtility.singleLineHeight + 4f;

            EditorGUI.PropertyField(r1, displayName, GUIContent.none);

            //r1.x += r1.width + 2f;
            //r1.width = rect.width * 0.25f - EditorGUIUtility.singleLineHeight - 4f;

            //EditorGUI.PropertyField(r1, entry.FindPropertyRelative("m_MaxQuantity"), GUIContent.none);

            r1.x = rect.x + rect.width - EditorGUIUtility.singleLineHeight;
            r1.width = EditorGUIUtility.singleLineHeight;
            r1.y += 2;
            r1.width -= 4;
            r1.height -= 4;

            // Show options menu
            if (GUI.Button(r1, m_OptionsContent, m_OptionsButton))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy ID"), false, (i) =>
                {
                    var prop = serializedObject.FindProperty("m_Entries").GetArrayElementAtIndex((int)i);
                    int ID = prop.FindPropertyRelative("m_Id").intValue;
                    string idString = ID.ToString();
                    Debug.Log(idString);
                    GUIUtility.systemCopyBuffer = idString;
                },
                index);
                menu.AddItem(new GUIContent("Remove Item"), false, (i) =>
                {
                    SerializedArrayUtility.RemoveAt(serializedObject.FindProperty("m_Entries"), (int)i);
                    serializedObject.ApplyModifiedProperties();
                },
                index);
                menu.AddItem(new GUIContent("Duplicate Item"), false, (i) =>
                {
                    int sourceIndex = (int)i;
                    var entries = serializedObject.FindProperty("m_Entries");

                    // Get a UID:
                    int id = GetUniqueID(entries);

                    // Add to array
                    int last = entries.arraySize++;

                    // Get elements
                    var original = entries.GetArrayElementAtIndex(sourceIndex);
                    var duplicate = entries.GetArrayElementAtIndex(last);

                    // Initialise duplicate
                    duplicate.FindPropertyRelative("m_DisplayName").stringValue = original.FindPropertyRelative("m_DisplayName").stringValue;
                    //duplicate.FindPropertyRelative("m_MaxQuantity").intValue = original.FindPropertyRelative("m_MaxQuantity").intValue;
                    duplicate.FindPropertyRelative("m_Id").intValue = id;

                    // Move to index after original
                    SerializedArrayUtility.Move(serializedObject.FindProperty("m_Entries"), last, sourceIndex + 1);

                    // Apply
                    serializedObject.ApplyModifiedProperties();
                },
                index);
                menu.ShowAsContext();
            }

            return true;
        }

        void DrawNewEntryControl()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("New Entry", EditorStyles.boldLabel);

            m_NewEntryName = EditorGUILayout.TextField("Name", m_NewEntryName);
            //m_NewEntryMaxQuantity = EditorGUILayout.IntField("Max Quantity", m_NewEntryMaxQuantity);

            if (string.IsNullOrWhiteSpace(m_NewEntryName))
                GUI.enabled = false;
            if (GUILayout.Button("Add New Entry"))
            {
                var cast = target as FpsInventoryDbTable;
                AddNewEntry(cast, m_NewEntryName);//, 1);
                m_NewEntryName = string.Empty;
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        void DrawManageDatabaseControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Manage Table", EditorStyles.boldLabel);
            
            // Sort by name (ascending)
            if (GUILayout.Button("Sort by Name (Ascending)"))
            {
                var entries = serializedObject.FindProperty("m_Entries");
                SortEntriesByName(entries, true);
                serializedObject.ApplyModifiedProperties();
            }

            // Sort by name (descending)
            if (GUILayout.Button("Sort by Name (Descending)"))
            {
                var entries = serializedObject.FindProperty("m_Entries");
                SortEntriesByName(entries, false);
                serializedObject.ApplyModifiedProperties();
            }

            // Clear
            if (GUILayout.Button("Clear Database"))
            {
                var entries = serializedObject.FindProperty("m_Entries");
                entries.ClearArray();
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }

        class EntryIntermediate
        {
            public string displayName = null;
            public int id = 0;

            public EntryIntermediate(string displayName, int id)
            {
                this.displayName = displayName;
                this.id = id;
            }
        }

        static void SortEntriesByName(SerializedProperty entries, bool ascending)
        {
            List<EntryIntermediate> intermediates = new List<EntryIntermediate>();

            // Build intermediates list
            for (int i = 0; i < entries.arraySize; ++i)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                intermediates.Add(new EntryIntermediate(
                    entry.FindPropertyRelative("m_DisplayName").stringValue,
                    entry.FindPropertyRelative("m_Id").intValue
                    ));
            }

            // Sort
            if (ascending)
                intermediates.Sort((x, y) => { return string.Compare(x.displayName, y.displayName); });
            else
                intermediates.Sort((x, y) => { return -string.Compare(x.displayName, y.displayName); });

            // Reapply
            for (int i = 0; i < entries.arraySize; ++i)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("m_DisplayName").stringValue = intermediates[i].displayName;
                entry.FindPropertyRelative("m_Id").intValue = intermediates[i].id;
            }
        }

        static void MergeEntries(SerializedProperty entries, SerializedProperty source)
        {
            // Iterate through entries in source database
            for (int i = 0; i < source.arraySize; ++i)
            {
                // Get source entry
                var sourceEntry = source.GetArrayElementAtIndex(i);
                int id = sourceEntry.FindPropertyRelative("m_Id").intValue;

                // Check for duplicate ID in destination
                string duplicate = null;
                for (int j = 0; j < entries.arraySize; ++j)
                {
                    var e = entries.GetArrayElementAtIndex(j);
                    if (e.FindPropertyRelative("m_Id").intValue == id)
                    {
                        duplicate = e.FindPropertyRelative("m_DisplayName").stringValue;
                        break;
                    }
                }

                // Log duplicates but don't merge
                if (duplicate != null)
                    Debug.Log(string.Format("Inventory database already found for key [{0}]: {1}.", id, duplicate));
                else
                {
                    // Add new entry and copy source entry properties across
                    ++entries.arraySize;
                    var newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);

                    newEntry.FindPropertyRelative("m_Id").intValue = id;
                    newEntry.FindPropertyRelative("m_DisplayName").stringValue = sourceEntry.FindPropertyRelative("m_DisplayName").stringValue;
                    //newEntry.FindPropertyRelative("m_MaxQuantity").intValue = sourceEntry.FindPropertyRelative("m_MaxQuantity").intValue;
                }
            }
        }

        public static int AddNewEntry (FpsInventoryDbTable database, string displayName)//, int maxQuantity)
        {
            var so = new SerializedObject(database);

            var entries = so.FindProperty("m_Entries");
            int id = GetUniqueID(entries);

            int last = entries.arraySize++;

            var newEntry = entries.GetArrayElementAtIndex(last);
            newEntry.FindPropertyRelative("m_DisplayName").stringValue = displayName;
            //newEntry.FindPropertyRelative("m_MaxQuantity").intValue = maxQuantity;

            newEntry.FindPropertyRelative("m_Id").intValue = id;

            so.ApplyModifiedProperties();

            return id;
        }
        
        static int GetUniqueID (SerializedProperty entries)
        {
            int id = 0;
            while (id == 0)
            {
                id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                for (int i = 0; i < entries.arraySize; ++i)
                {
                    var e = entries.GetArrayElementAtIndex(i);
                    if (e.FindPropertyRelative("m_Id").intValue == id)
                    {
                        id = 0;
                        break;
                    }
                }
            }

            return id;
        }
    }
}