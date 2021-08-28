using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoSaveGames.Serialization;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ModularFirearm), true)]
    public class ModularFirearmEditor : Editor
    {
        private readonly GUIContent k_AddShooterLabel = new GUIContent("Add Shooter");
        private readonly GUIContent k_AddTriggerLabel = new GUIContent("Add Trigger");
        private readonly GUIContent k_AddAimerLabel = new GUIContent("Add Aimer");
        private readonly GUIContent k_AddReloaderLabel = new GUIContent("Add Reloader");
        private readonly GUIContent k_AddAmmoLabel = new GUIContent("Add Ammo");
        private readonly GUIContent k_AddAmmoEffectLabel = new GUIContent("Add Ammo Effect");
        private readonly GUIContent k_AddMuzzleEffectLabel = new GUIContent("Add Muzzle Effect");
        private readonly GUIContent k_AddShellEjectorLabel = new GUIContent("Add Shell Ejector");
        private readonly GUIContent k_AddRecoilHandlerLabel = new GUIContent("Add Recoil Handler");
        private readonly GUIContent k_MatchOriginLabel = new GUIContent("Match Transform", "Drag a child transform here to automatically reposition the weapon geometry so that the child transform is aligned with the NeoFPS camera.");

        private static bool s_UseStandardInput = true;
        private static bool s_UseSaveSystem = true;
        private static InventoryType s_UseInventory = InventoryType.Standard;

        private ModularFirearm m_Firearm = null;
        private Texture2D m_Icon = null;
        private GameObject m_GeoSelection = null;

        public enum InventoryType
        {
            Standard,
            Stacked,
            Swappable,
            Custom
        }

        void Awake ()
        {
            m_Firearm = target as ModularFirearm;

            // Load icon
            var guids = AssetDatabase.FindAssets("EditorImage_NeoFpsInspectorIcon");
            if (guids != null && guids.Length > 0)
                m_Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            else
                m_Icon = null;
        }

        public override void OnInspectorGUI()
        {
            bool noGeometry = m_Firearm.transform.childCount == 0;
            if (!noGeometry)
            {
                // Show properties
                base.OnInspectorGUI();
                ShowOriginPointMatcher();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Modular Firearm Details", EditorStyles.boldLabel);

            if (GUILayout.Button("Modular Firearms Documentation"))
                Application.OpenURL("https://docs.neofps.com/manual/weapons-modular-firearms.html");

            string message = string.Empty;
            bool error = noGeometry || CheckFirearm(out message);
            if (noGeometry)
                message = "Firearm has no geometry. Please use the quick setup below to set up the weapon heirarchy.";

            if (m_Icon != null)
            {
                EditorGUILayout.BeginHorizontal();

                // Show NeoFPS icon
                Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(32), GUILayout.Height(32));
                if (m_Icon != null)
                {
                    r.width += 8f;
                    r.height += 8f;
                    GUI.Label(r, m_Icon);
                }
                
                EditorGUILayout.BeginVertical();
                if (error)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
                }
                else
                {
                    Color c = GUI.color;
                    GUI.color = NeoFpsEditorGUI.errorRed;
                    EditorGUILayout.LabelField(message, NeoFpsEditorGUI.wordWrappedBoldLabel);
                    GUI.color = c;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            else
            {
                if (error)
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                else
                    EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            if (noGeometry)
            {
                ShowQuickSetup();
            }
            else
            {
                ShowModuleDetails(typeof(IShooter), k_AddShooterLabel, "shooter", false);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(ITrigger), k_AddTriggerLabel, "trigger", false);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IAimer), k_AddAimerLabel, "aimer", true);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IReloader), k_AddReloaderLabel, "reloader", false);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IAmmo), k_AddAmmoLabel, "ammo", false);
                EditorGUILayout.Space();
                ShowAmmoEffectDetails();
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IRecoilHandler), k_AddRecoilHandlerLabel, "recoil handler", false);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IMuzzleEffect), k_AddMuzzleEffectLabel, "muzzle effect", true);
                EditorGUILayout.Space();
                ShowModuleDetails(typeof(IEjector), k_AddShellEjectorLabel, "shell ejector", true);
            }

            EditorGUILayout.EndVertical();
        }

        void ShowQuickSetup()
        {
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

            // Check if inspected firearm is a prefab and edited in the project view. Changing a prefab's heirarchy can only be done with PrefabUtility.ReplacePrefab
            // Which will cause Unity to fatal exception if the user hits undo afterwards
            // Might be able to remove this restriction for 2018.3+ due to new PrefabUtility.ApplyPrefabInstance method
            if (m_Firearm.gameObject.scene.rootCount == 0)
            {
                EditorGUILayout.HelpBox("Prefab quick setup cannot be used from the project heirarchy.\n\nPlace the prefab in a scene and edit from the scene heirarchy to continue.", MessageType.Error);
                return;
            }

            s_UseInventory = (InventoryType)EditorGUILayout.EnumPopup("Use Inventory", (Enum)s_UseInventory);
            s_UseStandardInput = EditorGUILayout.Toggle("Use Standard Input", s_UseStandardInput);
            s_UseSaveSystem = EditorGUILayout.Toggle("Use Save System", s_UseSaveSystem);
            m_GeoSelection = EditorGUILayout.ObjectField("Weapon Geometry", m_GeoSelection, typeof(GameObject), false) as GameObject;

            // Check if no object selected
            bool valid = true;
            if (m_GeoSelection == null)
            {
                NeoFpsEditorGUI.MiniError("Please select a geometry object");
                valid = false;
            }
            else
            {
                if (m_GeoSelection.GetComponentInChildren<MeshRenderer>() == null && m_GeoSelection.GetComponentInChildren<SkinnedMeshRenderer>() == null)
                {
                    NeoFpsEditorGUI.MiniError("Selection does not contain any mesh renderers");
                    valid = false;
                }

                if (m_GeoSelection.GetComponentInChildren<Animator>() == null)
                {
                    NeoFpsEditorGUI.MiniWarning("Selection does not contain an animator");
                }
            }

            if (valid)
                NeoFpsEditorGUI.MiniInfo("Geometry object is valid. Hit the button to set up your weapon");
            else
                GUI.enabled = false;

            if (GUILayout.Button("Set Up Firearm"))
            {
                // Add stance manager
                Undo.AddComponent<FirearmWieldableStanceManager>(m_Firearm.gameObject);

                // Add inventory
                switch (s_UseInventory)
                {
                    case InventoryType.Standard:
                        if (m_Firearm.GetComponent<FpsInventoryWieldable>() == null)
                            Undo.AddComponent<FpsInventoryWieldable>(m_Firearm.gameObject);
                        break;
                    case InventoryType.Stacked:
                        if (m_Firearm.GetComponent<FpsInventoryWieldable>() == null)
                            Undo.AddComponent<FpsInventoryWieldable>(m_Firearm.gameObject);
                        break;
                    case InventoryType.Swappable:
                        if (m_Firearm.GetComponent<FpsInventoryWieldableSwappable>() == null)
                            Undo.AddComponent<FpsInventoryWieldableSwappable>(m_Firearm.gameObject);
                        break;
                }

                // Add input
                if (s_UseStandardInput && m_Firearm.GetComponent<InputFirearm>() == null)
                    Undo.AddComponent<InputFirearm>(m_Firearm.gameObject);

                // Add NSGO
                if (s_UseSaveSystem && m_Firearm.GetComponent<NeoSerializedGameObject>() == null)
                    Undo.AddComponent<NeoSerializedGameObject>(m_Firearm.gameObject);

                // Create the weapon spring
                var springGO = new GameObject("WeaponSpring");
                Undo.RegisterCreatedObjectUndo(springGO, "Set up weapon");
                springGO.transform.SetParent(m_Firearm.transform);
                springGO.transform.localPosition = Vector3.zero;
                springGO.transform.localRotation = Quaternion.identity;
                springGO.transform.localScale = Vector3.one;

                // Add AdditiveTransformHandler
                Undo.AddComponent<AdditiveTransformHandler>(springGO);

                // Add additive effects
                Undo.AddComponent<FirearmRecoilEffect>(springGO);
                Undo.AddComponent<AdditiveKicker>(springGO);
                Undo.AddComponent<AdditiveJiggle>(springGO);
                Undo.AddComponent<BreathingEffect>(springGO);
                Undo.AddComponent<WeaponAimAmplifier>(springGO);
                var bob = new SerializedObject(Undo.AddComponent<PositionBob>(springGO));
                bob.FindProperty("m_BobType").enumValueIndex = 1;

                // Add geometry
                var geoTransform = Instantiate(m_GeoSelection, springGO.transform).transform;
                Undo.RegisterCreatedObjectUndo(geoTransform.gameObject, "Set up weapon");
                geoTransform.localPosition = Vector3.zero;
                geoTransform.localRotation = Quaternion.identity;
                geoTransform.localScale = Vector3.one;            
            }

            GUI.enabled = true;
        }

        void ShowOriginPointMatcher()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Origin Point", EditorStyles.boldLabel);

            Transform cameraTransform = EditorGUILayout.ObjectField(k_MatchOriginLabel, null, typeof(Transform), true) as Transform;
            if (cameraTransform != null)
            {
                Transform plus1 = null;
                Transform plus2 = null;

                // Check if transform is a child of this object
                bool found = false;
                Transform itr = cameraTransform;
                while (itr != null)
                {
                    plus2 = plus1;
                    plus1 = itr;
                    itr = itr.parent;
                    if (itr == m_Firearm.transform)
                    {
                        found = true;
                        break;
                    }
                }

                Vector3 rootPosition = m_Firearm.transform.position;

                // Check for valid hierarchy
                if (!found || plus2 == null || plus2 == cameraTransform)
                {
                    Debug.LogError("Transform must be a child of the firearm object and its weapon geometry object");
                    return;
                }

                // Get the difference and move the child
                Vector3 diff = cameraTransform.position - rootPosition;
                Undo.RecordObject(plus2, "Align Firearm To Camera");
                plus2.position -= diff;
            }
        }

        void ShowModuleDetails(Type baseType, GUIContent buttonLabel, string moduleName, bool optional)
        {
            if (EditorGUILayout.DropdownButton(buttonLabel, FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                List<Type> validTypes = GetScriptTypes(baseType);
                foreach (var t in validTypes)
                    menu.AddItem(new GUIContent(t.ToString()), false, AddModule, t);
                menu.ShowAsContext();
            }

            int activeShooters = 0;
            var modules = m_Firearm.GetComponentsInChildren(baseType, true);
            bool[] startEnabled = new bool[modules.Length];
            for (int i = 0; i < modules.Length; ++i)
            {
                // Check if monobehaviour disabled
                var mb = modules[i] as MonoBehaviour;
                if (!mb.enabled)
                {
                    startEnabled[i] = false;
                    continue;
                }

                // Only check components on active objects
                if (mb.gameObject.activeSelf)
                {
                    // Check if has "Start Active" property and it's enabled
                    var so = new SerializedObject(mb);
                    var prop = so.FindProperty("m_StartActive");

                    // If set to start active or doesn't have property, count it
                    startEnabled[i] = (prop == null || prop.boolValue);
                    if (startEnabled[i])
                        ++activeShooters;
                }
            }

            // Draw warnings
            switch (activeShooters)
            {
                case 0:
                    if (!optional)
                        EditorGUILayout.HelpBox(string.Format("Firearm requires a {0} module that is active on start.", moduleName), MessageType.Error);
                    break;
                case 1:
                    break;
                default:
                    EditorGUILayout.HelpBox(string.Format("Firearm has too many {0} modules that are active on start.\nEither remove the excess or make sure only one is set to start active.", moduleName), MessageType.Warning);
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Existing: ", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.BeginVertical();
            if (modules.Length > 0)
            {
                for (int i = 0; i < modules.Length; ++i)
                {
                    // Check if module is valid
                    var mod = modules[i] as IFirearmModule;
                    bool valid = (mod == null || CheckModuleValid(modules[i]));
                    
                    // Error colour
                    Color c = GUI.color;
                    if (!valid)
                        GUI.color = NeoFpsEditorGUI.errorRed;

                    // Check if module is on root object
                    bool isRoot = (modules[i].transform == m_Firearm.transform);

                    // Label string
                    string labelString = isRoot ?
                        modules[i].GetType().Name :
                        string.Format("{0} ({1})", modules[i].GetType().Name, modules[i].gameObject.name);

                    // Get label rect
                    var rect = EditorGUILayout.GetControlRect();
                    bool canViewChild = !isRoot && modules[i].gameObject.scene.IsValid();
                    if (canViewChild)
                        rect.width -= 20;

                    // Show label (start enabled as bold)
                    if (startEnabled[i])
                        EditorGUI.LabelField(rect, labelString, EditorStyles.boldLabel);
                    else
                        EditorGUI.LabelField(rect, labelString);

                    if (canViewChild)
                    {
                        rect.x += rect.width;
                        rect.width = 20;
                        if (GUI.Button(rect, EditorGUIUtility.FindTexture("d_ViewToolOrbit"), EditorStyles.label))
                            EditorGUIUtility.PingObject(modules[i].gameObject);
                    }

                    if (!valid)
                        GUI.color = c;
                }
            }
            else
                EditorGUILayout.LabelField("<none>");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void ShowAmmoEffectDetails()
        {
            if (EditorGUILayout.DropdownButton(k_AddAmmoEffectLabel, FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();
                List<Type> validTypes = GetScriptTypes(typeof(IAmmoEffect));
                foreach (var t in validTypes)
                    menu.AddItem(new GUIContent(t.ToString()), false, AddModule, t);
                menu.ShowAsContext();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Existing: ", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.BeginVertical();
            var modules = m_Firearm.GetComponentsInChildren(typeof(IAmmoEffect), true);
            if (modules.Length > 0)
            {
                foreach (var m in modules)
                {
                    // Label string
                    string labelString = (m.transform == m_Firearm.transform) ?
                        m.GetType().Name :
                        string.Format("{0} ({1})", m.GetType().Name, m.gameObject.name);
                    EditorGUILayout.LabelField(labelString);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Firearm requires a ammo effect module that is active on start.", MessageType.Error);
                EditorGUILayout.LabelField("<none>");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void AddModule(object o)
        {
            Undo.AddComponent(m_Firearm.gameObject, (Type)o);
        }

        List<Type> GetScriptTypes(Type baseClass)
        {
            List<Type> result = new List<Type>();

            var guids = AssetDatabase.FindAssets("t:MonoScript");
            for (int i = 0; i < guids.Length; ++i)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guids[i]));
                var t = script.GetClass();
                if (t != null && baseClass.IsAssignableFrom(t) && script.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
                    result.Add(t);
            }

            return result;
        }

        bool CheckFirearm (out string message)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                message = string.Empty;
                return true;
            }

            string issues = string.Empty;
            bool result = true;

            // Check shooters
            int numActive = GetActiveCount(typeof(IShooter));
            if (numActive != 1)
            {
                if (numActive == 0)
                    issues += "\n- Firearm requires a shooter module";
                else
                    issues += "\n- Too many shooter modules active on start";
                result = false;
            }
            if (!CheckModulesValid(typeof(IShooter)))
            {
                issues += "\n- One or more shooter modules has an error";
                result = false;
            }

            // Check triggers
            numActive = GetActiveCount(typeof(ITrigger));
            if (numActive != 1)
            {
                if (numActive == 0)
                    issues += "\n- Firearm requires a trigger module";
                else
                    issues += "\n- Too many trigger modules active on start";
                result = false;
            }
            if (!CheckModulesValid(typeof(ITrigger)))
            {
                issues += "\n- One or more trigger modules has an error";
                result = false;
            }

            // Check reloaders
            numActive = GetActiveCount(typeof(IReloader));
            if (numActive != 1)
            {
                if (numActive == 0)
                    issues += "\n- Firearm requires a reloader module";
                else
                    issues += "\n- Too many reloader modules active on start";
                result = false;
            }
            if (!CheckModulesValid(typeof(IReloader)))
            {
                issues += "\n- One or more reloader modules has an error";
                result = false;
            }

            // Check ammo
            numActive = GetActiveCount(typeof(IAmmo));
            if (numActive != 1)
            {
                if (numActive == 0)
                    issues += "\n- Firearm requires an ammo module";
                else
                    issues += "\n- Too many ammo modules active on start";
                result = false;
            }
            if (!CheckModulesValid(typeof(IAmmo)))
            {
                issues += "\n- One or more ammo modules has an error";
                result = false;
            }

            // Check ammo effects
            var ammoEffects = m_Firearm.GetComponentsInChildren<IAmmoEffect>(true);
            if (ammoEffects == null || ammoEffects.Length == 0)
            {
                issues += "\n- Firearm requires an ammo effect";
                result = false;
            }
            if (!CheckModulesValid(typeof(IAmmoEffect)))
            {
                issues += "\n- One or more ammo effect modules has an error";
                result = false;
            }

            // Check recoil handlers
            numActive = GetActiveCount(typeof(IRecoilHandler));
            if (numActive != 1)
            {
                if (numActive == 0)
                    issues += "\n- Firearm requires a recoil handler module";
                else
                    issues += "\n- Too many recoil handler modules active on start";
                result = false;
            }
            if (!CheckModulesValid(typeof(IRecoilHandler)))
            {
                issues += "\n- One or more recoil handler modules has an error";
                result = false;
            }

            // Check optional handlers
            if (!CheckModulesValid(typeof(IAimer)))
            {
                issues += "\n- One or more aimer modules has an error";
                result = false;
            }
            if (!CheckModulesValid(typeof(IMuzzleEffect)))
            {
                issues += "\n- One or more muzzle effect modules has an error";
                result = false;
            }
            if (!CheckModulesValid(typeof(IEjector)))
            {
                issues += "\n- One or more ejector modules has an error";
                result = false;
            }

            if (result)
                message = "Firearm has all the required modules set up correctly";
            else
                message = "Firearm has the following issues:" + issues;
            return result;
        }

        int GetActiveCount(Type baseType)
        {
            int result = 0;

            var modules = m_Firearm.GetComponentsInChildren(baseType, true);
            for (int i = 0; i < modules.Length; ++i)
            {
                var mb = modules[i] as MonoBehaviour;
                if (!mb.enabled || (mb.transform != m_Firearm.transform && !mb.gameObject.activeSelf))
                    continue;

                // Check if has "Start Active" property and it's enabled
                var so = new SerializedObject(mb);
                var prop = so.FindProperty("m_StartActive");

                // If set to start active or doesn't have property, count it
                if (prop == null || prop.boolValue)
                    ++result;
            }

            return result;
        }

        bool CheckModulesValid(Type baseType)
        {
            var modules = m_Firearm.GetComponentsInChildren(baseType, true);
            for (int i = 0; i < modules.Length; ++i)
            {
                if (!CheckModuleValid(modules[i]))
                    return false;
            }
            return true;
        }

        bool CheckModuleValid(Component component)
        {
            // Check module interface
            var module = component as IFirearmModuleValidity;
            if (module != null && !module.isModuleValid)
                return false;

            // Check for bad animator parameter keys (annoyingly has to be done from editor - IFirearmModule implementations can't do it)
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Get all fields
                var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    // Check if they have the AnimatorParameterKeyAttribute
                    var attributes = field.GetCustomAttributes(typeof(AnimatorParameterKeyAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        // Check if the value is valid
                        SerializedObject so = new SerializedObject(component);
                        var prop = so.FindProperty(field.Name);
                        foreach (AnimatorParameterKeyAttribute attr in attributes)
                        {
                            if (!AnimatorParameterKeyDrawer.CheckValid(attr, prop))
                                return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}