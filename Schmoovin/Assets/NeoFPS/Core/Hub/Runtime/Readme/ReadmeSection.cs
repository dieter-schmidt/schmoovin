using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.Hub
{
    [Serializable]
    public class ReadmeSection
    {
        public Texture2D image = null;
        public string h2Heading = string.Empty;
        public string h3Heading = string.Empty;
        public string[] bulletPoints = new string[0];

        [TextArea]
        public string text = string.Empty;

        public WebLink[] links = new WebLink[0];
        public ObjectLink[] highlightObjects = new ObjectLink[0];
        public ReadmeAction[] actions = new ReadmeAction[0];

        [Serializable]
        public class WebLink
        {
            public string text = string.Empty;
            public string url = string.Empty;
        }

        [Serializable]
        public class ObjectLink
        {
            public string text = string.Empty;
            public UnityEngine.Object gameObject = null;
        }

        [Serializable]
        public class ReadmeAction
        {
            public string text = string.Empty;
            public UnityEvent action = new UnityEvent();
        }
    }
}