using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using UnityEditorInternal;
using NeoFPS;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor
{
    public static class NeoFpsEditorGUI
    {
        #region UTILITIES

        private static GUIStyle m_Separator = null;
        public static GUIStyle separator
        {
            get
            {
                if (m_Separator == null)
                {
                    m_Separator = new GUIStyle();
                    m_Separator.stretchWidth = true;
                    m_Separator.fixedHeight = 1f;
                    m_Separator.clipping = TextClipping.Clip;
                    m_Separator.border = new RectOffset(2, 2, 2, 2);


                    if (EditorGUIUtility.isProSkin)
                        m_Separator.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/pre toolbar.png") as Texture2D;
                    else
                        m_Separator.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/pre toolbar.png") as Texture2D;
                }
                return m_Separator;
            }
        }

        private static GUIStyle m_WordWrappedBoldLabel = null;
        public static GUIStyle wordWrappedBoldLabel
        {
            get
            {
                if (m_WordWrappedBoldLabel == null)
                {
                    m_WordWrappedBoldLabel = new GUIStyle(EditorStyles.boldLabel);
                    m_WordWrappedBoldLabel.wordWrap = true;
                }
                return m_WordWrappedBoldLabel;
            }
        }

        public static void Header(string header)
        {
            GUILayout.Space(8);
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        }

        public static void Separator()
        {
            GUILayout.Space(2);
            GUILayout.Box("", separator);
            GUILayout.Space(4);
        }

        public static GUILayoutOption GetFieldWidth(int offset = 0)
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20 - offset);
        }

        #endregion

        #region MIN-MAX SLIDERS

        public static void MinMaxSlider(SerializedProperty vector2Prop, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            MinMaxSlider(EditorGUILayout.GetControlRect(), vector2Prop, minLimit, maxLimit);
        }

        public static void MinMaxSlider(GUIContent label, SerializedProperty vector2Prop, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            bool hasLabel = label != GUIContent.none;
            var position = EditorGUILayout.GetControlRect(hasLabel);
            MinMaxSlider(position, label, vector2Prop, minLimit, maxLimit);
        }

        public static void MinMaxSlider(string label, SerializedProperty vector2Prop, float minLimit, float maxLimit, params GUILayoutOption[] options)
        {
            bool hasLabel = string.IsNullOrEmpty(label);
            var position = EditorGUILayout.GetControlRect(hasLabel);
            MinMaxSlider(position, hasLabel ? new GUIContent(label, vector2Prop.stringValue) : GUIContent.none, vector2Prop, minLimit, maxLimit);
        }

        public static void MinMaxSlider(Rect position, SerializedProperty vector2Prop, float minLimit, float maxLimit)
        {
            var label = EditorGUI.BeginProperty(position, null, vector2Prop);

            Vector2 vector = vector2Prop.vector2Value;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref vector.x, ref vector.y, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
                vector2Prop.vector2Value = vector;

            EditorGUI.EndProperty();
        }

        public static void MinMaxSlider(Rect position, GUIContent label, SerializedProperty vector2Prop, float minLimit, float maxLimit)
        {
            label = EditorGUI.BeginProperty(position, label, vector2Prop);

            Vector2 vector = vector2Prop.vector2Value;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref vector.x, ref vector.y, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
                vector2Prop.vector2Value = vector;

            EditorGUI.EndProperty();
        }

        public static void MinMaxSlider(Rect position, string labelString, SerializedProperty vector2Prop, float minLimit, float maxLimit)
        {
            var label = EditorGUI.BeginProperty(position, new GUIContent(labelString, vector2Prop.tooltip), vector2Prop);

            Vector2 vector = vector2Prop.vector2Value;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref vector.x, ref vector.y, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
                vector2Prop.vector2Value = vector;

            EditorGUI.EndProperty();
        }

        #endregion

        #region MINI HELP BOXES

        public static readonly Color errorRed = new Color(1f, 0.35f, 0.25f);
        public static readonly Color warningYellow = new Color(1f, 0.8f, 0.25f);

        public static void MiniError(string message)
        {
            Color c = GUI.color;
            GUI.color = errorRed;
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight + 4));
            EditorGUI.HelpBox(r, message, MessageType.Error);
            GUI.color = c;
        }

        public static void MiniWarning(string message)
        {
            Color c = GUI.color;
            GUI.color = warningYellow;
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight + 4));
            EditorGUI.HelpBox(r, message, MessageType.Warning);
            GUI.color = c;
        }

        public static void MiniInfo(string message)
        {
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight + 4));
            EditorGUI.HelpBox(r, message, MessageType.Info);
        }

        #endregion

        #region INLINE TOGGLE & FOLDOUT

        public static bool TogglePropertyField(SerializedProperty prop, params GUILayoutOption[] options)
        {
            // Show property field
            EditorGUILayout.PropertyField(prop, options);
            return prop.boolValue;
        }

        public static bool FoldoutProperty(SerializedProperty prop, string title)
        {
            prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, title, true);
            return prop.boolValue;
        }

        public static bool FoldoutProperty(SerializedProperty prop, GUIContent label)
        {
            prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, label, true);
            return prop.boolValue;
        }

        #endregion

        #region CALLBACK PROPERTY FIELDS

        public static void CallbackFloatPropertyField(SerializedProperty prop, Func<float, float> callback, params GUILayoutOption[] options)
        {
            // Check
            if (callback == null)
            {
                Debug.LogError("Callback cannot be null");
                EditorGUILayout.PropertyField(prop);
                return;
            }

            // Record original value
            float original = prop.floatValue;

            // Show property field
            EditorGUILayout.DelayedFloatField(prop, options);

            // If the value changed, use the callback
            if (prop.floatValue != original)
                prop.floatValue = callback(prop.floatValue);
        }

        public static void CallbackIntPropertyField(SerializedProperty prop, Func<int, int> callback, params GUILayoutOption[] options)
        {
            // Check
            if (callback == null)
            {
                Debug.LogError("Callback cannot be null");
                EditorGUILayout.PropertyField(prop);
                return;
            }

            // Record original value
            int original = prop.intValue;

            // Show property field
            EditorGUILayout.DelayedIntField(prop, options);

            // If the value changed, use the callback
            if (prop.intValue != original)
                prop.intValue = callback(prop.intValue);
        }

        public static void CallbackStringPropertyField(SerializedProperty prop, Func<string, string> callback, params GUILayoutOption[] options)
        {
            // Check
            if (callback == null)
            {
                Debug.LogError("Callback cannot be null");
                EditorGUILayout.PropertyField(prop);
                return;
            }

            // Record original value
            string original = prop.stringValue;

            // Show property field
            EditorGUILayout.DelayedTextField(prop, options);

            // If the value changed, use the callback
            if (prop.stringValue != original)
                prop.stringValue = callback(prop.stringValue);
        }

        #endregion

        #region MULTI-CHOICE & DROPDOWNS

        private static SerializedProperty s_PendingDropdownProperty = null;

        public static int MultiChoiceField(GUIContent label, SerializedProperty prop, string[] options)
        {
            // Get the label
            bool hasLabel = (label != null && label != GUIContent.none);
            if (hasLabel)
                EditorGUILayout.LabelField(label);
            else
                GUILayout.Space(4);

            // Draw options
            DrawMultiChoiceOptions(prop, options, hasLabel);

            // return value
            return prop.intValue;
        }

        public static int MultiChoiceField(SerializedProperty prop, string[] options)
        {
            // Get the label
            var label = new GUIContent(prop.displayName + ":", prop.tooltip);
            EditorGUILayout.LabelField(label);

            // Draw options
            DrawMultiChoiceOptions(prop, options, true);

            // return value
            return prop.intValue;
        }

        static void DrawMultiChoiceOptions(SerializedProperty prop, string[] options, bool padStart)
        {
            // Show list of options
            int index = prop.intValue;
            using (new EditorGUILayout.VerticalScope())
            {
                for (int option = 0; option < options.Length; ++option)
                {
                    // Add padding
                    if (option != 0)
                        GUILayout.Space(2);

                    // Check if was selected option
                    bool wasSelected = (option == index);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (padStart)
                            GUILayout.Space(20);
                        else
                            GUILayout.Space(8);

                        // Draw toggle on the left
                        bool isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(18));
                        if (option != index && isSelected)
                        {
                            // Set option to index if toggled on
                            prop.intValue = option;
                            index = option;
                        }

                        // Option description on right of toggle
                        EditorGUILayout.LabelField(options[option], EditorStyles.wordWrappedMiniLabel);
                    }
                }

                GUILayout.Space(2);
            }
        }

        public static int DropdownField(SerializedProperty prop, string[] options)
        {
            // Get the currently selected object's name
            int index = prop.intValue;
            GUIContent selected = new GUIContent((index < 0 || index >= options.Length) ? "<Selection Required>" : options[index]);

            // Show the dropdown button
            bool showMenu = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
                if (EditorGUILayout.DropdownButton(selected, FocusType.Passive))
                    showMenu = true;
            }

            // Show the dropdown menu
            if (showMenu)
            {
                s_PendingDropdownProperty = prop;

                var menu = new GenericMenu();

                for (int option = 0; option < options.Length; ++option)
                    menu.AddItem(new GUIContent(options[option]), false, OnDropdownFieldSelect, option);

                menu.ShowAsContext();
            }

            return prop.intValue;
        }

        public static T AssetDropdown<T>(SerializedProperty prop, Func<T, bool> filter, Func<T, string> formatter) where T : ScriptableObject
        {
            // Get default formatted if required
            if (formatter == null)
                formatter = t => {
                    if (t != null)
                        return t.name;
                    else
                        return "None Selected";
                };

            // Show the dropdown button
            bool showMenu = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
                if (EditorGUILayout.DropdownButton(new GUIContent(formatter(prop.objectReferenceValue as T)), FocusType.Passive, GetFieldWidth()))
                    showMenu = true;
            }

            if (showMenu)
            {
                // Get valid options
                var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                List<T> valid = new List<T>();
                for (int i = 0; i < guids.Length; ++i)
                {
                    var o = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (filter == null || filter(o))
                        valid.Add(o);
                }

                // If there are valid options, show the dropdown menu
                if (valid.Count > 0)
                {
                    s_PendingDropdownProperty = prop;

                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("<None>"), false, OnNullSelected);
                    for (int i = 0; i < valid.Count; ++i)
                        menu.AddItem(new GUIContent(formatter(valid[i])), false, OnObjectDropdownFieldSelect, valid[i]);

                    menu.ShowAsContext();
                }
            }

            return prop.objectReferenceValue as T;
        }

        public static GameObject PrefabDropdown(SerializedProperty prop, Func<GameObject, bool> filter)
        {
            // Get and check current value
            var current = prop.objectReferenceValue as GameObject;
            if (filter != null && current != null && !filter(current))
            {
                prop.objectReferenceValue = null;
                current = null;
            }

            // Show the dropdown button
            bool showMenu = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
                if (EditorGUILayout.DropdownButton(new GUIContent(current == null ? "<None Selected>" : current.name), FocusType.Passive, GetFieldWidth()))
                    showMenu = true;
            }

            if (showMenu)
            {
                // Get valid options
                var guids = AssetDatabase.FindAssets("t:GameObject");
                List<GameObject> valid = new List<GameObject>();
                for (int i = 0; i < guids.Length; ++i)
                {
                    var o = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (filter == null || filter(o))
                        valid.Add(o);
                }

                // If there are valid options, show the dropdown menu
                if (valid.Count > 0)
                {
                    s_PendingDropdownProperty = prop;

                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("<None>"), false, OnNullSelected);
                    for (int i = 0; i < valid.Count; ++i)
                        menu.AddItem(new GUIContent(valid[i].name), false, OnObjectDropdownFieldSelect, valid[i]);

                    menu.ShowAsContext();
                }
            }

            return prop.objectReferenceValue as GameObject;
        }

        public static T PrefabDropdown<T>(SerializedProperty prop) where T : Component
        {
            // Get current value
            var current = prop.objectReferenceValue as T;

            // Show the dropdown button
            bool showMenu = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
                if (EditorGUILayout.DropdownButton(new GUIContent(current == null ? "<None Selected>" : current.name), FocusType.Passive, GetFieldWidth()))
                    showMenu = true;
            }

            if (showMenu)
            {
                // Get valid options
                var guids = AssetDatabase.FindAssets("t:GameObject");
                List<T> valid = new List<T>();
                for (int i = 0; i < guids.Length; ++i)
                {
                    var o = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    var c = o.GetComponent<T>();
                    if (c != null)
                        valid.Add(c);
                }

                // If there are valid options, show the dropdown menu
                if (valid.Count > 0)
                {
                    s_PendingDropdownProperty = prop;

                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("<None>"), false, OnNullSelected);
                    for (int i = 0; i < valid.Count; ++i)
                        menu.AddItem(new GUIContent(valid[i].name), false, OnObjectDropdownFieldSelect, valid[i]);

                    menu.ShowAsContext();
                }
            }

            return prop.objectReferenceValue as T;
        }

        public static T PrefabDropdown<T>(SerializedProperty prop, Func<T, bool> filter) where T : Component
        {
            // Get and check current value
            var current = prop.objectReferenceValue as T;
            if (filter != null && current != null && !filter(current))
            {
                prop.objectReferenceValue = null;
                current = null;
            }

            // Show the dropdown button
            bool showMenu = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
                if (EditorGUILayout.DropdownButton(new GUIContent(current == null ? "<None Selected>" : current.name), FocusType.Passive, GetFieldWidth()))
                    showMenu = true;
            }

            if (showMenu)
            {
                // Get valid options
                var guids = AssetDatabase.FindAssets("t:GameObject");
                List<T> valid = new List<T>();
                for (int i = 0; i < guids.Length; ++i)
                {
                    var o = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    var c = o.GetComponent<T>();
                    if (c != null && (filter == null || filter(c)))
                        valid.Add(c);
                }

                // If there are valid options, show the dropdown menu
                if (valid.Count > 0)
                {
                    s_PendingDropdownProperty = prop;

                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("<None>"), false, OnNullSelected);
                    for (int i = 0; i < valid.Count; ++i)
                        menu.AddItem(new GUIContent(valid[i].name), false, OnObjectDropdownFieldSelect, valid[i]);

                    menu.ShowAsContext();
                }
            }

            return prop.objectReferenceValue as T;
        }

        static void OnDropdownFieldSelect(object o)
        {
            s_PendingDropdownProperty.intValue = (int)o;
            s_PendingDropdownProperty.serializedObject.ApplyModifiedProperties();
            s_PendingDropdownProperty = null;
        }

        static void OnObjectDropdownFieldSelect(object o)
        {
            s_PendingDropdownProperty.objectReferenceValue = o as UnityEngine.Object;
            s_PendingDropdownProperty.serializedObject.ApplyModifiedProperties();
            s_PendingDropdownProperty = null;
        }

        static void OnNullSelected()
        {
            s_PendingDropdownProperty.objectReferenceValue = null;
            s_PendingDropdownProperty.serializedObject.ApplyModifiedProperties();
            s_PendingDropdownProperty = null;
        }

        #endregion

        #region OBJECT BROWSERS

        private static SerializedProperty s_PendingBrowserProperty = null;
        
        public static GameObject GameObjectInHierarchyField(SerializedProperty prop, Transform root, bool allowRoot = true)
        {
            return GameObjectInHierarchyField(new GUIContent(prop.displayName, prop.tooltip), prop, root, null, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField<T>(SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            return GameObjectInHierarchyField(new GUIContent(prop.displayName, prop.tooltip), prop, root, ObjectHierarchyBrowser.FilterByComponent<T>, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField(SerializedProperty prop, GUIContent label, Transform root, bool allowRoot = true)
        {
            return GameObjectInHierarchyField(label, prop, root, null, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField<T>(SerializedProperty prop, GUIContent label, Transform root, bool allowRoot = true) where T : class
        {
            return GameObjectInHierarchyField(label, prop, root, ObjectHierarchyBrowser.FilterByComponent<T>, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField(SerializedProperty prop, Transform root, GameObjectFilter filter, bool allowRoot = true)
        {
            return GameObjectInHierarchyField(new GUIContent(prop.displayName, prop.tooltip), prop, root, filter, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField(GUIContent label, SerializedProperty prop, Transform root, GameObjectFilter filter, bool allowRoot = true)
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    GUILayout.Label("<No Root Object Set>");
                }
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<GameObject>(label, prop, true))
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnGameObjectPicked, OnObjectPickingCancelled, filter);

                return prop.objectReferenceValue as GameObject;
            }
        }

        public static T ComponentInHierarchyField<T>(SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            return ComponentInHierarchyField<T>(new GUIContent(prop.displayName, prop.tooltip), prop, root, allowRoot);
        }

        public static T ComponentInHierarchyField<T>(GUIContent label, SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    GUILayout.Label("<No Root Object Set>");
                }
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy<T>(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<T>(label, prop, true))
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnComponentPicked<T>, OnObjectPickingCancelled);

                return prop.objectReferenceValue as T;
            }
        }

        public static T ComponentInHierarchyField<T>(SerializedProperty prop, Transform root, ComponentFilter<T> filter, bool allowRoot = true) where T : class
        {
            return ComponentInHierarchyField<T>(new GUIContent(prop.displayName, prop.tooltip), prop, root, filter, allowRoot);
        }

        public static T ComponentInHierarchyField<T>(GUIContent label, SerializedProperty prop, Transform root, ComponentFilter<T> filter, bool allowRoot = true) where T : class
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    GUILayout.Label("<No Root Object Set>");
                }
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy<T>(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<T>(label, prop, true))
                {
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnComponentPicked<T>, OnObjectPickingCancelled,
                        (obj) =>
                        {
                            var c = obj.GetComponent<T>();
                            if (c == null)
                                return false;
                            if (filter != null && !filter(c))
                                return false;
                            return true;
                        });
                }

                return prop.objectReferenceValue as T;
            }
        }

        static bool DrawCustomObjectField<T>(GUIContent label, SerializedProperty prop, bool allowSceneObjects)
        {
            bool result = false;

            // Show the object field
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);

                // Get the full rect
                var fullRect = scope.rect;
                fullRect.x += EditorGUIUtility.labelWidth;
                fullRect.width -= EditorGUIUtility.labelWidth;

                // Get the button rect
                var buttonRect = fullRect;
                buttonRect.x += buttonRect.width - 20;
                buttonRect.width = 20;

                // Check for button click and override
                var e = Event.current;
                if (e.isMouse && buttonRect.Contains(Event.current.mousePosition))
                {
                    if (e.type == EventType.MouseDown)
                    {
                        s_PendingBrowserProperty = prop;
                        result = true;
                    }
                    Event.current.Use();
                }

                // Show object field
                prop.objectReferenceValue = EditorGUI.ObjectField(fullRect, GUIContent.none, prop.objectReferenceValue, typeof(T), allowSceneObjects);
            }

            return result;
        }

        public static GameObject GameObjectInHierarchyField(Rect rect, SerializedProperty prop, Transform root, bool allowRoot = true)
        {
            return GameObjectInHierarchyField(rect, prop, root, null, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField<T>(Rect rect, SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            return GameObjectInHierarchyField(rect, prop, root, ObjectHierarchyBrowser.FilterByComponent<T>, allowRoot);
        }

        public static GameObject GameObjectInHierarchyField(Rect rect, SerializedProperty prop, Transform root, GameObjectFilter filter, bool allowRoot = true)
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                GUI.Label(rect, "<No Root Object Set>");
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<GameObject>(rect, prop, true))
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnGameObjectPicked, OnObjectPickingCancelled, filter);

                return prop.objectReferenceValue as GameObject;
            }
        }

        public static T ComponentInHierarchyField<T>(Rect rect, SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                GUI.Label(rect, "<No Root Object Set>");
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy<T>(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<T>(rect, prop, true))
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnComponentPicked<T>, OnObjectPickingCancelled);

                return prop.objectReferenceValue as T;
            }
        }

        public static T ComponentInHierarchyField<T>(Rect rect, SerializedProperty prop, Transform root, ComponentFilter<T> filter, bool allowRoot = true) where T : class
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                GUI.Label(rect, "<No Root Object Set>");
                return null;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy<T>(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                    prop.objectReferenceValue = null;

                // Show the object field
                if (DrawCustomObjectField<T>(rect, prop, true))
                {
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnComponentPicked<T>, OnObjectPickingCancelled,
                        (obj) =>
                        {
                            var c = obj.GetComponent<T>();
                            if (c == null)
                                return false;
                            if (filter != null && !filter(c))
                                return false;
                            return true;
                        });
                }

                return prop.objectReferenceValue as T;
            }
        }

        static bool DrawCustomObjectField<T>(Rect rect, SerializedProperty prop, bool allowSceneObjects)
        {
            bool result = false;

            float h = rect.height - EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += Mathf.FloorToInt(h / 2) - 1;

            // Get the button rect
            var buttonRect = rect;
            buttonRect.x += buttonRect.width - 20;
            buttonRect.width = 20;

            // Check for button click and override
            var e = Event.current;
            if (e.isMouse && buttonRect.Contains(Event.current.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    s_PendingBrowserProperty = prop;
                    result = true;
                }
                Event.current.Use();
            }

            // Show object field
            prop.objectReferenceValue = EditorGUI.ObjectField(rect, GUIContent.none, prop.objectReferenceValue, typeof(T), allowSceneObjects);

            return result;
        }

        static bool CheckTransformHierarchy(SerializedProperty prop, Transform root, bool allowRoot)
        {
            // Null is fine
            if (prop.objectReferenceValue == null)
                return true;

            // Wrong type is not
            var obj = prop.objectReferenceValue as GameObject;
            if (obj == null)
                return false;

            // Check hierarchy
            return CheckTransformHierarchy(obj.transform, root, allowRoot);
        }

        static bool CheckTransformHierarchy<T>(SerializedProperty prop, Transform root, bool allowRoot) where T : class
        {
            // Null is fine
            if (prop.objectReferenceValue == null)
                return true;

            // Wrong type is not
            var component = prop.objectReferenceValue as Component;
            if (component == null || !component is T)
                return false;

            // Check hierarchy
            return CheckTransformHierarchy(component.transform, root, allowRoot);
        }

        static bool CheckTransformHierarchy(Transform t, Transform root, bool allowRoot)
        {
            // Check if t is root
            if (t == root)
                return allowRoot;

            // Check if t is a child of root
            var itr = t;
            while (itr != null)
            {
                itr = itr.parent;
                if (itr == root)
                    return true;
            }

            return false;
        }
        
        public static GameObject PrefabField(SerializedProperty prop, GameObjectFilter filter)
        {
            return PrefabField(new GUIContent(prop.displayName, prop.tooltip), prop, filter);
        }

        public static GameObject PrefabField(GUIContent label, SerializedProperty prop, GameObjectFilter filter)
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                prop.objectReferenceValue = null;

            // Show the object field
            if (DrawCustomObjectField<GameObject>(label, prop, false))
                ProjectHierarchyBrowser.GetPrefab(OnGameObjectPicked, OnObjectPickingCancelled, filter);

            return prop.objectReferenceValue as GameObject;
        }

        public static GameObject PrefabField(SerializedProperty prop)
        {
            return PrefabField(prop, null);
        }

        public static GameObject ModelPrefabField(SerializedProperty prop)
        {
            return PrefabField(prop, (o) => { return o != null && PrefabUtility.IsPartOfModelPrefab(o); });
        }

        public static GameObject PrefabField(GUIContent label, SerializedProperty prop)
        {
            return PrefabField(label, prop, null);
        }

        public static GameObject ModelPrefabField(GUIContent label, SerializedProperty prop)
        {
            return PrefabField(label, prop, (o) => { return o != null && PrefabUtility.IsPartOfModelPrefab(o); });
        }

        public static T PrefabComponentField<T>(SerializedProperty prop) where T : class
        {
            return PrefabComponentField<T>(new GUIContent(prop.displayName, prop.tooltip), prop);
        }

        public static T PrefabComponentField<T>(GUIContent label, SerializedProperty prop) where T : class
        {
            // Show the object field
            if (DrawCustomObjectField<T>(label, prop, false))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled);

            return prop.objectReferenceValue as T;
        }

        public static T PrefabComponentField<T>(SerializedProperty prop, ComponentFilter<T> filter) where T : class
        {
            return PrefabComponentField<T>(new GUIContent(prop.displayName, prop.tooltip), prop, filter);
        }

        public static T PrefabComponentField<T>(GUIContent label, SerializedProperty prop, ComponentFilter<T> filter) where T : class
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                prop.objectReferenceValue = null;

            // Show the object field
            if (DrawCustomObjectField<T>(label, prop, false))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled, filter);

            return prop.objectReferenceValue as T;
        }

        public static T AssetField<T>(SerializedProperty prop) where T : class
        {
            return AssetField<T>(new GUIContent(prop.displayName, prop.tooltip), prop);
        }

        public static T AssetField<T>(GUIContent label, SerializedProperty prop) where T : class
        {
            // Show the object field
            if (DrawCustomObjectField<GameObject>(label, prop, false))
                ProjectHierarchyBrowser.GetAsset<T>(OnObjectPicked, OnObjectPickingCancelled);

            return prop.objectReferenceValue as T;
        }

        public static GameObject PrefabField(Rect rect, SerializedProperty prop, GameObjectFilter filter)
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                prop.objectReferenceValue = null;

            // Show the object field
            if (DrawCustomObjectField<GameObject>(rect, prop, false))
                ProjectHierarchyBrowser.GetPrefab(OnGameObjectPicked, OnObjectPickingCancelled, filter);

            return prop.objectReferenceValue as GameObject;
        }

        public static GameObject PrefabField(Rect rect, SerializedProperty prop)
        {
            return PrefabField(rect, prop, null);
        }

        public static GameObject ModelPrefabField(Rect rect, SerializedProperty prop)
        {
            return PrefabField(rect, prop, (o) => { return o != null && PrefabUtility.IsPartOfModelPrefab(o); });
        }

        public static T PrefabComponentField<T>(Rect rect, SerializedProperty prop) where T : class
        {
            // Show the object field
            if (DrawCustomObjectField<T>(rect, prop, false))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled);

            return prop.objectReferenceValue as T;
        }

        public static T PrefabComponentField<T>(Rect rect, SerializedProperty prop, ComponentFilter<T> filter) where T : class
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                prop.objectReferenceValue = null;

            // Show the object field
            if (DrawCustomObjectField<T>(rect, prop, false))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled, filter);

            return prop.objectReferenceValue as T;
        }

        public static T AssetField<T>(Rect rect, SerializedProperty prop) where T : class
        {
            // Show the object field
            if (DrawCustomObjectField<GameObject>(rect, prop, false))
                ProjectHierarchyBrowser.GetAsset<T>(OnObjectPicked, OnObjectPickingCancelled);

            return prop.objectReferenceValue as T;
        }

        static void OnGameObjectPicked(GameObject obj)
        {
            s_PendingBrowserProperty.objectReferenceValue = obj;
            s_PendingBrowserProperty.serializedObject.ApplyModifiedProperties();
            s_PendingBrowserProperty = null;
        }

        static void OnComponentPicked<T>(GameObject obj) where T : class
        {
            if (obj == null)
                s_PendingBrowserProperty.objectReferenceValue = null;
            else
                s_PendingBrowserProperty.objectReferenceValue = obj.GetComponent<T>() as UnityEngine.Object;

            s_PendingBrowserProperty.serializedObject.ApplyModifiedProperties();
            s_PendingBrowserProperty = null;
        }

        static void OnObjectPicked<T>(T obj) where T : class
        {
            s_PendingBrowserProperty.objectReferenceValue = obj as UnityEngine.Object;
            s_PendingBrowserProperty.serializedObject.ApplyModifiedProperties();
            s_PendingBrowserProperty = null;
        }

        static void OnObjectPickingCancelled()
        {
            s_PendingBrowserProperty = null;
        }

        #endregion

        #region OBJECT BROWSER LISTS

        public static ReorderableList GetGameObjectInHierarchyList(SerializedProperty prop, Func<Transform> rootGetter, bool allowRoot = true)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                GameObjectInHierarchyField(rect, prop.GetArrayElementAtIndex(index), rootGetter(), allowRoot);
            });
        }

        public static ReorderableList GetGameObjectInHierarchyList(SerializedProperty prop, Func<Transform> rootGetter, GameObjectFilter filter, bool allowRoot = true)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                GameObjectInHierarchyField(rect, prop.GetArrayElementAtIndex(index), rootGetter(), filter, allowRoot);
            });
        }

        public static ReorderableList GetGameObjectInHierarchyList<T>(SerializedProperty prop, Func<Transform> rootGetter, bool allowRoot = true) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                GameObjectInHierarchyField<T>(rect, prop.GetArrayElementAtIndex(index), rootGetter(), allowRoot);
            });
        }

        public static ReorderableList GetComponentInHierarchyList<T>(SerializedProperty prop, Func<Transform> rootGetter, bool allowRoot = true) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                ComponentInHierarchyField<T>(rect, prop.GetArrayElementAtIndex(index), rootGetter(), allowRoot);
            });
        }

        public static ReorderableList GetComponentInHierarchyList<T>(SerializedProperty prop, Func<Transform> rootGetter, ComponentFilter<T> filter, bool allowRoot = true) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                ComponentInHierarchyField<T>(rect, prop.GetArrayElementAtIndex(index), rootGetter(), filter, allowRoot);
            });
        }

        public static ReorderableList GetPrefabList(SerializedProperty prop)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                PrefabField(rect, prop.GetArrayElementAtIndex(index));
            });
        }

        public static ReorderableList GetPrefabList(SerializedProperty prop, GameObjectFilter filter)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                PrefabField(rect, prop.GetArrayElementAtIndex(index), filter);
            });
        }

        public static ReorderableList GetModelPrefabList(SerializedProperty prop)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                ModelPrefabField(rect, prop.GetArrayElementAtIndex(index));
            });
        }

        public static ReorderableList GetPrefabComponentList<T>(SerializedProperty prop) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                PrefabComponentField<T>(rect, prop.GetArrayElementAtIndex(index));
            });
        }

        public static ReorderableList GetPrefabComponentList<T>(SerializedProperty prop, ComponentFilter<T> filter) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                PrefabComponentField<T>(rect, prop.GetArrayElementAtIndex(index), filter);
            });
        }

        public static ReorderableList GetAssetList<T>(SerializedProperty prop) where T : class
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                AssetField<T>(rect, prop.GetArrayElementAtIndex(index));
            });
        }

        public static ReorderableList GetObjectList(SerializedProperty prop)
        {
            return GetObjectBrowserList(prop, (rect, index, isActive, isFocused) =>
            {
                rect.y += 2f;
                rect.height = 16f;
                EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, false);
            });
        }

        static ReorderableList GetObjectBrowserList (SerializedProperty prop, ReorderableList.ElementCallbackDelegate drawElementCallback)
        {
            var result = new ReorderableList(prop.serializedObject, prop);
            result.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, new GUIContent(prop.displayName, prop.tooltip)); };
            result.drawElementCallback = drawElementCallback;
            result.elementHeight = EditorGUIUtility.singleLineHeight + 6;
            return result;
        }

        #endregion

        #region ANIMATOR KEYS

        public static bool AnimatorBoolKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            return AnimatorBoolKeyField(new GUIContent(prop.displayName, prop.tooltip), prop, controller, allowEmpty);
        }

        public static bool AnimatorBoolKeyField(GUIContent label, SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            EditorGUILayout.PropertyField(prop, label);
            return CheckAnimatorKey(prop, controller, allowEmpty, UnityEngine.AnimatorControllerParameterType.Bool);
        }


        public static bool AnimatorIntKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            return AnimatorIntKeyField(new GUIContent(prop.displayName, prop.tooltip), prop, controller, allowEmpty);
        }

        public static bool AnimatorIntKeyField(GUIContent label, SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            EditorGUILayout.PropertyField(prop, label);
            return CheckAnimatorKey(prop, controller, allowEmpty, UnityEngine.AnimatorControllerParameterType.Int);
        }


        public static bool AnimatorFloatKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            return AnimatorFloatKeyField(new GUIContent(prop.displayName, prop.tooltip), prop, controller, allowEmpty);
        }

        public static bool AnimatorFloatKeyField(GUIContent label, SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            EditorGUILayout.PropertyField(prop, label);
            return CheckAnimatorKey(prop, controller, allowEmpty, UnityEngine.AnimatorControllerParameterType.Float);
        }


        public static bool AnimatorTriggerKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            return AnimatorTriggerKeyField(new GUIContent(prop.displayName, prop.tooltip), prop, controller, allowEmpty);
        }

        public static bool AnimatorTriggerKeyField(GUIContent label, SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty = true)
        {
            EditorGUILayout.PropertyField(prop, label);
            return CheckAnimatorKey(prop, controller, allowEmpty, UnityEngine.AnimatorControllerParameterType.Trigger);
        }

        static bool CheckAnimatorKey(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, bool allowEmpty, UnityEngine.AnimatorControllerParameterType parameterType)
        {
            // Controller
            if (controller == null)
            {
                MiniError("Animator controller is null");
                return false;
            }

            // Empty is always valid
            if (string.IsNullOrEmpty(prop.stringValue))
            {
                if (allowEmpty)
                    return true;
                else
                {
                    MiniError("Key is required. Cannot be empty");
                    return false;
                }
            }

            // Check parameter
            bool found = false;
            foreach (var p in controller.parameters)
            {
                if (p.type == parameterType && p.name == prop.stringValue)
                {
                    found = true;
                    break;
                }
            }


            if (!found)
            {
                switch (parameterType)
                {
                    case UnityEngine.AnimatorControllerParameterType.Bool:
                        MiniError("No bool parameter found with key");
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Int:
                        MiniError("No int parameter found with key");
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Float:
                        MiniError("No float parameter found with key");
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Trigger:
                        MiniError("No trigger parameter found with key");
                        return false;
                }
                MiniError("Parameter not found");
                return false;
            }

            return true;
        }

        public static bool ShowAnimatorKeys(UnityEditor.Animations.AnimatorController controller, bool foldout)
        {
            if (controller == null)
                return foldout;

            foldout = EditorGUILayout.Foldout(foldout, string.Format("Animator Controller Parameters ({0})", controller.name), true);
            if (foldout)
            {
                int triggerCount = 0, boolCount = 0, intCount = 0, floatCount = 0;
                var parameters = controller.parameters;

                if (parameters.Length > 0)
                {
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        switch (parameters[i].type)
                        {
                            case UnityEngine.AnimatorControllerParameterType.Trigger: ++triggerCount; break;
                            case UnityEngine.AnimatorControllerParameterType.Bool: ++boolCount; break;
                            case UnityEngine.AnimatorControllerParameterType.Int: ++intCount; break;
                            case UnityEngine.AnimatorControllerParameterType.Float: ++floatCount; break;
                        }
                    }

                    ++EditorGUI.indentLevel;
                    // Print triggers
                    if (triggerCount > 0)
                    {
                        EditorGUILayout.LabelField("- Trigger parameters:", EditorStyles.miniLabel);
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < parameters.Length; ++i)
                        {
                            if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Trigger)
                                EditorGUILayout.LabelField(parameters[i].name, EditorStyles.miniLabel);
                        }
                        --EditorGUI.indentLevel;
                    }

                    // Print bool parameters
                    if (boolCount > 0)
                    {
                        EditorGUILayout.LabelField("- Bool parameters:", EditorStyles.miniLabel);
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < parameters.Length; ++i)
                        {
                            if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Bool)
                                EditorGUILayout.LabelField(parameters[i].name, EditorStyles.miniLabel);
                        }
                        --EditorGUI.indentLevel;
                    }

                    // Print int parameters
                    if (intCount > 0)
                    {
                        EditorGUILayout.LabelField("- Int parameters:", EditorStyles.miniLabel);
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < parameters.Length; ++i)
                        {
                            if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Int)
                                EditorGUILayout.LabelField(parameters[i].name, EditorStyles.miniLabel);
                        }
                        --EditorGUI.indentLevel;
                    }

                    // Print float parameters
                    if (floatCount > 0)
                    {
                        EditorGUILayout.LabelField("- Float parameters:", EditorStyles.miniLabel);
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < parameters.Length; ++i)
                        {
                            if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Float)
                                EditorGUILayout.LabelField(parameters[i].name, EditorStyles.miniLabel);
                        }
                        --EditorGUI.indentLevel;
                    }
                    --EditorGUI.indentLevel;
                }
                else
                    EditorGUILayout.LabelField("- No parameters found", EditorStyles.miniLabel);
            }

            return foldout;
        }

        #endregion

        #region MOTION GRAPH KEYS

        private static Texture2D m_MgKeyFoundTexture = null;
        private static Texture2D m_MgKeyNotFoundTexture = null;
        private static List<SwitchParameter> s_MotionGraphSwitchParameters = new List<SwitchParameter>();
        private static List<IntParameter> s_MotionGraphIntParameters = new List<IntParameter>();
        private static List<FloatParameter> s_MotionGraphFloatParameters = new List<FloatParameter>();
        private static List<TriggerParameter> s_MotionGraphTriggerParameters = new List<TriggerParameter>();
        private static List<VectorParameter> s_MotionGraphVectorParameters = new List<VectorParameter>();
        private static List<TransformParameter> s_MotionGraphTransformParameters = new List<TransformParameter>();
        private static List<EventParameter> s_MotionGraphEventParameters = new List<EventParameter>();
        private static List<BoolData> s_MotionGraphBoolData = new List<BoolData>();
        private static List<IntData> s_MotionGraphIntData = new List<IntData>();
        private static List<FloatData> s_MotionGraphFloatData = new List<FloatData>();

        static Texture2D mgKeyFoundTexture
        {
            get
            {
                if (m_MgKeyFoundTexture == null)
                    m_MgKeyFoundTexture = EditorGUIUtility.Load("CollabNew") as Texture2D;
                return m_MgKeyFoundTexture;
            }
        }

        static Texture2D mgKeyNotFoundTexture
        {
            get
            {
                if (m_MgKeyNotFoundTexture == null)
                    m_MgKeyNotFoundTexture = EditorGUIUtility.Load("ColorPicker-2DThumb") as Texture2D;
                return m_MgKeyNotFoundTexture;
            }
        }

        public static bool MotionGraphTriggerParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphTriggerParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphTriggerParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphTriggerParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphTriggerParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectTriggerParameters(s_MotionGraphTriggerParameters);
                foreach (var p in s_MotionGraphTriggerParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphTriggerParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphSwitchParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphSwitchParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphSwitchParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphSwitchParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphSwitchParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectSwitchParameters(s_MotionGraphSwitchParameters);
                foreach (var p in s_MotionGraphSwitchParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphSwitchParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphIntParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphIntParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphIntParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphIntParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphIntParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectIntParameters(s_MotionGraphIntParameters);
                foreach (var p in s_MotionGraphIntParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphIntParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphFloatParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphFloatParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphFloatParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphFloatParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphFloatParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectFloatParameters(s_MotionGraphFloatParameters);
                foreach (var p in s_MotionGraphFloatParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphFloatParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphVectorParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphVectorParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphVectorParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphVectorParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphVectorParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectVectorParameters(s_MotionGraphVectorParameters);
                foreach (var p in s_MotionGraphVectorParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphVectorParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphTransformParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphTransformParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphTransformParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphTransformParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphTransformParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectTransformParameters(s_MotionGraphTransformParameters);
                foreach (var p in s_MotionGraphTransformParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphTransformParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphEventParamKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphEventParamKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphEventParamKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphEventParamKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphEventParamKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectEventParameters(s_MotionGraphEventParameters);
                foreach (var p in s_MotionGraphEventParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphEventParameters.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphBoolDataKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphBoolDataKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphBoolDataKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphBoolDataKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphBoolDataKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectBoolData(s_MotionGraphBoolData);
                foreach (var p in s_MotionGraphBoolData)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphBoolData.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphIntDataKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphIntDataKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphIntDataKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphIntDataKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphIntDataKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectIntData(s_MotionGraphIntData);
                foreach (var p in s_MotionGraphIntData)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphIntData.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }

        public static bool MotionGraphFloatDataKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphFloatDataKeyField(EditorGUILayout.GetControlRect(), new GUIContent(prop.displayName, prop.tooltip), prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphFloatDataKeyField(GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            return MotionGraphFloatDataKeyField(EditorGUILayout.GetControlRect(), label, prop, motionGraph, allowEmpty);
        }

        public static bool MotionGraphFloatDataKeyField(Rect rect, GUIContent label, SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            // Shorten rect for valid/invalid icon
            rect.width -= EditorGUIUtility.singleLineHeight;

            // Draw property
            EditorGUI.PropertyField(rect, prop, label);
            bool valid = CheckMotionGraphKey(prop, motionGraph, allowEmpty);

            // Check if parameter exists
            if (valid)
            {
                valid = false;
                motionGraph.CollectFloatData(s_MotionGraphFloatData);
                foreach (var p in s_MotionGraphFloatData)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }
                s_MotionGraphFloatData.Clear();
            }

            // Draw valid icon
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            if (valid)
                GUI.Label(rect, mgKeyFoundTexture);
            else
                GUI.Label(rect, mgKeyNotFoundTexture);

            return valid;
        }
        
        static bool CheckMotionGraphKey(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty)
        {
            // Controller
            if (motionGraph == null)
                return false;

            // Empty is always valid
            if (string.IsNullOrEmpty(prop.stringValue))
                return allowEmpty;

            return true;
        }

        public static bool ShowMotionGraphKeys(MotionGraphContainer motionGraph, bool foldout, bool showParameters, bool showData)
        {
            if (motionGraph == null)
                return foldout;

            foldout = EditorGUILayout.Foldout(foldout, string.Format("Animator Controller Parameters ({0})", motionGraph.name), true);
            if (foldout)
            {
                bool hasParameters = false;

                if (showParameters)
                {
                    // Trigger parameters
                    motionGraph.CollectTriggerParameters(s_MotionGraphTriggerParameters);
                    if (s_MotionGraphTriggerParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Trigger parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphTriggerParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphTriggerParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphTriggerParameters.Clear();
                    }

                    // Switch parameters
                    motionGraph.CollectSwitchParameters(s_MotionGraphSwitchParameters);
                    if (s_MotionGraphSwitchParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Switch parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphSwitchParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphSwitchParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphSwitchParameters.Clear();
                    }

                    // Int parameters
                    motionGraph.CollectIntParameters(s_MotionGraphIntParameters);
                    if (s_MotionGraphIntParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Int parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphIntParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphIntParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphIntParameters.Clear();
                    }

                    // Float parameters
                    motionGraph.CollectFloatParameters(s_MotionGraphFloatParameters);
                    if (s_MotionGraphFloatParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Float parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphFloatParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphFloatParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphFloatParameters.Clear();
                    }

                    // Vector parameters
                    motionGraph.CollectVectorParameters(s_MotionGraphVectorParameters);
                    if (s_MotionGraphVectorParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Vector parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphVectorParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphVectorParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphVectorParameters.Clear();
                    }

                    // Transform parameters
                    motionGraph.CollectTransformParameters(s_MotionGraphTransformParameters);
                    if (s_MotionGraphTransformParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Transform parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphTransformParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphTransformParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphTransformParameters.Clear();
                    }

                    // Event parameters
                    motionGraph.CollectEventParameters(s_MotionGraphEventParameters);
                    if (s_MotionGraphEventParameters.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Event parameters:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphEventParameters.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphEventParameters[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphEventParameters.Clear();
                    }
                }

                if (showData)
                {
                    // Event parameters
                    motionGraph.CollectBoolData(s_MotionGraphBoolData);
                    if (s_MotionGraphBoolData.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Bool motion data:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphBoolData.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphBoolData[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphBoolData.Clear();
                    }
                    // Event parameters
                    motionGraph.CollectIntData(s_MotionGraphIntData);
                    if (s_MotionGraphIntData.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Int motion data:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphIntData.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphIntData[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphIntData.Clear();
                    }
                    // Event parameters
                    motionGraph.CollectFloatData(s_MotionGraphFloatData);
                    if (s_MotionGraphFloatData.Count > 0)
                    {
                        hasParameters = true;

                        // Category label
                        EditorGUILayout.LabelField("- Float motion data:", EditorStyles.miniLabel);

                        // Draw parameter names
                        ++EditorGUI.indentLevel;
                        for (int i = 0; i < s_MotionGraphFloatData.Count; ++i)
                            EditorGUILayout.LabelField(s_MotionGraphFloatData[i].name, EditorStyles.miniLabel);
                        --EditorGUI.indentLevel;

                        s_MotionGraphFloatData.Clear();
                    }
                }

                if (!hasParameters)
                    EditorGUILayout.LabelField("- No parameters or data found", EditorStyles.miniLabel);
            }

            return foldout;
        }

        #endregion

        #region REQUIRED PROPERTY

        public static bool RequiredObjectField(SerializedProperty prop)
        {
            bool valid = prop.objectReferenceValue != null;
            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
            return valid;
        }

        public static bool RequiredStringField(SerializedProperty prop)
        {
            bool valid = !string.IsNullOrWhiteSpace(prop.stringValue);
            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
            return valid;
        }

        public static bool RequiredMultiChoiceField(SerializedProperty prop, string[] options)
        {
            bool valid = (prop.intValue >= 0 && prop.intValue < options.Length);

            DrawRequiredLabel(prop, valid);
            DrawMultiChoiceOptions(prop, options, true);

            return valid;
        }

        public static void DrawRequiredLabel(SerializedProperty prop, bool valid)
        {
            if (valid)
                EditorGUILayout.LabelField(new GUIContent(prop.displayName, prop.tooltip));
            else
                EditorGUILayout.LabelField(new GUIContent("*" + prop.displayName, prop.tooltip), EditorStyles.boldLabel);
        }

        public static void DrawRequiredPrefixLabel(SerializedProperty prop, bool valid)
        {
            if (valid)
                EditorGUILayout.PrefixLabel(new GUIContent(prop.displayName, prop.tooltip));
            else
                EditorGUILayout.LabelField(new GUIContent("*" + prop.displayName, prop.tooltip), EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
        }

        public static void DrawRequiredLabel(Rect rect, SerializedProperty prop, bool valid)
        {
            if (valid)
                EditorGUI.LabelField(rect, new GUIContent(prop.displayName, prop.tooltip));
            else
                EditorGUI.LabelField(rect, new GUIContent("*" + prop.displayName, prop.tooltip), EditorStyles.boldLabel);
        }

        public static Rect DrawRequiredPrefixLabel(Rect rect, SerializedProperty prop, bool valid)
        {
            if (valid)
                return EditorGUI.PrefixLabel(rect, new GUIContent(prop.displayName, prop.tooltip));
            else
                return EditorGUI.PrefixLabel(rect, new GUIContent("*" + prop.displayName, prop.tooltip), EditorStyles.boldLabel);
        }

        #endregion

        #region REQUIRED OBJECT BROWSERS

        public static bool RequiredGameObjectInHierarchyField(SerializedProperty prop, Transform root, bool allowRoot = true)
        {
            return RequiredGameObjectInHierarchyField(prop, root, null, allowRoot);
        }

        public static bool RequiredGameObjectInHierarchyField<T>(SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            return RequiredGameObjectInHierarchyField(prop, root, ObjectHierarchyBrowser.FilterByComponent<T>, allowRoot);
        }
        
        public static bool RequiredGameObjectInHierarchyField(SerializedProperty prop, Transform root, GameObjectFilter filter, bool allowRoot = true)
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawRequiredPrefixLabel(prop, false);
                    GUILayout.Label("<No Root Object Set>");
                }
                return false;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                    prop.objectReferenceValue = null;

                bool valid = prop.objectReferenceValue as GameObject != null;

                // Show the object field
                if (DrawRequiredCustomObjectField<GameObject>(prop, true, valid))
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnGameObjectPicked, OnObjectPickingCancelled, filter);

                return valid;
            }
        }

        public static bool RequiredComponentInHierarchyField<T>(SerializedProperty prop, Transform root, bool allowRoot = true) where T : class
        {
            return RequiredComponentInHierarchyField<T>(prop, root, null, allowRoot);
        }

        public static bool RequiredComponentInHierarchyField<T>(SerializedProperty prop, Transform root, ComponentFilter<T> filter, bool allowRoot = true) where T : class
        {
            if (root == null)
            {
                prop.objectReferenceValue = null;
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawRequiredPrefixLabel(prop, false);
                    GUILayout.Label("<No Root Object Set>");
                }
                return false;
            }
            else
            {
                // Check hierarchy
                if (!CheckTransformHierarchy<T>(prop, root, allowRoot))
                    prop.objectReferenceValue = null;

                // Check filter
                if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                    prop.objectReferenceValue = null;

                bool valid = prop.objectReferenceValue as T != null;

                // Show the object field
                if (DrawRequiredCustomObjectField<T>(prop, true, valid))
                {
                    ObjectHierarchyBrowser.GetChildObject(root, allowRoot, OnComponentPicked<T>, OnObjectPickingCancelled,
                        (obj) =>
                        {
                            var c = obj.GetComponent<T>();
                            if (c == null)
                                return false;
                            if (filter != null && !filter(c))
                                return false;
                            return true;
                        });
                }

                return valid;
            }
        }

        static bool DrawRequiredCustomObjectField<T>(SerializedProperty prop, bool allowSceneObjects, bool valid)
        {
            bool result = false;

            // Show the object field
            using (var scope = new EditorGUILayout.HorizontalScope())
            {
                DrawRequiredPrefixLabel(prop, valid);

                // Get the full rect
                var fullRect = scope.rect;
                fullRect.x += EditorGUIUtility.labelWidth;
                fullRect.width -= EditorGUIUtility.labelWidth;

                // Get the button rect
                var buttonRect = fullRect;
                buttonRect.x += buttonRect.width - 20;
                buttonRect.width = 20;

                // Check for button click and override
                var e = Event.current;
                if (e.isMouse && buttonRect.Contains(Event.current.mousePosition))
                {
                    if (e.type == EventType.MouseDown)
                    {
                        s_PendingBrowserProperty = prop;
                        result = true;
                    }
                    Event.current.Use();
                }

                // Show object field
                prop.objectReferenceValue = EditorGUI.ObjectField(fullRect, GUIContent.none, prop.objectReferenceValue, typeof(T), allowSceneObjects);
            }

            return result;
        }

        public static bool RequiredPrefabField(SerializedProperty prop, GameObjectFilter filter)
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as GameObject))
                prop.objectReferenceValue = null;

            bool valid = prop.objectReferenceValue as GameObject != null;

            // Show the object field
            if (DrawRequiredCustomObjectField<GameObject>(prop, false, valid))
                ProjectHierarchyBrowser.GetPrefab(OnGameObjectPicked, OnObjectPickingCancelled, filter);

            return valid;
        }
        
        public static bool RequiredPrefabField(SerializedProperty prop)
        {
            return RequiredPrefabField(prop, null);
        }

        public static bool RequiredModelPrefabField(SerializedProperty prop)
        {
            return RequiredPrefabField(prop, (o) => { return o != null && PrefabUtility.IsPartOfModelPrefab(o); });
        }
        
        public static bool RequiredPrefabComponentField<T>(SerializedProperty prop) where T : class
        {
            bool valid = prop.objectReferenceValue as T != null;

            // Show the object field
            if (DrawRequiredCustomObjectField<T>(prop, false, valid))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled);

            return valid;
        }
        
        public static bool RequiredPrefabComponentField<T>(SerializedProperty prop, ComponentFilter<T> filter) where T : class
        {
            // Check filter
            if (filter != null && prop.objectReferenceValue != null && !filter(prop.objectReferenceValue as T))
                prop.objectReferenceValue = null;

            bool valid = prop.objectReferenceValue as T != null;

            // Show the object field
            if (DrawRequiredCustomObjectField<T>(prop, false, valid))
                ProjectHierarchyBrowser.GetPrefabWithComponent<T>(OnObjectPicked, OnObjectPickingCancelled, filter);

            return valid;
        }

        public static bool RequiredAssetField<T>(SerializedProperty prop) where T : class
        {
            bool valid = prop.objectReferenceValue as T != null;

            // Show the object field
            if (DrawRequiredCustomObjectField<T>(prop, false, valid))
                ProjectHierarchyBrowser.GetAsset<T>(OnObjectPicked, OnObjectPickingCancelled);

            return valid;
        }
        
        #endregion
        
        #region REQUIRED ANIMATOR KEYS

        public static bool RequiredAnimatorBoolKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller)
        {
            string errorMessage = string.Empty;
            bool valid = CheckAnimatorKey(prop, controller, UnityEngine.AnimatorControllerParameterType.Bool, out errorMessage);

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredAnimatorIntKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller)
        {
            string errorMessage = string.Empty;
            bool valid = CheckAnimatorKey(prop, controller, UnityEngine.AnimatorControllerParameterType.Int, out errorMessage);

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredAnimatorFloatKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller)
        {
            string errorMessage = string.Empty;
            bool valid = CheckAnimatorKey(prop, controller, UnityEngine.AnimatorControllerParameterType.Float, out errorMessage);

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredAnimatorTriggerKeyField(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller)
        {
            string errorMessage = string.Empty;
            bool valid = CheckAnimatorKey(prop, controller, UnityEngine.AnimatorControllerParameterType.Trigger, out errorMessage);

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        static bool CheckAnimatorKey(SerializedProperty prop, UnityEditor.Animations.AnimatorController controller, UnityEngine.AnimatorControllerParameterType parameterType, out string message)
        {
            // Controller
            if (controller == null)
            {
                message = "Animator controller is null";
                return false;
            }

            // Empty is always valid
            if (string.IsNullOrEmpty(prop.stringValue))
            {
                message = "Key is required. Cannot be empty";
                return false;
            }

            // Check parameter
            bool found = false;
            foreach (var p in controller.parameters)
            {
                if (p.type == parameterType && p.name == prop.stringValue)
                {
                    found = true;
                    break;
                }
            }


            if (!found)
            {
                switch (parameterType)
                {
                    case UnityEngine.AnimatorControllerParameterType.Bool:
                        message = "No bool parameter found with key";
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Int:
                        message = "No int parameter found with key";
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Float:
                        message = "No float parameter found with key";
                        return false;
                    case UnityEngine.AnimatorControllerParameterType.Trigger:
                        message = "No trigger parameter found with key";
                        return false;
                }
                message = "Parameter not found";
                return false;
            }

            message = string.Empty;
            return true;
        }

        #endregion

        #region REQUIRED MOTION GRAPH KEYS

        public static bool RequiredMotionGraphTriggerKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectTriggerParameters(s_MotionGraphTriggerParameters);

                valid = false;
                foreach (var p in s_MotionGraphTriggerParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No trigger parameter found with key";

                s_MotionGraphTriggerParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphSwitchKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectSwitchParameters(s_MotionGraphSwitchParameters);

                valid = false;
                foreach (var p in s_MotionGraphSwitchParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No switch parameter found with key";

                s_MotionGraphSwitchParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphIntKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectIntParameters(s_MotionGraphIntParameters);

                valid = false;
                foreach (var p in s_MotionGraphIntParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No int parameter found with key";

                s_MotionGraphIntParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphFloatKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectFloatParameters(s_MotionGraphFloatParameters);

                valid = false;
                foreach (var p in s_MotionGraphFloatParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No float parameter found with key";

                s_MotionGraphFloatParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphVectorKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectVectorParameters(s_MotionGraphVectorParameters);

                valid = false;
                foreach (var p in s_MotionGraphVectorParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No vector parameter found with key";

                s_MotionGraphVectorParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphTransformKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectTransformParameters(s_MotionGraphTransformParameters);

                valid = false;
                foreach (var p in s_MotionGraphTransformParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No transform parameter found with key";

                s_MotionGraphTransformParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        public static bool RequiredMotionGraphEventKeyField(SerializedProperty prop, MotionGraphContainer motionGraph, bool allowEmpty = true)
        {
            string errorMessage = string.Empty;
            bool valid = CheckMotionGraphKey(prop, motionGraph, out errorMessage);

            if (valid)
            {
                motionGraph.CollectEventParameters(s_MotionGraphEventParameters);

                valid = false;
                foreach (var p in s_MotionGraphEventParameters)
                {
                    if (p.name == prop.stringValue)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                    errorMessage = "No event parameter found with key";

                s_MotionGraphEventParameters.Clear();
            }

            if (valid)
                EditorGUILayout.PropertyField(prop);
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRequiredPrefixLabel(prop, valid);
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                MiniError(errorMessage);
            }

            return valid;
        }

        static bool CheckMotionGraphKey(SerializedProperty prop, MotionGraphContainer motionGraph, out string message)
        {
            // Controller
            if (motionGraph == null)
            {
                message = "Motion graph is null";
                return false;
            }

            // Empty is always valid
            if (string.IsNullOrEmpty(prop.stringValue))
            {
                message = "Key is required. Cannot be empty";
                return false;
            }

            message = null;
            return true;
        }

        #endregion
    }
}
