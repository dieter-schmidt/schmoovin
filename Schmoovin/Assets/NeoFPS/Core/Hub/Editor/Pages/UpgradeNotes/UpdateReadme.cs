using NeoFPS.Hub;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
#if NEOFPS_INTERNAL
    [CreateAssetMenu(fileName = "UpdateReadme", menuName = "NeoFPS Internal/Update Readme")]
#endif
    public class UpdateReadme : ScriptableObject
    {
        public int version = 1101;

        public ReadmeSection[] sections = new ReadmeSection[0];

        public string GetVersionCode()
        {
            int major = version / 1000;
            int minor = (version / 100) % 10;
            int revision = version % 100;
            return string.Format("{0}.{1}.{2:D2}", major, minor, revision);
        }
    }
}