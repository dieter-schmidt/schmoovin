using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public abstract class FpsInventoryDbTableBase : ScriptableObject, ISerializationCallbackReceiver
    {
        public abstract string tableName { get; }

        public abstract FpsInventoryDatabaseEntry[] entries { get; }

        public abstract int count { get; }

#if UNITY_EDITOR

        public UnityAction editorOnContentsChanged;

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if (editorOnContentsChanged != null)
                editorOnContentsChanged();
        }

#else

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { }

#endif
    }
}