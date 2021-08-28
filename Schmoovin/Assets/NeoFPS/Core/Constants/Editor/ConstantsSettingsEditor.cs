using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Text;
using System.IO;
using System.Reflection;
using NeoFPS;

namespace NeoFPSEditor
{
	[CustomEditor (typeof (ConstantsSettings))]
	public class ConstantsSettingsEditor : Editor
	{
		private ConstantClassInfo[] m_Constants = null;
        private bool m_TemplatesExpanded = false;

		public class ConstantClassInfo
		{
			// The owning editor
			ConstantsSettingsEditor m_Editor = null;
            private int m_Index = 0;
            // The constant's titlebar
            public SubInspectorTitlebar titlebar = null;
            // Is this class info visible or folded?
            public bool expanded = true;
			// The properties to inspect
			public SerializedProperty property = null;
            public ReorderableList constants = null;
            // Is the info valid
            public bool duplicate = false;
			public bool validName = true;
			public bool validNamespace = true;
			public bool validConstants = true;
			public bool validSize = true;
			// Is the generated file out of date?
			public bool dirty = true;

			public bool validTemplates
			{
				get { return m_Editor.CheckTemplatesValid ((ConstantsSettings.BaseType)property.FindPropertyRelative ("baseType").enumValueIndex); }
			}

			public bool valid
			{
				get { return validName && validNamespace && validSize && validConstants && validTemplates && !duplicate; }
			}

			public ConstantClassInfo (ConstantsSettingsEditor owner, SerializedProperty prop, int index)
            {
                titlebar = new SubInspectorTitlebar();
                titlebar.AddContextOption("Move Up", MoveUp, CheckMoveUp);
                titlebar.AddContextOption("Move Down", MoveDown, CheckMoveDown);
                titlebar.getLabel = GetLabel;

                m_Index = index;
                m_Editor = owner;
				property = prop;
				constants = new ReorderableList (owner.serializedObject, prop.FindPropertyRelative ("constants"));
				constants.drawElementCallback = DrawListElement;
			}

            string GetLabel()
            {
                string title = property.FindPropertyRelative("className").stringValue;
                if (title == string.Empty)
                    title = "<Unnamed>";
                return title;
            }

			public void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
			{
				SerializedProperty element = property.FindPropertyRelative ("constants").GetArrayElementAtIndex (index);

				string label = (index == 0) ? "{0} Default" : string.Format ("{{{0}}}", index);
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 76, rect.height), label);
				EditorGUI.PropertyField (new Rect (rect.x + 76, rect.y, rect.width - 76, rect.height), element, GUIContent.none);
			}

            void MoveUp()
            {
                m_Editor.serializedObject.FindProperty("m_Constants").MoveArrayElement(m_Index, m_Index - 1);
                m_Editor.ResetConstantsInfo();
            }

            bool CheckMoveUp()
            {
                return m_Index > 0;
            }

            void MoveDown()
            {
                m_Editor.serializedObject.FindProperty("m_Constants").MoveArrayElement(m_Index, m_Index + 1);
                m_Editor.ResetConstantsInfo();
            }

            bool CheckMoveDown()
            {
                return m_Index < m_Editor.serializedObject.FindProperty("m_Constants").arraySize - 1;
            }

            public void CheckValidity ()
			{
                // Check name and namespace
                validSize = m_Editor.CheckValidSize(this);
				validName = m_Editor.CheckValidName (property.FindPropertyRelative ("className").stringValue);
				validNamespace = m_Editor.CheckValidNamespace (property.FindPropertyRelative ("classNamespace").stringValue);
				duplicate = m_Editor.CheckForDuplicateSettings (property.FindPropertyRelative ("className").stringValue);

                validConstants = true;
				int numConstants = constants.serializedProperty.arraySize;
				List<string> m_Checked = new List<string> (numConstants);
				for (int i = 0; i < numConstants; ++i)
				{
					string constantName = constants.serializedProperty.GetArrayElementAtIndex (i).stringValue;

					if (!m_Editor.CheckValidName (constantName))
					{
						validConstants = false;
						return;
					}

					if (m_Checked.Contains (constantName))
					{
						validConstants = false;
						return;
					}

					m_Checked.Add (constantName);
				}
			}
		}

		void OnEnable ()
		{
			ResetConstantsInfo ();
		}

		void ResetConstantsInfo ()
		{
			SerializedProperty constants = serializedObject.FindProperty ("m_Constants");

			m_Constants = new ConstantClassInfo[constants.arraySize];
			for (int i = 0; i < m_Constants.Length; ++i)
			{
				SerializedProperty constantsClassInfo = constants.GetArrayElementAtIndex (i);
				m_Constants [i] = new ConstantClassInfo (this, constantsClassInfo, i);
				m_Constants [i].CheckValidity ();
			}
		}

		public override void OnInspectorGUI ()
		{
			EditorGUI.BeginChangeCheck ();

			DrawTargetDirectoryGUI ();
			DrawTemplatesSelection ();

			// Check constants
			DrawConstantsGUI ();

			if (EditorGUI.EndChangeCheck ())
			{
				serializedObject.ApplyModifiedProperties ();
			}
		}

		private void DrawConstantsGUI ()
		{
			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Constants Classes:");


			if (GUILayout.Button ("Add constant"))
			{
				serializedObject.FindProperty ("m_Constants").arraySize += 1;
                ResetConstantsInfo();
            }

            EditorGUILayout.Separator();

            for (int i = 0; i < m_Constants.Length; ++i)
			{
				if (m_Constants [i].titlebar.DoLayout())
				{
					EditorGUILayout.BeginHorizontal ();
					GUI.enabled = m_Constants [i].valid && m_Constants [i].dirty;
					if (GUILayout.Button ("Generate"))
					{
						GenerateSource (i);
					}
					GUI.enabled = true;
					if (GUILayout.Button ("Remove"))
					{
						serializedObject.FindProperty ("m_Constants").DeleteArrayElementAtIndex (i);
                        ResetConstantsInfo();
                        break; // Prevents checks against missing index
					}
					EditorGUILayout.EndHorizontal ();

					// Show if settings are valid
					if (m_Constants [i].valid)
					{
						EditorGUILayout.HelpBox ("Constants setup is valid.", MessageType.Info);
					}
					else
					{
						bool duplicate = m_Constants [i].duplicate;
						bool validName = m_Constants [i].validName;
						bool validNamespace = m_Constants [i].validNamespace;
						bool validConstants = m_Constants [i].validConstants;
						bool validTemplates = m_Constants [i].validTemplates;
                        bool validSize = m_Constants [i].validSize;
						string message = "Invalid! Code can not be generated until the following errors are resolved:\n";
						if (!validName)
							message += "\n- Name must follow naming rules.";
						if (duplicate)
							message += "\n- Duplicate name.";
						if (!validNamespace)
							message += "\n- Namespace must follow naming rules (can be nested with '.').";
						if (!validConstants)
							message += "\n- One or more constants does not follow naming rules or is a duplicate.";
						if (!validTemplates)
							message += "\n- Templates not specified for base type.";
                        if (!validSize)
                            message += "\n- Number of entries is more than the base type allows.";
                        if (!validName || !validNamespace || !validConstants)
							message += "\n- Naming rules require names start with a capital letter and only use alphanumeric characters and underscores.";
                        EditorGUILayout.HelpBox (message, MessageType.Error);
					}

					// Show properties
					EditorGUILayout.PropertyField (m_Constants [i].property.FindPropertyRelative ("className"));
					EditorGUILayout.PropertyField (m_Constants [i].property.FindPropertyRelative ("classNamespace"));
					EditorGUILayout.PropertyField (m_Constants [i].property.FindPropertyRelative ("baseType"));

					// Show list
					m_Constants [i].constants.DoLayoutList ();
                    
                    // Check validity (is it a problem to do it every layout call?)
                    m_Constants [i].CheckValidity();

					EditorGUILayout.Separator ();
				}
			}

			NeoFpsEditorGUI.Separator();
			EditorGUILayout.Space();

			if (GUILayout.Button("Add constant"))
			{
				serializedObject.FindProperty("m_Constants").arraySize += 1;
				ResetConstantsInfo();
			}

			EditorGUILayout.Space();
		}

		private void DrawTemplatesSelection ()
		{
			EditorGUILayout.Separator ();

			m_TemplatesExpanded = EditorGUILayout.Foldout (m_TemplatesExpanded, "Source Templates");

			if (m_TemplatesExpanded)
			{
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateByteConstant"), new GUIContent ("Byte Constant"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateByteConstantDrawer"), new GUIContent ("Byte Drawer"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateIntConstant"), new GUIContent ("Int Constant"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateIntConstantDrawer"), new GUIContent ("Int Drawer"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateUIntConstant"), new GUIContent ("UInt Constant"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateUIntConstantDrawer"), new GUIContent ("UInt Drawer"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateShortConstant"), new GUIContent ("Short Constant"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateShortConstantDrawer"), new GUIContent ("Short Drawer"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateUShortConstant"), new GUIContent ("UShort Constant"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TemplateUShortConstantDrawer"), new GUIContent ("UShort Drawer"));
			}

			EditorGUILayout.Separator ();
		}

		private void DrawTargetDirectoryGUI ()
		{
			// Get current target folder object
			Object folderObject = null;
			Object prevFolderObject = null;
			SerializedProperty targetDir = serializedObject.FindProperty ("m_TargetDirectory");
			if (targetDir.stringValue != string.Empty)
			{
				folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset> (targetDir.stringValue);
				prevFolderObject = folderObject;

				if (folderObject == null)
					targetDir.stringValue = string.Empty;
			}

			// Show folder selection field
			folderObject = EditorGUILayout.ObjectField ("Target Directory", folderObject, typeof(DefaultAsset), false, null);
			if (folderObject != prevFolderObject)
			{
				if (folderObject == null)
					targetDir.stringValue = string.Empty;
				else
					targetDir.stringValue = AssetDatabase.GetAssetPath (folderObject);
			}

			// Get current target folder object
			folderObject = null;
			prevFolderObject = null;
			SerializedProperty editorDir = serializedObject.FindProperty ("m_EditorDirectory");
			if (editorDir.stringValue != string.Empty)
			{
				folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset> (editorDir.stringValue);
				prevFolderObject = folderObject;

				if (folderObject == null)
					editorDir.stringValue = string.Empty;
			}

			// Show folder selection field
			folderObject = EditorGUILayout.ObjectField ("Editor Directory", folderObject, typeof(DefaultAsset), false, null);
			if (folderObject != prevFolderObject)
			{
				if (folderObject == null)
					editorDir.stringValue = string.Empty;
				else
					editorDir.stringValue = AssetDatabase.GetAssetPath (folderObject);
			}

			// Check if no folder selected and show error
			if (targetDir.stringValue == string.Empty || editorDir.stringValue == string.Empty)
				EditorGUILayout.HelpBox ("A target directory and editor directory are required to start generating classes.", MessageType.Error);
		}

		#region GENERATE SOURCE

		private const string k_ReplaceName = "%NAME%";
		private const string k_ReplaceNamespace = "%NAMESPACE%";
		private const string k_ReplaceType = "%TYPE%";
		private const string k_ReplaceValues = "%VALUES%";
		private const string k_ReplaceValueNames = "%VALUE_NAMES%";
		private const string k_ReplaceCount = "%COUNT%";
		private const int k_IndentValues = 2;
		private const int k_IndentValueNames = 3;

		private void GenerateSource (int index)
		{
			string className = m_Constants [index].property.FindPropertyRelative ("className").stringValue;
			string namespaceString = m_Constants[index].property.FindPropertyRelative ("classNamespace").stringValue;

			TextAsset templateConstant = null;
			TextAsset templateDrawer = null;
			string baseTypeString = string.Empty;

			var baseType = (ConstantsSettings.BaseType)m_Constants [index].property.FindPropertyRelative ("baseType").intValue;
			switch (baseType)
			{
				case ConstantsSettings.BaseType.Byte:
					templateConstant = serializedObject.FindProperty ("m_TemplateByteConstant").objectReferenceValue as TextAsset;
					templateDrawer = serializedObject.FindProperty ("m_TemplateByteConstantDrawer").objectReferenceValue as TextAsset;
					baseTypeString = "byte";
					break;
				case ConstantsSettings.BaseType.Int:
					templateConstant = serializedObject.FindProperty ("m_TemplateIntConstant").objectReferenceValue as TextAsset;
					templateDrawer = serializedObject.FindProperty ("m_TemplateIntConstantDrawer").objectReferenceValue as TextAsset;
					baseTypeString = "int";
					break;
				case ConstantsSettings.BaseType.UnsignedInt:
					templateConstant = serializedObject.FindProperty ("m_TemplateUIntConstant").objectReferenceValue as TextAsset;
					templateDrawer = serializedObject.FindProperty ("m_TemplateUIntConstantDrawer").objectReferenceValue as TextAsset;
					baseTypeString = "uint";
					break;
				case ConstantsSettings.BaseType.Short:
					templateConstant = serializedObject.FindProperty ("m_TemplateShortConstant").objectReferenceValue as TextAsset;
					templateDrawer = serializedObject.FindProperty ("m_TemplateShortConstantDrawer").objectReferenceValue as TextAsset;
					baseTypeString = "short";
					break;
				case ConstantsSettings.BaseType.UnsignedShort:
					templateConstant = serializedObject.FindProperty ("m_TemplateUShortConstant").objectReferenceValue as TextAsset;
					templateDrawer = serializedObject.FindProperty ("m_TemplateUShortConstantDrawer").objectReferenceValue as TextAsset;
					baseTypeString = "ushort";
					break;
			}

			if (templateConstant == null || templateDrawer == null)
				return;

			string constantSource = templateConstant.text;
			constantSource = constantSource.Replace (k_ReplaceName, className);
			constantSource = constantSource.Replace (k_ReplaceNamespace, namespaceString);
			constantSource = constantSource.Replace (k_ReplaceType, baseTypeString);

			// Get constant values count
			int count = m_Constants [index].constants.count;
			constantSource = constantSource.Replace (k_ReplaceCount, count.ToString ());

			// Get array of constant name strings;
			string[] constantNames = new string[count];
			for (int i = 0; i < count; ++i)
				constantNames [i] = m_Constants [index].constants.serializedProperty.GetArrayElementAtIndex (i).stringValue;

			// Generate constant value strings
			StringBuilder sb = new StringBuilder ();
			sb.Append ("public const ").Append (baseTypeString).Append (' ').Append (constantNames [0]).Append (" = 0;");
			for (int i = 1; i < count; ++i)
			{
				sb.AppendLine ();
				for (int indent = 0; indent < k_IndentValues; ++indent)
					sb.Append ('\t');
				sb.Append ("public const ").Append (baseTypeString).Append (' ').Append (constantNames [i]).Append (" = ").Append (i).Append (';');
			}
			constantSource = constantSource.Replace (k_ReplaceValues, sb.ToString ());

			// Generate constant name strings
			sb.Length = 0;
			sb.Append ('"').Append (constantNames [0]).Append ('"');
			for (int i = 1; i < constantNames.Length; ++i)
			{
				sb.AppendLine (",");
				for (int indent = 0; indent < k_IndentValueNames; ++indent)
					sb.Append ('\t');
				sb.Append ('"').Append (constantNames [i]).Append ('"');
			}
			constantSource = constantSource.Replace (k_ReplaceValueNames, sb.ToString ());
			WriteConstantsSource (className, constantSource);

			// Generate drawer source
			string drawerSource = templateDrawer.text;
			drawerSource = drawerSource.Replace (k_ReplaceName, className);
			drawerSource = drawerSource.Replace (k_ReplaceNamespace, m_Constants[index].property.FindPropertyRelative ("classNamespace").stringValue);
			WriteDrawerSource (className, drawerSource);

			AssetDatabase.Refresh ();

			ProcessScripts (className, namespaceString);
		}

		private void WriteConstantsSource (string name, string source)
		{
			string path = Path.Combine (
				              serializedObject.FindProperty ("m_TargetDirectory").stringValue,
				              name + ".cs"
			              );
			WriteSource (path, source);
		}

		private void WriteDrawerSource (string name, string source)
		{
			string path = Path.Combine (
				serializedObject.FindProperty ("m_EditorDirectory").stringValue,
				name + "PropertyDrawer.cs"
			);
			WriteSource (path, source);
		}

		private void WriteSource (string path, string source)
		{
			Debug.Log ("Writing source at: " + path);// + "\n" + source);
			using (StreamWriter outfile = new StreamWriter (path, false))
			{
				outfile.Write (source);
			}
//			Debug.Log ("Generation complete");
		}
		
		#endregion

		#region CHECKS

		private readonly char[] k_NamespaceSplitter = new char[]
		{
			'.'
		};

		private bool CheckValidName (string n)
		{
			if (n.Length == 0)
				return false;

			if (!char.IsLetter (n [0]))
				return false;

			if (!char.IsUpper (n [0]))
				return false;

			for (int i = 0; i < n.Length; ++i)
			{
				if (!char.IsLetterOrDigit (n [i]) && n [i] != '_')
					return false;
			}
						
			return true;
		}

		private bool CheckValidNamespace (string ns)
		{
			if (ns.Length == 0)
				return false;

			string[] names = ns.Split (k_NamespaceSplitter);
			for (int i = 0; i < names.Length; ++i)
			{
				if (!CheckValidName (names[i]))
					return false;
			}

			return true;
		}

		private bool CheckTemplatesValid (ConstantsSettings.BaseType t)
		{
			switch (t)
			{
				case ConstantsSettings.BaseType.Byte:
					return (serializedObject.FindProperty ("m_TemplateByteConstant").objectReferenceValue != null && serializedObject.FindProperty ("m_TemplateByteConstantDrawer") != null);
				case ConstantsSettings.BaseType.Int:
					return (serializedObject.FindProperty ("m_TemplateIntConstant").objectReferenceValue != null && serializedObject.FindProperty ("m_TemplateIntConstantDrawer") != null);
				case ConstantsSettings.BaseType.UnsignedInt:
					return (serializedObject.FindProperty ("m_TemplateUIntConstant").objectReferenceValue != null && serializedObject.FindProperty ("m_TemplateUIntConstantDrawer") != null);
				case ConstantsSettings.BaseType.Short:
					return (serializedObject.FindProperty ("m_TemplateShortConstant").objectReferenceValue != null && serializedObject.FindProperty ("m_TemplateShortConstantDrawer") != null);
				case ConstantsSettings.BaseType.UnsignedShort:
					return (serializedObject.FindProperty ("m_TemplateUShortConstant").objectReferenceValue != null && serializedObject.FindProperty ("m_TemplateUShortConstantDrawer") != null);
			}
			return false;
		}

		private bool CheckForDuplicateSettings (string name)
		{
			int found = 0;
			SerializedProperty array = serializedObject.FindProperty ("m_Constants");
			for (int i = 0; i < array.arraySize; ++i)
			{
				if (array.GetArrayElementAtIndex (i).FindPropertyRelative ("className").stringValue == name)
					++found;
			}

			return (found > 1);
		}

        private bool CheckValidSize(ConstantClassInfo info)
        {
            // Get current count
            uint count = (uint)info.constants.count;

            // Get max count
            uint max = 0;
            int index = info.property.FindPropertyRelative("baseType").enumValueIndex;
            switch (index)
            {
                case 0: // Byte
                    max = (uint)byte.MaxValue;
                    break;
                case 1: // Int
                    max = (uint)int.MaxValue;
                    break;
                case 2: // UnsignedInt
                    max = uint.MaxValue;
                    break;
                case 3: // Short
                    max = (uint)short.MaxValue;
                    break;
                case 4: // UnsignedShort
                    max = (uint)ushort.MaxValue;
                    break;
            }
            
            return count < max;
        }

		#endregion

		#region PERSISTANCE

		// get name order (old), get name order (new)

		int[] BuildIndexMapping (string[] oldEntries, string[] newEntries)
		{
			return null;
		}

		void ProcessScripts (string className, string namespaceString)
		{
			ProcessScriptableObjects(className, namespaceString);
			ProcessGameObjects(className, namespaceString);
		}

		void ProcessScriptableObjects (string className, string namespaceString)
		{
			string fullClass = string.Format ("{0}.{1}", namespaceString, className);

			string[] assets = AssetDatabase.FindAssets("t:ScriptableObject");
			foreach (string s in assets)
			{
				ScriptableObject o = AssetDatabase.LoadMainAssetAtPath (AssetDatabase.GUIDToAssetPath (s)) as ScriptableObject;
				if (o != null)
				{
					//Debug.Log ("Checking fields in scriptable object: " + o.name);
					FieldInfo [] fieldInfo = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (fieldInfo == null)
						continue;
					foreach (FieldInfo f in fieldInfo)
					{
						if (f.FieldType.ToString() == fullClass)
						{
							Debug.Log (string.Format("  - Found field: {0} on asset: {1} (click to inspect)", f.Name, o), o);
						}
					}
				}
				else
					Debug.Log ("Null ScriptableObject");
			}
		}

		void ProcessGameObjects (string className, string namespaceString)
		{
			string fullClass = string.Format ("{0}.{1}", namespaceString, className);

			string[] assets = AssetDatabase.FindAssets("t:GameObject");
			foreach (string s in assets)
			{
				GameObject o = AssetDatabase.LoadMainAssetAtPath (AssetDatabase.GUIDToAssetPath (s)) as GameObject;
				if (o != null)
				{
					// Get behaviours on object (and children)
					MonoBehaviour[] behaviours = o.GetComponentsInChildren<MonoBehaviour> (true);
					if (behaviours == null)
						continue;

					// Iterate through behaviours
					foreach (MonoBehaviour behaviour in behaviours)
					{
						if (behaviour == null)
							continue;

						// Get fields on behaviour
						FieldInfo [] fieldInfo = behaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						if (fieldInfo == null)
							continue;

						// Iterate through fields and check for relevant class
						foreach (FieldInfo f in fieldInfo)
						{
							if (f.FieldType.ToString() == fullClass)
                            {
                                Debug.Log(string.Format("  - Found field: {0} on component {1} on prefab: {2} (click to inspect)", f.Name, behaviour.GetType().Name, o.name), o);
							}
						}
					}
				}
				else
					Debug.Log ("Null GameObject");
			}
		}

		#endregion
	}
}