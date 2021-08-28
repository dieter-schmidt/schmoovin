using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.CharacterMotion.EditorClasses
{
    public class SerializedArray<T> where T : UnityEngine.Object, new()
    {
        public SerializedProperty property { get; private set; }

        public SerializedArray(SerializedProperty p)
        {
            property = p;
        }
        public SerializedArray(SerializedObject so, string propertyName)
        {
            if (so != null)
                property = so.FindProperty(propertyName);
        }

        public void Add(T item)
        {
            if (!Contains(item))
            {
                ++property.arraySize;
                property.serializedObject.ApplyModifiedProperties();
                var entry = property.GetArrayElementAtIndex(property.arraySize - 1);
                entry.objectReferenceValue = item;
                entry.serializedObject.ApplyModifiedProperties();
            }
        }

        public void Remove(T item)
        {
            RemoveAt(IndexOf(item));
        }

        public void RemoveAt(int index)
        {
            if (property == null)
                return;
            if (index < 0 || index >= property.arraySize)
                return;
           
            property.DeleteArrayElementAtIndex(index);
            for (int i = index + 1; i < property.arraySize; ++i)
                property.MoveArrayElement(i, i - 1);
            --property.arraySize;
            property.serializedObject.ApplyModifiedProperties();
        }

        public void Clear()
        {
            if (property == null)
                return;
            property.ClearArray();
            property.arraySize = 0;
            property.serializedObject.ApplyModifiedProperties();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public int IndexOf (T item)
        {
            if (property == null)
                return -1;
            int count = property.arraySize;
            for (int i = 0; i < count; ++i)
            {
                var entry = property.GetArrayElementAtIndex(i);
                T cast = entry.objectReferenceValue as T;
                if (cast == item)
                    return i;
            }
            return -1;
        }

        public void Move(int fromIndex, int toIndex)
        {
            if (property == null)
                return;
            if (fromIndex < 0 || fromIndex >= property.arraySize)
                return;
            if (toIndex < 0 || toIndex >= property.arraySize)
                return;
            
            property.MoveArrayElement(fromIndex, toIndex);
            property.serializedObject.ApplyModifiedProperties();
        }

        public void Move(T item, int offset)
        {
            int index = IndexOf(item);
            Move(index, index + offset);
        }

        public T this [int index]
        {
            get 
            {
                if (property == null)
                    return null;
                if (index < 0 || index >= property.arraySize)
                    return null;
                return property.GetArrayElementAtIndex(index).objectReferenceValue as T; 
            }
            set
            {
                if (property == null)
                    return;
                if (index < 0 || index >= property.arraySize)
                    return;
                var entry = property.GetArrayElementAtIndex(index);
                if (entry != null)
                    entry.objectReferenceValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public int Count
        {
            get 
            {
                if (property == null)
                    return 0;
                return property.arraySize; 
            }
        }
    }
}
