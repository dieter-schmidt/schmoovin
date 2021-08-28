using NeoFPS.ModularFirearms;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ModularFirearmModeSwitcher), true)]
    public class ModularFirearmModeSwitcherEditor : Editor
    {
        private List<ModeEntry> m_Entries = new List<ModeEntry>();

        [SerializeField] private ModuleType m_FilterModules = ModuleType.Aimer | ModuleType.Trigger | ModuleType.Shooter | ModuleType.Reloader | ModuleType.Ammo | ModuleType.Recoil | ModuleType.MuzzleEffect | ModuleType.Ejector;

        [Flags]
        public enum ModuleType
        {
            Aimer = 1,
            Trigger = 2,
            Shooter = 4,
            Reloader = 8,
            Ammo = 16,
            Recoil = 32,
            MuzzleEffect = 64,
            Ejector = 128
        }

        public class ModeEntry
        {
            private ModularFirearmModeSwitcherEditor m_Editor = null;
            private SubInspectorTitlebar m_Titlebar = null;
            private SerializedProperty m_ModesProp = null;
            private int m_Index = 0;
            private ReorderableList m_ComponentList = null;

            static List<Component> s_ComponentSearch = new List<Component>();

            static int GetComponentIndex(Component c)
            {
                // Get similar components
                var t = c.GetType();
                c.gameObject.GetComponents(t, s_ComponentSearch);

                // Check
                int result = 0;
                for (int i = 0; i < s_ComponentSearch.Count; ++i)
                {
                    if (s_ComponentSearch[i] == c)
                        return result;
                    else
                    {
                        // Only increment for same type, not children
                        if (s_ComponentSearch[i].GetType() == t)
                            ++result;
                    }
                }

                // Reset
                s_ComponentSearch.Clear();
                return 0;
            }

            public ModeEntry(ModularFirearmModeSwitcherEditor editor, int i, SerializedProperty modesProp)
            {
                m_Editor = editor;
                m_Index = i;
                m_ModesProp = modesProp;
                m_Titlebar = new SubInspectorTitlebar(true);
                m_Titlebar.getLabel = GetTitle;
                m_Titlebar.AddContextOption("Move Up", OnMenuMoveUp, CanMenuMoveUp);
                m_Titlebar.AddContextOption("Move Down", OnMenuMoveDown, CanMenuMoveDown);
                m_Titlebar.AddContextOption("Remove", OnMenuRemove, null);
                m_ComponentList = new ReorderableList(modesProp.serializedObject, modesProp.GetArrayElementAtIndex(i).FindPropertyRelative("components"), true, true, true, true);
                m_ComponentList.drawElementCallback = DrawComponentListElement;
                m_ComponentList.drawHeaderCallback = DrawComponentListHeader;
                m_ComponentList.onAddDropdownCallback = OnAddDropdown;
            }

            private void DrawComponentListHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, "Components");
            }

            private void DrawComponentListElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.y += 1;
                rect.height -= 1;
                var componentProp = m_ComponentList.serializedProperty.GetArrayElementAtIndex(index);
                if (componentProp.objectReferenceValue == null)
                    EditorGUI.LabelField(rect, "<Missing>");
                else
                {
                    var c = componentProp.objectReferenceValue as Component;
                    var cast = m_Editor.target as ModularFirearmModeSwitcher;

                    string label = string.Empty;

                    // Add object name
                    if (cast.gameObject != c.gameObject)
                        label += c.gameObject.name + "/";

                    // Add component name
                    label += c.GetType().Name;

                    // Add component index
                    int compIndex = GetComponentIndex(c);
                    if (compIndex > 0)
                        label += string.Format(" [{0}]", compIndex);

                    EditorGUI.LabelField(rect, new GUIContent(label, label));
                }
            }

            private void OnAddDropdown(Rect buttonRect, ReorderableList list)
            {
                var gameobject = (m_Editor.target as Component).gameObject;
                var componentsProp = m_ComponentList.serializedProperty;

                // Gather components based on filter
                List<Component> gathered = new List<Component>();
                if ((m_Editor.m_FilterModules & ModuleType.Aimer) == ModuleType.Aimer)
                   gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IAimer)));
                if ((m_Editor.m_FilterModules & ModuleType.Trigger) == ModuleType.Trigger)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(ITrigger)));
                if ((m_Editor.m_FilterModules & ModuleType.Shooter) == ModuleType.Shooter)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IShooter)));
                if ((m_Editor.m_FilterModules & ModuleType.Reloader) == ModuleType.Reloader)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IReloader)));
                if ((m_Editor.m_FilterModules & ModuleType.Ammo) == ModuleType.Ammo)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IAmmo)));
                if ((m_Editor.m_FilterModules & ModuleType.Recoil) == ModuleType.Recoil)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IRecoilHandler)));
                if ((m_Editor.m_FilterModules & ModuleType.MuzzleEffect) == ModuleType.MuzzleEffect)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IMuzzleEffect)));
                if ((m_Editor.m_FilterModules & ModuleType.Ejector) == ModuleType.Ejector)
                    gathered.AddRange(gameobject.GetComponentsInChildren(typeof(IEjector)));

                gathered.Sort((x, y) => {
                    if (x.gameObject == gameobject && y.gameObject != gameobject)
                        return 1;
                    if (x.gameObject != gameobject && y.gameObject == gameobject)
                        return -1;
                    if (x.gameObject != y.gameObject)
                        return string.Compare(x.gameObject.name, y.gameObject.name);
                    return string.Compare(x.GetType().Name, y.GetType().Name);
                });

                // Add menu entries
                var menu = new GenericMenu();
                foreach (var entry in gathered)
                {
                    if (!SerializedArrayUtility.Contains(componentsProp, entry))
                    {
                        string path = string.Empty;

                        // Add object name
                        if (entry.gameObject != gameobject)
                            path += entry.gameObject.name + "/";

                        // Add component name
                        path += entry.GetType().Name;

                        // Add component index
                        int compIndex = GetComponentIndex(entry);
                        if (compIndex > 0)
                            path += string.Format(" [{0}]", compIndex);

                        menu.AddItem(new GUIContent(path), false, OnComponentSelected, entry);
                    }
                }

                // Show the menu
                menu.ShowAsContext();
            }

            private void OnComponentSelected(object o)
            {
                SerializedArrayUtility.Add(m_ComponentList.serializedProperty, (UnityEngine.Object)o, true);
                m_ModesProp.serializedObject.ApplyModifiedProperties();
            }

            string GetTitle()
            {
                var title = m_ModesProp.GetArrayElementAtIndex(m_Index).FindPropertyRelative("descriptiveName").stringValue;
                if (string.IsNullOrEmpty(title))
                    return "<Unnamed>";
                else
                    return title;
            }

            public void DoLayout()
            {
                if (m_Titlebar.DoLayout())
                {
                    var modeProp = m_ModesProp.GetArrayElementAtIndex(m_Index);

                    EditorGUILayout.PropertyField(modeProp.FindPropertyRelative("descriptiveName"), true);
                    m_ComponentList.DoLayoutList();
                }
            }

            void OnMenuMoveUp()
            {
                SerializedArrayUtility.Move(m_ModesProp, m_Index, m_Index - 1);
                m_ModesProp.serializedObject.ApplyModifiedProperties();
            }

            void OnMenuMoveDown()
            {
                SerializedArrayUtility.Move(m_ModesProp, m_Index, m_Index + 1);
                m_ModesProp.serializedObject.ApplyModifiedProperties();
            }

            bool CanMenuMoveUp()
            {
                return m_Index > 0;
            }

            bool CanMenuMoveDown()
            {
                return m_Index < m_ModesProp.arraySize - 1;
            }

            void OnMenuRemove()
            {
                SerializedArrayUtility.RemoveAt(m_ModesProp, m_Index);
                m_ModesProp.serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSwitchModes"));

            // Get the modes property
            var modesProp = serializedObject.FindProperty("m_Modes");

            if (GUILayout.Button("Add Mode"))
            {
                ++modesProp.arraySize;
                var newModeProp = modesProp.GetArrayElementAtIndex(modesProp.arraySize - 1);
                newModeProp.FindPropertyRelative("descriptiveName").stringValue = string.Empty;
                newModeProp.FindPropertyRelative("components").arraySize = 0;
                serializedObject.ApplyModifiedProperties();
            }

            m_FilterModules = (ModuleType)EditorGUILayout.EnumFlagsField("Filter Module Types", m_FilterModules);

            GUILayout.Space(4);
            GUILayout.Label("* Hover over the entries to see their full component path", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(4);

            for (int i = 0; i < modesProp.arraySize; ++i)
            {
                // Create entry if not stored
                if (i >= m_Entries.Count)
                    m_Entries.Add(new  ModeEntry(this, i, modesProp));

                // Draw entry
                m_Entries[i].DoLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}