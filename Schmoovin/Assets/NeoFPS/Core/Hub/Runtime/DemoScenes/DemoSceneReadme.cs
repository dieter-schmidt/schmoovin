using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS.Hub.Pages
{
    [ExecuteAlways]
    public class DemoSceneReadme : ReadmeBehaviour
    {
        private static DemoSceneReadme s_Current = null;

        public static event UnityAction<DemoSceneReadme> onCurrentSceneChanged;

        public static DemoSceneReadme current
        {
            get { return s_Current; }
            private set
            {
                s_Current = value;
                if (onCurrentSceneChanged != null)
                    onCurrentSceneChanged(s_Current);
            }
        }

        void Awake()
        {
            current = this;
        }

        void OnEnable()
        {
            current = this;
        }

        void OnDestroy()
        {
            if (current == this)
                current = null;
        }
    }
}
