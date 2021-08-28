using System;
using UnityEngine;


namespace NeoFPS
{
    public class NeoObjectInHierarchyFieldAttribute : PropertyAttribute
    {
        public bool allowRoot
        {
            get;
            private set;
        }

        public bool required
        {
            get;
            set;
        }

        public GameObjectFilter filter
        {
            get;
            private set;
        }

        public string rootProperty
        {
            get;
            private set;
        }

        public RootPropertyType rootPropertyType
        {
            get;
            private set;
        }

        public NeoObjectInHierarchyFieldAttribute(bool allowRoot)
        {
            filter = null;
            rootProperty = null;
            this.allowRoot = allowRoot;
        }

        public NeoObjectInHierarchyFieldAttribute(string rootProperty, RootPropertyType rootPropertyType, bool allowRoot)
        {
            filter = null;
            this.rootProperty = rootProperty;
            this.rootPropertyType = rootPropertyType;
            this.allowRoot = allowRoot;
        }

        public NeoObjectInHierarchyFieldAttribute(bool allowRoot, GameObjectFilter filter)
        {
            rootProperty = null;
            this.allowRoot = allowRoot;
            this.filter = filter;
        }

        public NeoObjectInHierarchyFieldAttribute(string rootProperty, RootPropertyType rootPropertyType, bool allowRoot, GameObjectFilter filter)
        {
            this.rootProperty = rootProperty;
            this.rootPropertyType = rootPropertyType;
            this.allowRoot = allowRoot;
            this.filter = filter;
        }
    }

    public enum RootPropertyType
    {
        Transform,
        GameObject,
        Component
    }
}