#if UNITY_STANDALONE // Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using System;
using UnityEngine;

#if SETTINGS_USES_JSON
using System.IO;
#else
using System.Text;
#endif

namespace NeoFPS
{
	public abstract class SettingsContext<T> : SettingsContextBase where T : SettingsContext<T>
    {
        protected static T instance
        {
            get;
            private set;
        }
                
		public static T GetInstance (string filename)
		{
			if (instance == null)
			{
                // Load the settings from resources
                var loaded = Resources.Load<T>(filename);

                if (loaded == null)
                {
                    // Not found - Create new
                    instance = CreateInstance<T>();
                }
                else
                {
                    // Found - Duplicate to prevent changing settings in game overwriting in editor
                    instance = Instantiate(loaded);
                }
            }
			return instance;
		}        
    }
}

