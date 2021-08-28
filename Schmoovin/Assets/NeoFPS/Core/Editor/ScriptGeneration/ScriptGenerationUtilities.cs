 using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ScriptGeneration
{
    public static class ScriptGenerationUtilities
    {
        /// <summary>
        /// Checks if a string is a valid class or property name (no whitespace, no invalid characters)
        /// </summary>
        /// <param name="toCheck">The class or property name string to check</param>
        /// <param name="camelCase">Is camel case (first letter lower case) allowed</param>
        /// <returns>true if the name is valid, false if not</returns>
        public static bool CheckClassOrPropertyName(string toCheck, bool camelCase = true)
        {
            // Use a regex to check the classname (alphanumeric and underscore only)
            if (camelCase)
                return Regex.IsMatch(toCheck, @"^[a-zA-Z0-9_]+$");
            else
                return Regex.IsMatch(toCheck, @"^[A-Z]+[a-zA-Z0-9_]*$"); // Force capitals for first letter
        }

        /// <summary>
        /// Checks if a namespace name is valid (no whitespace, alphanumeric words, starting with a letter, separated by `.`"
        /// </summary>
        /// <param name="ns">The namespace name to check</param>
        /// <returns>true if the namespace is valid, false if not</returns>
        public static bool CheckNamespace(string ns)
        {
            // Use a regex to check the namespace name (alphanumeric and underscore separated by '.')
            bool result = Regex.IsMatch(ns, @"^[a-zA-Z0-9_]+(\.[a-zA-Z0-9_]+)*$");
            return result;
        }

        /// <summary>
        /// Check that the string at the provided index in the array property is unique within the array
        /// </summary>
        /// <param name="prop">A serialized property pointing to an array of strings</param>
        /// <param name="index">The index within the array</param>
        /// <returns>True if the string is unique, false if not</returns>
        public static bool CheckNameIsUnique(SerializedProperty prop, int index)
        {
            var compare = prop.GetArrayElementAtIndex(index).stringValue;
            for (int i = 0; i < prop.arraySize; ++i)
            {
                if (i == index)
                    continue;

                if (prop.GetArrayElementAtIndex(i).stringValue == compare)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check that the string relative property at the provided index in the array property is unique within the array
        /// </summary>
        /// <param name="prop">A serialized property pointing to an array of serialized classes / structs</param>
        /// <param name="index">The index within the array</param>
        /// <param name="relativePropertyName">The relative property within the array element for the name string</param>
        /// <returns>True if the string is unique, false if not</returns>
        public static bool CheckNameIsUnique(SerializedProperty prop, int index, string relativePropertyName)
        {
            var compare = prop.GetArrayElementAtIndex(index).FindPropertyRelative(relativePropertyName).stringValue;
            for (int i = 0; i < prop.arraySize; ++i)
            {
                if (i == index)
                    continue;

                if (prop.GetArrayElementAtIndex(i).FindPropertyRelative(relativePropertyName).stringValue == compare)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check that the string at the provided index in the array property is unique within the array, and store any duplicate indices in a list (including the index to check)
        /// </summary>
        /// <param name="prop">A serialized property pointing to an array of strings</param>
        /// <param name="index">The index within the array</param>
        /// <param name="duplicates">A list to fill with duplicate indices (will be cleared beforehand)</param>
        /// <returns>The number of total duplicates found</returns>
        public static int CheckForDuplicateNames(SerializedProperty prop, int index, List<int> duplicates)
        {
            duplicates.Clear();

            // Get compare string from index
            var compare = prop.GetArrayElementAtIndex(index).stringValue;

            // Check against other elements
            for (int i = 0; i < prop.arraySize; ++i)
            {
                if (i == index)
                    continue;

                if (prop.GetArrayElementAtIndex(i).stringValue == compare)
                    duplicates.Add(i);
            }

            // If duplicates were found, also store original index
            if (duplicates.Count > 0)
                duplicates.Add(index);

            // return number of duplicates
            return duplicates.Count;
        }

        /// <summary>
        /// Check that the string relative property at the provided index in the array property is unique within the array, and store any duplicate indices in a list (including the index to check)
        /// </summary>
        /// <param name="prop">A serialized property pointing to an array of strings</param>
        /// <param name="index">The index within the array</param>
        /// <param name="relativePropertyName">The relative property within the array element for the name string</param>
        /// <param name="duplicates">A list to fill with duplicate indices (will be cleared beforehand)</param>
        /// <returns>The number of total duplicates found</returns>
        public static int CheckForDuplicateNames(SerializedProperty prop, int index, string relativePropertyName, List<int> duplicates)
        {
            duplicates.Clear();

            // Get compare string from index
            var compare = prop.GetArrayElementAtIndex(index).FindPropertyRelative(relativePropertyName).stringValue;

            // Check against other elements
            for (int i = 0; i < prop.arraySize; ++i)
            {
                if (i == index)
                    continue;

                if (prop.GetArrayElementAtIndex(i).FindPropertyRelative(relativePropertyName).stringValue == compare)
                    duplicates.Add(i);
            }

            // If duplicates were found, also store original index
            if (duplicates.Count > 0)
                duplicates.Add(index);

            // return number of duplicates
            return duplicates.Count;
        }

        /// <summary>
        /// Check if the name is found within the provided array
        /// </summary>
        /// <param name="n">The name to check</param>
        /// <param name="reserved">An array of names to check against</param>
        /// <returns>True if found, false if not</returns>
        public static bool CheckNameCollision(string n, string[] reserved)
        {
            foreach (var s in reserved)
            {
                if (s == n)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Draw an inspector field for a folder path that shows the folder itself and allows drag and drop
        /// </summary>
        /// <param name="pathProperty">The folder path serialized property</param>
        /// <returns>True if the path changed, false if not</returns>
        public static bool DrawFolderPathField(SerializedProperty pathProperty)
        {
            bool changed = false;
            Object folderObject = null;
            Object prevFolderObject = null;

            // Get current target folder object
            if (pathProperty.stringValue != string.Empty)
            {
                folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(pathProperty.stringValue);
                prevFolderObject = folderObject;

                if (folderObject == null)
                    pathProperty.stringValue = string.Empty;
            }

            // Show folder selection field
            folderObject = EditorGUILayout.ObjectField("Target Directory", folderObject, typeof(DefaultAsset), false, null);
            if (folderObject != prevFolderObject)
            {
                changed = true;
                if (folderObject == null)
                    pathProperty.stringValue = string.Empty;
                else
                    pathProperty.stringValue = AssetDatabase.GetAssetPath(folderObject);
            }

            return changed;
        }

        /// <summary>
        /// Draw an inspector field for a folder object that allows drag and drop
        /// </summary>
        /// <param name="pathProperty">The folder object (type UnityEngine.Object) serialized property</param>
        /// <returns>True if the path changed, false if not</returns>
        public static bool DrawFolderObjectField(SerializedProperty folderProperty)
        {
            bool changed = false;

            Object folderObject = EditorGUILayout.ObjectField("Target Directory", folderProperty.objectReferenceValue, typeof(DefaultAsset), false, null);
            if (folderObject != folderProperty.objectReferenceValue)
            {
                changed = true;
                folderProperty.objectReferenceValue = folderObject;
            }

            return changed;
        }

        /// <summary>
        /// Create a full script path that can be used for file IO from a folder object property and script name. If the folder object is set to "None", then the root assets folder will be used.
        /// </summary>
        /// <param name="folderProperty">The serialized property pointing to the folder object.</param>
        /// <param name="scriptName">The name of the script (no extension)</param>
        /// <returns>The resulting path</returns>
        public static string GetFullScriptPath(SerializedProperty folderProperty, string scriptName)
        {
            Object folderObject = folderProperty.objectReferenceValue;
            if (folderObject == null)
                return Application.dataPath + "/" + scriptName + ".cs";
            else
                return AssetDatabase.GetAssetPath(folderObject) + "/" + scriptName + ".cs";
        }

        /// <summary>
        /// Create a full script path that can be used for file IO from a folder path and script name. If the folder path is null or empty then the root assets folder will be used.
        /// </summary>
        /// <param name="folderPath">The path of the output folder</param>
        /// <param name="scriptName">The name of the script (no extension)</param>
        /// <returns>The resulting path</returns>
        public static string GetFullScriptPath(string folderPath, string scriptName)
        {
            if (string.IsNullOrEmpty(folderPath))
                folderPath = Application.dataPath;
            return folderPath + "/" + scriptName + ".cs";
        }

        public static string ReplaceKeywords(string source, KeyValuePair<string, string>[] replace)
        {
            foreach (var pair in replace)
                source = source.Replace(string.Format("%{0}%", pair.Key.ToUpper()), pair.Value);
            return source;
        }

        /// <summary>
        /// Search and replace within the source string for each of the key value pairs
        /// The key is the string to search for (will be converted to "%KEY%" from "key" or "KEY"), while the value is the string to replace it with
        /// </summary>
        /// <param name="source">The source string to modify</param>
        /// <param name="replace">An array of key value pairs to search and replace</param>
        /// <returns>The modified source string</returns>
        public static string ReplaceKeyword(string source, string searchKey, string replace)
        {
            return source.Replace(string.Format("%{0}%", searchKey.ToUpper()), replace);
        }

        /// <summary>
        /// Search and replace a keyword with a series of lines based on a format string.
        /// </summary>
        /// <param name="source">The source string to modify</param>
        /// <param name="searchKey">The keyword to replace</param>
        /// <param name="formatString">A format string for each line as used in the method string.Format(). 0 is the entry string, 1 is the index.</param>
        /// <param name="entries">The string lines to use</param>
        /// <returns>The modified source string</returns>
        public static string InsertMultipleLines(string source, string searchKey, string formatString, string[] entries, string countKey = null)
        {
            StringBuilder sb = new StringBuilder();

            // Set the count (if a key is provided)
            if (!string.IsNullOrEmpty(countKey))
                source = source.Replace(string.Format("%{0}%", countKey.ToUpper()), entries.Length.ToString());

            // Iterate through entries and append formatted string
            for (int i = 0; i < entries.Length; ++i)
                sb.AppendLine(string.Format(formatString, entries[i], i));

            // Return modified string
            return source.Replace(string.Format("%{0}%", searchKey.ToUpper()), sb.ToString());
        }

        /// <summary>
        /// The callback to use for inserting multiple lines into a template script
        /// </summary>
        /// <param name="index">The current line index</param>
        /// <returns>The line string, or null if the index is out of range</returns>
        public delegate string LineFormattingCallback(int index);

        /// <summary>
        /// Search and replace a keyword with a series of lines based on a formatting callback.
        /// </summary>
        /// <param name="source">The source string to modify</param>
        /// <param name="searchKey">The keyword to replace</param>
        /// <param name="func">A callback to retrieve the next string in the sequence. If null is returned then the sequence is completed</param>
        /// <returns>The modified source string</returns>
        public static string InsertMultipleLines(string source, string searchKey, LineFormattingCallback func, string countKey = null)
        {
            StringBuilder sb = new StringBuilder();

            // Iterate through entries and append formatted string
            int i = 0;
            for (; i < 1000; ++i)
            {
                string line = func(i);
                if (!string.IsNullOrEmpty(line))
                    sb.AppendLine(line);
                else
                    break;
            }

            // Set the count (if a key is provided)
            if (!string.IsNullOrEmpty(countKey))
                source = source.Replace(string.Format("%{0}%", countKey.ToUpper()), i.ToString());

            // Return modified string
            return source.Replace(string.Format("%{0}%", searchKey.ToUpper()), sb.ToString());
        }

        /// <summary>
        /// Write a script string to file.
        /// </summary>
        /// <param name="path">The file path to write to</param>
        /// <param name="text">The contents of the script</param>
        /// <param name="refresh">Should the asset database be refreshed after generating? Set this to true for the last of a batch if writing multiple</param>
        public static void WriteScript(string path, string text, bool refresh)
        {
            var targetDir = Path.GetDirectoryName(path);

            // Check the directory exists
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // Write the file
            File.WriteAllText(path, text);

            // Refresh the asset database
            if (refresh)
                AssetDatabase.Refresh();
        }
    }
}