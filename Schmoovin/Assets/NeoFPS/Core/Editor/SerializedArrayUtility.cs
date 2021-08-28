using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    class SerializedArrayUtility
    {
        public static void Add(SerializedProperty arrayProp, Object o, bool allowMultiple = false)
        {
            if (allowMultiple || !Contains(arrayProp, o))
            {
                ++arrayProp.arraySize;
                var entry = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);
                entry.objectReferenceValue = o;
            }
        }

        public static void Remove(SerializedProperty arrayProp, Object o)
        {
            RemoveAt(arrayProp, IndexOf(arrayProp, o));
        }

        public static void RemoveAt(SerializedProperty arrayProp, int index)
        {
            if (arrayProp == null)
                return;
            if (index < 0 || index >= arrayProp.arraySize)
                return;

            for (int i = index + 1; i < arrayProp.arraySize; ++i)
                arrayProp.MoveArrayElement(i, i - 1);
            --arrayProp.arraySize;
        }
        
        public static void Clear(SerializedProperty arrayProp)
        {
            if (arrayProp == null)
                return;

            arrayProp.ClearArray();
            arrayProp.arraySize = 0;
        }

        public static bool Contains(SerializedProperty arrayProp, Object o)
        {
            return IndexOf(arrayProp, o) != -1;
        }

        public static int IndexOf(SerializedProperty arrayProp, Object o)
        {
            if (arrayProp == null)
                return -1;

            int count = arrayProp.arraySize;
            for (int i = 0; i < count; ++i)
            {
                var entry = arrayProp.GetArrayElementAtIndex(i);
                if (entry.objectReferenceValue == o)
                    return i;
            }
            return -1;
        }

        public static void Move(SerializedProperty arrayProp, int fromIndex, int toIndex)
        {
            if (arrayProp == null)
                return;
            if (fromIndex < 0 || fromIndex >= arrayProp.arraySize)
                return;
            if (toIndex < 0 || toIndex >= arrayProp.arraySize)
                return;

            arrayProp.MoveArrayElement(fromIndex, toIndex);
        }

        public static void Move(SerializedProperty arrayProp, Object o, int offset)
        {
            int index = IndexOf(arrayProp, o);
            Move(arrayProp, index, index + offset);
        }

        public static T GetItemAtIndex<T>(SerializedProperty arrayProp, int index) where T : Object
        {
            if (arrayProp == null)
                return null;
            if (index < 0 || index >= arrayProp.arraySize)
                return null;

            return arrayProp.GetArrayElementAtIndex(index).objectReferenceValue as T;
        }

        public static void SetItemAtIndex(SerializedProperty arrayProp, int index, Object o)
        {
            if (arrayProp == null)
                return;
            if (index < 0 || index >= arrayProp.arraySize)
                return;

            var entry = arrayProp.GetArrayElementAtIndex(index);
            if (entry != null)
                entry.objectReferenceValue = o;
        }
    }
}
