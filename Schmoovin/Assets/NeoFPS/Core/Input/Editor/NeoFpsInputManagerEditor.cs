using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;
using NeoFPSEditor.ScriptGeneration;
using System;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(NeoFpsInputManager))]
    public class NeoFpsInputManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InspectEmbedded();
        }

        public void InspectEmbedded()
        {
            serializedObject.UpdateIfRequiredOrScript();

            InspectInputButtons();

            EditorGUILayout.Space();

            InspectInputAxes();

            EditorGUILayout.Space();

            InspectInputContexts();

            EditorGUILayout.Space();

            InspectGamepadProfiles();

            serializedObject.ApplyModifiedProperties();
        }

        #region INPUT BUTTONS


        [Flags]
        enum InputButtonsState
        {
            Valid = 0,
            RequiresRebuild = 1,
            NameValidErrors = 2,
            NameDuplicateErrors = 4,
            NameReservedErrors = 8,
            DisplayValidErrors = 16,
            DisplayDuplicateErrors = 32
        }

        InputButtonList m_InputButtons = null;
        private InputButtonList inputButtonsList
        {
            get
            {
                if (m_InputButtons == null)
                    m_InputButtons = new InputButtonList(serializedObject);
                return m_InputButtons;
            }
        }

        class InputButtonList : BaseReorderableList
        {
            private SerializedProperty m_RequiresRebuild = null;
            private SerializedProperty m_ButtonPropsDirty = null;
            private SerializedProperty m_ButtonsError = null;

            public int numNameValidErrors { get; private set; }
            public int numNameDuplicateErrors { get; private set; }
            public int numNameReservedErrors { get; private set; }
            public int numDisplayValidErrors { get; private set; }
            public int numDisplayDuplicateErrors { get; private set; }

            public bool isValid
            {
                get { return numNameValidErrors + numNameDuplicateErrors + numNameReservedErrors + numDisplayValidErrors + numDisplayDuplicateErrors == 0; }
            }

            public InputButtonList(SerializedObject serializedObject) :
                base(serializedObject.FindProperty("m_InputButtons"))
            {
                m_RequiresRebuild = serializedObject.FindProperty("m_ButtonsRequireRebuild");
                m_ButtonPropsDirty = serializedObject.FindProperty("m_InputButtonsDirty");
                m_ButtonsError = serializedObject.FindProperty("m_InputButtonsError");
            }

            protected override string heading
            {
                get { return "Input Buttons"; }
            }

            protected override void DrawListElement(Rect line1, int index)
            {
                var element = GetListElement(index);
                var spacing = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                bool error = false;

                // Check the name
                var nameProp = element.FindPropertyRelative("m_Name");
                var nameValidError = element.FindPropertyRelative("m_NameInvalidError");
                var nameDuplicateError = element.FindPropertyRelative("m_NameDuplicateError");
                var nameReservedError = element.FindPropertyRelative("m_NameReservedError");
                if (nameValidError.boolValue)
                {
                    error = true;
                    ++numNameValidErrors;
                }
                if (nameDuplicateError.boolValue)
                {
                    error = true;
                    ++numNameDuplicateErrors;
                }
                if (nameReservedError.boolValue)
                {
                    error = true;
                    ++numNameReservedErrors;
                }

                // Show name field
                var r = EditorGUI.PrefixLabel(line1, new GUIContent("Name"));
                if (error)
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                string newName = EditorGUI.DelayedTextField(r, nameProp.stringValue);
                if (newName != nameProp.stringValue)
                {
                    nameProp.stringValue = newName;
                    m_RequiresRebuild.boolValue = true;

                    nameValidError.boolValue = !ScriptGenerationUtilities.CheckClassOrPropertyName(nameProp.stringValue, false);
                    nameReservedError.boolValue = ScriptGenerationUtilities.CheckNameCollision(nameProp.stringValue, NeoFpsInputManager.fixedInputButtons);

                    // Check for duplicates
                    if (nameDuplicateError.boolValue)
                    {
                        for (int i = 0; i < serializedProperty.arraySize; ++i)
                        {
                            var e = serializedProperty.GetArrayElementAtIndex(i);
                            e.FindPropertyRelative("m_NameDuplicateError").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "m_Name");
                        }
                    }
                    else
                    {
                        List<int> duplicates = new List<int>();
                        if (ScriptGenerationUtilities.CheckForDuplicateNames(serializedProperty, index, "m_Name", duplicates) > 0)
                        {
                            for (int i = 0; i < duplicates.Count; ++i)
                                GetListElement(duplicates[i]).FindPropertyRelative("m_NameDuplicateError").boolValue = true;
                        }
                        else
                            nameDuplicateError.boolValue = false;
                    }
                }
                GUI.color = Color.white;

                line1.y += spacing;

                // Check the display name
                error = false;
                var displayNameProp = element.FindPropertyRelative("m_DisplayName");
                var displayNameValidError = element.FindPropertyRelative("m_DisplayNameInvalidError");
                var displayNameDuplicateError = element.FindPropertyRelative("m_DisplayNameDuplicateError");
                if (displayNameValidError.boolValue)
                {
                    error = true;
                    ++numDisplayValidErrors;
                }
                if (displayNameDuplicateError.boolValue)
                {
                    error = true;
                    ++numDisplayDuplicateErrors;
                }

                // Show the display name field
                r = EditorGUI.PrefixLabel(line1, new GUIContent("Display Name"));
                if (error)
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                newName = EditorGUI.DelayedTextField(r, displayNameProp.stringValue);
                if (newName != displayNameProp.stringValue)
                {
                    displayNameProp.stringValue = newName;
                    m_ButtonPropsDirty.boolValue = true;

                    displayNameValidError.boolValue = string.IsNullOrEmpty(newName);

                    // Check for duplicates
                    if (displayNameDuplicateError.boolValue)
                    {
                        for (int i = 0; i < serializedProperty.arraySize; ++i)
                        {
                            var e = serializedProperty.GetArrayElementAtIndex(i);
                            e.FindPropertyRelative("m_DisplayNameDuplicateError").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "m_Name");
                        }
                    }
                    else
                    {
                        List<int> duplicates = new List<int>();
                        if (ScriptGenerationUtilities.CheckForDuplicateNames(serializedProperty, index, "m_DisplayName", duplicates) > 0)
                        {
                            for (int i = 0; i < duplicates.Count; ++i)
                                GetListElement(duplicates[i]).FindPropertyRelative("m_DisplayNameDuplicateError").boolValue = true;
                        }
                        else
                            displayNameDuplicateError.boolValue = false;
                    }
                }
                GUI.color = Color.white;

                // Show category
                line1.y += spacing;
                var prop = element.FindPropertyRelative("m_Category");
                int previous = prop.enumValueIndex;
                EditorGUI.PropertyField(line1, prop);
                if (prop.enumValueIndex != previous)
                    m_ButtonPropsDirty.boolValue = true;

                // Show context
                line1.y += spacing;
                prop = element.FindPropertyRelative("m_Context");
                previous = prop.enumValueIndex;
                EditorGUI.PropertyField(line1, prop);
                if (prop.enumValueIndex != previous)
                    m_ButtonPropsDirty.boolValue = true;

                // Layout default keys
                line1.y += spacing;
                line1 = EditorGUI.PrefixLabel(line1, new GUIContent("Default Keys"));
                line1.width *= 0.5f;

                // Show primary
                prop = element.FindPropertyRelative("m_DefaultPrimary");
                previous = prop.enumValueIndex;
                EditorGUI.PropertyField(line1, prop, GUIContent.none);
                if (prop.enumValueIndex != previous)
                    m_ButtonPropsDirty.boolValue = true;

                // Show secondary
                line1.x += line1.width;
                prop = element.FindPropertyRelative("m_DefaultSecondary");
                previous = prop.enumValueIndex;
                EditorGUI.PropertyField(line1, prop, GUIContent.none);
                if (prop.enumValueIndex != previous)
                    m_ButtonPropsDirty.boolValue = true;
            }

            protected override void OnAdded(ReorderableList list)
            {
                base.OnAdded(list);

                var newElement = serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);

                // Reset properties
                newElement.FindPropertyRelative("m_Name").stringValue = string.Empty;
                newElement.FindPropertyRelative("m_DisplayName").stringValue = string.Empty;
                newElement.FindPropertyRelative("m_Category").enumValueIndex = 0;
                newElement.FindPropertyRelative("m_Context").enumValueIndex = 0;
                newElement.FindPropertyRelative("m_DefaultPrimary").enumValueIndex = 0;
                newElement.FindPropertyRelative("m_DefaultSecondary").enumValueIndex = 0;

                // Reset errors
                newElement.FindPropertyRelative("m_NameInvalidError").boolValue = true;
                newElement.FindPropertyRelative("m_NameDuplicateError").boolValue = false;
                newElement.FindPropertyRelative("m_NameReservedError").boolValue = false;
                newElement.FindPropertyRelative("m_DisplayNameInvalidError").boolValue = true;
                newElement.FindPropertyRelative("m_DisplayNameDuplicateError").boolValue = false;
            }

            protected override void OnRemoved(ReorderableList list)
            {
                // Check for duplicate errors
                var toRemove = serializedProperty.GetArrayElementAtIndex(list.index);
                bool duplicateName = toRemove.FindPropertyRelative("m_NameDuplicateError").boolValue;
                bool duplicateDisplay = toRemove.FindPropertyRelative("m_DisplayNameDuplicateError").boolValue;

                base.OnRemoved(list);

                // Check all remaining for duplicate names
                if (duplicateName)
                {
                    for (int i = 0; i < serializedProperty.arraySize; ++i)
                    {
                        var element = serializedProperty.GetArrayElementAtIndex(i);
                        element.FindPropertyRelative("m_NameDuplicateError").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "m_Name");
                    }
                }

                // Check all remaining for duplicate display names
                if (duplicateDisplay)
                {
                    for (int i = 0; i < serializedProperty.arraySize; ++i)
                    {
                        var element = serializedProperty.GetArrayElementAtIndex(i);
                        element.FindPropertyRelative("m_DisplayNameDuplicateError").boolValue = !ScriptGenerationUtilities.CheckNameIsUnique(serializedProperty, i, "m_DisplayName");
                    }
                }
            }

            protected override int GetNumLines(int index)
            {
                return 5;
            }

            protected override void OnChanged(ReorderableList list)
            {
                m_RequiresRebuild.boolValue = true;
            }

            public override void DoLayoutList()
            {
                numNameValidErrors = 0;
                numNameDuplicateErrors = 0;
                numNameReservedErrors = 0;
                numDisplayValidErrors = 0;
                numDisplayDuplicateErrors = 0;

                base.DoLayoutList();

                // Get state
                var state = InputButtonsState.Valid;
                if (m_RequiresRebuild.boolValue)
                    state |= InputButtonsState.RequiresRebuild;
                if (numNameValidErrors > 0)
                    state |= InputButtonsState.NameValidErrors;
                if (numNameDuplicateErrors > 0)
                    state |= InputButtonsState.NameDuplicateErrors;
                if (numNameReservedErrors > 0)
                    state |= InputButtonsState.NameReservedErrors;
                if (numDisplayValidErrors > 0)
                    state |= InputButtonsState.DisplayValidErrors;
                if (numDisplayDuplicateErrors > 0)
                    state |= InputButtonsState.DisplayDuplicateErrors;
                m_ButtonsError.intValue = (int)state;
            }
        }

        void InspectInputButtons()
        {
            var expandButtons = serializedObject.FindProperty("m_ExpandInputButtons");
            var requiresRebuild = serializedObject.FindProperty("m_ButtonsRequireRebuild");
            var buttonsDirty = serializedObject.FindProperty("m_InputButtonsDirty");
            var inputButtons = serializedObject.FindProperty("m_InputButtons");
            var revertTo = serializedObject.FindProperty("m_Revert");
            var snapshot = serializedObject.FindProperty("m_Snapshot");
            var errors = serializedObject.FindProperty("m_InputButtonsError");

            EditorGUILayout.LabelField("Input Buttons", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultKeyboardLayout"));

            // Draw the list
            expandButtons.boolValue = EditorGUILayout.Foldout(expandButtons.boolValue, "Input Button Details", true);
            if (expandButtons.boolValue)
            {
                EditorGUILayout.HelpBox("The following button names are reserved:\n• None\n• Menu\n• Back\n• Cancel", MessageType.None);
                inputButtonsList.DoLayoutList();
                EditorGUILayout.Space();
            }

            // Get buttons state
            var state = (InputButtonsState)errors.intValue;

            // Show buttons state
            switch (state)
            {
                case InputButtonsState.Valid:
                    EditorGUILayout.HelpBox("FpsInputButton constants are valid and up to date.", MessageType.Info);
                    break;
                case InputButtonsState.RequiresRebuild:
                    EditorGUILayout.HelpBox("FpsInputButton constants settings have changed and need generating.", MessageType.Warning);
                    break;
                default:
                    string message = "The following errors were found:";
                    if ((state & InputButtonsState.NameValidErrors) != InputButtonsState.Valid)
                        message += "\n- One or more button names are not valid.";
                    if ((state & InputButtonsState.NameDuplicateErrors) != InputButtonsState.Valid)
                        message += "\n- Duplicate button names were found.";
                    if ((state & InputButtonsState.NameReservedErrors) != InputButtonsState.Valid)
                        message += "\n- One or more button names is reserved.";
                    if ((state & InputButtonsState.DisplayValidErrors) != InputButtonsState.Valid)
                        message += "\n- One or more button display names are not valid.";
                    if ((state & InputButtonsState.DisplayDuplicateErrors) != InputButtonsState.Valid)
                        message += "\n- Duplicate button display names were found.";
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                    break;
            }

            // Disable if unchanged or invalid
            if (!requiresRebuild.boolValue)
                GUI.enabled = false;

            // Generate FpsInputButton Constants
            if (GUILayout.Button("Generate FpsInputButton Constants"))
            {
                // Get the source script
                var folderObjectProp = serializedObject.FindProperty("m_ScriptFolder");
                var sourceAsset = serializedObject.FindProperty("m_ScriptTemplate").objectReferenceValue as TextAsset;
                if (sourceAsset != null)
                {
                    // Get the lines as an array
                    string[] lines = new string[inputButtons.arraySize];
                    for (int i = 0; i < inputButtons.arraySize; ++i)
                        lines[i] = inputButtons.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue;

                    // Apply lines
                    string source = sourceAsset.text;
                    source = ScriptGenerationUtilities.ReplaceKeyword(source, "NAME", "FpsInputButton");
                    source = ScriptGenerationUtilities.InsertMultipleLines(source, "VALUES", (int index) =>
                    {
                        int fixedCount = NeoFpsInputManager.fixedInputButtons.Length;
                        if (index >= lines.Length + fixedCount)
                            return null;

                        if (index < fixedCount)
                            return string.Format("\t\tpublic const int {0} = {1};", NeoFpsInputManager.fixedInputButtons[index], index);
                        else
                            return string.Format("\t\tpublic const int {0} = {1};", lines[index - fixedCount], index);
                    }, "COUNT");
                    source = ScriptGenerationUtilities.InsertMultipleLines(source, "VALUE_NAMES", (int index) =>
                    {
                        if (index >= lines.Length + NeoFpsInputManager.fixedInputButtons.Length)
                            return null;

                        int fixedCount = NeoFpsInputManager.fixedInputButtons.Length;
                        if (index < fixedCount)
                            return string.Format("\t\t\t\"{0}\",", NeoFpsInputManager.fixedInputButtons[index]);
                        else
                        {
                            if (index < lines.Length + fixedCount - 1)
                                return string.Format("\t\t\t\"{0}\",", lines[index - fixedCount]);
                            else
                                return string.Format("\t\t\t\"{0}\"", lines[index - fixedCount]);
                        }
                    });

                    // Write the script
                    ScriptGenerationUtilities.WriteScript(ScriptGenerationUtilities.GetFullScriptPath(folderObjectProp, "FpsInputButton"), source, true);

                    // Reset dirty flags
                    requiresRebuild.boolValue = false;
                    buttonsDirty.boolValue = false;
                    errors.intValue = 0;
                    CopyInputButtons(inputButtons, revertTo);
                    snapshot.arraySize = 0;
                }

                // Delete the user settings file
                var guids = AssetDatabase.FindAssets("t:FpsKeyBindings");
                if (guids != null && guids.Length > 0)
                {
                    var settingsResource = AssetDatabase.LoadAssetAtPath<FpsKeyBindings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (settingsResource != null)
                        settingsResource.DeleteSaveFile();
                }
            }

            // Disable if unchanged
            GUI.enabled = requiresRebuild.boolValue || buttonsDirty.boolValue;

            // Create snapshot
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Snapshot"))
            {
                CopyInputButtons(inputButtons, snapshot);
            }

            // Disable if snapshot doesn't exist
            GUI.enabled = serializedObject.FindProperty("m_Snapshot").arraySize > 0;

            // Revert to snapshot
            if (GUILayout.Button("Revert To Snapshot"))
            {
                CopyInputButtons(snapshot, inputButtons);
            }
            EditorGUILayout.EndHorizontal();

            // Disable if unchanged
            GUI.enabled = requiresRebuild.boolValue || buttonsDirty.boolValue;

            // Revert changes
            if (GUILayout.Button("Revert To Last Generated"))
            {
                requiresRebuild.boolValue = false;
                buttonsDirty.boolValue = false;

                CopyInputButtons(revertTo, inputButtons);
                snapshot.arraySize = 0;
            }

            GUI.enabled = true;

            EditorGUILayout.Space();
        }
        
        void CopyInputButtons(SerializedProperty source, SerializedProperty destination)
        {
            destination.arraySize = source.arraySize;
            for (int i = 0; i < destination.arraySize; ++i)
            {
                var copyFrom = source.GetArrayElementAtIndex(i);
                var copyTo = destination.GetArrayElementAtIndex(i);

                copyTo.FindPropertyRelative("m_Name").stringValue = copyFrom.FindPropertyRelative("m_Name").stringValue;
                copyTo.FindPropertyRelative("m_DisplayName").stringValue = copyFrom.FindPropertyRelative("m_DisplayName").stringValue;
                copyTo.FindPropertyRelative("m_Category").enumValueIndex = copyFrom.FindPropertyRelative("m_Category").enumValueIndex;
                copyTo.FindPropertyRelative("m_Context").enumValueIndex = copyFrom.FindPropertyRelative("m_Context").enumValueIndex;
                copyTo.FindPropertyRelative("m_DefaultPrimary").enumValueIndex = copyFrom.FindPropertyRelative("m_DefaultPrimary").enumValueIndex;
                copyTo.FindPropertyRelative("m_DefaultSecondary").enumValueIndex = copyFrom.FindPropertyRelative("m_DefaultSecondary").enumValueIndex;
            }
        }

        #endregion

        #region INPUT AXES

        private ConstantsGenerator m_InputAxesGenerator = null;
        public ConstantsGenerator inputAxesGenerator
        {
            get
            {
                if (m_InputAxesGenerator == null)
                    m_InputAxesGenerator = new ConstantsGenerator(serializedObject, "m_InputAxisInfo", "m_InputAxisDirty", "m_InputAxisError");
                return m_InputAxesGenerator;
            }
        }

        void InspectInputAxes()
        {
            EditorGUILayout.LabelField("Input Axes", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MouseXAxis"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MouseYAxis"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MouseScrollAxis"));

            if (inputAxesGenerator.DoLayoutGenerator())
            {
                var scriptSource = serializedObject.FindProperty("m_ScriptTemplate").objectReferenceValue as TextAsset;
                if (scriptSource == null)
                {
                    Debug.LogError("Attempting to generate constants script when no source files has been set");
                }
                else
                {
                    var folderObject = serializedObject.FindProperty("m_ScriptFolder");
                    inputAxesGenerator.GenerateConstants(folderObject, "FpsInputAxis", scriptSource.text);
                }
            }
        }
        
        #endregion

        #region INPUT CONTEXTS

        private ConstantsGenerator m_InputContextsGenerator = null;
        public ConstantsGenerator inputContextsGenerator
        {
            get
            {
                if (m_InputContextsGenerator == null)
                    m_InputContextsGenerator = new ConstantsGenerator(serializedObject, "m_InputContextInfo", "m_InputContextDirty", "m_InputContextError");
                return m_InputContextsGenerator;
            }
        }

        void InspectInputContexts()
        {
            EditorGUILayout.LabelField("Input Contexts", EditorStyles.boldLabel);

            if (inputContextsGenerator.DoLayoutGenerator())
            {
                var scriptSource = serializedObject.FindProperty("m_ScriptTemplate").objectReferenceValue as TextAsset;
                if (scriptSource == null)
                {
                    Debug.LogError("Attempting to generate constants script when no source files has been set");
                }
                else
                {
                    var folderObject = serializedObject.FindProperty("m_ScriptFolder");
                    inputContextsGenerator.GenerateConstants(folderObject, "FpsInputContext", scriptSource.text);
                }
            }
        }

        #endregion

        #region GAMEPAD PROFILES

        void InspectGamepadProfiles()
        {
            EditorGUILayout.LabelField("Gamepad Profiles", EditorStyles.boldLabel);

            var gamepadProfiles = serializedObject.FindProperty("m_GamepadProfiles");

            if (GUILayout.Button("Add Profile"))
            {
                ++gamepadProfiles.arraySize;
            }

            for (int i = 0; i < gamepadProfiles.arraySize; ++i)
            {
                // Get the profile
                var profile = gamepadProfiles.GetArrayElementAtIndex(i);


                var expanded = profile.FindPropertyRelative("expanded");
                var profileName = profile.FindPropertyRelative("m_Name");

                expanded.boolValue = EditorGUILayout.Foldout(expanded.boolValue, profileName.stringValue, true);

                if (expanded.boolValue)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.PropertyField(profileName);
                    GUILayout.Space(2);
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("m_AnalogueSetup"));
                    GUILayout.Space(2);

                    var buttonMappings = profile.FindPropertyRelative("m_ButtonMappings");
                    for (int j = 0; j < buttonMappings.arraySize; ++j)
                        InspectGamepadButtonMapping(buttonMappings.GetArrayElementAtIndex(j), ((GamepadButton)j).ToString());

                    GUILayout.Space(2);
                    if (GUILayout.Button("Remove Profile"))
                    {
                        SerializedArrayUtility.RemoveAt(gamepadProfiles, i);
                        break;
                    }

                        EditorGUILayout.EndVertical();
                    }
                }
        }

        void InspectGamepadButtonMapping(SerializedProperty prop, string gamepadButton)
        {
            var buttonsProp = prop.FindPropertyRelative("m_Buttons");
            for (int i = 0; i <= buttonsProp.arraySize; ++i)
            {
                var rect = EditorGUILayout.GetControlRect();

                // Draw label
                if (i == 0)
                {
                    var labelRect = rect;
                    labelRect.width = EditorGUIUtility.labelWidth;
                    EditorGUI.LabelField(labelRect, gamepadButton);
                }

                // Draw dropdowns
                if (i == buttonsProp.arraySize)
                {
                    // Get field rect
                    rect.width -= EditorGUIUtility.labelWidth;
                    rect.x += EditorGUIUtility.labelWidth;

                    if (EditorGUI.DropdownButton(rect, new GUIContent("Add Button..."), FocusType.Passive))
                    {
                        List<FpsInputButton> validButtons = GetValidButtons(buttonsProp);
                        if (validButtons.Count > 0)
                        {
                            var menu = new GenericMenu();

                            for (int j = 0; j < validButtons.Count; ++j)
                                menu.AddItem(new GUIContent(FpsInputButton.names[validButtons[j]]), false, (o) => {
                                    int index = buttonsProp.arraySize++;
                                    buttonsProp.GetArrayElementAtIndex(index).FindPropertyRelative("m_Value").intValue = (int)o;
                                    buttonsProp.serializedObject.ApplyModifiedProperties();
                                }, (int)validButtons[j]);

                            menu.ShowAsContext();
                        }
                    }
                }
                else
                {
                    // Get field rect
                    rect.width -= EditorGUIUtility.labelWidth + 62;
                    rect.x += EditorGUIUtility.labelWidth;

                    // Draw the button name
                    var mapping = buttonsProp.GetArrayElementAtIndex(i);
                    var fpsButtonName = FpsInputButton.names[mapping.FindPropertyRelative("m_Value").intValue];
                    EditorGUI.SelectableLabel(rect, fpsButtonName, EditorStyles.textField);

                    // Draw the remove button
                    rect.x += rect.width + 2;
                    rect.width = 60;
                    if (GUI.Button(rect, "Remove"))
                    {
                        SerializedArrayUtility.RemoveAt(buttonsProp, i);
                        buttonsProp.serializedObject.ApplyModifiedProperties();
                        throw new ExitGUIException();
                    }
                }
            }
            GUILayout.Space(2);
        }

        List<FpsInputButton> GetValidButtons(SerializedProperty prop)
        {
            var result = new List<FpsInputButton>();
            if (prop.arraySize == 0)
            {
                for (int i = 0; i < FpsInputButton.count; ++i)
                    result.Add(i);
                return result;
            }
            else
            {
                // Get used
                var used = new List<KeyBindingContext>();
                for (int i = 0; i < prop.arraySize; ++i)
                    used.Add(GetContextForButton(prop.GetArrayElementAtIndex(i).FindPropertyRelative("m_Value").intValue));

                // Check against used
                for (int i = 0; i < FpsInputButton.count; ++i)
                {
                    bool canOverlap = true;
                    for (int j = 0; j < used.Count; ++j)
                        canOverlap &= KeyBindingContextMatrix.CanOverlap(GetContextForButton(i), used[j]);

                    if (canOverlap)
                        result.Add(i);
                }

                return result;
            }
        }

        KeyBindingContext GetContextForButton(FpsInputButton b)
        {
            if (b < NeoFpsInputManager.fixedInputButtons.Length)
                return KeyBindingContext.Default;

            var buttonsSetup = serializedObject.FindProperty("m_InputButtons");
            var button = buttonsSetup.GetArrayElementAtIndex(b - NeoFpsInputManager.fixedInputButtons.Length);
            return (KeyBindingContext)(button.FindPropertyRelative("m_Context").enumValueIndex);
        }

        #endregion
    }
}