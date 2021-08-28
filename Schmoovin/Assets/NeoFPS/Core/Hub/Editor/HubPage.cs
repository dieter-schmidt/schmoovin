using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub
{
    public abstract class HubPage
    {
        public abstract string pageHeader { get; }

        public virtual MessageType notification
        {
            get { return MessageType.None; }
        }

        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public abstract void OnGUI();

        protected Texture2D LoadIcon(string imageName)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D " + imageName);
            if (guids == null || guids.Length == 0)
                return null;
            else
                return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        protected Texture2D LoadIcon(string lightSkin, string darkSkin)
        {
            var guids = EditorGUIUtility.isProSkin ? AssetDatabase.FindAssets("t:Texture2D " + darkSkin) : AssetDatabase.FindAssets("t:Texture2D " + lightSkin);
            if (guids == null || guids.Length == 0)
                return null;
            else
                return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}