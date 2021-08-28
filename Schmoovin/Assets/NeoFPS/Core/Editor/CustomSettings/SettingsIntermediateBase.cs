using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor
{
    public abstract class SettingsIntermediateBase : ScriptableObject
    {
        public bool LoadFromJsonAsset(string assetName)
        {
            string[] guids = AssetDatabase.FindAssets(assetName);
            if (guids.Length == 0)
                return false;

            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (json == null)
                return false;

            JsonUtility.FromJsonOverwrite(json.text, this);

            return true;
        }

        public bool ApplySettings(string settingsPath)
        {
            var settings = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
            if (settings.Length == 0)
            {
                Debug.LogError("Could not find settings file at path: " + settingsPath + ", It may not have been created yet. Save the project and try again");
                return false;
            }

            Undo.SetCurrentGroupName("Apply Custom Settings");

            SerializedObject so = new SerializedObject(settings[0]);
            bool result = ApplySettingsInternal(so);
            if (result)
                so.ApplyModifiedProperties();

            return result;
        }

        protected abstract bool ApplySettingsInternal(SerializedObject serializedObject);
    }
}