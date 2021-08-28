using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using NeoFPS;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphEditorFactory
    {
        static readonly char[] k_PathSeparators = new char[] { '/', '\\' };
        const bool k_HideElementsInHeirarchy = true;

        private static List<ElementCreator> s_Parameters = new List<ElementCreator>();
        private static List<ElementCreator> s_States = new List<ElementCreator>();
        private static List<ElementCreator> s_Conditions = new List<ElementCreator>();
        private static List<ElementCreator> s_Behaviours = new List<ElementCreator>();
        private static List<ElementCreator> s_Data = new List<ElementCreator>();
        private static List<ValidConnectables> s_BehaviourTargets = new List<ValidConnectables>();
        private static Dictionary<Type, MonoScript> s_Scripts = new Dictionary<Type, MonoScript>();
        private static Dictionary<Type, Type> s_BehaviourEditors = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> s_PropertyDrawers = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> s_ConditionDrawers = new Dictionary<Type, Type>();

        private static MotionGraphContainer s_Container = null;
        private static SerializedObject s_ContainerSO = null;
        private static SerializedObject s_ConnectionSO = null;
        private static SerializedObject s_ParentGraphSO = null;
        private static Vector2 s_Position = Vector2.zero;
        private static int s_ConditionGroupIndex = -1;

        public static event Action onRebuild;

        [MenuItem("Assets/Create/NeoFPS/Motion Graph/Motion Graph Asset", priority = NeoFpsMenuPriorities.motiongraph_asset)]
        static void CreateMotionGraphMenu()
        {
            CreateMotionGraphAsset();
        }

        [DidReloadScripts]
        static void OnReloadedScripts()
        {
            RebuildFactory();
        }

#if !UNITY_2018_OR_NEWER
        // UNITY 2017.4 doesn't call DidReloadScripts on startup
        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            RebuildFactory();
        }
#endif

        static void RebuildFactory()
        {
            s_Scripts.Clear();
            s_Parameters.Clear();
            s_States.Clear();
            s_Conditions.Clear();
            s_Behaviours.Clear();
            s_BehaviourTargets.Clear();
            s_BehaviourEditors.Clear();
            s_Data.Clear();
            s_PropertyDrawers.Clear();
            s_ConditionDrawers.Clear();

            // Gather graph elements
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in guids)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                var scriptClass = script.GetClass();

                if (scriptClass == null)
                    continue;

                // Runtime scripts
                if (scriptClass.IsSubclassOf(typeof(MotionGraphParameter)))
                {
                    if (ProcessParameterScript(scriptClass))
                        s_Scripts.Add(scriptClass, script);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphState)))
                {
                    if (ProcessStateScript(scriptClass))
                        s_Scripts.Add(scriptClass, script);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphBehaviour)))
                {
                    if (ProcessBehaviourScript(scriptClass))
                        s_Scripts.Add(scriptClass, script);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphCondition)))
                {
                    if (ProcessConditionScript(scriptClass))
                        s_Scripts.Add(scriptClass, script);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphDataBase)))
                {
                    if (ProcessDataScript(scriptClass))
                        s_Scripts.Add(scriptClass, script);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphPropertyDrawer)))
                {
                    ProcessPropertyDrawerScript(scriptClass);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphBehaviourEditor)))
                {
                    ProcessBehaviourEditorScript(scriptClass);
                    continue;
                }

                if (scriptClass.IsSubclassOf(typeof(MotionGraphConditionDrawer)))
                {
                    ProcessConditionDrawerScript(scriptClass);
                    continue;
                }
            }

            // Sort graph elements
            s_Parameters.Sort(CompareElements);
            s_States.Sort(CompareElements);
            s_Conditions.Sort(CompareElements);
            s_Data.Sort(CompareElements);

            if (onRebuild != null)
                onRebuild();
        }

        static int CompareElements(ElementCreator x, ElementCreator y)
        {
            return x.menuPath.CompareTo(y.menuPath);
        }

        public static MonoScript GetScriptForGraphElement (Type t)
        {
            MonoScript result;
            s_Scripts.TryGetValue(t, out result);
            return result;
        }
        
        class ElementCreator
        {
            public string menuPath { get; private set; }
            public string defaultName { get; private set; }
            public Type elementType { get; private set; }

            public ElementCreator(string p, string n, Type t)
            {
                menuPath = p;
                defaultName = n;
                elementType = t;
            }

            public ScriptableObject CreateElement (MotionGraphContainer container)
            {
                // Create
                var result = ScriptableObject.CreateInstance(elementType);
                result.name = defaultName;

                // Add to asset
                if (AssetDatabase.IsMainAsset(container))
                {
                    if (k_HideElementsInHeirarchy)
                        result.hideFlags |= HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(result, container);
                    //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(result));
                }

                return result;
            }
        }

        #region ASSET

        public static MotionGraphContainer CreateMotionGraphAsset()
        {
            // Get the asset path
            string path = "Assets";
            if (Selection.activeObject != null)
            {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    int index = path.LastIndexOfAny(k_PathSeparators);
                    path = path.Remove(index);
                }
            }
            path = AssetDatabase.GenerateUniqueAssetPath(path + "/NeoFPSMotionGraph.asset");

            // Create the container asset
            var container = ScriptableObject.CreateInstance<MotionGraphContainer>();
            AssetDatabase.CreateAsset(container, path);

            // Create the root node
            var root = ScriptableObject.CreateInstance<MotionGraph>();
            var rootSO = new SerializedObject(root);
            rootSO.FindProperty("m_Name").stringValue = "Root";
            rootSO.FindProperty("m_Container").objectReferenceValue = container;
            rootSO.FindProperty("uiPosition").vector2Value = new Vector2(0f, -200f);
            rootSO.FindProperty("internalUiPosition").vector2Value = new Vector2(0f, -200f);
            rootSO.ApplyModifiedProperties();
            if (k_HideElementsInHeirarchy)
                root.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(root, container);

            // Attach the root node
            var containerSO = new SerializedObject(container);
            containerSO.FindProperty("m_RootNode").objectReferenceValue = root;
            containerSO.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();

            return container;
        }

        #endregion

        #region PARAMETERS

        static bool ProcessParameterScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphElementAttribute;
                if (attr == null)
                    continue;

                s_Parameters.Add(new ElementCreator(attr.menuPath, attr.defaultName, t));
                return true;
            }
            return false;
        }

        public static GenericMenu GetParametersMenu (SerializedObject graphRoot)
        {
            GenericMenu menu = new GenericMenu();

            if (graphRoot.targetObject is MotionGraphContainer)
            {
                s_ContainerSO = graphRoot;
                for (int i = 0; i < s_Parameters.Count; ++i)
                    menu.AddItem(new GUIContent(s_Parameters[i].menuPath), false, ParametersMenuClickHandler, i);
            }
            else
            {
                menu.AddItem(new GUIContent("<error>"), false, null);
                Debug.LogError("Serialized object target is incorrect type");
            }

            return menu;
        }

        static void ParametersMenuClickHandler(object o)
        {
            int index = (int)o;
            CreateParameter (s_ContainerSO, index);
            s_ContainerSO = null;
        }

        static void CreateParameter (SerializedObject container, int index)
        {
            // Check for index out of bounds
            if (index < 0 || index >= s_Parameters.Count)
            {
                Debug.LogError("Parameter index out of bounds");
                return;
            }

            // Create parameter
            var owner = container.targetObject as MotionGraphContainer;
            var result = s_Parameters[index].CreateElement(owner) as MotionGraphParameter;

            // Assign to parameter list
            SerializedArrayUtility.Add(container.FindProperty("m_Parameters"), result);
            container.ApplyModifiedProperties();
        }

        public static ScriptableObject CreateParameter(SerializedObject container, Type t)
        {
            if (!t.IsSubclassOf(typeof(MotionGraphParameter)))
                return null;

            var owner = container.targetObject as MotionGraphContainer;
            foreach (var creator in s_Parameters)
            {
                if (creator.elementType == t)
                {
                    var result = creator.CreateElement(owner);

                    // Assign to data list
                    SerializedArrayUtility.Add(container.FindProperty("m_Parameters"), result);
                    container.ApplyModifiedProperties();

                    return result;
                }
            }

            return null;
        }

        #endregion

        #region MOTION GRAPH DATA

        static bool ProcessDataScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphElementAttribute;
                if (attr == null)
                    continue;

                s_Data.Add(new ElementCreator(attr.menuPath, attr.defaultName, t));
                return true;
            }
            return false;
        }


        public static GenericMenu GetDataEntryMenu(SerializedObject container)
        {
            GenericMenu menu = new GenericMenu();

            if (container.targetObject is MotionGraphContainer)
            {
                s_ContainerSO = container;
                for (int i = 0; i < s_Data.Count; ++i)
                    menu.AddItem(new GUIContent(s_Data[i].menuPath), false, DataEntryMenuClickHandler, i);
            }
            else
            {
                menu.AddItem(new GUIContent("<error>"), false, null);
                Debug.LogError("Serialized object target is incorrect type");
            }

            return menu;
        }

        static void DataEntryMenuClickHandler(object o)
        {
            int index = (int)o;
            CreateDataEntry(s_ContainerSO, index);
            s_ContainerSO = null;
        }

        static void CreateDataEntry(SerializedObject container, int index)
        {
            // Check for index out of bounds
            if (index < 0 || index >= s_Data.Count)
            {
                Debug.LogError("Parameter index out of bounds");
                return;
            }

            // Create parameter
            var owner = container.targetObject as MotionGraphContainer;
            var result = s_Data[index].CreateElement(owner);

            // Assign to parameter list
            SerializedArrayUtility.Add(container.FindProperty("m_Data"), result);
            container.ApplyModifiedProperties();
        }

        public static ScriptableObject CreateMotionData(SerializedObject container, Type t)
        {
            if (!t.IsSubclassOf(typeof(MotionGraphDataBase)))
                return null;

            var owner = container.targetObject as MotionGraphContainer;
            foreach (var creator in s_Data)
            {
                if (creator.elementType == t)
                {
                    var result = creator.CreateElement(owner);

                    // Assign to data list
                    SerializedArrayUtility.Add(container.FindProperty("m_Data"), result);
                    container.ApplyModifiedProperties();

                    return result;
                }
            }

            return null;
        }

        #endregion

        #region CONDITIONS

        static bool ProcessConditionScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphElementAttribute;
                if (attr == null)
                    continue;

                s_Conditions.Add(new ElementCreator(attr.menuPath, attr.defaultName, t));
                return true;
            }
            return false;
        }

        static void ProcessConditionDrawerScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphConditionDrawerAttribute;
                if (attr == null)
                    continue;

                s_ConditionDrawers.Add(attr.conditionType, t);
            }
        }

        public static MotionGraphConditionDrawer GetConditionDrawer(MotionGraphContainer container, MotionGraphCondition condition)
        {
            Type drawerType;
            if (!s_ConditionDrawers.TryGetValue(condition.GetType(), out drawerType))
                return null;

            var result = (MotionGraphConditionDrawer)Activator.CreateInstance(drawerType);
            result.Initialise(container, condition);
            return result;
        }

        public static GenericMenu GetConditionMenu(MotionGraphContainer container, SerializedObject connection, int groupIndex = -1)
        {
            GenericMenu menu = new GenericMenu();

            if (connection.targetObject is MotionGraphConnection)
            {
                s_Container = container;
                s_ConnectionSO = connection;
                s_ConditionGroupIndex = groupIndex;
                for (int i = 0; i < s_Conditions.Count; ++i)
                    menu.AddItem(new GUIContent(s_Conditions[i].menuPath), false, ConditionMenuClickHandler, i);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Condition Group"), false, ConditionMenuClickHandler, - 1);
            }
            else
            {
                menu.AddItem(new GUIContent("<error>"), false, null);
                Debug.LogError("Serialized object target is incorrect type");
            }

            return menu;
        }

        static void ConditionMenuClickHandler(object o)
        {
            int index = (int)o;
            if (index == -1)
                CreateConditionGroupCondition(s_Container, s_ConnectionSO, s_ConditionGroupIndex);
            else
                CreateCondition(s_Container, s_ConnectionSO, index, s_ConditionGroupIndex);

            s_ContainerSO = null;
            s_ConnectionSO = null;
            s_ConditionGroupIndex = -1;
        }

        static void CreateConditionGroupCondition(MotionGraphContainer container, SerializedObject connection, int groupIndex)
        {
            // Create parameter
            var result = ScriptableObject.CreateInstance<ConditionGroupCondition>();
            if (AssetDatabase.IsMainAsset(container))
            {
                if (k_HideElementsInHeirarchy)
                    result.hideFlags |= HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(result, container);
            }

            var resultSO = new SerializedObject(result);
            resultSO.FindProperty("m_Name").stringValue = "Condition";
            resultSO.FindProperty("m_Connection").objectReferenceValue = connection.targetObject;
            resultSO.ApplyModifiedProperties();

            // Assign to parameter list
            var conditions = (groupIndex == -1) ? connection.FindProperty("m_Conditions") :
                connection.FindProperty("m_ConditionGroups").GetArrayElementAtIndex(groupIndex).FindPropertyRelative("m_Conditions");
            ++conditions.arraySize;
            conditions.GetArrayElementAtIndex(conditions.arraySize - 1).objectReferenceValue = result;
            connection.ApplyModifiedProperties();
        }

        static void CreateCondition(MotionGraphContainer container, SerializedObject connection, int index, int groupIndex)
        {
            // Check for index out of bounds
            if (index < 0 || index >= s_Conditions.Count)
            {
                Debug.LogError("Parameter index out of bounds");
                return;
            }

            // Create parameter
            var result = s_Conditions[index].CreateElement(container) as MotionGraphCondition;
            var resultSO = new SerializedObject(result);
            resultSO.FindProperty("m_Name").stringValue = "Condition Group";
            resultSO.ApplyModifiedProperties();

            // Assign to parameter list
            var conditions = (groupIndex == -1) ? connection.FindProperty("m_Conditions") :
                connection.FindProperty("m_ConditionGroups").GetArrayElementAtIndex(groupIndex).FindPropertyRelative("m_Conditions");
            ++conditions.arraySize;
            conditions.GetArrayElementAtIndex(conditions.arraySize - 1).objectReferenceValue = result;
            connection.ApplyModifiedProperties();
        }

        public static int CreateConditionGroup(MotionGraphConnection connection)
        {
            var connectionSO = new SerializedObject(connection);
            var arrayProp = connectionSO.FindProperty("m_ConditionGroups");
            int index = arrayProp.arraySize++;
            var groupProp = arrayProp.GetArrayElementAtIndex(index);

            int newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            groupProp.FindPropertyRelative("m_Name").stringValue = connection.GetUniqueConditionGroupName("ConditionGroup", index);
            groupProp.FindPropertyRelative("m_Conditions").arraySize = 0;
            groupProp.FindPropertyRelative("m_TransitionOn").enumValueIndex = (int)MotionGraphConnection.ConditionRequirements.AllTrue;
            groupProp.FindPropertyRelative("m_ID").intValue = newID;
            groupProp.FindPropertyRelative("m_Connection").objectReferenceValue = connection;

            connectionSO.ApplyModifiedProperties();

            return newID;
        }

        #endregion

        #region STATES
        
        static bool ProcessStateScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphElementAttribute;
                if (attr == null)
                    continue;

                s_States.Add(new ElementCreator(attr.menuPath, attr.defaultName, t));
                return true;
            }
            return false;
        }

        public static GenericMenu GetStateMenu(MotionGraphContainer container, SerializedObject parent, Vector2 position)
        {
            GenericMenu menu = new GenericMenu();

            if (parent.targetObject is MotionGraph)
            {
                s_Position = position;
                s_Container = container;
                s_ParentGraphSO = parent;
                for (int i = 0; i < s_States.Count; ++i)
                    menu.AddItem(new GUIContent("Add State/" + s_States[i].menuPath), false, StateMenuClickHandler, i);
            }
            else
            {
                Debug.LogError("Serialized object target is incorrect type");
            }

            return menu;
        }

        static void StateMenuClickHandler(object o)
        {
            int index = (int)o;
            var state = CreateState(s_Container, s_ParentGraphSO, index);
            Selection.activeObject = state;

            s_ContainerSO = null;
            s_ParentGraphSO = null;
        }

        static MotionGraphState CreateState(MotionGraphContainer container, SerializedObject parent, int index)
        {
            // Check for index out of bounds
            if (index < 0 || index >= s_States.Count)
            {
                Debug.LogError("Parameter index out of bounds");
                return null;
            }

            // Create parameter
            var state = s_States[index].CreateElement(container) as MotionGraphState;
            var stateSO = new SerializedObject(state);
            stateSO.FindProperty("m_Parent").objectReferenceValue = parent.targetObject;
            stateSO.FindProperty("uiPosition").vector2Value = s_Position;
            stateSO.ApplyModifiedProperties();

            // Assign to parameter list
            var prop = parent.FindProperty("m_States");
            SerializedArrayUtility.Add(prop, state);
            prop = parent.FindProperty("m_DefaultEntry");
            if (prop.objectReferenceValue == null)
                prop.objectReferenceValue = state;
            parent.ApplyModifiedProperties();

            return state;
        }

        #endregion

        #region BEHAVIOURS

        static bool ProcessBehaviourScript(Type t)
        {
            MotionGraphElementAttribute element = null;
            MotionGraphBehaviourTargetAttribute target = null;

            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var elementAttribute = attributes[i] as MotionGraphElementAttribute;
                if (elementAttribute != null)
                {
                    element = elementAttribute;
                    if (target != null)
                        break;
                    else
                        continue;
                }

                var targetAttribute = attributes[i] as MotionGraphBehaviourTargetAttribute;
                if (targetAttribute != null)
                {
                    target = targetAttribute;
                    if (element != null)
                        break;
                }
            }

            if (element != null)
            {
                s_Behaviours.Add(new ElementCreator(element.menuPath, element.defaultName, t));
                s_BehaviourTargets.Add(target != null ? target.validConnectable : ValidConnectables.Both);
                return true;
            }
            return false;
        }

        static void ProcessBehaviourEditorScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphBehaviourEditorAttribute;
                if (attr == null)
                    continue;

                s_BehaviourEditors.Add(attr.behaviourType, t);
            }
        }

        public static MotionGraphBehaviourEditor GetBehaviourEditor(MotionGraphBehaviour b)
        {
            Type editorType;
            if (s_BehaviourEditors.TryGetValue(b.GetType(), out editorType))
                return (MotionGraphBehaviourEditor)Activator.CreateInstance(editorType);
            else
            {
                Debug.LogError("Failed to get motion graph behaviour editor for type: " + b.GetType());
                return new MotionGraphBehaviourEditor();
            }
        }

        public static EditorPicker GetStateBehaviourPicker()
        {
            var result = new EditorPicker("Add Behaviour", "Behaviours", true, false);

            for (int i = 0; i < s_Behaviours.Count; ++i)
            {
                if (s_BehaviourTargets[i] != ValidConnectables.Graph)
                    result.Add(s_Behaviours[i].menuPath, i);
            }

            return result;
        }

        public static EditorPicker GetSubGraphBehaviourPicker()
        {
            var result = new EditorPicker("Add Behaviour", "Behaviours", true, false);

            for (int i = 0; i < s_Behaviours.Count; ++i)
            {
                if (s_BehaviourTargets[i] != ValidConnectables.State)
                    result.Add(s_Behaviours[i].menuPath, i);
            }

            return result;
        }

        public static MotionGraphBehaviourPopup GetStateBehaviourPopup(MotionGraphContainer container, SerializedObject connectableSO)
        {
            var result = new MotionGraphBehaviourPopup(container, connectableSO);

            for (int i = 0; i < s_Behaviours.Count; ++i)
            {
                if (s_BehaviourTargets[i] != ValidConnectables.Graph)
                    result.Add(s_Behaviours[i].menuPath, i);
            }

            return result;
        }

        public static MotionGraphBehaviourPopup GetSubGraphBehaviourPopup(MotionGraphContainer container, SerializedObject connectableSO)
        {
            var result = new MotionGraphBehaviourPopup(container, connectableSO);

            for (int i = 0; i < s_Behaviours.Count; ++i)
            {
                if (s_BehaviourTargets[i] != ValidConnectables.State)
                    result.Add(s_Behaviours[i].menuPath, i);
            }

            return result;
        }

        public static MotionGraphBehaviour CreateBehaviour(MotionGraphContainer container, SerializedObject connectableSO, int index)
        {
            if (container == null || connectableSO == null)
                return null;

            var connectable = connectableSO.targetObject as MotionGraphConnectable;
            if (connectable == null)
                return null;
            
            // Create parameter
            var result = s_Behaviours[index].CreateElement(container) as MotionGraphBehaviour;
            result.name = s_Behaviours[index].defaultName;

            // Assign to parameter list
            var behaviours = connectableSO.FindProperty("m_Behaviours");
            SerializedArrayUtility.Add(behaviours, result);
            connectableSO.ApplyModifiedProperties();

            return result;
        }

        #endregion

        #region GRAPH PROPERTIES (PARAMETERS & DATA)

        static void ProcessPropertyDrawerScript(Type t)
        {
            var attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; ++i)
            {
                var attr = attributes[i] as MotionGraphPropertyDrawerAttribute;
                if (attr == null)
                    continue;

                s_PropertyDrawers.Add(attr.propertyType, t);
            }
        }

        public static MotionGraphPropertyDrawer GetPropertyDrawer(UnityEngine.Object obj, MotionGraphEditor editor)
        {
            Type drawerType;
            if (!s_PropertyDrawers.TryGetValue(obj.GetType(), out drawerType))
                return null;

            var result = (MotionGraphPropertyDrawer)Activator.CreateInstance(drawerType);
            result.Initialise(obj, editor);
            return result;
        }

        #endregion

        #region CONNECTIONS

        public static MotionGraphConnection CreateConnection(MotionGraphContainer container, SerializedObject source, MotionGraphConnectable destination)
        {
            if (container == null || source == null || destination == null)
                return null;

            MotionGraphConnectable sourceConnectable = source.targetObject as MotionGraphConnectable;
            if (sourceConnectable == null)
                return null;

            // Create & set properties
            var connection = ScriptableObject.CreateInstance<MotionGraphConnection>();
            var connectionSO = new SerializedObject(connection);
            connectionSO.FindProperty("m_Name").stringValue = "Connection";
            connectionSO.FindProperty("m_Source").objectReferenceValue = sourceConnectable;
            connectionSO.FindProperty("m_Destination").objectReferenceValue = destination;
            connectionSO.ApplyModifiedProperties();

            // Add to asset
            if (AssetDatabase.IsMainAsset(container))
            {
                if (k_HideElementsInHeirarchy)
                    connection.hideFlags |= HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(connection, container);
            }

            // Add to source connections
            var prop = source.FindProperty("m_Connections");
            SerializedArrayUtility.Add(prop, connection);
            source.ApplyModifiedProperties();

            return connection;
        }

        #endregion

        #region SUBGRAPHS

        public static MotionGraph CreateSubgraph(MotionGraphContainer container, SerializedObject parent)
        {
            return CreateSubgraph(container, parent, Vector2.zero);
        }

        public static MotionGraph CreateSubgraph(MotionGraphContainer container, SerializedObject parent, Vector2 position)
        {
            if (container == null || parent == null)
                return null;

            MotionGraph parentGraph = parent.targetObject as MotionGraph;
            if (parentGraph == null)
                return null;

            // Create & set properties
            var graph = ScriptableObject.CreateInstance<MotionGraph>();
            var graphSO = new SerializedObject(graph);
            graphSO.FindProperty("m_Name").stringValue = "New SubGraph";
            graphSO.FindProperty("m_Parent").objectReferenceValue = parentGraph;
            graphSO.FindProperty("m_Container").objectReferenceValue = container;
            graphSO.FindProperty("uiPosition").vector2Value = position;
            graphSO.FindProperty("internalUiPosition").vector2Value = new Vector2(0f, -200f);
            graphSO.ApplyModifiedProperties();

            // Add to asset
            if (AssetDatabase.IsMainAsset(container))
            {
                if (k_HideElementsInHeirarchy)
                    graph.hideFlags |= HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(graph, container);
            }

            // Add to parent graphs
            var prop = parent.FindProperty("m_SubGraphs");
            SerializedArrayUtility.Add(prop, graph);
            prop = parent.FindProperty("m_DefaultEntry");
            if (prop.objectReferenceValue == null)
                prop.objectReferenceValue = graph;
            parent.ApplyModifiedProperties();

            return graph;
        }

        #endregion
    }
}