using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    public class FpsInventoryKeyPopup : EditorWindow
    {
        public static void PickKey(OnPickedDelegate onPicked, OnCancelledDelegate onCancelled)
        {
            if (NeoFpsInventoryDatabase.instance != null)
            {
                var window = GetWindow<FpsInventoryKeyPopup>(true, "Inventory Database Browser", true);
                window.minSize = new Vector2(320, 588);
                window.maxSize = new Vector2(320, 588);
                window.m_OnPicked = onPicked;
                window.m_OnCancelled = onCancelled;
                s_Instance = window;
            }
        }

        public delegate void OnPickedDelegate(int id);
        public delegate void OnCancelledDelegate();

        private static FpsInventoryKeyPopup s_Instance = null;

        private OnPickedDelegate m_OnPicked = null;
        private OnCancelledDelegate m_OnCancelled = null;

        private GUIStyle m_HScrollBar = null;
        private GUIStyle m_VScrollBar = null;
        private Vector2 m_ScrollPosition = Vector2.zero;
        private int m_Selected = 0;
        private bool m_Picked = false;
        private bool m_DoubleClicked = false;
        private FpsInventoryDbTableBase[] m_Tables = null;
        private bool[] m_Expanded = null;

        [SerializeField] private string m_Filter = string.Empty;
        [SerializeField] private string m_NewEntryName = string.Empty;
        [SerializeField] private int m_NewEntryTable = 1;

        private void OnEnable()
        {
            m_Filter = string.Empty;

            // Get the available tables
            var tables = NeoFpsInventoryDatabase.tables;
            if (tables != null)
            {
                // Get valid table count
                int count = 0;
                for (int i = 0; i < tables.Length; ++i)
                {
                    if (tables[i] != null)
                        ++count;
                }

                // Add valid tables to array
                m_Tables = new FpsInventoryDbTableBase[count];
                for (int itr = 0, i = 0; i < tables.Length; ++i)
                {
                    if (tables[i] != null)
                        m_Tables[itr++] = tables[i];
                }

                // Initial expanded states
                m_Expanded = new bool[count];
                for (int i = 0; i < m_Expanded.Length; ++i)
                    m_Expanded[i] = true;
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown)
                m_DoubleClicked = (Event.current.clickCount > 1);

            DrawEntries();
            GUILayout.Space(4);
            DrawPickControls();
            GUILayout.Space(4);
            DrawNewEntryControl();
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDestroy()
        {
            if (m_Picked)
            {
                if (m_OnPicked != null)
                    m_OnPicked(m_Selected);
            }
            else
            {
                if (m_OnCancelled != null)
                    m_OnCancelled();
            }

            s_Instance = null;
        }

        void DrawEntries()
        {
            // Draw filter
            GUILayout.Space(4);
            m_Filter = EditorGUILayout.TextField("Filter", m_Filter);
            GUILayout.Space(2);

            // Check scrollbar styles
            if (m_HScrollBar == null)
                m_HScrollBar = new GUIStyle(GUI.skin.horizontalScrollbar);
            if (m_VScrollBar == null)
                m_VScrollBar = new GUIStyle(GUI.skin.verticalScrollbar);

            bool valid = NeoFpsInventoryDatabase.CheckInstance() && m_Tables != null;
            if (valid)
            {
                // Get lower case filter
                string filter = m_Filter.ToLower();

                // Get length
                int length = 0;
                for (int i = 0; i < m_Tables.Length; ++i)
                {
                    ++length; // One for table name
                    if (m_Expanded[i])
                        length += GetUnfilteredCount(m_Tables[i], filter);
                }

                // Get scroll view rect
                var rect = EditorGUILayout.GetControlRect(false, 400f);
                var contentRect = new Rect(0f, 0f, rect.width - m_VScrollBar.fixedWidth, length * EditorGUIUtility.singleLineHeight + (length + 1) * EditorGUIUtility.standardVerticalSpacing);

                // Begin the scroll view
                m_ScrollPosition = GUI.BeginScrollView(rect, m_ScrollPosition, contentRect, false, true, m_HScrollBar, m_VScrollBar);
                {
                    // Draw the background
                    contentRect.y += m_ScrollPosition.y;
                    contentRect.height = 400f;
                    GUI.Box(contentRect, GUIContent.none, "InnerShadowBg");

                    // Get the first line
                    var line = new Rect(2f, EditorGUIUtility.standardVerticalSpacing, contentRect.width - 3f, EditorGUIUtility.singleLineHeight);

                    // Draw each table
                    for (int i = m_Tables.Length - 1; i >= 0; --i)
                    {
                        if (GUI.Button(line, m_Tables[i].tableName, EditorStyles.boldLabel))
                        {
                            m_Expanded[i] = !m_Expanded[i];
                            throw new ExitGUIException();
                        }
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                        if (m_Expanded[i])
                        {
                            // Draw the relevant entries
                            var entries = m_Tables[i].entries;
                            for (int j = 0; j < entries.Length; ++j)
                            {
                                if (DrawEntry(line, entries[j]))
                                    line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
                GUI.EndScrollView();
            }
            else
                EditorGUILayout.HelpBox("The inventory database does not exist or does not have any tables set up. Cannot pick a key.", MessageType.Error);
        }

        int GetUnfilteredCount(FpsInventoryDbTableBase table, string filter)
        {
            int count = 0;
            for (int i = 0; i < table.entries.Length; ++i)
            {
                if (table.entries[i].displayName.ToLower().Contains(filter))
                    ++count;
            }
            return count;
        }

        bool DrawEntry(Rect rect, FpsInventoryDatabaseEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(m_Filter) && !entry.displayName.ToLower().Contains(m_Filter.ToLower()))
                return false;

            var buttonStyle = EditorStyles.label;
            if (entry.id == m_Selected)
                buttonStyle = EditorStyles.boldLabel;

            if (GUI.Button(rect, entry.displayName, buttonStyle))
            {
                m_Selected = entry.id;

                // Check if double click
                if (m_DoubleClicked)
                {
                    m_Picked = true;
                    Close();
                }
            }

            return true;
        }

        void DrawPickControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current: ", GUILayout.Width(56));
            if (m_Selected == 0)
                EditorGUILayout.LabelField("<None>", EditorStyles.boldLabel);
            else
                EditorGUILayout.LabelField(NeoFpsInventoryDatabase.GetEntryName(m_Selected), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            if (m_Selected == 0)
                GUI.enabled = false;
            if (GUILayout.Button("Pick Selection"))
            {
                m_Picked = true;
                Close();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Pick None"))
            {
                m_Selected = 0;
                m_Picked = true;
                Close();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawNewEntryControl()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("New Entry", EditorStyles.boldLabel);

            bool canAdd = true;

            if (m_Tables.Length > 1)
            {
                var dropdownLabel = (m_NewEntryTable == -1) ? new GUIContent("Select a table") : new GUIContent(m_Tables[m_NewEntryTable].tableName);
                if (EditorGUILayout.DropdownButton(dropdownLabel, FocusType.Passive))
                {
                    var menu = new GenericMenu();

                    for (int i = 1; i < m_Tables.Length; ++i)
                    {
                        if (m_Tables[i] is FpsInventoryDbTable)
                            menu.AddItem(new GUIContent(m_Tables[i].tableName), false, SelectNewEntryTable, i);
                    }

                    menu.ShowAsContext();
                }
            }
            else
            {
                NeoFpsEditorGUI.MiniError("Please add a new database table to the inventory database to allow editing");
                canAdd = false;
            }

            // Get name
            m_NewEntryName = EditorGUILayout.TextField("Name", m_NewEntryName);
            canAdd &= !string.IsNullOrWhiteSpace(m_NewEntryName);
            canAdd &= m_NewEntryTable != -1;

            if (!canAdd)
                GUI.enabled = false;
            if (GUILayout.Button("Add New Entry"))
            {
                // Need a better way to do this!
                m_Selected = FpsInventoryDbTableEditor.AddNewEntry(m_Tables[m_NewEntryTable] as FpsInventoryDbTable, m_NewEntryName);
                m_Picked = true;
                Close();
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        void SelectNewEntryTable(object i)
        {
            m_NewEntryTable = (int)i;
        }
    }
}
