using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using NeoFPS.CharacterMotion;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphCustomPropertyDrawer
    {
    	#region FACTORY

        public delegate MotionGraphParameter PropertyCreationDelegate ();
        public delegate MotionGraphCustomPropertyDrawer DrawerCreationDelegate (MotionGraphParameter prop);

    	static Dictionary<Type, DrawerCreationDelegate> s_DrawerFactory = null;
        static List<PropertyFactoryInfo> s_PropertyFactory = null;

        private class PropertyFactoryInfo
    	{
    		public Type propertyType = null;
            public string menuEntry = string.Empty;
    		public PropertyCreationDelegate creator = null;

            public PropertyFactoryInfo (Type t, string menuEntry, PropertyCreationDelegate creator)
    		{
    			this.propertyType = t;
    			this.menuEntry = menuEntry;
    			this.creator = creator;
    		}
    	}

    	protected static void RegisterProperty<PropertyType, EditorType> (string name)
            where PropertyType : MotionGraphParameter, new()
            where EditorType : MotionGraphCustomPropertyDrawer, new()
    	{
    		// Check factories are set up
    		if (s_DrawerFactory == null)
    			s_DrawerFactory = new Dictionary<Type, DrawerCreationDelegate> ();
    		if (s_PropertyFactory == null)
    			s_PropertyFactory = new List<PropertyFactoryInfo> ();

    		// Add editor
    		s_DrawerFactory.Add (typeof (PropertyType), (c) => {
    			EditorType result = new EditorType ();
    			result.property = c;
    			return result;
    		});

    		// Add state creator
    		s_PropertyFactory.Add (new PropertyFactoryInfo (typeof(PropertyType), name, () => {
                PropertyType result = ScriptableObject.CreateInstance<PropertyType> ();
                result.name = name;
                return result;
    		}));
    		s_PropertyFactory.Sort ((PropertyFactoryInfo lhs, PropertyFactoryInfo rhs) => {
    			return string.Compare (lhs.menuEntry, rhs.menuEntry);
    		});
    	}

        public static MotionGraphCustomPropertyDrawer GetPropertyDrawer (MotionGraphParameter p)
    	{
    		Type t = p.GetType ();
            MotionGraphCustomPropertyDrawer result = null;
    		while (result == null) {
    			// Check for editor matching state type, or try again with base type
    			if (s_DrawerFactory.ContainsKey (t))
    				result = s_DrawerFactory [t].Invoke (p);
    			else
    				t = t.BaseType;
    			// NB: This editor type is pre-registered so acts as lowest level
    		}
    		return result;
    	}

        public static MotionGraphParameter CreatePropertyInstance (int menuIndex)
    	{
    		return s_PropertyFactory [menuIndex].creator ();
    	}

    	public static string[] GetMenuEntries ()
    	{
    		string[] result = new string[s_PropertyFactory.Count];
    		for (int i = 0; i < s_PropertyFactory.Count; ++i)
    			result [i] = s_PropertyFactory [i].menuEntry;
    		return result;
    	}

    	#endregion 

        private MotionGraphParameter m_Property = null;
        public MotionGraphParameter property
        {
            get { return m_Property; }
            set
            {
                m_Property = value;
                if (m_Property == null)
                    serializedObject = null;
                else
                    serializedObject = new SerializedObject(m_Property);
            }
        }

        public SerializedObject serializedObject
        {
            get;
            private set;
        }

        public void DrawPropertyElement (Rect r)
    	{
            if (serializedObject == null)
                return;

            serializedObject.UpdateIfRequiredOrScript();

            Rect rl = r;
            rl.width *= 0.6f;
            Rect rr = r;
            rr.width *= 0.4f;
            rr.width -= 8f;
            rr.x += rl.width + 8f;

            // Show (editable) name label
            SerializedProperty nameProp = serializedObject.FindProperty("m_Name");
            nameProp.stringValue = EditorGUI.TextField(rl, "", nameProp.stringValue, EditorStyles.toolbarTextField);

            // Draw the property data
            DrawPropertyData(rr);

            // Apply any changes
            serializedObject.ApplyModifiedProperties();
    	}

        protected virtual void DrawPropertyData (Rect r)
        {
        }
    }
}