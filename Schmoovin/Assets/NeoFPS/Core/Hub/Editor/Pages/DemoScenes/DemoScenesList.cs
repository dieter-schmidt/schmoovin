using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeoFPSEditor.Hub.Pages
{
    //[CreateAssetMenu(fileName = "DemoScenesList", menuName = "NeoFPS Internal/Demo Scenes List")]
    public class DemoScenesList : ScriptableObject
    {
        public string category = "Scenes";
        public int priority = 0;
        [FormerlySerializedAs("m_Scenes")]
        public DemoScene[] scenes = new DemoScene[0];

        [Serializable]
        public class DemoScene
        {
            public string title = string.Empty;
            public string loadName = string.Empty;
            public Texture2D thumbnail = null;

            [TextArea]
            public string description = string.Empty;
        }
    }
}
